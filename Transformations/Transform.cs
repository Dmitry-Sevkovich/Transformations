using System;
using System.Collections.Generic;
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
            Crawler crawler = new Crawler();
            if (args.Length == 0)
            {
                crawler.Crawl();
            }
            else
            {
                crawler.Crawl(args[0]);
            }
        }
    }
}
