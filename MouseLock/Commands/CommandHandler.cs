using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dalamud.Game.Command;
using MouseLock.Configuration;
using MouseLock.MouseLook;
using MouseLock.MouseLook.Activation;

namespace MouseLock.Commands;

internal sealed class CommandHandler : IDisposable
{
    private const string MainCommand = "/mouselock";
    private const string HelpDescription = "MouseLock 指令。使用「/mouselock help」查看可用選項。";

    private readonly Dictionary<string, SubCommand> _subCommands;

    public CommandHandler()
    {
        _subCommands = new Dictionary<string, SubCommand>(StringComparer.OrdinalIgnoreCase)
        {
            ["help"] = new(_ => PrintHelp(), "顯示此說明訊息"),
            ["off"] = new(_ => SetEnabled(false), "停用滑鼠視角鎖定"),
            ["on"] = new(_ => SetEnabled(true), "啟用滑鼠視角鎖定"),
            ["status"] = new(_ => PrintStatus(), "顯示目前狀態"),
            ["suspend"] = new(HandleSuspend, "切換或控制指令要求的暫停狀態", "[on|off|resume|clear|status] [source]"),
            ["toggle"] = new(_ => ToggleEnabled(), "切換滑鼠視角鎖定"),
        };

        Service.CommandManager.AddHandler(MainCommand, new CommandInfo(OnCommand)
        {
            DisplayOrder = 1,
            ShowInHelp = true,
            HelpMessage = HelpDescription,
        });
    }

    public void Dispose()
    {
        Service.CommandManager.RemoveHandler(MainCommand);
    }

    private void OnCommand(string command, string args)
    {
        var parts = args.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            PluginState.ConfigWindow.Toggle();
            return;
        }

        var subCommandName = parts[0];
        var subArgs = parts.Length > 1 ? parts[1] : string.Empty;

        if (_subCommands.TryGetValue(subCommandName, out var subCommand))
        {
            subCommand.Action(subArgs);
            return;
        }

        PrintChat($"未知指令：{subCommandName}。使用「{MainCommand} help」查看可用指令。");
    }

    private static void SetEnabled(bool enabled)
    {
        MouseLockStateController.SetEnabled(enabled);
        PrintStatus();
    }

    private static void ToggleEnabled()
    {
        MouseLockStateController.ToggleEnabled();
        PrintStatus();
    }

    private static void PrintStatus()
    {
        var status = PluginState.MouseLookService?.Status
                     ?? MouseLookStatus.Unavailable(MouseLookPauseReason.HookUnavailable);
        PrintChat($"{MouseLookStatusFormatter.GetSummary(status)} - {MouseLookStatusFormatter.GetDetail(status)}");
    }

    private static void HandleSuspend(string args)
    {
        var parts = args.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            SetSuspended(DefaultCommandSuspensionSource, !SuspensionRegistry.IsSuspendedBy(DefaultCommandSuspensionSource));
            return;
        }

        var action = parts[0];
        var source = parts.Length > 1 ? parts[1] : DefaultCommandSuspensionSource;
        switch (action.ToLowerInvariant())
        {
            case "on":
                SetSuspended(source, true);
                break;

            case "off":
            case "resume":
                SetSuspended(source, false);
                break;

            case "clear":
                ClearSuspensions();
                break;

            case "status":
                PrintSuspensionStatus();
                break;

            default:
                PrintChat($"未知的暫停操作：{action}。請使用「{MainCommand} suspend on|off|clear|status [source]」。");
                break;
        }
    }

    private const string DefaultCommandSuspensionSource = "Command";

    private static void SetSuspended(string source, bool suspended)
    {
        SuspensionRegistry.SetSuspended(source, suspended);
        PluginState.MouseLookService?.RefreshCurrentStatus();
        PrintChat(suspended
            ? $"已由「{MouseLookStatusFormatter.GetSuspensionSourceLabel(source)}」要求暫停。"
            : $"已解除「{MouseLookStatusFormatter.GetSuspensionSourceLabel(source)}」的暫停要求。");
        PrintSuspensionStatus();
    }

    private static void ClearSuspensions()
    {
        var clearedCount = SuspensionRegistry.Clear();
        PluginState.MouseLookService?.RefreshCurrentStatus();
        PrintChat($"已清除 {clearedCount} 個暫停來源。");
        PrintSuspensionStatus();
    }

    private static void PrintSuspensionStatus()
    {
        PrintChat(SuspensionRegistry.IsSuspended
            ? $"暫停來源：{MouseLookStatusFormatter.GetSuspensionSourcesSummary()}"
            : "目前沒有外部暫停要求。");
    }

    private void PrintHelp()
    {
        var builder = new StringBuilder("MouseLock 指令：\n");
        foreach (var (name, subCommand) in _subCommands.OrderBy(pair => pair.Key))
        {
            builder.Append(MainCommand).Append(' ').Append(name);
            if (!string.IsNullOrWhiteSpace(subCommand.Usage))
            {
                builder.Append(' ').Append(subCommand.Usage);
            }

            builder.Append(" - ").AppendLine(subCommand.Description);
        }

        PrintChat(builder.ToString());
    }

    private static void PrintChat(string message)
    {
        Service.ChatGui.Print(message, "MouseLock");
    }

    private sealed record SubCommand(Action<string> Action, string Description, string Usage = "");
}
