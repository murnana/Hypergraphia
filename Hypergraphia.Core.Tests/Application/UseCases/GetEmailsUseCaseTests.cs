using Hypergraphia.Core.Application.DTOs;
using Hypergraphia.Core.Application.UseCases;
using Hypergraphia.Core.Domain.Entities;
using Hypergraphia.Core.Domain.Repositories;
using Hypergraphia.Core.Domain.Services;
using Moq;

namespace Hypergraphia.Core.Tests.Application.UseCases;

/// <summary>
/// GetEmailsUseCaseの単体テストです。
/// ユースケースのビジネスロジックをモックを使用してテストします。
/// </summary>
[TestFixture]
public class GetEmailsUseCaseTests
{
    private Mock<IEmailRepository> _mockEmailRepository = null!;
    private Mock<IEmailDomainService> _mockDomainService = null!;
    private GetEmailsUseCase _useCase = null!;

    [SetUp]
    public void Setup()
    {
        _mockEmailRepository = new Mock<IEmailRepository>();
        _mockDomainService = new Mock<IEmailDomainService>();
        _useCase = new GetEmailsUseCase(_mockEmailRepository.Object, _mockDomainService.Object);
    }

    [Test]
    public async Task ExecuteAsync_正常なリクエスト_メールが取得される()
    {
        // Arrange
        var request = new GetEmailsRequestDto
        {
            Query = "from:test@example.com",
            MaxResults = 10,
            IncludeSpamTrash = false
        };

        var emailMessage = new EmailMessage
        {
            Id = "msg123",
            ThreadId = "thread456",
            Subject = "テストメール",
            From = "test@example.com",
            To = "recipient@example.com",
            ReceivedDate = new DateTime(2025, 1, 15, 10, 0, 0),
            Body = "テスト本文",
            IsRead = false,
            IsImportant = false
        };

        var sanitizedEmail = new EmailMessage
        {
            Id = emailMessage.Id,
            ThreadId = emailMessage.ThreadId,
            Subject = emailMessage.Subject,
            From = emailMessage.From,
            To = emailMessage.To,
            ReceivedDate = emailMessage.ReceivedDate,
            Body = "サニタイズされた本文",
            PlainTextBody = "プレーンテキスト本文",
            IsRead = emailMessage.IsRead,
            IsImportant = emailMessage.IsImportant
        };

        _mockEmailRepository
            .Setup(x => x.GetMessagesAsync(It.IsAny<EmailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { emailMessage });

        _mockDomainService
            .Setup(x => x.SanitizeEmailContentAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(sanitizedEmail);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Emails, Has.Count.EqualTo(1));
        Assert.That(result.Emails[0].Id, Is.EqualTo("msg123"));
        Assert.That(result.Emails[0].Subject, Is.EqualTo("テストメール"));
        Assert.That(result.Emails[0].From, Is.EqualTo("test@example.com"));
        Assert.That(result.Emails[0].Body, Is.EqualTo("サニタイズされた本文"));
        Assert.That(result.Emails[0].PlainTextBody, Is.EqualTo("プレーンテキスト本文"));

        _mockEmailRepository.Verify(
            x => x.GetMessagesAsync(It.IsAny<EmailQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mockDomainService.Verify(
            x => x.SanitizeEmailContentAsync(It.IsAny<EmailMessage>()),
            Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_クエリパラメータ_リポジトリに正しく渡される()
    {
        // Arrange
        var request = new GetEmailsRequestDto
        {
            Query = "has:attachment",
            LabelIds = new[] { "INBOX", "IMPORTANT" },
            MaxResults = 25,
            PageToken = "next_page_token",
            IncludeSpamTrash = true
        };

        _mockEmailRepository
            .Setup(x => x.GetMessagesAsync(It.IsAny<EmailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailMessage[0]);

        // Act
        await _useCase.ExecuteAsync(request);

        // Assert
        _mockEmailRepository.Verify(
            x => x.GetMessagesAsync(
                It.Is<EmailQuery>(q =>
                    q.Query == "has:attachment" &&
                    q.LabelIds!.SequenceEqual(new[] { "INBOX", "IMPORTANT" }) &&
                    q.MaxResults == 25 &&
                    q.PageToken == "next_page_token" &&
                    q.IncludeSpamTrash == true),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_複数のメール_すべてサニタイズされる()
    {
        // Arrange
        var request = new GetEmailsRequestDto { MaxResults = 2 };

        var email1 = new EmailMessage { Id = "msg1", Subject = "メール1" };
        var email2 = new EmailMessage { Id = "msg2", Subject = "メール2" };

        var sanitizedEmail1 = new EmailMessage { Id = "msg1", Subject = "メール1", Body = "サニタイズ済み1" };
        var sanitizedEmail2 = new EmailMessage { Id = "msg2", Subject = "メール2", Body = "サニタイズ済み2" };

        _mockEmailRepository
            .Setup(x => x.GetMessagesAsync(It.IsAny<EmailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { email1, email2 });

        _mockDomainService
            .Setup(x => x.SanitizeEmailContentAsync(email1))
            .ReturnsAsync(sanitizedEmail1);
        _mockDomainService
            .Setup(x => x.SanitizeEmailContentAsync(email2))
            .ReturnsAsync(sanitizedEmail2);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.That(result.Emails, Has.Count.EqualTo(2));
        Assert.That(result.Emails[0].Id, Is.EqualTo("msg1"));
        Assert.That(result.Emails[0].Body, Is.EqualTo("サニタイズ済み1"));
        Assert.That(result.Emails[1].Id, Is.EqualTo("msg2"));
        Assert.That(result.Emails[1].Body, Is.EqualTo("サニタイズ済み2"));

        _mockDomainService.Verify(
            x => x.SanitizeEmailContentAsync(It.IsAny<EmailMessage>()),
            Times.Exactly(2));
    }

    [Test]
    public async Task ExecuteAsync_ラベル付きメール_ラベルがDTOにマッピングされる()
    {
        // Arrange
        var request = new GetEmailsRequestDto();
        var label1 = new EmailLabel { Id = "INBOX", Name = "受信トレイ", Type = EmailLabelType.System };
        var label2 = new EmailLabel { Id = "project", Name = "プロジェクト", Type = EmailLabelType.User };

        var emailMessage = new EmailMessage
        {
            Id = "msg123",
            Subject = "ラベル付きメール",
            Labels = new List<EmailLabel> { label1, label2 }
        };

        _mockEmailRepository
            .Setup(x => x.GetMessagesAsync(It.IsAny<EmailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { emailMessage });

        _mockDomainService
            .Setup(x => x.SanitizeEmailContentAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(emailMessage);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.That(result.Emails[0].Labels, Has.Count.EqualTo(2));
        Assert.That(result.Emails[0].Labels[0].Id, Is.EqualTo("INBOX"));
        Assert.That(result.Emails[0].Labels[0].Name, Is.EqualTo("受信トレイ"));
        Assert.That(result.Emails[0].Labels[0].Type, Is.EqualTo("System"));
        Assert.That(result.Emails[0].Labels[1].Id, Is.EqualTo("project"));
        Assert.That(result.Emails[0].Labels[1].Name, Is.EqualTo("プロジェクト"));
        Assert.That(result.Emails[0].Labels[1].Type, Is.EqualTo("User"));
    }

    [Test]
    public async Task ExecuteAsync_添付ファイル付きメール_添付ファイルがDTOにマッピングされる()
    {
        // Arrange
        var request = new GetEmailsRequestDto();
        var attachment1 = new EmailAttachment
        {
            Id = "att1",
            FileName = "document.pdf",
            MimeType = "application/pdf",
            Size = 1024,
            IsInline = false
        };
        var attachment2 = new EmailAttachment
        {
            Id = "att2",
            FileName = "image.jpg",
            MimeType = "image/jpeg",
            Size = 2048,
            IsInline = true,
            ContentId = "img001"
        };

        var emailMessage = new EmailMessage
        {
            Id = "msg123",
            Subject = "添付ファイル付きメール",
            Attachments = new List<EmailAttachment> { attachment1, attachment2 }
        };

        _mockEmailRepository
            .Setup(x => x.GetMessagesAsync(It.IsAny<EmailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { emailMessage });

        _mockDomainService
            .Setup(x => x.SanitizeEmailContentAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(emailMessage);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.That(result.Emails[0].Attachments, Has.Count.EqualTo(2));
        Assert.That(result.Emails[0].Attachments[0].Id, Is.EqualTo("att1"));
        Assert.That(result.Emails[0].Attachments[0].FileName, Is.EqualTo("document.pdf"));
        Assert.That(result.Emails[0].Attachments[0].MimeType, Is.EqualTo("application/pdf"));
        Assert.That(result.Emails[0].Attachments[0].Size, Is.EqualTo(1024));
        Assert.That(result.Emails[0].Attachments[0].IsInline, Is.False);
        Assert.That(result.Emails[0].Attachments[0].ContentId, Is.Null);

        Assert.That(result.Emails[0].Attachments[1].Id, Is.EqualTo("att2"));
        Assert.That(result.Emails[0].Attachments[1].ContentId, Is.EqualTo("img001"));
        Assert.That(result.Emails[0].Attachments[1].IsInline, Is.True);
    }

    [Test]
    public async Task ExecuteAsync_空の結果_空のリストが返される()
    {
        // Arrange
        var request = new GetEmailsRequestDto();

        _mockEmailRepository
            .Setup(x => x.GetMessagesAsync(It.IsAny<EmailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmailMessage[0]);

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Emails, Is.Empty);
        Assert.That(result.NextPageToken, Is.Null);
    }
}