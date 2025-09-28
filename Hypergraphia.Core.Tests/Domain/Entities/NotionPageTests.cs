using Hypergraphia.Core.Domain.Entities;

namespace Hypergraphia.Core.Tests.Domain.Entities
{
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
            var notionPage = new NotionPage(title: "");

            // Assert
            Assert.That(actual: notionPage.Id, expression: Is.Null);
            Assert.That(actual: notionPage.Title, expression: Is.EqualTo(expected: string.Empty));
            Assert.That(actual: notionPage.DatabaseId, expression: Is.Null);
            Assert.That(actual: notionPage.ParentPageId, expression: Is.Null);
            Assert.That(actual: notionPage.Properties, expression: Is.Not.Null);
            Assert.That(actual: notionPage.Properties, expression: Is.Empty);
            Assert.That(actual: notionPage.Blocks, expression: Is.Not.Null);
            Assert.That(actual: notionPage.Blocks, expression: Is.Empty);
            Assert.That(actual: notionPage.CreatedTime, expression: Is.Null);
            Assert.That(actual: notionPage.LastEditedTime, expression: Is.Null);
        }

        [Test]
        public void プロパティ設定_すべての値_正常に設定される()
        {
            // Arrange
            var notionPage = new NotionPage(title: "");
            var createdTime = new DateTime(
                year: 2025,
                month: 1,
                day: 15,
                hour: 10,
                minute: 30,
                second: 0
            );
            var lastEditedTime = new DateTime(
                year: 2025,
                month: 1,
                day: 15,
                hour: 11,
                minute: 0,
                second: 0
            );
            var titleProperty = new NotionProperty(
                name: "title",
                type: NotionPropertyType.Title,
                value: "ページタイトル"
            );
            var textBlock = new NotionBlock(
                type: NotionBlockType.Paragraph,
                content: "テキストコンテンツ",
                id: "block1",
                hasChildren: false
            );

            // Act
            notionPage.Id                       = "page123";
            notionPage.Title                    = "テストページ";
            notionPage.DatabaseId               = "db456";
            notionPage.ParentPageId             = "parent789";
            notionPage.CreatedTime              = createdTime;
            notionPage.LastEditedTime           = lastEditedTime;
            notionPage.Properties[key: "title"] = titleProperty;
            notionPage.Blocks.Add(item: textBlock);

            // Assert
            Assert.That(actual: notionPage.Id, expression: Is.EqualTo(expected: "page123"));
            Assert.That(actual: notionPage.Title, expression: Is.EqualTo(expected: "テストページ"));
            Assert.That(actual: notionPage.DatabaseId, expression: Is.EqualTo(expected: "db456"));
            Assert.That(actual: notionPage.ParentPageId, expression: Is.EqualTo(expected: "parent789"));
            Assert.That(actual: notionPage.CreatedTime, expression: Is.EqualTo(expected: createdTime));
            Assert.That(actual: notionPage.LastEditedTime, expression: Is.EqualTo(expected: lastEditedTime));
            Assert.That(actual: notionPage.Properties, expression: Has.Count.EqualTo(expected: 1));
            Assert.That(actual: notionPage.Properties[key: "title"], expression: Is.EqualTo(expected: titleProperty));
            Assert.That(actual: notionPage.Blocks, expression: Has.Count.EqualTo(expected: 1));
            Assert.That(actual: notionPage.Blocks[index: 0], expression: Is.EqualTo(expected: textBlock));
        }

        [Test]
        public void Properties_複数のプロパティ追加_すべて正常に追加される()
        {
            // Arrange
            var notionPage = new NotionPage(title: "");
            var titleProperty = new NotionProperty(
                name: "title",
                type: NotionPropertyType.Title,
                value: "ページタイトル"
            );
            var dateProperty = new NotionProperty(
                name: "created_date",
                type: NotionPropertyType.Date,
                value: new DateTime(
                    year: 2025,
                    month: 1,
                    day: 15
                )
            );

            // Act
            notionPage.Properties[key: "title"]        = titleProperty;
            notionPage.Properties[key: "created_date"] = dateProperty;

            // Assert
            Assert.That(actual: notionPage.Properties, expression: Has.Count.EqualTo(expected: 2));
            Assert.That(actual: notionPage.Properties[key: "title"], expression: Is.EqualTo(expected: titleProperty));
            Assert.That(
                actual: notionPage.Properties[key: "created_date"],
                expression: Is.EqualTo(expected: dateProperty)
            );
        }

        [Test]
        public void Blocks_階層構造のブロック追加_正常に追加される()
        {
            // Arrange
            var notionPage = new NotionPage(title: "");
            var parentBlock = new NotionBlock(
                type: NotionBlockType.Toggle,
                content: "トグルブロック",
                id: "parent1",
                hasChildren: true
            );
            var childBlock = new NotionBlock(
                type: NotionBlockType.Paragraph,
                content: "子ブロック",
                id: "child1",
                hasChildren: false
            );

            // Act
            parentBlock.Children.Add(item: childBlock);
            notionPage.Blocks.Add(item: parentBlock);

            // Assert
            Assert.That(actual: notionPage.Blocks, expression: Has.Count.EqualTo(expected: 1));
            Assert.That(actual: notionPage.Blocks[index: 0], expression: Is.EqualTo(expected: parentBlock));
            Assert.That(actual: notionPage.Blocks[index: 0].HasChildren, expression: Is.True);
            Assert.That(actual: notionPage.Blocks[index: 0].Children, expression: Has.Count.EqualTo(expected: 1));
            Assert.That(
                actual: notionPage.Blocks[index: 0].Children[index: 0],
                expression: Is.EqualTo(expected: childBlock)
            );
        }

        [Test]
        public void Blocks_複数のブロック追加_順序が保持される()
        {
            // Arrange
            var notionPage = new NotionPage(title: "");
            var heading = new NotionBlock(
                type: NotionBlockType.Heading1,
                content: "見出し",
                id: "h1"
            );
            var paragraph = new NotionBlock(
                type: NotionBlockType.Paragraph,
                content: "段落",
                id: "p1"
            );
            var divider = new NotionBlock(
                type: NotionBlockType.Divider,
                content: null,
                id: "div1"
            );

            // Act
            notionPage.Blocks.Add(item: heading);
            notionPage.Blocks.Add(item: paragraph);
            notionPage.Blocks.Add(item: divider);

            // Assert
            Assert.That(actual: notionPage.Blocks, expression: Has.Count.EqualTo(expected: 3));
            Assert.That(actual: notionPage.Blocks[index: 0], expression: Is.EqualTo(expected: heading));
            Assert.That(actual: notionPage.Blocks[index: 1], expression: Is.EqualTo(expected: paragraph));
            Assert.That(actual: notionPage.Blocks[index: 2], expression: Is.EqualTo(expected: divider));
        }
    }
}
