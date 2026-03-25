using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cofrox.App.Models;
using Cofrox.App.Services;

namespace Cofrox.App.ViewModels.Settings;

public sealed partial class LicensesViewModel(
    AppResourceService appResourceService,
    ClipboardService clipboardService) : ObservableObject
{
    public ObservableCollection<LegalLibraryNotice> Notices { get; } =
    [
        CreateNotice("FFmpeg", "6.x", "LGPL-2.1", "The FFmpeg Project", "LegalLicenseTextLgpl21", LegalBadgeKind.Lgpl, "LegalLicenseNoteFfmpegApp"),
        CreateNotice("Magick.NET (ImageMagick)", "13.x", "Apache-2.0", "Dirk Lemstra", "LegalLicenseTextApache20", LegalBadgeKind.Permissive),
        CreateNotice("LibRaw", "0.21.x", "LGPL-2.1", "LibRaw LLC", "LegalLicenseTextLgpl21", LegalBadgeKind.Lgpl),
        CreateNotice("Ghostscript", "10.x", "AGPL-3.0", "Artifex Software Inc.", "LegalLicenseTextAgpl30", LegalBadgeKind.Agpl, "LegalLicenseNoteGhostscriptApp"),
        CreateNotice("Pandoc", "3.x", "GPL-2.0", "John MacFarlane", "LegalLicenseTextGpl20", LegalBadgeKind.Gpl),
        CreateNotice("LibreOffice SDK", "24.x", "LGPL-2.1", "The Document Foundation", "LegalLicenseTextLgpl21", LegalBadgeKind.Lgpl),
        CreateNotice("Syncfusion Document SDK", "Community", "Syncfusion Community License", "Syncfusion Inc.", "LegalLicenseTextSyncfusionCommunity", LegalBadgeKind.Community),
        CreateNotice("7-Zip / LZMA SDK", "23.x", "LGPL-2.1", "Igor Pavlov", "LegalLicenseTextLgpl21", LegalBadgeKind.Lgpl),
        CreateNotice("SevenZipSharp", "latest", "LGPL-3.0", "Markovtsev Vadim", "LegalLicenseTextLgpl30", LegalBadgeKind.Lgpl),
        CreateNotice("ZstdNet", "latest", "BSD-3-Clause", "SKB Kontur", "LegalLicenseTextBsd3Clause", LegalBadgeKind.Permissive),
        CreateNotice("K4os.Compression.LZ4", "latest", "MIT", "Milosz Krajewski", "LegalLicenseTextMit", LegalBadgeKind.Permissive),
        CreateNotice("AssimpNet", "latest", "MIT", "Nicholas Woodfield", "LegalLicenseTextMit", LegalBadgeKind.Permissive),
        CreateNotice("CsvHelper", "latest", "MS-PL / Apache-2.0", "Josh Close", "LegalLicenseTextMsPlApache20", LegalBadgeKind.Permissive),
        CreateNotice("YamlDotNet", "latest", "MIT", "Antoine Aubry", "LegalLicenseTextMit", LegalBadgeKind.Permissive),
        CreateNotice("Newtonsoft.Json", "latest", "MIT", "James Newton-King", "LegalLicenseTextMit", LegalBadgeKind.Permissive),
        CreateNotice("CommunityToolkit.Mvvm", "latest", "MIT", "Microsoft Corporation", "LegalLicenseTextMit", LegalBadgeKind.Permissive),
        CreateNotice("Microsoft.Extensions.DependencyInjection", "latest", "MIT", "Microsoft Corporation", "LegalLicenseTextMit", LegalBadgeKind.Permissive),
        CreateNotice("Microsoft.Data.Sqlite", "latest", "MIT", "Microsoft Corporation", "LegalLicenseTextMit", LegalBadgeKind.Permissive),
        CreateNotice("Markdig", "latest", "BSD-2-Clause", "Alexandre Mutel", "LegalLicenseTextBsd2Clause", LegalBadgeKind.Permissive),
    ];

    public string IntroText => appResourceService.GetString("LegalLicensesIntro");

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

        clipboardService.CopyText(content);
    }

    private LegalLibraryNotice CreateNotice(
        string name,
        string version,
        string licenseType,
        string copyright,
        string licenseTextKey,
        LegalBadgeKind badgeKind,
        string? noteKey = null) =>
        new()
        {
            Name = name,
            Version = version,
            LicenseType = licenseType,
            Copyright = copyright,
            LicenseText = appResourceService.GetString(licenseTextKey),
            Note = string.IsNullOrWhiteSpace(noteKey) ? null : appResourceService.GetString(noteKey),
            BadgeKind = badgeKind,
        };
}
