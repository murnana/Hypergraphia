namespace Hypergraphia.Core.Infrastructure.Configuration
{
    /// <summary>
    /// アプリケーション設定を管理するクラス
    /// </summary>
    public sealed class AppConfiguration
    {
        /// <summary>
        /// Gmail設定
        /// </summary>
        public GmailConfiguration Gmail { get; set; } = new();

        /// <summary>
        /// Notion設定
        /// </summary>
        public NotionConfiguration Notion { get; set; } = new();

        /// <summary>
        /// ログ設定
        /// </summary>
        public LoggingConfiguration Logging { get; set; } = new();
    }

    /// <summary>
    /// Gmail API設定
    /// </summary>
    public sealed class GmailConfiguration
    {
        /// <summary>
        /// Gmail API認証情報ファイルのパス
        /// </summary>
        public string CredentialsPath { get; set; } = "";

        /// <summary>
        /// OAuth2認証のスコープ
        /// </summary>
        public string[] Scopes { get; set; } = new[] { "https://www.googleapis.com/auth/gmail.readonly" };

        /// <summary>
        /// アプリケーション名
        /// </summary>
        public string ApplicationName { get; set; } = "Hypergraphia";

        /// <summary>
        /// 一回の検索で取得する最大メール数
        /// </summary>
        public int MaxResults { get; set; } = 100;
    }

    /// <summary>
    /// Notion API設定
    /// </summary>
    public sealed class NotionConfiguration
    {
        /// <summary>
        /// Notion API トークン
        /// </summary>
        public string ApiToken { get; set; } = "";

        /// <summary>
        /// デフォルトのデータベースID
        /// </summary>
        public string DefaultDatabaseId { get; set; } = "";

        /// <summary>
        /// API バージョン
        /// </summary>
        public string ApiVersion { get; set; } = "2022-06-28";

        /// <summary>
        /// HTTPリクエストのタイムアウト（秒）
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;
    }

    /// <summary>
    /// ログ設定
    /// </summary>
    public sealed class LoggingConfiguration
    {
        /// <summary>
        /// ログレベル
        /// </summary>
        public string LogLevel { get; set; } = "Information";

        /// <summary>
        /// ログファイルのパス
        /// </summary>
        public string LogFilePath { get; set; } = "logs/hypergraphia.log";

        /// <summary>
        /// コンソールへのログ出力を有効にするか
        /// </summary>
        public bool EnableConsoleLogging { get; set; } = true;

        /// <summary>
        /// ファイルへのログ出力を有効にするか
        /// </summary>
        public bool EnableFileLogging { get; set; } = true;
    }
}
