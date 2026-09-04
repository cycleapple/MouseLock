using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.UI;
using MouseLock.Configuration;
using MouseLock.Game;
using MouseLock.MouseLook;
using MouseLock.MouseLook.Activation;

namespace MouseLock.Windows.Tabs;

internal sealed class DiagnosticsTab(SystemConfiguration config, Action save)
{
    private string _message = string.Empty;

    public unsafe void Draw()
    {
        using var tab = ImRaii.TabItem("診斷");
        if (!tab)
        {
            return;
        }

        var debugEnabled = config.General.DebugEnabled;
        if (ImGui.Checkbox("啟用除錯", ref debugEnabled))
        {
            config.General.DebugEnabled = debugEnabled;
            save();
        }

        var service = PluginState.MouseLookService;
        var status = service?.Status ?? MouseLookStatus.Unavailable(MouseLookPauseReason.HookUnavailable);
        var lastDecision = service?.LastDecision ?? MouseLookDecision.Pause(MouseLookPauseReason.HookUnavailable);

        ImGui.TextUnformatted($"狀態：{MouseLookStatusFormatter.GetSummary(status)}");
        ImGui.TextUnformatted($"最近判定：{(lastDecision.ShouldLock ? "允許" : "暫停")}（{MouseLookStatusFormatter.GetReasonLabel(lastDecision.Reason)}）");
        ImGui.TextUnformatted($"整體可用性：{FormatReady(service?.IsMouseLookAvailable == true)}");
        ImGui.TextUnformatted($"Atk 輸入攔截：{FormatReady(service?.IsAtkModuleHandleInputHookReady == true)}");
        ImGui.TextUnformatted($"鏡頭輸入攔截：{FormatReady(service?.IsCameraInputSourceHookReady == true)}");
        ImGui.TextUnformatted($"原生滑鼠拖曳：{FormatReady(service?.IsMouseDragAvailable == true)}");
        ImGui.TextUnformatted($"游標置中排程器：{FormatReady(service?.IsCursorRecenterAvailable == true)}");

        var focusedAddon = NativeUiState.TryGetFocusedBlockingAddonName(out var focusedAddonName)
            ? focusedAddonName
            : "無";
        var inputData = UIInputData.Instance();
        var hoveredAddon = inputData is not null &&
                           NativeUiState.TryGetHoveredBlockingAddonName(inputData, out var hoveredAddonName)
            ? ConfigWindow.DisplayAddonName(hoveredAddonName)
            : inputData is null
                ? "無法使用輸入"
                : "無";

        ImGui.TextUnformatted($"取得焦點的遊戲原生視窗：{focusedAddon}");
        ImGui.TextUnformatted($"游標停留的遊戲原生視窗：{hoveredAddon}");
        ImGui.TextUnformatted($"外部暫停來源：{DisplayExternalSuspensions()}");

        ImGui.Spacing();
        if (ImGui.Button("強制釋放游標"))
        {
            service?.ForceReleaseCursor();
            _message = "已要求釋放游標。";
        }

        ImGui.SameLine();
        if (ImGui.Button("重試 Hook"))
        {
            service?.RetryHooks();
            _message = "已要求重試 Hook。";
        }

        if (!string.IsNullOrEmpty(_message))
        {
            ImGui.TextDisabled(_message);
        }
    }

    private static string FormatReady(bool ready)
        => ready ? "可用" : "無法使用";

    private static string DisplayExternalSuspensions()
        => SuspensionRegistry.IsSuspended ? MouseLookStatusFormatter.GetSuspensionSourcesSummary() : "無";
}
