using Hypergraphia.Console.Commands;
using Hypergraphia.Core.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace Hypergraphia.Console;

/// <summary>
/// Hypergraphia Consoleアプリケーションのメインプログラム
/// </summary>
public class Program
{
    /// <summary>
    /// アプリケーションのエントリーポイント
    /// </summary>
    /// <param name="args">コマンドライン引数</param>
    /// <returns>終了コード</returns>
    public static async Task<int> Main(string[] args)
    {
        // ホストビルダーを作成
        var host = CreateHostBuilder(args).Build();

        try
        {
            // ルートコマンドを作成
            var rootCommand = new RootCommand("Hypergraphia - Personal Knowledge Management Tool");

            // サブコマンドを追加
            var importCommand = CreateImportCommand(host.Services);
            rootCommand.AddCommand(importCommand);

            // コマンドを実行
            return await rootCommand.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "アプリケーション実行中にエラーが発生しました");
            return 1;
        }
        finally
        {
            await host.StopAsync();
            host.Dispose();
        }
    }

    /// <summary>
    /// ホストビルダーを作成します
    /// </summary>
    /// <param name="args">コマンドライン引数</param>
    /// <returns>ホストビルダー</returns>
    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                    optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                // Hypergraphia.Coreサービスを登録
                services.AddHypergraphiaCore(context.Configuration);
                services.AddHypergraphiaLogging(context.Configuration);

                // Consoleコマンドを登録
                services.AddScoped<ImportGmailCommand>();
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            });

    /// <summary>
    /// インポートコマンドを作成します
    /// </summary>
    /// <param name="serviceProvider">サービスプロバイダー</param>
    /// <returns>インポートコマンド</returns>
    private static Command CreateImportCommand(IServiceProvider serviceProvider)
    {
        var importCommand = new Command("import", "外部サービスからデータをインポートします");

        // Gmail インポートコマンド
        var gmailCommand = new Command("gmail", "GmailからNotionにメールをインポートします");

        var queryOption = new Option<string>(
            name: "--query",
            description: "Gmail検索クエリ（例: 'is:unread'）",
            getDefaultValue: () => "is:unread");

        var maxResultsOption = new Option<int>(
            name: "--max-results",
            description: "取得する最大メール数",
            getDefaultValue: () => 10);

        var databaseIdOption = new Option<string>(
            name: "--database-id",
            description: "NotionデータベースID（設定ファイルのデフォルト値を上書きします）");

        gmailCommand.AddOption(queryOption);
        gmailCommand.AddOption(maxResultsOption);
        gmailCommand.AddOption(databaseIdOption);

        gmailCommand.SetHandler(async (query, maxResults, databaseId) =>
        {
            var command = serviceProvider.GetRequiredService<ImportGmailCommand>();
            await command.ExecuteAsync(query, maxResults, databaseId);
        }, queryOption, maxResultsOption, databaseIdOption);

        importCommand.AddCommand(gmailCommand);
        return importCommand;
    }
}
