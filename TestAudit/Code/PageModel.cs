using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestAudit.Code
{
    public class PageModel
    {
        public List<ArticleModel> ArticleList { get; set; }
        public List<string> PagingList { get; set; }
    }
}
