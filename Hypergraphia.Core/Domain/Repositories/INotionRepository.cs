using Hypergraphia.Core.Domain.Entities;

namespace Hypergraphia.Core.Domain.Repositories;

/// <summary>
/// Notionページ操作を管理するリポジトリインターフェースです。
/// ページとデータベースのCRUD操作に対するNotion APIの抽象化を提供します。
/// </summary>
public interface INotionRepository
{
    /// <summary>
    /// 指定されたNotionデータベース内または他のページの子として新しいページを非同期で作成します。
    /// </summary>
    /// <param name="page">作成するコンテンツとプロパティを含むページエンティティ。</param>
    /// <param name="cancellationToken">非同期操作制御用のキャンセレーショントークン。</param>
    /// <returns>割り当てられたIDとタイムスタンプを持つ作成されたページ。</returns>
    Task<NotionPage> CreatePageAsync(NotionPage page, CancellationToken cancellationToken = default);

    /// <summary>
    /// 既存のNotionページを新しいコンテンツとプロパティで非同期で更新します。
    /// </summary>
    /// <param name="pageId">更新するページの一意識別子。</param>
    /// <param name="page">新しいコンテンツを含む更新されたページエンティティ。</param>
    /// <param name="cancellationToken">非同期操作制御用のキャンセレーショントークン。</param>
    /// <returns>更新されたページエンティティ。</returns>
    Task<NotionPage> UpdatePageAsync(string pageId, NotionPage page, CancellationToken cancellationToken = default);

    /// <summary>
    /// 一意識別子によって特定のページを非同期で取得します。
    /// </summary>
    /// <param name="pageId">取得するページの一意識別子。</param>
    /// <param name="cancellationToken">非同期操作制御用のキャンセレーショントークン。</param>
    /// <returns>見つかった場合はページエンティティ、それ以外はnull。</returns>
    Task<NotionPage?> GetPageByIdAsync(string pageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 提供されたクエリ条件に基づいてデータベース内のページを非同期で検索します。
    /// </summary>
    /// <param name="query">フィルタと並び替えオプションを含む検索条件。</param>
    /// <param name="cancellationToken">非同期操作制御用のキャンセレーショントークン。</param>
    /// <returns>検索条件に一致するページのコレクション。</returns>
    Task<IEnumerable<NotionPage>> QueryDatabaseAsync(NotionDatabaseQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// 既存のページに新しいコンテンツブロックを非同期で追加します。
    /// </summary>
    /// <param name="pageId">コンテンツを追加するページの一意識別子。</param>
    /// <param name="blocks">ページに追加するコンテンツブロック。</param>
    /// <param name="cancellationToken">非同期操作制御用のキャンセレーショントークン。</param>
    /// <returns>非同期操作の完了を表すタスク。</returns>
    Task AppendBlocksToPageAsync(string pageId, IEnumerable<NotionBlock> blocks, CancellationToken cancellationToken = default);
}

/// <summary>
/// フィルタリング、並び替え、ページネーションオプションを持つデータベースクエリを表します。
/// Notionデータベースからページを検索・取得するために使用されます。
/// </summary>
public class NotionDatabaseQuery
{
    /// <summary>
    /// クエリ対象のデータベースIDを取得または設定します。
    /// 検索するデータベースを指定するための必須フィールドです。
    /// </summary>
    public string DatabaseId { get; set; } = string.Empty;

    /// <summary>
    /// クエリのフィルタ条件を取得または設定します。
    /// プロパティ値と条件に基づくフィルタリングを可能にします。
    /// </summary>
    public object? Filter { get; set; }

    /// <summary>
    /// 結果の並び替え条件を取得または設定します。
    /// 返されるページの順序を指定します。
    /// </summary>
    public object? Sorts { get; set; }

    /// <summary>
    /// 返す結果の最大数を取得または設定します。
    /// Notion APIの制約により制限されます（通常最大100）。
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// 後続ページを取得するためのページネーションカーソルを取得または設定します。
    /// 結果がページサイズを超える場合のページネーション実装に使用されます。
    /// </summary>
    public string? StartCursor { get; set; }
}