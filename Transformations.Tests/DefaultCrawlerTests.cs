using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Transformations;
using Transformations.Crawlers;

namespace Transformations.Tests
{
    [TestFixture]
    internal class DefaultCrawlerTests
    {
        [Test]
        public void ReplaceTokenTest()
        {
            const string text = "testText${testToken}MoreTestText";
            var dict = new Dictionary<string, string>
            {
                {"testToken", "ResultingText"}
            };
            var crawler = new DefaultCrawler();
            Assert.That("testTextResultingTextMoreTestText", Is.EqualTo(crawler.ReplaceTokens(text, dict)));
        }

        [Test]
        public void ReplaceTokenExceptionThrow()
        {
            const string text = "testText${testToken}MoreTest${failingToken}Text";
            var dict = new Dictionary<string, string>
            {
                {"testToken", "ResultingText"}
            };
            var crawler = new DefaultCrawler();
            Assert.Throws(Is.TypeOf<Exception>()
                    .And.Message.EqualTo("failingToken token is not defined in config files"),
                delegate { crawler.ReplaceTokens(text, dict); });
        }

#if !DEBUG
        [Ignore("This test runs in DEBUG mode only")]
#endif
        [Test]
        //requires the file C:\Test\local.environment to exist
        public void EnvironmentTest()
        {
            Assert.That(DefaultCrawler.GetEnvironment, Is.EqualTo("local"));
        }

        [Test]
        public void LogicHandlingTest()
        {
            var text = "<term>" + System.Environment.NewLine + 
                       "	<tes name=\"${Global.Property1}\" value=\"FirstGlobalPropOverwriteTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Local.Property1}\" value=\"FirstLocalPropTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Local.Property2}\" value=\"SecondLocalPropTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Global.Property3}\" value=\"ThirdGlobalPropOverwriteTEST\"/>" + System.Environment.NewLine +
                       "#if(local)" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.local}\" value=\"This is local Environment\"/>" + System.Environment.NewLine +
                       "#endif" + System.Environment.NewLine +
                       "#if(dev)" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.dev}\" value=\"This is dev Environment\"/>" + System.Environment.NewLine +
                       "#endif" + System.Environment.NewLine +
                       "</term>";
            var textForLocal = "<term>" + System.Environment.NewLine +
                       "	<tes name=\"${Global.Property1}\" value=\"FirstGlobalPropOverwriteTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Local.Property1}\" value=\"FirstLocalPropTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Local.Property2}\" value=\"SecondLocalPropTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Global.Property3}\" value=\"ThirdGlobalPropOverwriteTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.local}\" value=\"This is local Environment\"/>" + System.Environment.NewLine +
                       "</term>";
            var textForDev = "<term>" + System.Environment.NewLine +
                       "	<tes name=\"${Global.Property1}\" value=\"FirstGlobalPropOverwriteTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Local.Property1}\" value=\"FirstLocalPropTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Local.Property2}\" value=\"SecondLocalPropTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Global.Property3}\" value=\"ThirdGlobalPropOverwriteTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.dev}\" value=\"This is dev Environment\"/>" + System.Environment.NewLine +
                       "</term>";
            DefaultCrawler crawler = new DefaultCrawler();

            Assert.That(textForLocal, Is.EqualTo(crawler.HandleLogic(text, "local")));
#if DEBUG
            Assert.That(textForLocal, Is.EqualTo(crawler.HandleLogic(text, DefaultCrawler.GetEnvironment)));
#endif
            Assert.That(textForDev, Is.EqualTo(crawler.HandleLogic(text, "dev")));
        }

        [Test]
        public void LogicHandlingTestForMultienvironmentalIfs()
        {
            var text = "<term>" + System.Environment.NewLine +
                       "	<tes name=\"${Global.Property1}\" value=\"FirstGlobalPropOverwriteTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Local.Property1}\" value=\"FirstLocalPropTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Local.Property2}\" value=\"SecondLocalPropTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Global.Property3}\" value=\"ThirdGlobalPropOverwriteTEST\"/>" + System.Environment.NewLine +
                       "#if(local)" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.local}\" value=\"This is local Environment\"/>" + System.Environment.NewLine +
                       "#endif" + System.Environment.NewLine +
                       "#if(dev||local)" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.devOrLocal}\" value=\"This is dev and local Environment\"/>" + System.Environment.NewLine +
                       "#endif" + System.Environment.NewLine +
                       "#if(local||dev)" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.localOrDev}\" value=\"This is local and dev Environment\"/>" + System.Environment.NewLine +
                       "#endif" + System.Environment.NewLine +
                       "#if(dev||local||auth)" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.devLocalAuth}\" value=\"This is dev, local and auth Environment\"/>" + System.Environment.NewLine +
                       "#endif" + System.Environment.NewLine +
                       "#if(auth||dev||local)" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.authDevLocal}\" value=\"This is auth, dev and local Environment\"/>" + System.Environment.NewLine +
                       "#endif" + System.Environment.NewLine +
                       "#if(dev)" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.dev}\" value=\"This is dev Environment\"/>" + System.Environment.NewLine +
                       "#endif" + System.Environment.NewLine +
                       "</term>";
            var textForLocal = "<term>" + System.Environment.NewLine +
                       "	<tes name=\"${Global.Property1}\" value=\"FirstGlobalPropOverwriteTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Local.Property1}\" value=\"FirstLocalPropTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Local.Property2}\" value=\"SecondLocalPropTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Global.Property3}\" value=\"ThirdGlobalPropOverwriteTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.local}\" value=\"This is local Environment\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.devOrLocal}\" value=\"This is dev and local Environment\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.localOrDev}\" value=\"This is local and dev Environment\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.devLocalAuth}\" value=\"This is dev, local and auth Environment\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.authDevLocal}\" value=\"This is auth, dev and local Environment\"/>" + System.Environment.NewLine +
                       "</term>";
            var textForDev = "<term>" + System.Environment.NewLine +
                       "	<tes name=\"${Global.Property1}\" value=\"FirstGlobalPropOverwriteTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Local.Property1}\" value=\"FirstLocalPropTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Local.Property2}\" value=\"SecondLocalPropTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Global.Property3}\" value=\"ThirdGlobalPropOverwriteTEST\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.devOrLocal}\" value=\"This is dev and local Environment\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.localOrDev}\" value=\"This is local and dev Environment\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.devLocalAuth}\" value=\"This is dev, local and auth Environment\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.authDevLocal}\" value=\"This is auth, dev and local Environment\"/>" + System.Environment.NewLine +
                       "	<tes name=\"${Environment.dev}\" value=\"This is dev Environment\"/>" + System.Environment.NewLine +
                       "</term>";
            DefaultCrawler crawler = new DefaultCrawler();

            Assert.That(textForLocal, Is.EqualTo(crawler.HandleLogic(text, "local")));
#if DEBUG
            Assert.That(textForLocal, Is.EqualTo(crawler.HandleLogic(text, DefaultCrawler.GetEnvironment)));
#endif
            Assert.That(textForDev, Is.EqualTo(crawler.HandleLogic(text, "dev")));
        }

        [Test]
        public void MultivariableTokenTest()
        {
            const string text = "testText${mostOutterToken}MoreTestText";
            var dict = new Dictionary<string, string>
            {
                {"testToken", "ResultingText"},
                {"outterToken", "Outter${testToken}Test" },
                {"mostOutterToken","Most${outterToken}Test" }
            };
            var crawler = new DefaultCrawler();
            Assert.That("testTextMostOutterResultingTextTestTestMoreTestText", Is.EqualTo(crawler.ReplaceTokens(text, dict)));
        }
    }
}
