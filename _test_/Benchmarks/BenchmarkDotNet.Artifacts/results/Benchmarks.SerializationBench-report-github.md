```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8524/25H2/2025Update/HudsonValley2)
12th Gen Intel Core i7-12700K 3.60GHz, 1 CPU, 20 logical and 12 physical cores
.NET SDK 10.0.107
  [Host]   : .NET 8.0.28 (8.0.28, 8.0.2826.26413), X64 RyuJIT x86-64-v3
  .NET 8.0 : .NET 8.0.28 (8.0.28, 8.0.2826.26413), X64 RyuJIT x86-64-v3

Job=.NET 8.0  Runtime=.NET 8.0  

```
| Method                     |       Mean |    Error |    StdDev |     Median |     Gen0 |     Gen1 |     Gen2 |  Allocated |
|----------------------------|-----------:|---------:|----------:|-----------:|---------:|---------:|---------:|-----------:|
| Mempack_Deserialize        |   101.4 μs |  1.97 μs |   1.84 μs |   100.4 μs |  23.3154 |  12.0850 |        - |  298.02 KB |
| SystemTextJson_Deserialize |   421.2 μs |  6.05 μs |   5.05 μs |   419.5 μs |  29.7852 |  15.6250 |        - |  384.91 KB |
| Newtonsoft_Deserialize     | 1,370.8 μs | 17.01 μs |  13.28 μs | 1,368.1 μs | 166.0156 |  82.0313 |  82.0313 | 1891.87 KB |
| Mempack_Serialize          |   187.2 μs |  1.98 μs |   1.85 μs |   187.2 μs |  66.6504 |  66.6504 |  66.6504 |  206.17 KB |
| SystemTextJson_Serialize   |   242.9 μs |  3.14 μs |   2.79 μs |   242.0 μs |  76.1719 |  76.1719 |  76.1719 |  263.53 KB |
| Newtonsoft_Serialize       |   684.7 μs | 68.70 μs | 202.56 μs |   821.2 μs | 166.5039 | 166.5039 | 166.5039 |  817.02 KB |
