using FreeGuide;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.Tasks;
using Moq;
using Xunit;
public class CachedGuideTests {
 [Fact] public async Task CachedGuideRegistersProviderEvenWhenNextDownloadIsNotDue(){
  var root=Path.Combine(Path.GetTempPath(),"freeguide-test-"+Guid.NewGuid());
  Directory.CreateDirectory(Path.Combine(root,"hdhomerun-free-guide"));
  var guide=Path.Combine(root,"hdhomerun-free-guide","guide.xml");
  try {
   await File.WriteAllTextAsync(guide,"<tv><channel id='2'/><programme channel='2'/></tv>");
   await File.WriteAllTextAsync(Path.Combine(root,"hdhomerun-free-guide","next-download.txt"),DateTimeOffset.UtcNow.AddHours(24).ToString("O"));
   var options=new LiveTvOptions{TunerHosts=[new(){Id="tuner1",Type="hdhomerun",Url="http://127.0.0.1:1"}],ListingProviders=[new(){Id="other",Type="xmltv",Path="other.xml"}]};
   var manager=new Mock<IConfigurationManager>();manager.Setup(m=>m.GetConfiguration("livetv")).Returns(options);
   var paths=new Mock<IApplicationPaths>();paths.SetupGet(p=>p.DataPath).Returns(root);
   var tasks=new Mock<ITaskManager>();tasks.SetupGet(t=>t.ScheduledTasks).Returns([]);
   var task=new DownloadTask(manager.Object,paths.Object,tasks.Object);
   await task.ExecuteAsync(new Progress<double>(),CancellationToken.None);
   manager.Verify(m=>m.SaveConfiguration("livetv",options),Times.Once);
   Assert.Equal(2,options.ListingProviders.Length);
   var provider=Assert.Single(options.ListingProviders,p=>p.Path==guide);
   Assert.Equal("xmltv",provider.Type);Assert.Contains("tuner1",provider.EnabledTuners);
   await task.ExecuteAsync(new Progress<double>(),CancellationToken.None);
   manager.Verify(m=>m.SaveConfiguration("livetv",options),Times.Once);
  }finally {Directory.Delete(root,true);}
 }
}
