namespace Hypergraphia.Core.Domain.Entities
{
    /// <summary>
    /// Notionページのプロパティとコンテンツブロックを表すドメインエンティティです。
    /// メールコンテンツから作成されるNotionページの核となるデータ構造をカプセル化します。
    /// </summary>
    public sealed class NotionPage
    (
        string                              title,
        string?                             id             = null,
        string?                             databaseId     = null,
        string?                             parentPageId   = null,
        Dictionary<string, NotionProperty>? properties     = null,
        List<NotionBlock>?                  blocks         = null,
        DateTime?                           createdTime    = null,
        DateTime?                           lastEditedTime = null
    )
    {
        /// <summary>
        /// Notionページの一意識別子を取得または設定します。
        /// このIDはページ作成時にNotionによって割り当てられます。
        /// </summary>
        public string? Id { get; set; } = id;

        /// <summary>
        /// Notionページのタイトルを取得または設定します。
        /// 通常はメールの件名に対応します。
        /// </summary>
        public string Title { get; set; } = title ?? throw new ArgumentNullException(paramName: nameof(title));

        /// <summary>
        /// このページが属するデータベースのIDを取得または設定します。
        /// Notionデータベース内にページを作成する場合に必要です。
        /// </summary>
        public string? DatabaseId { get; set; } = databaseId;

        /// <summary>
        /// このページが子ページの場合の親ページIDを取得または設定します。
        /// 階層的なページ構成のためのオプショナルプロパティです。
        /// </summary>
        public string? ParentPageId { get; set; } = parentPageId;

        /// <summary>
        /// Notionページのプロパティを取得または設定します。
        /// プロパティはページのメタデータと構造化データを定義します。
        /// </summary>
        public Dictionary<string, NotionProperty> Properties { get; set; } = properties ?? new();

        /// <summary>
        /// ページ本文を構成するコンテンツブロックを取得または設定します。
        /// ブロックはテキスト、見出し、リストなどの実際のコンテンツを表します。
        /// </summary>
        public List<NotionBlock> Blocks { get; set; } = blocks ?? [];

        /// <summary>
        /// ページの作成タイムスタンプを取得または設定します。
        /// 通常、ページがNotionで初めて作成された時に設定されます。
        /// </summary>
        public DateTime? CreatedTime { get; set; } = createdTime;

        /// <summary>
        /// ページの最終編集タイムスタンプを取得または設定します。
        /// ページのコンテンツやプロパティが変更されるたびに更新されます。
        /// </summary>
        public DateTime? LastEditedTime { get; set; } = lastEditedTime;
    }
}
