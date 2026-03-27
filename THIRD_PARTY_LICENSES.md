# Third-Party Licenses

This document is the GitHub-friendly companion to
[THIRD_PARTY_LICENSES.txt](./THIRD_PARTY_LICENSES.txt).

It lists the components that are actually referenced by the current Cofrox
source tree and/or bundled in the repository, the license posture for each, and
the main redistribution obligations.

## Highest-Risk Findings

1. The currently bundled FFmpeg binary is **GPL-3.0-or-later**, not LGPL-only.
2. The currently bundled Pandoc binary is **GPL-2.0-or-later**.
3. Ghostscript is **not bundled today**, but if bundled later it would create an
   **AGPL** compliance problem unless Cofrox adopts AGPL-compatible distribution
   for that component or obtains a commercial license.
4. The previously checked-in license inventory overstated some unused roadmap
   libraries and understated the license risk of the binaries that are actually
   bundled.

## Bundled in the Current Repository

| Component | Version observed | License | Purpose | Main obligations |
|---|---|---|---|---|
| FFmpeg | `N-123619-g44ad73031d-20260325` | Effective `GPL-3.0-or-later` for the bundled build | Media conversion and probing | Preserve GPL notices, provide exact corresponding source and build configuration, do not market this binary as LGPL-only |
| ImageMagick CLI | `7.1.2-18` | ImageMagick License | Image conversion backend | Preserve `LICENSE.txt` and `NOTICE.txt`, keep attribution to ImageMagick Studio LLC |
| Pandoc | `3.9.0.2` | `GPL-2.0-or-later` | Document conversion backend | Preserve GPL notices and make corresponding source available for the exact redistributed version |
| 7-Zip | `25.00` | `LGPL-2.1-or-later` plus BSD portions and unRAR restriction | Archive conversion backend | Preserve upstream license information and unRAR restriction notice |
| CommunityToolkit.Mvvm | `8.4.0` | MIT | MVVM toolkit | Preserve MIT notice |
| Microsoft.Extensions.DependencyInjection | `9.0.0` | MIT | Dependency injection | Preserve MIT notice |
| Microsoft.Extensions.DependencyInjection.Abstractions | `9.0.0` | MIT | DI abstractions | Preserve MIT notice |
| Microsoft.Extensions.Hosting | `9.0.0` | MIT | Generic host bootstrap | Preserve MIT notice |
| Microsoft.WindowsAppSDK | `1.5.240311000` | MIT | WinUI 3 / Windows App SDK runtime package | Preserve MIT notice |
| Microsoft.Data.Sqlite | `9.0.0` | MIT | SQLite provider for local history | Preserve MIT notice |
| CsvHelper | `33.0.1` | MS-PL or Apache-2.0 | CSV parsing/writing | Preserve upstream license notice |
| YamlDotNet | `15.1.2` | MIT | YAML parsing/serialization | Preserve MIT notice |

## Optional External Integrations Not Bundled Today

| Component | Status | Risk note |
|---|---|---|
| Ghostscript | Not present in `src/Cofrox.App/Tools` | Very high risk if bundled because the open-source release is AGPL |
| LibreOffice | Not present in `src/Cofrox.App/Tools` | Medium redistribution complexity; if bundled later, carry forward the exact notices from the chosen installer package |

## Recommended License Strategy for Cofrox

- Keep the Cofrox application code under the MIT License.
- Scope copyleft compliance to separately redistributed third-party executables
  and their notices.
- Before Microsoft Store publication, either:
  - replace FFmpeg with a verified LGPL-only build, or
  - ship the exact corresponding source for the current GPLv3 FFmpeg build.
- Strongly consider removing the bundled Pandoc binary from the default Store
  package unless GPL redistribution obligations are acceptable for that
  component.

## Related Files

- [LICENSE](./LICENSE)
- [NOTICE](./NOTICE)
- [THIRD_PARTY_LICENSES.txt](./THIRD_PARTY_LICENSES.txt)
- [FFMPEG_COMPLIANCE.md](./FFMPEG_COMPLIANCE.md)
