using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cofrox.App.Models;
using Cofrox.App.Services;
using Cofrox.Domain.Interfaces;

namespace Cofrox.App.ViewModels.Settings;

public sealed partial class LicensesViewModel : ObservableObject
{
    private readonly string _bundledToolsRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "Tools"));
    private readonly AppResourceService _appResourceService;
    private readonly ClipboardService _clipboardService;

    public ObservableCollection<LegalLibraryNotice> Notices { get; } = [];

    public string IntroText => _appResourceService.GetString("LegalLicensesIntro");

    public LicensesViewModel(
        AppResourceService appResourceService,
        ClipboardService clipboardService,
        IExternalToolLocator toolLocator)
    {
        _appResourceService = appResourceService;
        _clipboardService = clipboardService;
        AddManagedNotices(appResourceService);
        AddBundledToolNotices(appResourceService, toolLocator);
    }

    [RelayCommand]
    private void CopyAllNotices()
    {
        var content = string.Join(
            Environment.NewLine + Environment.NewLine + "----------------------------------------" + Environment.NewLine + Environment.NewLine,
            Notices.Select(
                static notice =>
                    $"{notice.Name} ({notice.Version}){Environment.NewLine}" +
                    $"License: {notice.LicenseType}{Environment.NewLine}" +
                    $"Copyright: {notice.Copyright}{Environment.NewLine}" +
                    (!string.IsNullOrWhiteSpace(notice.Note) ? $"{notice.Note}{Environment.NewLine}" : string.Empty) +
                    notice.LicenseText));

        _clipboardService.CopyText(content);
    }

    private void AddManagedNotices(AppResourceService resourceService)
    {
        AddNotice(CreateNotice("Microsoft.WindowsAppSDK", "1.5.240311000", "MIT", "Microsoft Corporation", "LegalNoticeTextMit", LegalBadgeKind.Permissive, resourceService));
        AddNotice(CreateNotice("CommunityToolkit.Mvvm", "8.4.0", "MIT", "Microsoft Corporation", "LegalNoticeTextMit", LegalBadgeKind.Permissive, resourceService));
        AddNotice(CreateNotice("Microsoft.Extensions.DependencyInjection", "8.0.0", "MIT", "Microsoft Corporation", "LegalNoticeTextMit", LegalBadgeKind.Permissive, resourceService));
        AddNotice(CreateNotice("Microsoft.Extensions.DependencyInjection.Abstractions", "8.0.0", "MIT", "Microsoft Corporation", "LegalNoticeTextMit", LegalBadgeKind.Permissive, resourceService));
        AddNotice(CreateNotice("Microsoft.Extensions.Hosting", "8.0.0", "MIT", "Microsoft Corporation", "LegalNoticeTextMit", LegalBadgeKind.Permissive, resourceService));
        AddNotice(CreateNotice("Microsoft.Data.Sqlite", "8.0.0", "MIT", "Microsoft Corporation", "LegalNoticeTextMit", LegalBadgeKind.Permissive, resourceService));
        AddNotice(CreateNotice("CsvHelper", "33.0.1", "MS-PL / Apache-2.0", "Josh Close", "LegalNoticeTextMsPlApache", LegalBadgeKind.Permissive, resourceService));
        AddNotice(CreateNotice("YamlDotNet", "15.1.2", "MIT", "Antoine Aubry", "LegalNoticeTextMit", LegalBadgeKind.Permissive, resourceService));
    }

    private void AddBundledToolNotices(AppResourceService resourceService, IExternalToolLocator toolLocator)
    {
        AddBundledToolNotice(
            toolLocator,
            "ffmpeg",
            "FFmpeg",
            "GPL/LGPL varies by bundled binary",
            "The FFmpeg Project",
            "LegalNoticeTextFfmpeg",
            LegalBadgeKind.Community,
            resourceService,
            "LegalLicenseNoteFfmpegApp");

        AddBundledToolNotice(
            toolLocator,
            "magick",
            "ImageMagick CLI",
            "ImageMagick License",
            "ImageMagick Studio LLC",
            "LegalNoticeTextImageMagick",
            LegalBadgeKind.Permissive,
            resourceService,
            "LegalLicenseNoteImageMagickApp");

        AddBundledToolNotice(
            toolLocator,
            "pandoc",
            "Pandoc",
            "GPL-2.0-or-later",
            "John MacFarlane and contributors",
            "LegalNoticeTextPandoc",
            LegalBadgeKind.Gpl,
            resourceService,
            "LegalLicenseNotePandocApp");

        AddBundledToolNotice(
            toolLocator,
            "7zip",
            "7-Zip CLI",
            "LGPL-2.1-or-later",
            "Igor Pavlov",
            "LegalNoticeTextSevenZip",
            LegalBadgeKind.Lgpl,
            resourceService,
            "LegalLicenseNoteSevenZipApp");

        AddBundledToolNotice(
            toolLocator,
            "libreoffice",
            "LibreOffice",
            "Multiple licenses",
            "The Document Foundation and contributors",
            "LegalNoticeTextLibreOffice",
            LegalBadgeKind.Community,
            resourceService,
            "LegalLicenseNoteLibreOfficeApp");

        AddBundledToolNotice(
            toolLocator,
            "ghostscript",
            "Ghostscript",
            "AGPL-3.0 or commercial",
            "Artifex Software Inc.",
            "LegalNoticeTextGhostscript",
            LegalBadgeKind.Agpl,
            resourceService,
            "LegalLicenseNoteGhostscriptApp");
    }

    private void AddBundledToolNotice(
        IExternalToolLocator toolLocator,
        string logicalName,
        string name,
        string licenseType,
        string copyright,
        string licenseTextKey,
        LegalBadgeKind badgeKind,
        AppResourceService resourceService,
        string? noteKey = null)
    {
        var bundledPath = ResolveBundledToolPath(toolLocator, logicalName);
        if (bundledPath is null)
        {
            return;
        }

        AddNotice(CreateNotice(
            name,
            ResolveVersion(bundledPath),
            licenseType,
            copyright,
            licenseTextKey,
            badgeKind,
            resourceService,
            noteKey));
    }

    private void AddNotice(LegalLibraryNotice notice)
    {
        Notices.Add(notice);
    }

    private string? ResolveBundledToolPath(IExternalToolLocator toolLocator, string logicalName)
    {
        var resolved = toolLocator.Resolve(logicalName);
        if (string.IsNullOrWhiteSpace(resolved))
        {
            return null;
        }

        var fullPath = Path.GetFullPath(resolved);
        var bundledRoot = _bundledToolsRoot.EndsWith(Path.DirectorySeparatorChar)
            ? _bundledToolsRoot
            : _bundledToolsRoot + Path.DirectorySeparatorChar;

        return fullPath.StartsWith(bundledRoot, StringComparison.OrdinalIgnoreCase)
            ? fullPath
            : null;
    }

    private static string ResolveVersion(string path)
    {
        var versionInfo = FileVersionInfo.GetVersionInfo(path);
        return !string.IsNullOrWhiteSpace(versionInfo.ProductVersion)
            ? versionInfo.ProductVersion
            : !string.IsNullOrWhiteSpace(versionInfo.FileVersion)
                ? versionInfo.FileVersion
                : "Bundled executable";
    }

    private static LegalLibraryNotice CreateNotice(
        string name,
        string version,
        string licenseType,
        string copyright,
        string licenseTextKey,
        LegalBadgeKind badgeKind,
        AppResourceService resourceService,
        string? noteKey = null) =>
        new()
        {
            Name = name,
            Version = version,
            LicenseType = licenseType,
            Copyright = copyright,
            LicenseText = resourceService.GetString(licenseTextKey),
            Note = string.IsNullOrWhiteSpace(noteKey) ? null : resourceService.GetString(noteKey),
            BadgeKind = badgeKind,
        };
}
