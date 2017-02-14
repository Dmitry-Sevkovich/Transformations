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

        private static string _environment = GetEnvironment;

        private const string FileExtension = ".ttemplate";

        internal static string GetEnvironment {
            get
            {
                if (string.IsNullOrEmpty(_environment))
                {
                    return Path.GetFileNameWithoutExtension(Directory.GetFiles(
                        CurrentDir, "*.environment").FirstOrDefault());
                }
                return _environment;
            }
        }

        private static string _workingDir = CurrentDir;
        internal static string CurrentDir
        {
            get
            {
#if DEBUG
                return "C:\\Test";
#endif
                if (string.IsNullOrEmpty(_workingDir))
                {
                    return Directory.GetCurrentDirectory();
                }
                return _workingDir;
            }
        }
        internal void Crawl(string[] args)
        {
            if (args.Length >= 1 && !string.IsNullOrEmpty(args[0]))
            {
                _environment = args[0];
            }
            if (args.Length >= 2 && !string.IsNullOrEmpty(args[1]))
            {
                _workingDir = args[1];
            }

            try
            {
                _allPropertiesDict = PopulateDictionaryWithProperties();
                Traverse(CurrentDir);
            }
            catch (Exception ex)
            {
                File.WriteAllText("TransformationsErrorLog.txt", ex.Message + System.Environment.NewLine + ex.StackTrace);
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        internal Dictionary<string, string> PopulateDictionaryWithProperties() 
        {
            var configDir = $"{CurrentDir}\\config\\";
            XDocument globalDocument = XDocument.Load($"{configDir}global.properties");
            var allPropertiesGlobal = globalDocument.Root?.DescendantNodes().OfType<XElement>();
            var allItemsDictGlobal = allPropertiesGlobal?.ToDictionary(n => n.Attribute("name")?.Value,
                n => n.Attribute("value")?.Value);
            return PopulateDictionaryWithPropertiesHelper(allItemsDictGlobal, _environment);

        }

        private Dictionary<string, string> PopulateDictionaryWithPropertiesHelper(Dictionary<string, string> baseDictionary, string environment)
        {
            var configDir = $"{CurrentDir}\\config\\";
            XDocument localDocument = XDocument.Load($"{configDir}{environment}.properties");
            var allNodes = localDocument.Root?.DescendantNodes().OfType<XElement>();
            var allNodesArray = allNodes as XElement[] ?? allNodes.ToArray();
            var includeNodes = allNodesArray.Where(i => i.Name.ToString().ToLower() == "include");
            
            foreach (var includeNode in includeNodes)
            {
                baseDictionary = PopulateDictionaryWithPropertiesHelper(baseDictionary,
                        includeNode.Attribute("environment").Value);
            }
            var allPropertiesLocal = allNodesArray.Where(i => i.Name.ToString().ToLower() == "property");
            var allItemsDictLocal = allPropertiesLocal.ToDictionary(n => n.Attribute("name").Value,
                n => n.Attribute("value").Value);
            
            return baseDictionary.Concat(allItemsDictLocal).GroupBy(d => d.Key)
                    .ToDictionary(d => d.Key, d => d.Last().Value); ;

        }
        internal void Traverse(string currentDir)
        {
            var templateFiles = Directory.GetFiles(currentDir, $"*{FileExtension}");
            foreach (var templateFile in templateFiles)
            {
                var text = File.ReadAllText(templateFile);
                text = HandleLogic(text, _environment);
                var newFileName = templateFile.Remove(templateFile.Length - FileExtension.Length);
                text = ReplaceTokens(text);
                if (string.IsNullOrEmpty(text))
                {
                    Console.WriteLine($"{newFileName} has NOT been written, because there is nothing to write.");
                }
                else
                {
                    File.WriteAllText(newFileName, text);
                    Console.WriteLine($"{newFileName} has been written");
                }
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
            string originalText;
            do
            {
                originalText = text;
                text = dict.Aggregate(text,
                    (current, property) => current.Replace($"${{{property.Key}}}", property.Value));
            } while (originalText != text);
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
                environment = _environment;
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
