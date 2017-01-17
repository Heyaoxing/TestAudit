using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace TestAudit.Code
{
    /// <summary>
    /// 
    /// </summary>
    public class CatchCore
    {
        readonly string DateRegString = @"(([0-9]{4}(-)[0-9]{1,2}(-)[0-9]{1,2} [0-9]{1,2}(:|：)[0-9]{2})|([0-9]{4}年[0-9]{1,2}月[0-9]{1,2}日 [0-9]{1,2}(:|：)[0-9]{2})|([0-9]{4}/[0-9]{1,2}/[0-9]{1,2} [0-9]{1,2}(:|：)[0-9]{2})|([0-9]{1,2}(-)[0-9]{1,2} [0-9]{1,2}(:|：)[0-9]{2})|([0-9]{1,2}月[0-9]{1,2}日 [0-9]{1,2}(:|：)[0-9]{2})|([0-9]{1,2}/[0-9]{1,2} [0-9]{1,2}(:|：)[0-9]{2})|([0-9]{4}(-)[0-9]{1,2}(-)[0-9]{1,2})|([0-9]{4}年[0-9]{1,2}月[0-9]{1,2}日)|([0-9]{4}/[0-9]{1,2}/[0-9]{1,2})|(?<!\d)([0-9]{1,2}(-)[0-9]{1,2})|([0-9]{1,2}月[0-9]{1,2}日)|(?<!\d)([0-9]{1,2}/[0-9]{1,2})|(?<!\d)([0-9]{1,2}(:|：)[0-9]{2}))";

        string _url = "";
        string _html = "";
        Code.CatchConfig config;

        public CatchCore(string url, string html, Code.CatchConfig config)
        {
            _url = url;
            _html = html;
            this.config = config;
        }

        /// <summary>
        /// 计算
        /// </summary>
        public PageModel Compute()
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.OptionAutoCloseOnEnd = true;
            htmlDoc.LoadHtml(_html);

            PageModel pageModel = new PageModel();
            //解析文章列表
            pageModel.ArticleList = AnalysisArticle(htmlDoc);
            //解析分页超链接
            pageModel.PagingList = AnalysisPaging(htmlDoc);

            return pageModel;
        }

        /// <summary>
        /// 解析文章列表
        /// </summary>
        /// <param name="listNode"></param>
        /// <returns></returns>
        private List<ArticleModel> AnalysisArticle(HtmlDocument htmlDoc)
        {
            HtmlNode contentNode = null;
            if (!string.IsNullOrEmpty(config.ListXPath))
            {
                contentNode = htmlDoc.DocumentNode.SelectSingleNode(config.ListXPath);
            }
            else
            {
                contentNode = htmlDoc.DocumentNode;
            }

            if (contentNode == null)
            {
                return null;
            }

            HtmlNodeCollection listNode = null;
            if (string.IsNullOrEmpty(config.ItemXPath))//未指定xpath
            {
                listNode = contentNode.SelectNodes("descendant::div|descendant::li|descendant::tr");
            }
            else//指定xpath
            {
                listNode = contentNode.SelectNodes(config.ItemXPath);
            }

            if (listNode == null)
            {
                return null;
            }

            List<ArticleModel> list = new List<ArticleModel>();
            foreach (var item in listNode)
            {
                if (item.InnerText.Length <= 5 || item.InnerHtml.Contains("<a") == false)//跳过字符数小于5，跳过没有超链接的
                {
                    continue;
                }

                ArticleModel model = new ArticleModel();
                model.InnerHtml = item.InnerHtml;
                model.Node = item;

                string url = "";

                //取标题
                if (string.IsNullOrEmpty(config.TitleXPath) == false)
                {
                    var titleNode = item.SelectSingleNode(config.TitleXPath);
                    if (titleNode == null)
                    {
                        continue;
                    }
                    else
                    {
                        var result = GetTitle(titleNode.InnerHtml, ref url);
                        if (result == "-1")
                        {
                            continue;
                        }
                        else
                        {
                            model.Title = result;
                        }
                    }
                }
                else
                {
                    var result = GetTitle(item.InnerHtml, ref url);
                    if (result == "-1")
                    {
                        continue;
                    }
                    else
                    {
                        model.Title = result;
                    }

                }
                model.Url = url;

                //取时间
                Match mcDate = new Regex(DateRegString, RegexOptions.IgnoreCase).Match(item.OuterHtml);
                if (mcDate.Success)
                {
                    model.Date = mcDate.Value;
                }

                //取状态
                if (config.NoPassStatus != null && config.NoPassStatus.Count > 0)
                {
                    foreach (var li in config.NoPassStatus)
                    {
                        if (item.InnerHtml.Contains(li))
                        {
                            continue;
                        }
                    }
                }
                if (config.PassStatus != null && config.PassStatus.Count > 0)
                {
                    bool s = false;
                    foreach (var li in config.PassStatus)
                    {
                        if (item.InnerHtml.Contains(li))
                        {
                            s = true;
                            break;
                        }
                    }
                    if (s == false)
                    {
                        continue;
                    }
                }

                list.Add(model);
            }

            list = FilterRepeart(list);

            return list;
        }

        /// <summary>
        /// 解析分页部分的超链接
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private List<string> AnalysisPaging(HtmlDocument doc)
        {
            //分页中抓取超链接
            List<string> list = new List<string>();
            if (config.PagingXPath != "")
            {
                var pageNode = doc.DocumentNode.SelectSingleNode(config.PagingXPath);
                if (pageNode != null)
                {
                    var matches = new Regex(@"<a[^>]+href=\s*(?:'(?<href>[^']+)'|""(?<href>[^""]+)""|(?<href>[^>\s]+))\s*[^>]*>(?<text>.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline).Matches(pageNode.OuterHtml);
                    foreach (Match m in matches)
                    {
                        string url = m.Groups["href"].Value;
                        url = HandleUrl(url);
                        list.Add(url);
                    }
                    list = list.Distinct().ToList();
                }
            }
            return list;
        }

        private string GetTitle(string txt, ref string url)
        {
            var mc = new Regex(@"<a[^>]*?href=""[^(javascript|#)][^>]*""[^>]*?>(?<title>[\s\S]*?)</a>", RegexOptions.IgnoreCase).Match(txt);

            if (mc.Success)
            {
                string title = mc.Groups["title"].Value;

                if (title == "" || title.Length <= 4 || title.Contains("<div") || title.Contains("<img"))
                {
                    return "-1";
                }
                else
                {
                    var mc2 = new Regex(@"<a[^>]+href=\s*(?:'(?<href>[^']+)'|""(?<href>[^""]+)""|(?<href>[^>\s]+))\s*[^>]*>(?<text>.*?)</a>", RegexOptions.IgnoreCase | RegexOptions.Singleline).Match(txt);
                    if (mc2.Success)
                    {
                        url = mc2.Groups["href"].Value;
                        url = HandleUrl(url);
                    }
                    return title;
                }
            }
            else
            {
                return "-1";
            }
        }

        /// <summary>
        /// 过滤重复
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<ArticleModel> FilterRepeart(List<ArticleModel> list)
        {
            //过滤包含的重复
            List<int> listRemove = new List<int>();
            if (list.Count > 1)
            {
                for (int i = list.Count - 1; i > 0; i--)
                {
                    for (int j = 0; j <= i - 1; j++)
                    {
                        Console.WriteLine("j:" + j + "  i:" + i);
                        if (list[j].Node.OuterHtml.Contains(list[i].Node.OuterHtml))
                        {
                            listRemove.Add(j);
                        }
                    }
                }
            }
            listRemove = listRemove.Distinct().ToList();
            listRemove.Sort();
            for (int i = listRemove.Count - 1; i >= 0; i--)
            {
                list.RemoveAt(listRemove[i]);
            }
            return list;
        }

        private string HandleUrl(string url)
        {
            if (!string.IsNullOrEmpty(_url))
            {
                Uri baseUri = new Uri(_url);
                Uri absoluteUri = new Uri(baseUri, url);//相对绝对路径都在这里转 这里的urlx ="../test.html"
                return absoluteUri.ToString();
            }
            else
            {
                return url;
            }
        }

    }
}
