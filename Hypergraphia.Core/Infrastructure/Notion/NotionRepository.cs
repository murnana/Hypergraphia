using System.Net;
using System.Text;
using System.Text.Json;
using Hypergraphia.Core.Domain.Entities;
using Hypergraphia.Core.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Hypergraphia.Core.Infrastructure.Notion
{
    /// <summary>
    /// Notion APIを使用してページ情報を操作するリポジトリの実装
    /// </summary>
    public sealed class NotionRepository : INotionRepository
    {
        private readonly string                    _apiToken;
        private readonly HttpClient                _httpClient;
        private readonly ILogger<NotionRepository> _logger;

        /// <summary>
        /// NotionRepositoryの新しいインスタンスを初期化します
        /// </summary>
        /// <param name="httpClient">HTTPクライアント</param>
        /// <param name="apiToken">Notion API トークン</param>
        /// <param name="logger">ロガー</param>
        public NotionRepository(
            HttpClient                httpClient,
            string                    apiToken,
            ILogger<NotionRepository> logger
        )
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(paramName: nameof(httpClient));
            _apiToken   = apiToken   ?? throw new ArgumentNullException(paramName: nameof(apiToken));
            _logger     = logger     ?? throw new ArgumentNullException(paramName: nameof(logger));

            ConfigureHttpClient();
        }

        /// <summary>
        /// 新しいページを作成します
        /// </summary>
        /// <param name="page">作成するページ</param>
        /// <param name="cancellationToken">キャンセレーショントークン</param>
        /// <returns>作成されたページ</returns>
        public async Task<NotionPage> CreatePageAsync(NotionPage page, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(message: "Notionページ作成を開始: タイトル={Title}", page.Title);

                var requestBody = CreatePageRequestBody(page: page);
                var content = new StringContent(
                    content: requestBody,
                    encoding: Encoding.UTF8,
                    mediaType: "application/json"
                );

                var response = await _httpClient.PostAsync(
                    requestUri: "v1/pages",
                    content: content,
                    cancellationToken: cancellationToken
                );
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken);
                var responseJson    = JsonDocument.Parse(json: responseContent);

                var pageId      = responseJson.RootElement.GetProperty(propertyName: "id").GetString();
                var createdPage = ConvertToNotionPage(json: responseContent);

                _logger.LogInformation(message: "Notionページ作成完了: PageId={PageId}", pageId);
                return createdPage;
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    exception: ex,
                    message: "Notionページ作成中にエラーが発生しました: タイトル={Title}",
                    page.Title
                );
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
                _logger.LogInformation(message: "Notionページ取得を開始: PageId={PageId}", pageId);

                var response = await _httpClient.GetAsync(
                    requestUri: $"v1/pages/{pageId}",
                    cancellationToken: cancellationToken
                );

                if(response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning(message: "Notionページが見つかりませんでした: PageId={PageId}", pageId);
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken);
                var page            = ConvertToNotionPage(json: responseContent);

                _logger.LogInformation(message: "Notionページ取得完了: PageId={PageId}", pageId);
                return page;
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    exception: ex,
                    message: "Notionページ取得中にエラーが発生しました: PageId={PageId}",
                    pageId
                );
                throw;
            }
        }

        /// <summary>
        /// 提供されたクエリ条件に基づいてデータベース内のページを非同期で検索します
        /// </summary>
        /// <param name="query">フィルタと並び替えオプションを含む検索条件</param>
        /// <param name="cancellationToken">非同期操作制御用のキャンセレーショントークン</param>
        /// <returns>検索条件に一致するページのコレクション</returns>
        public async Task<IEnumerable<NotionPage>> QueryDatabaseAsync(
            NotionDatabaseQuery query,
            CancellationToken   cancellationToken = default
        )
        {
            try
            {
                _logger.LogInformation(message: "Notionデータベースクエリを開始: DatabaseId={DatabaseId}", query.DatabaseId);

                var requestBody = CreateDatabaseQueryRequestBody(query: query);
                var content = new StringContent(
                    content: requestBody,
                    encoding: Encoding.UTF8,
                    mediaType: "application/json"
                );

                var response = await _httpClient.PostAsync(
                    requestUri: $"v1/databases/{query.DatabaseId}/query",
                    content: content,
                    cancellationToken: cancellationToken
                );
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken);
                var pages           = ConvertToNotionPages(json: responseContent);

                _logger.LogInformation(message: "Notionデータベースクエリ完了: {Count}件のページを取得", pages.Count());
                return pages;
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    exception: ex,
                    message: "Notionデータベースクエリ中にエラーが発生しました: DatabaseId={DatabaseId}",
                    query.DatabaseId
                );
                throw;
            }
        }

        /// <summary>
        /// 既存のNotionページを新しいコンテンツとプロパティで非同期で更新します
        /// </summary>
        /// <param name="pageId">更新するページの一意識別子</param>
        /// <param name="page">新しいコンテンツを含む更新されたページエンティティ</param>
        /// <param name="cancellationToken">非同期操作制御用のキャンセレーショントークン</param>
        /// <returns>更新されたページエンティティ</returns>
        public async Task<NotionPage> UpdatePageAsync(
            string            pageId,
            NotionPage        page,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                _logger.LogInformation(message: "Notionページ更新を開始: PageId={PageId}", pageId);

                var requestBody = CreatePageUpdateRequestBody(page: page);
                var content = new StringContent(
                    content: requestBody,
                    encoding: Encoding.UTF8,
                    mediaType: "application/json"
                );

                var response = await _httpClient.PatchAsync(
                    requestUri: $"v1/pages/{pageId}",
                    content: content,
                    cancellationToken: cancellationToken
                );
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken);
                var updatedPage     = ConvertToNotionPage(json: responseContent);

                _logger.LogInformation(message: "Notionページ更新完了: PageId={PageId}", pageId);
                return updatedPage;
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    exception: ex,
                    message: "Notionページ更新中にエラーが発生しました: PageId={PageId}",
                    pageId
                );
                throw;
            }
        }

        /// <summary>
        /// 既存のページに新しいコンテンツブロックを非同期で追加します
        /// </summary>
        /// <param name="pageId">コンテンツを追加するページの一意識別子</param>
        /// <param name="blocks">ページに追加するコンテンツブロック</param>
        /// <param name="cancellationToken">非同期操作制御用のキャンセレーショントークン</param>
        /// <returns>非同期操作の完了を表すタスク</returns>
        public async Task AppendBlocksToPageAsync(
            string                   pageId,
            IEnumerable<NotionBlock> blocks,
            CancellationToken        cancellationToken = default
        )
        {
            try
            {
                _logger.LogInformation(
                    message: "Notionページへのブロック追加を開始: PageId={PageId}, BlockCount={BlockCount}",
                    pageId,
                    blocks.Count()
                );

                var requestBody = CreateAppendBlocksRequestBody(blocks: blocks);
                var content = new StringContent(
                    content: requestBody,
                    encoding: Encoding.UTF8,
                    mediaType: "application/json"
                );

                var response = await _httpClient.PatchAsync(
                    requestUri: $"v1/blocks/{pageId}/children",
                    content: content,
                    cancellationToken: cancellationToken
                );
                response.EnsureSuccessStatusCode();

                _logger.LogInformation(message: "Notionページへのブロック追加完了: PageId={PageId}", pageId);
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    exception: ex,
                    message: "Notionページへのブロック追加中にエラーが発生しました: PageId={PageId}",
                    pageId
                );
                throw;
            }
        }

        /// <summary>
        /// HTTPクライアントの設定を行います
        /// </summary>
        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new(uriString: "https://api.notion.com/");
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(name: "Authorization", value: $"Bearer {_apiToken}");
            _httpClient.DefaultRequestHeaders.Add(name: "Notion-Version", value: "2022-06-28");
            _httpClient.DefaultRequestHeaders.Add(name: "Content-Type", value: "application/json");
        }

        /// <summary>
        /// 指定されたデータベースでページを検索します
        /// </summary>
        /// <param name="databaseId">データベースID</param>
        /// <param name="filter">検索フィルター（JSON形式）</param>
        /// <param name="cancellationToken">キャンセレーショントークン</param>
        /// <returns>検索されたページのコレクション</returns>
        public async Task<IEnumerable<NotionPage>> SearchPagesAsync(
            string            databaseId,
            string?           filter            = null,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                _logger.LogInformation(message: "Notionページ検索を開始: DatabaseId={DatabaseId}", databaseId);

                var requestBody = CreateSearchRequestBody(filter: filter);
                var content = new StringContent(
                    content: requestBody,
                    encoding: Encoding.UTF8,
                    mediaType: "application/json"
                );

                var response = await _httpClient.PostAsync(
                    requestUri: $"v1/databases/{databaseId}/query",
                    content: content,
                    cancellationToken: cancellationToken
                );
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken);
                var pages           = ConvertToNotionPages(json: responseContent);

                _logger.LogInformation(message: "Notionページ検索完了: {Count}件のページを取得", pages.Count());
                return pages;
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    exception: ex,
                    message: "Notionページ検索中にエラーが発生しました: DatabaseId={DatabaseId}",
                    databaseId
                );
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
                    type        = "database_id",
                    database_id = page.DatabaseId
                },
                properties = CreatePropertiesObject(properties: page.Properties),
                children   = page.Blocks.Select(selector: CreateBlockObject).ToArray()
            };

            return JsonSerializer.Serialize(
                value: requestObj,
                options: new()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                }
            );
        }

        /// <summary>
        /// プロパティオブジェクトを作成します
        /// </summary>
        /// <param name="properties">プロパティのコレクション</param>
        /// <returns>プロパティオブジェクト</returns>
        private object CreatePropertiesObject(Dictionary<string, NotionProperty> properties)
        {
            var propertiesDict = new Dictionary<string, object>();

            foreach(var kvp in properties)
            {
                propertiesDict[key: kvp.Key] = CreatePropertyValue(property: kvp.Value);
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
                NotionPropertyType.Title => new
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
                NotionPropertyType.RichText => new
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
                NotionPropertyType.Date => new
                {
                    date = DateTime.TryParse(s: property.Value?.ToString(), result: out var dateValue)
                        ? new { start = dateValue.ToString(format: "yyyy-MM-dd") }
                        : null
                },
                NotionPropertyType.Email => new
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
                NotionBlockType.Paragraph => new
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
                NotionBlockType.Heading1 => new
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
                NotionBlockType.Heading2 => new
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

            if(!string.IsNullOrEmpty(value: filter))
            {
                var json = JsonSerializer.Deserialize<object>(json: filter);
                requestObj[key: "filter"] = json ?? string.Empty;
            }

            return JsonSerializer.Serialize(value: requestObj);
        }

        /// <summary>
        /// データベースクエリ用のリクエストボディを作成します
        /// </summary>
        /// <param name="query">データベースクエリ</param>
        /// <returns>JSON形式のリクエストボディ</returns>
        private string CreateDatabaseQueryRequestBody(NotionDatabaseQuery query)
        {
            var requestObj = new Dictionary<string, object>();

            if(query.Filter != null)
            {
                requestObj[key: "filter"] = query.Filter;
            }

            if(query.Sorts != null)
            {
                requestObj[key: "sorts"] = query.Sorts;
            }

            if(query.PageSize > 0)
            {
                requestObj[key: "page_size"] = query.PageSize;
            }

            if(!string.IsNullOrEmpty(value: query.StartCursor))
            {
                requestObj[key: "start_cursor"] = query.StartCursor;
            }

            return JsonSerializer.Serialize(value: requestObj);
        }

        /// <summary>
        /// JSON文字列からNotionPageエンティティに変換します
        /// </summary>
        /// <param name="json">JSON文字列</param>
        /// <returns>変換されたNotionPageエンティティ</returns>
        private NotionPage ConvertToNotionPage(string json)
        {
            var jsonDoc = JsonDocument.Parse(json: json);
            var root    = jsonDoc.RootElement;

            var pageId     = root.GetProperty(propertyName: "id").GetString() ?? "";
            var databaseId = "";

            if(root.TryGetProperty(propertyName: "parent", value: out var parent) &&
               parent.TryGetProperty(propertyName: "database_id", value: out var dbId))
            {
                databaseId = dbId.GetString() ?? "";
            }

            var title      = ExtractTitle(root: root);
            var properties = ExtractProperties(root: root);
            var blocks     = new List<NotionBlock>(); // ブロックは別途取得が必要

            return new(
                title: title,
                id: pageId,
                databaseId: databaseId,
                parentPageId: null,
                properties: properties,
                blocks: blocks,
                createdTime: DateTime.UtcNow,
                lastEditedTime: DateTime.UtcNow
            );
        }

        /// <summary>
        /// JSON文字列からNotionPageエンティティのコレクションに変換します
        /// </summary>
        /// <param name="json">JSON文字列</param>
        /// <returns>変換されたNotionPageエンティティのコレクション</returns>
        private IEnumerable<NotionPage> ConvertToNotionPages(string json)
        {
            var jsonDoc = JsonDocument.Parse(json: json);
            var results = jsonDoc.RootElement.GetProperty(propertyName: "results");

            foreach(var result in results.EnumerateArray())
            {
                yield return ConvertToNotionPage(json: result.GetRawText());
            }
        }

        /// <summary>
        /// JSONからタイトルを抽出します
        /// </summary>
        /// <param name="root">JSONルート要素</param>
        /// <returns>抽出されたタイトル</returns>
        private string ExtractTitle(JsonElement root)
        {
            if(root.TryGetProperty(propertyName: "properties", value: out var properties))
            {
                foreach(var property in properties.EnumerateObject())
                {
                    if(property.Value.TryGetProperty(propertyName: "type", value: out var type)        &&
                       (type.GetString() == "title")                                                   &&
                       property.Value.TryGetProperty(propertyName: "title", value: out var titleArray) &&
                       (titleArray.GetArrayLength() > 0))
                    {
                        var firstTitle = titleArray[index: 0];
                        if(firstTitle.TryGetProperty(propertyName: "text", value: out var text) &&
                           text.TryGetProperty(propertyName: "content", value: out var content))
                        {
                            return content.GetString() ?? "";
                        }
                    }
                }
            }

            return"";
        }

        /// <summary>
        /// JSONからプロパティを抽出します
        /// </summary>
        /// <param name="root">JSONルート要素</param>
        /// <returns>抽出されたプロパティのコレクション</returns>
        private Dictionary<string, NotionProperty> ExtractProperties(JsonElement root)
        {
            var properties = new Dictionary<string, NotionProperty>();

            if(root.TryGetProperty(propertyName: "properties", value: out var propsElement))
            {
                foreach(var property in propsElement.EnumerateObject())
                {
                    var name  = property.Name;
                    var value = ExtractPropertyValue(propertyElement: property.Value);
                    var typeString = property.Value.TryGetProperty(propertyName: "type", value: out var typeElement)
                        ? typeElement.GetString() ?? "RichText"
                        : "RichText";

                    var type = Enum.TryParse<NotionPropertyType>(
                        value: typeString,
                        ignoreCase: true,
                        result: out var parsedType
                    )
                        ? parsedType
                        : NotionPropertyType.RichText;

                    properties[key: name] = new(
                        name: name,
                        type: type,
                        value: value
                    );
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
            if(propertyElement.TryGetProperty(propertyName: "type", value: out var type))
            {
                var typeName = type.GetString();

                return typeName switch
                {
                    "title" when propertyElement.TryGetProperty(propertyName: "title", value: out var titleArray)
                                 && (titleArray.GetArrayLength() > 0) =>
                        ExtractTextContent(textElement: titleArray[index: 0]),
                    "rich_text" when propertyElement.TryGetProperty(
                            propertyName: "rich_text",
                            value: out var richTextArray
                        ) && (richTextArray.GetArrayLength() > 0) =>
                        ExtractTextContent(textElement: richTextArray[index: 0]),
                    "email" when propertyElement.TryGetProperty(propertyName: "email", value: out var email) =>
                        email.GetString() ?? "",
                    "date" when propertyElement.TryGetProperty(propertyName: "date", value: out var date)
                                && date.TryGetProperty(propertyName: "start", value: out var start) =>
                        start.GetString() ?? "",
                    _ => ""
                };
            }

            return"";
        }

        /// <summary>
        /// テキストコンテンツを抽出します
        /// </summary>
        /// <param name="textElement">テキスト要素</param>
        /// <returns>抽出されたテキスト</returns>
        private string ExtractTextContent(JsonElement textElement)
        {
            if(textElement.TryGetProperty(propertyName: "text", value: out var text) &&
               text.TryGetProperty(propertyName: "content", value: out var content))
            {
                return content.GetString() ?? "";
            }

            return"";
        }

        /// <summary>
        /// ページ更新用のリクエストボディを作成します
        /// </summary>
        /// <param name="page">更新するページ</param>
        /// <returns>JSON形式のリクエストボディ</returns>
        private string CreatePageUpdateRequestBody(NotionPage page)
        {
            var requestObj = new
            {
                properties = CreatePropertiesObject(properties: page.Properties)
            };

            return JsonSerializer.Serialize(
                value: requestObj,
                options: new()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                }
            );
        }

        /// <summary>
        /// ブロック追加用のリクエストボディを作成します
        /// </summary>
        /// <param name="blocks">追加するブロックのコレクション</param>
        /// <returns>JSON形式のリクエストボディ</returns>
        private string CreateAppendBlocksRequestBody(IEnumerable<NotionBlock> blocks)
        {
            var requestObj = new
            {
                children = blocks.Select(selector: block => CreateBlockObject(block: block)).ToArray()
            };

            return JsonSerializer.Serialize(
                value: requestObj,
                options: new()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                }
            );
        }
    }
}
