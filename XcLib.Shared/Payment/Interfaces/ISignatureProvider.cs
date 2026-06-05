namespace XcLib.Shared.Payment.Interfaces;

public interface ISignatureProvider
{
    string Sign(object data);
    string Sign(List<string> values);
}