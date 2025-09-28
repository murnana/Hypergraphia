using Hypergraphia.Core.Application.DTOs;
using Hypergraphia.Core.Application.UseCases;
using Hypergraphia.Core.Domain.Entities;
using Hypergraphia.Core.Domain.Repositories;
using Hypergraphia.Core.Domain.Services;
using Moq;

namespace Hypergraphia.Core.Tests.Application.UseCases
{
    /// <summary>
    /// GetEmailsUseCaseの単体テストです。
    /// ユースケースのビジネスロジックをモックを使用してテストします。
    /// </summary>
    [TestFixture]
    public class GetEmailsUseCaseTests
    {
        [SetUp]
        public void Setup()
        {
            _mockEmailRepository = new();
            _mockDomainService = new();
            _useCase = new(emailRepository: _mockEmailRepository.Object, domainService: _mockDomainService.Object);
        }

        private Mock<IEmailRepository>    _mockEmailRepository = null!;
        private Mock<IEmailDomainService> _mockDomainService   = null!;
        private GetEmailsUseCase          _useCase             = null!;

        [Test]
        public async Task ExecuteAsync_正常なリクエスト_メールが取得される()
        {
            // Arrange
            var request = new GetEmailsRequestDto
            {
                Query            = "from:test@example.com",
                MaxResults       = 10,
                IncludeSpamTrash = false
            };

            var emailMessage = new GmailMessage(
                id: "msg123",
                threadId: "thread456",
                subject: "テストメール",
                from: "test@example.com",
                to: "recipient@example.com",
                cc: null,
                bcc: null,
                receivedDate: new(
                    year: 2025,
                    month: 1,
                    day: 15,
                    hour: 10,
                    minute: 0,
                    second: 0
                ),
                body: "テスト本文",
                isRead: false,
                isImportant: false,
                labels: new(),
                attachments: new()
            );

            var sanitizedEmail = new GmailMessage(
                id: emailMessage.Id,
                threadId: emailMessage.ThreadId,
                subject: emailMessage.Subject,
                from: emailMessage.From,
                to: emailMessage.To,
                cc: emailMessage.Cc,
                bcc: emailMessage.Bcc,
                receivedDate: emailMessage.ReceivedDate,
                body: "サニタイズされた本文",
                isRead: emailMessage.IsRead,
                isImportant: emailMessage.IsImportant,
                labels: emailMessage.Labels,
                attachments: emailMessage.Attachments
            );

            _mockEmailRepository
                .Setup(expression: x => x.GetMessagesAsync(It.IsAny<EmailQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(value: new[] { emailMessage });

            _mockDomainService
                .Setup(expression: x => x.SanitizeEmailContentAsync(It.IsAny<GmailMessage>()))
                .ReturnsAsync(value: sanitizedEmail);

            // Act
            var result = await _useCase.ExecuteAsync(request: request);

            // Assert
            Assert.That(actual: result, expression: Is.Not.Null);
            Assert.That(actual: result.Emails, expression: Has.Count.EqualTo(expected: 1));
            Assert.That(actual: result.Emails[index: 0].Id, expression: Is.EqualTo(expected: "msg123"));
            Assert.That(actual: result.Emails[index: 0].Subject, expression: Is.EqualTo(expected: "テストメール"));
            Assert.That(actual: result.Emails[index: 0].From, expression: Is.EqualTo(expected: "test@example.com"));
            Assert.That(actual: result.Emails[index: 0].Body, expression: Is.EqualTo(expected: "サニタイズされた本文"));

            _mockEmailRepository.Verify(
                expression: x => x.GetMessagesAsync(It.IsAny<EmailQuery>(), It.IsAny<CancellationToken>()),
                times: Times.Once
            );
            _mockDomainService.Verify(
                expression: x => x.SanitizeEmailContentAsync(It.IsAny<GmailMessage>()),
                times: Times.Once
            );
        }

        [Test]
        public async Task ExecuteAsync_クエリパラメータ_リポジトリに正しく渡される()
        {
            // Arrange
            var request = new GetEmailsRequestDto
            {
                Query            = "has:attachment",
                LabelIds         = new[] { "INBOX", "IMPORTANT" },
                MaxResults       = 25,
                PageToken        = "next_page_token",
                IncludeSpamTrash = true
            };

            _mockEmailRepository
                .Setup(expression: x => x.GetMessagesAsync(It.IsAny<EmailQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(value: new GmailMessage[0]);

            // Act
            await _useCase.ExecuteAsync(request: request);

            // Assert
            _mockEmailRepository.Verify(
                expression: x => x.GetMessagesAsync(
                    It.Is<EmailQuery>(q =>
                                          (q.Query == "has:attachment")                             &&
                                          q.LabelIds!.SequenceEqual(new[] { "INBOX", "IMPORTANT" }) &&
                                          (q.MaxResults       == 25)                                &&
                                          (q.PageToken        == "next_page_token")                 &&
                                          (q.IncludeSpamTrash == true)
                    ),
                    It.IsAny<CancellationToken>()
                ),
                times: Times.Once
            );
        }

        [Test]
        public async Task ExecuteAsync_複数のメール_すべてサニタイズされる()
        {
            // Arrange
            var request = new GetEmailsRequestDto { MaxResults = 2 };

            var email1 = new GmailMessage(
                id: "msg1",
                threadId: "thread1",
                subject: "メール1",
                from: "sender1@example.com",
                to: "recipient@example.com",
                cc: null,
                bcc: null,
                receivedDate: DateTime.Now,
                body: "本文1",
                isRead: false,
                isImportant: false,
                labels: new(),
                attachments: new()
            );

            var email2 = new GmailMessage(
                id: "msg2",
                threadId: "thread2",
                subject: "メール2",
                from: "sender2@example.com",
                to: "recipient@example.com",
                cc: null,
                bcc: null,
                receivedDate: DateTime.Now,
                body: "本文2",
                isRead: false,
                isImportant: false,
                labels: new(),
                attachments: new()
            );

            var sanitizedEmail1 = new GmailMessage(
                id: "msg1",
                threadId: "thread1",
                subject: "メール1",
                from: "sender1@example.com",
                to: "recipient@example.com",
                cc: null,
                bcc: null,
                receivedDate: DateTime.Now,
                body: "サニタイズ済み1",
                isRead: false,
                isImportant: false,
                labels: new(),
                attachments: new()
            );

            var sanitizedEmail2 = new GmailMessage(
                id: "msg2",
                threadId: "thread2",
                subject: "メール2",
                from: "sender2@example.com",
                to: "recipient@example.com",
                cc: null,
                bcc: null,
                receivedDate: DateTime.Now,
                body: "サニタイズ済み2",
                isRead: false,
                isImportant: false,
                labels: new(),
                attachments: new()
            );

            _mockEmailRepository
                .Setup(expression: x => x.GetMessagesAsync(It.IsAny<EmailQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(value: new[] { email1, email2 });

            _mockDomainService
                .Setup(expression: x => x.SanitizeEmailContentAsync(email1))
                .ReturnsAsync(value: sanitizedEmail1);
            _mockDomainService
                .Setup(expression: x => x.SanitizeEmailContentAsync(email2))
                .ReturnsAsync(value: sanitizedEmail2);

            // Act
            var result = await _useCase.ExecuteAsync(request: request);

            // Assert
            Assert.That(actual: result.Emails, expression: Has.Count.EqualTo(expected: 2));
            Assert.That(actual: result.Emails[index: 0].Id, expression: Is.EqualTo(expected: "msg1"));
            Assert.That(actual: result.Emails[index: 0].Body, expression: Is.EqualTo(expected: "サニタイズ済み1"));
            Assert.That(actual: result.Emails[index: 1].Id, expression: Is.EqualTo(expected: "msg2"));
            Assert.That(actual: result.Emails[index: 1].Body, expression: Is.EqualTo(expected: "サニタイズ済み2"));

            _mockDomainService.Verify(
                expression: x => x.SanitizeEmailContentAsync(It.IsAny<GmailMessage>()),
                times: Times.Exactly(callCount: 2)
            );
        }

        [Test]
        public async Task ExecuteAsync_ラベル付きメール_ラベルがDTOにマッピングされる()
        {
            // Arrange
            var request = new GetEmailsRequestDto();
            var label1 = new GmailLabel(
                id: "INBOX",
                name: "受信トレイ",
                type: GmailLabelType.System
            );

            var label2 = new GmailLabel(
                id: "project",
                name: "プロジェクト",
                type: GmailLabelType.User
            );

            var emailMessage = new GmailMessage(
                id: "msg123",
                threadId: "thread123",
                subject: "ラベル付きメール",
                from: "sender@example.com",
                to: "recipient@example.com",
                cc: null,
                bcc: null,
                receivedDate: DateTime.Now,
                body: "ラベル付きメール本文",
                isRead: false,
                isImportant: false,
                labels: new() { label1, label2 },
                attachments: new()
            );

            _mockEmailRepository
                .Setup(expression: x => x.GetMessagesAsync(It.IsAny<EmailQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(value: new[] { emailMessage });

            _mockDomainService
                .Setup(expression: x => x.SanitizeEmailContentAsync(It.IsAny<GmailMessage>()))
                .ReturnsAsync(value: emailMessage);

            // Act
            var result = await _useCase.ExecuteAsync(request: request);

            // Assert
            Assert.That(actual: result.Emails[index: 0].Labels, expression: Has.Count.EqualTo(expected: 2));
            Assert.That(actual: result.Emails[index: 0].Labels[index: 0].Id, expression: Is.EqualTo(expected: "INBOX"));
            Assert.That(
                actual: result.Emails[index: 0].Labels[index: 0].Name,
                expression: Is.EqualTo(expected: "受信トレイ")
            );
            Assert.That(
                actual: result.Emails[index: 0].Labels[index: 0].Type,
                expression: Is.EqualTo(expected: "System")
            );
            Assert.That(
                actual: result.Emails[index: 0].Labels[index: 1].Id,
                expression: Is.EqualTo(expected: "project")
            );
            Assert.That(
                actual: result.Emails[index: 0].Labels[index: 1].Name,
                expression: Is.EqualTo(expected: "プロジェクト")
            );
            Assert.That(
                actual: result.Emails[index: 0].Labels[index: 1].Type,
                expression: Is.EqualTo(expected: "User")
            );
        }

        [Test]
        public async Task ExecuteAsync_添付ファイル付きメール_添付ファイルがDTOにマッピングされる()
        {
            // Arrange
            var request = new GetEmailsRequestDto();
            var attachment1 = new EmailAttachment(
                id: "att1",
                fileName: "document.pdf",
                mimeType: "application/pdf",
                size: 1024,
                isInline: false,
                contentId: null
            );

            var attachment2 = new EmailAttachment(
                id: "att2",
                fileName: "image.jpg",
                mimeType: "image/jpeg",
                size: 2048,
                isInline: true,
                contentId: "img001"
            );

            var emailMessage = new GmailMessage(
                id: "msg123",
                threadId: "thread123",
                subject: "添付ファイル付きメール",
                from: "sender@example.com",
                to: "recipient@example.com",
                cc: null,
                bcc: null,
                receivedDate: DateTime.Now,
                body: "添付ファイル付きメール本文",
                isRead: false,
                isImportant: false,
                labels: new(),
                attachments: new() { attachment1, attachment2 }
            );

            _mockEmailRepository
                .Setup(expression: x => x.GetMessagesAsync(It.IsAny<EmailQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(value: new[] { emailMessage });

            _mockDomainService
                .Setup(expression: x => x.SanitizeEmailContentAsync(It.IsAny<GmailMessage>()))
                .ReturnsAsync(value: emailMessage);

            // Act
            var result = await _useCase.ExecuteAsync(request: request);

            // Assert
            Assert.That(actual: result.Emails[index: 0].Attachments, expression: Has.Count.EqualTo(expected: 2));
            Assert.That(
                actual: result.Emails[index: 0].Attachments[index: 0].Id,
                expression: Is.EqualTo(expected: "att1")
            );
            Assert.That(
                actual: result.Emails[index: 0].Attachments[index: 0].FileName,
                expression: Is.EqualTo(expected: "document.pdf")
            );
            Assert.That(
                actual: result.Emails[index: 0].Attachments[index: 0].MimeType,
                expression: Is.EqualTo(expected: "application/pdf")
            );
            Assert.That(
                actual: result.Emails[index: 0].Attachments[index: 0].Size,
                expression: Is.EqualTo(expected: 1024)
            );
            Assert.That(actual: result.Emails[index: 0].Attachments[index: 0].IsInline, expression: Is.False);
            Assert.That(actual: result.Emails[index: 0].Attachments[index: 0].ContentId, expression: Is.Null);

            Assert.That(
                actual: result.Emails[index: 0].Attachments[index: 1].Id,
                expression: Is.EqualTo(expected: "att2")
            );
            Assert.That(
                actual: result.Emails[index: 0].Attachments[index: 1].ContentId,
                expression: Is.EqualTo(expected: "img001")
            );
            Assert.That(actual: result.Emails[index: 0].Attachments[index: 1].IsInline, expression: Is.True);
        }

        [Test]
        public async Task ExecuteAsync_空の結果_空のリストが返される()
        {
            // Arrange
            var request = new GetEmailsRequestDto();

            _mockEmailRepository
                .Setup(expression: x => x.GetMessagesAsync(It.IsAny<EmailQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(value: new GmailMessage[0]);

            // Act
            var result = await _useCase.ExecuteAsync(request: request);

            // Assert
            Assert.That(actual: result, expression: Is.Not.Null);
            Assert.That(actual: result.Emails, expression: Is.Empty);
            Assert.That(actual: result.NextPageToken, expression: Is.Null);
        }
    }
}
