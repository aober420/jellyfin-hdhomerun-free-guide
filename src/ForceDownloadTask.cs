using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Tasks;

namespace FreeGuide;

public sealed class ForceDownloadTask(IConfigurationManager configuration, IApplicationPaths paths, ITaskManager tasks) : IScheduledTask {
 public string Name => "Force Download HDHomeRun Free Guide";
 public string Key => "ForceDownloadHDHomeRunFreeGuide";
 public string Description => "Downloads fresh guide data immediately, including after an HDHomeRun channel scan, and refreshes Jellyfin's guide.";
 public string Category => "Live TV";
 public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() => [];
 public Task ExecuteAsync(IProgress<double> progress,CancellationToken cancellationToken) =>
  new DownloadTask(configuration,paths,tasks).ExecuteDownloadAsync(progress,cancellationToken,true);
}
