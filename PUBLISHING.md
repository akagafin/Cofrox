# Publishing Notes

## Build and automated tests

The repo includes `global.json` so the **.NET 8 SDK** (for example `8.0.419`)
is selected when multiple SDKs are installed. The WinUI app targets **x64**
only, so use `-p:Platform=x64` for CLI builds.

- **Libraries and tests** can be built and executed from the CLI.
- **WinUI (`Cofrox.App`)** still relies on the WinUI/XAML toolchain that is
  most reliably exercised from Visual Studio 2022 with the .NET desktop
  development workload and Windows App SDK support.

## Bundled tools

Default publish profiles do **not** redistribute external engines. This keeps
Store/default output legally safer. If you intentionally want an offline tool
bundle, use `PortableWithBundledTools` and audit the resulting package first.

Optional engine locations:

- `src/Cofrox.App/Tools/ffmpeg/ffmpeg.exe`
- `src/Cofrox.App/Tools/pandoc/pandoc.exe` or a versioned subfolder that
  contains `pandoc.exe`
- `src/Cofrox.App/Tools/imagemagick/magick.exe`
- `src/Cofrox.App/Tools/ghostscript/gswin64c.exe`
- `src/Cofrox.App/Tools/7zip/7z.exe`
- `src/Cofrox.App/Tools/libreoffice/program/soffice.exe` (optional)

## License and compliance notes

- The repo includes `LICENSE`, `NOTICE`, `THIRD_PARTY_LICENSES.txt`, and
  `FFMPEG_COMPLIANCE.md`.
- The FFmpeg binary currently present under `Tools/ffmpeg` is a GPL build, not
  an LGPL-only build. Review `FFMPEG_COMPLIANCE.md` before any public release.
- The Pandoc binary currently present under `Tools/pandoc` is GPL-2.0-or-later
  software and adds corresponding-source obligations if bundled in a release.
- ImageMagick's `LICENSE.txt` and `NOTICE.txt` should remain beside the shipped
  binaries.

## Packaging

- `WindowsPackageType=None` is enabled so the project can run unpackaged during
  development.
- Use the default `Portable` publish profile for a self-contained, no-tools
  build that is suitable as a baseline for Store packaging work.
- Use `PortableWithBundledTools` only for audited offline distributions.
- Complete the MSIX packaging flow in Visual Studio 2022 and validate the final
  package with the Windows App Certification Kit before submission.
- Keep the runtime target at `win-x64`.
- Review [STORE_COMPLIANCE.md](./STORE_COMPLIANCE.md) and
  [RELEASE_CHECKLIST.md](./RELEASE_CHECKLIST.md) before submission.

## Verification checklist

- Theme switches between Windows Light and Dark while the app is running.
- Mica is active on the main window on Windows 11.
- Drop zone accepts drag and drop and file picker import.
- Conversion queue remains responsive while jobs run.
- History persists to SQLite and can be cleared through the confirmation dialog.
- Test on Windows 10 build `17763` and Windows 11 build `22000+`.
