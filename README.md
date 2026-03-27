# Cofrox Native Windows

Cofrox is a native WinUI 3 desktop application for secure, offline file
conversion on Windows. It is designed as a universal converter for media,
images, documents, archives, data formats, subtitles, fonts, and 3D assets
while staying privacy-first and Microsoft Store aware.

## Highlights

- Fully on-device conversion with no cloud upload
- WinUI 3 desktop UI with Windows theme follow and Fluent materials
- Plugin-style conversion engines for media, image, document, archive, data,
  subtitle, font, and 3D workflows
- Smart conversion planning layer with presets, queue persistence, and FFmpeg
  command mapping
- Privacy-first defaults with local history, temp cleanup, and no telemetry

## Solution Layout

- `src/Cofrox.App/`: WinUI 3 shell, XAML views, navigation, UX services, and
  view models
- `src/Cofrox.Application/`: application services for presets, smart
  recommendations, queue state, and FFmpeg planning
- `src/Cofrox.Core/`: format catalog, compatibility matrix, format detection,
  and shared utilities
- `src/Cofrox.Domain/`: entities, enums, interfaces, and value objects
- `src/Cofrox.Data/`: SQLite history, local settings, temp file management, and
  bundled tool discovery
- `src/Cofrox.Converters/`: conversion coordinator, engine adapters, and
  external process execution
- `tests/Cofrox.Core.Tests/`: core unit tests
- `tests/Cofrox.Application.Tests/`: application-service unit tests and
  integration scenario scaffolding

## Requirements

- Windows 10 1809 or newer (Windows 11 recommended)
- Visual Studio 2022 with the .NET desktop development workload
- .NET 8 SDK (see `global.json`; tested against `8.0.419+`)
- Windows App SDK 1.5 or later
- Windows 10 SDK `17763+`

## Quick Start

1. Clone the repository.
2. Open `Cofrox.sln` in Visual Studio and select `x64`.
3. Restore NuGet packages.
4. Build `Cofrox.App` in `Debug` or `Release`.

CLI example:

```powershell
dotnet restore .\Cofrox.sln
dotnet build .\Cofrox.sln -c Release -p:Platform=x64
dotnet test .\tests\Cofrox.Core.Tests\Cofrox.Core.Tests.csproj
dotnet test .\tests\Cofrox.Application.Tests\Cofrox.Application.Tests.csproj
```

## Engineering Docs

| Document | Purpose |
|---|---|
| [ARCHITECTURE.md](./ARCHITECTURE.md) | Clean architecture, plugin engine design, and key production services |
| [QA_REPORT.md](./QA_REPORT.md) | Test strategy, critical bug list, and coverage baseline |
| [SECURITY_AUDIT.md](./SECURITY_AUDIT.md) | Command execution, file handling, privacy, and hardening review |
| [PERFORMANCE_AUDIT.md](./PERFORMANCE_AUDIT.md) | CPU, memory, queue, and throughput analysis |
| [STORE_COMPLIANCE.md](./STORE_COMPLIANCE.md) | Microsoft Store readiness, packaging, and certification checklist |
| [RELEASE_CHECKLIST.md](./RELEASE_CHECKLIST.md) | Build, versioning, packaging, and ship checklist |
| [PUBLISHING.md](./PUBLISHING.md) | Practical packaging and bundled-tool notes |
| [FFMPEG_COMPLIANCE.md](./FFMPEG_COMPLIANCE.md) | Audit of the current FFmpeg bundle and release obligations |

## Legal

| Document | Description |
|---|---|
| [License](./LICENSE) | MIT license for the Cofrox application code |
| [Notice](./NOTICE) | Redistribution notices for bundled third-party components |
| [Privacy Policy](./PRIVACY_POLICY.md) | What data Cofrox collects (none) and how local data is handled |
| [Terms of Use](./TERMS_OF_USE.md) | Rules governing use of the application |
| [Disclaimer](./DISCLAIMER.md) | Liability disclaimer and user responsibilities |
| [Third-Party Licenses](./THIRD_PARTY_LICENSES.md) | Audited GitHub-friendly dependency and redistribution summary |
| [Third-Party Licenses (Text)](./THIRD_PARTY_LICENSES.txt) | Release-engineering inventory of licenses, obligations, and risks |

Cofrox is 100% on-device. No data is ever sent to any server.

## Current Release Reality

The repository still needs additional work before a Microsoft Store release:

- a final MSIX packaging pass still needs to be completed on a clean Windows machine
- Windows App Certification Kit still needs to be run on the final MSIX output
- any optional offline tool bundle still needs a separate legal audit before redistribution

## Contributing

1. Open an issue to discuss bugs or feature requests.
2. Create a branch from `main` using the `codex/` prefix for Codex-authored
   work.
3. Add or update tests for every behavioral change.
4. Keep converter changes isolated from UI-only work where possible.
5. Open a pull request with test notes and risk notes.
