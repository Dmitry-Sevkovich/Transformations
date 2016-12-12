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
    internal class TransformationTests
    {
        [Test]
        public void ReplaceTokenTest()
        {
            const string text = "testText${testToken}MoreTestText";
            var dict = new Dictionary<string, string>
            {
                {"testToken", "ResultingText"}
            };
            var crawler = new Crawler();
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
            var crawler = new Crawler();
            Assert.Throws(Is.TypeOf<Exception>()
                    .And.Message.EqualTo("failingToken token is not defined in config files"),
                delegate { crawler.ReplaceTokens(text, dict); });
        }

#if !DEBUG
        [Ignore("This test runs in DEBUG mode only")]
#endif
        [Test]
        public void EnvironmentTest()
        {
            Assert.That(Crawler.GetEnvironment, Is.EqualTo("local"));
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
            Crawler crawler = new Crawler();

            Assert.That(textForLocal, Is.EqualTo(crawler.HandleLogic(text, "local")));
#if DEBUG
            Assert.That(textForLocal, Is.EqualTo(crawler.HandleLogic(text, Crawler.GetEnvironment)));
#endif
            Assert.That(textForDev, Is.EqualTo(crawler.HandleLogic(text, "dev")));



        }
    }
}
