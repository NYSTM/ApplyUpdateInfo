# ApplyUpdateInfo

.NET 8 / WinFormsで動作する、修正情報JSONの確認・DB適用ツールです。

JSONまたはDBテーブルを読み込み、Grid上で適用対象・操作種別・値を確認してから、DBへ適用できます。

## 主な機能

- SQLite / SQL Serverへの接続
- `appsettings.json` に定義した複数接続先の切り替え
- 接続先ごとの開発系・本番系表示とフォーム背景色の変更
- 開発テーブルのGrid表示
- `Selected` 列による適用・出力対象の選択
- `OperationType` 列による行単位の操作種別選択
  - 新規（`insert`）
  - 更新（`update`）
  - 削除（`delete`）
- JSONの読み込みとGrid上での編集
- DB適用前の内容チェック
- 適用前バックアップ
- 大量データ読込の上限設定
- エラー・診断ログの画面表示
- 適用前の差分プレビューと操作件数確認
- 本番環境向けの追加確認
- Gridの検索・操作種別フィルター・選択行の一括種別変更
- Gridの列幅自動調整
- 適用結果の監査ログ
- バックアップからの復元用JSON作成API
- DB接続テストAPI

## 動作環境

- Windows
- .NET 8 SDK
- Microsoft Visual Studio 2026以降を推奨

## ビルド

ソリューションまたはプロジェクトをVisual Studioで開いてビルドします。

```powershell
dotnet build .\ApplyUpdateInfo\ApplyUpdateInfo.csproj
```

開発用SQLite DBを新規作成する場合は、先にDB作成アプリを実行してください。

```powershell
dotnet run --project .\ApplyUpdateInfo.DatabaseSetup\ApplyUpdateInfo.DatabaseSetup.csproj -- --reset
```

DB作成アプリは既定で`applyupdateinfo.test.db`を作成します。別のパスを指定する場合は`--database <パス>`を使用してください。既存DBは`--reset`を指定しない限り変更されません。

実行ファイルは通常、次の場所に生成されます。

```text
ApplyUpdateInfo\bin\Debug\net8.0-windows\
```

## 設定

`appsettings.json` は実行ファイルと同じディレクトリに配置してください。

接続先は `Database.Connections` に配列で定義します。

```json
{
  "Database": {
	"Connections": [
	  {
		"Name": "テストDB",
		"ProviderInvariantName": "Microsoft.Data.Sqlite",
		"ConnectionString": "Data Source=applyupdateinfo.test.db",
		"BackupDirectory": "backups/test",
		"MaxLoadRows": 10000,
		"Environment": "Development",
		"BackgroundColor": "#EAF3FF"
	  },
	  {
		"Name": "本番DB",
		"ProviderInvariantName": "Microsoft.Data.Sqlite",
		"ConnectionString": "Data Source=applyupdateinfo.production.db",
		"BackupDirectory": "backups/production",
		"MaxLoadRows": 10000,
		"Environment": "Production",
		"BackgroundColor": "#FFF0F0"
	  },
	  {
		"Name": "SQL Server開発DB",
		"ProviderInvariantName": "Microsoft.Data.SqlClient",
		"ConnectionString": "Data Source=IP\\AAA,5000;Initial Catalog=AAADB;User ID=<user>;Password=<password>;TrustServerCertificate=True;",
		"BackupDirectory": "backups/sqlserver-development",
		"MaxLoadRows": 10000,
		"Environment": "Development",
		"BackgroundColor": "#EAF3FF"
	  }
	]
  }
}
```

### 設定項目

| 項目 | 説明 |
|---|---|
| `Name` | 画面の接続先一覧に表示する名前 |
| `ProviderInvariantName` | DBプロバイダー名。SQLiteは`Microsoft.Data.Sqlite`、SQL Serverは`Microsoft.Data.SqlClient` |
| `ConnectionString` | DB接続文字列 |
| `BackupDirectory` | DB適用前バックアップの保存先 |
| `MaxLoadRows` | Gridへ読み込める最大レコード数 |
| `Environment` | `Development` または `Production` などの環境区分 |
| `BackgroundColor` | フォーム背景色。HTML形式のカラーコード |

本番接続のパスワードを設定ファイルへ直接保存する場合は、ファイルのアクセス権を制限してください。可能であれば、Windows認証や安全なシークレット管理を使用してください。

SQLiteファイルDBは既存ファイルに対してのみ接続します。存在しないファイルを接続時に自動作成しないため、開発用DBは`ApplyUpdateInfo.DatabaseSetup`で事前に作成してください。

## 基本操作

### 1. 接続先を選択

画面上部の接続先コンボボックスから対象DBを選択し、「接続切替」を押します。

接続先切り替え時は、現在表示中のデータや未適用の編集内容がリセットされます。

### 2. 開発テーブルを読み込む

左側のテーブル一覧からテーブルを選択します。

大量データ対策として、テーブル件数が `MaxLoadRows` を超える場合は読み込みを中止します。上限値を超えるテーブルを開く場合は、設定値を見直してください。

### 3. 操作種別と適用対象を設定

開発テーブル表示では、次の列を使用します。

- `Selected`: JSON出力対象または適用対象
- `OperationType`: 行単位の操作種別

`OperationType` は手入力ではなく、コンボボックスから選択します。

