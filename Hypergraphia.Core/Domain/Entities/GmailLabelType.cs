namespace Hypergraphia.Core.Domain.Entities
{
    /// <summary>メールラベルの種類を表す列挙型です。 Gmailのラベルシステムにおける分類を示します。</summary>
    public enum GmailLabelType
    {
        /// <summary>Gmailによって事前定義されたシステムラベル。 受信トレイ、送信済み、下書き、スパムなどが含まれます。</summary>
        System,

        /// <summary>ユーザーが作成したカスタムラベル。 プロジェクト名、カテゴリなどユーザー独自の分類に使用されます。</summary>
        User
    }
}
