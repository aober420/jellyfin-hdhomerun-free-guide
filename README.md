# HDHomeRun Free Guide

A Jellyfin 12.1 plugin that downloads the free two-day HDHomeRun TV guide and keeps it updated. You'll need a physical HDHomeRun tuner. No SiliconDust account or subscription is needed.

## Installation

Add your HDHomeRun under **Dashboard → Live TV → Tuner Devices**.

Open **Dashboard → Plugins → Repositories** and add this repository URL:

```text
https://raw.githubusercontent.com/aober420/jellyfin-hdhomerun-free-guide/main/manifest.json
```

Install **HDHomeRun Free Guide** from the plugin catalog and restart Jellyfin.

The plugin downloads the guide on startup and adds it to Live TV. After that, it updates roughly once a day. You can run **Download HDHomeRun Free Guide** from **Scheduled Tasks** if you need to check it yourself. Some channels may need to be mapped in Jellyfin's guide settings.

If Jellyfin can't find your tuner, enter its address under **Plugins → HDHomeRun Free Guide**. Make sure it's also added under Live TV. Disable any other HDHomeRun guide plugin before using this one.

Built for Jellyfin 12.1.
