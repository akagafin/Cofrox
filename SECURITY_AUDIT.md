# Cofrox Security Audit

Updated: March 27, 2026

## Threat Model

Cofrox processes untrusted local files, constructs command-line arguments for
external converters, writes output files to user-selected locations, and stores
small amounts of local metadata. The main risks are:

- command and filter injection through external process execution
- unsafe file writes or overwrite behavior
- parser vulnerabilities in bundled third-party tools
- temp-file leakage or incomplete cleanup
- undocumented network egress introduced by future dependencies

## Findings

### 1. Manual argument construction remains the primary injection surface

Current state:

- External tools are launched with `UseShellExecute = false`.
- Standard output and error are redirected safely.
- Timeouts and process-tree termination are implemented.

Risk:

- Engine argument strings are still assembled manually.
- FFmpeg filter arguments are more complex than ordinary shell quoting.
- The current subtitle filter escaping only normalizes slashes and colons. It
  does not fully harden apostrophes and other FFmpeg filtergraph metacharacters.

Recommendation:

- Keep FFmpeg argument generation centralized in `IFFmpegCommandBuilder`.
- Add regression tests for spaces, apostrophes, commas, Unicode, and crafted
  subtitle paths.
- Prefer dedicated escaping helpers for filtergraph values over ad hoc string
  concatenation.

### 2. Output path handling is improved but still needs canonical validation

Current state:

- Output naming avoids silent overwrite in the queue UI layer.
- Temp work is isolated under `%TEMP%\\Cofrox\\`.

Risk:

- Output targets are not canonicalized and approved centrally before engine
  execution.
- Protected folders, network shares, or reparse-point-heavy paths may behave
  differently across machines and Store-sandboxed packaging scenarios.

Recommendation:

- Resolve and validate output paths centrally before handing them to engines.
- Reject output locations inside the app install directory.
- Add tests for junctions, UNC paths, and read-only targets.

### 3. The bundled ImageMagick policy is the default open policy

Current state:

- The repository includes ImageMagick's upstream `policy.xml`.

Risk:

- Resource limits, delegate restrictions, and unsafe coders are largely left in
  their permissive defaults.
- This increases exposure to malformed-image denial-of-service scenarios and to
  accidental delegate execution paths.

Recommendation:

- Harden `policy.xml` for production builds.
- Set realistic memory, disk, time, width, height, and list-length limits.
- Disable unused delegates and coders before broad release.

### 4. Third-party document parsers remain high-risk components

Risk profile:

- FFmpeg: medium, due to extensive codec and filter surface
- Pandoc: medium, due to broad document-format parsing surface
- Ghostscript: high if bundled later, due to AGPL and document parser attack
  history
- LibreOffice headless: medium, if bundled later
- ImageMagick: medium, until policy hardening is in place

Recommendation:

- Ship the minimum viable binary set.
- Track hashes and provenance for each bundled executable.
- Re-test converters when binaries are updated, not only when C# code changes.

### 5. Temp cleanup is functional but not exhaustive

Current state:

- The app cleans stale job directories under `%TEMP%\\Cofrox\\`.

Risk:

- Cleanup currently enumerates directories only. Top-level stray files in the
  temp root would remain behind if future code writes them there.

Recommendation:

- Extend cleanup to remove top-level files as well as per-job directories.

## Overall Assessment

Current posture:

- Acceptable for a local-first desktop beta with no network features.

Not yet sufficient for an aggressively distributed public release:

- FFmpeg filter escaping needs dedicated regression tests.
- Output-path validation should be centralized.
- ImageMagick policy hardening is still pending.
- Third-party binary provenance and update discipline need to be formalized.
