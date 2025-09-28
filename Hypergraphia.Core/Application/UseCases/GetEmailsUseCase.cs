using Hypergraphia.Core.Application.DTOs;
using Hypergraphia.Core.Domain.Entities;
using Hypergraphia.Core.Domain.Repositories;
using Hypergraphia.Core.Domain.Services;

namespace Hypergraphia.Core.Application.UseCases
{
    /// <summary>
    /// メール取得のビジネスロジックを実行するユースケースクラスです。
    /// メールリポジトリとドメインサービスを統合し、メール取得と処理の一連の流れを管理します。
    /// </summary>
    public sealed class GetEmailsUseCase
    {
        private readonly IEmailDomainService _domainService;
        private readonly IEmailRepository    _emailRepository;

        /// <summary>
        /// GetEmailsUseCaseの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="emailRepository">メールデータアクセス用のリポジトリ。</param>
        /// <param name="domainService">メールドメインロジック用のサービス。</param>
        public GetEmailsUseCase(IEmailRepository emailRepository, IEmailDomainService domainService)
        {
            _emailRepository = emailRepository;
            _domainService   = domainService;
        }

        /// <summary>
        /// メール取得リクエストを非同期で実行し、サニタイズされたメールデータを返します。
        /// </summary>
        /// <param name="request">メール取得のリクエストパラメータ。</param>
        /// <param name="cancellationToken">非同期操作のキャンセル用トークン。</param>
        /// <returns>取得・処理されたメールの応答データ。</returns>
        public async Task<EmailsResponseDto> ExecuteAsync(
            GetEmailsRequestDto request,
            CancellationToken   cancellationToken = default
        )
        {
            var query = new EmailQuery
            {
                Query            = request.Query,
                LabelIds         = request.LabelIds,
                MaxResults       = request.MaxResults,
                PageToken        = request.PageToken,
                IncludeSpamTrash = request.IncludeSpamTrash
            };

            var emails = await _emailRepository.GetMessagesAsync(query: query, cancellationToken: cancellationToken);

            var sanitizedEmails = new List<GmailMessage>();
            foreach(var email in emails)
            {
                var sanitized = await _domainService.SanitizeEmailContentAsync(gmail: email);
                sanitizedEmails.Add(item: sanitized);
            }

            return new()
            {
                Emails        = sanitizedEmails.Select(selector: MapToDto).ToList(),
                NextPageToken = request.PageToken // TODO: 適切なページネーション実装
            };
        }

        /// <summary>
        /// EmailMessageエンティティをEmailDTOにマッピングします。
        /// </summary>
        /// <param name="gmail">マッピング元のメールエンティティ。</param>
        /// <returns>DTOにマッピングされたメールデータ。</returns>
        private static EmailDto MapToDto(GmailMessage gmail)
        {
            return new()
            {
                Id           = gmail.Id,
                ThreadId     = gmail.ThreadId,
                Subject      = gmail.Subject,
                From         = gmail.From,
                To           = gmail.To,
                Cc           = gmail.Cc,
                Bcc          = gmail.Bcc,
                ReceivedDate = gmail.ReceivedDate,
                Body         = gmail.Body,
                IsRead       = gmail.IsRead,
                IsImportant  = gmail.IsImportant,
                Labels = gmail.Labels.Select(
                                  selector: l => new EmailLabelDto
                                  {
                                      Id   = l.Id,
                                      Name = l.Name,
                                      Type = l.Type.ToString()
                                  }
                              )
                              .ToList(),
                Attachments = gmail.Attachments.Select(
                                       selector: a => new EmailAttachmentDto
                                       {
                                           Id        = a.Id,
                                           FileName  = a.FileName,
                                           MimeType  = a.MimeType,
                                           Size      = a.Size,
                                           IsInline  = a.IsInline,
                                           ContentId = a.ContentId
                                       }
                                   )
                                   .ToList()
            };
        }
    }
}
