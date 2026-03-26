# Publishing Notes

## Build and automated tests

The repo includes `global.json` so the **.NET 10 SDK** (e.g. 10.0.201) is selected when multiple SDKs are installed. The WinUI app targets **x64** only: use `dotnet build .\Cofrox.sln -c Release -p:Platform=x64` (not `AnyCPU`).

- **Libraries and tests** (Domain, Core, Data, Converters, `Cofrox.Core.Tests`) build and run with `dotnet test` from the CLI.
- **WinUI (`Cofrox.App`)** compiles XAML via `XamlCompiler.exe`. If `dotnet build` fails at the XamlCompiler step with no clear message, build and run **`Cofrox.App` from Visual Studio 2026** with the **.NET desktop development** workload and **Windows App SDK** support (this is the supported path for WinUI 3 on many setups).

## Bundled Tools

Place offline conversion binaries under:

- `src/Cofrox.App/Tools/ffmpeg/ffmpeg.exe`
- `src/Cofrox.App/Tools/pandoc/pandoc.exe`
- `src/Cofrox.App/Tools/imagemagick/magick.exe`
- `src/Cofrox.App/Tools/ghostscript/gswin64c.exe`
- `src/Cofrox.App/Tools/7zip/7z.exe`
- `src/Cofrox.App/Tools/libreoffice/program/soffice.exe` (optional)

## Packaging

- `WindowsPackageType=None` is enabled so the project can run unpackaged during development.
- For Store-ready MSIX, add a packaging project in Visual Studio 2026 and point it at `src/Cofrox.App`.
- Keep the runtime target at `win-x64`.

## Verification Checklist

- Theme switches between Windows Light and Dark while the app is running.
- Mica is active on the main window on Windows 11.
- Drop zone accepts drag/drop and file picker import.
- Conversion queue remains responsive while jobs run.
- History persists to SQLite and can be cleared through the confirmation dialog.
- Test on Windows 10 build `17763` and Windows 11 build `22000+`.
