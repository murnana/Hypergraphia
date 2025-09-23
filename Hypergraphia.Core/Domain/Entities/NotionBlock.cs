namespace Hypergraphia.Core.Domain.Entities;

/// <summary>
/// Notionページ内のコンテンツブロックを表すドメインエンティティです。
/// ブロックはページコンテンツの構成要素で、それぞれ特定の型とコンテンツを持ちます。
/// </summary>
public class NotionBlock
{
    /// <summary>
    /// このブロックの一意識別子を取得または設定します。
    /// このIDはブロック作成時にNotionによって割り当てられます。
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// このコンテンツブロックの型を取得または設定します。
    /// 型によってコンテンツのレンダリング方法と構造が決まります。
    /// </summary>
    public NotionBlockType Type { get; set; }

    /// <summary>
    /// このブロックのコンテンツを取得または設定します。
    /// 構造とフォーマットはブロック型に依存します。
    /// </summary>
    public object? Content { get; set; }

    /// <summary>
    /// このブロックが子ブロックを持つかどうかを示す値を取得または設定します。
    /// ネストされたリストやトグルブロックなどの階層的コンテンツに使用されます。
    /// </summary>
    public bool HasChildren { get; set; }

    /// <summary>
    /// このブロックの下にネストされた子ブロックを取得または設定します。
    /// HasChildrenがtrueの場合にのみ適用されます。
    /// </summary>
    public List<NotionBlock> Children { get; set; } = new();
}

/// <summary>
/// Notionでサポートされているコンテンツブロックの種類を定義する列挙型です。
/// 各型には特定のコンテンツ構造とレンダリング動作があります。
/// </summary>
public enum NotionBlockType
{
    /// <summary>
    /// リッチフォーマット機能をサポートする通常の段落テキスト。
    /// 太字、斜体、リンクなどの装飾が可能です。
    /// </summary>
    Paragraph,

    /// <summary>
    /// 主要セクション用の第1レベル見出し。
    /// 最も大きなフォントサイズで表示されます。
    /// </summary>
    Heading1,

    /// <summary>
    /// サブセクション用の第2レベル見出し。
    /// 中程度のフォントサイズで表示されます。
    /// </summary>
    Heading2,

    /// <summary>
    /// 小セクション用の第3レベル見出し。
    /// 最も小さな見出しフォントサイズで表示されます。
    /// </summary>
    Heading3,

    /// <summary>
    /// オプションのネスト機能付き箇条書きリストアイテム。
    /// 階層的なリスト構造を作成できます。
    /// </summary>
    BulletedListItem,

    /// <summary>
    /// 自動番号付け機能付き番号付きリストアイテム。
    /// 順序のあるリストを作成できます。
    /// </summary>
    NumberedListItem,

    /// <summary>
    /// 子コンテンツの表示/非表示を切り替えられるトグルブロック。
    /// 折りたたみ可能なセクションの作成に使用されます。
    /// </summary>
    Toggle,

    /// <summary>
    /// シンタックスハイライト機能をサポートするコードブロック。
    /// プログラミング言語のコードの表示に使用されます。
    /// </summary>
    Code,

    /// <summary>
    /// 強調または引用テキスト用のブロッククオート。
    /// 左側にインデントラインが表示されます。
    /// </summary>
    Quote,

    /// <summary>
    /// アイコンと色付き背景を持つコールアウトブロック。
    /// 注意事項や重要な情報の強調に使用されます。
    /// </summary>
    Callout,

    /// <summary>
    /// コンテンツセクションを分離する区切り線。
    /// 視覚的な分離線として機能します。
    /// </summary>
    Divider,

    /// <summary>
    /// オプションのキャプション付き埋め込み画像。
    /// 画像ファイルの表示に使用されます。
    /// </summary>
    Image,

    /// <summary>
    /// 埋め込み動画コンテンツ。
    /// 動画ファイルやYouTubeなどの動画プラットフォームのコンテンツ表示に使用されます。
    /// </summary>
    Video,

    /// <summary>
    /// ファイル添付ブロック。
    /// ドキュメントやその他のファイルの添付に使用されます。
    /// </summary>
    File,

    /// <summary>
    /// 埋め込みWebコンテンツまたはブックマーク。
    /// 外部Webサイトやサービスのコンテンツを埋め込むために使用されます。
    /// </summary>
    Embed
}