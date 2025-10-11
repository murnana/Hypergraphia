# Console アプリケーション開発セットアップガイド

Hypergraphia.Console プロジェクトの開発環境セットアップ手順です。

## 📋 目次

1. [プロジェクト概要](#プロジェクト概要)
2. [事前準備](#事前準備)
3. [依存関係の設定](#依存関係の設定)
4. [設定ファイルの準備](#設定ファイルの準備)
5. [Gmail API 認証設定](#gmail-api-認証設定)
6. [Notion API 設定](#notion-api-設定)
7. [開発環境でのビルド・実行](#開発環境でのビルド実行)
8. [トラブルシューティング](#トラブルシューティング)

## プロジェクト概要

Hypergraphia.Console は Gmail から Notion へのデータインポート機能を提供するコマンドライン アプリケーションです。

### 技術仕様
- **フレームワーク**: .NET 8.0
- **AOT コンパイル**: Release ビルドで有効
- **コマンドライン フレームワーク**: System.CommandLine
- **ホスティング**: Microsoft.Extensions.Hosting
- **設定管理**: Microsoft.Extensions.Configuration

### 主要機能
- Gmail から未読メールの取得・インポート
- Notion データベースへのメール情報保存
- 構造化ログ出力（コンソール・ファイル）

## 事前準備

### 必要なツール
```bash
# .NET 8.0 SDK がインストールされていることを確認
dotnet --version
# 8.0.x が表示されることを確認
```

### リポジトリのクローン
```bash
git clone <repository-url>
cd Hypergraphia
```

## 依存関係の設定

### NuGet パッケージの復元
```bash
# ソリューション全体の依存関係を復元
dotnet restore

# MAUI ワークロードも復元（必要に応じて）
dotnet workload restore
```

### 主要な依存関係
- `Microsoft.DotNet.ILCompiler` (9.0.9) - AOT コンパイル
- `Microsoft.Extensions.Hosting` (8.0.0) - ホスティング インフラ
- `System.CommandLine` (2.0.0-beta4) - コマンドライン パーサー
- `Hypergraphia.Core` - ビジネスロジック (プロジェクト参照)

## 設定ファイルの準備

### 1. appsettings.json の確認

`Hypergraphia.Console/appsettings.json` には公開可能な設定のみが含まれています：

```json
{
  \"Hypergraphia\": {
    \"Gmail\": {
      \"CredentialsPath\": \"gmail-credentials.json\",
      \"Scopes\": [
        \"https://www.googleapis.com/auth/gmail.readonly\"
      ],
      \"ApplicationName\": \"Hypergraphia\",
      \"MaxResults\": 100
    },
    \"Notion\": {
      \"ApiToken\": \"\",
      \"DefaultDatabaseId\": \"\",
      \"ApiVersion\": \"2022-06-28\",
      \"TimeoutSeconds\": 30
    },
    \"Logging\": {
      \"LogLevel\": \"Information\",
      \"LogFilePath\": \"logs/hypergraphia.log\",
      \"EnableConsoleLogging\": true,
      \"EnableFileLogging\": false
    }
  }
}
```

⚠️ **重要**: Notion の `ApiToken` と `DefaultDatabaseId` は空文字列のままにしてください。実際の値は `appsettings.notion.json` に設定します。

### 2. 開発環境設定

`appsettings.Development.json` は既に設定済み（デバッグ ログ有効）。

## Gmail API 認証設定

Gmail API を使用するには、Google Cloud Platform で OAuth 2.0 デスクトップアプリの認証情報を作成する必要があります。

詳細な手順については、**[Gmail API セットアップガイド](../gmail-api-setup.md)** を参照してください。

### クイック セットアップ手順

1. **OAuth 2.0 クライアント ID の作成**
   - [Gmail API セットアップガイド](../gmail-api-setup.md)の手順に従って認証情報を作成
   - **重要**: アプリケーションの種類は「**デスクトップ アプリ**」を選択

2. **認証情報ファイルの配置**

   ダウンロードした JSON ファイルを以下の場所に配置：
   ```bash
   cd Hypergraphia.Console
   # ダウンロードした認証情報ファイルを gmail-credentials.json にリネーム
   ```

   配置先: `Hypergraphia.Console/gmail-credentials.json`

3. **初回認証フロー**

   アプリケーションを初めて実行すると、ブラウザが開いて Google アカウントでの認証が求められます：

   ```bash
   dotnet run --project Hypergraphia.Console
   ```

   - ブラウザで Gmail へのアクセスを許可
   - 認証成功後、アクセストークンは `gmail-token.json/` ディレクトリに自動保存されます
   - 以降の実行では、保存されたトークンが再利用され、ブラウザは開きません

4. **必要な権限スコープ**

   このアプリケーションは以下のスコープを使用します：
   ```
   https://www.googleapis.com/auth/gmail.readonly
   ```

   読み取り専用のため、メールの変更や削除はできません。

⚠️ **セキュリティ重要**:
- `gmail-credentials.json` - OAuth 2.0 クライアント ID 認証情報（`.gitignore` に含まれています）
- `gmail-token.json/` - Gmail OAuth 認証トークン（初回認証後に自動生成、`.gitignore` に含まれています）
- これらのファイルを Git にコミットしないでください
- ファイルには機密情報が含まれているため、他人と共有しないでください

### appsettings.json の確認

認証情報ファイルのパスが正しく設定されていることを確認：

```json
{
  "Hypergraphia": {
    "Gmail": {
      "CredentialsPath": "gmail-credentials.json",
      "Scopes": [
        "https://www.googleapis.com/auth/gmail.readonly"
      ]
    }
  }
}
```

## Notion API 設定

### 1. Notion 統合の作成

1. [Notion Developers](https://developers.notion.com/) にアクセス
2. 新しい統合を作成
3. API トークンを取得

### 2. データベースの準備

1. Notion でデータベースを作成
2. 統合にデータベースへのアクセス権限を付与
3. データベース ID を取得（URL から）

### 3. 認証情報ファイルの作成

`appsettings.notion.json.sample` をコピーして `appsettings.notion.json` を作成：

```bash
cd Hypergraphia.Console
cp appsettings.notion.json.sample appsettings.notion.json
```

`appsettings.notion.json` を編集して、実際の認証情報を設定：

```json
{
  "Hypergraphia": {
    "Notion": {
      "ApiToken": "secret_YOUR_ACTUAL_NOTION_TOKEN",
      "DefaultDatabaseId": "YOUR_ACTUAL_DATABASE_ID"
    }
  }
}
```

⚠️ **セキュリティ重要**:
- `appsettings.notion.json` は `.gitignore` に含まれています
- このファイルを Git にコミットしないでください
- ファイルには機密情報が含まれているため、他人と共有しないでください

## 開発環境でのビルド・実行

### プロジェクトのビルド

```bash
# Console プロジェクトのビルド
dotnet build Hypergraphia.Console

# または、ソリューション全体のビルド
dotnet build
```

### アプリケーションの実行

```bash
# Console アプリケーションの実行
dotnet run --project Hypergraphia.Console

# ヘルプの表示
dotnet run --project Hypergraphia.Console -- --help

# Gmail インポート コマンドの実行例
dotnet run --project Hypergraphia.Console -- import gmail --query \"is:unread\" --max-results 5
```

### 利用可能なコマンド

```bash
# 基本コマンド構造
hypergraphia import gmail [オプション]

# オプション:
--query          Gmail 検索クエリ（デフォルト: \"is:unread\"）
--max-results    取得する最大メール数（デフォルト: 10）
--database-id    Notion データベース ID（設定ファイルの値を上書き）
```

### Release ビルド（AOT）

```bash
# AOT コンパイルでの Release ビルド
dotnet publish Hypergraphia.Console -c Release

# 生成された実行ファイル（例: Windows）
./Hypergraphia.Console/bin/Release/net8.0/win-x64/publish/Hypergraphia.Console.exe
```

## トラブルシューティング

### よくある問題

#### 1. Gmail API 認証エラー

**症状**: "Gmail認証ファイルが見つかりません" エラー
**解決策**:
- `gmail-credentials.json` ファイルが `Hypergraphia.Console/` ディレクトリに存在することを確認
- ビルド時にファイルが出力ディレクトリにコピーされているか確認
- ファイルパスが `appsettings.json` の設定と一致することを確認

**症状**: "Unrecognized credential type" エラー
**解決策**:
- OAuth 2.0 クライアント ID（**デスクトップアプリ**）の認証情報を使用していることを確認
- JSON ファイルに `installed` キーが含まれているか確認
- サービスアカウント形式ではなく、デスクトップアプリ形式であることを確認

**症状**: ブラウザが開かない
**解決策**:
- デフォルトブラウザが正しく設定されているか確認
- ファイアウォールがブラウザの起動をブロックしていないか確認
- コンソールに表示される認証 URL を手動でブラウザで開く

**症状**: トークンの有効期限切れ
**解決策**:
- `gmail-token.json/` ディレクトリを削除
- アプリケーションを再実行して再認証

#### 2. Notion API エラー
**症状**: \"Unauthorized\" エラー
**解決策**:
- API トークンが正しいことを確認
- データベース ID が正しいことを確認
- 統合にデータベースへのアクセス権限があることを確認

#### 3. ビルド エラー
**症状**: 依存関係エラー
**解決策**:
```bash
# 依存関係のクリーンアップと復元
dotnet clean
dotnet restore
dotnet build
```

#### 4. AOT コンパイル エラー
**症状**: Publish 時のエラー
**解決策**:
- .NET 8.0 SDK の最新版を使用
- `InvariantGlobalization` 設定を確認

### ログの確認

開発環境では詳細なログが出力されます：

```bash
# ログレベルを Debug に設定（appsettings.Development.json）
# コンソールとファイル両方にログ出力

# ログファイルの場所
logs/hypergraphia.log
```

### 設定の検証

設定が正しく読み込まれているかを確認：

```bash
# アプリケーション起動時にログで確認
dotnet run --project Hypergraphia.Console -- import gmail --query \"test\" --max-results 1
```

---

## 関連ドキュメント

- **[アーキテクチャ](architecture/)** - システム設計詳細
- **[デプロイメント](deployment/)** - 本番環境への配置
- **[.claude/CLAUDE.md](../../.claude/CLAUDE.md)** - 開発ガイドライン

---

[← 開発者向けドキュメントに戻る](../)