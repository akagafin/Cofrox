# FFmpeg Compliance Notes

Updated: March 27, 2026

## Current Audit Result

The FFmpeg binary currently bundled in this repository is **not** an LGPL-only
build.

Local verification of `ffmpeg.exe -buildconf` shows all of the following:

- `--enable-gpl`
- `--enable-version3`
- `--enable-libx264`
- `--enable-libx265`
- `--enable-libxvid`
- additional GPL-triggering and version3-triggering libraries

Under FFmpeg's own license documentation, `--enable-gpl` changes FFmpeg from
LGPL to GPL, and `--enable-version3` upgrades the legal terms to GPLv3 when the
binary is combined with version3-only or Apache-2.0-incompatible components.

## What That Means for Cofrox

1. The current bundled FFmpeg binary must not be described as "LGPL-only".
2. If Cofrox redistributes this exact binary, the FFmpeg component must be
   handled as GPL-3.0-or-later software.
3. Shipping the binary inside an MIT-licensed app repository does not remove
   the FFmpeg component's GPL obligations.
4. Patent obligations for codecs such as H.264, H.265/HEVC, AAC, and MP3 are
   separate from copyright licensing and still need business review.

## Recommended Release Strategy

### Preferred path for Microsoft Store simplicity

Replace the current bundled binary with a custom FFmpeg build that:

- disables all GPL components
- does not use `--enable-gpl`
- does not use `--enable-nonfree`
- avoids GPL-only external libraries such as `libx264`, `libx265`, and `libxvid`
- is distributed as a separate executable or DLL set that users can replace

This keeps the FFmpeg component in the LGPL posture instead of GPL.

### If you keep the current bundled binary

You must, at minimum:

- preserve the GPL license text
- provide the exact corresponding source for the exact shipped binary
- preserve and publish the exact build configuration used
- preserve notices and attributions in documentation or the app's legal notice
- review the EULA/Terms so they do not prohibit reverse engineering where the
  GPL requires otherwise

## Attribution Text

Use the following attribution if you continue redistributing FFmpeg:

> This product includes FFmpeg. The FFmpeg component in this distribution is
> licensed under the GNU General Public License, version 3 or later. The exact
> corresponding source code and build configuration for the shipped FFmpeg
> binary must be provided alongside this distribution.

If you switch to a verified LGPL-only build, replace the text above with:

> This product includes FFmpeg libraries and executables licensed under the GNU
> Lesser General Public License, version 2.1 or later. The exact corresponding
> source code and build configuration for the shipped FFmpeg binaries are
> provided with this distribution.

## Checklist

- [ ] Decide whether the Store package will ship FFmpeg at all.
- [ ] If yes, decide whether the release target is LGPL-only FFmpeg or GPLv3
      FFmpeg.
- [ ] Archive the exact upstream source tarball or git commit for the shipped
      binary.
- [ ] Archive the exact build configuration and build scripts.
- [ ] Preserve notices in app UI, repo, and packaged distribution.
- [ ] Verify codec patent exposure with business/legal review.
- [ ] Re-run `ffmpeg -buildconf` for the final release candidate and store the
      output in release artifacts.
