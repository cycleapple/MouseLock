using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace MouseLock.MouseLook.Native;

internal static class MouseButtonSuppression
{
    public static unsafe void Apply(UIInputData* inputData, MouseButtonFlags buttons)
    {
        if (buttons == MouseButtonFlags.None)
        {
            return;
        }

        ClearButtons(&inputData->CursorInputs, buttons);
        ClearButtons(&inputData->UIFilteredCursorInputs, buttons);

        var currentMouseDragButtons = (byte*)inputData + 0x9AC;
        *currentMouseDragButtons &= unchecked((byte)~(byte)buttons);
        inputData->UIFilteredCursorInputsButtonsChanged = true;
    }

    private static unsafe void ClearButtons(CursorInputData* inputData, MouseButtonFlags buttons)
    {
        inputData->MouseButtonHeldFlags &= ~buttons;
        inputData->MouseButtonPressedFlags &= ~buttons;
        inputData->MouseButtonReleasedFlags &= ~buttons;
        inputData->MouseButtonHeldThrottledFlags &= ~buttons;
    }
}
