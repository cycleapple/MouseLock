# MouseLock (API 13 port)

Based on [Zeffuro/MouseLock](https://github.com/Zeffuro/MouseLock).

MouseLock keeps FFXIV in mouselook until you actually need the cursor. It pauses for chat, bags, menus, and other UI, and can also turn LMB/RMB into configurable game inputs, hotbar actions, or MouseLock controls.

## API 13 port

The `api13` branch targets the pinned Traditional Chinese Dalamud API 13 runtime on .NET 9. It keeps the upstream feature set while replacing API 15-only plugin lifecycle and FFXIVClientStructs input bindings with API 13-compatible implementations.

Build with the API 13 Dalamud directory supplied as `DalamudLibPath`:

```powershell
dotnet build .\MouseLock.sln --configuration Release /p:DalamudLibPath='C:\path\to\Dalamud\'
```

The packaged plugin is written to `MouseLock\bin\x64\Release\MouseLock\latest.zip`.

