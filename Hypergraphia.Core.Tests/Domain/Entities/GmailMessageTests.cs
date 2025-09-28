using Hypergraphia.Core.Domain.Entities;

namespace Hypergraphia.Core.Tests.Domain.Entities
{
    /// <summary>
    /// EmailMessageエンティティの単体テストです。
    /// エンティティの初期化とプロパティ設定をテストします。
    /// </summary>
    [TestFixture]
    public class GmailMessageTests
    {
        [Test]
        public void コンストラクタ_最小限のパラメータ_すべてのプロパティが初期化される()
        {
            // Act
            var emailMessage = new GmailMessage(
                id: "msg123",
                threadId: "thread456",
                subject: "テスト件名",
                from: "sender@example.com",
                to: "recipient@example.com",
                cc: null,
                bcc: null,
                receivedDate: new(
                    year: 2025,
                    month: 1,
                    day: 15
                ),
                body: "テスト本文",
                isRead: false,
                isImportant: false,
                labels: new(),
                attachments: new()
            );

            // Assert
            Assert.That(actual: emailMessage.Id, expression: Is.EqualTo(expected: "msg123"));
            Assert.That(actual: emailMessage.ThreadId, expression: Is.EqualTo(expected: "thread456"));
            Assert.That(actual: emailMessage.Subject, expression: Is.EqualTo(expected: "テスト件名"));
            Assert.That(actual: emailMessage.From, expression: Is.EqualTo(expected: "sender@example.com"));
            Assert.That(actual: emailMessage.To, expression: Is.EqualTo(expected: "recipient@example.com"));
            Assert.That(actual: emailMessage.Cc, expression: Is.Null);
            Assert.That(actual: emailMessage.Bcc, expression: Is.Null);
            Assert.That(
                actual: emailMessage.ReceivedDate,
                expression: Is.EqualTo(
                    expected: new DateTime(
                        year: 2025,
                        month: 1,
                        day: 15
                    )
                )
            );
            Assert.That(actual: emailMessage.Body, expression: Is.EqualTo(expected: "テスト本文"));
            Assert.That(actual: emailMessage.IsRead, expression: Is.False);
            Assert.That(actual: emailMessage.IsImportant, expression: Is.False);
            Assert.That(actual: emailMessage.Labels, expression: Is.Not.Null);
            Assert.Multiple(
                testDelegate: () =>
                {
                    Assert.That(actual: emailMessage.Labels, expression: Is.Empty);
                    Assert.That(actual: emailMessage.Attachments, expression: Is.Not.Null);
                }
            );
            Assert.That(actual: emailMessage.Attachments, expression: Is.Empty);
        }

