using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Hypergraphia.Core.Application.UseCases;
using Hypergraphia.Core.Domain.Repositories;
using Hypergraphia.Core.Domain.Services;
using Hypergraphia.Core.Infrastructure.Configuration;
using Hypergraphia.Core.Infrastructure.Gmail;
using Hypergraphia.Core.Infrastructure.Notion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hypergraphia.Core.Infrastructure;

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
        IConfiguration configuration)
    {
        // 設定の登録
        services.Configure<AppConfiguration>(configuration.GetSection("Hypergraphia"));
        services.Configure<GmailConfiguration>(configuration.GetSection("Hypergraphia:Gmail"));
        services.Configure<NotionConfiguration>(configuration.GetSection("Hypergraphia:Notion"));

        // アプリケーションサービスの登録
        services.AddScoped<GetEmailsUseCase>();

        // ドメインサービスの登録
        services.AddScoped<IEmailDomainService, EmailDomainService>();

        // インフラストラクチャサービスの登録
        services.AddScoped<IEmailRepository>(provider =>
        {
            var gmailConfig = provider.GetRequiredService<IOptions<GmailConfiguration>>().Value;
            var logger = provider.GetRequiredService<ILogger<GmailRepository>>();

            var credential = GoogleCredential.FromFile(gmailConfig.CredentialsPath)
                .CreateScoped(gmailConfig.Scopes);

            return new GmailRepository(credential, logger);
        });

        services.AddScoped<INotionRepository>(provider =>
        {
            var notionConfig = provider.GetRequiredService<IOptions<NotionConfiguration>>().Value;
            var logger = provider.GetRequiredService<ILogger<NotionRepository>>();
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

            var httpClient = httpClientFactory.CreateClient("NotionApi");
            httpClient.Timeout = TimeSpan.FromSeconds(notionConfig.TimeoutSeconds);

            return new NotionRepository(httpClient, notionConfig.ApiToken, logger);
        });

        // HTTPクライアントの登録
        services.AddHttpClient();

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
        IConfiguration configuration)
    {
        services.AddLogging(builder =>
        {
            var loggingConfig = configuration.GetSection("Hypergraphia:Logging").Get<LoggingConfiguration>();

            if (loggingConfig?.EnableConsoleLogging == true)
            {
                builder.AddConsole();
            }

            // ファイルログ設定は必要に応じて追加
            // builder.AddFile(loggingConfig?.LogFilePath ?? "logs/hypergraphia.log");

            builder.SetMinimumLevel(Enum.TryParse<LogLevel>(loggingConfig?.LogLevel, out var level)
                ? level
                : LogLevel.Information);
        });

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
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// メールが有効かどうかを判定します
    /// </summary>
    /// <param name="email">判定するメール</param>
    /// <returns>有効な場合はtrue、そうでなければfalse</returns>
    public bool IsValidEmail(Domain.Entities.EmailMessage email)
    {
        if (email == null)
        {
            _logger.LogWarning("メールオブジェクトがnullです");
            return false;
        }

        if (string.IsNullOrWhiteSpace(email.Subject))
        {
            _logger.LogWarning("メールの件名が空です: MessageId={MessageId}", email.MessageId);
            return false;
        }

        if (email.From == null)
        {
            _logger.LogWarning("メールの送信者が指定されていません: MessageId={MessageId}", email.MessageId);
            return false;
        }

        if (!email.To.Any())
        {
            _logger.LogWarning("メールの宛先が指定されていません: MessageId={MessageId}", email.MessageId);
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
    public IEnumerable<Domain.Entities.EmailMessage> FilterByImportance(
        IEnumerable<Domain.Entities.EmailMessage> emails,
        bool onlyImportant = false)
    {
        if (!onlyImportant)
        {
            return emails;
        }

        // 重要なメールの判定ロジック（例：特定のラベルがある、添付ファイルがある等）
        return emails.Where(email =>
            email.Labels.Any(label => label.Name.Contains("IMPORTANT", StringComparison.OrdinalIgnoreCase)) ||
            email.Attachments.Any() ||
            email.Subject.Contains("緊急", StringComparison.OrdinalIgnoreCase) ||
            email.Subject.Contains("重要", StringComparison.OrdinalIgnoreCase)
        );
    }
}