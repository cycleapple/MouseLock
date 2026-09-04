using Dalamud.Bindings.ImGui;
using MouseLock.MouseLook;
using MouseLock.MouseLook.Activation;

namespace MouseLock.Windows.Components;

internal sealed class MouseLookStatusCard
{
    public void Draw()
    {
        var status = PluginState.MouseLookService?.Status
                     ?? MouseLookStatus.Unavailable(MouseLookPauseReason.HookUnavailable);

        ImGui.TextUnformatted($"狀態：{MouseLookStatusFormatter.GetSummary(status)}");
        ImGui.TextDisabled(MouseLookStatusFormatter.GetDetail(status));
        if (SuspensionRegistry.IsSuspended)
        {
            ImGui.TextDisabled($"外部暫停來源：{MouseLookStatusFormatter.GetSuspensionSourcesSummary()}");
        }

        ImGui.Spacing();
    }
}
