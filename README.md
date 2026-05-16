# Lenovo Battery Tray

Lenovo Battery Tray is a small Windows system tray app for quickly switching Lenovo Vantage battery charge modes on compatible Lenovo IdeaPad devices.

## Features

- Runs without opening a normal window.
- Uses a tray icon and right-click menu.
- Supports Normal / Full Charge, Battery Conservation / Storage, and Quick Charge modes.
- Resolves the newest installed Lenovo Vantage `IdeaNotebookAddin` directory at runtime.
- Calls Lenovo Vantage assemblies through reflection; Lenovo DLL files are not copied into this repository or redistributed.
- Enforces x64 runtime because Lenovo Vantage assemblies can fail under Any CPU / 32-bit execution.
- Supports current-user Windows startup through `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`.
- Writes logs to `%AppData%\LenovoBatteryTray\logs\app.log`.

## Requirements

- Windows
- .NET Framework 4.8
- Visual Studio with .NET Framework desktop development tools
- Lenovo Vantage installed with `IdeaNotebookAddin`
- x64 build/runtime

## Build

Open `LenovoBatteryTray.sln` in Visual Studio and build the `x64` configuration.

Command line:

```powershell
dotnet build LenovoBatteryTray.sln -p:Configuration=Release -p:Platform=x64
```

## Notes

The app expects Lenovo Vantage addin DLLs under:

```text
C:\ProgramData\Lenovo\Vantage\Addins\IdeaNotebookAddin\
```

It selects the highest valid version folder containing:

- `BatteryManagementContract.dll`
- `IdeaBatteryAgent.dll`

These DLLs must remain installed on the user's machine and should not be committed to the repository.
