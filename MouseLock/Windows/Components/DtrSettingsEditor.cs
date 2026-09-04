using System;
using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using MouseLock.Configuration;

namespace MouseLock.Windows.Components;

internal sealed class DtrSettingsEditor(Action save)
{
    private static readonly ComboOption<DtrTextMode>[] TextModeOptions =
    [
        new(DtrTextMode.Simple, "簡潔：MouseLock：已暫停"),
        new(DtrTextMode.Detailed, "詳細：MouseLock：已暫停：原因"),
        new(DtrTextMode.Compact, "精簡：ML：暫停"),
    ];

    private static readonly string[] TextModeLabels = CreateLabels(TextModeOptions);

    public void Draw(DtrSettings settings)
    {
        var enabled = settings.Enabled;
        if (ImGui.Checkbox("顯示於伺服器資訊列", ref enabled))
        {
            settings.Enabled = enabled;
            save();
        }

        using (ImRaii.Disabled(!settings.Enabled))
        {
            ConfigWindow.DrawNestIndicator(1);
            var textModeIndex = FindOptionIndex(TextModeOptions, settings.TextMode);
            if (ImGui.Combo("狀態文字", ref textModeIndex, TextModeLabels, TextModeLabels.Length))
            {
                settings.TextMode = TextModeOptions[textModeIndex].Value;
                save();
            }
        }

        ImGui.TextWrapped("在伺服器資訊列項目上按左鍵可開關 MouseLock；按右鍵可開關此設定視窗。");
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
