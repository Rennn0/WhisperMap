namespace XcLib.Data.Abstractions;

public abstract record ConnectionOptions
{
    public string? ConnectionString { get; set; }
    public string? Database { get; set; }
    public int? Port { get; set; }
}