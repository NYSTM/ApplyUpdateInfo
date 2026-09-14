# ApplyUpdateInfo

.NET 8 / WinFormsで動作する、修正情報JSONの確認・DB適用ツールです。

JSONまたはDBテーブルを読み込み、適用対象・操作種別・値を確認してから、対象DBへ安全に適用できます。

## プロジェクト構成

| プロジェクト | 役割 |
|---|---|
| `ApplyUpdateInfo` | 修正情報JSONの確認、バックアップ、DB更新を行うWinFormsアプリ |
| `ApplyUpdateInfo.DatabaseSetup` | 開発用SQLite DBを明示的に作成するコンソールアプリ |
| `ApplyUpdateInfo.Tests` | テストプロジェクト |

## 動作環境

- Windows
- .NET 8 SDK
- Microsoft Visual Studio 2026以降を推奨

## ビルド

ソリューション全体をビルドします。

```powershell
dotnet build .\ApplyUpdateInfo.slnx
```

ApplyUpdateInfoだけをビルドする場合:

```powershell
dotnet build .\ApplyUpdateInfo\ApplyUpdateInfo.csproj
```

## 開発用SQLite DBの作成

`ApplyUpdateInfo`は、接続時にSQLite DBやテーブルを自動作成しません。開発用DBが存在しない場合は、先に専用アプリを実行してください。

既定では`applyupdateinfo.test.db`を作成します。

```powershell
dotnet run --project .\ApplyUpdateInfo.DatabaseSetup\ApplyUpdateInfo.DatabaseSetup.csproj
```

既存の開発用DBを削除して作り直す場合:

```powershell
dotnet run --project .\ApplyUpdateInfo.DatabaseSetup\ApplyUpdateInfo.DatabaseSetup.csproj -- --reset
```

出力先を指定する場合:

```powershell
dotnet run --project .\ApplyUpdateInfo.DatabaseSetup\ApplyUpdateInfo.DatabaseSetup.csproj -- --database .\data\applyupdateinfo.test.db
```

既存DBは、`--reset`を指定しない限り変更されません。

## ApplyUpdateInfoの実行

1. 開発用SQLite DBを使用する場合は、DB作成アプリを実行します。
2. `ApplyUpdateInfo\appsettings.json`を実行ファイルと同じディレクトリに配置します。
3. `ApplyUpdateInfo`を起動します。
4. 接続先を選択し、テーブル一覧を読み込みます。
5. JSONまたはDBテーブルから修正内容を確認します。
6. 内容チェック後、必要に応じてバックアップを確認してDBへ適用します。

## 接続先設定

接続先は`ApplyUpdateInfo\appsettings.json`の`Database.Connections`に定義します。

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
	  }
	]
  }
}
```

SQLiteのファイルDBは既存ファイルに対してのみ接続します。存在しないファイルを接続時に自動作成しないため、本番DBのパスを誤って指定しても空のDBファイルは作成されません。

主な設定項目:

| 項目 | 説明 |
|---|---|
| `Name` | 接続先一覧に表示する名前 |
| `ProviderInvariantName` | `Microsoft.Data.Sqlite`または`Microsoft.Data.SqlClient` |
| `ConnectionString` | DB接続文字列 |
| `BackupDirectory` | DB適用前バックアップの保存先 |
| `MaxLoadRows` | Gridへ読み込める最大レコード数 |
| `Environment` | `Development`または`Production` |
| `BackgroundColor` | 接続先ごとのフォーム背景色 |

## 主な機能

- SQLite / SQL Serverへの接続
- 複数接続先の切り替え
- 開発・本番環境の表示と背景色の変更
- DBテーブルのGrid表示
- JSONの読み込みと編集
- `insert`、`update`、`delete`操作の確認
- 適用前の内容チェックと差分プレビュー
- 適用前バックアップ
- 本番環境向けの追加確認
- 適用結果の監査ログ
- 検索、フィルター、列幅自動調整
- `$now`によるJST現在時刻の設定

## 注意事項

- 本番DBへ接続する前に、接続先名、環境表示、背景色、対象テーブルを確認してください。
- 本番DB用の接続文字列やパスワードをソース管理へコミットしないでください。
- `MaxLoadRows`を大きくするとメモリ使用量が増加します。
- DB適用前にバックアップファイルが作成されたことを確認してください。
- 開発用DBの初期化SQLは`ApplyUpdateInfo.DatabaseSetup`だけが使用します。
