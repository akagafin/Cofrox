# Publishing Notes

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
- For Store-ready MSIX, add a packaging project in Visual Studio and point it at `src/Cofrox.App`.
- Keep the runtime target at `win-x64`.

## Verification Checklist

- Theme switches between Windows Light and Dark while the app is running.
- Mica is active on the main window on Windows 11.
- Drop zone accepts drag/drop and file picker import.
- Conversion queue remains responsive while jobs run.
- History persists to SQLite and can be cleared through the confirmation dialog.
- Test on Windows 10 build `17763` and Windows 11 build `22000+`.
