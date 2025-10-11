# Gmail API セットアップガイド

このドキュメントでは、Hypergraphia ConsoleアプリケーションでGmail APIを使用するために必要な認証情報ファイル (`gmail-credentials.json`) を作成する手順を説明します。

## 前提条件

- Googleアカウント（個人用GmailまたはGoogle Workspace）
- Google Cloud Platform (GCP) プロジェクトへのアクセス権限

## 認証方式について

このアプリケーションは **OAuth 2.0 デスクトップアプリケーション** 認証方式を使用します。初回実行時にブラウザが開き、Googleアカウントでの認証が求められます。認証後、アクセストークンは `gmail-token.json` に保存され、次回以降は自動的に再利用されます。

## セットアップ手順

### 1. Google Cloud Platformプロジェクトの作成

1. [Google Cloud Console](https://console.cloud.google.com/) にアクセス
2. 新しいプロジェクトを作成、または既存のプロジェクトを選択
3. プロジェクト名を入力（例: "Hypergraphia Gmail Integration"）

### 2. Gmail APIの有効化

1. Google Cloud Consoleで、左側のメニューから **「APIとサービス」** > **「ライブラリ」** を選択
2. 検索バーで「Gmail API」を検索
3. **「Gmail API」** を選択
4. **「有効にする」** ボタンをクリック

### 3. 必要なAPIスコープ（権限）

このアプリケーションでは、以下のGmail API操作を使用します:

| 操作 | 説明 | 必要なスコープ |
|------|------|----------------|
| メッセージ一覧の取得 | `Users.Messages.List` | `https://www.googleapis.com/auth/gmail.readonly` |
| メッセージ詳細の取得 | `Users.Messages.Get` | `https://www.googleapis.com/auth/gmail.readonly` |
| ラベル一覧の取得 | `Users.Labels.List` | `https://www.googleapis.com/auth/gmail.readonly` |
| ラベル詳細の取得 | `Users.Labels.Get` | `https://www.googleapis.com/auth/gmail.readonly` |
| 添付ファイルの取得 | `Users.Messages.Attachments.Get` | `https://www.googleapis.com/auth/gmail.readonly` |

**推奨スコープ**: `https://www.googleapis.com/auth/gmail.readonly`
- このスコープは読み取り専用のため、メールの変更や削除はできません
- セキュリティ上、最小限の権限で動作します

### 4. OAuth 2.0 同意画面の設定

OAuth 2.0 クライアント ID を作成する前に、同意画面の設定が必要です：

1. 左側のメニューから **「APIとサービス」** > **「OAuth 同意画面」** を選択
2. User Type を選択:
   - **個人用Gmail**: **「外部」** を選択
   - **Google Workspace**: **「内部」** または **「外部」** を選択
3. **「作成」** をクリック
4. アプリ情報を入力:
   - **アプリ名**: `Hypergraphia`
   - **ユーザーサポートメール**: 自分のメールアドレス
   - **デベロッパーの連絡先情報**: 自分のメールアドレス
5. **「保存して次へ」** をクリック
6. スコープの追加:
   - **「スコープを追加または削除」** をクリック
   - フィルタで `gmail` を検索
   - `https://www.googleapis.com/auth/gmail.readonly` を選択
   - **「更新」** をクリック
7. **「保存して次へ」** をクリック
8. テストユーザーの追加（外部アプリの場合）:
   - **「ADD USERS」** をクリック
   - 自分のGmailアドレスを追加
   - **「保存して次へ」** をクリック
9. 概要を確認して **「ダッシュボードに戻る」** をクリック

### 5. OAuth 2.0 クライアント ID の作成

1. 左側のメニューから **「APIとサービス」** > **「認証情報」** を選択
2. 上部の **「認証情報を作成」** をクリック
3. **「OAuth 2.0 クライアント ID」** を選択
4. アプリケーションの種類で **「デスクトップ アプリ」** を選択
5. 名前を入力（例: `Hypergraphia Desktop Client`）
6. **「作成」** をクリック
7. 表示されたダイアログで **「JSON をダウンロード」** をクリック

### 6. 認証情報ファイルの配置

1. ダウンロードしたJSONファイルの名前を `gmail-credentials.json` に変更
2. ファイルを以下の場所に配置:
   ```
   Hypergraphia.Console/gmail-credentials.json
   ```

**JSONファイルの形式**:

OAuth 2.0 デスクトップアプリの認証情報ファイルには `installed` キーが含まれます：

```json
{
  "installed": {
    "client_id": "YOUR_CLIENT_ID.apps.googleusercontent.com",
    "project_id": "your-project-id",
    "auth_uri": "https://accounts.google.com/o/oauth2/auth",
    "token_uri": "https://oauth2.googleapis.com/token",
    "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
    "client_secret": "YOUR_CLIENT_SECRET",
    "redirect_uris": ["http://localhost"]
  }
}
```

### 7. 初回認証フロー

アプリケーションを初めて実行すると、以下の流れで認証が行われます：

1. アプリケーションを実行:
   ```bash
   dotnet run --project Hypergraphia.Console
   ```

2. ブラウザが自動的に開き、Google アカウントへのログインを求められます

3. アプリケーションが Gmail へのアクセスを要求していることを確認

4. **「許可」** をクリック

5. 認証成功後、ブラウザに「認証が完了しました」というメッセージが表示されます

6. アクセストークンが `gmail-token.json` ディレクトリに保存されます

7. 以降の実行では、`gmail-token.json` のトークンが自動的に使用されるため、ブラウザは開きません

**トークンの保存場所**: プロジェクトルートの `gmail-token.json/` ディレクトリ

## セキュリティ上の注意事項

⚠️ **重要**: 以下のファイルには機密情報が含まれています：

### 保護が必要なファイル

1. **`gmail-credentials.json`** - OAuth 2.0 クライアント ID 認証情報
   - クライアント ID とシークレットが含まれます
   - `.gitignore` に追加済み

2. **`gmail-token.json/`** - Gmail OAuth 認証トークン（初回認証後に自動生成）
   - ユーザーのアクセストークンとリフレッシュトークンが含まれます
   - `.gitignore` に追加済み

### セキュリティベストプラクティス

- これらのファイルを Git リポジトリにコミットしないでください
- ファイルを他人と共有しないでください
- 本番環境では、環境変数やシークレット管理サービスの使用を検討してください
- トークンが漏洩した場合は、Google Cloud Console で OAuth クライアント ID を無効化してください

## トラブルシューティング

### エラー: "Gmail認証ファイルが見つかりません"

**原因**: `gmail-credentials.json` が見つからない

**解決策**:
- ファイルが `Hypergraphia.Console/gmail-credentials.json` に配置されているか確認
- ファイル名が正確に一致しているか確認（大文字小文字を含む）
- ビルド時にファイルが出力ディレクトリにコピーされているか確認

### エラー: "Unrecognized credential type"

**原因**: 認証情報ファイルの形式が正しくない

**解決策**:
- OAuth 2.0 クライアント ID（デスクトップアプリ）の認証情報を使用していることを確認
- JSONファイルに `installed` キーが含まれているか確認
- サービスアカウント形式（`service_account` キー）ではなく、デスクトップアプリ形式を使用

### ブラウザが開かない / 認証画面が表示されない

**原因**: ブラウザの起動に失敗している

**解決策**:
- デフォルトブラウザが正しく設定されているか確認
- ファイアウォールがブラウザの起動をブロックしていないか確認
- 手動で認証URLをブラウザで開く（コンソールに表示されます）

### エラー: "Access blocked: This app's request is invalid"

**原因**: OAuth 同意画面の設定が不完適

**解決策**:
- OAuth 同意画面で必要なスコープ (`gmail.readonly`) が追加されているか確認
- テストユーザーに自分のメールアドレスが追加されているか確認（外部アプリの場合）
- OAuth クライアント ID が「デスクトップアプリ」タイプであることを確認

### トークンの再認証

トークンが期限切れまたは無効になった場合：

1. `gmail-token.json/` ディレクトリを削除
2. アプリケーションを再実行
3. ブラウザで再度認証を行う

## 関連リンク

- [Gmail API ドキュメント](https://developers.google.com/gmail/api)
- [Google Cloud 認証情報の管理](https://cloud.google.com/docs/authentication/getting-started)
- [サービスアカウントの作成と管理](https://cloud.google.com/iam/docs/creating-managing-service-accounts)
