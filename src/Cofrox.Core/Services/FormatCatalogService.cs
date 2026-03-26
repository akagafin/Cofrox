using Cofrox.Domain.Enums;
using Cofrox.Domain.Interfaces;
using Cofrox.Domain.ValueObjects;

namespace Cofrox.Core.Services;

public sealed class FormatCatalogService : IFormatCatalog
{
    private static readonly IReadOnlyDictionary<string, FormatDefinition> Formats =
        BuildFormats().ToDictionary(static item => item.Extension, StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyDictionary<string, string[]> CompatibilityMatrix =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["mp4"] = ["mkv", "avi", "mov", "webm", "gif", "mp3", "aac", "wav", "flac"],
            ["mkv"] = ["mp4", "avi", "mov", "webm", "mp3", "aac", "wav"],
            ["mp3"] = ["wav", "flac", "aac", "ogg", "m4a", "opus"],
            ["wav"] = ["mp3", "flac", "aac", "ogg", "m4a"],
            ["jpg"] = ["jpg", "png", "webp", "bmp", "tiff", "gif", "ico", "avif", "pdf"],
            ["jpeg"] = ["jpg", "png", "webp", "bmp", "tiff", "gif", "ico", "avif", "pdf"],
            ["png"] = ["jpg", "png", "webp", "bmp", "tiff", "gif", "ico", "avif", "pdf"],
            ["webp"] = ["jpg", "png", "bmp", "tiff", "gif", "ico", "avif", "pdf"],
            ["heic"] = ["jpg", "png", "webp", "avif"],
            ["heif"] = ["jpg", "png", "webp", "avif"],
            ["cr2"] = ["jpg", "png", "tiff", "dng"],
            ["nef"] = ["jpg", "png", "tiff", "dng"],
            ["arw"] = ["jpg", "png", "tiff", "dng"],
            ["pdf"] = ["docx", "txt", "html", "jpg", "png"],
            ["docx"] = ["pdf", "txt", "html", "odt", "rtf", "md", "epub"],
            ["xlsx"] = ["csv", "json", "pdf", "ods", "tsv", "xml"],
            ["json"] = ["csv", "xml", "yaml", "tsv", "xlsx"],
            ["xml"] = ["json", "yaml", "csv", "tsv"],
            ["yaml"] = ["json", "xml", "csv", "tsv"],
            ["zip"] = ["tar", "7z", "gz", "tar.gz"],
            ["7z"] = ["zip", "tar", "gz"],
            ["obj"] = ["stl", "gltf", "glb", "ply"],
            ["srt"] = ["vtt", "ass", "lrc"],
            ["ttf"] = ["otf", "woff", "woff2"],
            ["otf"] = ["ttf", "woff", "woff2"],
        };

    public IReadOnlyList<FormatDefinition> GetAllFormats() => Formats.Values.OrderBy(static item => item.Category).ThenBy(static item => item.DisplayName).ToArray();

    public FormatDefinition GetByExtension(string extension)
    {
        var normalized = NormalizeExtension(extension);
        return Formats.TryGetValue(normalized, out var format)
            ? format
            : new FormatDefinition("unknown", "Unknown", "Other", FileFamily.Unknown, "Format not yet recognized.");
    }

    public IReadOnlyList<FormatDefinition> GetTargets(string sourceExtension)
    {
        var normalized = NormalizeExtension(sourceExtension);
        if (!CompatibilityMatrix.TryGetValue(normalized, out var targets))
        {
            return [];
        }

        return targets.Select(GetByExtension).ToArray();
    }

    public IReadOnlyList<FormatOptionDefinition> GetOptions(string sourceExtension, string targetExtension)
    {
        var target = GetByExtension(targetExtension);
        return target.Family switch
        {
            FileFamily.Video => BuildVideoOptions(target.Extension),
            FileFamily.Audio => BuildAudioOptions(target.Extension),
            FileFamily.Image or FileFamily.RawImage => BuildImageOptions(target.Extension),
            FileFamily.Pdf => BuildPdfOptions(),
            FileFamily.Document or FileFamily.Ebook => BuildDocumentOptions(),
            FileFamily.Archive => BuildArchiveOptions(target.Extension),
            FileFamily.Data or FileFamily.Spreadsheet => BuildDataOptions(target.Extension),
            FileFamily.Model3D => BuildModelOptions(),
            FileFamily.Subtitle => BuildSubtitleOptions(),
            FileFamily.Font => BuildFontOptions(),
            _ => [],
        };
    }

