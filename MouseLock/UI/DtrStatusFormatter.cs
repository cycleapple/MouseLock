using MouseLock.Configuration;
using MouseLock.MouseLook;

namespace MouseLock.UI;

internal static class DtrStatusFormatter
{
    public static string GetText(MouseLookStatus status, DtrTextMode textMode)
        => textMode switch
        {
            DtrTextMode.Detailed => GetDetailedText(status),
            DtrTextMode.Compact => GetCompactText(status),
            _ => GetSimpleText(status),
        };

    private static string GetDetailedText(MouseLookStatus status)
        => status.Kind switch
        {
            MouseLookStatusKind.Off => "MouseLock：關閉",
            MouseLookStatusKind.Ready => "MouseLock：開啟",
            MouseLookStatusKind.Active => "MouseLock：作用中",
            MouseLookStatusKind.Paused => $"MouseLock：已暫停：{MouseLookStatusFormatter.GetReasonLabel(status.Reason)}",
            MouseLookStatusKind.Unavailable => "MouseLock：無法使用",
            _ => "MouseLock",
        };

    private static string GetSimpleText(MouseLookStatus status)
        => status.Kind switch
        {
            MouseLookStatusKind.Off => "MouseLock：關閉",
            MouseLookStatusKind.Ready => "MouseLock：開啟",
            MouseLookStatusKind.Active => "MouseLock：作用中",
            MouseLookStatusKind.Paused => "MouseLock：已暫停",
            MouseLookStatusKind.Unavailable => "MouseLock：無法使用",
            _ => "MouseLock",
        };

    private static string GetCompactText(MouseLookStatus status)
        => status.Kind switch
        {
            MouseLookStatusKind.Off => "ML：關",
            MouseLookStatusKind.Ready => "ML：開",
            MouseLookStatusKind.Active => "ML：作用中",
            MouseLookStatusKind.Paused => "ML：暫停",
            MouseLookStatusKind.Unavailable => "ML：無法使用",
            _ => "ML",
        };
}
