# Cofrox Release Checklist

## Build Configuration

- SDK: .NET 10 (`global.json`)
- IDE: Visual Studio 2026
- Platform: `x64`
- App model: WinUI 3 desktop
- Packaging target: MSIX for Store, unpackaged for internal debug/portable distribution

## Versioning Strategy

Use semantic versioning:

- `MAJOR`: breaking UX or engine behavior changes
- `MINOR`: new formats, presets, or compatible features
- `PATCH`: bug fixes, security fixes, and packaging corrections

Recommended release metadata:

- assembly version: stable within major series when possible
- file version: increment every shipped build
- package version: match release version and revision for Store submission

## Pre-Release Checklist

1. Restore and build the full solution on a clean Windows machine.
2. Run unit tests:
   - `Cofrox.Core.Tests`
   - `Cofrox.Application.Tests`
3. Run integration matrix with bundled tools and fixture files.
4. Verify legal docs, disclaimer gate, and offline behavior.
5. Verify temp cleanup and history persistence.
6. Run Windows App Certification Kit on the MSIX output.
7. Review third-party binary versions and notices.

## Release Validation

| Area | Check |
|---|---|
| Windows 10 | Launch, theme switching, conversion queue, bundled tools |
| Windows 11 | Mica, snap layouts, accent/theme behavior |
| Low-end device | low-memory FFmpeg policy and parallel limit |
| Hardware encoders | NVENC, Quick Sync, and AMF fallback behavior |
| Offline mode | no outbound dependency after install |

## Post-Release Monitoring

- track GitHub issues for crash and conversion regressions
- tag releases with tested bundled-engine versions
- maintain a rolling compatibility matrix for newly added formats
