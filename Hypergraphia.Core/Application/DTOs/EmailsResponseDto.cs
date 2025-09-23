namespace Hypergraphia.Core.Application.DTOs;

/// <summary>
/// メール取得リクエストの応答データを表すDTOです。
/// ページネーション情報と取得されたメールのリストを含みます。
/// </summary>
public class EmailsResponseDto
{
    /// <summary>
    /// 取得されたメールのリストを取得または設定します。
    /// 要求された条件に一致するメールが含まれます。
    /// </summary>
    public List<EmailDto> Emails { get; set; } = new();

    /// <summary>
    /// 次のページを取得するためのトークンを取得または設定します。
    /// 全てのメールが取得された場合はnullになります。
    /// </summary>
    public string? NextPageToken { get; set; }
}

/// <summary>
/// メール情報を転送するためのデータ転送オブジェクトです。
/// UIレイヤーとの間でメール情報をやり取りするために使用されます。
/// </summary>
public class EmailDto
{
    /// <summary>
    /// メールメッセージの一意識別子を取得または設定します。
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// メールが属するスレッドの識別子を取得または設定します。
    /// </summary>
    public string ThreadId { get; set; } = string.Empty;

    /// <summary>
    /// メールの件名を取得または設定します。
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// 送信者のメールアドレスを取得または設定します。
    /// </summary>
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// 受信者のメールアドレスを取得または設定します。
    /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// CCの受信者リストを取得または設定します。
    /// </summary>
    public string? Cc { get; set; }

    /// <summary>
    /// BCCの受信者リストを取得または設定します。
    /// </summary>
    public string? Bcc { get; set; }

    /// <summary>
    /// メールが受信された日時を取得または設定します。
    /// </summary>
    public DateTime ReceivedDate { get; set; }

    /// <summary>
    /// メールの本文を取得または設定します。
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// プレーンテキスト形式の本文を取得または設定します。
    /// </summary>
    public string? PlainTextBody { get; set; }

    /// <summary>
    /// メールが既読かどうかを示す値を取得または設定します。
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// メールが重要としてマークされているかを示す値を取得または設定します。
    /// </summary>
    public bool IsImportant { get; set; }

    /// <summary>
    /// メールに適用されているラベルのリストを取得または設定します。
    /// </summary>
    public List<EmailLabelDto> Labels { get; set; } = new();

    /// <summary>
    /// メールに添付されたファイルのリストを取得または設定します。
    /// </summary>
    public List<EmailAttachmentDto> Attachments { get; set; } = new();
}

/// <summary>
/// メールラベル情報を転送するためのデータ転送オブジェクトです。
/// </summary>
public class EmailLabelDto
{
    /// <summary>
    /// ラベルの一意識別子を取得または設定します。
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// ラベルの表示名を取得または設定します。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// ラベルの種類を文字列で取得または設定します。
    /// </summary>
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// メール添付ファイル情報を転送するためのデータ転送オブジェクトです。
/// </summary>
public class EmailAttachmentDto
{
    /// <summary>
    /// 添付ファイルの一意識別子を取得または設定します。
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 添付ファイルのファイル名を取得または設定します。
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 添付ファイルのMIMEタイプを取得または設定します。
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// 添付ファイルのサイズをバイト単位で取得または設定します。
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// ファイルがインライン表示かどうかを示す値を取得または設定します。
    /// </summary>
    public bool IsInline { get; set; }

    /// <summary>
    /// インライン画像のコンテンツIDを取得または設定します。
    /// </summary>
    public string? ContentId { get; set; }
}