using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transformations.Crawlers;

namespace Transformations
{
    class Transform
    {
        static void Main(string[] args)
        {
            var crawlerClassName = ConfigurationManager.AppSettings["Crawler"];
            var crawlerType = Type.GetType(crawlerClassName);
            if (crawlerType != null)
            {
                var crawler = Activator.CreateInstance(crawlerType) as ICrawler;
                crawler.Crawl(args);
            }
            else
            {
                Console.WriteLine($"Crawler {crawlerClassName} is not implemented");
                Console.ReadKey();
            }
        }
    }
}
