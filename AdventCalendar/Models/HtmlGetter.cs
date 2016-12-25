using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShuminaviCrawl.Models
{
    public class HtmlGetter
    {
        // HTML取得
        public static async Task<string> GetHtml(string url)
        {
            string exedir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string cachedir = Path.GetFullPath(exedir + "\\..\\..\\..\\cache");
            string fname = Regex.Replace(url, @"[:/\?]", "_");
            string fpath = Path.Combine(cachedir, fname);
            if (!File.Exists(fpath))
            {
                // サーバアクセスは 0.3秒待ってから行う（サーバ過負荷防止)
                await Task.Delay(300);

                // サーバアクセス
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.99 Safari/537.36");
                string _html = "";
                try
                {
                    // _html = await client.GetStringAsync(url);
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return "404 NOT FOUND";
                    }
                    _html = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fetch Error:" + ex.Message);
                }
                File.WriteAllText(fpath, _html);
            }
            string html = File.ReadAllText(fpath);

            // コメント要素は全削除した結果を返す
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                foreach (HtmlNode comment in doc.DocumentNode.SelectNodes("//comment()"))
                {
                    comment.ParentNode.RemoveChild(comment);
                }
                html = doc.DocumentNode.OuterHtml;
            }
            return html;
        }
    }
}
