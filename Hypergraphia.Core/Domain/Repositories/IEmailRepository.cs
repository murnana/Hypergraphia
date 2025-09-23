using Hypergraphia.Core.Domain.Entities;

namespace Hypergraphia.Core.Domain.Repositories;

/// <summary>
/// メール情報のデータアクセスを抽象化するリポジトリインターフェースです。
/// Gmail APIやその他のメールプロバイダーとのやり取りを隠蔽します。
/// </summary>
public interface IEmailRepository
{
    /// <summary>
    /// 指定されたクエリ条件に基づいてメールメッセージを非同期で取得します。
    /// </summary>
    /// <param name="query">メール検索条件を含むクエリオブジェクト。</param>
    /// <param name="cancellationToken">非同期操作のキャンセル用トークン。</param>
    /// <returns>条件に一致するメールメッセージのコレクション。</returns>
    Task<IEnumerable<EmailMessage>> GetMessagesAsync(EmailQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// 指定されたメッセージIDに基づいて単一のメールメッセージを非同期で取得します。
    /// </summary>
    /// <param name="messageId">取得するメールメッセージの一意識別子。</param>
    /// <param name="cancellationToken">非同期操作のキャンセル用トークン。</param>
    /// <returns>指定されたIDのメールメッセージ。存在しない場合はnull。</returns>
    Task<EmailMessage?> GetMessageByIdAsync(string messageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 利用可能なすべてのメールラベルを非同期で取得します。
    /// </summary>
    /// <param name="cancellationToken">非同期操作のキャンセル用トークン。</param>
    /// <returns>システムラベルとユーザー定義ラベルのコレクション。</returns>
    Task<IEnumerable<EmailLabel>> GetLabelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 指定されたメールの添付ファイルデータを非同期で取得します。
    /// </summary>
    /// <param name="messageId">添付ファイルを含むメールメッセージの識別子。</param>
    /// <param name="attachmentId">取得する添付ファイルの識別子。</param>
    /// <param name="cancellationToken">非同期操作のキャンセル用トークン。</param>
    /// <returns>添付ファイルのバイナリデータ。</returns>
    Task<byte[]> GetAttachmentDataAsync(string messageId, string attachmentId, CancellationToken cancellationToken = default);
}

/// <summary>
/// メール検索のクエリ条件を表すクラスです。
/// フィルタリング、ページネーション、ラベル指定などの検索オプションを提供します。
/// </summary>
public class EmailQuery
{
    /// <summary>
    /// 検索クエリ文字列を取得または設定します。
    /// Gmail APIの検索構文に従った条件を指定できます（例：「from:sender@example.com」）。
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// 検索対象のラベルIDのコレクションを取得または設定します。
    /// 指定されたラベルが付いたメールのみを取得します。
    /// </summary>
    public IEnumerable<string>? LabelIds { get; set; }

    /// <summary>
    /// 一度に取得する最大結果数を取得または設定します。
    /// デフォルトは50件で、API制限により100件を超えることはできません。
    /// </summary>
    public int MaxResults { get; set; } = 50;

    /// <summary>
    /// ページネーション用のトークンを取得または設定します。
    /// 次のページを取得する際に前のレスポンスから得られたトークンを指定します。
    /// </summary>
    public string? PageToken { get; set; }

    /// <summary>
    /// スパムフォルダとゴミ箱のメールを検索結果に含めるかどうかを示す値を取得または設定します。
    /// デフォルトはfalse（スパムとゴミ箱は除外）です。
    /// </summary>
    public bool IncludeSpamTrash { get; set; } = false;
}