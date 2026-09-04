using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using MouseLock.Configuration;
using MouseLock.Game;

namespace MouseLock.Windows.Components;

internal sealed class DalamudWindowExceptionEditor(Action save)
{
    public void Draw(MouseLookConditionSettings conditions)
    {
        var focus = DalamudUiState.LastExternalFocus;
        var suggestedWindowPattern = GetSuggestedWindowPattern(focus.WindowName);
        var canUseExceptions = conditions.DisableWhenDalamudWindowFocused;

        ConfigWindow.DrawSection("Dalamud 視窗例外");
        using (ImRaii.Disabled(!canUseExceptions))
        {
            ImGui.TextDisabled("允許指定的 Dalamud／ImGui 視窗或子視窗前綴在開啟時維持 MouseLock。");
            ImGui.TextWrapped($"最近的外部視窗系統：{ConfigWindow.DisplayAddonName(focus.WindowSystemNamespace)}");
            ImGui.TextWrapped($"最近的外部視窗：{ConfigWindow.DisplayAddonName(focus.WindowName)}");

            if (!string.IsNullOrEmpty(focus.WindowName))
            {
                if (ImGui.SmallButton("允許此視窗"))
                {
                    AddAllowedWindowName(conditions, focus.WindowName);
                }

                if (!string.IsNullOrEmpty(suggestedWindowPattern))
                {
                    ImGui.SameLine();
                    if (ImGui.SmallButton("允許此視窗群組"))
                    {
                        AddAllowedWindowName(conditions, suggestedWindowPattern);
                    }
                }
            }

            if (!string.IsNullOrEmpty(focus.WindowSystemNamespace))
            {
                ImGui.SameLine();
                if (ImGui.SmallButton("允許整個視窗系統"))
                {
                    AddAllowedWindowSystemNamespace(conditions, focus.WindowSystemNamespace);
                }
            }

            if (focus.IsEmpty)
            {
                ImGui.TextDisabled("請先將焦點移至其他插件視窗，再返回此處將它加入允許清單。");
            }

            ImGui.TextDisabled("以 * 結尾的項目會比對名稱前綴，適合名稱由插件動態產生的子視窗。");
            DrawAllowedWindowNameList(conditions);
            DrawAllowedWindowSystemNamespaceList(conditions);
        }

        if (!canUseExceptions)
        {
            ImGui.TextDisabled("請先啟用上方的 Dalamud／ImGui 視窗暫停選項，才能使用例外清單。");
        }
    }

    private void AddAllowedWindowSystemNamespace(MouseLookConditionSettings conditions, string windowSystemNamespace)
    {
        if (AddName(conditions.IgnoredDalamudWindowSystemNamespaces, windowSystemNamespace))
        {
            save();
        }
    }

    private void AddAllowedWindowName(MouseLookConditionSettings conditions, string windowName)
    {
        if (AddName(conditions.IgnoredDalamudWindowNames, windowName))
        {
            save();
        }
    }

    private static bool AddName(List<string> names, string name)
    {
        var normalizedName = name.Trim();
        if (string.IsNullOrEmpty(normalizedName) ||
            names.Any(existing => string.Equals(existing, normalizedName, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        names.Add(normalizedName);
        names.Sort(StringComparer.OrdinalIgnoreCase);
        return true;
    }

    private void DrawAllowedWindowNameList(MouseLookConditionSettings conditions)
    {
        var windowNames = conditions.IgnoredDalamudWindowNames
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(windowName => windowName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        ImGui.TextUnformatted("允許的 Dalamud 視窗／前綴");
        if (windowNames.Count == 0)
        {
            ImGui.TextDisabled("無");
            return;
        }

        foreach (var windowName in windowNames)
        {
            ImGui.BulletText(windowName);
            ImGui.SameLine();

            using var id = ImRaii.PushId($"AllowedDalamudWindow{windowName}");
            if (ImGui.SmallButton("移除"))
            {
                RemoveAllowedWindowName(conditions, windowName);
                return;
            }
        }
    }

    private void DrawAllowedWindowSystemNamespaceList(MouseLookConditionSettings conditions)
    {
        var windowSystemNamespaces = conditions.IgnoredDalamudWindowSystemNamespaces
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(windowSystemNamespace => windowSystemNamespace, StringComparer.OrdinalIgnoreCase)
            .ToList();

        ImGui.TextUnformatted("允許的完整 Dalamud 視窗系統");
        if (windowSystemNamespaces.Count == 0)
        {
            ImGui.TextDisabled("無");
            return;
        }

        foreach (var windowSystemNamespace in windowSystemNamespaces)
        {
            ImGui.BulletText(windowSystemNamespace);
            ImGui.SameLine();

            using var id = ImRaii.PushId($"AllowedDalamudWindowSystem{windowSystemNamespace}");
            if (ImGui.SmallButton("移除"))
            {
                RemoveAllowedWindowSystemNamespace(conditions, windowSystemNamespace);
                return;
            }
        }
    }

    private void RemoveAllowedWindowSystemNamespace(MouseLookConditionSettings conditions, string windowSystemNamespace)
    {
        conditions.IgnoredDalamudWindowSystemNamespaces.RemoveAll(existing => string.Equals(existing, windowSystemNamespace, StringComparison.OrdinalIgnoreCase));
        save();
    }

    private void RemoveAllowedWindowName(MouseLookConditionSettings conditions, string windowName)
    {
        conditions.IgnoredDalamudWindowNames.RemoveAll(existing => string.Equals(existing, windowName, StringComparison.OrdinalIgnoreCase));
        save();
    }

    private static string GetSuggestedWindowPattern(string windowName)
    {
        if (string.IsNullOrWhiteSpace(windowName))
        {
            return string.Empty;
        }

        var slashIndex = windowName.IndexOf('/');
        return slashIndex <= 0
            ? string.Empty
            : $"{windowName[..(slashIndex + 1)]}*";
    }
}
