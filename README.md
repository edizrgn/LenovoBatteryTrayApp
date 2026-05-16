# Lenovo Battery Tray

Lenovo Battery Tray is a small Windows tray utility for switching Lenovo Vantage battery charge modes without opening Lenovo Vantage.

It is built for compatible Lenovo IdeaPad devices where Lenovo Vantage exposes the `IdeaNotebookAddin` battery management components.

> This is an unofficial community utility. It is not affiliated with, endorsed by, or supported by Lenovo.

## What It Does

- Runs quietly in the Windows notification area.
- Lets you switch between:
  - Normal / Full Charge
  - Battery Conservation / Storage
  - Quick Charge
- Shows the current mode in the tray tooltip and checked menu item.
- Can refresh the current battery mode from Lenovo Vantage.
- Can start automatically with Windows for the current user.
- Supports English and Turkish.
- Writes diagnostic logs to `%AppData%\LenovoBatteryTray\logs\app.log`.

## Download

Download the latest build from the repository's GitHub Releases page.

The release asset is named:

```text
LenovoBatteryTray-win-x64.zip
```

Extract the zip file and run:

```text
LenovoBatteryTray.exe
```

There is currently no installer. The app is distributed as a portable zip.

## Requirements

- Windows x64
- .NET Framework 4.8
- Lenovo Vantage installed and up to date
- A compatible Lenovo device with the `IdeaNotebookAddin` battery components

The app expects Lenovo Vantage files to exist under:

```text
C:\ProgramData\Lenovo\Vantage\Addins\IdeaNotebookAddin\
```

It does not include or redistribute Lenovo DLL files.

## Usage

1. Start `LenovoBatteryTray.exe`.
2. Find the battery icon in the Windows notification area.
3. Right-click the tray icon.
4. Choose one of the available battery modes:
   - `Normal / Full Charge`
   - `Battery Conservation`
   - `Quick Charge`
5. Use `Refresh Status` if Lenovo Vantage changed the mode outside this app.
6. Enable `Start with Windows` if you want the app to launch on sign-in.

The selected language is detected from the Windows UI language on first launch. You can change it from the tray menu later.

## Compatibility Notes

Lenovo Vantage versions and device support can vary by model and region. If the app cannot find the required Lenovo Vantage components, it will show an error and write details to the log file.

The app is intentionally built as x64. Lenovo Vantage battery assemblies may fail when loaded from a 32-bit or Any CPU process.

## Build From Source

Open `LenovoBatteryTray.sln` in Visual Studio and build the `x64` configuration.

Command line:

```powershell
dotnet build LenovoBatteryTray.sln -p:Configuration=Release -p:Platform=x64
```

Build requirements:

- Visual Studio with .NET Framework desktop development tools
- .NET Framework 4.8 targeting pack
- x64 build configuration

## GitHub Actions

The repository includes a workflow at `.github/workflows/dotnet-desktop.yml`.

It runs on Windows, builds the app with MSBuild, creates a zipped x64 artifact, and uploads it for every push or pull request.

## Publishing A Release

Create and push a version tag:

```powershell
git tag v1.0.0
git push origin v1.0.0
```

When the workflow finishes, GitHub creates a release named `v1.0.0` and attaches:

```text
LenovoBatteryTray-win-x64.zip
```

Use semantic version tags such as `v1.0.0`, `v1.1.0`, and `v2.0.0`.

## GitHub Packages

This project is a desktop executable, not a reusable library or container image. GitHub Releases are the best place to distribute builds.

GitHub Packages is not needed unless the project later adds a NuGet package, installer package, or container image.

## Troubleshooting

If the app cannot read or change the battery mode:

- Make sure Lenovo Vantage is installed.
- Update Lenovo Vantage from Microsoft Store or Lenovo's update tools.
- Confirm the `IdeaNotebookAddin` folder exists under `C:\ProgramData\Lenovo\Vantage\Addins\`.
- Run the x64 build of the app.
- Check the log file:

```text
%AppData%\LenovoBatteryTray\logs\app.log
```

## Repository Notes

Lenovo Vantage DLL files are intentionally not committed to this repository. The app loads the installed Lenovo Vantage assemblies from the user's machine at runtime.

The app looks for the newest valid `IdeaNotebookAddin` version folder containing:

- `BatteryManagementContract.dll`
- `IdeaBatteryAgent.dll`

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE).
