using Hypergraphia.Core.Domain.ValueObjects;

namespace Hypergraphia.Core.Tests.Domain.ValueObjects
{
    /// <summary>
    /// EmailAddressクラスの単体テストです。
    /// 値オブジェクトとしての不変性と検証機能をテストします。
    /// </summary>
    [TestFixture]
    public class EmailAddressTests
    {
        [Test]
        public void コンストラクタ_有効なメールアドレス_正常に作成される()
        {
            // Arrange
            var address     = "test@example.com";
            var displayName = "テストユーザー";

            // Act
            var emailAddress = new EmailAddress(address: address, displayName: displayName);

            // Assert
            Assert.That(actual: emailAddress.Address, expression: Is.EqualTo(expected: address));
            Assert.That(actual: emailAddress.DisplayName, expression: Is.EqualTo(expected: displayName));
        }

        [Test]
        public void コンストラクタ_表示名なし_メールアドレスのみで作成される()
        {
            // Arrange
            var address = "user@domain.co.jp";

            // Act
            var emailAddress = new EmailAddress(address: address);

            // Assert
            Assert.That(actual: emailAddress.Address, expression: Is.EqualTo(expected: address));
            Assert.That(actual: emailAddress.DisplayName, expression: Is.Null);
        }

        [Test]
        public void コンストラクタ_前後に空白があるメールアドレス_空白が除去される()
        {
            // Arrange
            var addressWithSpaces     = "  test@example.com  ";
            var displayNameWithSpaces = "  テストユーザー  ";
            var expectedAddress       = "test@example.com";
            var expectedDisplayName   = "テストユーザー";

            // Act
            var emailAddress = new EmailAddress(address: addressWithSpaces, displayName: displayNameWithSpaces);

            // Assert
            Assert.That(actual: emailAddress.Address, expression: Is.EqualTo(expected: expectedAddress));
            Assert.That(actual: emailAddress.DisplayName, expression: Is.EqualTo(expected: expectedDisplayName));
        }

        [Test]
        public void コンストラクタ_空のメールアドレス_ArgumentExceptionがスローされる()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(code: () => new EmailAddress(address: ""));
        }

        [Test]
        public void コンストラクタ_nullのメールアドレス_ArgumentExceptionがスローされる()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(code: () => new EmailAddress(address: null!));
        }

        [Test]
        public void コンストラクタ_空白のみのメールアドレス_ArgumentExceptionがスローされる()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(code: () => new EmailAddress(address: "   "));
        }

        [Test]
        public void ToString_表示名あり_フォーマットされた文字列が返される()
        {
            // Arrange
            var emailAddress = new EmailAddress(address: "test@example.com", displayName: "山田太郎");
            var expected     = "山田太郎 <test@example.com>";

            // Act
            var result = emailAddress.ToString();

            // Assert
            Assert.That(actual: result, expression: Is.EqualTo(expected: expected));
        }

        [Test]
        public void ToString_表示名なし_メールアドレスのみが返される()
        {
            // Arrange
            var emailAddress = new EmailAddress(address: "test@example.com");
            var expected     = "test@example.com";

            // Act
            var result = emailAddress.ToString();

            // Assert
            Assert.That(actual: result, expression: Is.EqualTo(expected: expected));
        }

        [Test]
        public void ToString_表示名が空文字_メールアドレスのみが返される()
        {
            // Arrange
            var emailAddress = new EmailAddress(address: "test@example.com", displayName: "");
            var expected     = "test@example.com";

            // Act
            var result = emailAddress.ToString();

            // Assert
            Assert.That(actual: result, expression: Is.EqualTo(expected: expected));
        }

        [Test]
        public void Equals_同じ値のEmailAddress_等しいと判定される()
        {
            // Arrange
            var emailAddress1 = new EmailAddress(address: "test@example.com", displayName: "テストユーザー");
            var emailAddress2 = new EmailAddress(address: "test@example.com", displayName: "テストユーザー");

            // Act & Assert
            Assert.That(actual: emailAddress1, expression: Is.EqualTo(expected: emailAddress2));
            Assert.That(
                actual: emailAddress1.GetHashCode(),
                expression: Is.EqualTo(expected: emailAddress2.GetHashCode())
            );
        }

        [Test]
        public void Equals_異なるメールアドレス_等しくないと判定される()
        {
            // Arrange
            var emailAddress1 = new EmailAddress(address: "test1@example.com", displayName: "ユーザー1");
            var emailAddress2 = new EmailAddress(address: "test2@example.com", displayName: "ユーザー2");

            // Act & Assert
            Assert.That(actual: emailAddress1, expression: Is.Not.EqualTo(expected: emailAddress2));
        }

        [Test]
        public void Equals_異なる表示名_等しくないと判定される()
        {
            // Arrange
            var emailAddress1 = new EmailAddress(address: "test@example.com", displayName: "ユーザー1");
            var emailAddress2 = new EmailAddress(address: "test@example.com", displayName: "ユーザー2");

            // Act & Assert
            Assert.That(actual: emailAddress1, expression: Is.Not.EqualTo(expected: emailAddress2));
        }
    }
}
