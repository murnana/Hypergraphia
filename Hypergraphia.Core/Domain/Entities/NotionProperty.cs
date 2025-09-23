namespace Hypergraphia.Core.Domain.Entities;

/// <summary>
/// Notionページのプロパティとその型および値を表すドメインエンティティです。
/// プロパティはNotionデータベースの構造化されたメタデータフィールドです。
/// </summary>
public class NotionProperty
{
    /// <summary>
    /// Notionプロパティの型を取得または設定します。
    /// この型によってプロパティの値がどのように解釈され表示されるかが決まります。
    /// </summary>
    public NotionPropertyType Type { get; set; }

    /// <summary>
    /// プロパティの値を取得または設定します。
    /// 実際のデータ型はPropertyTypeに依存します。
    /// </summary>
    public object? Value { get; set; }
}

/// <summary>
/// Notionでサポートされているプロパティの種類を定義する列挙型です。
/// 各型には特定のフォーマットと検証要件があります。
/// </summary>
public enum NotionPropertyType
{
    /// <summary>
    /// フォーマットとリンクをサポートするリッチテキストプロパティ。
    /// 太字、斜体、下線、リンクなどの装飾が可能です。
    /// </summary>
    RichText,

    /// <summary>
    /// タイトルやシンプルなテキストフィールド用のプレーンテキストプロパティ。
    /// ページのメインタイトルに使用されます。
    /// </summary>
    Title,

    /// <summary>
    /// 整数値と小数値用の数値プロパティ。
    /// 計算や並び替えに使用できます。
    /// </summary>
    Number,

    /// <summary>
    /// 事前定義されたオプションリストからの単一選択プロパティ。
    /// ステータス、カテゴリなどに使用されます。
    /// </summary>
    Select,

    /// <summary>
    /// 事前定義されたオプションリストからの複数選択プロパティ。
    /// タグ、複数カテゴリなどに使用されます。
    /// </summary>
    MultiSelect,

    /// <summary>
    /// 時刻情報を含むことができる日付プロパティ。
    /// 期限、作成日、イベント日時などに使用されます。
    /// </summary>
    Date,

    /// <summary>
    /// 検証機能付きのメールアドレスプロパティ。
    /// 有効なメールアドレス形式が必要です。
    /// </summary>
    Email,

    /// <summary>
    /// リンク検証機能付きのURLプロパティ。
    /// Webサイト、リソースへのリンクに使用されます。
    /// </summary>
    Url,

    /// <summary>
    /// true/false値用のチェックボックスプロパティ。
    /// 完了状態、有効/無効フラグなどに使用されます。
    /// </summary>
    Checkbox,

    /// <summary>
    /// ワークスペース内の人やユーザーへの参照プロパティ。
    /// 担当者、作成者などの人的リソースの指定に使用されます。
    /// </summary>
    People,

    /// <summary>
    /// ファイル添付とメディアを扱うプロパティ。
    /// 画像、ドキュメント、その他のファイルを添付できます。
    /// </summary>
    Files,

    /// <summary>
    /// データベース内の他のページへのリレーションプロパティ。
    /// ページ間の関係性を定義するために使用されます。
    /// </summary>
    Relation
}