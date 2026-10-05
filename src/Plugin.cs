using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
namespace FreeGuide;
public class Settings : BasePluginConfiguration { public string TunerUrls { get; set; } = ""; }
public class Plugin : BasePlugin<Settings>, IHasWebPages {
 public static Plugin? Instance {get;private set;}
 public Plugin(IApplicationPaths paths, IXmlSerializer serializer):base(paths,serializer){Instance=this;}
 public override string Name => "HDHomeRun Free Guide";
 public override Guid Id => Guid.Parse("94079c51-6e85-4483-8b34-85b162638f75");
 public IEnumerable<PluginPageInfo> GetPages() => [new(){Name="freeguide",EmbeddedResourcePath="FreeGuide.config.html"}];
}
