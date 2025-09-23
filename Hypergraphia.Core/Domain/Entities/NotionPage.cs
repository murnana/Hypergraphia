namespace Hypergraphia.Core.Domain.Entities;

/// <summary>
/// Notionページのプロパティとコンテンツブロックを表すドメインエンティティです。
/// メールコンテンツから作成されるNotionページの核となるデータ構造をカプセル化します。
/// </summary>
public class NotionPage
{
    /// <summary>
    /// Notionページの一意識別子を取得または設定します。
    /// このIDはページ作成時にNotionによって割り当てられます。
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Notionページのタイトルを取得または設定します。
    /// 通常はメールの件名に対応します。
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// このページが属するデータベースのIDを取得または設定します。
    /// Notionデータベース内にページを作成する場合に必要です。
    /// </summary>
    public string? DatabaseId { get; set; }

    /// <summary>
    /// このページが子ページの場合の親ページIDを取得または設定します。
    /// 階層的なページ構成のためのオプショナルプロパティです。
    /// </summary>
    public string? ParentPageId { get; set; }

    /// <summary>
    /// Notionページのプロパティを取得または設定します。
    /// プロパティはページのメタデータと構造化データを定義します。
    /// </summary>
    public Dictionary<string, NotionProperty> Properties { get; set; } = new();

    /// <summary>
    /// ページ本文を構成するコンテンツブロックを取得または設定します。
    /// ブロックはテキスト、見出し、リストなどの実際のコンテンツを表します。
    /// </summary>
    public List<NotionBlock> Blocks { get; set; } = new();

    /// <summary>
    /// ページの作成タイムスタンプを取得または設定します。
    /// 通常、ページがNotionで初めて作成された時に設定されます。
    /// </summary>
    public DateTime? CreatedTime { get; set; }

    /// <summary>
    /// ページの最終編集タイムスタンプを取得または設定します。
    /// ページのコンテンツやプロパティが変更されるたびに更新されます。
    /// </summary>
    public DateTime? LastEditedTime { get; set; }
}