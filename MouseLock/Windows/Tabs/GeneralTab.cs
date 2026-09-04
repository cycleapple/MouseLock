using System;
using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using MouseLock.Configuration;
using MouseLock.MouseLook;
using MouseLock.Windows.Components;

namespace MouseLock.Windows.Tabs;

internal sealed class GeneralTab(
    SystemConfiguration config,
    Action save,
    ToggleKeybindEditor toggleKeybindEditor,
    DtrSettingsEditor dtrSettingsEditor,
    ConfigurationTransferPanel configurationTransferPanel)
{
    private static readonly ComboOption<ReleaseModifierKey>[] ReleaseModifierOptions =
    [
        new(ReleaseModifierKey.Alt, "Alt"),
        new(ReleaseModifierKey.Control, "Ctrl"),
        new(ReleaseModifierKey.Shift, "Shift"),
        new(ReleaseModifierKey.None, "無"),
    ];

    private static readonly string[] ReleaseModifierLabels = CreateLabels(ReleaseModifierOptions);

    private static readonly ComboOption<ReleaseModifierTapBehavior>[] ReleaseModifierTapBehaviorOptions =
    [
        new(ReleaseModifierTapBehavior.None, "不執行任何動作"),
        new(ReleaseModifierTapBehavior.UntilWorldClick, "釋放至點擊遊戲世界或再次輕按"),
        new(ReleaseModifierTapBehavior.UntilNextTap, "釋放至再次輕按"),
    ];

    private static readonly string[] ReleaseModifierTapBehaviorLabels = CreateLabels(ReleaseModifierTapBehaviorOptions);

    public void Draw()
    {
        using var tab = ImRaii.TabItem("一般");
        if (!tab)
        {
            return;
        }

        var enabled = config.General.Enabled;
        if (ImGui.Checkbox("啟用", ref enabled))
        {
            MouseLockStateController.SetEnabled(enabled);
        }

        var releaseModifierIndex = FindOptionIndex(ReleaseModifierOptions, config.General.ReleaseModifier);
        if (ImGui.Combo("暫時釋放游標的修飾鍵", ref releaseModifierIndex, ReleaseModifierLabels, ReleaseModifierLabels.Length))
        {
            config.General.ReleaseModifier = ReleaseModifierOptions[releaseModifierIndex].Value;
            save();
        }

        var restoreCursorPositionOnRelease = config.General.RestoreCursorPositionOnRelease;
        if (ImGui.Checkbox("釋放時將游標移回先前位置", ref restoreCursorPositionOnRelease))
        {
            config.General.RestoreCursorPositionOnRelease = restoreCursorPositionOnRelease;
            save();
        }
        ConfigWindow.DrawTooltip("停用後，MouseLock 暫停或釋放游標時，游標會留在畫面中央。");

        var releaseModifierTapBehaviorIndex = FindOptionIndex(
            ReleaseModifierTapBehaviorOptions,
            config.General.ReleaseModifierTapBehavior);
        if (ImGui.Combo(
                "輕按修飾鍵時",
                ref releaseModifierTapBehaviorIndex,
                ReleaseModifierTapBehaviorLabels,
                ReleaseModifierTapBehaviorLabels.Length))
        {
            config.General.ReleaseModifierTapBehavior = ReleaseModifierTapBehaviorOptions[releaseModifierTapBehaviorIndex].Value;
            save();
        }
        ConfigWindow.DrawTooltip(GetReleaseModifierTapBehaviorTooltip());

        ConfigWindow.DrawSection("切換快捷鍵");
        toggleKeybindEditor.Draw(config.ToggleKeybind);

        ConfigWindow.DrawSection("伺服器資訊列");
        dtrSettingsEditor.Draw(config.Dtr);

        ConfigWindow.DrawSection("設定管理");
        configurationTransferPanel.Draw();
    }

    private readonly record struct ComboOption<T>(T Value, string Label);

    private string GetReleaseModifierTapBehaviorTooltip()
    {
        if (config.General.ReleaseModifier == ReleaseModifierKey.None)
        {
            return "尚未設定暫時釋放游標的修飾鍵。請選擇 Alt、Ctrl 或 Shift，才能使用輕按釋放功能。";
        }

        var modifier = GetReleaseModifierLabel(config.General.ReleaseModifier);
        return config.General.ReleaseModifierTapBehavior switch
        {
            ReleaseModifierTapBehavior.UntilWorldClick =>
                $"按住 {modifier} 可暫時釋放游標。\n\n" +
                $"目前模式：釋放至點擊遊戲世界或再次輕按。輕按 {modifier} 後，游標會保持自由，直到再次輕按 {modifier}，或在遊戲世界中按下滑鼠左鍵／右鍵。\n\n" +
                "點擊遊戲原生介面或 Dalamud 介面不會重新鎖定游標。",
            ReleaseModifierTapBehavior.UntilNextTap =>
                $"按住 {modifier} 可暫時釋放游標。\n\n" +
                $"目前模式：釋放至再次輕按。輕按 {modifier} 可切換游標釋放狀態；只有再次輕按 {modifier} 才會讓 MouseLock 重新鎖定。\n\n" +
                "點擊遊戲世界及原生的滑鼠左右鍵移動都不會重新鎖定游標。",
            _ =>
                $"按住 {modifier} 可暫時釋放游標。\n\n" +
                $"目前模式：不執行任何動作。輕按 {modifier} 不會持續釋放游標。",
        };
    }

    private static string[] CreateLabels<T>(IReadOnlyList<ComboOption<T>> options)
    {
        var labels = new string[options.Count];
        for (var index = 0; index < options.Count; index++)
        {
            labels[index] = options[index].Label;
        }

        return labels;
    }

    private static string GetReleaseModifierLabel(ReleaseModifierKey modifier)
        => modifier switch
        {
            ReleaseModifierKey.Alt => "Alt",
            ReleaseModifierKey.Control => "Ctrl",
            ReleaseModifierKey.Shift => "Shift",
            _ => "無",
        };

    private static int FindOptionIndex<T>(IReadOnlyList<ComboOption<T>> options, T value)
    {
        for (var index = 0; index < options.Count; index++)
        {
            if (EqualityComparer<T>.Default.Equals(options[index].Value, value))
            {
                return index;
            }
        }

        return 0;
    }
}
