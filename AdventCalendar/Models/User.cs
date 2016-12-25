using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuminaviCrawl.Models
{
    public class User
    {
        public int Rank { get; set; }
        public string UserName { get; set; }
        public string UserIconUrl { get; set; }
        public List<Article> Articles { get; set; }
        public int ArticleCount { get { return Articles.Count; } } // 冗長だが見た目確認しやすくするために入れる。
    }
}
