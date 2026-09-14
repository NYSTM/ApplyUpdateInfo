using Microsoft.Data.Sqlite;

namespace ApplyUpdateInfo.DatabaseSetup;

internal static class Program
{
    private const string DefaultDatabaseFileName = "applyupdateinfo.test.db";

    private static int Main(string[] args)
    {
        try
        {
            Options options = ParseOptions(args);
            string databasePath = Path.GetFullPath(options.DatabasePath);

            if (File.Exists(databasePath) && !options.Reset)
            {
                Console.WriteLine($"開発用DBは既に存在します。初期化を行いません: {databasePath}");
                Console.WriteLine("作り直す場合は --reset を指定してください。");
                return 0;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
            if (options.Reset)
            {
                DeleteDatabaseFiles(databasePath);
            }

            string script = ReadInitializationScript();
            using SqliteConnection connection = new(new SqliteConnectionStringBuilder
            {
                DataSource = databasePath,
                ForeignKeys = true
            }.ConnectionString);
            connection.Open();
            using SqliteCommand command = connection.CreateCommand();
            command.CommandText = script;
            command.ExecuteNonQuery();

            Console.WriteLine($"開発用DBを作成しました: {databasePath}");
            return 0;
        }
        catch (ArgumentException exception)
        {
            Console.Error.WriteLine($"引数エラー: {exception.Message}");
            PrintUsage();
            return 2;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"開発用DBの作成に失敗しました: {exception.Message}");
            return 1;
        }
    }

    private static Options ParseOptions(string[] args)
    {
        string databasePath = DefaultDatabaseFileName;
        bool reset = false;

        for (int index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--database":
                    if (++index >= args.Length || string.IsNullOrWhiteSpace(args[index]))
                    {
                        throw new ArgumentException("--databaseにはDBファイルのパスを指定してください。");
                    }

                    databasePath = args[index];
                    break;
                case "--reset":
                    reset = true;
                    break;
                case "--help":
                case "-h":
                    PrintUsage();
                    Environment.Exit(0);
                    break;
                default:
                    throw new ArgumentException($"不明なオプションです: {args[index]}");
            }
        }

        return new(databasePath, reset);
    }

    private static string ReadInitializationScript()
    {
        const string resourceName = "ApplyUpdateInfo.DatabaseSetup.sqlite-init.sql";
        using Stream stream = typeof(Program).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("初期化SQLがアプリケーションに埋め込まれていません。");
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    private static void DeleteDatabaseFiles(string databasePath)
    {
        foreach (string path in new[] { databasePath, $"{databasePath}-wal", $"{databasePath}-shm" })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("使用方法: ApplyUpdateInfo.DatabaseSetup [--database <パス>] [--reset]");
        Console.WriteLine($"既定のDB: {DefaultDatabaseFileName}");
        Console.WriteLine("--reset 既存のDBとSQLiteの補助ファイルを削除して作り直します。");
    }

    private sealed record Options(string DatabasePath, bool Reset);
}
