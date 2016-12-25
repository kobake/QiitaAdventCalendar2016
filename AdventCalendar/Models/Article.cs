using CsQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuminaviCrawl.Models
{
    public class Article
    {
        public string Error { get; set; } // ページ取得時に何かしらエラーが発生 (NOT FOUND 等) が発生した場合はここにエラー内容が入ります
        public string CalendarUrl { get; set; }
        public string CalendarName { get; set; }
        public int Day { get; set; }
        public string AuthorName { get; set; }
        public string AuthorIconUrl { get; set; }
        public string ArticleName { get; set; }
        public string ArticleUrl { get; set; }

        public bool IsValid()
        {
            if (Day <= 0) return false;
            if (string.IsNullOrEmpty(AuthorName)) return false;
            //if (string.IsNullOrEmpty(AuthorIconUrl)) return false; // アイコンが無い場合もあるのかなぁ。未調査。
            if (string.IsNullOrEmpty(ArticleName)) return false;
            if (string.IsNullOrEmpty(ArticleUrl)) return false;
            return true;
        }

        public override string ToString()
        {
            return "Article" + JsonConvert.SerializeObject(this);
        }
    }
    public class ArticleManager
    {
        public async static Task<List<Article>> GetCalendarArticles(string calendarUrl, string calendarName)
        {
            List<Article> ret = new List<Article>();

            Console.WriteLine(calendarUrl);
            string calrndarHtml = await HtmlGetter.GetHtml(calendarUrl);
            CQ cq = new CQ(calrndarHtml);
            cq.Find(".adventCalendarCalendar_day").Each((_e) => {
                Article article = new Article { CalendarUrl = calendarUrl, CalendarName = calendarName };
                CQ e = new CQ(_e);

                // 日付
                {
                    var p = e.Find(".adventCalendarCalendar_date");
                    if (p == null) return;
                    article.Day = int.Parse(p.Text().Trim());
                }

                // 投稿者
                {
                    var a = e.Find(".adventCalendarCalendar_author a");
                    if (a == null) return;
                    article.AuthorName = a.Text().Replace("&nbsp;", "").Trim();
                    article.AuthorIconUrl = a.Find("img").Attr("src");
                }

                // 記事情報
                {
                    var a = e.Find(".adventCalendarCalendar_comment a");
                    if (a == null) return;
                    article.ArticleName = a.Text().Trim();
                    article.ArticleUrl = a.Attr("href");
                }

                // 結果に追加
                ret.Add(article);
                Console.WriteLine(article);
            });
            return ret;
        }
    }
}
