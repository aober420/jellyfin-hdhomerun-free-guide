# HDHomeRun Free Guide for Jellyfin 12.1

A community plugin built against Jellyfin 12.1.0 (.NET 10). No SiliconDust email, account password or paid subscription required. Requires a physical, supported HDHomeRun tuner with guide access and internet connectivity. SiliconDust controls available channels and the guide window (normally two days free).

## Install from Jellyfin

Add the hosted manifest URL under Dashboard → Plugins → Repositories, then install **HDHomeRun Free Guide** from Catalog and restart Jellyfin. Add your native HDHomeRun tuner under Live TV → Tuner Devices first. The startup task automatically downloads and registers a local XMLTV guide, then queues Jellyfin's Refresh Guide task. You can also run **Download HDHomeRun Free Guide** under Scheduled Tasks. Channels that Jellyfin cannot match may need initial channel mapping.

Optional additional tuner addresses are under Dashboard → Plugins → HDHomeRun Free Guide. There is no email field. Add each tuner to Live TV as well so the guide can be assigned to it.

The task checks hourly but downloads only when due, on a persisted randomized 20–28-hour interval. Every download reads current DeviceAuth from the tuners. Duplicate devices are deduplicated and multiple devices' keys are concatenated as specified by SiliconDust. Tokens are not stored in the guide configuration. XMLTV is fetched with gzip support and validated before atomically replacing the previous file. Existing unrelated guide sources remain configured. Disable another HDHomeRun guide plugin before enabling this one to avoid competing refreshes.

## Build and publish (maintainer only)

Run `pwsh ./package.ps1 -Repository OWNER/REPOSITORY`. Commit the source and generated manifest to the public repository's main branch. Create release `v1.0.0` and attach `dist/HDHomeRunFreeGuide_1.0.0.0.zip`. Users add `https://raw.githubusercontent.com/OWNER/REPOSITORY/main/manifest.json`; they do not need an SDK or manual build.

## Validation

Release build and automated tests cover XMLTV response validation, malformed/empty responses, DTD handling, external entity rejection, settings resource packaging, and startup scheduling. Not yet tested inside a running Jellyfin server or against a physical tuner. Version 12.1 compilation does not establish compatibility with other server versions.

Official guide API: https://github.com/Silicondust/documentation/wiki/XMLTV-Guide-Data
