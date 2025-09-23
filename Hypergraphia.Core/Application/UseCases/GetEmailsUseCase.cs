using Hypergraphia.Core.Application.DTOs;
using Hypergraphia.Core.Domain.Entities;
using Hypergraphia.Core.Domain.Repositories;
using Hypergraphia.Core.Domain.Services;

namespace Hypergraphia.Core.Application.UseCases;

/// <summary>
/// メール取得のビジネスロジックを実行するユースケースクラスです。
/// メールリポジトリとドメインサービスを統合し、メール取得と処理の一連の流れを管理します。
/// </summary>
public class GetEmailsUseCase
{
    private readonly IEmailRepository _emailRepository;
    private readonly IEmailDomainService _domainService;

    /// <summary>
    /// GetEmailsUseCaseの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="emailRepository">メールデータアクセス用のリポジトリ。</param>
    /// <param name="domainService">メールドメインロジック用のサービス。</param>
    public GetEmailsUseCase(IEmailRepository emailRepository, IEmailDomainService domainService)
    {
        _emailRepository = emailRepository;
        _domainService = domainService;
    }

    /// <summary>
    /// メール取得リクエストを非同期で実行し、サニタイズされたメールデータを返します。
    /// </summary>
    /// <param name="request">メール取得のリクエストパラメータ。</param>
    /// <param name="cancellationToken">非同期操作のキャンセル用トークン。</param>
    /// <returns>取得・処理されたメールの応答データ。</returns>
    public async Task<EmailsResponseDto> ExecuteAsync(GetEmailsRequestDto request, CancellationToken cancellationToken = default)
    {
        var query = new EmailQuery
        {
            Query = request.Query,
            LabelIds = request.LabelIds,
            MaxResults = request.MaxResults,
            PageToken = request.PageToken,
            IncludeSpamTrash = request.IncludeSpamTrash
        };

        var emails = await _emailRepository.GetMessagesAsync(query, cancellationToken);

        var sanitizedEmails = new List<EmailMessage>();
        foreach (var email in emails)
        {
            var sanitized = await _domainService.SanitizeEmailContentAsync(email);
            sanitizedEmails.Add(sanitized);
        }

        return new EmailsResponseDto
        {
            Emails = sanitizedEmails.Select(MapToDto).ToList(),
            NextPageToken = request.PageToken // TODO: 適切なページネーション実装
        };
    }

    /// <summary>
    /// EmailMessageエンティティをEmailDTOにマッピングします。
    /// </summary>
    /// <param name="email">マッピング元のメールエンティティ。</param>
    /// <returns>DTOにマッピングされたメールデータ。</returns>
    private static EmailDto MapToDto(EmailMessage email)
    {
        return new EmailDto
        {
            Id = email.Id,
            ThreadId = email.ThreadId,
            Subject = email.Subject,
            From = email.From,
            To = email.To,
            Cc = email.Cc,
            Bcc = email.Bcc,
            ReceivedDate = email.ReceivedDate,
            Body = email.Body,
            PlainTextBody = email.PlainTextBody,
            IsRead = email.IsRead,
            IsImportant = email.IsImportant,
            Labels = email.Labels.Select(l => new EmailLabelDto
            {
                Id = l.Id,
                Name = l.Name,
                Type = l.Type.ToString()
            }).ToList(),
            Attachments = email.Attachments.Select(a => new EmailAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                MimeType = a.MimeType,
                Size = a.Size,
                IsInline = a.IsInline,
                ContentId = a.ContentId
            }).ToList()
        };
    }
}