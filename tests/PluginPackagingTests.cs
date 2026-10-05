using FreeGuide;
using MediaBrowser.Model.Tasks;
using Xunit;
public class PluginPackagingTests {
 [Fact] public void SettingsPageIsEmbedded(){Assert.Contains("FreeGuide.config.html",typeof(Plugin).Assembly.GetManifestResourceNames());}
 [Fact] public void ScheduleIncludesStartupAndHourlyChecks(){var task=new DownloadTask(null!,null!,null!);var triggers=task.GetDefaultTriggers().ToArray();Assert.Contains(triggers,t=>t.Type==TaskTriggerInfoType.StartupTrigger);Assert.Contains(triggers,t=>t.Type==TaskTriggerInfoType.IntervalTrigger&&t.IntervalTicks==TimeSpan.FromHours(1).Ticks);}
}
