using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MouseLock.Configuration.Persistence;

internal static class ConfigSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    public static string SerializeConfig(SystemConfiguration config)
    {
        config.EnsureInitialized();

        var json = JsonSerializer.Serialize(config, Options);
        return Convert.ToBase64String(Dalamud.Utility.Util.CompressString(json));
    }

    public static bool TryDeserializeConfig(
        string input,
        out SystemConfiguration? config,
        out string error)
    {
        config = null;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "剪貼簿是空的。";
            return false;
        }

        var trimmed = input.Trim();
        if (TryDecompress(trimmed, out var json) && TryDeserializeJson(json, out config, out error))
        {
            return true;
        }

        if (trimmed.StartsWith('{') && TryDeserializeJson(trimmed, out config, out error))
        {
            return true;
        }

        if (string.IsNullOrEmpty(error))
        {
            error = "剪貼簿內容不是有效的 MouseLock 設定資料。";
        }

        return false;
    }

    private static bool TryDecompress(string input, out string json)
    {
        try
        {
            json = Dalamud.Utility.Util.DecompressString(Convert.FromBase64String(input));
            return true;
        }
        catch
        {
            json = string.Empty;
            return false;
        }
    }

    private static bool TryDeserializeJson(
        string json,
        out SystemConfiguration? config,
        out string error)
    {
        config = null;
        error = string.Empty;

        try
        {
            var imported = JsonSerializer.Deserialize<SystemConfiguration>(json, Options);
            if (imported is null)
            {
                error = "剪貼簿內容未包含 MouseLock 設定。";
                return false;
            }

            if (imported.Version > SystemConfiguration.CurrentVersion)
            {
                error = $"設定版本 {imported.Version} 高於目前支援的版本 {SystemConfiguration.CurrentVersion}。";
                return false;
            }

            imported.EnsureInitialized();
            config = imported;
            return true;
        }
        catch (Exception exception) when (exception is JsonException or NotSupportedException)
        {
            error = "無法將剪貼簿內容解析為 MouseLock 設定 JSON。";
            return false;
        }
    }
}
