using System.Net;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.Tasks;
namespace FreeGuide;
public sealed class DownloadTask(IConfigurationManager configuration, IApplicationPaths paths, ITaskManager tasks) : IScheduledTask {
 private static readonly SemaphoreSlim Gate = new(1,1);
 public string Name => "Download HDHomeRun Free Guide";
 public string Key => "DownloadHDHomeRunFreeGuide";
 public string Description => "Downloads the free HDHomeRun XMLTV guide using fresh tuner authorization; checks hourly and downloads every 20–28 hours.";
 public string Category => "Live TV";
 public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() => [new(){Type=TaskTriggerInfoType.StartupTrigger},new(){Type=TaskTriggerInfoType.IntervalTrigger,IntervalTicks=TimeSpan.FromHours(1).Ticks}];
 public async Task ExecuteAsync(IProgress<double> progress,CancellationToken cancellationToken){
 await Gate.WaitAsync(cancellationToken);
 try {
 var folder=Path.Combine(paths.DataPath,"hdhomerun-free-guide"); Directory.CreateDirectory(folder);
 var guide=Path.Combine(folder,"guide.xml"); var schedule=Path.Combine(folder,"next-download.txt");
 if(File.Exists(guide)&&File.Exists(schedule)&&DateTimeOffset.TryParse(await File.ReadAllTextAsync(schedule,cancellationToken),out var next)&&next>DateTimeOffset.UtcNow){progress.Report(100);return;}
 var options=configuration.GetConfiguration<LiveTvOptions>("livetv");
 var urls=options.TunerHosts.Where(t=>string.Equals(t.Type,"hdhomerun",StringComparison.OrdinalIgnoreCase)).Select(t=>t.Url)
 .Concat((Plugin.Instance?.Configuration.TunerUrls??"").Split(',',StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries)).Where(u=>!string.IsNullOrWhiteSpace(u)).Distinct().ToArray();
 if(urls.Length==0)throw new InvalidOperationException("Add an HDHomeRun tuner in Live TV or enter its address in plugin settings.");
 using var client=new HttpClient(new HttpClientHandler{AutomaticDecompression=DecompressionMethods.GZip|DecompressionMethods.Deflate}){Timeout=TimeSpan.FromMinutes(3)};
 var auths=new SortedDictionary<string,string>(StringComparer.OrdinalIgnoreCase);
 foreach(var raw in urls){
 var baseUrl=raw.Contains("://",StringComparison.Ordinal)?raw:"http://"+raw;
 if(!Uri.TryCreate(baseUrl,UriKind.Absolute,out var uri)||(uri.Scheme!="http"&&uri.Scheme!="https"))throw new InvalidOperationException("Tuner addresses must use HTTP or HTTPS.");
 using var response=await client.GetAsync(baseUrl.TrimEnd('/')+"/discover.json",cancellationToken);
 if(!response.IsSuccessStatusCode)throw new InvalidOperationException("A configured tuner could not be reached. Previous guide retained.");
 using var doc=JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
 var root=doc.RootElement;
 if(!root.TryGetProperty("DeviceAuth",out var token)||string.IsNullOrWhiteSpace(token.GetString()))throw new InvalidOperationException("A configured tuner did not supply guide authorization.");
 var id=root.TryGetProperty("DeviceID",out var deviceId)?deviceId.GetString():baseUrl;
 auths[id??baseUrl]=token.GetString()!;
 }
 progress.Report(25);
 // Never log or persist the authorization URL. Fetch new keys for every download.
 var requestUrl="https://api.hdhomerun.com/api/xmltv?DeviceAuth="+Uri.EscapeDataString(string.Concat(auths.Values));
 string xml;
 try{using var response=await client.GetAsync(requestUrl,cancellationToken);if(!response.IsSuccessStatusCode)throw new InvalidOperationException("Guide service rejected the request. Previous guide retained.");xml=await response.Content.ReadAsStringAsync(cancellationToken);}
 catch(HttpRequestException){throw new InvalidOperationException("Guide service could not be reached. Previous guide retained.");}
 ValidateGuide(xml);
 progress.Report(70);
 var temporary=guide+".tmp";
 await File.WriteAllTextAsync(temporary,xml,cancellationToken);File.Move(temporary,guide,true);
 var provider=options.ListingProviders.FirstOrDefault(p=>string.Equals(p.Path,guide,StringComparison.OrdinalIgnoreCase));
 if(provider is null){options.ListingProviders=options.ListingProviders.Append(new ListingsProviderInfo{Id=Guid.NewGuid().ToString("N"),Type="xmltv",Path=guide,EnableAllTuners=false,EnabledTuners=options.TunerHosts.Where(t=>string.Equals(t.Type,"hdhomerun",StringComparison.OrdinalIgnoreCase)).Select(t=>t.Id).ToArray()}).ToArray();configuration.SaveConfiguration("livetv",options);}
 var worker=tasks.ScheduledTasks.FirstOrDefault(w=>w.ScheduledTask.Key=="RefreshGuide");
 if(worker is not null)tasks.QueueScheduledTask(worker.ScheduledTask,new TaskOptions());
 await File.WriteAllTextAsync(schedule,DateTimeOffset.UtcNow.AddHours(20+Random.Shared.NextDouble()*8).ToString("O"),cancellationToken);
 progress.Report(100);
 }finally{Gate.Release();}
 }
 public static void ValidateGuide(string xml){
 using var reader=XmlReader.Create(new StringReader(xml),new XmlReaderSettings{DtdProcessing=DtdProcessing.Ignore,XmlResolver=null,MaxCharactersInDocument=100_000_000});
 var document=XDocument.Load(reader);
 if(document.Root?.Name!="tv"||!document.Root.Elements("channel").Any()||!document.Root.Elements("programme").Any())throw new InvalidOperationException("Response did not contain an XMLTV guide with channels and programmes. Previous guide retained.");
 }
}