    private static string NormalizeExtension(string extension) => extension.Trim().TrimStart('.').ToLowerInvariant();

    private static IReadOnlyList<FormatDefinition> BuildFormats() =>
    [
        Define("mp4", "MP4", "Video", FileFamily.Video, "MPEG-4 video container"),
        Define("mkv", "MKV", "Video", FileFamily.Video, "Matroska video container"),
        Define("avi", "AVI", "Video", FileFamily.Video, "Audio Video Interleave"),
        Define("mov", "MOV", "Video", FileFamily.Video, "QuickTime movie"),
        Define("webm", "WebM", "Video", FileFamily.Video, "Web media container"),
        Define("gif", "GIF", "Image", FileFamily.Image, "Animated or static GIF"),
        Define("mp3", "MP3", "Audio", FileFamily.Audio, "MPEG Layer III audio"),
        Define("aac", "AAC", "Audio", FileFamily.Audio, "Advanced Audio Coding"),
        Define("wav", "WAV", "Audio", FileFamily.Audio, "Waveform audio", true),
        Define("flac", "FLAC", "Audio", FileFamily.Audio, "Lossless audio", true),
        Define("ogg", "OGG", "Audio", FileFamily.Audio, "Ogg audio container"),
        Define("m4a", "M4A", "Audio", FileFamily.Audio, "MPEG-4 audio"),
        Define("opus", "Opus", "Audio", FileFamily.Audio, "Opus audio"),
        Define("jpg", "JPG", "Image", FileFamily.Image, "JPEG image"),
        Define("jpeg", "JPEG", "Image", FileFamily.Image, "JPEG image"),
        Define("png", "PNG", "Image", FileFamily.Image, "Portable Network Graphics", true),
        Define("webp", "WebP", "Image", FileFamily.Image, "Modern web image"),
        Define("bmp", "BMP", "Image", FileFamily.Image, "Bitmap image", true),
        Define("tiff", "TIFF", "Image", FileFamily.Image, "Tagged image file", true),
        Define("ico", "ICO", "Image", FileFamily.Image, "Windows icon"),
        Define("heic", "HEIC", "Image", FileFamily.Image, "High-efficiency image"),
        Define("heif", "HEIF", "Image", FileFamily.Image, "High-efficiency image file"),
        Define("avif", "AVIF", "Image", FileFamily.Image, "AV1 image file"),
        Define("cr2", "CR2", "RAW", FileFamily.RawImage, "Canon RAW image"),
        Define("nef", "NEF", "RAW", FileFamily.RawImage, "Nikon RAW image"),
        Define("arw", "RAW", "RAW", FileFamily.RawImage, "Sony RAW image"),
        Define("dng", "DNG", "RAW", FileFamily.RawImage, "Digital negative"),
        Define("pdf", "PDF", "Document", FileFamily.Pdf, "Portable Document Format"),
        Define("docx", "DOCX", "Document", FileFamily.Document, "Word document"),
        Define("doc", "DOC", "Document", FileFamily.Document, "Legacy Word document"),
        Define("txt", "TXT", "Document", FileFamily.Document, "Plain text"),
        Define("html", "HTML", "Document", FileFamily.Document, "HyperText Markup Language"),
        Define("md", "Markdown", "Document", FileFamily.Document, "Markdown document"),
        Define("odt", "ODT", "Document", FileFamily.Document, "OpenDocument text"),
        Define("rtf", "RTF", "Document", FileFamily.Document, "Rich text"),
        Define("epub", "EPUB", "Ebook", FileFamily.Ebook, "Electronic publication"),
        Define("xlsx", "XLSX", "Spreadsheet", FileFamily.Spreadsheet, "Excel workbook"),
        Define("xls", "XLS", "Spreadsheet", FileFamily.Spreadsheet, "Legacy Excel workbook"),
        Define("ods", "ODS", "Spreadsheet", FileFamily.Spreadsheet, "OpenDocument spreadsheet"),
        Define("csv", "CSV", "Data", FileFamily.Data, "Comma-separated values"),
        Define("tsv", "TSV", "Data", FileFamily.Data, "Tab-separated values"),
        Define("json", "JSON", "Data", FileFamily.Data, "JavaScript Object Notation"),
        Define("xml", "XML", "Data", FileFamily.Data, "Extensible Markup Language"),
        Define("yaml", "YAML", "Data", FileFamily.Data, "YAML Ain't Markup Language"),
        Define("yml", "YAML", "Data", FileFamily.Data, "YAML Ain't Markup Language"),
        Define("zip", "ZIP", "Archive", FileFamily.Archive, "Zip archive"),
        Define("7z", "7Z", "Archive", FileFamily.Archive, "7-Zip archive"),
        Define("tar", "TAR", "Archive", FileFamily.Archive, "Tape archive"),
        Define("tar.gz", "TAR.GZ", "Archive", FileFamily.Archive, "Gzipped tar archive"),
        Define("gz", "GZ", "Archive", FileFamily.Archive, "Gzip archive"),
        Define("obj", "OBJ", "3D", FileFamily.Model3D, "Wavefront object"),
        Define("stl", "STL", "3D", FileFamily.Model3D, "Stereolithography mesh"),
        Define("gltf", "glTF", "3D", FileFamily.Model3D, "GL transmission format"),
        Define("glb", "GLB", "3D", FileFamily.Model3D, "Binary glTF"),
        Define("ply", "PLY", "3D", FileFamily.Model3D, "Polygon file format"),
        Define("srt", "SRT", "Subtitle", FileFamily.Subtitle, "SubRip subtitle"),
        Define("vtt", "VTT", "Subtitle", FileFamily.Subtitle, "WebVTT subtitle"),
        Define("ass", "ASS", "Subtitle", FileFamily.Subtitle, "Advanced SubStation Alpha"),
        Define("lrc", "LRC", "Subtitle", FileFamily.Subtitle, "Lyric caption"),
        Define("ttf", "TTF", "Font", FileFamily.Font, "TrueType font"),
        Define("otf", "OTF", "Font", FileFamily.Font, "OpenType font"),
        Define("woff", "WOFF", "Font", FileFamily.Font, "Web Open Font Format"),
        Define("woff2", "WOFF2", "Font", FileFamily.Font, "Compressed web font"),
    ];

