
using Azure;
using Azure.Data.Tables;
using System;

public record TableEntity : ITableEntity
{
    public string RowKey { get; set; } = default!;

    public string PartitionKey { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; } = default!;
    public ETag ETag { get; set; } = default!;

    public string Name { get; init; } = default!;

    public int views {get;set;}
}