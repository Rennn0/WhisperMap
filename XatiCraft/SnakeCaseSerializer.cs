using System.Text;
using System.Text.Json;

namespace XatiCraft;

/// <summary>
/// </summary>
public class SnakeCaseSerializer : JsonNamingPolicy
{
    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            char c = name[i];
            if (char.IsUpper(c))
            {
                if (i > 0) sb.Append('_');
                sb.Append(char.ToLower(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}