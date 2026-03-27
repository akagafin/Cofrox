# Cofrox QA Report

## Scope

This report covers the current production-hardening pass for:

- architecture refactoring
- smart encoding orchestration
- queue durability
- FFmpeg command planning
- error handling and user-facing stability

## Automated Test Suite Added

### Unit tests

- `PresetManagerTests`
- `SmartConversionAdvisorTests`
- `QueueManagerTests`
- `FFmpegCommandBuilderTests`
- existing `FormatCatalogServiceTests`

### Integration and resilience scaffolding

The following integration scenarios were added as explicit skipped tests because they require bundled tools, fixture media, and local Windows hardware:

- large files above 10 GB
- corrupted files
- unsupported formats
- interrupted conversions
- concurrent jobs

## Coverage Baseline

Covered in code:

- preset persistence logic
- smart recommendation defaults
- queue pause/resume/retry state changes
- FFmpeg command argument mapping for low-memory and audio-normalization flows

Not yet covered by runnable automation in this sandbox:

- WinUI page-level UI behavior
- external engine integration against real bundled binaries
- MSIX packaging validation
- hardware acceleration matrix across NVIDIA, Intel, and AMD devices

## Critical Bug List

### Fixed in this pass

1. FFmpeg progress estimation used a hardcoded denominator equivalent to ten minutes.
   Impact: ETA and progress became misleading for almost every real file.
   Fix: progress now parses input duration and compares against `out_time_ms`.

2. Output files could silently overwrite previous conversions with the same name.
   Impact: user data loss and confusing results in repeated batch runs.
   Fix: output path generation now appends numeric suffixes and falls back to a GUID suffix after repeated collisions.

3. Queue state was not persisted independently from UI memory.
   Impact: no reliable retry/pause/resume audit trail and weak recovery story after interruption.
   Fix: queue snapshots are now stored in app settings through `IQueueManager`.

4. Unexpected engine exceptions could bubble without being normalized into conversion results.
   Impact: unstable UX during engine faults.
   Fix: coordinator now converts unexpected engine exceptions into failed results and persists the failure state.

### Still open

1. Queue snapshots are persisted but not yet rehydrated into the visible Home queue after app restart.
   Severity: medium

2. Hardware encoder options are selectable even if the current machine lacks that encoder runtime.
   Severity: medium

3. Integration fixtures and Store packaging tests still need execution on physical Windows 10 and Windows 11 targets.
   Severity: medium

## Manual Regression Matrix

Run before release:

| Area | Scenario | Expected |
|---|---|---|
| Queue | Add 20 mixed files and start convert all | UI stays responsive and no deadlock occurs |
| Cancel | Cancel long FFmpeg conversion mid-run | job ends as cancelled and temp files are cleaned |
| Error path | Convert corrupted input | failure is surfaced clearly, app does not crash |
| History | Complete and fail several jobs | SQLite history records both outcomes correctly |
| Theme | Change Windows theme while app is open | app updates without restart |
| Large files | Convert >10 GB media on low-memory device | warning shown and parallelism is constrained |

## Execution Note

NuGet restore is currently blocked in this sandbox by TLS/security-package failures against `api.nuget.org`, so these tests were authored and wired into the solution but not executed here. They should be run locally in Visual Studio or a Windows CI agent with working NuGet connectivity.
