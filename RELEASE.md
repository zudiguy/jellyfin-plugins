# Creating a Release

This guide explains how to create a new release of the JellyTag plugin.

## Automatic Release Process

The repository is configured with GitHub Actions to automatically build and publish releases when you create a new tag.

### Steps to Create a Release

1. **Update the version** in these files:
   - `Jellyfin.Plugin.JellyTag/Jellyfin.Plugin.JellyTag.csproj` (change `<Version>`)
   - `build.yaml` (change `version`)
   - `manifest.json` (add new version entry)

2. **Commit your changes**:
   ```bash
   git add -A
   git commit -m "Release v1.0.0"
   git push
   ```

3. **Create and push a tag**:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```

4. **GitHub Actions will automatically**:
   - Build the plugin
   - Create a GitHub release
   - Upload the DLL file
   - Make it available in the manifest

## Manual Release Process (if needed)

If you need to create a release manually:

1. **Build the plugin**:
   ```bash
   ./build.sh
   ```

2. **Go to GitHub**:
   - Navigate to https://github.com/zudiguy/jellyfin-plugins/releases
   - Click "Draft a new release"

3. **Create the release**:
   - Tag: `v1.0.0` (create new tag)
   - Title: `JellyTag v1.0.0`
   - Description: Add changelog
   - Upload: `Jellyfin.Plugin.JellyTag/bin/Release/net9.0/Jellyfin.Plugin.JellyTag.dll`
   - Click "Publish release"

4. **Update manifest.json**:
   - Calculate SHA256 checksum:
     ```bash
     sha256sum Jellyfin.Plugin.JellyTag/bin/Release/net9.0/Jellyfin.Plugin.JellyTag.dll
     ```
   - Update the `checksum` field in manifest.json
   - Update the `sourceUrl` to point to the release asset
   - Commit and push the updated manifest

## Version Numbering

Follow semantic versioning: `MAJOR.MINOR.PATCH.BUILD`

- **MAJOR**: Breaking changes
- **MINOR**: New features (backwards compatible)
- **PATCH**: Bug fixes
- **BUILD**: Build number (usually 0)

Examples:
- Initial release: `1.0.0.0`
- Bug fix: `1.0.1.0`
- New feature: `1.1.0.0`
- Breaking change: `2.0.0.0`

## First Release

For your first release (v1.0.0), run:

```bash
git tag v1.0.0
git push origin v1.0.0
```

Then wait for GitHub Actions to complete (about 2-3 minutes).

After the release is created, you'll need to manually update the checksum in `manifest.json`:

1. Download the DLL from the release
2. Calculate SHA256: `sha256sum Jellyfin.Plugin.JellyTag.dll`
3. Update `manifest.json` with the checksum
4. Commit and push

After that, users can add your repository to Jellyfin!
