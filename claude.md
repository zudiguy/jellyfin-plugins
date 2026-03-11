# JellyTag - Recreation Project

## Project Overview
JellyTag is a Jellyfin plugin that automatically overlays quality badges (resolution, HDR, codec, audio, language) on media posters and thumbnails. The badges are rendered server-side through HTTP middleware, ensuring consistent appearance across all Jellyfin clients.

## What We're Building
A Jellyfin plugin that:
- Intercepts image requests via HTTP middleware
- Detects media metadata (resolution, HDR, codecs, audio formats, languages)
- Composites visual overlays using SkiaSharp
- Caches processed images locally
- Provides a configuration UI for customization

### Key Modification: .strm File Support
**IMPORTANT**: This implementation differs from the original. Since the library uses .strm files (streaming files) which don't contain embedded metadata in Jellyfin, we need to parse the **filename** to extract quality tags instead of reading from Jellyfin's media metadata.

Example filenames from user's library:

**Movies:**
```
The Amazing Spider-Man (2012) [Remux-2160p HEVC DV HDR10 10-bit TrueHD Atmos 7.1]-FraMeSToR.strm
```

**TV Shows:**
```
The BMF Documentary - Blowing Money Fast (2022) - S02E04 [AMZN WEBDL-1080p 8-bit h264 EAC3 5.1]-RAWR.strm
```

Parsing patterns:
- Pattern 1: `Title (Year) [Quality-Info]-ReleaseGroup.strm`
- Pattern 2: `Title (Year) - S##E## [Source Quality-Info]-ReleaseGroup.strm`
- Metadata within square brackets `[...]` separated by spaces/hyphens
- Multiple formats can coexist (DV + HDR10, AMZN WEBDL, etc.)
- Source can be prefixed (AMZN, DSNP, HMAX, etc.)

The plugin will parse these filenames to detect:
- **Resolution**: 2160p, 1080p, 720p, 480p, 4K, etc.
- **Source**: Remux, WEBDL/WEB-DL, BluRay, HDTV, WEBRip, AMZN, DSNP, HMAX, etc.
- **Video codecs**: HEVC, h264, H.264, H.265, x264, x265, AV1, VP9
- **HDR formats**: DV (Dolby Vision), HDR10+, HDR10, HDR, HLG
- **Bit depth**: 10-bit, 8-bit
- **Audio codecs**: TrueHD, Atmos, DTS-HD MA, DTS-HD HRA, AAC, AC3, EAC3, FLAC
- **Audio channels**: 7.1, 5.1, 2.0
- **Season/Episode**: S##E## (for tracking, not for badges)

## Key Features
- **Multi-category badge support**: Resolution, HDR, codecs, audio formats, language flags
- **Customizable badges**: Positioning, sizing, styling for both image and text formats
- **Per-library filtering**: Exclude specific content collections
- **Configuration management**: Export/import capabilities
- **Live preview**: Preview functionality in settings interface
- **Custom badge support**: Replace badges via UI or API endpoints

## Technical Requirements
- Jellyfin 10.11.x or later
- .NET 9.0 runtime (bundled with Jellyfin 10.11+)
- SkiaSharp for image processing

## Project Structure
```
JellyTag/
├── Jellyfin.Plugin.JellyTag/     # Core plugin implementation
│   ├── Configuration/            # Plugin configuration classes
│   ├── Middleware/               # HTTP middleware for image processing
│   ├── Services/                 # Badge rendering and caching services
│   ├── Api/                      # API controllers
│   └── Plugin.cs                 # Main plugin entry point
├── scripts/                      # Build and automation utilities
├── build.yaml                    # Build configuration
├── build.sh                      # Build script
└── README.md                     # Documentation
```

## Implementation Plan
1. Set up project structure and .NET plugin boilerplate
2. Implement configuration system
3. **Create filename parser for .strm files** ⚠️ Custom implementation
4. Create HTTP middleware for image interception
5. Build metadata detection service (using filename parser for .strm files)
6. Build badge rendering engine with SkiaSharp
7. Implement caching system
8. Create API endpoints for configuration
9. Build web UI for settings and preview
10. Add per-library filtering
11. Implement custom badge upload functionality
12. Testing and documentation

## Progress
- [x] Created claude.md documentation
- [x] Project structure setup
- [x] Configuration implementation
- [x] Middleware implementation (needs fix - see below)
- [x] Badge rendering engine (needs SkiaSharp API updates)
- [x] Caching system
- [x] Web UI (config.html)
- [x] Library filtering
- [x] FileNameParser for .strm files
- [x] GitHub repository setup
- [x] Plugin repository manifest (manifest.json)
- [x] GitHub Actions automated builds
- [x] First release (v1.0.0) - created but has bugs
- [ ] Fix middleware registration (v1.0.1)
- [ ] Update SkiaSharp APIs (v1.0.1)
- [ ] Test with actual .strm files
- [ ] Custom badge support (future enhancement)

## Filename Parsing Strategy
For .strm files, we'll use regex patterns to extract metadata from filenames.

### Parsing Logic:
1. Extract content within square brackets: `\[(.*?)\]`
2. Split by spaces/hyphens to get individual tags
3. Match against known patterns

