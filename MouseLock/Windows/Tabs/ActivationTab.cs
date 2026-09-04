using System;
using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using MouseLock.Configuration;
using MouseLock.Windows.Components;

namespace MouseLock.Windows.Tabs;

internal sealed class ActivationTab(
    SystemConfiguration config,
    Action save,
    NativeAddonExceptionEditor nativeAddonExceptionEditor,
    DalamudWindowExceptionEditor dalamudWindowExceptionEditor)
{
    private static readonly ComboOption<MouseLookResumePolicy>[] ResumePolicyOptions =
    [
        new(MouseLookResumePolicy.Immediate, "立即恢復"),
        new(MouseLookResumePolicy.Delay, "短暫延遲後恢復"),
        new(MouseLookResumePolicy.WorldClick, "下次點擊遊戲世界時恢復"),
    ];

    private static readonly string[] ResumePolicyLabels = CreateLabels(ResumePolicyOptions);

    public void Draw()
    {
        using var tab = ImRaii.TabItem("啟用條件");
        if (!tab)
        {
            return;
        }

        var activation = config.Activation;
        var conditions = activation.Conditions;

        var disableWhileTextInputActive = conditions.DisableWhileTextInputActive;
        if (ImGui.Checkbox("聊天／文字輸入期間暫停", ref disableWhileTextInputActive))
        {
            conditions.DisableWhileTextInputActive = disableWhileTextInputActive;
            save();
        }

        var disableWhenTalkAddonVisible = conditions.DisableWhenTalkAddonVisible;
        if (ImGui.Checkbox("與 NPC 對話時暫停", ref disableWhenTalkAddonVisible))
        {
            conditions.DisableWhenTalkAddonVisible = disableWhenTalkAddonVisible;
            save();
        }

        var disableWhenDalamudWindowFocused = conditions.DisableWhenDalamudWindowFocused;
        if (ImGui.Checkbox("焦點位於 Dalamud／ImGui 視窗時暫停", ref disableWhenDalamudWindowFocused))
        {
            conditions.DisableWhenDalamudWindowFocused = disableWhenDalamudWindowFocused;
            save();
        }

        var disableWhenNativeAddonFocused = conditions.DisableWhenNativeAddonFocused;
        if (ImGui.Checkbox("焦點位於遊戲原生視窗時暫停", ref disableWhenNativeAddonFocused))
        {
            conditions.DisableWhenNativeAddonFocused = disableWhenNativeAddonFocused;
            save();
        }

        var disableWhenNativeAddonHovered = conditions.DisableWhenNativeAddonHovered;
        if (ConfigWindow.DrawNestedCheckbox("游標停在遊戲原生視窗上時也暫停", ref disableWhenNativeAddonHovered))
        {
            conditions.DisableWhenNativeAddonHovered = disableWhenNativeAddonHovered;
            save();
        }
        ConfigWindow.DrawTooltip("不建議啟用；若視窗位於畫面正中央，可能造成異常行為。");

        var requireCombat = conditions.RequireCombat;
        if (ImGui.Checkbox("僅在戰鬥中鎖定", ref requireCombat))
        {
            conditions.RequireCombat = requireCombat;
            save();
        }

        ConfigWindow.DrawDisabled(!conditions.RequireCombat, () =>
        {
            var countCountdownAsCombat = conditions.CountCountdownAsCombat;
            if (ConfigWindow.DrawNestedCheckbox("將戰鬥倒數視為戰鬥中", ref countCountdownAsCombat))
            {
                conditions.CountCountdownAsCombat = countCountdownAsCombat;
                save();
            }
        });

        ConfigWindow.DrawSection("自動暫停後恢復");
        var resumePolicyIndex = FindOptionIndex(ResumePolicyOptions, activation.ResumePolicy);
        if (ImGui.Combo("聊天、介面或視窗焦點造成暫停後", ref resumePolicyIndex, ResumePolicyLabels, ResumePolicyLabels.Length))
        {
            activation.ResumePolicy = ResumePolicyOptions[resumePolicyIndex].Value;
            save();
        }

        ConfigWindow.DrawDisabled(activation.ResumePolicy != MouseLookResumePolicy.Delay, () =>
        {
            ConfigWindow.DrawNestIndicator(1);
            var resumeDelay = activation.ResumeDelayMilliseconds;
            ImGui.SetNextItemWidth(ImGui.GetFrameHeight() * 4.0f);
            if (ImGui.InputInt("延遲（毫秒）", ref resumeDelay, 0, 0))
            {
                activation.ResumeDelayMilliseconds = Math.Clamp(resumeDelay, 100, 2_000);
                save();
            }
        });

        if (activation.ResumePolicy == MouseLookResumePolicy.WorldClick)
        {
            ImGui.TextWrapped(
                "自動暫停結束後，MouseLock 會等待你在遊戲世界中按下滑鼠左鍵或右鍵。" +
                "點擊遊戲原生介面或 Dalamud 介面不會恢復鎖定。" +
                "此設定不影響釋放修飾鍵或滑鼠按鍵動作。");
        }

        ConfigWindow.DrawSection("遊戲狀態暫停條件");
        DrawGameStatePauseSettings(conditions);

        dalamudWindowExceptionEditor.Draw(conditions);
        nativeAddonExceptionEditor.Draw(conditions);
    }

    private void DrawGameStatePauseSettings(MouseLookConditionSettings conditions)
    {
        var disableDuringCutscenes = conditions.DisableDuringCutscenes;
        if (ImGui.Checkbox("過場動畫期間暫停", ref disableDuringCutscenes))
        {
            conditions.DisableDuringCutscenes = disableDuringCutscenes;
            save();
        }

        var disableDuringGpose = conditions.DisableDuringGpose;
        if (ImGui.Checkbox("團體姿勢期間暫停", ref disableDuringGpose))
        {
            conditions.DisableDuringGpose = disableDuringGpose;
            save();
        }

        var disableDuringTerritoryTransitions = conditions.DisableDuringTerritoryTransitions;
        if (ImGui.Checkbox("切換區域期間暫停", ref disableDuringTerritoryTransitions))
        {
            conditions.DisableDuringTerritoryTransitions = disableDuringTerritoryTransitions;
            save();
        }

        var disableDuringCrafting = conditions.DisableDuringCrafting;
        if (ImGui.Checkbox("製作期間暫停", ref disableDuringCrafting))
        {
            conditions.DisableDuringCrafting = disableDuringCrafting;
            save();
        }

        var disableDuringGathering = conditions.DisableDuringGathering;
        if (ImGui.Checkbox("採集期間暫停", ref disableDuringGathering))
        {
            conditions.DisableDuringGathering = disableDuringGathering;
            save();
        }

        var disableDuringGroundTargeting = conditions.DisableDuringGroundTargeting;
        if (ImGui.Checkbox("選擇地面目標期間暫停", ref disableDuringGroundTargeting))
        {
            conditions.DisableDuringGroundTargeting = disableDuringGroundTargeting;
            save();
        }

        var disableDuringHousingPlacement = conditions.DisableDuringHousingPlacement;
        if (ImGui.Checkbox("擺設房屋家具時暫停", ref disableDuringHousingPlacement))
        {
            conditions.DisableDuringHousingPlacement = disableDuringHousingPlacement;
            save();
        }

        var disableWhileMounted = conditions.DisableWhileMounted;
        if (ImGui.Checkbox("騎乘坐騎時暫停", ref disableWhileMounted))
        {
            conditions.DisableWhileMounted = disableWhileMounted;
            save();
        }

        var disableDuringGamepadMouseMode = conditions.DisableDuringGamepadMouseMode;
        if (ImGui.Checkbox("控制器滑鼠模式期間暫停", ref disableDuringGamepadMouseMode))
        {
            conditions.DisableDuringGamepadMouseMode = disableDuringGamepadMouseMode;
            save();
        }
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
