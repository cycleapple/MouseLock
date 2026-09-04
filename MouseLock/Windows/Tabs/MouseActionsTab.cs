using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using MouseLock.Configuration;
using MouseLock.Windows.Components;

namespace MouseLock.Windows.Tabs;

internal sealed class MouseActionsTab(
    SystemConfiguration config,
    Action save,
    HotbarSlotPicker hotbarSlotPicker)
{
    private const float LayerColumnWidthInFrames = 5.0f;
    private const float ComboWidthInFrames = 11.0f;

    private static readonly ComboOption<MouseButtonBindingKind>[] BindingKindOptions =
    [
        new(MouseButtonBindingKind.None, "無"),
        new(MouseButtonBindingKind.GameInput, "遊戲輸入"),
        new(MouseButtonBindingKind.HotbarSlot, "快捷列欄位"),
        new(MouseButtonBindingKind.TemporaryRelease, "暫時釋放游標"),
        new(MouseButtonBindingKind.ToggleMouseLock, "切換 MouseLock"),
        new(MouseButtonBindingKind.OpenConfig, "開啟設定"),
    ];

    private static readonly string[] BindingKindLabels = CreateLabels(BindingKindOptions);

    private static readonly ComboOption<CuratedGameInput>[] GameInputOptions =
    [
        new(CuratedGameInput.TabTargetNext, "切換至下一個目標"),
        new(CuratedGameInput.TabTargetPrevious, "切換至上一個目標"),
        new(CuratedGameInput.TargetNearestEnemy, "選取最近的敵人"),
        new(CuratedGameInput.MoveForward, "向前移動"),
        new(CuratedGameInput.MoveBack, "向後移動"),
        new(CuratedGameInput.MoveLeft, "向左移動"),
        new(CuratedGameInput.MoveRight, "向右移動"),
        new(CuratedGameInput.StrafeLeft, "向左平移"),
        new(CuratedGameInput.StrafeRight, "向右平移"),
        new(CuratedGameInput.Jump, "跳躍"),
        new(CuratedGameInput.Autorun, "自動奔跑"),
    ];

    private static readonly string[] GameInputLabels = CreateLabels(GameInputOptions);

    public void Draw()
    {
        using var tab = ImRaii.TabItem("滑鼠動作");
        if (!tab)
        {
            return;
        }

        var actions = config.MouseActions;

        ConfigWindow.DrawSection("原生滑鼠移動");

        var classicMouseMovementEnabled = actions.ClassicMouseMovementEnabled;
        if (ImGui.Checkbox("同時按下滑鼠左鍵與右鍵以向前移動", ref classicMouseMovementEnabled))
        {
            actions.ClassicMouseMovementEnabled = classicMouseMovementEnabled;
            save();
        }
        ConfigWindow.DrawTooltip(
            "在 MouseLock 啟用時恢復 FFXIV 原本的雙鍵滑鼠移動。\n" +
            "此功能與標準／傳統移動模式設定無關。\n" +
            "啟用後，所有設定的滑鼠左鍵／右鍵動作都會停用。");

        if (actions.ClassicMouseMovementEnabled)
        {
            ImGui.TextDisabled("啟用原生滑鼠左右鍵移動時，滑鼠動作將停用。");
        }

        ConfigWindow.DrawDisabled(actions.ClassicMouseMovementEnabled, () =>
        {
            DrawMouseButtonActionGroup(
                "滑鼠左鍵",
                "LMB",
                actions.LeftButton,
                actions.LeftAltButton,
                actions.LeftControlButton,
                actions.LeftShiftButton);

            DrawMouseButtonActionGroup(
                "滑鼠右鍵",
                "RMB",
                actions.RightButton,
                actions.RightAltButton,
                actions.RightControlButton,
                actions.RightShiftButton);
        });
    }

    private void DrawMouseButtonActionGroup(
        string label,
        string id,
        MouseButtonGameInputBinding defaultBinding,
        MouseButtonGameInputBinding altBinding,
        MouseButtonGameInputBinding controlBinding,
        MouseButtonGameInputBinding shiftBinding)
    {
        ConfigWindow.DrawSection(label);

        using var table = ImRaii.Table(
            $"##{id}ActionLayers",
            2,
            ImGuiTableFlags.NoPadOuterX | ImGuiTableFlags.SizingStretchProp);
        if (!table)
        {
            return;
        }

        ImGui.TableSetupColumn("##Modifier", ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight() * LayerColumnWidthInFrames);
        ImGui.TableSetupColumn("##Binding", ImGuiTableColumnFlags.WidthStretch);

        DrawMouseActionLayerRow($"{id}Default", "預設", ReleaseModifierKey.None, defaultBinding);
        DrawMouseActionLayerRow($"{id}Alt", "Alt", ReleaseModifierKey.Alt, altBinding);
        DrawMouseActionLayerRow($"{id}Control", "Ctrl", ReleaseModifierKey.Control, controlBinding);
        DrawMouseActionLayerRow($"{id}Shift", "Shift", ReleaseModifierKey.Shift, shiftBinding);
    }

    private void DrawMouseActionLayerRow(
        string id,
        string layerLabel,
        ReleaseModifierKey layerModifier,
        MouseButtonGameInputBinding binding)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        if (layerModifier != ReleaseModifierKey.None && binding.Kind == MouseButtonBindingKind.None)
        {
            ImGui.TextDisabled(layerLabel);
        }
        else
        {
            ImGui.TextUnformatted(layerLabel);
        }

        ImGui.TableNextColumn();
        DrawMouseActionBinding(id, binding);
        DrawMouseActionLayerConflictWarning(layerModifier, binding);
    }

    private void DrawMouseActionBinding(string label, MouseButtonGameInputBinding binding)
    {
        using var id = ImRaii.PushId(label);

        var kindIndex = FindOptionIndex(BindingKindOptions, binding.Kind);
        ImGui.SetNextItemWidth(GetControlWidth());
        if (ImGui.Combo("##Binding", ref kindIndex, BindingKindLabels, BindingKindLabels.Length))
        {
            binding.Kind = BindingKindOptions[kindIndex].Value;
            binding.Clamp();
            save();
        }

        switch (binding.Kind)
        {
            case MouseButtonBindingKind.GameInput:
                DrawGameInputBinding(binding);
                break;
            case MouseButtonBindingKind.HotbarSlot:
                hotbarSlotPicker.Draw(binding);
                break;
        }
    }

    private void DrawGameInputBinding(MouseButtonGameInputBinding binding)
    {
        ConfigWindow.DrawNestIndicator(1);
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted("遊戲輸入");
        ImGui.SameLine();

        var gameInputIndex = FindOptionIndex(GameInputOptions, binding.GameInput);
        ImGui.SetNextItemWidth(GetControlWidth());
        if (ImGui.Combo("##GameInput", ref gameInputIndex, GameInputLabels, GameInputLabels.Length))
        {
            binding.GameInput = GameInputOptions[gameInputIndex].Value;
            save();
        }
    }

    private void DrawMouseActionLayerConflictWarning(ReleaseModifierKey layerModifier, MouseButtonGameInputBinding binding)
    {
        if (config.MouseActions.ClassicMouseMovementEnabled ||
            binding.Kind == MouseButtonBindingKind.None ||
            config.General.ReleaseModifier != layerModifier)
        {
            return;
        }

        ConfigWindow.DrawNestIndicator(1);
        using var warningColor = ImRaii.PushColor(ImGuiCol.Text, GetWarningTextColor());
        ImGui.TextWrapped($"{GetReleaseModifierLabel(layerModifier)} 同時也是暫時釋放游標的修飾鍵，因此此層級會先暫停 MouseLock，無法執行動作。");
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

    private static string GetReleaseModifierLabel(ReleaseModifierKey modifier)
        => modifier switch
        {
            ReleaseModifierKey.Alt => "Alt",
            ReleaseModifierKey.Control => "Ctrl",
            ReleaseModifierKey.Shift => "Shift",
            _ => "無",
        };

    private static float GetControlWidth()
        => MathF.Min(ImGui.GetContentRegionAvail().X, ImGui.GetFrameHeight() * ComboWidthInFrames);

    private static Vector4 GetWarningTextColor()
        => ImGuiColors.DalamudOrange;
}