### Patterns to Detect (case-insensitive):
1. **Resolution**: `(2160p|1080p|720p|576p|480p|360p|4k|8k)`
2. **Source Prefix**: `(AMZN|DSNP|HMAX|ATVP|PCOK|PMTP|NF|HULU)` (streaming service identifiers)
3. **Source Type**: `(Remux|REMUX|BluRay|Blu-ray|WEBDL|WEB-?DL|WEBRip|HDTV|SDTV|DVDRip|BDRip)`
4. **Video Codec**: `(HEVC|h264|h265|H\.?265|H\.?264|x265|x264|AV1|VP9|VC-?1)`
5. **HDR Formats**: `(DV|Dolby\.?Vision|HDR10\+|HDR10|HDR|HLG)`
6. **Bit Depth**: `(10-?bit|8-?bit)`
7. **Audio Codec**: `(TrueHD|Atmos|DTS-?HD\.?MA|DTS-?HD\.?HRA|DTS|AAC|AC-?3|E-?AC-?3|EAC3|FLAC|PCM|MP3)`
8. **Audio Channels**: `([0-9]\.[0-9])`
9. **Season/Episode**: `S([0-9]{2})E([0-9]{2})`

### Example Parses:

**Example 1 - Movie:**
```
Filename: The Amazing Spider-Man (2012) [Remux-2160p HEVC DV HDR10 10-bit TrueHD Atmos 7.1]-FraMeSToR.strm
Extract: "Remux-2160p HEVC DV HDR10 10-bit TrueHD Atmos 7.1"
Detected:
  - Source: Remux
  - Resolution: 2160p (4K)
  - Video Codec: HEVC
  - HDR: DV (Dolby Vision), HDR10
  - Bit Depth: 10-bit
  - Audio: TrueHD Atmos
  - Channels: 7.1
```

**Example 2 - TV Show:**
```
Filename: The BMF Documentary - Blowing Money Fast (2022) - S02E04 [AMZN WEBDL-1080p 8-bit h264 EAC3 5.1]-RAWR.strm
Extract: "AMZN WEBDL-1080p 8-bit h264 EAC3 5.1"
Detected:
  - Source Prefix: AMZN (Amazon Prime)
  - Source Type: WEBDL (WEB-DL)
  - Resolution: 1080p (Full HD)
  - Bit Depth: 8-bit
  - Video Codec: h264
  - Audio: EAC3 (E-AC3)
  - Channels: 5.1
```

### Implementation Approach:
- Create a `FileNameParser` service class
- Use compiled regex patterns for performance
- Extract metadata from square brackets `[...]`
- Support both bracket-based and standard scene naming
- Fall back to Jellyfin metadata if filename parsing fails
- Make patterns configurable via plugin settings

## Installation for Users

Users can install JellyTag directly from Jellyfin's plugin catalog:

1. In Jellyfin: **Dashboard → Plugins → Repositories**
2. Add repository: `https://raw.githubusercontent.com/zudiguy/jellyfin-plugins/main/manifest.json`
3. Go to **Catalog** and install **JellyTag**

## Creating the First Release

To make the plugin available for installation:

```bash
git tag v1.0.0
git push origin v1.0.0
```

GitHub Actions will automatically:
- Build the plugin
- Create a release
- Upload the DLL file

Then update `manifest.json` with the correct checksum and push.

See [RELEASE.md](RELEASE.md) for detailed instructions.

## Issues Found in v1.0.0 and Fixes for v1.0.1

### Issue #1: Middleware Not Properly Registered (CRITICAL)
**Problem**: The `ImageBadgeMiddleware` is registered in DI but not added to the HTTP pipeline. The plugin won't actually intercept image requests.

**Solution**: Create `PluginStartup.cs` implementing `IPluginStartup` to properly register middleware with `app.UseMiddleware<ImageBadgeMiddleware>()`.

**Status**: Fixing now

### Issue #2: Obsolete SkiaSharp APIs
**Problem**: Using deprecated SkiaSharp methods that may break in future:
- `SKPaint.TextSize` → Should use `SKFont.Size`
- `SKPaint.Typeface` → Should use `SKFont.Typeface`
- `SKPaint.MeasureText()` → Should use `SKFont.MeasureText()`
- `SKCanvas.DrawText()` → Needs updated signature with `SKFont`

**Solution**: Update `BadgeRenderer.cs` to use modern SkiaSharp APIs with `SKFont` instead of `SKPaint` for text rendering.

**Status**: Pending

### Issue #3: Build Warnings
**Problem**: Build succeeded but with 8 warnings about obsolete APIs.

**Solution**: Same as Issue #2 - update to modern APIs.

**Status**: Pending

## Release History

### v1.0.0 (2026-03-11)
- Initial release
- Known issues: Middleware not working, obsolete APIs
- **Not functional** - badges won't appear

### v1.0.1 (2026-03-11) ✅
- Fixed middleware registration using IStartupFilter
- Updated SkiaSharp to modern APIs
- **First working release**
- Checksum: aaca2eeb6dd711ca1b4219703e19ecab78beb102de56788844399b6247b7df6e

## Notes
- Original project: https://github.com/Atilil/jellyfin-plugins/tree/main/Jellytag
- License: MIT
- Recreation started: 2026-03-11
- **Modified**: Filename-based metadata extraction for .strm file support
- Repository: https://github.com/zudiguy/jellyfin-plugins
