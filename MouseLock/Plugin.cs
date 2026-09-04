using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using MouseLock.Windows;
using MouseLock.Configuration.Persistence;
using MouseLock.Commands;
using MouseLock.Input;
using MouseLock.Input.Keybinds;
using MouseLock.MouseLook;
using MouseLock.UI;

namespace MouseLock;

public sealed class Plugin : IDalamudPlugin
{
    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        PluginState.Reset();
        PluginState.Config = ConfigRepository.LoadOrDefault();
        ConfigBackup.DoConfigBackup(Service.PluginInterface);

        PluginState.WindowSystem = new WindowSystem("MouseLock");
        PluginState.ConfigWindow = new ConfigWindow(PluginState.Config);
        PluginState.FirstRunWindow = new FirstRunWindow(PluginState.Config);
        PluginState.WindowSystem.AddWindow(PluginState.ConfigWindow);
        PluginState.WindowSystem.AddWindow(PluginState.FirstRunWindow);

        Service.PluginInterface.UiBuilder.Draw += DrawUi;
        Service.PluginInterface.UiBuilder.OpenMainUi += ToggleUi;
        Service.PluginInterface.UiBuilder.OpenConfigUi += ToggleUi;

        PluginState.CommandHandler = new CommandHandler();
        PluginState.TextInputMonitor = new TextInputMonitor();
        PluginState.MouseLookService = new MouseLookService(PluginState.TextInputMonitor);
        PluginState.ToggleKeybindListener = new ToggleKeybindListener(PluginState.TextInputMonitor);
        PluginState.DtrStatusService = new DtrStatusService();
        PluginState.IpcProvider = new();

        ConfigRepository.SaveImmediate(PluginState.Config);
    }

    public void Dispose()
    {
        PluginState.IpcProvider?.Dispose();
        PluginState.CommandHandler?.Dispose();
        PluginState.MouseLookService?.Dispose();
        PluginState.ToggleKeybindListener?.Dispose();
        PluginState.TextInputMonitor?.Dispose();
        PluginState.DtrStatusService?.Dispose();

        Service.PluginInterface.UiBuilder.Draw -= DrawUi;
        Service.PluginInterface.UiBuilder.OpenMainUi -= ToggleUi;
        Service.PluginInterface.UiBuilder.OpenConfigUi -= ToggleUi;

        PluginState.WindowSystem?.RemoveAllWindows();

        ConfigRepository.SaveImmediate(PluginState.Config);

        PluginState.Reset();
    }

    private static void DrawUi() => PluginState.WindowSystem.Draw();

    private static void ToggleUi() => PluginState.ConfigWindow.Toggle();
}
