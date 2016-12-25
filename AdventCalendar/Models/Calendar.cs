using CsQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuminaviCrawl.Models
{
    public class Calendar
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
    public class CalendarManager
    {
        public static async Task GetAlllCalendars()
        {
            // まずは全てのカレンダー
            List<Calendar> calendars = new List<Calendar>();
            string urlbase = "http://qiita.com/advent-calendar/2016/calendars";
            for (int i = 1; i <= 26; i += 1)
            {
                Console.WriteLine(string.Format("-------- {0}/26 --------", i));
                string url = urlbase + "?page=" + i;
                var c = await ScrapeIndex(url);
                calendars.AddRange(c);
            }

            // JSON出力
            JsonIo.SaveJson("calendars.json", calendars);
            Console.WriteLine("calender count - " + calendars.Count);
        }

        async static Task<List<Calendar>> ScrapeIndex(string indexUrl)
        {
            List<Calendar> ret = new List<Calendar>();
            Console.WriteLine(indexUrl);
            string indexHtml = await HtmlGetter.GetHtml(indexUrl);
            CQ cq = new CQ(indexHtml);
            cq.Find(".adventCalendarList tr").Each((_e) => {
                CQ e = new CQ(_e);
                var a = e.Find("a");
                if (a == null) return;
                if (string.IsNullOrEmpty(a.Attr("href"))) return;
                var pageUrl = a.Attr("href");
                if (!pageUrl.StartsWith("http")) pageUrl = "http://qiita.com" + pageUrl;
                var name = a.Text().Trim();
                Console.WriteLine("name " + name);
                Console.WriteLine("pageUrl " + pageUrl);
                ret.Add(new Calendar { Name = name, Url = pageUrl });
            });
            return ret;
        }
    }
}