        [Test]
        public void プロパティ設定_すべての値_正常に設定される()
        {
            // Arrange
            var receivedDate = new DateTime(
                year: 2025,
                month: 1,
                day: 15,
                hour: 10,
                minute: 30,
                second: 0
            );
            var label = new GmailLabel(
                id: "INBOX",
                name: "受信トレイ",
                type: GmailLabelType.System
            );
            var attachment = new EmailAttachment(
                id: "att1",
                fileName: "document.pdf",
                mimeType: "application/pdf",
                size: 1024,
                isInline: false,
                contentId: null
            );

            var emailMessage = new GmailMessage(
                id: "initial",
                threadId: "initial_thread",
                subject: "初期件名",
                from: "initial@example.com",
                to: "initial_to@example.com",
                cc: null,
                bcc: null,
                receivedDate: DateTime.MinValue,
                body: "初期本文",
                isRead: false,
                isImportant: false,
                labels: new(),
                attachments: new()
            );

            // Act
            emailMessage.Id           = "msg123";
            emailMessage.ThreadId     = "thread456";
            emailMessage.Subject      = "テストメール件名";
            emailMessage.From         = "sender@example.com";
            emailMessage.To           = "recipient@example.com";
            emailMessage.Cc           = "cc@example.com";
            emailMessage.Bcc          = "bcc@example.com";
            emailMessage.ReceivedDate = receivedDate;
            emailMessage.Body         = "<p>HTML本文</p>";
            emailMessage.IsRead       = true;
            emailMessage.IsImportant  = true;
            emailMessage.Labels.Add(item: label);
            emailMessage.Attachments.Add(item: attachment);

            // Assert
            Assert.That(actual: emailMessage.Id, expression: Is.EqualTo(expected: "msg123"));
            Assert.That(actual: emailMessage.ThreadId, expression: Is.EqualTo(expected: "thread456"));
            Assert.That(actual: emailMessage.Subject, expression: Is.EqualTo(expected: "テストメール件名"));
            Assert.That(actual: emailMessage.From, expression: Is.EqualTo(expected: "sender@example.com"));
            Assert.That(actual: emailMessage.To, expression: Is.EqualTo(expected: "recipient@example.com"));
            Assert.That(actual: emailMessage.Cc, expression: Is.EqualTo(expected: "cc@example.com"));
            Assert.That(actual: emailMessage.Bcc, expression: Is.EqualTo(expected: "bcc@example.com"));
            Assert.That(actual: emailMessage.ReceivedDate, expression: Is.EqualTo(expected: receivedDate));
            Assert.That(actual: emailMessage.Body, expression: Is.EqualTo(expected: "<p>HTML本文</p>"));
            Assert.That(actual: emailMessage.IsRead, expression: Is.True);
            Assert.That(actual: emailMessage.IsImportant, expression: Is.True);
            Assert.That(actual: emailMessage.Labels, expression: Has.Count.EqualTo(expected: 1));
            Assert.That(actual: emailMessage.Labels[index: 0], expression: Is.EqualTo(expected: label));
            Assert.That(actual: emailMessage.Attachments, expression: Has.Count.EqualTo(expected: 1));
            Assert.That(actual: emailMessage.Attachments[index: 0], expression: Is.EqualTo(expected: attachment));
        }

        [Test]
        public void Labels_複数のラベル追加_すべて正常に追加される()
        {
            // Arrange
            var emailMessage = new GmailMessage(
                id: "msg123",
                threadId: "thread456",
                subject: "テスト件名",
                from: "sender@example.com",
                to: "recipient@example.com",
                cc: null,
                bcc: null,
                receivedDate: DateTime.Now,
                body: "テスト本文",
                isRead: false,
                isImportant: false,
                labels: new(),
                attachments: new()
            );

            var systemLabel = new GmailLabel(
                id: "INBOX",
                name: "受信トレイ",
                type: GmailLabelType.System
            );

            var userLabel = new GmailLabel(
                id: "project",
                name: "プロジェクト",
                type: GmailLabelType.User
            );

            // Act
            emailMessage.Labels.Add(item: systemLabel);
            emailMessage.Labels.Add(item: userLabel);

            // Assert
            Assert.That(actual: emailMessage.Labels, expression: Has.Count.EqualTo(expected: 2));
            Assert.That(actual: emailMessage.Labels[index: 0], expression: Is.EqualTo(expected: systemLabel));
            Assert.That(actual: emailMessage.Labels[index: 1], expression: Is.EqualTo(expected: userLabel));
        }

        [Test]
        public void Attachments_複数の添付ファイル追加_すべて正常に追加される()
        {
            // Arrange
            var emailMessage = new GmailMessage(
                id: "msg123",
                threadId: "thread456",
                subject: "テスト件名",
                from: "sender@example.com",
                to: "recipient@example.com",
                cc: null,
                bcc: null,
                receivedDate: DateTime.Now,
                body: "テスト本文",
                isRead: false,
                isImportant: false,
                labels: new(),
                attachments: new()
            );

            var document = new EmailAttachment(
                id: "att1",
                fileName: "document.pdf",
                mimeType: "application/pdf",
                size: 2048,
                isInline: false,
                contentId: null
            );

            var image = new EmailAttachment(
                id: "att2",
                fileName: "image.jpg",
                mimeType: "image/jpeg",
                size: 1024,
                isInline: true,
                contentId: "img001"
            );

            // Act
            emailMessage.Attachments.Add(item: document);
            emailMessage.Attachments.Add(item: image);

            // Assert
            Assert.That(actual: emailMessage.Attachments, expression: Has.Count.EqualTo(expected: 2));
            Assert.That(actual: emailMessage.Attachments[index: 0], expression: Is.EqualTo(expected: document));
            Assert.That(actual: emailMessage.Attachments[index: 1], expression: Is.EqualTo(expected: image));
        }
    }
}
