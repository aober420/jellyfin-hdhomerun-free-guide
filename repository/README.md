# HDHomeRun Free Guide plugin catalog

Add this URL under Jellyfin Dashboard → Plugins → Repositories:

```text
https://raw.githubusercontent.com/aober420/jellyfin-hdhomerun-free-guide/main/manifest.json
```

The catalog's version history is stored directly in `manifest.json`, newest first.

To publish a new version, update the C# project version and build with `dotnet build src/FreeGuide.csproj -c Release`. Package `FreeGuide.dll` and `FreeGuide.deps.json` at the ZIP root, then upload the ZIP to its GitHub release. Add an entry to `manifest.json` with the version, release notes, target ABI, download URL, MD5 checksum, and timestamp. Keep earlier entries and their original checksums.
