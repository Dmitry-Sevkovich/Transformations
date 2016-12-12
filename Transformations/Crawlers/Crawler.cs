using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.RegularExpressions;
namespace Transformations.Crawlers
{
    internal class Crawler
    {
        private Dictionary<string, string> _allPropertiesDict;

        private static readonly string Environment = GetEnvironment;
        internal static string GetEnvironment => Path.GetFileNameWithoutExtension(Directory.GetFiles(CurrentDir, "*.environment").FirstOrDefault());
        public static string CurrentDir
        {
            get
            {
#if DEBUG
                return "C:\\Test";
#endif
                return Directory.GetCurrentDirectory();
            }
        }
        internal void Crawl()
        {
            var configDir = $"{CurrentDir}\\config\\";

            try
            {
                XDocument globalDocument = XDocument.Load($"{configDir}global.properties");
                var allPropertiesGlobal = globalDocument.Root?.DescendantNodes().OfType<XElement>();
                var allItemsDictGlobal = allPropertiesGlobal?.ToDictionary(n => n.Attribute("name")?.Value,
                    n => n.Attribute("value")?.Value);
                XDocument localDocument = XDocument.Load($"{configDir}{Environment}.properties");
                var allPropertiesLocal = localDocument.Root?.DescendantNodes().OfType<XElement>();
                var allItemsDictLocal = allPropertiesLocal.ToDictionary(n => n.Attribute("name").Value,
                    n => n.Attribute("value").Value);
                _allPropertiesDict =
                    allItemsDictGlobal.Concat(allItemsDictLocal)
                        .GroupBy(d => d.Key)
                        .ToDictionary(d => d.Key, d => d.Last().Value);
                Traverse(CurrentDir);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }
        internal void Traverse(string currentDir)
        {
            var templateFiles = Directory.GetFiles(currentDir, "*.ttemplate");
            foreach (var templateFile in templateFiles)
            {
                var text = File.ReadAllText(templateFile);
                text = HandleLogic(text, Environment);
                var newFileName = templateFile.Remove(templateFile.Length - 9);
                text = ReplaceTokens(text);
                File.WriteAllText(newFileName, text);
            }

            var directoryList = Directory.GetDirectories(currentDir);

            foreach (var directory in directoryList)
            {
                Traverse(directory);
            }
        }

        internal string ReplaceTokens(string text, Dictionary<string, string> dict = null )
        {
            if (dict == null)
            {
                dict = _allPropertiesDict;
            }
            text = dict.Aggregate(text, (current, property) => current.Replace($"${{{property.Key}}}", property.Value));
            var indexOfTokenBeginning = text.IndexOf("${", StringComparison.Ordinal);
            if (indexOfTokenBeginning < 0) return text;
            var textAfterTokenBeginning = text.Substring(indexOfTokenBeginning);
            var indexOfTokenEnd = textAfterTokenBeginning.IndexOf("}", StringComparison.Ordinal);
            if (indexOfTokenEnd < 0) return text;
            var token = textAfterTokenBeginning.Substring(2, indexOfTokenEnd - 2);
            throw new Exception($"{token} token is not defined in config files");
        }

        internal string HandleLogic(string text, string environment = null)
        {
            if (string.IsNullOrEmpty(environment))
            {
                environment = Environment;
            }
            Regex regexForIf = new Regex($"#if\\((([a-z0-9]*)\\|\\|)*{environment}((\\|\\|([a-z0-9]*)))*\\)(.*?)#endif({System.Environment.NewLine})?", RegexOptions.Singleline);
            Match match = regexForIf.Match(text);
            while (match.Success)
            {
                text = regexForIf.Replace(text, RemoveIfAndEndif(match.Value), 1);
                match = match.NextMatch();
            }
            regexForIf = new Regex($"#if\\((.*?)\\)(.*?)#endif({System.Environment.NewLine})?", RegexOptions.Singleline);
            text = regexForIf.Replace(text, "");
            return text;
        }

        internal string RemoveIfAndEndif(string text)
        {
            Regex r = new Regex("(?<=#if\\(.*\\)?\n)(.*?)(?=#endif)", RegexOptions.Singleline);
            Match match = r.Match(text);
            return match.Value;
        }
        
    }
}
