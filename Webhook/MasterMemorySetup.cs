using MasterMemory;
using MessagePack;
using Webhook.Tables;

namespace Webhook;

public class MasterMemorySetup
{
    public void Test()
    {
        ReadOnlySpan<byte> bts = Create();
        Save(bts);
        MemoryDatabase db = new MemoryDatabase(bts.ToArray());
        db.MmsPersonTable.TryFindById(2, out MmsPerson p);
        db.MmsPersonTable.TryFindById(40, out MmsPerson pNotYet);
        db.MmsPersonTable.TryFindByAgeAndName((1, "a"), out MmsPerson aaxax);
        RangeView<MmsPerson> zz = db.MmsPersonTable.FindRangeByKills(int.MinValue, int.MaxValue);
        db = new MemoryDatabase(Redeploy().ToArray());
        db.MmsPersonTable.TryFindById(40, out MmsPerson pmaybeHere);
        db.MmsPersonTable.TryFindById(2, out MmsPerson pnothere);
    }
    private const string DbPath = "0_mmsdb.bin";
    public static bool Exists => File.Exists(DbPath);
    public static void Save(ReadOnlySpan<byte> data) => File.WriteAllBytes(DbPath, data.ToArray());

    public static ReadOnlySpan<byte> Create()
    {
        DatabaseBuilder builder = new DatabaseBuilder();
        builder.Append(new List<MmsPerson>
        {
            new MmsPerson { Id = 1, Age = 21, Name = "Luka", Kills = 4 },
            new MmsPerson { Id = 2, Age = 25, Name = "Luka" },
            new MmsPerson { Id = 3, Age = 21, Name = "Giorgi" },
            new MmsPerson { Id = 4, Age = 19, Name = "Ana" },
            new MmsPerson { Id = 5, Age = 42, Name = "David" },
            new MmsPerson { Id = 6, Age = 37, Name = "Mariam" },
            new MmsPerson { Id = 7, Age = 28, Name = "Saba" },
            new MmsPerson { Id = 8, Age = 33, Name = "Elene" },
            new MmsPerson { Id = 9, Age = 24, Name = "Vakho" },
            new MmsPerson { Id = 10, Age = 45, Name = "Irakli" },
            new MmsPerson { Id = 11, Age = 18, Name = "Tako" },
            new MmsPerson { Id = 12, Age = 29, Name = "Lasha" },
            new MmsPerson { Id = 13, Age = 36, Name = "Nika" },
            new MmsPerson { Id = 14, Age = 27, Name = "Salome" },
            new MmsPerson { Id = 15, Age = 31, Name = "Zura" },
            new MmsPerson { Id = 16, Age = 22, Name = "Tea" },
            new MmsPerson { Id = 17, Age = 40, Name = "Levan" },
            new MmsPerson { Id = 18, Age = 26, Name = "Keti" },
            new MmsPerson { Id = 19, Age = 34, Name = "Beka" },
            new MmsPerson { Id = 20, Age = 23, Name = "Maka" }
        });
        return builder.Build();
    }

    public static ReadOnlySpan<byte> Redeploy()
    {
        DatabaseBuilder builder = new DatabaseBuilder();
        builder.Append(new List<MmsPerson>
        {
            new MmsPerson { Id = 1, Age = 21, Name = "Luka" },
            new MmsPerson { Id = 40, Age = 25, Name = "ZORO" }
        });
        return builder.Build();
    }

    public static MemoryDatabase ReadOrCreate() =>
        new MemoryDatabase(Exists ? File.ReadAllBytes(DbPath) : Create().ToArray());
}

[MemoryTable("mms_person")]
[MessagePackObject(true)]
public record MmsPerson
{
    [PrimaryKey] public required int Id { get; init; }

    [SecondaryKey(0)]
    [SecondaryKey(1)]
    public required int Age { get; init; }

    [SecondaryKey(1, 1)]
    [SecondaryKey(2, 5)]
    public required string Name { get; init; }

    [SecondaryKey(3, 4)]
    [SecondaryKey(2, 1)]
    public int? Kills { get; init; }
}