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
- Chooses Turkish or English automatically from the Windows UI language and allows changing language from the tray menu.
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

## GitHub Releases

The repository includes a GitHub Actions workflow at `.github/workflows/dotnet-desktop.yml`.
It builds the app on `windows-latest` with MSBuild, uploads a zipped Windows x64 artifact for every push or pull request, and creates a GitHub Release automatically when a version tag is pushed.

To publish a release:

```powershell
git tag v1.0.0
git push origin v1.0.0
```

After the workflow finishes, GitHub will create a release named `v1.0.0` and attach `LenovoBatteryTray-win-x64.zip`.

## GitHub Packages

This app is a desktop executable, not a reusable library or container image, so GitHub Releases are the right place for downloadable builds.
GitHub Packages is only useful here if the project later adds a NuGet package, installer package, or container image.

## Notes

The app expects Lenovo Vantage addin DLLs under:

```text
C:\ProgramData\Lenovo\Vantage\Addins\IdeaNotebookAddin\
```

It selects the highest valid version folder containing:

- `BatteryManagementContract.dll`
- `IdeaBatteryAgent.dll`

These DLLs must remain installed on the user's machine and should not be committed to the repository.
