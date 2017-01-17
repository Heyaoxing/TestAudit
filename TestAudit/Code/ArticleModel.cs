using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestAudit.Code
{
    /// <summary>
    /// 文章模型
    /// </summary>
    public class ArticleModel
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Date { get; set; }
        public bool Status { get; set; }
        public string InnerHtml { get; set; }
        public HtmlAgilityPack.HtmlNode Node { get; set; }
    }
}
