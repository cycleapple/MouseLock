using System;
using System.Linq;
using MouseLock.MouseLook.Activation;

namespace MouseLock.MouseLook;

internal static class MouseLookStatusFormatter
{
    public static string GetSummary(MouseLookStatus status)
        => status.Kind switch
        {
            MouseLookStatusKind.Off => "關閉",
            MouseLookStatusKind.Ready => "開啟",
            MouseLookStatusKind.Active => "作用中",
            MouseLookStatusKind.Paused => $"已暫停：{GetReasonLabel(status.Reason)}",
            MouseLookStatusKind.Unavailable => $"無法使用：{GetReasonLabel(status.Reason)}",
            _ => status.Kind.ToString(),
        };

    public static string GetDetail(MouseLookStatus status)
        => status.Kind switch
        {
            MouseLookStatusKind.Off => "MouseLock 已停用。",
            MouseLookStatusKind.Ready => "MouseLock 已啟用並準備就緒。",
            MouseLookStatusKind.Active => "MouseLock 目前正在控制鏡頭輸入。",
            MouseLookStatusKind.Paused => $"MouseLock 已啟用但目前暫停：{GetReasonLabel(status.Reason)}。",
            MouseLookStatusKind.Unavailable => $"MouseLock 無法執行：{GetReasonLabel(status.Reason)}。",
            _ => GetSummary(status),
        };

    public static string GetReasonLabel(MouseLookPauseReason reason)
        => reason switch
        {
            MouseLookPauseReason.None => "無",
            MouseLookPauseReason.PluginDisabled => "插件已停用",
            MouseLookPauseReason.LoggedOut => "尚未登入",
            MouseLookPauseReason.ConfigWindowOpen => "設定視窗已開啟",
            MouseLookPauseReason.FirstRunWindowOpen => "快速入門視窗已開啟",
            MouseLookPauseReason.GameUnfocused => "遊戲不在前景",
            MouseLookPauseReason.CombatRequired => "非戰鬥狀態",
            MouseLookPauseReason.Cutscene => "過場動畫",
            MouseLookPauseReason.Gpose => "團體姿勢",
            MouseLookPauseReason.Crafting => "製作中",
            MouseLookPauseReason.Gathering => "採集中",
            MouseLookPauseReason.GroundTargeting => "正在選擇地面目標",
            MouseLookPauseReason.HousingPlacement => "正在擺設房屋家具",
            MouseLookPauseReason.Mounted => "騎乘坐騎中",
            MouseLookPauseReason.TerritoryTransition => "正在切換區域",
            MouseLookPauseReason.GamepadMouseMode => "控制器滑鼠模式",
            MouseLookPauseReason.TextInput => "正在輸入文字",
            MouseLookPauseReason.TalkAddon => "NPC 對話",
            MouseLookPauseReason.DalamudWindowFocused => "焦點位於 Dalamud 視窗",
            MouseLookPauseReason.NativeAddonFocused => "焦點位於遊戲視窗",
            MouseLookPauseReason.NativeAddonHovered => "游標停在遊戲視窗上",
            MouseLookPauseReason.TPie => "TPie",
            MouseLookPauseReason.ExternalSuspension => "外部要求暫停",
            MouseLookPauseReason.ReleaseModifier => "按住釋放修飾鍵",
            MouseLookPauseReason.StickyRelease => "游標持續釋放",
            MouseLookPauseReason.ToggleRelease => "已切換游標釋放",
            MouseLookPauseReason.MouseActionRelease => "滑鼠動作要求釋放",
            MouseLookPauseReason.ResumeDelay => "等待恢復延遲",
            MouseLookPauseReason.WaitingForWorldClick => "等待點擊遊戲世界",
            MouseLookPauseReason.InputUnavailable => "無法使用輸入",
            MouseLookPauseReason.HookUnavailable => "無法使用 Hook",
            MouseLookPauseReason.NativeSubsystemUnavailable => "無法使用原生輸入",
            _ => reason.ToString(),
        };

    public static string GetInvariantReasonLabel(MouseLookPauseReason reason)
        => reason switch
        {
            MouseLookPauseReason.None => "None",
            MouseLookPauseReason.PluginDisabled => "Disabled",
            MouseLookPauseReason.LoggedOut => "Logged out",
            MouseLookPauseReason.ConfigWindowOpen => "Config open",
            MouseLookPauseReason.FirstRunWindowOpen => "Quick start open",
            MouseLookPauseReason.GameUnfocused => "Game unfocused",
            MouseLookPauseReason.CombatRequired => "Out of combat",
            MouseLookPauseReason.Cutscene => "Cutscene",
            MouseLookPauseReason.Gpose => "GPose",
            MouseLookPauseReason.Crafting => "Crafting",
            MouseLookPauseReason.Gathering => "Gathering",
            MouseLookPauseReason.GroundTargeting => "Ground targeting",
            MouseLookPauseReason.HousingPlacement => "Housing placement",
            MouseLookPauseReason.Mounted => "Mounted",
            MouseLookPauseReason.TerritoryTransition => "Territory transition",
            MouseLookPauseReason.GamepadMouseMode => "Gamepad mouse mode",
            MouseLookPauseReason.TextInput => "Text input",
            MouseLookPauseReason.TalkAddon => "NPC dialogue",
            MouseLookPauseReason.DalamudWindowFocused => "Dalamud window focused",
            MouseLookPauseReason.NativeAddonFocused => "Game window focused",
            MouseLookPauseReason.NativeAddonHovered => "Hovering game window",
            MouseLookPauseReason.TPie => "TPie",
            MouseLookPauseReason.ExternalSuspension => "External suspension",
            MouseLookPauseReason.ReleaseModifier => "Release modifier",
            MouseLookPauseReason.StickyRelease => "Sticky release",
            MouseLookPauseReason.ToggleRelease => "Cursor release toggled",
            MouseLookPauseReason.MouseActionRelease => "Mouse action release",
            MouseLookPauseReason.ResumeDelay => "Resume delay",
            MouseLookPauseReason.WaitingForWorldClick => "Waiting for world click",
            MouseLookPauseReason.InputUnavailable => "Input unavailable",
            MouseLookPauseReason.HookUnavailable => "Hook unavailable",
            MouseLookPauseReason.NativeSubsystemUnavailable => "Native input unavailable",
            _ => reason.ToString(),
        };

    public static string GetSuspensionSourceLabel(string source)
        => source switch
        {
            var value when value.Equals("Command", StringComparison.OrdinalIgnoreCase) => "指令",
            var value when value.Equals("External", StringComparison.OrdinalIgnoreCase) => "外部",
            _ => source,
        };

    public static string GetSuspensionSourcesSummary()
        => string.Join(", ", SuspensionRegistry.SourcesSummary
            .Split(", ", StringSplitOptions.RemoveEmptyEntries)
            .Select(GetSuspensionSourceLabel));
}
