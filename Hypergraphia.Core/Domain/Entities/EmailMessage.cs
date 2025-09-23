namespace Hypergraphia.Core.Domain.Entities;

/// <summary>
/// Gmail APIから取得したメールメッセージを表すドメインエンティティです。
/// このクラスはメールの基本情報、ラベル、添付ファイルなどの包括的なデータを保持します。
/// </summary>
public class EmailMessage
{
    /// <summary>
    /// メールメッセージの一意識別子を取得または設定します。
    /// Gmail APIによって割り当てられる固有のIDです。
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// メールが属するスレッドの識別子を取得または設定します。
    /// 同じ会話に属するメールは同じスレッドIDを持ちます。
    /// </summary>
    public string ThreadId { get; set; } = string.Empty;

    /// <summary>
    /// メールの件名を取得または設定します。
    /// 件名が空の場合は空文字列となります。
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// 送信者のメールアドレスを取得または設定します。
    /// 表示名が含まれる場合は「表示名 &lt;メールアドレス&gt;」の形式になります。
    /// </summary>
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// 受信者のメールアドレスを取得または設定します。
    /// 複数の受信者がいる場合はカンマ区切りで格納されます。
    /// </summary>
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// CCの受信者リストを取得または設定します。
    /// CCが設定されていない場合はnullになります。
    /// </summary>
    public string? Cc { get; set; }

    /// <summary>
    /// BCCの受信者リストを取得または設定します。
    /// BCCが設定されていない場合はnullになります。
    /// </summary>
    public string? Bcc { get; set; }

    /// <summary>
    /// メールが受信された日時を取得または設定します。
    /// UTCタイムゾーンで保存されます。
    /// </summary>
    public DateTime ReceivedDate { get; set; }

    /// <summary>
    /// メールの本文（HTML形式）を取得または設定します。
    /// プレーンテキストのみの場合もこのプロパティに格納されます。
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// HTMLタグを除去したプレーンテキストの本文を取得または設定します。
    /// HTMLメールの場合に自動生成されるテキスト版です。
    /// </summary>
    public string? PlainTextBody { get; set; }

    /// <summary>
    /// メールが既読かどうかを示す値を取得または設定します。
    /// trueの場合は既読、falseの場合は未読です。
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// メールが重要としてマークされているかを示す値を取得または設定します。
    /// Gmailの重要マーカーまたは優先度の高いメールの場合にtrueになります。
    /// </summary>
    public bool IsImportant { get; set; }

    /// <summary>
    /// メールに適用されているラベルのリストを取得または設定します。
    /// Gmailのシステムラベル（受信トレイ、送信済みなど）とユーザー定義ラベルが含まれます。
    /// </summary>
    public List<EmailLabel> Labels { get; set; } = new();

    /// <summary>
    /// メールに添付されたファイルのリストを取得または設定します。
    /// インライン画像とファイル添付の両方が含まれます。
    /// </summary>
    public List<EmailAttachment> Attachments { get; set; } = new();
}