    private static FormatDefinition Define(string extension, string displayName, string category, FileFamily family, string description, bool isLossless = false) =>
        new(extension, displayName, category, family, description, isLossless);

    private static IReadOnlyList<FormatOptionDefinition> BuildVideoOptions(string targetExtension)
    {
        var codecs = targetExtension switch
        {
            "mp4" => ChoiceSet(("h264", "H.264"), ("h265", "H.265 / HEVC"), ("av1", "AV1")),
            "mkv" => ChoiceSet(("h264", "H.264"), ("h265", "H.265 / HEVC"), ("vp9", "VP9"), ("av1", "AV1")),
            "webm" => ChoiceSet(("vp8", "VP8"), ("vp9", "VP9"), ("av1", "AV1")),
            _ => ChoiceSet(("h264", "H.264"), ("mpeg4", "MPEG-4"), ("divx", "DivX")),
        };

        return
        [
            new("video_preset", "Preset (HandBrake-style)", "Quick profiles like HandBrake — pick Custom for full manual control.", OptionControlType.ComboBox, "custom", ChoiceSet(
                ("custom", "Custom (full control)"),
                ("hb_web_720p", "Web / Small file — 720p H.264"),
                ("hb_fast_1080p30", "Fast 1080p30 — H.264"),
                ("hb_hq_1080p30", "HQ 1080p30 — H.264"),
                ("hb_super_hq_1080p", "Super HQ 1080p — H.264"),
                ("hb_anime_1080p", "Animation 1080p — H.264"),
                ("hb_fast_4k", "Fast 4K — HEVC"),
                ("hb_hq_4k", "HQ 4K — HEVC"))),
            new("video_encoder", "Encoder", "Software (libx264/x265) or GPU encoders when available (same idea as HandBrake).", OptionControlType.ComboBox, "software", ChoiceSet(
                ("software", "Software (x264 / x265 / VP9 / AV1)"),
                ("nvenc", "NVIDIA NVENC (H.264 / HEVC / AV1)"),
                ("qsv", "Intel Quick Sync"),
                ("amf", "AMD AMF"))),
            new("encoding_speed", "Encoder preset (software)", "x264/x265 speed vs compression — like HandBrake’s Encoder Preset slider (software only).", OptionControlType.ComboBox, "medium", ChoiceSet(
                ("ultrafast", "Ultrafast"),
                ("fast", "Fast"),
                ("medium", "Medium"),
                ("slow", "Slow"),
                ("slower", "Slower"))),
            new("deinterlace", "Deinterlace", "Remove combing from interlaced sources (HandBrake Deinterlace).", OptionControlType.ComboBox, "none", ChoiceSet(
                ("none", "None"),
                ("yadif", "Yadif"),
                ("bwdif", "Bwdif"))),
            new("resolution", "Resolution", "Keep original or resize the output.", OptionControlType.ComboBox, "original", ChoiceSet(
                ("original", "Original"),
                ("4320p", "7680x4320 (8K)"),
                ("2160p", "3840x2160 (4K)"),
                ("1440p", "2560x1440 (2K)"),
                ("1080p", "1920x1080 (1080p)"),
                ("720p", "1280x720 (720p)"),
                ("480p", "854x480 (480p)"),
                ("360p", "640x360 (360p)"),
                ("custom", "Custom"))),
            new("video_codec", "Video Codec", "Choose the encoder for the target format.", OptionControlType.ComboBox, codecs[0].Key, codecs),
            new("quality_mode", "Video Quality Mode", "Switch between CRF quality and bitrate targeting.", OptionControlType.ComboBox, "crf", ChoiceSet(("crf", "CRF"), ("bitrate", "Target Bitrate"))),
            new("quality_value", "Video Quality", "Lower CRF gives higher quality and larger files.", OptionControlType.Slider, 23d, minimum: 0, maximum: 51, step: 1),
            new("target_bitrate", "Target Bitrate (kbps)", "Used when bitrate mode is selected.", OptionControlType.NumberBox, 2500d, minimum: 64, maximum: 50000, step: 50),
            new("frame_rate", "Frame Rate", "Control playback smoothness.", OptionControlType.ComboBox, "original", ChoiceSet(("original", "Original"), ("60", "60"), ("59.94", "59.94"), ("50", "50"), ("30", "30"), ("29.97", "29.97"), ("25", "25"), ("24", "24"), ("23.976", "23.976"), ("15", "15"))),
            new("audio_mode", "Audio", "Keep, re-encode, or remove the audio stream.", OptionControlType.ComboBox, "keep", ChoiceSet(("keep", "Keep Original"), ("reencode", "Re-encode"), ("remove", "Remove Audio"))),
            new("audio_codec", "Audio Codec", "Codec used when re-encoding embedded audio.", OptionControlType.ComboBox, "aac", ChoiceSet(("aac", "AAC"), ("mp3", "MP3"), ("ogg", "OGG"))),
            new("audio_bitrate", "Audio Bitrate (kbps)", "Applies when audio is re-encoded.", OptionControlType.ComboBox, "128", ChoiceSet(("96", "96 kbps"), ("128", "128 kbps"), ("192", "192 kbps"), ("320", "320 kbps"))),
            new("trim_start", "Trim Start", "Optional start timestamp in HH:MM:SS.mmm.", OptionControlType.Text, "", placeholder: "00:00:00.000"),
            new("trim_end", "Trim End", "Optional end timestamp in HH:MM:SS.mmm.", OptionControlType.Text, "", placeholder: "end"),
            new("rotate", "Rotate / Flip", "Rotate or flip the video output.", OptionControlType.ComboBox, "none", ChoiceSet(("none", "No rotation"), ("cw90", "Rotate 90° CW"), ("ccw90", "Rotate 90° CCW"), ("180", "Rotate 180°"), ("flip_h", "Flip Horizontal"), ("flip_v", "Flip Vertical"))),
            new("subtitle_path", "Subtitle Burn-In", "Optional SRT / ASS / VTT file path.", OptionControlType.Text, ""),
            new("speed", "Playback Speed", "Change speed from 0.25x to 4x.", OptionControlType.Slider, 1d, minimum: 0.25, maximum: 4, step: 0.05),
        ];
    }

