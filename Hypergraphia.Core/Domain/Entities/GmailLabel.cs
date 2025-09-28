namespace Hypergraphia.Core.Domain.Entities
{
    /// <summary>
    /// Gmailのラベル情報を表すドメインエンティティです。 システムラベル（受信トレイ、送信済みなど）とユーザー定義ラベルの両方を扱います。
    /// </summary>
    public sealed class GmailLabel
    (
        string         id,
        string         name,
        GmailLabelType type
    )
    {
        /// <summary>
        /// ラベルの一意識別子を取得または設定します。 Gmail APIによって割り当てられる固有のIDです。
        /// </summary>
        public string Id { get; } = id ?? throw new ArgumentNullException(paramName: nameof(id));

        /// <summary>
        /// ラベルの表示名を取得または設定します。 ユーザーに表示されるラベル名です。
        /// </summary>
        public string Name { get; } = name ?? throw new ArgumentNullException(paramName: nameof(name));

        /// <summary>
        /// ラベルの種類を取得または設定します。 システム定義かユーザー定義かを区別します。
        /// </summary>
        public GmailLabelType Type { get; } = type;
    }
}
