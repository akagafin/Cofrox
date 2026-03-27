# Cofrox Performance Audit

## Audit Focus

- CPU usage during media conversion
- memory behavior on low-end devices
- queue throughput and parallelism
- UI responsiveness while background jobs run

## Current Strengths

- conversion work runs off the UI thread
- external processes are limited through `SemaphoreSlim`
- system profile detection reduces FFmpeg threads on low-memory devices
- external tools run at below-normal priority to protect desktop responsiveness
- temp cleanup keeps disk pressure lower across long sessions

## Bottlenecks Observed in Design Review

1. `MultimediaConversionEngine` previously mixed planning and execution.
   Result: harder to optimize and test.
   Action: moved planning into `FFmpegCommandBuilder`.

2. Queue state previously lived only in memory.
   Result: poor recovery after interruption.
   Action: added `IQueueManager` persistence.

3. Progress reporting previously used an invalid fixed denominator.
   Result: poor ETA and confidence.
   Action: parse actual media duration during FFmpeg startup.

## Performance Recommendations by Area

### Media

- probe hardware encoder availability before command generation
- keep `-threads 2` and `ultrafast` on low-memory machines
- consider `ffprobe` sidecar duration probing for more accurate ETA before encode start
- consider serializing heavy 4K/8K jobs even on high-end machines when temp disk is slow

### Images

- prefer streaming and dispose image objects aggressively
- cap huge raster dimensions before intermediate bitmap expansion
- apply resource limits for delegates such as Ghostscript

### Documents

- isolate LibreOffice headless work behind a dedicated concurrency lane
- avoid running more than one heavy Office-to-PDF conversion at once on 8 GB RAM devices

## Resource Policy

Recommended defaults:

| Device profile | Parallel jobs | FFmpeg threads | Notes |
|---|---|---|---|
| < 4 GB RAM | 1 | 2 | favor stability |
| 8-16 GB RAM | 2 | auto | default desktop target |
| > 16 GB RAM | 2 | auto | keep UI responsive over raw throughput |

## Remaining Work

1. Add benchmark fixtures for short clip, long clip, large image batch, and mixed document queue.
2. Capture peak working set and total temp-disk usage during stress tests.
3. Add UI telemetry-free diagnostic counters in debug builds only.

## Overall Assessment

The current architecture is in a better position for production tuning because planning, queue persistence, and execution are now separated. The next performance milestone should be measured profiling on real Windows 10 and Windows 11 hardware rather than more architecture-only changes.