    private static IReadOnlyList<FormatOptionDefinition> BuildAudioOptions(string targetExtension)
    {
        var codecChoices = targetExtension switch
        {
            "mp3" => ChoiceSet(("mp3", "LAME MP3")),
            "aac" => ChoiceSet(("aac", "AAC"), ("he-aac", "HE-AAC"), ("he-aac-v2", "HE-AAC v2")),
            "ogg" => ChoiceSet(("vorbis", "Vorbis"), ("opus", "Opus")),
            "m4a" => ChoiceSet(("aac", "AAC"), ("alac", "ALAC")),
            "wav" => ChoiceSet(("pcm16", "PCM 16-bit"), ("pcm24", "PCM 24-bit"), ("pcm32f", "PCM 32-bit float")),
            _ => ChoiceSet(("flac", "FLAC")),
        };

        return
        [
            new("audio_codec", "Audio Codec", "Codec used by the target container.", OptionControlType.ComboBox, codecChoices[0].Key, codecChoices),
            new("bitrate", "Bitrate", "Hidden for lossless outputs in the UI.", OptionControlType.ComboBox, "192", ChoiceSet(("320", "320 kbps"), ("256", "256 kbps"), ("192", "192 kbps"), ("128", "128 kbps"), ("96", "96 kbps"), ("64", "64 kbps"), ("32", "32 kbps"))),
            new("sample_rate", "Sample Rate", "Resample the output if needed.", OptionControlType.ComboBox, "original", ChoiceSet(("original", "Original"), ("48000", "48000 Hz"), ("44100", "44100 Hz"), ("32000", "32000 Hz"), ("22050", "22050 Hz"), ("16000", "16000 Hz"), ("8000", "8000 Hz"))),
            new("channels", "Channels", "Keep original layout or downmix.", OptionControlType.ComboBox, "original", ChoiceSet(("original", "Original"), ("2", "Stereo (2ch)"), ("1", "Mono (1ch)"), ("6", "5.1 Surround (6ch)"))),
            new("volume", "Volume (dB)", "Amplify or attenuate the output signal.", OptionControlType.Slider, 0d, minimum: -20, maximum: 20, step: 0.5),
            new("trim_start", "Trim Start", "Optional start timestamp in MM:SS.mmm.", OptionControlType.Text, "", placeholder: "00:00.000"),
            new("trim_end", "Trim End", "Optional end timestamp in MM:SS.mmm.", OptionControlType.Text, "", placeholder: "end"),
            new("normalize", "Normalize Audio", "Target streaming loudness around -14 LUFS.", OptionControlType.Toggle, false),
        ];
    }

