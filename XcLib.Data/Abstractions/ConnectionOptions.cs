using System.ComponentModel.DataAnnotations;

namespace XcLib.Data.Abstractions;

public abstract record ConnectionOptions
{
    [Required] public string? ConnectionString { get; set; }
    public string? Database { get; set; }
    public int? Port { get; set; }
}