### 4. 修正情報JSONを作成

「修正情報JSON作成」を押して保存先を指定します。

`Selected` が有効な行だけがJSONに出力されます。操作種別は行ごとにJSONの `type` へ反映されます。

### 5. JSONを読み込む

「JSON読込」で修正情報JSONを読み込みます。

JSON読込後もGrid上で次の項目を編集できます。

- `Selected`
- `OperationType`
- キー列・値列

値列のセルを選択して「現在時刻を設定」を押すと、そのセルへ`$now`を設定できます。`$now`は適用時にJSTの現在時刻へ変換されます。主キー列には設定できません。

操作種別を変更すると、主キー情報が操作種別に合わせて自動同期されます。

### 6. 適用前チェックとDB適用

「適用内容をチェック」を押すと、選択された操作についてDBとの整合性を確認します。

- 新規: 同一主キーのレコードが存在しないこと
- 更新: 対象レコードが1件だけ存在すること
- 削除: 対象レコードが1件だけ存在すること

チェック成功後に「選択内容をDBへ適用」が有効になります。

DB適用時には、適用対象のキー・値と操作件数をプレビュー表示します。本番環境では接続先名と本番環境であることを明示した確認メッセージを表示します。

以下の操作を行うとチェック済み状態は無効になり、再チェックが必要になります。

- Gridの値を変更
- `Selected` を変更
- `OperationType` を変更
- JSONを再読込
- 接続先を切り替え

## JSON形式

```json
{
  "tableName": "Users",
  "operations": [
	{
	  "type": "insert",
	  "values": {
		"Id": 99,
		"Name": "新しいユーザー"
	  }
	},
	{
	  "type": "update",
	  "keys": {
		"Id": 1
	  },
	  "values": {
		"Name": "更新後の名前"
	  }
	},
	{
	  "type": "delete",
	  "keys": {
		"Id": 2
	  }
	}
  ]
}
```

### 操作種別ごとのキー

- `insert`: 新規レコード全体を `values` に指定します。主キーも `values` に含めてください。
- `update`: 対象レコードを特定する主キーを `keys` に指定し、更新値を `values` に指定します。
- `delete`: 対象レコードを特定する主キーを `keys` に指定します。

### 現在時刻の予約トークン

`insert` または `update` の `values` には、現在時刻を設定する予約トークン `$now` を指定できます。

```json
{
  "type": "update",
  "keys": { "Id": 1 },
  "values": {
	"UpdatedAt": "$now"
  }
}
```

`$now` は日本標準時（JST、UTC+09:00）の適用予定時刻としてプレビューに表示され、確認後のDB適用でも同じJST時刻が使用されます。通常の文字列値として `$now` を保存する用途には使用できません。

## バックアップ

DB適用前に、対象操作のバックアップが `BackupDirectory` に作成されます。

本番DBへ適用する場合は、接続先・対象テーブル・操作種別・主キーを必ず確認してください。

適用結果は接続先の`BackupDirectory`に`audit.log.jsonl`として記録されます。バックアップJSONは`BackupRestoreService.CreateRestoreJsonAsync`を使用して復元用JSONへ変換できます。

## 検索・フィルター

Grid上部の検索欄で文字列を検索できます。表示用の操作種別フィルターでは「すべて」「新規」「更新」「削除」を選択できます。「すべて」は表示フィルターを解除するための選択肢です。

一括変更用の操作種別コンボボックスでは「新規」「更新」「削除」から変更先を選択し、「選択行の種別変更」を押すと、Gridで選択している行を一括変更できます。表示フィルターで「すべて」を選択していても、一括変更用コンボボックスから操作種別を選択できます。

値が見切れる場合は「列幅自動調整」を押してください。表示中の値に合わせてGridの各列幅を調整できます。データ量や列数によって横幅が広くなる場合は、横スクロールバーを使用してください。

## SQL Server

SQL Server接続には`Microsoft.Data.SqlClient`を使用します。`dbo.Users`のようにスキーマ付きのテーブル名にも対応しています。接続先の接続確認には`DatabaseUpdateService.TestConnectionAsync`を使用します。

## 関連ファイル

- `UpdateInfoForm.cs`: メイン画面と操作フロー
- `DatabaseUpdateService.cs`: DB接続、読込、検証、適用、バックアップ
- `UpdateInfoModels.cs`: JSONモデルとJSON検証
- `UpdateInfoExportService.cs`: 開発テーブルからJSONを作成
- `appsettings.json`: 接続先設定
- `ApplyUpdateInfo.DatabaseSetup`: 開発用SQLite DB作成アプリ
- `sample-update-info.json`: JSONサンプル
- `UpdateOperationSummary.cs`: 操作件数集計と監査ログモデル
- `ApplyPreviewDialog.cs`: 適用前プレビュー
- `AuditLogService.cs`: 監査ログと復元用JSON

## 注意事項

- 本番DBで操作する前に、必ず接続先の背景色・環境表示・テーブル名を確認してください。
- `MaxLoadRows` は安全対策のための上限です。大きな値に変更するとメモリ使用量が増加します。
- DB適用はバックアップ作成後に実行されますが、適用前にバックアップファイルが作成されたことを確認してください。
- 接続文字列やパスワードをソース管理へコミットしないでください。
