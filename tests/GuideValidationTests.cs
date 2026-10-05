using FreeGuide;
using Xunit;
public class GuideValidationTests {
 [Fact] public void ValidGuideAccepted(){DownloadTask.ValidateGuide("<tv><channel id='2'/><programme channel='2' start='20261005000000 +0000'><title>News</title></programme></tv>");}
 [Theory][InlineData("<html>Sign in</html>")][InlineData("<tv><channel id='2'/></tv>")][InlineData("<tv><programme channel='2'/></tv>")][InlineData("")][InlineData("<tv>")]
 public void InvalidOrEmptyGuideRejected(string xml){Assert.ThrowsAny<Exception>(()=>DownloadTask.ValidateGuide(xml));}
 [Fact] public void StandardXmlTvDoctypeAcceptedWithoutFetchingExternalDtd(){DownloadTask.ValidateGuide("<!DOCTYPE tv SYSTEM 'http://127.0.0.1:1/unreachable.dtd'><tv><channel id='2'/><programme channel='2'/></tv>");}
 [Fact] public void ExternalEntitiesAreNotExpanded(){Assert.ThrowsAny<Exception>(()=>DownloadTask.ValidateGuide("<!DOCTYPE tv [<!ENTITY external SYSTEM 'file:///nonexistent'>]><tv><channel id='2'/><programme channel='2'><title>&external;</title></programme></tv>"));}
}
