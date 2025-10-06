# Gmail API セットアップガイド

このドキュメントでは、Hypergraphia ConsoleアプリケーションでGmail APIを使用するために必要な認証情報ファイル (`gmail-credentials.json`) を作成する手順を説明します。

## 前提条件

- Googleアカウント
- Google Cloud Platform (GCP) プロジェクトへのアクセス権限

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

### 4. サービスアカウントの作成（個人用Gmailの場合は不要）

⚠️ **注意**: サービスアカウントはGoogle Workspaceアカウントでのみ使用できます。個人用Gmailアカウントの場合は、次のセクション「OAuth 2.0クライアントIDの作成」に進んでください。

1. 左側のメニューから **「APIとサービス」** > **「認証情報」** を選択
2. 上部の **「認証情報を作成」** をクリック
3. **「サービスアカウント」** を選択
4. サービスアカウントの詳細を入力:
   - サービスアカウント名: `hypergraphia-gmail-service`
   - サービスアカウントID: 自動生成される
   - サービスアカウントの説明: `Gmail API access for Hypergraphia`
5. **「作成して続行」** をクリック
6. ロールの選択（オプション）で **「続行」** をクリック
7. **「完了」** をクリック

#### Google Workspaceでドメイン全体の委任を設定（管理者のみ）

サービスアカウントでユーザーのGmailにアクセスするには、ドメイン全体の委任が必要です:

1. 作成したサービスアカウントを開く
2. **「詳細を表示」** をクリック
3. クライアントIDをコピー
4. [Google Workspace管理コンソール](https://admin.google.com/) にアクセス
5. **「セキュリティ」** > **「アクセスとデータ管理」** > **「APIの制御」** を選択
6. **「ドメイン全体の委任を管理」** をクリック
7. **「新しく追加」** をクリック
8. クライアントIDとスコープを入力:
   - クライアントID: コピーしたサービスアカウントのクライアントID
   - OAuth スコープ: `https://www.googleapis.com/auth/gmail.readonly`
9. **「承認」** をクリック

### 5. サービスアカウントキーの作成

1. 作成したサービスアカウントをクリック
2. **「キー」** タブを選択
3. **「鍵を追加」** > **「新しい鍵を作成」** をクリック
4. キーのタイプで **「JSON」** を選択
5. **「作成」** をクリック
6. JSONファイルが自動的にダウンロードされます

### 6. OAuth 2.0クライアントIDの作成（個人用Gmail用）

個人用Gmailアカウントでアプリケーションを使用する場合:

1. 左側のメニューから **「APIとサービス」** > **「認証情報」** を選択
2. 上部の **「認証情報を作成」** をクリック
3. **「OAuth 2.0 クライアント ID」** を選択
4. 同意画面の構成が必要な場合:
   - **「OAuth同意画面を構成」** をクリック
   - User Type: **「外部」** を選択（個人用の場合）
   - アプリ情報を入力:
     - アプリ名: `Hypergraphia`
     - ユーザーサポートメール: 自分のメールアドレス
     - デベロッパーの連絡先情報: 自分のメールアドレス
   - **「保存して次へ」** をクリック
   - スコープの追加:
     - **「スコープを追加または削除」** をクリック
     - `https://www.googleapis.com/auth/gmail.readonly` を追加
   - テストユーザーの追加:
     - **「ADD USERS」** をクリック
     - 自分のGmailアドレスを追加
   - **「保存して次へ」** をクリック
5. **「認証情報」** タブに戻る
6. **「認証情報を作成」** > **「OAuth 2.0 クライアント ID」** を選択
7. アプリケーションの種類で **「デスクトップアプリ」** を選択
8. 名前を入力（例: `Hypergraphia Desktop Client`）
9. **「作成」** をクリック
10. JSONファイルをダウンロード

### 7. 認証情報ファイルの配置

1. ダウンロードしたJSONファイルの名前を `gmail-credentials.json` に変更
2. ファイルを以下の場所に配置:
   ```
   Hypergraphia.Console/gmail-credentials.json
   ```

**JSONファイルの形式**:

- **サービスアカウント**の場合: `type: "service_account"` を含む
- **OAuth 2.0クライアント**の場合: `installed` または `web` キーを含む

## セキュリティ上の注意事項

⚠️ **重要**: `gmail-credentials.json` ファイルには機密情報が含まれています。

- このファイルは `.gitignore` に追加されており、Gitリポジトリには含まれません
- ファイルを他人と共有しないでください
- 本番環境では、環境変数やシークレット管理サービスの使用を検討してください

## トラブルシューティング

### エラー: "The caller does not have permission"

- Gmail APIが有効になっているか確認
- サービスアカウントに適切な権限が付与されているか確認
- Google Workspace管理者の場合、ドメイン全体の委任が必要な場合があります

### ファイルが見つからないエラー

- `gmail-credentials.json` が正しいディレクトリに配置されているか確認
- ファイル名が正確に一致しているか確認（大文字小文字を含む）

## 関連リンク

- [Gmail API ドキュメント](https://developers.google.com/gmail/api)
- [Google Cloud 認証情報の管理](https://cloud.google.com/docs/authentication/getting-started)
- [サービスアカウントの作成と管理](https://cloud.google.com/iam/docs/creating-managing-service-accounts)
