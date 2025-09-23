using Hypergraphia.Core.Domain.Entities;
using Hypergraphia.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Hypergraphia.Core.Infrastructure.Notion;

/// <summary>
/// Notion APIを使用してページ情報を操作するリポジトリの実装
/// </summary>
public class NotionRepository : INotionRepository
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotionRepository> _logger;
    private readonly string _apiToken;

    /// <summary>
    /// NotionRepositoryの新しいインスタンスを初期化します
    /// </summary>
    /// <param name="httpClient">HTTPクライアント</param>
    /// <param name="apiToken">Notion API トークン</param>
    /// <param name="logger">ロガー</param>
    public NotionRepository(HttpClient httpClient, string apiToken, ILogger<NotionRepository> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiToken = apiToken ?? throw new ArgumentNullException(nameof(apiToken));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ConfigureHttpClient();
    }

    /// <summary>
    /// HTTPクライアントの設定を行います
    /// </summary>
    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri("https://api.notion.com/");
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiToken}");
        _httpClient.DefaultRequestHeaders.Add("Notion-Version", "2022-06-28");
        _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
    }

    /// <summary>
    /// 新しいページを作成します
    /// </summary>
    /// <param name="page">作成するページ</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>作成されたページのID</returns>
    public async Task<string> CreatePageAsync(NotionPage page, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Notionページ作成を開始: タイトル={Title}", page.Title);

            var requestBody = CreatePageRequestBody(page);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("v1/pages", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseJson = JsonDocument.Parse(responseContent);

            var pageId = responseJson.RootElement.GetProperty("id").GetString();

            _logger.LogInformation("Notionページ作成完了: PageId={PageId}", pageId);
            return pageId ?? throw new InvalidOperationException("ページIDが取得できませんでした");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Notionページ作成中にエラーが発生しました: タイトル={Title}", page.Title);
            throw;
        }
    }

    /// <summary>
    /// ページIDを指定してページを取得します
    /// </summary>
    /// <param name="pageId">ページID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>取得されたページ、見つからない場合はnull</returns>
    public async Task<NotionPage?> GetPageByIdAsync(string pageId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Notionページ取得を開始: PageId={PageId}", pageId);

            var response = await _httpClient.GetAsync($"v1/pages/{pageId}", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Notionページが見つかりませんでした: PageId={PageId}", pageId);
                return null;
            }

            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var page = ConvertToNotionPage(responseContent);

            _logger.LogInformation("Notionページ取得完了: PageId={PageId}", pageId);
            return page;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Notionページ取得中にエラーが発生しました: PageId={PageId}", pageId);
            throw;
        }
    }

    /// <summary>
    /// 指定されたデータベースでページを検索します
    /// </summary>
    /// <param name="databaseId">データベースID</param>
    /// <param name="filter">検索フィルター（JSON形式）</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>検索されたページのコレクション</returns>
    public async Task<IEnumerable<NotionPage>> SearchPagesAsync(
        string databaseId,
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Notionページ検索を開始: DatabaseId={DatabaseId}", databaseId);

            var requestBody = CreateSearchRequestBody(filter);
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"v1/databases/{databaseId}/query", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var pages = ConvertToNotionPages(responseContent);

            _logger.LogInformation("Notionページ検索完了: {Count}件のページを取得", pages.Count());
            return pages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Notionページ検索中にエラーが発生しました: DatabaseId={DatabaseId}", databaseId);
            throw;
        }
    }

    /// <summary>
    /// ページ作成用のリクエストボディを作成します
    /// </summary>
    /// <param name="page">作成するページ</param>
    /// <returns>JSON形式のリクエストボディ</returns>
    private string CreatePageRequestBody(NotionPage page)
    {
        var requestObj = new
        {
            parent = new
            {
                type = "database_id",
                database_id = page.DatabaseId
            },
            properties = CreatePropertiesObject(page.Properties),
            children = page.Blocks.Select(block => CreateBlockObject(block)).ToArray()
        };

        return JsonSerializer.Serialize(requestObj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });
    }

    /// <summary>
    /// プロパティオブジェクトを作成します
    /// </summary>
    /// <param name="properties">プロパティのコレクション</param>
    /// <returns>プロパティオブジェクト</returns>
    private object CreatePropertiesObject(IEnumerable<NotionProperty> properties)
    {
        var propertiesDict = new Dictionary<string, object>();

        foreach (var property in properties)
        {
            propertiesDict[property.Name] = CreatePropertyValue(property);
        }

        return propertiesDict;
    }

    /// <summary>
    /// プロパティ値オブジェクトを作成します
    /// </summary>
    /// <param name="property">プロパティ</param>
    /// <returns>プロパティ値オブジェクト</returns>
    private object CreatePropertyValue(NotionProperty property)
    {
        return property.Type switch
        {
            "title" => new
            {
                title = new[]
                {
                    new
                    {
                        text = new { content = property.Value },
                        type = "text"
                    }
                }
            },
            "rich_text" => new
            {
                rich_text = new[]
                {
                    new
                    {
                        text = new { content = property.Value },
                        type = "text"
                    }
                }
            },
            "date" => new
            {
                date = DateTime.TryParse(property.Value, out var dateValue)
                    ? new { start = dateValue.ToString("yyyy-MM-dd") }
                    : null
            },
            "email" => new
            {
                email = property.Value
            },
            _ => new { rich_text = new[] { new { text = new { content = property.Value }, type = "text" } } }
        };
    }

    /// <summary>
    /// ブロックオブジェクトを作成します
    /// </summary>
    /// <param name="block">ブロック</param>
    /// <returns>ブロックオブジェクト</returns>
    private object CreateBlockObject(NotionBlock block)
    {
        return block.Type switch
        {
            "paragraph" => new
            {
                type = "paragraph",
                paragraph = new
                {
                    rich_text = new[]
                    {
                        new
                        {
                            text = new { content = block.Content },
                            type = "text"
                        }
                    }
                }
            },
            "heading_1" => new
            {
                type = "heading_1",
                heading_1 = new
                {
                    rich_text = new[]
                    {
                        new
                        {
                            text = new { content = block.Content },
                            type = "text"
                        }
                    }
                }
            },
            "heading_2" => new
            {
                type = "heading_2",
                heading_2 = new
                {
                    rich_text = new[]
                    {
                        new
                        {
                            text = new { content = block.Content },
                            type = "text"
                        }
                    }
                }
            },
            _ => new
            {
                type = "paragraph",
                paragraph = new
                {
                    rich_text = new[]
                    {
                        new
                        {
                            text = new { content = block.Content },
                            type = "text"
                        }
                    }
                }
            }
        };
    }

    /// <summary>
    /// 検索用のリクエストボディを作成します
    /// </summary>
    /// <param name="filter">検索フィルター</param>
    /// <returns>JSON形式のリクエストボディ</returns>
    private string CreateSearchRequestBody(string? filter)
    {
        var requestObj = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(filter))
        {
            requestObj["filter"] = JsonSerializer.Deserialize<object>(filter);
        }

        return JsonSerializer.Serialize(requestObj);
    }

    /// <summary>
    /// JSON文字列からNotionPageエンティティに変換します
    /// </summary>
    /// <param name="json">JSON文字列</param>
    /// <returns>変換されたNotionPageエンティティ</returns>
    private NotionPage ConvertToNotionPage(string json)
    {
        var jsonDoc = JsonDocument.Parse(json);
        var root = jsonDoc.RootElement;

        var pageId = root.GetProperty("id").GetString() ?? "";
        var databaseId = "";

        if (root.TryGetProperty("parent", out var parent) &&
            parent.TryGetProperty("database_id", out var dbId))
        {
            databaseId = dbId.GetString() ?? "";
        }

        var title = ExtractTitle(root);
        var properties = ExtractProperties(root);
        var blocks = new List<NotionBlock>(); // ブロックは別途取得が必要

        return new NotionPage(
            pageId: pageId,
            databaseId: databaseId,
            title: title,
            properties: properties,
            blocks: blocks,
            createdAt: DateTime.UtcNow,
            updatedAt: DateTime.UtcNow
        );
    }

    /// <summary>
    /// JSON文字列からNotionPageエンティティのコレクションに変換します
    /// </summary>
    /// <param name="json">JSON文字列</param>
    /// <returns>変換されたNotionPageエンティティのコレクション</returns>
    private IEnumerable<NotionPage> ConvertToNotionPages(string json)
    {
        var jsonDoc = JsonDocument.Parse(json);
        var results = jsonDoc.RootElement.GetProperty("results");

        foreach (var result in results.EnumerateArray())
        {
            yield return ConvertToNotionPage(result.GetRawText());
        }
    }

    /// <summary>
    /// JSONからタイトルを抽出します
    /// </summary>
    /// <param name="root">JSONルート要素</param>
    /// <returns>抽出されたタイトル</returns>
    private string ExtractTitle(JsonElement root)
    {
        if (root.TryGetProperty("properties", out var properties))
        {
            foreach (var property in properties.EnumerateObject())
            {
                if (property.Value.TryGetProperty("type", out var type) &&
                    type.GetString() == "title" &&
                    property.Value.TryGetProperty("title", out var titleArray) &&
                    titleArray.GetArrayLength() > 0)
                {
                    var firstTitle = titleArray[0];
                    if (firstTitle.TryGetProperty("text", out var text) &&
                        text.TryGetProperty("content", out var content))
                    {
                        return content.GetString() ?? "";
                    }
                }
            }
        }

        return "";
    }

    /// <summary>
    /// JSONからプロパティを抽出します
    /// </summary>
    /// <param name="root">JSONルート要素</param>
    /// <returns>抽出されたプロパティのコレクション</returns>
    private List<NotionProperty> ExtractProperties(JsonElement root)
    {
        var properties = new List<NotionProperty>();

        if (root.TryGetProperty("properties", out var propsElement))
        {
            foreach (var property in propsElement.EnumerateObject())
            {
                var name = property.Name;
                var value = ExtractPropertyValue(property.Value);
                var type = property.Value.TryGetProperty("type", out var typeElement)
                    ? typeElement.GetString() ?? "rich_text"
                    : "rich_text";

                properties.Add(new NotionProperty(name, value, type));
            }
        }

        return properties;
    }

    /// <summary>
    /// プロパティの値を抽出します
    /// </summary>
    /// <param name="propertyElement">プロパティのJSON要素</param>
    /// <returns>抽出された値</returns>
    private string ExtractPropertyValue(JsonElement propertyElement)
    {
        if (propertyElement.TryGetProperty("type", out var type))
        {
            var typeName = type.GetString();

            return typeName switch
            {
                "title" when propertyElement.TryGetProperty("title", out var titleArray) && titleArray.GetArrayLength() > 0 =>
                    ExtractTextContent(titleArray[0]),
                "rich_text" when propertyElement.TryGetProperty("rich_text", out var richTextArray) && richTextArray.GetArrayLength() > 0 =>
                    ExtractTextContent(richTextArray[0]),
                "email" when propertyElement.TryGetProperty("email", out var email) =>
                    email.GetString() ?? "",
                "date" when propertyElement.TryGetProperty("date", out var date) && date.TryGetProperty("start", out var start) =>
                    start.GetString() ?? "",
                _ => ""
            };
        }

        return "";
    }

    /// <summary>
    /// テキストコンテンツを抽出します
    /// </summary>
    /// <param name="textElement">テキスト要素</param>
    /// <returns>抽出されたテキスト</returns>
    private string ExtractTextContent(JsonElement textElement)
    {
        if (textElement.TryGetProperty("text", out var text) &&
            text.TryGetProperty("content", out var content))
        {
            return content.GetString() ?? "";
        }

        return "";
    }
}