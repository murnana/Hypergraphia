using System.CommandLine;
using Hypergraphia.Console.Commands;
using Hypergraphia.Core.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Hypergraphia.Console
{
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
            var host = CreateHostBuilder(args: args).Build();

            try
            {
                // ルートコマンドを作成
                var rootCommand = new RootCommand(description: "Hypergraphia - Personal Knowledge Management Tool");

                // サブコマンドを追加
                var importCommand = CreateImportCommand(serviceProvider: host.Services);
                rootCommand.AddCommand(command: importCommand);

                // コマンドを実行
                return await rootCommand.InvokeAsync(args: args);
            }
            catch(Exception ex)
            {
                var logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError(exception: ex, message: "アプリケーション実行中にエラーが発生しました");
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
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args: args)
                       .ConfigureAppConfiguration(
                           configureDelegate: (context, config) =>
                           {
                               config.AddJsonFile(
                                   path: "appsettings.json",
                                   optional: false,
                                   reloadOnChange: true
                               );
                               config.AddJsonFile(
                                   path: $"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                                   optional: true,
                                   reloadOnChange: true
                               );
                               config.AddEnvironmentVariables();
                               config.AddCommandLine(args: args);
                           }
                       )
                       .ConfigureServices(
                           configureDelegate: (context, services) =>
                           {
                               // Hypergraphia.Coreサービスを登録
                               services.AddHypergraphiaCore(configuration: context.Configuration);
                               services.AddHypergraphiaLogging(configuration: context.Configuration);

                               // Consoleコマンドを登録
                               services.AddScoped<ImportGmailCommand>();
                           }
                       )
                       .ConfigureLogging(
                           configureLogging: (context, logging) =>
                           {
                               logging.ClearProviders();
                               logging.AddConsole();
                           }
                       );
        }

        /// <summary>
        /// インポートコマンドを作成します
        /// </summary>
        /// <param name="serviceProvider">サービスプロバイダー</param>
        /// <returns>インポートコマンド</returns>
        private static Command CreateImportCommand(IServiceProvider serviceProvider)
        {
            var importCommand = new Command(name: "import", description: "外部サービスからデータをインポートします");

            // Gmail インポートコマンド
            var gmailCommand = new Command(name: "gmail", description: "GmailからNotionにメールをインポートします");

            var queryOption = new Option<string>(
                name: "--query",
                description: "Gmail検索クエリ（例: 'is:unread'）",
                getDefaultValue: () => "is:unread"
            );

            var maxResultsOption = new Option<int>(
                name: "--max-results",
                description: "取得する最大メール数",
                getDefaultValue: () => 10
            );

            var databaseIdOption = new Option<string>(
                name: "--database-id",
                description: "NotionデータベースID（設定ファイルのデフォルト値を上書きします）"
            );

            gmailCommand.AddOption(option: queryOption);
            gmailCommand.AddOption(option: maxResultsOption);
            gmailCommand.AddOption(option: databaseIdOption);

            gmailCommand.SetHandler(
                handle: async (
                    query,
                    maxResults,
                    databaseId
                ) =>
                {
                    var command = serviceProvider.GetRequiredService<ImportGmailCommand>();
                    await command.ExecuteAsync(
                        query: query,
                        maxResults: maxResults,
                        databaseId: databaseId
                    );
                },
                symbol1: queryOption,
                symbol2: maxResultsOption,
                symbol3: databaseIdOption
            );

            importCommand.AddCommand(command: gmailCommand);
            return importCommand;
        }
    }
}