    private static IReadOnlyList<FormatOptionDefinition> BuildImageOptions(string targetExtension)
    {
        var qualityLabel = targetExtension is "png"
            ? "Compression Level"
            : "Quality";
        var qualityMaximum = targetExtension is "png" ? 9 : 100;
        var qualityDefault = targetExtension is "png" ? 6d : 85d;

        return
        [
            new("quality", qualityLabel, "Compression or quality depending on output format.", OptionControlType.Slider, qualityDefault, minimum: 0, maximum: qualityMaximum, step: 1),
            new("resize_mode", "Resize", "Resize by percentage, bounds, or explicit size.", OptionControlType.ComboBox, "none", ChoiceSet(("none", "No resize"), ("percentage", "By percentage"), ("dimensions", "By dimensions"), ("fit_within", "Fit within"), ("fit_width", "Fit to width"), ("fit_height", "Fit to height"))),
            new("width", "Width", "Width value for dimension-based resizing.", OptionControlType.NumberBox, 1920d, minimum: 1, maximum: 20000, step: 1),
            new("height", "Height", "Height value for dimension-based resizing.", OptionControlType.NumberBox, 1080d, minimum: 1, maximum: 20000, step: 1),
            new("resample_filter", "Resample Filter", "Choose the resize interpolation strategy.", OptionControlType.ComboBox, "lanczos", ChoiceSet(("lanczos", "Lanczos"), ("bicubic", "Bicubic"), ("bilinear", "Bilinear"), ("nearest", "Nearest Neighbor"))),
            new("color_space", "Color Space", "Adjust the output color model.", OptionControlType.ComboBox, "original", ChoiceSet(("original", "Original"), ("rgb", "RGB"), ("rgba", "RGBA"), ("grayscale", "Grayscale"), ("cmyk", "CMYK"))),
            new("rotate", "Rotate", "Rotate or auto-orient the output.", OptionControlType.ComboBox, "none", ChoiceSet(("none", "No rotation"), ("cw90", "90° CW"), ("ccw90", "90° CCW"), ("180", "180°"), ("auto", "Auto (EXIF)"))),
            new("strip_metadata", "Strip Metadata", "Remove EXIF and device metadata.", OptionControlType.Toggle, false),
            new("dpi", "DPI", "Target raster DPI for screen or print.", OptionControlType.ComboBox, "72", ChoiceSet(("72", "72"), ("96", "96"), ("150", "150"), ("300", "300"), ("600", "600"))),
            new("background", "Background Color", "Used when flattening transparency to opaque formats.", OptionControlType.Color, "#FFFFFFFF"),
        ];
    }

