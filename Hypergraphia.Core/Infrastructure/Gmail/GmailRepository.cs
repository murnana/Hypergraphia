using System.Text;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Hypergraphia.Core.Domain.Entities;
using Hypergraphia.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Hypergraphia.Core.Infrastructure.Gmail
{
    /// <summary>
    /// Gmail APIを使用してメール情報を取得するリポジトリの実装
    /// </summary>
    public sealed class GmailRepository : IEmailRepository
    {
        private readonly GmailService             _gmailService;
        private readonly ILogger<GmailRepository> _logger;

        /// <summary>
        /// GmailRepositoryの新しいインスタンスを初期化します
        /// </summary>
        /// <param name="credential">Gmail API認証情報</param>
        /// <param name="logger">ロガー</param>
        public GmailRepository(GoogleCredential credential, ILogger<GmailRepository> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(paramName: nameof(logger));

            _gmailService = new(
                initializer: new()
                {
                    HttpClientInitializer = credential,
                    ApplicationName       = "Hypergraphia"
                }
            );
        }

        /// <summary>
        /// 指定されたクエリ条件に基づいてメールメッセージを非同期で取得します
        /// </summary>
        /// <param name="query">メール検索条件を含むクエリオブジェクト</param>
        /// <param name="cancellationToken">非同期操作のキャンセル用トークン</param>
        /// <returns>条件に一致するメールメッセージのコレクション</returns>
        public async Task<IEnumerable<GmailMessage>> GetMessagesAsync(
            EmailQuery        query,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                _logger.LogInformation(
                    message: "Gmail検索を開始: クエリ={Query}, 最大件数={MaxResults}",
                    query.Query,
                    query.MaxResults
                );

                var request = _gmailService.Users.Messages.List(userId: "me");
                request.Q          = query.Query;
                request.MaxResults = query.MaxResults;

                if(query.LabelIds?.Any() == true)
                {
                    request.LabelIds = query.LabelIds.ToList();
                }

                if(!string.IsNullOrEmpty(value: query.PageToken))
                {
                    request.PageToken = query.PageToken;
                }

                request.IncludeSpamTrash = query.IncludeSpamTrash;

                var response = await request.ExecuteAsync(cancellationToken: cancellationToken);

                if(response.Messages == null)
                {
                    _logger.LogInformation(message: "検索結果が見つかりませんでした");
                    return Enumerable.Empty<GmailMessage>();
                }

                var emails = new List<GmailMessage>();

                foreach(var messageInfo in response.Messages)
                {
                    var email = await GetMessageByIdAsync(
                        messageId: messageInfo.Id,
                        cancellationToken: cancellationToken
                    );
                    if(email != null)
                    {
                        emails.Add(item: email);
                    }
                }

                _logger.LogInformation(message: "Gmail検索完了: {Count}件のメールを取得", emails.Count);
                return emails;
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    exception: ex,
                    message: "Gmail検索中にエラーが発生しました: クエリ={Query}",
                    query.Query
                );
                throw;
            }
        }

        /// <summary>
        /// 指定されたメッセージIDに基づいて単一のメールメッセージを非同期で取得します
        /// </summary>
        /// <param name="messageId">取得するメールメッセージの一意識別子</param>
        /// <param name="cancellationToken">非同期操作のキャンセル用トークン</param>
        /// <returns>指定されたIDのメールメッセージ。存在しない場合はnull</returns>
        public async Task<GmailMessage?> GetMessageByIdAsync(
            string            messageId,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var request = _gmailService.Users.Messages.Get(userId: "me", id: messageId);
                var message = await request.ExecuteAsync(cancellationToken: cancellationToken);

                return await ConvertToEmailMessage(message: message);
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    exception: ex,
                    message: "メール取得中にエラーが発生しました: MessageId={MessageId}",
                    messageId
                );
                return null;
            }
        }

        /// <summary>
        /// 利用可能なすべてのメールラベルを非同期で取得します
        /// </summary>
        /// <param name="cancellationToken">非同期操作のキャンセル用トークン</param>
        /// <returns>システムラベルとユーザー定義ラベルのコレクション</returns>
        public async Task<IEnumerable<GmailLabel>> GetLabelsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(message: "Gmailラベル取得を開始");

                var request  = _gmailService.Users.Labels.List(userId: "me");
                var response = await request.ExecuteAsync(cancellationToken: cancellationToken);

                if(response.Labels == null)
                {
                    _logger.LogInformation(message: "ラベルが見つかりませんでした");
                    return[];
                }

                var labels = response.Labels.Select(
                                         selector: label => new GmailLabel(
                                             id: label.Id     ?? "",
                                             name: label.Name ?? "",
                                             type: DetermineEmailLabelType(label: label)
                                         )
                                     )
                                     .ToList();

                _logger.LogInformation(message: "Gmailラベル取得完了: {Count}件のラベルを取得", labels.Count);
                return labels;
            }
            catch(Exception ex)
            {
                _logger.LogError(exception: ex, message: "Gmailラベル取得中にエラーが発生しました");
                throw;
            }
        }

        /// <summary>
        /// 指定されたメールの添付ファイルデータを非同期で取得します
        /// </summary>
        /// <param name="messageId">添付ファイルを含むメールメッセージの識別子</param>
        /// <param name="attachmentId">取得する添付ファイルの識別子</param>
        /// <param name="cancellationToken">非同期操作のキャンセル用トークン</param>
        /// <returns>添付ファイルのバイナリデータ</returns>
        public async Task<byte[]> GetAttachmentDataAsync(
            string            messageId,
            string            attachmentId,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                _logger.LogInformation(
                    message: "Gmail添付ファイル取得を開始: MessageId={MessageId}, AttachmentId={AttachmentId}",
                    messageId,
                    attachmentId
                );

                var request = _gmailService.Users.Messages.Attachments.Get(
                    userId: "me",
                    messageId: messageId,
                    id: attachmentId
                );
                var attachment = await request.ExecuteAsync(cancellationToken: cancellationToken);

                if(attachment.Data == null)
                {
                    _logger.LogWarning(
                        message: "添付ファイルデータが見つかりませんでした: MessageId={MessageId}, AttachmentId={AttachmentId}",
                        messageId,
                        attachmentId
                    );
                    return Array.Empty<byte>();
                }

                // URL-safe base64デコード
                var base64  = attachment.Data.Replace(oldChar: '-', newChar: '+').Replace(oldChar: '_', newChar: '/');
                var padding = base64.Length % 4;
                if(padding > 0)
                {
                    base64 += new string(c: '=', count: 4 - padding);
                }

                var data = Convert.FromBase64String(s: base64);

                _logger.LogInformation(message: "Gmail添付ファイル取得完了: サイズ={Size}bytes", data.Length);
                return data;
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    exception: ex,
                    message: "Gmail添付ファイル取得中にエラーが発生しました: MessageId={MessageId}, AttachmentId={AttachmentId}",
                    messageId,
                    attachmentId
                );
                throw;
            }
        }

        /// <summary>
        /// Gmail APIのMessageオブジェクトをEmailMessageエンティティに変換します
        /// </summary>
        /// <param name="message">Gmail APIのMessageオブジェクト</param>
        /// <returns>変換されたEmailMessageエンティティ</returns>
        private async Task<GmailMessage> ConvertToEmailMessage(Message message)
        {
            var headers = message.Payload.Headers;

            var subject    = GetHeaderValue(headers: headers, name: "Subject") ?? "";
            var fromHeader = GetHeaderValue(headers: headers, name: "From")    ?? "";
            var toHeader   = GetHeaderValue(headers: headers, name: "To")      ?? "";
            var ccHeader   = GetHeaderValue(headers: headers, name: "Cc");
            var bccHeader  = GetHeaderValue(headers: headers, name: "Bcc");
            var dateHeader = GetHeaderValue(headers: headers, name: "Date") ?? "";

            var fromAddress = ExtractEmailAddress(header: fromHeader);
            var toAddresses = string.Join(separator: ", ", values: ExtractEmailAddresses(header: toHeader));

            var receivedAt = DateTime.TryParse(s: dateHeader, result: out var parsedDate) ? parsedDate : DateTime.Now;

            var body        = ExtractBody(payload: message.Payload);
            var labels      = await GetLabelsFromLabelIds(labelIds: message.LabelIds);
            var attachments = ExtractAttachments(payload: message.Payload);

            // ラベルから既読・重要フラグを判定
            var isRead      = !message.LabelIds?.Contains(item: "UNREAD")   ?? true;
            var isImportant = message.LabelIds?.Contains(item: "IMPORTANT") ?? false;

            return new(
                id: message.Id             ?? "",
                threadId: message.ThreadId ?? "",
                subject: subject,
                from: fromAddress,
                to: toAddresses,
                cc: ccHeader,
                bcc: bccHeader,
                receivedDate: receivedAt,
                body: body,
                isRead: isRead,
                isImportant: isImportant,
                labels: labels,
                attachments: attachments
            );
        }

        /// <summary>
        /// ラベルIDリストからラベル詳細情報を取得します
        /// </summary>
        /// <param name="labelIds">ラベルIDのリスト</param>
        /// <returns>ラベル詳細情報のリスト</returns>
        private async Task<List<GmailLabel>> GetLabelsFromLabelIds(IList<string>? labelIds)
        {
            if((labelIds == null) || (labelIds.Count == 0))
            {
                return new();
            }

            var labels = new List<GmailLabel>(capacity: labelIds.Count);
            foreach(var labelId in labelIds)
            {
                try
                {
                    var request   = _gmailService.Users.Labels.Get(userId: "me", id: labelId);
                    var labelInfo = await request.ExecuteAsync();

                    labels.Add(
                        item: new(
                            id: labelInfo.Id     ?? "",
                            name: labelInfo.Name ?? "",
                            type: DetermineEmailLabelType(label: labelInfo)
                        )
                    );
                }
                catch(Exception ex)
                {
                    _logger.LogWarning(
                        exception: ex,
                        message: "ラベル詳細取得に失敗しました: LabelId={LabelId}",
                        labelId
                    );

                    // ラベル詳細取得に失敗した場合はIDのみでラベルを作成
                    labels.Add(
                        item: new(
                            id: labelId,
                            name: string.Empty,
                            type: GmailLabelType.User
                        )
                    );
                }
            }

            return labels;
        }

        /// <summary>
        /// ヘッダーから指定された名前の値を取得します
        /// </summary>
        /// <param name="headers">ヘッダーのコレクション</param>
        /// <param name="name">取得するヘッダー名</param>
        /// <returns>ヘッダー値、見つからない場合はnull</returns>
        private static string? GetHeaderValue(IList<MessagePartHeader> headers, string name)
        {
            return headers?.FirstOrDefault(
                              predicate: h => h.Name?.Equals(
                                  value: name,
                                  comparisonType: StringComparison.OrdinalIgnoreCase
                              ) ?? false
                          )
                          ?.Value;
        }

        /// <summary>
        /// Gmail APIのラベル情報からEmailLabelTypeを判定します
        /// Gmail公式ドキュメントに基づく完全なシステムラベルリストを使用します
        /// </summary>
        /// <param name="label">Gmail APIのラベル情報</param>
        /// <returns>判定されたEmailLabelType</returns>
        private static GmailLabelType DetermineEmailLabelType(Label label)
        {
            // Gmail公式ドキュメントから取得した完全なシステムラベルID一覧
            // 参照: https://developers.google.com/workspace/gmail/api/guides/labels
            var systemLabelIds = new HashSet<string>
            {
                "INBOX", "SPAM", "TRASH", "UNREAD", "STARRED", "IMPORTANT",
                "SENT", "DRAFT", "CATEGORY_PERSONAL", "CATEGORY_SOCIAL",
                "CATEGORY_PROMOTIONS", "CATEGORY_UPDATES", "CATEGORY_FORUMS"
            };

            return systemLabelIds.Contains(item: label.Id ?? "") ? GmailLabelType.System : GmailLabelType.User;
        }

        /// <summary>
        /// ヘッダー文字列からメールアドレスを抽出します
        /// </summary>
        /// <param name="header">ヘッダー文字列</param>
        /// <returns>抽出されたメールアドレス</returns>
        private static string ExtractEmailAddress(string header)
        {
            // 簡単な実装：<email@domain.com>の形式から抽出
            var match = Regex.Match(input: header, pattern: @"<([^>]+)>");
            return match.Success ? match.Groups[groupnum: 1].Value : header.Trim();
        }

        /// <summary>
        /// ヘッダー文字列から複数のメールアドレスを抽出します
        /// </summary>
        /// <param name="header">ヘッダー文字列</param>
        /// <returns>抽出されたメールアドレスのコレクション</returns>
        private static IEnumerable<string> ExtractEmailAddresses(string header)
        {
            if(string.IsNullOrWhiteSpace(value: header))
            {
                yield break;
            }

            // カンマ区切りで分割し、各アドレスを抽出
            var addresses = header.Split(separator: ',', options: StringSplitOptions.RemoveEmptyEntries);
            foreach(var address in addresses)
            {
                yield return ExtractEmailAddress(header: address.Trim());
            }
        }

        /// <summary>
        /// メッセージペイロードからメール本文を抽出します
        /// </summary>
        /// <param name="payload">メッセージペイロード</param>
        /// <returns>抽出されたメール本文</returns>
        private static string ExtractBody(MessagePart payload)
        {
            if(payload.Body?.Data != null)
            {
                return DecodeBase64(base64String: payload.Body.Data);
            }

            if(payload.Parts != null)
            {
                foreach(var part in payload.Parts)
                {
                    if((part.MimeType == "text/plain") && (part.Body?.Data != null))
                    {
                        return DecodeBase64(base64String: part.Body.Data);
                    }
                }

                // HTMLの場合
                foreach(var part in payload.Parts)
                {
                    if((part.MimeType == "text/html") && (part.Body?.Data != null))
                    {
                        return DecodeBase64(base64String: part.Body.Data);
                    }
                }
            }

            return"";
        }

        /// <summary>
        /// メッセージペイロードから添付ファイル情報を抽出します
        /// </summary>
        /// <param name="payload">メッセージペイロード</param>
        /// <returns>添付ファイル情報のコレクション</returns>
        private static List<EmailAttachment> ExtractAttachments(MessagePart payload)
        {
            var attachments = new List<EmailAttachment>();

            if(payload.Parts != null)
            {
                foreach(var part in payload.Parts)
                {
                    if((part.Filename != null) && !string.IsNullOrEmpty(value: part.Filename))
                    {
                        attachments.Add(
                            item: new(
                                id: part.Body?.AttachmentId ?? "",
                                fileName: part.Filename,
                                mimeType: part.MimeType ?? "application/octet-stream",
                                size: part.Body?.Size   ?? 0,
                                isInline: part.Headers?.Any(
                                    predicate: h =>
                                        (h.Name                                == "Content-Disposition")
                                        && (h.Value?.Contains(value: "inline") == true)
                                ) ?? false,
                                contentId: part.Headers?.FirstOrDefault(predicate: h => h.Name == "Content-ID")?.Value
                            )
                        );
                    }
                }
            }

            return attachments;
        }

        /// <summary>
        /// Base64エンコードされた文字列をデコードします
        /// </summary>
        /// <param name="base64String">Base64エンコードされた文字列</param>
        /// <returns>デコードされた文字列</returns>
        private static string DecodeBase64(string base64String)
        {
            try
            {
                // URL-safe base64デコード
                var base64  = base64String.Replace(oldChar: '-', newChar: '+').Replace(oldChar: '_', newChar: '/');
                var padding = base64.Length % 4;
                if(padding > 0)
                {
                    base64 += new string(c: '=', count: 4 - padding);
                }

                var bytes = Convert.FromBase64String(s: base64);
                return Encoding.UTF8.GetString(bytes: bytes);
            }
            catch
            {
                return base64String;
            }
        }

        /// <summary>
        /// リソースを解放します
        /// </summary>
        public void Dispose()
        {
            _gmailService?.Dispose();
        }
    }
}
