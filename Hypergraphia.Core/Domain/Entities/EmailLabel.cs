namespace Hypergraphia.Core.Domain.Entities;

/// <summary>
/// Gmailのラベル情報を表すドメインエンティティです。
/// システムラベル（受信トレイ、送信済みなど）とユーザー定義ラベルの両方を扱います。
/// </summary>
public class EmailLabel
{
    /// <summary>
    /// ラベルの一意識別子を取得または設定します。
    /// Gmail APIによって割り当てられる固有のIDです。
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// ラベルの表示名を取得または設定します。
    /// ユーザーに表示されるラベル名です。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ラベルの種類を取得または設定します。
    /// システム定義かユーザー定義かを区別します。
    /// </summary>
    public EmailLabelType Type { get; set; }
}

/// <summary>
/// メールラベルの種類を表す列挙型です。
/// Gmailのラベルシステムにおける分類を示します。
/// </summary>
public enum EmailLabelType
{
    /// <summary>
    /// Gmailによって事前定義されたシステムラベル。
    /// 受信トレイ、送信済み、下書き、スパムなどが含まれます。
    /// </summary>
    System,

    /// <summary>
    /// ユーザーが作成したカスタムラベル。
    /// プロジェクト名、カテゴリなどユーザー独自の分類に使用されます。
    /// </summary>
    User
}