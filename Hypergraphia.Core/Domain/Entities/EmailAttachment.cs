namespace Hypergraphia.Core.Domain.Entities;

/// <summary>
/// メールに添付されたファイルの情報を表すドメインエンティティです。
/// 通常の添付ファイルとインライン画像の両方を扱います。
/// </summary>
public class EmailAttachment
{
    /// <summary>
    /// 添付ファイルの一意識別子を取得または設定します。
    /// Gmail APIによって割り当てられる固有のIDです。
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 添付ファイルのファイル名を取得または設定します。
    /// 元のファイル名が保持されます。拡張子も含まれます。
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 添付ファイルのMIMEタイプを取得または設定します。
    /// ファイルの種類を示します（例：image/jpeg, application/pdf）。
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// 添付ファイルのサイズをバイト単位で取得または設定します。
    /// ファイルダウンロード前にサイズを確認するために使用されます。
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// ファイルがインライン表示かどうかを示す値を取得または設定します。
    /// trueの場合はメール本文内に埋め込まれた画像、falseの場合は通常の添付ファイルです。
    /// </summary>
    public bool IsInline { get; set; }

    /// <summary>
    /// インライン画像のコンテンツIDを取得または設定します。
    /// HTML本文内でimg src="cid:コンテンツID"として参照される場合に使用されます。
    /// インライン画像でない場合はnullになります。
    /// </summary>
    public string? ContentId { get; set; }
}