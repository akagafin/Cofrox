# Cofrox Native Windows

Cofrox is a native WinUI 3 desktop app that provides secure, offline file conversion for Windows. Conversion operations are performed on-device with modern UI conventions, Windows theme support, and Microsoft Store packaging readiness.

## 🚀 Highlights

- Fully on-device conversion (no cloud upload)
- WinUI 3 desktop UI with light/dark mode follow
- Extensible engine-based converter architecture
- Format compatibility tracking and history
- Privacy-first: no telemetry, no user data leaks

## 📦 Solution Layout

- `src/Cofrox.App/`: WinUI 3 application shell, XAML UI, view models, navigation, UX services.
- `src/Cofrox.Core/`: shared format catalog, format detection, compatibility matrix, common utilities.
- `src/Cofrox.Domain/`: domain entities, enums, options, interfaces, value objects.
- `src/Cofrox.Data/`: persistence layers (SQLite history, app settings, temp file and profile management).
- `src/Cofrox.Converters/`: conversion coordination, engine adapters, external process runners.
- `tests/Cofrox.Core.Tests/`: unit tests for format catalog, detection logic, compatibility and utilities.

## 🛠️ Requirements

- Windows 10 1809 or newer (Windows 11 recommended)
- Visual Studio 2022 17.8 or later
- .NET 8 SDK
- Windows App SDK 1.5 or later
- Windows 10 SDK 17763+

Optional tools for full engine coverage:
- FFmpeg
- Pandoc
- Ghostscript
- 7-Zip
- LibreOffice

## 🧩 Quick Setup

1. Clone repository:

```powershell
git clone https://github.com/<your-org>/cofrox.git
cd cofrox
```

2. Restore dependencies (automatic with .NET build):

```powershell
dotnet restore
```

3. Open `Cofrox.sln` in Visual Studio, select `x64` build, then `Debug` or `Release`.

## 🏗️ Build and Run

### Using Visual Studio
- Set `Cofrox.App` as startup project.
- Use `Debug` for local testing, `Release` for publication.
- Build and run.

### CLI

```powershell
dotnet build ./Cofrox.sln -c Release -r win10-x64
dotnet run --project ./src/Cofrox.App/Cofrox.App.csproj
```

## 🧪 Tests

```powershell
dotnet test ./tests/Cofrox.Core.Tests/Cofrox.Core.Tests.csproj
```

## 🛡️ Privacy and Legal

- Privacy first: no user data is sent to any server.
- [PRIVACY_POLICY.md](./PRIVACY_POLICY.md)
- [TERMS_OF_USE.md](./TERMS_OF_USE.md)
- [DISCLAIMER.md](./DISCLAIMER.md)
- [THIRD_PARTY_LICENSES.md](./THIRD_PARTY_LICENSES.md)

## 📝 Contributing

1. Open an issue to discuss bugs or feature requests.
2. Create a branch from `main`: `feature/<your-feature-name>`.
3. Commit code with descriptive messages.
4. Add unit tests for any feature or bug fix.
5. Open a pull request.
