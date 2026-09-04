using System;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using MouseLock.Configuration;
using MouseLock.Configuration.ImportExport;
using MouseLock.Configuration.Persistence;

namespace MouseLock.Windows.Components;

internal sealed class ConfigurationTransferPanel(SystemConfiguration config)
{
    private const string ConfirmationPopupName = "確認變更設定";
    private const float ConfirmationPopupWidth = 420.0f;
    private const float ConfirmationButtonWidth = 96.0f;

    private string _message = string.Empty;
    private bool _messageIsError;
    private ConfirmationAction _pendingConfirmation;

    public void Draw()
    {
        if (ImGui.Button("匯出至剪貼簿"))
        {
            SetMessage(ConfigPorter.TryExportConfigToClipboard(config, out var message), message);
        }

        ImGui.SameLine();
        if (ImGui.Button("按住 Shift 匯入"))
        {
            if (!Service.KeyState[VirtualKey.SHIFT])
            {
                SetMessage(false, "請按住 Shift 再點擊匯入。");
            }
            else
            {
                OpenConfirmation(ConfirmationAction.Import);
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("重設為預設值"))
        {
            OpenConfirmation(ConfirmationAction.Reset);
        }

        ImGui.Spacing();
        if (ImGui.Button("還原最新備份"))
        {
            OpenConfirmation(ConfirmationAction.RestoreLatestBackup);
        }

        ImGui.SameLine();
        if (ImGui.Button("開啟備份資料夾"))
        {
            SetMessage(TryOpenBackupFolder(out var message), message);
        }

        if (!string.IsNullOrEmpty(_message))
        {
            ImGui.TextColored(
                _messageIsError ? ImGuiColors.DalamudRed : ImGuiColors.HealerGreen,
                _message);
        }

        DrawConfirmationPopup();
    }

    private void SetMessage(bool success, string message)
    {
        _messageIsError = !success;
        _message = message;
    }

    private void OpenConfirmation(ConfirmationAction action)
    {
        _pendingConfirmation = action;
        ImGui.OpenPopup(ConfirmationPopupName);
    }

    private void DrawConfirmationPopup()
    {
        var open = _pendingConfirmation != ConfirmationAction.None;
        var popupWidth = ConfirmationPopupWidth * ImGuiHelpers.GlobalScale;
        ImGui.SetNextWindowSize(new Vector2(popupWidth, 0.0f), ImGuiCond.Appearing);
        ImGui.SetNextWindowSizeConstraints(
            new Vector2(popupWidth, 0.0f),
            new Vector2(popupWidth, float.MaxValue));

        using var popup = ImRaii.PopupModal(ConfirmationPopupName, ref open, ImGuiWindowFlags.AlwaysAutoResize);
        if (!popup)
        {
            if (!open)
            {
                _pendingConfirmation = ConfirmationAction.None;
            }

            return;
        }

        ImGui.TextWrapped(GetConfirmationText());
        ImGui.Spacing();

        var buttonSize = new Vector2(ConfirmationButtonWidth * ImGuiHelpers.GlobalScale, 0.0f);
        if (ImGui.Button(GetConfirmLabel(), buttonSize))
        {
            ConfirmPendingAction();
            ImGui.CloseCurrentPopup();
            _pendingConfirmation = ConfirmationAction.None;
        }

        ImGui.SameLine();
        if (ImGui.Button("取消", buttonSize))
        {
            ImGui.CloseCurrentPopup();
            _pendingConfirmation = ConfirmationAction.None;
        }
    }

    private string GetConfirmLabel()
        => _pendingConfirmation switch
        {
            ConfirmationAction.Reset => "重設",
            ConfirmationAction.RestoreLatestBackup => "還原",
            _ => "匯入",
        };

    private string GetConfirmationText()
        => _pendingConfirmation switch
        {
            ConfirmationAction.Import => "要以剪貼簿中的設定取代目前的 MouseLock 設定嗎？",
            ConfirmationAction.Reset => "要將所有 MouseLock 設定重設為預設值嗎？",
            ConfirmationAction.RestoreLatestBackup => "要以最新的自動備份取代目前的 MouseLock 設定嗎？",
            _ => string.Empty,
        };

    private void ConfirmPendingAction()
    {
        switch (_pendingConfirmation)
        {
            case ConfirmationAction.Import:
                SetMessage(ConfigPorter.TryImportConfigFromClipboard(config, out var importMessage), importMessage);
                break;

            case ConfirmationAction.Reset:
                SetMessage(ConfigPorter.TryResetConfig(config, out var resetMessage), resetMessage);
                break;

            case ConfirmationAction.RestoreLatestBackup:
                SetMessage(ConfigPorter.TryRestoreLatestBackup(config, out var restoreMessage), restoreMessage);
                break;
        }
    }

    private static bool TryOpenBackupFolder(out string message)
    {
        try
        {
            var backupDirectory = ConfigBackup.GetBackupDirectory(Service.PluginInterface);
            if (backupDirectory is null)
            {
                message = "無法取得備份資料夾位置。";
                return false;
            }

            if (!backupDirectory.Exists)
            {
                backupDirectory.Create();
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = backupDirectory.FullName,
                UseShellExecute = true,
            });

            message = "已開啟備份資料夾。";
            return true;
        }
        catch (Exception exception)
        {
            message = "無法開啟備份資料夾。";
            Service.Logger.Error(exception, message);
            return false;
        }
    }

    private enum ConfirmationAction
    {
        None,
        Import,
        Reset,
        RestoreLatestBackup,
    }
}
