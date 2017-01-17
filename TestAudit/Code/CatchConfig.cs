using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestAudit.Code
{
    /// <summary>
    /// 
    /// </summary>
    public class CatchConfig
    {
        public string ListXPath { get; set; }

        public string ItemXPath { get; set; }
        public string TitleXPath { get; set; }
        //public string TimeXPath { get; set; }
        //public string StatusClass { get; set; }

        public List<string> PassStatus { get; set; }
        public List<string> NoPassStatus { get; set; }


        public string PagingXPath { get; set; }
    }

}
