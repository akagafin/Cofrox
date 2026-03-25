# Privacy Policy

*Last updated: March 25, 2026*

Cofrox ("the App") is a native Windows application designed as a privacy-first, fully on-device file converter.

## 1. Data We Do Not Collect

Cofrox does not collect, store, transmit, or share any personal data or usage data. Specifically:

- We do not collect your name, email address, or any identifying information.
- We do not collect, read, or transmit the files you convert or their contents.
- We do not use analytics, telemetry, crash reporting services, or any third-party tracking tools.
- We do not display advertisements.
- We do not connect to any remote server after the app is installed.

## 2. Data Stored Locally on Your Device

The following data is stored exclusively on your local device and is never transmitted anywhere:

- **Conversion history** — a log of recent conversions containing only file names and format pairs (not file contents), stored in a local SQLite database at `%LOCALAPPDATA%\Cofrox\history.db`.
- **App preferences** — your settings (theme, quality defaults, output folder path) stored via Windows ApplicationData.LocalSettings.
- **Temporary files** — intermediate files created during conversion, stored in `%TEMP%\Cofrox\` and automatically deleted after each conversion completes.

You can delete all locally stored data at any time from Settings → Storage → Clear All Data, or by uninstalling the application.

## 3. Third-Party Open Source Libraries

Cofrox uses open source libraries (FFmpeg, ImageMagick, Pandoc, and others) to perform file conversions. All of these libraries run entirely on your device. They do not independently connect to the internet or collect data when used within this application. A full list is available in Settings → Legal → Open Source Licenses.

## 4. Children's Privacy

This application does not knowingly collect data from anyone, including children. As stated above, no data is collected at all.

## 5. Changes to This Policy

If this policy is updated, the new version will be committed to the project's public GitHub repository and reflected in the next app update. The version date at the top of this document indicates when it was last changed.

## 6. Contact

For privacy-related questions, open an issue on the project's GitHub repository.
