using System;
using System.Linq;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Utility.Raii;
using MouseLock.Configuration;
using MouseLock.Input;

namespace MouseLock.Windows.Components;

internal sealed class ToggleKeybindEditor(Action save)
{
    private bool _isRecording;
    private bool _skipCaptureFrame;

    public void Draw(ToggleKeybindSettings settings)
    {
        var enabled = settings.Enabled;
        if (ImGui.Checkbox("啟用切換快捷鍵", ref enabled))
        {
            settings.Enabled = enabled;
            save();
        }

        if (!settings.Enabled)
        {
            _isRecording = false;
        }

        using var disabled = ImRaii.Disabled(!settings.Enabled);

        ConfigWindow.DrawNestIndicator(1);
        DrawRecordedKeybind(settings);

        ConfigWindow.DrawNestIndicator(1);
        var ignoreTextInput = settings.IgnoreWhileTextInputActive;
        if (ImGui.Checkbox("文字輸入期間忽略快捷鍵", ref ignoreTextInput))
        {
            settings.IgnoreWhileTextInputActive = ignoreTextInput;
            save();
        }

        ConfigWindow.DrawNestIndicator(1);
        var showChatFeedback = settings.ShowChatFeedback;
        if (ImGui.Checkbox("使用時在聊天欄顯示結果", ref showChatFeedback))
        {
            settings.ShowChatFeedback = showChatFeedback;
            save();
        }
    }

    private void DrawRecordedKeybind(ToggleKeybindSettings settings)
    {
        ImGui.TextUnformatted($"切換按鍵：{settings.Keybind}");
        ImGui.SameLine();

        if (ImGui.Button(_isRecording ? "錄製中…" : "錄製"))
        {
            _isRecording = true;
            _skipCaptureFrame = true;
        }

        ImGui.SameLine();
        if (ImGui.Button("清除"))
        {
            settings.SetKeybind(new RecordedKeybind());
            save();
        }

        if (_isRecording)
        {
            ConfigWindow.DrawNestIndicator(2);
            ImGui.TextDisabled("請按下按鍵組合；按 Esc 取消。");

            if (_skipCaptureFrame)
            {
                _skipCaptureFrame = false;
            }
            else if (Service.KeyState[VirtualKey.ESCAPE])
            {
                _isRecording = false;
            }
            else if (TryCaptureKeybind(out var keybind))
            {
                settings.SetKeybind(keybind);
                _isRecording = false;
                save();
            }
        }

        DrawKeybindConflicts(settings.Keybind);
    }

    private static bool TryCaptureKeybind(out RecordedKeybind keybind)
    {
        keybind = new RecordedKeybind();

        var pressedKeys = Service.KeyState.GetValidVirtualKeys()
            .Where(key => key.IsModifier() || key.IsPrimaryBindable())
            .Where(key => Service.KeyState[key])
            .ToArray();

        var primaryKey = pressedKeys.FirstOrDefault(key => key.IsPrimaryBindable(), VirtualKey.NO_KEY);
        if (primaryKey == VirtualKey.NO_KEY)
        {
            return false;
        }

        keybind.Key = primaryKey;
        keybind.Modifiers = pressedKeys
            .Where(key => key.IsModifier())
            .Select(key => key.NormalizeModifier())
            .Distinct()
            .OrderByModifier()
            .ToList();

        return true;
    }

    private static void DrawKeybindConflicts(RecordedKeybind keybind)
    {
        var conflicts = KeybindConflictDetector.GetGameConflicts(keybind);
        if (conflicts.Count == 0)
        {
            return;
        }

        ConfigWindow.DrawNestIndicator(2);
        var preview = string.Join(", ", conflicts.Take(4));
        if (conflicts.Count > 4)
        {
            preview += $"，另有 {conflicts.Count - 4} 個";
        }

        ImGui.TextDisabled($"與遊戲快捷鍵衝突：{preview}");
    }
}
