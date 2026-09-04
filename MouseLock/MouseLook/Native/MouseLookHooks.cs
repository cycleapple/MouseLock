using System;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace MouseLock.MouseLook.Native;

internal sealed unsafe class MouseLookHooks(
    AtkModuleHandleInputDelegate atkModuleHandleInputDetour,
    CameraInputSourceDelegate cameraInputSourceDetour)
    : IDisposable
{
    private const string AtkModuleHandleInputSignature = "E8 ?? ?? ?? ?? 41 B6 01 40 B5 02";
    private const string CameraInputSourceSignature = "48 83 EC ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 74 ?? B8";

    private Hook<AtkModuleHandleInputDelegate>? _atkModuleHandleInputHook;
    private Hook<CameraInputSourceDelegate>? _cameraInputSourceHook;

    public void Enable()
    {
        EnableAtkModuleHandleInputHook();
        EnableCameraInputSourceHook();
    }

    public bool IsAtkModuleHandleInputHookReady { get; private set; }

    public bool IsCameraInputSourceHookReady { get; private set; }

    public bool AreDetoursReady => IsAtkModuleHandleInputHookReady && IsCameraInputSourceHookReady;

    public byte RunOriginalAtkModuleHandleInput(
        AtkModule* atkModule,
        UIInputData* inputData,
        bool isPadMouseModeEnabled)
        => _atkModuleHandleInputHook!.Original(atkModule, inputData, isPadMouseModeEnabled);

    public CameraInputSource RunOriginalCameraInputSource()
        => _cameraInputSourceHook!.Original();

    public void Retry()
    {
        if (!IsAtkModuleHandleInputHookReady)
        {
            DisposeAtkModuleHandleInputHook();
            EnableAtkModuleHandleInputHook();
        }

        if (!IsCameraInputSourceHookReady)
        {
            DisposeCameraInputSourceHook();
            EnableCameraInputSourceHook();
        }
    }

    public void Dispose()
    {
        DisposeCameraInputSourceHook();
        DisposeAtkModuleHandleInputHook();
    }

    private void EnableAtkModuleHandleInputHook()
    {
        if (!Service.SigScanner.TryScanText(AtkModuleHandleInputSignature, out var address))
        {
            Service.Logger.Error("Could not hook AtkModule.HandleInput: signature was not found.");
            return;
        }

        _atkModuleHandleInputHook = Service.GameInteropProvider.HookFromAddress<AtkModuleHandleInputDelegate>(
            address,
            atkModuleHandleInputDetour);
        _atkModuleHandleInputHook.Enable();
        IsAtkModuleHandleInputHookReady = true;
        Service.Logger.Information("Hooked AtkModule.HandleInput at 0x{Address:X}.", address);
    }

    private void DisposeAtkModuleHandleInputHook()
    {
        _atkModuleHandleInputHook?.Dispose();
        _atkModuleHandleInputHook = null;
        IsAtkModuleHandleInputHookReady = false;
    }

    private void EnableCameraInputSourceHook()
    {
        if (!Service.SigScanner.TryScanText(CameraInputSourceSignature, out var address))
        {
            Service.Logger.Error("Could not hook Camera.GetCameraInputSource: signature was not found.");
            return;
        }

        _cameraInputSourceHook = Service.GameInteropProvider.HookFromAddress(
            address,
            cameraInputSourceDetour);
        _cameraInputSourceHook.Enable();
        IsCameraInputSourceHookReady = true;
        Service.Logger.Information("Hooked camera input source at 0x{Address:X}.", address);
    }

    private void DisposeCameraInputSourceHook()
    {
        _cameraInputSourceHook?.Dispose();
        _cameraInputSourceHook = null;
        IsCameraInputSourceHookReady = false;
    }
}

internal unsafe delegate byte AtkModuleHandleInputDelegate(
    AtkModule* atkModule,
    UIInputData* inputData,
    bool isPadMouseModeEnabled);

internal delegate CameraInputSource CameraInputSourceDelegate();

internal enum CameraInputSource
{
    None = 0,
    Keyboard = 1,
    Analog = 2,
    MouseDrag = 3,
}
