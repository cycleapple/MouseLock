using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using MouseLock.Configuration;
using MouseLock.Configuration.Persistence;
using MouseLock.MouseLook;

namespace MouseLock.Windows;

internal sealed class FirstRunWindow : Window
{
    private readonly SystemConfiguration _config;

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

    public FirstRunWindow(SystemConfiguration config)
        : base("MouseLock 快速入門###MouseLockFirstRun")
    {
        _config = config;
        IsOpen = !_config.General.FirstRunIntroCompleted;
        ShowCloseButton = true;
        Size = new Vector2(500.0f, 0.0f);
    }

    public override void Draw()
    {
        ImGui.TextWrapped("MouseLock 會維持滑鼠視角，直到有操作需要使用游標。");
        ImGui.Spacing();

        ImGui.TextWrapped("重要事項：");
        ImGui.BulletText("視角啟用時：觀看方向會跟隨滑鼠移動。");
        ImGui.BulletText("游標位置：鎖定期間會移回畫面中央。");
        ImGui.BulletText("視窗位置：請避免將插件視窗直接放在畫面中央。");

        DrawQuickSettings();

        ImGui.Spacing();
        ImGui.TextWrapped(GetReleaseModifierHelpText());

        ImGui.Spacing();
        if (ImGui.Button("知道了"))
        {
            CompleteIntro();
        }

        ImGui.SameLine();
        if (ImGui.Button("開啟完整設定"))
        {
            PluginState.ConfigWindow.IsOpen = true;
            CompleteIntro();
        }
    }

    public override void OnClose()
    {
        CompleteIntro();
    }

    private void DrawQuickSettings()
    {
        var enabled = _config.General.Enabled;
        if (ImGui.Checkbox("啟用 MouseLock", ref enabled))
        {
            MouseLockStateController.SetEnabled(enabled);
        }

        var releaseModifierIndex = FindOptionIndex(ReleaseModifierOptions, _config.General.ReleaseModifier);
        if (ImGui.Combo("暫時釋放游標的修飾鍵", ref releaseModifierIndex, ReleaseModifierLabels, ReleaseModifierLabels.Length))
        {
            _config.General.ReleaseModifier = ReleaseModifierOptions[releaseModifierIndex].Value;
            Save();
        }

        var releaseModifierTapBehaviorIndex = FindOptionIndex(
            ReleaseModifierTapBehaviorOptions,
            _config.General.ReleaseModifierTapBehavior);
        if (ImGui.Combo(
                "輕按修飾鍵時",
                ref releaseModifierTapBehaviorIndex,
                ReleaseModifierTapBehaviorLabels,
                ReleaseModifierTapBehaviorLabels.Length))
        {
            _config.General.ReleaseModifierTapBehavior =
                ReleaseModifierTapBehaviorOptions[releaseModifierTapBehaviorIndex].Value;
            Save();
        }

        var conditions = _config.Activation.Conditions;

        var disableWhileTextInputActive = conditions.DisableWhileTextInputActive;
        if (ImGui.Checkbox("聊天／文字輸入期間暫停", ref disableWhileTextInputActive))
        {
            conditions.DisableWhileTextInputActive = disableWhileTextInputActive;
            Save();
        }

        var disableWhenDalamudWindowFocused = conditions.DisableWhenDalamudWindowFocused;
        if (ImGui.Checkbox("焦點位於 Dalamud／ImGui 視窗時暫停", ref disableWhenDalamudWindowFocused))
        {
            conditions.DisableWhenDalamudWindowFocused = disableWhenDalamudWindowFocused;
            Save();
        }

        var disableWhenNativeAddonFocused = conditions.DisableWhenNativeAddonFocused;
        if (ImGui.Checkbox("焦點位於遊戲原生視窗時暫停", ref disableWhenNativeAddonFocused))
        {
            conditions.DisableWhenNativeAddonFocused = disableWhenNativeAddonFocused;
            Save();
        }
    }

    private string GetReleaseModifierHelpText()
    {
        if (_config.General.ReleaseModifier == ReleaseModifierKey.None)
        {
            return "尚未設定暫時釋放游標的修飾鍵。稍後可在「一般」設定中新增。";
        }

        var modifier = GetReleaseModifierLabel(_config.General.ReleaseModifier);
        return _config.General.ReleaseModifierTapBehavior switch
        {
            ReleaseModifierTapBehavior.UntilWorldClick =>
                $"按住 {modifier} 可暫時釋放游標。輕按 {modifier} 後，游標會保持自由，直到再次輕按 {modifier}，或在遊戲世界中按下滑鼠左鍵／右鍵。",
            ReleaseModifierTapBehavior.UntilNextTap =>
                $"按住 {modifier} 可暫時釋放游標。輕按 {modifier} 可切換游標釋放狀態；只有再次輕按 {modifier} 才會讓 MouseLock 重新鎖定。",
            _ => $"按住 {modifier} 可暫時釋放游標。",
        };
    }

    private void CompleteIntro()
    {
        if (_config.General.FirstRunIntroCompleted)
        {
            IsOpen = false;
            return;
        }

        _config.General.FirstRunIntroCompleted = true;
        IsOpen = false;
        Save();
    }

    private static string GetReleaseModifierLabel(ReleaseModifierKey modifier)
        => modifier switch
        {
            ReleaseModifierKey.Alt => "Alt",
            ReleaseModifierKey.Control => "Ctrl",
            ReleaseModifierKey.Shift => "Shift",
            _ => "無",
        };

    private void Save()
    {
        ConfigRepository.Save(_config);
        PluginState.MouseLookService?.RefreshCurrentStatus();
        PluginState.DtrStatusService?.Refresh();
    }

    private readonly record struct ComboOption<T>(T Value, string Label);

    private static string[] CreateLabels<T>(IReadOnlyList<ComboOption<T>> options)
    {
        var labels = new string[options.Count];
        for (var index = 0; index < options.Count; index++)
        {
            labels[index] = options[index].Label;
        }

        return labels;
    }

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
