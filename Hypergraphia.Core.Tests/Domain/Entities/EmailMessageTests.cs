using Hypergraphia.Core.Domain.Entities;

namespace Hypergraphia.Core.Tests.Domain.Entities;

/// <summary>
/// EmailMessageエンティティの単体テストです。
/// エンティティの初期化とプロパティ設定をテストします。
/// </summary>
[TestFixture]
public class EmailMessageTests
{
    [Test]
    public void コンストラクタ_デフォルト値_すべてのプロパティが初期化される()
    {
        // Act
        var emailMessage = new EmailMessage();

        // Assert
        Assert.That(emailMessage.Id, Is.EqualTo(string.Empty));
        Assert.That(emailMessage.ThreadId, Is.EqualTo(string.Empty));
        Assert.That(emailMessage.Subject, Is.EqualTo(string.Empty));
        Assert.That(emailMessage.From, Is.EqualTo(string.Empty));
        Assert.That(emailMessage.To, Is.EqualTo(string.Empty));
        Assert.That(emailMessage.Cc, Is.Null);
        Assert.That(emailMessage.Bcc, Is.Null);
        Assert.That(emailMessage.ReceivedDate, Is.EqualTo(default(DateTime)));
        Assert.That(emailMessage.Body, Is.EqualTo(string.Empty));
        Assert.That(emailMessage.PlainTextBody, Is.Null);
        Assert.That(emailMessage.IsRead, Is.False);
        Assert.That(emailMessage.IsImportant, Is.False);
        Assert.That(emailMessage.Labels, Is.Not.Null);
        Assert.That(emailMessage.Labels, Is.Empty);
        Assert.That(emailMessage.Attachments, Is.Not.Null);
        Assert.That(emailMessage.Attachments, Is.Empty);
    }

    [Test]
    public void プロパティ設定_すべての値_正常に設定される()
    {
        // Arrange
        var emailMessage = new EmailMessage();
        var receivedDate = new DateTime(2025, 1, 15, 10, 30, 0);
        var label = new EmailLabel { Id = "INBOX", Name = "受信トレイ", Type = EmailLabelType.System };
        var attachment = new EmailAttachment
        {
            Id = "att1",
            FileName = "document.pdf",
            MimeType = "application/pdf",
            Size = 1024,
            IsInline = false
        };

        // Act
        emailMessage.Id = "msg123";
        emailMessage.ThreadId = "thread456";
        emailMessage.Subject = "テストメール件名";
        emailMessage.From = "sender@example.com";
        emailMessage.To = "recipient@example.com";
        emailMessage.Cc = "cc@example.com";
        emailMessage.Bcc = "bcc@example.com";
        emailMessage.ReceivedDate = receivedDate;
        emailMessage.Body = "<p>HTML本文</p>";
        emailMessage.PlainTextBody = "プレーンテキスト本文";
        emailMessage.IsRead = true;
        emailMessage.IsImportant = true;
        emailMessage.Labels.Add(label);
        emailMessage.Attachments.Add(attachment);

        // Assert
        Assert.That(emailMessage.Id, Is.EqualTo("msg123"));
        Assert.That(emailMessage.ThreadId, Is.EqualTo("thread456"));
        Assert.That(emailMessage.Subject, Is.EqualTo("テストメール件名"));
        Assert.That(emailMessage.From, Is.EqualTo("sender@example.com"));
        Assert.That(emailMessage.To, Is.EqualTo("recipient@example.com"));
        Assert.That(emailMessage.Cc, Is.EqualTo("cc@example.com"));
        Assert.That(emailMessage.Bcc, Is.EqualTo("bcc@example.com"));
        Assert.That(emailMessage.ReceivedDate, Is.EqualTo(receivedDate));
        Assert.That(emailMessage.Body, Is.EqualTo("<p>HTML本文</p>"));
        Assert.That(emailMessage.PlainTextBody, Is.EqualTo("プレーンテキスト本文"));
        Assert.That(emailMessage.IsRead, Is.True);
        Assert.That(emailMessage.IsImportant, Is.True);
        Assert.That(emailMessage.Labels, Has.Count.EqualTo(1));
        Assert.That(emailMessage.Labels[0], Is.EqualTo(label));
        Assert.That(emailMessage.Attachments, Has.Count.EqualTo(1));
        Assert.That(emailMessage.Attachments[0], Is.EqualTo(attachment));
    }

    [Test]
    public void Labels_複数のラベル追加_すべて正常に追加される()
    {
        // Arrange
        var emailMessage = new EmailMessage();
        var systemLabel = new EmailLabel { Id = "INBOX", Name = "受信トレイ", Type = EmailLabelType.System };
        var userLabel = new EmailLabel { Id = "project", Name = "プロジェクト", Type = EmailLabelType.User };

        // Act
        emailMessage.Labels.Add(systemLabel);
        emailMessage.Labels.Add(userLabel);

        // Assert
        Assert.That(emailMessage.Labels, Has.Count.EqualTo(2));
        Assert.That(emailMessage.Labels[0], Is.EqualTo(systemLabel));
        Assert.That(emailMessage.Labels[1], Is.EqualTo(userLabel));
    }

    [Test]
    public void Attachments_複数の添付ファイル追加_すべて正常に追加される()
    {
        // Arrange
        var emailMessage = new EmailMessage();
        var document = new EmailAttachment
        {
            Id = "att1",
            FileName = "document.pdf",
            MimeType = "application/pdf",
            Size = 2048,
            IsInline = false
        };
        var image = new EmailAttachment
        {
            Id = "att2",
            FileName = "image.jpg",
            MimeType = "image/jpeg",
            Size = 1024,
            IsInline = true,
            ContentId = "img001"
        };

        // Act
        emailMessage.Attachments.Add(document);
        emailMessage.Attachments.Add(image);

        // Assert
        Assert.That(emailMessage.Attachments, Has.Count.EqualTo(2));
        Assert.That(emailMessage.Attachments[0], Is.EqualTo(document));
        Assert.That(emailMessage.Attachments[1], Is.EqualTo(image));
    }
}