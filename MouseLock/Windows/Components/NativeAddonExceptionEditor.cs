using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using MouseLock.Configuration;
using MouseLock.Game;

namespace MouseLock.Windows.Components;

internal sealed class NativeAddonExceptionEditor(Action save)
{
    public void Draw(MouseLookConditionSettings conditions)
    {
        var hasFocusedAddon = NativeUiState.TryGetFocusedBlockingAddonName(out var focusedAddonName);
        var currentAddonName = hasFocusedAddon
            ? focusedAddonName
            : string.Empty;
        var canUseExceptions = conditions.DisableWhenNativeAddonFocused || conditions.DisableWhenNativeAddonHovered;

        ConfigWindow.DrawSection("遊戲原生視窗例外");
        using (ImRaii.Disabled(!canUseExceptions))
        {
            ImGui.TextDisabled("允許指定的遊戲視窗在開啟時維持 MouseLock。");
            ImGui.TextUnformatted($"目前的遊戲視窗：{ConfigWindow.DisplayAddonName(currentAddonName)}");

            if (!string.IsNullOrEmpty(currentAddonName))
            {
                ImGui.SameLine();
                if (ImGui.SmallButton("允許此視窗"))
                {
                    AddAllowedAddonName(conditions, currentAddonName);
                }
            }

            DrawAllowedAddonNameList(conditions);
        }

        if (!canUseExceptions)
        {
            ImGui.TextDisabled("請先啟用上方的遊戲原生視窗暫停選項，才能使用例外清單。");
        }
    }

    private void AddAllowedAddonName(MouseLookConditionSettings conditions, string addonName)
    {
        var changed = AddAddonName(conditions.IgnoredFocusedAddonNames, addonName);
        changed |= AddAddonName(conditions.IgnoredHoveredAddonNames, addonName);

        if (changed)
        {
            save();
        }
    }

    private static bool AddAddonName(List<string> addonNames, string addonName)
    {
        if (addonNames.Any(existing => string.Equals(existing, addonName, System.StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        addonNames.Add(addonName);
        addonNames.Sort(System.StringComparer.OrdinalIgnoreCase);
        return true;
    }

    private void DrawAllowedAddonNameList(MouseLookConditionSettings conditions)
    {
        var addonNames = conditions.IgnoredFocusedAddonNames
            .Concat(conditions.IgnoredHoveredAddonNames)
            .Distinct(System.StringComparer.OrdinalIgnoreCase)
            .OrderBy(addonName => addonName, System.StringComparer.OrdinalIgnoreCase)
            .ToList();

        ImGui.TextUnformatted("允許的視窗");
        if (addonNames.Count == 0)
        {
            ImGui.TextDisabled("無");
            return;
        }

        foreach (var addonName in addonNames)
        {
            ImGui.BulletText(addonName);
            ImGui.SameLine();

            if (ImGui.SmallButton($"移除##AllowedNativeAddon{addonName}"))
            {
                RemoveAllowedAddonName(conditions, addonName);
                return;
            }
        }
    }

    private void RemoveAllowedAddonName(MouseLookConditionSettings conditions, string addonName)
    {
        conditions.IgnoredFocusedAddonNames.RemoveAll(existing => string.Equals(existing, addonName, System.StringComparison.OrdinalIgnoreCase));
        conditions.IgnoredHoveredAddonNames.RemoveAll(existing => string.Equals(existing, addonName, System.StringComparison.OrdinalIgnoreCase));
        save();
    }
}
