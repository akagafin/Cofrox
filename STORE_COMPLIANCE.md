# Cofrox Microsoft Store Compliance

Updated: March 27, 2026

## Executive Summary

Cofrox is **not yet ready** for Microsoft Store publication.

The largest remaining blockers are:

1. No MSIX packaging project or verified single-project MSIX setup is present.
2. The current bundled FFmpeg binary is GPLv3-class, which is incompatible with
   the repo's simpler MIT-oriented release story unless corresponding-source
   obligations are fully implemented for that component.
3. A final Store-specific package identity, signing flow, and capability review
   still need to be completed on a clean Windows machine.
4. Windows App Certification Kit has not been run against a final MSIX artifact.

## Checklist

### 1. Packaging

- [ ] Add a Windows Application Packaging Project or switch to a verified
      single-project MSIX setup.
- [ ] Configure package identity, publisher, display name, logos, and signing.
- [ ] Produce x64 MSIX or MSIXBundle artifacts from a clean build machine.

### 2. Capabilities

Recommended baseline for Store submission:

- declare only the capabilities that are genuinely required
- avoid `broadFileSystemAccess` unless a strong approval case exists
- avoid unrelated restricted capabilities entirely
- expect `runFullTrust` for a packaged desktop app only if the final MSIX
  configuration requires it

Current repo review:

- no explicit restricted capabilities are declared today because no Store-ready
  package manifest is present
- file access is intended to rely on user-driven file and folder selection

### 3. Privacy

- [x] `PRIVACY_POLICY.md` exists in the repo
- [ ] publish a public privacy-policy URL for Partner Center submission
- [x] no analytics, ad SDKs, or remote telemetry are present in the current
      source tree
- [ ] verify the final Store listing points to the live privacy-policy URL

### 4. Third-Party Licenses

- [x] `LICENSE`, `NOTICE`, and `THIRD_PARTY_LICENSES.txt` now exist in the repo
- [x] align the in-app legal surface with the audited dependency inventory
- [ ] decide whether the Store package will ship FFmpeg and Pandoc binaries at
      all
- [ ] if FFmpeg remains bundled, archive exact corresponding source and build
      configuration for the shipped binary

### 5. Technical Compliance

- [ ] run the Windows App Certification Kit against the final MSIX
- [ ] confirm the package installs and launches on Windows 10 build `17763`
- [ ] confirm the package installs and launches on Windows 11 build `22000+`
- [ ] validate startup disclaimer, theme follow, drag and drop, history, temp
      cleanup, and offline behavior on packaged builds

## Submission Readiness Report

### What looks healthy

- WinUI 3 / Windows App SDK desktop architecture is Store-compatible in
  principle.
- No network stack or telemetry requirement is visible in the current source.
- Privacy policy, terms, disclaimer, and third-party notice files are present
  in the repository.

### What is still blocking publication

- No verified MSIX packaging path in the solution today.
- No WACK test evidence in the repo.
- Optional FFmpeg bundles still have GPLv3 obligations unless replaced with a
  verified LGPL-only build.
- Optional Pandoc bundles still add GPL redistribution obligations.
- Current ImageMagick policy is not yet hardened for a broad public release.

## Recommended Store Package Policy

For the first public Store release:

1. Keep the packaged app offline-first and telemetry-free.
2. Bundle only the minimum set of third-party tools required for the launch
   feature set.
3. Prefer an LGPL-only FFmpeg build, or remove FFmpeg from the first Store
   package if the legal/source-distribution workflow is not ready.
4. Consider making Pandoc an optional post-install component instead of part of
   the base Store package.
5. Keep Ghostscript out of the Store package unless a commercial or AGPL
   strategy is consciously adopted.
