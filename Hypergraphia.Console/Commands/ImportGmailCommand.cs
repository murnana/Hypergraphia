using Hypergraphia.Core.Application.DTOs;
using Hypergraphia.Core.Application.UseCases;
using Hypergraphia.Core.Domain.Entities;
using Hypergraphia.Core.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hypergraphia.Console.Commands;

/// <summary>
/// GmailからNotionへのインポートを実行するコマンド
/// </summary>
public class ImportGmailCommand
{
    private readonly GetEmailsUseCase _getEmailsUseCase;
    private readonly ILogger<ImportGmailCommand> _logger;
    private readonly NotionConfiguration _notionConfig;

    /// <summary>
    /// ImportGmailCommandの新しいインスタンスを初期化します
    /// </summary>
    /// <param name="getEmailsUseCase">メール取得ユースケース</param>
    /// <param name="notionConfig">Notion設定</param>
    /// <param name="logger">ロガー</param>
    public ImportGmailCommand(
        GetEmailsUseCase getEmailsUseCase,
        IOptions<NotionConfiguration> notionConfig,
        ILogger<ImportGmailCommand> logger)
    {
        _getEmailsUseCase = getEmailsUseCase ?? throw new ArgumentNullException(nameof(getEmailsUseCase));
        _notionConfig = notionConfig?.Value ?? throw new ArgumentNullException(nameof(notionConfig));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// GmailからNotionへのインポートを実行します
    /// </summary>
    /// <param name="query">Gmail検索クエリ</param>
    /// <param name="maxResults">取得する最大メール数</param>
    /// <param name="databaseId">NotionデータベースID（省略可能）</param>
    /// <returns>処理結果</returns>
    public async Task ExecuteAsync(string query, int maxResults, string? databaseId = null)
    {
        try
        {
            _logger.LogInformation("Gmailインポート開始: クエリ={Query}, 最大件数={MaxResults}", query, maxResults);

            // Notionデータベース IDを決定
            var targetDatabaseId = databaseId ?? _notionConfig.DefaultDatabaseId;
            if (string.IsNullOrEmpty(targetDatabaseId))
            {
                System.Console.WriteLine("エラー: NotionデータベースIDが指定されていません。");
                System.Console.WriteLine("設定ファイルでDefaultDatabaseIdを設定するか、--database-idオプションを使用してください。");
                return;
            }

            // 進行状況表示用のコンソール出力
            System.Console.WriteLine($"Gmail検索中... (クエリ: {query})");

            // リクエストDTOを作成
            var request = new GetEmailsRequestDto
            {
                Query = query,
                MaxResults = maxResults,
                DatabaseId = targetDatabaseId
            };

            // メール取得と変換を実行
            var response = await _getEmailsUseCase.ExecuteAsync(request);

            // 結果を表示
            DisplayResults(response);

            _logger.LogInformation("Gmailインポート完了: 処理件数={ProcessedCount}, 成功件数={SuccessCount}, 失敗件数={FailedCount}",
                response.ProcessedCount, response.SuccessCount, response.FailedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gmailインポート中にエラーが発生しました");
            System.Console.WriteLine($"エラーが発生しました: {ex.Message}");

            if (ex.InnerException != null)
            {
                System.Console.WriteLine($"詳細: {ex.InnerException.Message}");
            }
        }
    }

    /// <summary>
    /// 実行結果をコンソールに表示します
    /// </summary>
    /// <param name="response">実行結果</param>
    private static void DisplayResults(EmailsResponseDto response)
    {
        System.Console.WriteLine();
        System.Console.WriteLine("=== インポート結果 ===");
        System.Console.WriteLine($"処理件数: {response.ProcessedCount}");
        System.Console.WriteLine($"成功件数: {response.SuccessCount}");
        System.Console.WriteLine($"失敗件数: {response.FailedCount}");

        if (response.Errors.Any())
        {
            System.Console.WriteLine();
            System.Console.WriteLine("=== エラー詳細 ===");
            foreach (var error in response.Errors)
            {
                System.Console.WriteLine($"- {error}");
            }
        }

        if (response.CreatedPages.Any())
        {
            System.Console.WriteLine();
            System.Console.WriteLine("=== 作成されたページ ===");
            foreach (var page in response.CreatedPages)
            {
                System.Console.WriteLine($"- {page.Title} (ID: {page.PageId})");
            }
        }

        System.Console.WriteLine();
        System.Console.WriteLine("インポート処理が完了しました。");
    }
}