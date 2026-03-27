# Privacy Policy

*Last updated: March 27, 2026*

Cofrox ("the App") is a native Windows application designed as a privacy-first,
fully on-device file converter for Windows.

## 1. Data We Do Not Collect

Cofrox does not collect, store, transmit, or share personal data or usage data.
Specifically:

- We do not collect your name, email address, or account information.
- We do not upload, inspect, or transmit the contents of files you convert.
- We do not include analytics, telemetry, advertising SDKs, or third-party
  tracking tools.
- We do not send conversion history, settings, or logs to a remote server.
- We do not require an internet connection for normal file conversion after the
  app is installed.

## 2. Data Stored Locally on Your Device

The App stores a limited amount of local-only data on your device so core
features can work:

- **Conversion history** - a record of recent conversions, including file names,
  source format, target format, time, status, and optional output path. File
  contents are not stored in the history database.
- **App preferences** - local settings such as theme, output folder, and
  quality defaults.
- **Temporary files** - short-lived intermediate files created during
  conversion. These are intended to live under `%TEMP%\Cofrox\` and are deleted
  automatically after conversion completes when cleanup succeeds.

## 3. Third-Party Conversion Components

Cofrox may invoke locally installed or bundled third-party conversion tools such
as FFmpeg, ImageMagick, Pandoc, 7-Zip, Ghostscript, or LibreOffice. When used
by Cofrox, these tools run locally on your device as child processes. Cofrox is
designed so that normal conversion workflows remain offline.

## 4. Microsoft Store and Public Distribution

For Microsoft Store or other public distribution channels, Cofrox will provide a
publicly accessible version of this Privacy Policy and an offline copy inside
the repository and application package. This document is intended to satisfy
that requirement.

## 5. Children's Privacy

Cofrox does not knowingly collect data from anyone, including children. Because
the App does not collect personal data at all, there is no child-specific
collection, profiling, or sharing activity.

## 6. Changes to This Policy

If this policy changes, the updated version will be committed to this
repository, included in the next app update, and dated at the top of this
document.

## 7. Contact

For privacy-related questions, open an issue in the Cofrox project repository.
