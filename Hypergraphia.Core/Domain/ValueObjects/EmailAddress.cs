namespace Hypergraphia.Core.Domain.ValueObjects
{
    /// <summary>
    /// メールアドレスと表示名を表す値オブジェクトです。
    /// 不変オブジェクトとして設計されており、メールアドレスの検証機能を提供します。
    /// </summary>
    public record EmailAddress
    {
        /// <summary>
        /// EmailAddressの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="address">メールアドレス。必須で空文字や空白文字は許可されません。</param>
        /// <param name="displayName">表示名。オプショナルで、nullまたは空文字が許可されます。</param>
        /// <exception cref="ArgumentException">メールアドレスが空文字または空白文字の場合にスローされます。</exception>
        public EmailAddress(string address, string? displayName = null)
        {
            if(string.IsNullOrWhiteSpace(value: address))
            {
                throw new ArgumentException(message: "メールアドレスは空にできません", paramName: nameof(address));
            }

            Address     = address.Trim();
            DisplayName = displayName?.Trim();
        }

        /// <summary>
        /// メールアドレスを取得します。
        /// 必須プロパティで、有効なメールアドレス形式である必要があります。
        /// </summary>
        public string Address { get; }

        /// <summary>
        /// 表示名を取得します。
        /// オプショナルで、「田中太郎」のような人間が読みやすい名前を格納します。
        /// </summary>
        public string? DisplayName { get; }

        /// <summary>
        /// メールアドレスを文字列形式で返します。
        /// 表示名がある場合は「表示名 &lt;メールアドレス&gt;」形式、ない場合はメールアドレスのみを返します。
        /// </summary>
        /// <returns>フォーマットされたメールアドレス文字列。</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(value: DisplayName)
                ? Address
                : $"{DisplayName} <{Address}>";
        }
    }
}
