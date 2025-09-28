namespace Hypergraphia.Core.Application.DTOs
{
    /// <summary>
    /// メール取得リクエストのパラメータを表すデータ転送オブジェクトです。
    /// UIレイヤーからアプリケーション層へのメール検索条件を伝達するために使用されます。
    /// </summary>
    public sealed class GetEmailsRequestDto
    {
        /// <summary>
        /// メール検索のクエリ文字列を取得または設定します。
        /// Gmail APIの検索構文に従った条件を指定できます（例：「from:example@gmail.com has:attachment」）。
        /// </summary>
        public string? Query { get; set; }

        /// <summary>
        /// 検索対象のラベルIDのコレクションを取得または設定します。
        /// 指定されたラベルが付いたメールのみを取得対象とします。
        /// </summary>
        public IEnumerable<string>? LabelIds { get; set; }

        /// <summary>
        /// 一度に取得する最大結果数を取得または設定します。
        /// デフォルトは50件で、パフォーマンスを考慮して適切な値を設定してください。
        /// </summary>
        public int MaxResults { get; set; } = 50;

        /// <summary>
        /// ページネーション用のトークンを取得または設定します。
        /// 続きのページを取得する際に前のレスポンスから得られたトークンを指定します。
        /// </summary>
        public string? PageToken { get; set; }

        /// <summary>
        /// スパムフォルダとゴミ箱のメールを検索結果に含めるかどうかを示す値を取得または設定します。
        /// デフォルトはfalse（通常のメールのみ）です。
        /// </summary>
        public bool IncludeSpamTrash { get; set; } = false;

        /// <summary>
        /// NotionデータベースIDを取得または設定します。
        /// メールをインポートする先のNotionデータベースを指定します。
        /// </summary>
        public string? DatabaseId { get; set; }
    }
}
