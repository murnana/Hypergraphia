using Hypergraphia.Core.Domain.Entities;

namespace Hypergraphia.Core.Tests.Domain.Entities;

/// <summary>
/// NotionPageエンティティの単体テストです。
/// エンティティの初期化とプロパティ設定をテストします。
/// </summary>
[TestFixture]
public class NotionPageTests
{
    [Test]
    public void コンストラクタ_デフォルト値_すべてのプロパティが初期化される()
    {
        // Act
        var notionPage = new NotionPage();

        // Assert
        Assert.That(notionPage.Id, Is.Null);
        Assert.That(notionPage.Title, Is.EqualTo(string.Empty));
        Assert.That(notionPage.DatabaseId, Is.Null);
        Assert.That(notionPage.ParentPageId, Is.Null);
        Assert.That(notionPage.Properties, Is.Not.Null);
        Assert.That(notionPage.Properties, Is.Empty);
        Assert.That(notionPage.Blocks, Is.Not.Null);
        Assert.That(notionPage.Blocks, Is.Empty);
        Assert.That(notionPage.CreatedTime, Is.Null);
        Assert.That(notionPage.LastEditedTime, Is.Null);
    }

    [Test]
    public void プロパティ設定_すべての値_正常に設定される()
    {
        // Arrange
        var notionPage = new NotionPage();
        var createdTime = new DateTime(2025, 1, 15, 10, 30, 0);
        var lastEditedTime = new DateTime(2025, 1, 15, 11, 0, 0);
        var titleProperty = new NotionProperty
        {
            Type = NotionPropertyType.Title,
            Value = "ページタイトル"
        };
        var textBlock = new NotionBlock
        {
            Id = "block1",
            Type = NotionBlockType.Paragraph,
            Content = "テキストコンテンツ",
            HasChildren = false
        };

        // Act
        notionPage.Id = "page123";
        notionPage.Title = "テストページ";
        notionPage.DatabaseId = "db456";
        notionPage.ParentPageId = "parent789";
        notionPage.CreatedTime = createdTime;
        notionPage.LastEditedTime = lastEditedTime;
        notionPage.Properties["title"] = titleProperty;
        notionPage.Blocks.Add(textBlock);

        // Assert
        Assert.That(notionPage.Id, Is.EqualTo("page123"));
        Assert.That(notionPage.Title, Is.EqualTo("テストページ"));
        Assert.That(notionPage.DatabaseId, Is.EqualTo("db456"));
        Assert.That(notionPage.ParentPageId, Is.EqualTo("parent789"));
        Assert.That(notionPage.CreatedTime, Is.EqualTo(createdTime));
        Assert.That(notionPage.LastEditedTime, Is.EqualTo(lastEditedTime));
        Assert.That(notionPage.Properties, Has.Count.EqualTo(1));
        Assert.That(notionPage.Properties["title"], Is.EqualTo(titleProperty));
        Assert.That(notionPage.Blocks, Has.Count.EqualTo(1));
        Assert.That(notionPage.Blocks[0], Is.EqualTo(textBlock));
    }

    [Test]
    public void Properties_複数のプロパティ追加_すべて正常に追加される()
    {
        // Arrange
        var notionPage = new NotionPage();
        var titleProperty = new NotionProperty
        {
            Type = NotionPropertyType.Title,
            Value = "ページタイトル"
        };
        var dateProperty = new NotionProperty
        {
            Type = NotionPropertyType.Date,
            Value = new DateTime(2025, 1, 15)
        };

        // Act
        notionPage.Properties["title"] = titleProperty;
        notionPage.Properties["created_date"] = dateProperty;

        // Assert
        Assert.That(notionPage.Properties, Has.Count.EqualTo(2));
        Assert.That(notionPage.Properties["title"], Is.EqualTo(titleProperty));
        Assert.That(notionPage.Properties["created_date"], Is.EqualTo(dateProperty));
    }

    [Test]
    public void Blocks_階層構造のブロック追加_正常に追加される()
    {
        // Arrange
        var notionPage = new NotionPage();
        var parentBlock = new NotionBlock
        {
            Id = "parent1",
            Type = NotionBlockType.Toggle,
            Content = "トグルブロック",
            HasChildren = true
        };
        var childBlock = new NotionBlock
        {
            Id = "child1",
            Type = NotionBlockType.Paragraph,
            Content = "子ブロック",
            HasChildren = false
        };

        // Act
        parentBlock.Children.Add(childBlock);
        notionPage.Blocks.Add(parentBlock);

        // Assert
        Assert.That(notionPage.Blocks, Has.Count.EqualTo(1));
        Assert.That(notionPage.Blocks[0], Is.EqualTo(parentBlock));
        Assert.That(notionPage.Blocks[0].HasChildren, Is.True);
        Assert.That(notionPage.Blocks[0].Children, Has.Count.EqualTo(1));
        Assert.That(notionPage.Blocks[0].Children[0], Is.EqualTo(childBlock));
    }

    [Test]
    public void Blocks_複数のブロック追加_順序が保持される()
    {
        // Arrange
        var notionPage = new NotionPage();
        var heading = new NotionBlock
        {
            Id = "h1",
            Type = NotionBlockType.Heading1,
            Content = "見出し"
        };
        var paragraph = new NotionBlock
        {
            Id = "p1",
            Type = NotionBlockType.Paragraph,
            Content = "段落"
        };
        var divider = new NotionBlock
        {
            Id = "div1",
            Type = NotionBlockType.Divider,
            Content = null
        };

        // Act
        notionPage.Blocks.Add(heading);
        notionPage.Blocks.Add(paragraph);
        notionPage.Blocks.Add(divider);

        // Assert
        Assert.That(notionPage.Blocks, Has.Count.EqualTo(3));
        Assert.That(notionPage.Blocks[0], Is.EqualTo(heading));
        Assert.That(notionPage.Blocks[1], Is.EqualTo(paragraph));
        Assert.That(notionPage.Blocks[2], Is.EqualTo(divider));
    }
}