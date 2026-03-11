# JellyTag

A Jellyfin plugin that automatically overlays quality badges (resolution, HDR, codec, audio, language) on your media posters and thumbnails.

**Plugin Repository URL**: `https://raw.githubusercontent.com/zudiguy/jellyfin-plugins/main/manifest.json`

## Features

- **Automatic Badge Overlays**: Displays quality indicators directly on media posters
- **Multi-category Support**: Resolution, HDR formats, video codecs, audio codecs, source types
- **Customizable**: Adjust badge position, size, and styling
- **Per-library Filtering**: Enable/disable badges for specific libraries
- **Server-side Rendering**: Works across all Jellyfin clients without client-side configuration

## Key Modification: .strm File Support

This implementation has been specifically modified to support **`.strm` files** (streaming files). Since .strm files don't contain embedded metadata in Jellyfin, the plugin parses **filenames** to extract quality tags.

### Supported Filename Patterns

**Movies:**
```
The Amazing Spider-Man (2012) [Remux-2160p HEVC DV HDR10 10-bit TrueHD Atmos 7.1]-FraMeSToR.strm
```

**TV Shows:**
```
The BMF Documentary - Blowing Money Fast (2022) - S02E04 [AMZN WEBDL-1080p 8-bit h264 EAC3 5.1]-RAWR.strm
```

### Detected Metadata

The plugin automatically detects:
- **Resolution**: 4K, 2160p, 1080p, 720p, 480p, etc.
- **HDR Formats**: DV (Dolby Vision), HDR10+, HDR10, HDR, HLG
- **Video Codecs**: HEVC, H.264, H.265, x264, x265, AV1, VP9
- **Audio Codecs**: TrueHD, Atmos, DTS-HD MA, AAC, EAC3, AC3, FLAC
- **Audio Channels**: 7.1, 5.1, 2.0
- **Source**: Remux, WEB-DL, BluRay, HDTV, WEBRip
- **Streaming Service**: AMZN, DSNP, HMAX, NF, HULU, etc.

## Requirements

- Jellyfin 10.11.x or later
- .NET 9.0 runtime (bundled with Jellyfin 10.11+)

## Installation

### Plugin Repository (Recommended)

The easiest way to install JellyTag is through Jellyfin's plugin repository system:

1. In Jellyfin, go to **Dashboard → Plugins → Repositories**
2. Click the **+** button to add a new repository
3. Enter the following information:
   - **Repository Name**: `JellyTag`
   - **Repository URL**: `https://raw.githubusercontent.com/zudiguy/jellyfin-plugins/main/manifest.json`
4. Click **Save**
5. Go to **Dashboard → Plugins → Catalog**
6. Find **JellyTag** in the catalog
7. Click **Install**
8. Restart Jellyfin when prompted
9. Navigate to **Dashboard → Plugins → JellyTag** to configure

### Manual Installation

If you prefer to install manually:

1. Download the latest release DLL from the [Releases](https://github.com/zudiguy/jellyfin-plugins/releases) page
2. Place the DLL in your Jellyfin plugins folder:
   - Linux: `/var/lib/jellyfin/plugins/JellyTag/`
   - Windows: `%ProgramData%\Jellyfin\Server\plugins\JellyTag\`
   - macOS: `/Library/Application Support/jellyfin/plugins/JellyTag/`
3. Restart Jellyfin
4. Navigate to Dashboard → Plugins → JellyTag to configure

## Configuration

Access the plugin settings through:
**Dashboard → Plugins → JellyTag**

### Options

- **Enable Badges**: Master toggle for all badges
- **Badge Position**: TopLeft, TopRight, BottomLeft, BottomRight
- **Badge Size**: Adjust size percentage (50-200%)
- **Category Toggles**: Enable/disable specific badge types:
  - Resolution badges
  - HDR badges
  - Codec badges
  - Audio badges
  - Source badges
- **Excluded Libraries**: Select libraries to exclude from badge overlays

## Building from Source

### Prerequisites

- .NET 9.0 SDK
- Git

### Build Steps

```bash
# Clone the repository
git clone https://github.com/zudiguy/jellyfin-plugins.git
cd jellyfin-plugins/JellyTag

# Build the plugin
dotnet build --configuration Release

# The output DLL will be in:
# Jellyfin.Plugin.JellyTag/bin/Release/net9.0/Jellyfin.Plugin.JellyTag.dll
```

## How It Works

1. **HTTP Middleware Interception**: Intercepts image requests to Jellyfin
2. **Filename Parsing**: For .strm files, extracts metadata from the filename using regex patterns
3. **Badge Rendering**: Uses SkiaSharp to composite badge overlays onto images
4. **Caching**: Caches processed images locally for performance
5. **Client Delivery**: Serves modified images to all Jellyfin clients

## Filename Parsing Examples

The parser extracts metadata from square brackets `[...]` in filenames:

```
Input:  [Remux-2160p HEVC DV HDR10 10-bit TrueHD Atmos 7.1]
Output:
  - Source: Remux
  - Resolution: 4K
  - Video Codec: HEVC
  - HDR: DV, HDR10
  - Bit Depth: 10-bit
  - Audio: TrueHD Atmos
  - Channels: 7.1
```

```
Input:  [AMZN WEBDL-1080p 8-bit h264 EAC3 5.1]
Output:
  - Source Prefix: AMZN
  - Source Type: WEB-DL
  - Resolution: 1080p
  - Bit Depth: 8-bit
  - Video Codec: h264
  - Audio: EAC3
  - Channels: 5.1
```

## Troubleshooting

### Badges not appearing

1. Ensure the plugin is enabled in Dashboard → Plugins
2. Check that badges are enabled in plugin settings
3. Verify your filename contains metadata in square brackets `[...]`
4. Restart Jellyfin server
5. Clear browser cache

### Badges showing incorrect information

1. Check your filename format matches the supported patterns
2. Ensure metadata is within square brackets
3. Review the filename parsing patterns in the documentation

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Credits

- Original JellyTag concept and implementation: [Atilil/jellyfin-plugins](https://github.com/Atilil/jellyfin-plugins)
- Modified for .strm file support by: [zudiguy](https://github.com/zudiguy)
- Uses [SkiaSharp](https://github.com/mono/SkiaSharp) for image processing

## Support

For issues, questions, or feature requests, please open an issue on the [GitHub repository](https://github.com/zudiguy/jellyfin-plugins/issues).
