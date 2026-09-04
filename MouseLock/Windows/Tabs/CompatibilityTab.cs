using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using MouseLock.Configuration;

namespace MouseLock.Windows.Tabs;

internal sealed class CompatibilityTab(SystemConfiguration config, Action save)
{
    public void Draw()
    {
        using var tab = ImRaii.TabItem("相容性");
        if (!tab)
        {
            return;
        }

        var hideCursorOverlayPlugins = config.Compatibility.HideCursorOverlayPluginsDuringMouseLook;
        if (ImGui.Checkbox("滑鼠視角啟用時隱藏游標覆蓋插件", ref hideCursorOverlayPlugins))
        {
            config.Compatibility.HideCursorOverlayPluginsDuringMouseLook = hideCursorOverlayPlugins;
            save();
        }

        var disableDuringTPieRing = config.Compatibility.DisableDuringTPieRing;
        if (ImGui.Checkbox("使用 TPie 時暫停", ref disableDuringTPieRing))
        {
            config.Compatibility.DisableDuringTPieRing = disableDuringTPieRing;
            save();
        }
    }
}
