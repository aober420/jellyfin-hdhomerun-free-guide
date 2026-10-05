using System.Net;
using FreeGuide;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.Tasks;
using Moq;
using Xunit;

public class ForceDownloadTests {
 [Fact] public void ForceTaskHasNoAutomaticTriggers(){
  var task=new ForceDownloadTask(null!,null!,null!);
  Assert.Empty(task.GetDefaultTriggers());
  Assert.NotEqual(new DownloadTask(null!,null!,null!).Key,task.Key);
 }
 [Theory][InlineData(false)][InlineData(true)]
 public async Task ForceBypassesScheduleAndFailurePreservesPreviousGuide(bool fail){
  var root=Path.Combine(Path.GetTempPath(),"freeguide-force-"+Guid.NewGuid());
  var folder=Path.Combine(root,"hdhomerun-free-guide");Directory.CreateDirectory(folder);
  var guide=Path.Combine(folder,"guide.xml");var schedule=Path.Combine(folder,"next-download.txt");
  const string original="<tv><channel id='old'/><programme channel='old'/></tv>";
  const string fresh="<tv><channel id='new'/><programme channel='new'/></tv>";
  var next=DateTimeOffset.UtcNow.AddHours(40).ToString("O");
  try {
   await File.WriteAllTextAsync(guide,original);await File.WriteAllTextAsync(schedule,next);
   var options=new LiveTvOptions{TunerHosts=[new(){Id="tuner1",Type="hdhomerun",Url="http://tuner.test"}],ListingProviders=[new(){Id="guide1",Type="xmltv",Path=guide}]};
   var manager=new Mock<IConfigurationManager>();manager.Setup(m=>m.GetConfiguration("livetv")).Returns(options);
   var paths=new Mock<IApplicationPaths>();paths.SetupGet(p=>p.DataPath).Returns(root);
   var tasks=new Mock<ITaskManager>();tasks.SetupGet(t=>t.ScheduledTasks).Returns([]);
   var handler=new FakeGuideHandler(fail,fresh);using var client=new HttpClient(handler);
   var task=new DownloadTask(manager.Object,paths.Object,tasks.Object);
   await task.ExecuteDownloadAsync(new Progress<double>(),CancellationToken.None,false,client);
   Assert.Equal(0,handler.Requests);
   if(fail){
    var error=await Assert.ThrowsAsync<InvalidOperationException>(()=>task.ExecuteDownloadAsync(new Progress<double>(),CancellationToken.None,true,client));
    Assert.Contains("HTTP 403",error.Message);Assert.DoesNotContain("test-secret",error.Message);
    Assert.Equal(original,await File.ReadAllTextAsync(guide));Assert.Equal(next,await File.ReadAllTextAsync(schedule));
   }else{
    await task.ExecuteDownloadAsync(new Progress<double>(),CancellationToken.None,true,client);
    Assert.Equal(fresh,await File.ReadAllTextAsync(guide));
    var updated=DateTimeOffset.Parse(await File.ReadAllTextAsync(schedule));
    Assert.InRange(updated-DateTimeOffset.UtcNow,TimeSpan.FromHours(19.9),TimeSpan.FromHours(28.1));
   }
   Assert.Equal(2,handler.Requests);
   Assert.True(handler.SawFreshAuth);
  }finally {Directory.Delete(root,true);}
 }
 private sealed class FakeGuideHandler(bool fail,string xml):HttpMessageHandler {
  public int Requests {get;private set;}
  public bool SawFreshAuth {get;private set;}
  protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){
   Requests++;
   if(request.RequestUri!.AbsolutePath=="/discover.json")return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent("{\"DeviceID\":\"tuner1\",\"DeviceAuth\":\"test-secret\"}")});
   SawFreshAuth=request.RequestUri.Query=="?DeviceAuth=test-secret";
   return Task.FromResult(new HttpResponseMessage(fail?HttpStatusCode.Forbidden:HttpStatusCode.OK){Content=new StringContent(fail?"Forbidden":xml)});
  }
 }
}
