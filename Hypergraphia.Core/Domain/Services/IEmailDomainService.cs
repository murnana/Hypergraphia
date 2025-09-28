using Hypergraphia.Core.Domain.Entities;

namespace Hypergraphia.Core.Domain.Services
{
    /// <summary>
    /// メールドメインに関するビジネスロジックを提供するドメインサービスインターフェースです。
    /// メールの重要性判定、コンテンツ変換、キーワード抽出などの専門的な処理を担当します。
    /// </summary>
    public interface IEmailDomainService
    {
        /// <summary>
        /// メールが重要かどうかを非同期で判定します。
        /// </summary>
        /// <param name="gmail">判定対象のメールメッセージ。</param>
        /// <returns>メールが重要と判定された場合はtrue、それ以外はfalse。</returns>
        Task<bool> IsEmailImportantAsync(GmailMessage gmail);

        /// <summary>
        /// HTMLコンテンツからプレーンテキストを非同期で抽出します。
        /// </summary>
        /// <param name="htmlContent">変換元のHTMLコンテンツ。</param>
        /// <returns>HTMLタグが除去されたプレーンテキスト。</returns>
        Task<string> ExtractPlainTextFromHtmlAsync(string htmlContent);

        /// <summary>
        /// メールからキーワードを非同期で抽出します。
        /// </summary>
        /// <param name="gmail">キーワード抽出対象のメールメッセージ。</param>
        /// <returns>抽出されたキーワードのコレクション。</returns>
        Task<IEnumerable<string>> ExtractKeywordsAsync(GmailMessage gmail);

        /// <summary>
        /// メールコンテンツを非同期でサニタイズします。
        /// </summary>
        /// <param name="gmail">サニタイズ対象のメールメッセージ。</param>
        /// <returns>サニタイズされたメールメッセージ。</returns>
        Task<GmailMessage> SanitizeEmailContentAsync(GmailMessage gmail);
    }
}
