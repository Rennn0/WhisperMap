using System.Text;
using Newtonsoft.Json;

namespace XcLib.Shared.Utils;

public class NewtonsoftJsonSerializer : SystemJsonSerializer
{
    public override ReadOnlyMemory<byte> Serialize<T>(T value)
    {
        string json = JsonConvert.SerializeObject(value);
        return Encoding.UTF8.GetBytes(json);
    }

    public override T Deserialize<T>(ReadOnlyMemory<byte> bytes) =>
        JsonConvert.DeserializeObject<T>(
            Encoding.UTF8.GetString(bytes.Span)) ?? throw new InvalidOperationException();
}