    private static IReadOnlyList<FormatOptionDefinition> BuildPdfOptions() =>
    [
        new("page_size", "Page Size", "Keep source size or normalize output pages.", OptionControlType.ComboBox, "original", ChoiceSet(("original", "Original"), ("A4", "A4"), ("A3", "A3"), ("A5", "A5"), ("Letter", "Letter"), ("Legal", "Legal"), ("custom", "Custom"))),
        new("orientation", "Orientation", "Auto-rotate or force page orientation.", OptionControlType.ComboBox, "auto", ChoiceSet(("auto", "Auto"), ("portrait", "Portrait"), ("landscape", "Landscape"))),
        new("image_quality", "Image Quality", "Compression quality for embedded images.", OptionControlType.Slider, 85d, minimum: 1, maximum: 100, step: 1),
        new("pdf_version", "PDF Version", "Choose compatibility or archival profile.", OptionControlType.ComboBox, "1.7", ChoiceSet(("1.7", "PDF 1.7"), ("2.0", "PDF 2.0"), ("pdfa-1b", "PDF/A-1b"), ("pdfa-2b", "PDF/A-2b"), ("pdfx-1a", "PDF/X-1a"))),
        new("open_password", "Open Password", "Optional password required to open the PDF.", OptionControlType.Password, ""),
        new("edit_password", "Edit Password", "Optional password required to edit the PDF.", OptionControlType.Password, ""),
        new("compress_pdf", "Compress PDF", "Apply maximum output compression.", OptionControlType.Toggle, true),
    ];

    private static IReadOnlyList<FormatOptionDefinition> BuildDocumentOptions() =>
    [
        new("encoding", "Encoding", "Character encoding for text-based outputs.", OptionControlType.ComboBox, "utf-8", ChoiceSet(("utf-8", "UTF-8"), ("utf-16", "UTF-16"), ("latin1", "Latin-1"), ("ascii", "ASCII"))),
        new("line_endings", "Line Endings", "Preferred newline style for text outputs.", OptionControlType.ComboBox, "crlf", ChoiceSet(("crlf", "Windows (CRLF)"), ("lf", "Unix (LF)"), ("cr", "Mac (CR)"))),
        new("include_images", "Include Images", "Embed or skip document images.", OptionControlType.Toggle, true),
        new("toc", "Table of Contents", "Generate a table of contents when headings exist.", OptionControlType.Toggle, false),
    ];

    private static IReadOnlyList<FormatOptionDefinition> BuildArchiveOptions(string targetExtension)
    {
        var methods = targetExtension switch
        {
            "zip" => ChoiceSet(("deflate", "Deflate"), ("store", "Store"), ("lzma", "LZMA"), ("bzip2", "BZip2")),
            "7z" => ChoiceSet(("lzma2", "LZMA2"), ("lzma", "LZMA"), ("ppmd", "PPMd"), ("bzip2", "BZip2"), ("deflate", "Deflate")),
            "tar.gz" => ChoiceSet(("gzip", "Gzip")),
            "tar.bz2" => ChoiceSet(("bzip2", "BZip2")),
            "tar.xz" => ChoiceSet(("xz", "XZ / LZMA")),
            "zst" => ChoiceSet(("zstd", "Zstandard")),
            _ => ChoiceSet(("default", "Default")),
        };

        return
        [
            new("compression_level", "Compression Level", "Trade speed for smaller output.", OptionControlType.ComboBox, "normal", ChoiceSet(("store", "No compression"), ("fastest", "Fastest"), ("fast", "Fast"), ("normal", "Normal"), ("maximum", "Maximum"), ("ultra", "Ultra"))),
            new("compression_method", "Compression Method", "Algorithm used by the archive.", OptionControlType.ComboBox, methods[0].Key, methods),
            new("split_size", "Split Archive", "Maximum size per part in MB.", OptionControlType.NumberBox, 0d, minimum: 0, maximum: 102400, step: 1),
            new("password", "Password", "Optional archive encryption password.", OptionControlType.Password, ""),
            new("include_subfolders", "Include Subfolders", "Recurse into child folders when archiving folders.", OptionControlType.Toggle, true),
            new("sfx", "Self-Extracting Archive", "Available for 7Z targets.", OptionControlType.Toggle, false),
        ];
    }

