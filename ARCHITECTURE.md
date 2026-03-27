# Cofrox Architecture

## Goals

Cofrox is designed to be:

- Universal: one desktop shell for media, image, document, archive, data, subtitle, font, and 3D conversions
- Beginner-friendly: safe defaults and guided presets
- Professional-grade: advanced FFmpeg-style control and queue orchestration
- Offline and private: all conversion work happens locally
- Microsoft Store ready: no restricted telemetry stack, no runtime network dependency after install

## Solution Structure

```text
/src
  /Cofrox.App
  /Cofrox.Application
  /Cofrox.Core
  /Cofrox.Domain
  /Cofrox.Data
  /Cofrox.Converters
/tests
  /Cofrox.Core.Tests
  /Cofrox.Application.Tests
```

## Layer Responsibilities

### `Cofrox.App`

- WinUI 3 shell, pages, components, and view models
- Navigation, theme, accent color, dialogs, drag and drop, and file picking
- Thin UI orchestration only

### `Cofrox.Application`

- `IPresetManager`: built-in and user-defined preset storage
- `ISmartConversionAdvisor`: goal-driven recommendation layer for quality, size, YouTube, and mobile targets
- `IFFmpegCommandBuilder`: central FFmpeg command planning and compatibility fallback logic
- `IQueueManager`: durable queue snapshot persistence for retry, pause, resume, and auditability

This layer is the main anti-corruption boundary between raw UI state and the conversion engines.

### `Cofrox.Core`

- Format catalog and compatibility matrix
- Option definitions for dynamic conversion controls
- Shared utilities such as format detection and file-size helpers

### `Cofrox.Domain`

- Entities: `FileItem`, `ConversionJob`, `ConversionResult`, `HistoryEntry`, `AppSettings`
- Interfaces for repositories, engines, coordinator, profile services, and tool runners
- Enums and immutable value objects

### `Cofrox.Data`

- SQLite history repository
- `ApplicationData` backed settings repository
- Temp folder management and cleanup
- Bundled tool discovery
- System profile detection

### `Cofrox.Converters`

- `ConversionCoordinator` for queue-safe engine execution and cleanup
- Plugin-like engine implementations through `IConversionEngine`
- External process runner with timeout, cancellation, and below-normal priority

## Plugin-Based Engine Model

All engines implement `IConversionEngine`:

- Validate input and output combinations
- Build or delegate execution plans
- Execute conversions
- Report progress
- Return a structured `ConversionResult`

Current engines:

1. `MultimediaConversionEngine`
2. `ImageConversionEngine`
3. `DocumentConversionEngine`
4. `ArchiveConversionEngine`
5. `DataConversionEngine`
6. `Model3DConversionEngine`
7. `SubtitleConversionEngine`
8. `FontConversionEngine`
9. `UnsupportedConversionEngine`

## Core Runtime Flow

1. UI creates a `ConversionJob`
2. `ConversionCoordinator` persists queue state and resolves the engine
3. The engine validates tool availability
4. Application services generate smart defaults, presets, and execution plans
5. Engine executes and reports progress
6. Coordinator writes history, updates queue state, and cleans temp files

## Key Production Improvements Added

- Introduced `Cofrox.Application` as an orchestration layer instead of keeping all planning logic inside engines or view models
- Moved FFmpeg planning into `FFmpegCommandBuilder` so it can be unit tested independently
- Added persistent queue snapshot support through `AppSettings.PersistentQueueStateJson`
- Added custom preset persistence through `AppSettings.CustomPresetsJson`
- Refactored `MultimediaConversionEngine` into a thin adapter that combines preset expansion, smart recommendation, command planning, and runtime execution
- Improved FFmpeg progress estimation by parsing duration plus processed microseconds instead of using a hardcoded ten-minute denominator
- Prevented silent output overwrite by generating collision-safe output filenames

## Recommended Next Phase

- Rehydrate queue snapshots back into the UI on startup
- Add a first-class simple/smart/advanced mode selector in the Home page
- Split engine capability metadata into discoverable plugins for future external packages
- Add hardware capability probing for NVENC, Quick Sync, and AMF availability before presenting those options as active
