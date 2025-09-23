using Hypergraphia.Core.Domain.ValueObjects;

namespace Hypergraphia.Core.Tests.Domain.ValueObjects;

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
        var address = "test@example.com";
        var displayName = "テストユーザー";

        // Act
        var emailAddress = new EmailAddress(address, displayName);

        // Assert
        Assert.That(emailAddress.Address, Is.EqualTo(address));
        Assert.That(emailAddress.DisplayName, Is.EqualTo(displayName));
    }

    [Test]
    public void コンストラクタ_表示名なし_メールアドレスのみで作成される()
    {
        // Arrange
        var address = "user@domain.co.jp";

        // Act
        var emailAddress = new EmailAddress(address);

        // Assert
        Assert.That(emailAddress.Address, Is.EqualTo(address));
        Assert.That(emailAddress.DisplayName, Is.Null);
    }

    [Test]
    public void コンストラクタ_前後に空白があるメールアドレス_空白が除去される()
    {
        // Arrange
        var addressWithSpaces = "  test@example.com  ";
        var displayNameWithSpaces = "  テストユーザー  ";
        var expectedAddress = "test@example.com";
        var expectedDisplayName = "テストユーザー";

        // Act
        var emailAddress = new EmailAddress(addressWithSpaces, displayNameWithSpaces);

        // Assert
        Assert.That(emailAddress.Address, Is.EqualTo(expectedAddress));
        Assert.That(emailAddress.DisplayName, Is.EqualTo(expectedDisplayName));
    }

    [Test]
    public void コンストラクタ_空のメールアドレス_ArgumentExceptionがスローされる()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new EmailAddress(""));
    }

    [Test]
    public void コンストラクタ_nullのメールアドレス_ArgumentExceptionがスローされる()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new EmailAddress(null!));
    }

    [Test]
    public void コンストラクタ_空白のみのメールアドレス_ArgumentExceptionがスローされる()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new EmailAddress("   "));
    }

    [Test]
    public void ToString_表示名あり_フォーマットされた文字列が返される()
    {
        // Arrange
        var emailAddress = new EmailAddress("test@example.com", "山田太郎");
        var expected = "山田太郎 <test@example.com>";

        // Act
        var result = emailAddress.ToString();

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ToString_表示名なし_メールアドレスのみが返される()
    {
        // Arrange
        var emailAddress = new EmailAddress("test@example.com");
        var expected = "test@example.com";

        // Act
        var result = emailAddress.ToString();

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ToString_表示名が空文字_メールアドレスのみが返される()
    {
        // Arrange
        var emailAddress = new EmailAddress("test@example.com", "");
        var expected = "test@example.com";

        // Act
        var result = emailAddress.ToString();

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Equals_同じ値のEmailAddress_等しいと判定される()
    {
        // Arrange
        var emailAddress1 = new EmailAddress("test@example.com", "テストユーザー");
        var emailAddress2 = new EmailAddress("test@example.com", "テストユーザー");

        // Act & Assert
        Assert.That(emailAddress1, Is.EqualTo(emailAddress2));
        Assert.That(emailAddress1.GetHashCode(), Is.EqualTo(emailAddress2.GetHashCode()));
    }

    [Test]
    public void Equals_異なるメールアドレス_等しくないと判定される()
    {
        // Arrange
        var emailAddress1 = new EmailAddress("test1@example.com", "ユーザー1");
        var emailAddress2 = new EmailAddress("test2@example.com", "ユーザー2");

        // Act & Assert
        Assert.That(emailAddress1, Is.Not.EqualTo(emailAddress2));
    }

    [Test]
    public void Equals_異なる表示名_等しくないと判定される()
    {
        // Arrange
        var emailAddress1 = new EmailAddress("test@example.com", "ユーザー1");
        var emailAddress2 = new EmailAddress("test@example.com", "ユーザー2");

        // Act & Assert
        Assert.That(emailAddress1, Is.Not.EqualTo(emailAddress2));
    }
}