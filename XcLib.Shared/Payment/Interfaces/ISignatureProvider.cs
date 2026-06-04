using System.Text.Json;

namespace XcLib.Shared.Payment.Interfaces;

public interface ISignatureProvider
{
    string Sign(object data);
    string Sign(JsonElement data);
}