using CsQuery;
using HtmlAgilityPack;
using Newtonsoft.Json;
using ShuminaviCrawl.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ShuminaviCrawl
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                var s = new Scraper();
                try
                {
                    await s.DoAll();
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }).Wait();
        }
    }
    public class Scraper
    {
        // Key:ShuminaviPageUrl Value:PageInfo
        // Dictionary<string, PageInfo> m_pages = new Dictionary<string, PageInfo>();

        // 全巡回
        public async Task DoAll()
        {
            // フェーズ1: カレンダー一覧取得 (514カレンダー)
            if (false)
            {
                await CalendarManager.GetAlllCalendars();
            }

            // フェーズ2: カレンダー毎の記事情報取得
            if (false)
            {
                List<Article> articles = new List<Article>();
                var calendars = JsonIo.LoadJson<List<Calendar>>("calendars.json");
                foreach (var calendar in calendars)
                {
                    var a = await ArticleManager.GetCalendarArticles(calendar.Url, calendar.Name);
                    articles.AddRange(a);
                    //break;
                }
                JsonIo.SaveJson("articles.json", articles);
            }

            // フェーズ3: 集計
            if (false)
            {
                // 作者毎にまとめる
                Dictionary<string, List<Article>> authorArticles = new Dictionary<string, List<Article>>();
                var articles = JsonIo.LoadJson<List<Article>>("articles.json");
                foreach (var article in articles)
                {
                    if (article.IsValid())
                    {
                        if (!authorArticles.ContainsKey(article.AuthorName))
                        {
                            authorArticles[article.AuthorName] = new List<Article>();
                        }
                        authorArticles[article.AuthorName].Add(article);
                    }
                }
                // 作者毎の記事を日付でソート
                foreach (var authorName in authorArticles.Keys)
                {
                    authorArticles[authorName].Sort((a, b) => { return a.Day - b.Day; });
                }
                // 保存
                JsonIo.SaveJson("author_articles.json", authorArticles);
            }

            // フェーズ4: 記事数順に並べる
            if (false)
            {
                List<User> users = new List<User>();
                Dictionary<string, List<Article>> authorArticles = JsonIo.LoadJson<Dictionary<string, List<Article>>>("author_articles.json");
                foreach (var p in authorArticles)
                {
                    User user = new User
                    {
                        UserName = p.Value[0].AuthorName,
                        UserIconUrl = p.Value[0].AuthorIconUrl,
                        Articles = p.Value
                    };
                    users.Add(user);
                }

                // 投稿数順にソート
                users.Sort((a, b) => { return -(a.Articles.Count - b.Articles.Count); }); // 降順

                // 保存
                JsonIo.SaveJson("pre_ranking.json", users);
            }

            // フェーズ5: 順位番号を付ける（同順は同じ数値)
            if (false)
            {
                List<User> users = JsonIo.LoadJson<List<User>>("pre_ranking.json");
                int rank = 1;
                for (int i = 0; i < users.Count; i++)
                {
                    if (i == 0) users[i].Rank = rank;
                    else
                    {
                        if (users[i].ArticleCount == users[i - 1].ArticleCount) users[i].Rank = users[i - 1].Rank;
                        else users[i].Rank = rank;
                    }
                    rank++;
                }
                // 保存
                JsonIo.SaveJson("ranking.json", users);
            }

            // フェーズ6: マークダウンとして出力
            if (true)
            {
                // OutputRankingMarkdown("ranking_10.md", 10);
                // OutputRankingMarkdown("ranking_15.md", 15);
                // OutputRankingMarkdown("ranking_20.md", 20);
                // OutputRankingMarkdown("ranking_30.md", 30);
                // OutputRankingMarkdown("ranking_50.md", 50);
                // OutputRankingMarkdown("ranking_all.md", -1);
                OutputRankingMarkdown("ranking_100.md", 100);
                OutputRankingMarkdownEx("ranking_ex.md", 50);
            }
        }
        string GetArticlesLinks(List<Article> articles)
        {
            string content = "";
            foreach (var a in articles)
            {
                if (content != "") content += "<br/>";
                content += string.Format("([{0}]({1}) {2}日目) [{3}]({4})",
                    a.CalendarName, a.CalendarUrl, a.Day, a.ArticleName.Replace("|", "&#x7c;"), a.ArticleUrl);
            }
            return content;
        }
        void OutputRankingMarkdown(string filename, int limit)
        {
            List<User> users = JsonIo.LoadJson<List<User>>("ranking.json");
            string content = "";
            content += "|  No|ユーザ|投稿数|記事一覧|\n";
            content += "|---:|:-----|-----:|:-------|\n";
            foreach (var user in users)
            {
                if (limit != -1 && user.Rank > limit) break; // 切り上げ条件
                content += string.Format("|{0}|<img src='{1}' width='18' height='18' alt='{2}'> @{3}|{4}|{5}|\n",
                    user.Rank,
                    user.UserIconUrl, user.UserName,
                    user.UserName,
                    user.ArticleCount,
                    GetArticlesLinks(user.Articles)
                );
            }
            JsonIo.SaveText(filename, content);
        }
        void OutputRankingMarkdownEx(string filename, int limit)
        {
            List<User> users = JsonIo.LoadJson<List<User>>("ranking.json");
            string content = "";
            content += "|  No|ユーザ|投稿数|記事一覧|\n";
            content += "|---:|:-----|-----:|:-------|\n";
            int cnt = 0;
            foreach (var user in users)
            {
                if (limit != -1 && user.Rank > limit) break; // 切り上げ条件
                content += string.Format("|{0}|<img src='{1}' width='18' height='18' alt='{2}'> @{3}|{4}|{5}|\n",
                    user.Rank,
                    user.UserIconUrl, user.UserName,
                    user.UserName,
                    user.ArticleCount,
                    GetArticlesLinks(user.Articles)
                );
                cnt++;
            }

            // limit以降
            content += "\n";
            content += "|  No|投稿数|ユーザ|\n";
            content += "|---:|-----:|:-----|\n";
            Dictionary<int, List<User>> extraUsers = new Dictionary<int, List<User>>();
            for (int i = cnt; i < users.Count; i++) {
                int rank = users[i].Rank;
                if (!extraUsers.ContainsKey(rank)) extraUsers[rank] = new List<User>();
                extraUsers[rank].Add(users[i]);
            }
            foreach(var rank in extraUsers.Keys)
            {
                content += string.Format("|{0}|{1}|", rank, extraUsers[rank][0].ArticleCount);
                string names = "";
                foreach (var user in extraUsers[rank])
                {
                    if (names != "") names += ", ";
                    if (true)
                    {
                        names += string.Format(
                            "<img src='{0}' width='18' height='18' alt='{1}'>@{2}",
                            user.UserIconUrl, user.UserName, user.UserName
                        );
                    }
                    else
                    {
                        names += string.Format(
                            "@{0}", user.UserName
                        );
                    }
                }
                content += names + "|\n";
            }

            JsonIo.SaveText(filename, content);
        }
    }
}