    private static IReadOnlyList<FormatOptionDefinition> BuildDataOptions(string targetExtension) =>
    [
        new("indentation", "Indentation", "Formatting style for JSON or XML output.", OptionControlType.ComboBox, "2", ChoiceSet(("2", "2 spaces"), ("4", "4 spaces"), ("tab", "Tab"), ("minified", "Minified"))),
        new("delimiter", "CSV Delimiter", "Delimiter used when generating CSV-like output.", OptionControlType.ComboBox, targetExtension is "tsv" ? "tab" : "comma", ChoiceSet(("comma", "Comma"), ("semicolon", "Semicolon"), ("tab", "Tab"), ("pipe", "Pipe"))),
        new("csv_encoding", "CSV Encoding", "Choose the text encoding for CSV output.", OptionControlType.ComboBox, "utf8-bom", ChoiceSet(("utf8-bom", "UTF-8 with BOM"), ("utf8", "UTF-8 without BOM"), ("latin1", "Latin-1"))),
        new("header_row", "Include Header Row", "Emit column names in CSV output.", OptionControlType.Toggle, true),
        new("array_handling", "Array Handling", "How nested JSON arrays are flattened to rows.", OptionControlType.ComboBox, "flatten", ChoiceSet(("flatten", "Flatten nested objects"), ("skip", "Skip nested"), ("first-level", "First level only"))),
    ];

    private static IReadOnlyList<FormatOptionDefinition> BuildModelOptions() =>
    [
        new("scale", "Scale", "Uniform scale factor for the export.", OptionControlType.NumberBox, 1d, minimum: 0.001, maximum: 1000, step: 0.01),
        new("axis_up", "Axis Up", "Target up-axis for the exported model.", OptionControlType.ComboBox, "y", ChoiceSet(("y", "Y-up"), ("z", "Z-up"), ("x", "X-up"))),
        new("include_materials", "Include Materials", "Export materials and textures when supported.", OptionControlType.Toggle, true),
        new("include_animations", "Include Animations", "Export animation data when available.", OptionControlType.Toggle, true),
        new("merge_meshes", "Merge Meshes", "Combine all meshes into a single output.", OptionControlType.Toggle, false),
    ];

    private static IReadOnlyList<FormatOptionDefinition> BuildSubtitleOptions() =>
    [
        new("fps", "Frame Rate (FPS)", "Used for frame-based subtitle formats.", OptionControlType.NumberBox, 23.976d, minimum: 1, maximum: 240, step: 0.001),
        new("offset_ms", "Offset / Shift (ms)", "Positive values delay subtitles, negative values advance them.", OptionControlType.NumberBox, 0d, minimum: -600000, maximum: 600000, step: 50),
        new("encoding", "Encoding", "Choose the subtitle file encoding.", OptionControlType.ComboBox, "utf-8", ChoiceSet(("utf-8", "UTF-8"), ("utf-16", "UTF-16"), ("latin1", "Latin-1"), ("cp1252", "Windows-1252"))),
    ];

    private static IReadOnlyList<FormatOptionDefinition> BuildFontOptions() =>
    [
        new("hinting", "Preserve Hinting", "Keep hinting tables when possible.", OptionControlType.Toggle, true),
        new("subset", "Subset", "Optional comma-separated Unicode ranges.", OptionControlType.Text, ""),
    ];

    private static OptionChoice[] ChoiceSet(params (string Key, string Label)[] values) =>
        values.Select(static value => new OptionChoice(value.Key, value.Label)).ToArray();
}
