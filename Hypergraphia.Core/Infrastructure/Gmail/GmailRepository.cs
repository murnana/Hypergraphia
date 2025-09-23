using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Hypergraphia.Core.Domain.Entities;
using Hypergraphia.Core.Domain.Repositories;
using Hypergraphia.Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Hypergraphia.Core.Infrastructure.Gmail;

/// <summary>
/// Gmail APIを使用してメール情報を取得するリポジトリの実装
/// </summary>
public class GmailRepository : IEmailRepository
{
    private readonly GmailService _gmailService;
    private readonly ILogger<GmailRepository> _logger;

    /// <summary>
    /// GmailRepositoryの新しいインスタンスを初期化します
    /// </summary>
    /// <param name="credential">Gmail API認証情報</param>
    /// <param name="logger">ロガー</param>
    public GmailRepository(GoogleCredential credential, ILogger<GmailRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _gmailService = new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "Hypergraphia"
        });
    }

    /// <summary>
    /// 指定されたクエリ条件に基づいてメールメッセージを非同期で取得します
    /// </summary>
    /// <param name="query">メール検索条件を含むクエリオブジェクト</param>
    /// <param name="cancellationToken">非同期操作のキャンセル用トークン</param>
    /// <returns>条件に一致するメールメッセージのコレクション</returns>
    public async Task<IEnumerable<EmailMessage>> GetMessagesAsync(
        EmailQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Gmail検索を開始: クエリ={Query}, 最大件数={MaxResults}", query.Query, query.MaxResults);

            var request = _gmailService.Users.Messages.List("me");
            request.Q = query.Query;
            request.MaxResults = query.MaxResults;

            if (query.LabelIds?.Any() == true)
            {
                request.LabelIds = query.LabelIds.ToList();
            }

            if (!string.IsNullOrEmpty(query.PageToken))
            {
                request.PageToken = query.PageToken;
            }

            request.IncludeSpamTrash = query.IncludeSpamTrash;

            var response = await request.ExecuteAsync(cancellationToken);

            if (response.Messages == null)
            {
                _logger.LogInformation("検索結果が見つかりませんでした");
                return Enumerable.Empty<EmailMessage>();
            }

            var emails = new List<EmailMessage>();

            foreach (var messageInfo in response.Messages)
            {
                var email = await GetMessageByIdAsync(messageInfo.Id, cancellationToken);
                if (email != null)
                {
                    emails.Add(email);
                }
            }

            _logger.LogInformation("Gmail検索完了: {Count}件のメールを取得", emails.Count);
            return emails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gmail検索中にエラーが発生しました: クエリ={Query}", query.Query);
            throw;
        }
    }

    /// <summary>
    /// 指定されたメッセージIDに基づいて単一のメールメッセージを非同期で取得します
    /// </summary>
    /// <param name="messageId">取得するメールメッセージの一意識別子</param>
    /// <param name="cancellationToken">非同期操作のキャンセル用トークン</param>
    /// <returns>指定されたIDのメールメッセージ。存在しない場合はnull</returns>
    public async Task<EmailMessage?> GetMessageByIdAsync(string messageId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = _gmailService.Users.Messages.Get("me", messageId);
            var message = await request.ExecuteAsync(cancellationToken);

            return ConvertToEmailMessage(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "メール取得中にエラーが発生しました: MessageId={MessageId}", messageId);
            return null;
        }
    }

    /// <summary>
    /// Gmail APIのMessageオブジェクトをEmailMessageエンティティに変換します
    /// </summary>
    /// <param name="message">Gmail APIのMessageオブジェクト</param>
    /// <returns>変換されたEmailMessageエンティティ</returns>
    private EmailMessage ConvertToEmailMessage(Message message)
    {
        var headers = message.Payload.Headers;

        var subject = GetHeaderValue(headers, "Subject") ?? "";
        var fromHeader = GetHeaderValue(headers, "From") ?? "";
        var toHeader = GetHeaderValue(headers, "To") ?? "";
        var dateHeader = GetHeaderValue(headers, "Date") ?? "";

        var from = EmailAddress.Create(ExtractEmailAddress(fromHeader));
        var to = ExtractEmailAddresses(toHeader).Select(EmailAddress.Create).ToList();

        DateTime.TryParse(dateHeader, out var receivedAt);

        var body = ExtractBody(message.Payload);
        var labels = message.LabelIds?.Select(labelId => new EmailLabel(labelId, labelId)).ToList() ?? new List<EmailLabel>();
        var attachments = ExtractAttachments(message.Payload);

        return new EmailMessage(
            messageId: message.Id,
            subject: subject,
            body: body,
            from: from,
            to: to,
            receivedAt: receivedAt,
            labels: labels,
            attachments: attachments
        );
    }

    /// <summary>
    /// ヘッダーから指定された名前の値を取得します
    /// </summary>
    /// <param name="headers">ヘッダーのコレクション</param>
    /// <param name="name">取得するヘッダー名</param>
    /// <returns>ヘッダー値、見つからない場合はnull</returns>
    private static string? GetHeaderValue(IList<MessagePartHeader> headers, string name)
    {
        return headers?.FirstOrDefault(h => h.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;
    }

    /// <summary>
    /// ヘッダー文字列からメールアドレスを抽出します
    /// </summary>
    /// <param name="header">ヘッダー文字列</param>
    /// <returns>抽出されたメールアドレス</returns>
    private static string ExtractEmailAddress(string header)
    {
        // 簡単な実装：<email@domain.com>の形式から抽出
        var match = System.Text.RegularExpressions.Regex.Match(header, @"<([^>]+)>");
        return match.Success ? match.Groups[1].Value : header.Trim();
    }

    /// <summary>
    /// ヘッダー文字列から複数のメールアドレスを抽出します
    /// </summary>
    /// <param name="header">ヘッダー文字列</param>
    /// <returns>抽出されたメールアドレスのコレクション</returns>
    private static IEnumerable<string> ExtractEmailAddresses(string header)
    {
        if (string.IsNullOrWhiteSpace(header))
            yield break;

        // カンマ区切りで分割し、各アドレスを抽出
        var addresses = header.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var address in addresses)
        {
            yield return ExtractEmailAddress(address.Trim());
        }
    }

    /// <summary>
    /// メッセージペイロードからメール本文を抽出します
    /// </summary>
    /// <param name="payload">メッセージペイロード</param>
    /// <returns>抽出されたメール本文</returns>
    private static string ExtractBody(MessagePart payload)
    {
        if (payload.Body?.Data != null)
        {
            return DecodeBase64(payload.Body.Data);
        }

        if (payload.Parts != null)
        {
            foreach (var part in payload.Parts)
            {
                if (part.MimeType == "text/plain" && part.Body?.Data != null)
                {
                    return DecodeBase64(part.Body.Data);
                }
            }

            // HTMLの場合
            foreach (var part in payload.Parts)
            {
                if (part.MimeType == "text/html" && part.Body?.Data != null)
                {
                    return DecodeBase64(part.Body.Data);
                }
            }
        }

        return "";
    }

    /// <summary>
    /// メッセージペイロードから添付ファイル情報を抽出します
    /// </summary>
    /// <param name="payload">メッセージペイロード</param>
    /// <returns>添付ファイル情報のコレクション</returns>
    private static List<EmailAttachment> ExtractAttachments(MessagePart payload)
    {
        var attachments = new List<EmailAttachment>();

        if (payload.Parts != null)
        {
            foreach (var part in payload.Parts)
            {
                if (part.Filename != null && !string.IsNullOrEmpty(part.Filename))
                {
                    attachments.Add(new EmailAttachment(
                        fileName: part.Filename,
                        mimeType: part.MimeType ?? "application/octet-stream",
                        size: part.Body?.Size ?? 0,
                        attachmentId: part.Body?.AttachmentId ?? ""
                    ));
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
            var base64 = base64String.Replace('-', '+').Replace('_', '/');
            var padding = base64.Length % 4;
            if (padding > 0)
            {
                base64 += new string('=', 4 - padding);
            }

            var bytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return base64String;
        }
    }

    /// <summary>
    /// 利用可能なすべてのメールラベルを非同期で取得します
    /// </summary>
    /// <param name="cancellationToken">非同期操作のキャンセル用トークン</param>
    /// <returns>システムラベルとユーザー定義ラベルのコレクション</returns>
    public async Task<IEnumerable<EmailLabel>> GetLabelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Gmailラベル取得を開始");

            var request = _gmailService.Users.Labels.List("me");
            var response = await request.ExecuteAsync(cancellationToken);

            if (response.Labels == null)
            {
                _logger.LogInformation("ラベルが見つかりませんでした");
                return Enumerable.Empty<EmailLabel>();
            }

            var labels = response.Labels.Select(label => new EmailLabel(
                id: label.Id ?? "",
                name: label.Name ?? ""
            )).ToList();

            _logger.LogInformation("Gmailラベル取得完了: {Count}件のラベルを取得", labels.Count);
            return labels;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gmailラベル取得中にエラーが発生しました");
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
    public async Task<byte[]> GetAttachmentDataAsync(string messageId, string attachmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Gmail添付ファイル取得を開始: MessageId={MessageId}, AttachmentId={AttachmentId}", messageId, attachmentId);

            var request = _gmailService.Users.Messages.Attachments.Get("me", messageId, attachmentId);
            var attachment = await request.ExecuteAsync(cancellationToken);

            if (attachment.Data == null)
            {
                _logger.LogWarning("添付ファイルデータが見つかりませんでした: MessageId={MessageId}, AttachmentId={AttachmentId}", messageId, attachmentId);
                return Array.Empty<byte>();
            }

            // URL-safe base64デコード
            var base64 = attachment.Data.Replace('-', '+').Replace('_', '/');
            var padding = base64.Length % 4;
            if (padding > 0)
            {
                base64 += new string('=', 4 - padding);
            }

            var data = Convert.FromBase64String(base64);

            _logger.LogInformation("Gmail添付ファイル取得完了: サイズ={Size}bytes", data.Length);
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gmail添付ファイル取得中にエラーが発生しました: MessageId={MessageId}, AttachmentId={AttachmentId}", messageId, attachmentId);
            throw;
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