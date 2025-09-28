using System.Net;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
using Hypergraphia.Core.Application.UseCases;
using Hypergraphia.Core.Domain.Entities;
using Hypergraphia.Core.Domain.Repositories;
using Hypergraphia.Core.Domain.Services;
using Hypergraphia.Core.Infrastructure.Configuration;
using Hypergraphia.Core.Infrastructure.Gmail;
using Hypergraphia.Core.Infrastructure.Notion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hypergraphia.Core.Infrastructure
{
    /// <summary>
    /// DIコンテナへのサービス登録を行う拡張メソッドクラス
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Hypergraphia.CoreのサービスをDIコンテナに登録します
        /// </summary>
        /// <param name="services">サービスコレクション</param>
        /// <param name="configuration">設定</param>
        /// <returns>更新されたサービスコレクション</returns>
        public static IServiceCollection AddHypergraphiaCore(
            this IServiceCollection services,
            IConfiguration          configuration
        )
        {
            // 設定の登録
            services.Configure<AppConfiguration>(config: configuration.GetSection(key: "Hypergraphia"));
            services.Configure<GmailConfiguration>(config: configuration.GetSection(key: "Hypergraphia:Gmail"));
            services.Configure<NotionConfiguration>(config: configuration.GetSection(key: "Hypergraphia:Notion"));

            // アプリケーションサービスの登録
            services.AddScoped<GetEmailsUseCase>();

            // ドメインサービスの登録
            services.AddScoped<IEmailDomainService, EmailDomainService>();

            // インフラストラクチャサービスの登録
            services.AddScoped<IEmailRepository>(
                implementationFactory: provider =>
                {
                    var gmailConfig = provider.GetRequiredService<IOptions<GmailConfiguration>>().Value;
                    var logger      = provider.GetRequiredService<ILogger<GmailRepository>>();

                    var credential = GoogleCredential.FromFile(path: gmailConfig.CredentialsPath)
                                                     .CreateScoped(scopes: gmailConfig.Scopes);

                    return new GmailRepository(credential: credential, logger: logger);
                }
            );

            services.AddScoped<INotionRepository>(
                implementationFactory: provider =>
                {
                    var notionConfig = provider.GetRequiredService<IOptions<NotionConfiguration>>().Value;
                    var logger       = provider.GetRequiredService<ILogger<NotionRepository>>();

                    var httpClient = new HttpClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(value: notionConfig.TimeoutSeconds);

                    return new NotionRepository(
                        httpClient: httpClient,
                        apiToken: notionConfig.ApiToken,
                        logger: logger
                    );
                }
            );

            return services;
        }

        /// <summary>
        /// ログ設定を追加します
        /// </summary>
        /// <param name="services">サービスコレクション</param>
        /// <param name="configuration">設定</param>
        /// <returns>更新されたサービスコレクション</returns>
        public static IServiceCollection AddHypergraphiaLogging(
            this IServiceCollection services,
            IConfiguration          configuration
        )
        {
            services.AddLogging(
                configure: builder =>
                {
                    var loggingConfig = configuration.GetSection(key: "Hypergraphia:Logging")
                                                     .Get<LoggingConfiguration>();

                    if(loggingConfig?.EnableConsoleLogging == true)
                    {
                        builder.AddConsole();
                    }

                    // ファイルログ設定は必要に応じて追加
                    // builder.AddFile(loggingConfig?.LogFilePath ?? "logs/hypergraphia.log");

                    builder.SetMinimumLevel(
                        level: Enum.TryParse<LogLevel>(value: loggingConfig?.LogLevel, result: out var level)
                            ? level
                            : LogLevel.Information
                    );
                }
            );

            return services;
        }
    }

    /// <summary>
    /// メールドメインサービスの実装
    /// </summary>
    internal class EmailDomainService : IEmailDomainService
    {
        private readonly ILogger<EmailDomainService> _logger;

        /// <summary>
        /// EmailDomainServiceの新しいインスタンスを初期化します
        /// </summary>
        /// <param name="logger">ロガー</param>
        public EmailDomainService(ILogger<EmailDomainService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(paramName: nameof(logger));
        }

        /// <summary>
        /// メールが重要かどうかを非同期で判定します
        /// </summary>
        /// <param name="gmail">判定対象のメールメッセージ</param>
        /// <returns>メールが重要と判定された場合はtrue、それ以外はfalse</returns>
        public async Task<bool> IsEmailImportantAsync(GmailMessage gmail)
        {
            if(gmail == null)
            {
                _logger.LogWarning(message: "メールオブジェクトがnullです");
                return false;
            }

            _logger.LogDebug(message: "メールの重要性を判定中: MessageId={MessageId}", args: gmail.Id);

            await Task.Delay(millisecondsDelay: 1); // 非同期操作のプレースホルダー

            // 重要なメールの判定ロジック
            var isImportant = gmail.Labels.Any(
                                  predicate: label => label.Name.Contains(
                                      value: "IMPORTANT",
                                      comparisonType: StringComparison.OrdinalIgnoreCase
                                  )
                              )                                                                                       ||
                              gmail.Attachments.Any()                                                                 ||
                              gmail.Subject.Contains(value: "緊急", comparisonType: StringComparison.OrdinalIgnoreCase) ||
                              gmail.Subject.Contains(value: "重要", comparisonType: StringComparison.OrdinalIgnoreCase) ||
                              gmail.Subject.Contains(value: "至急", comparisonType: StringComparison.OrdinalIgnoreCase);

            _logger.LogDebug(
                message: "メール重要性判定完了: MessageId={MessageId}, Important={Important}",
                gmail.Id,
                isImportant
            );

            return isImportant;
        }

        /// <summary>
        /// HTMLコンテンツからプレーンテキストを非同期で抽出します
        /// </summary>
        /// <param name="htmlContent">変換元のHTMLコンテンツ</param>
        /// <returns>HTMLタグが除去されたプレーンテキスト</returns>
        public async Task<string> ExtractPlainTextFromHtmlAsync(string htmlContent)
        {
            if(string.IsNullOrWhiteSpace(value: htmlContent))
            {
                _logger.LogWarning(message: "HTMLコンテンツが空またはnullです");
                return string.Empty;
            }

            _logger.LogDebug(message: "HTMLからプレーンテキストを抽出中");

            await Task.Delay(millisecondsDelay: 1); // 非同期操作のプレースホルダー

            // 簡単なHTMLタグ除去処理
            var plainText = Regex.Replace(
                input: htmlContent,
                pattern: "<[^>]+>",
                replacement: ""
            );
            plainText = WebUtility.HtmlDecode(value: plainText);
            plainText = plainText.Trim();

            _logger.LogDebug(
                message: "HTMLからプレーンテキスト抽出完了: 元の長さ={OriginalLength}, 抽出後の長さ={PlainTextLength}",
                htmlContent.Length,
                plainText.Length
            );

            return plainText;
        }

        /// <summary>
        /// メールからキーワードを非同期で抽出します
        /// </summary>
        /// <param name="gmail">キーワード抽出対象のメールメッセージ</param>
        /// <returns>抽出されたキーワードのコレクション</returns>
        public async Task<IEnumerable<string>> ExtractKeywordsAsync(GmailMessage gmail)
        {
            if(gmail == null)
            {
                _logger.LogWarning(message: "メールオブジェクトがnullです");
                return Enumerable.Empty<string>();
            }

            _logger.LogDebug(message: "メールからキーワードを抽出中: MessageId={MessageId}", args: gmail.Id);

            await Task.Delay(millisecondsDelay: 1); // 非同期操作のプレースホルダー

            var keywords = new List<string>();

            // 件名からキーワードを抽出
            if(!string.IsNullOrWhiteSpace(value: gmail.Subject))
            {
                keywords.AddRange(collection: ExtractKeywordsFromText(text: gmail.Subject));
            }

            // 本文からキーワードを抽出
            if(!string.IsNullOrWhiteSpace(value: gmail.Body))
            {
                var plainText = await ExtractPlainTextFromHtmlAsync(htmlContent: gmail.Body);
                keywords.AddRange(collection: ExtractKeywordsFromText(text: plainText));
            }

            // 重複を除去し、空でないキーワードのみを返す
            var uniqueKeywords = keywords
                                 .Where(predicate: k => !string.IsNullOrWhiteSpace(value: k))
                                 .Distinct(comparer: StringComparer.OrdinalIgnoreCase)
                                 .ToList();

            _logger.LogDebug(
                message: "キーワード抽出完了: MessageId={MessageId}, キーワード数={KeywordCount}",
                gmail.Id,
                uniqueKeywords.Count
            );

            return uniqueKeywords;
        }

        /// <summary>
        /// メールコンテンツを非同期でサニタイズします
        /// </summary>
        /// <param name="gmail">サニタイズ対象のメールメッセージ</param>
        /// <returns>サニタイズされたメールメッセージ</returns>
        public async Task<GmailMessage> SanitizeEmailContentAsync(GmailMessage gmail)
        {
            if(gmail == null)
            {
                _logger.LogWarning(message: "メールオブジェクトがnullです");
                throw new ArgumentNullException(paramName: nameof(gmail));
            }

            _logger.LogDebug(message: "メールコンテンツをサニタイズ中: MessageId={MessageId}", args: gmail.Id);

            await Task.Delay(millisecondsDelay: 1); // 非同期操作のプレースホルダー

            // サニタイズされた件名
            var sanitizedSubject = SanitizeText(text: gmail.Subject);

            // サニタイズされた本文
            var sanitizedBody = SanitizeText(text: gmail.Body);

            // 新しいEmailMessageインスタンスを作成（既存のプロパティを保持）
            var sanitizedEmail = new GmailMessage(
                id: gmail.Id,
                threadId: gmail.ThreadId,
                subject: sanitizedSubject,
                from: gmail.From,
                to: gmail.To,
                cc: gmail.Cc,
                bcc: gmail.Bcc,
                receivedDate: gmail.ReceivedDate,
                body: sanitizedBody,
                isRead: gmail.IsRead,
                isImportant: gmail.IsImportant,
                labels: gmail.Labels,
                attachments: gmail.Attachments
            );

            _logger.LogDebug(message: "メールコンテンツサニタイズ完了: MessageId={MessageId}", args: gmail.Id);

            return sanitizedEmail;
        }

        /// <summary>
        /// メールが有効かどうかを判定します
        /// </summary>
        /// <param name="gmail">判定するメール</param>
        /// <returns>有効な場合はtrue、そうでなければfalse</returns>
        public bool IsValidEmail(GmailMessage gmail)
        {
            if(gmail == null)
            {
                _logger.LogWarning(message: "メールオブジェクトがnullです");
                return false;
            }

            if(string.IsNullOrWhiteSpace(value: gmail.Subject))
            {
                _logger.LogWarning(message: "メールの件名が空です: MessageId={MessageId}", args: gmail.Id);
                return false;
            }

            if(string.IsNullOrWhiteSpace(value: gmail.From))
            {
                _logger.LogWarning(message: "メールの送信者が指定されていません: MessageId={MessageId}", args: gmail.Id);
                return false;
            }

            if(string.IsNullOrWhiteSpace(value: gmail.To))
            {
                _logger.LogWarning(message: "メールの宛先が指定されていません: MessageId={MessageId}", args: gmail.Id);
                return false;
            }

            return true;
        }

        /// <summary>
        /// メールを重要度でフィルタリングします
        /// </summary>
        /// <param name="emails">フィルタリングするメールのコレクション</param>
        /// <param name="onlyImportant">重要なメールのみを取得するかどうか</param>
        /// <returns>フィルタリングされたメールのコレクション</returns>
        public IEnumerable<GmailMessage> FilterByImportance(
            IEnumerable<GmailMessage> emails,
            bool                      onlyImportant = false
        )
        {
            if(!onlyImportant)
            {
                return emails;
            }

            // 重要なメールの判定ロジック（例：特定のラベルがある、添付ファイルがある等）
            return emails.Where(
                predicate: email =>
                    email.Labels.Any(
                        predicate: label => label.Name.Contains(
                            value: "IMPORTANT",
                            comparisonType: StringComparison.OrdinalIgnoreCase
                        )
                    )                                                                                       ||
                    email.Attachments.Any()                                                                 ||
                    email.Subject.Contains(value: "緊急", comparisonType: StringComparison.OrdinalIgnoreCase) ||
                    email.Subject.Contains(value: "重要", comparisonType: StringComparison.OrdinalIgnoreCase)
            );
        }

        /// <summary>
        /// テキストから単純なキーワードを抽出します
        /// </summary>
        /// <param name="text">抽出元のテキスト</param>
        /// <returns>抽出されたキーワードのコレクション</returns>
        private IEnumerable<string> ExtractKeywordsFromText(string text)
        {
            if(string.IsNullOrWhiteSpace(value: text))
            {
                return Enumerable.Empty<string>();
            }

            // 単語を分割（アルファベット、ひらがな、カタカナ、漢字のみ）
            var words = Regex
                        .Matches(input: text, pattern: @"[\p{L}\p{N}]+")
                        .Select(selector: m => m.Value)
                        .Where(predicate: w => w.Length >= 2) // 2文字以上のキーワードのみ
                        .ToList();

            return words;
        }

        /// <summary>
        /// テキストをサニタイズします
        /// </summary>
        /// <param name="text">サニタイズ対象のテキスト</param>
        /// <returns>サニタイズされたテキスト</returns>
        private string SanitizeText(string text)
        {
            if(string.IsNullOrWhiteSpace(value: text))
            {
                return string.Empty;
            }

            // 危険な文字列を除去
            var sanitized = text
                            .Replace(
                                oldValue: "<script",
                                newValue: "&lt;script",
                                comparisonType: StringComparison.OrdinalIgnoreCase
                            )
                            .Replace(
                                oldValue: "javascript:",
                                newValue: "blocked:",
                                comparisonType: StringComparison.OrdinalIgnoreCase
                            )
                            .Replace(
                                oldValue: "data:text/html",
                                newValue: "blocked:",
                                comparisonType: StringComparison.OrdinalIgnoreCase
                            );

            // 制御文字を除去
            sanitized = Regex.Replace(
                input: sanitized,
                pattern: @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]",
                replacement: ""
            );

            return sanitized.Trim();
        }
    }
}
