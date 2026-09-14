namespace ApplyUpdateInfo;

public sealed record OperationSummary(int InsertCount, int UpdateCount, int DeleteCount)
{
    public int TotalCount => InsertCount + UpdateCount + DeleteCount;

    public static OperationSummary FromOperations(IEnumerable<UpdateOperation> operations)
    {
        int insertCount = 0;
        int updateCount = 0;
        int deleteCount = 0;
        foreach (UpdateOperation operation in operations.Where(static operation => operation.IsSelected))
        {
            switch (operation.Type.ToLowerInvariant())
            {
                case "insert": insertCount++; break;
                case "update": updateCount++; break;
                case "delete": deleteCount++; break;
            }
        }

        return new(insertCount, updateCount, deleteCount);
    }

    public override string ToString() => $"新規 {InsertCount:N0}件 / 更新 {UpdateCount:N0}件 / 削除 {DeleteCount:N0}件";
}

public sealed record AuditLogEntry(
    DateTimeOffset CreatedAtJst,
    string ConnectionName,
    string TableName,
    OperationSummary Summary,
    bool Succeeded,
    string? BackupPath,
    string? ErrorMessage);
