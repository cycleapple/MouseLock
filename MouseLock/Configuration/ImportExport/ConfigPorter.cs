using System;
using Dalamud.Bindings.ImGui;
using MouseLock.Configuration.Persistence;

namespace MouseLock.Configuration.ImportExport;

internal static class ConfigPorter
{
    public static bool TryExportConfigToClipboard(SystemConfiguration config, out string message)
    {
        try
        {
            var exportString = ConfigSerializer.SerializeConfig(config);
            ImGui.SetClipboardText(exportString);

            message = "已將設定匯出至剪貼簿。";
            Service.Logger.Information(message);
            return true;
        }
        catch (Exception exception)
        {
            message = "設定匯出失敗。";
            Service.Logger.Error(exception, message);
            return false;
        }
    }

    public static bool TryImportConfigFromClipboard(SystemConfiguration target, out string message)
    {
        var clipboard = ImGui.GetClipboardText();
        if (!ConfigSerializer.TryDeserializeConfig(clipboard, out var imported, out message) || imported is null)
        {
            Service.Logger.Warning(message);
            return false;
        }

        target.CopyFrom(imported);
        ConfigRepository.SaveImmediate(target);
        PluginState.MouseLookService?.RefreshCurrentStatus();
        PluginState.DtrStatusService?.Refresh();

        message = "已從剪貼簿匯入設定。";
        Service.Logger.Information(message);
        return true;
    }

    public static bool TryResetConfig(SystemConfiguration target, out string message)
    {
        try
        {
            var reset = new SystemConfiguration();
            reset.EnsureInitialized();

            target.CopyFrom(reset);
            ConfigRepository.SaveImmediate(target);
            PluginState.MouseLookService?.RefreshCurrentStatus();
            PluginState.DtrStatusService?.Refresh();

            message = "已將設定重設為預設值。";
            Service.Logger.Information(message);
            return true;
        }
        catch (Exception exception)
        {
            message = "設定重設失敗。";
            Service.Logger.Error(exception, message);
            return false;
        }
    }

    public static bool TryRestoreLatestBackup(SystemConfiguration target, out string message)
    {
        try
        {
            if (!ConfigBackup.TryLoadLatestBackup(out var backupJson, out message))
            {
                Service.Logger.Warning(message);
                return false;
            }

            if (!ConfigSerializer.TryDeserializeConfig(backupJson, out var backupConfig, out message) || backupConfig is null)
            {
                Service.Logger.Warning(message);
                return false;
            }

            target.CopyFrom(backupConfig);
            ConfigRepository.SaveImmediate(target);
            PluginState.MouseLookService?.RefreshCurrentStatus();
            PluginState.DtrStatusService?.Refresh();

            message = "已還原最新備份。";
            Service.Logger.Information(message);
            return true;
        }
        catch (Exception exception)
        {
            message = "備份還原失敗。";
            Service.Logger.Error(exception, message);
            return false;
        }
    }
}
