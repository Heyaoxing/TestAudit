using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace TestAudit
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            tbContent.Text = "";
        }

        private void btnComp_Click(object sender, EventArgs e)
        {
            string strUrl = tbUrl.Text.Trim();
            string strContent = tbContent.Text.Trim();
            string strListXPath = tbListXPath.Text.Trim();
            string strItemXPath = tbItemXPath.Text.Trim();
            string strTitleXPath = tbTitleXPath.Text.Trim();
            string strPagingXPath = tbPagingXPath.Text.Trim();
            string strPassStatus = tbPassStatus.Text.Trim();
            string strNoPassStatus = tbNoPassStatus.Text.Trim();

            //配置
            Code.CatchConfig config = new Code.CatchConfig();
            config.ListXPath = strListXPath;
            config.ItemXPath = strItemXPath;
            config.TitleXPath = strTitleXPath;
            config.PassStatus = strPassStatus.Split(';').ToList();
            config.NoPassStatus = strNoPassStatus.Split(';').ToList();
            config.PagingXPath = strPagingXPath;

            config = HandleConfig(config);

            Code.CatchCore catchCore = new Code.CatchCore(strUrl, strContent, config);
            var result = catchCore.Compute();
            if (result != null)
            {
                this.dataGridView1.DataSource = new BindingList<Code.ArticleModel>(result.ArticleList);
                string pageText = "";
                foreach (var li in result.PagingList)
                {
                    pageText += li + "\r\n";
                }
                rtbPage.Text = pageText;
            }
        }

        /// <summary>
        /// 处理优化所填配置
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private Code.CatchConfig HandleConfig(Code.CatchConfig config)
        {
            if (config.ListXPath != "")
            {
                config.ListXPath = config.ListXPath.Replace("\"", "").Replace("'", "");
                var tmp = config.ListXPath.Split('=');
                config.ListXPath = string.Format("//div[@{0}='{1}']|//td[@{0}='{1}']|//table[@{0}='{1}']", tmp[0], tmp[1]);
            }
            if (config.ItemXPath != "")
            {
                config.ItemXPath = config.ItemXPath.Replace("\"", "").Replace("'", "");
                var tmp = config.ItemXPath.Split('=');
                config.ItemXPath = string.Format("descendant::div[@{0}='{1}']|descendant::li[@{0}='{1}']|descendant::tr[@{0}='{1}']", tmp[0], tmp[1]);
            }
            if (config.TitleXPath != "")
            {
                config.TitleXPath = config.TitleXPath.Replace("\"", "").Replace("'", "");
                var tmp = config.TitleXPath.Split('=');
                config.TitleXPath = string.Format("descendant::div[@{0}='{1}']|descendant::td[@{0}='{1}']|descendant::span[@{0}='{1}']", tmp[0], tmp[1]);
            }
            if (config.PagingXPath != "")
            {
                config.PagingXPath = config.PagingXPath.Replace("\"", "").Replace("'", "");
                var tmp = config.PagingXPath.Split('=');
                config.PagingXPath = string.Format("//div[@{0}='{1}']|//td[@{0}='{1}']|//span[@{0}='{1}']", tmp[0], tmp[1]);
            }
            return config;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tbListXPath.Text = "//div[@class='list_div']";
            tbItemXPath.Text = "descendant::div[@class='tabletr']";
            tbTitleXPath.Text = "descendant::span[@class='STYLE2']";
            tbPagingXPath.Text = "";
            tbPassStatus.Text = "正常";
            tbNoPassStatus.Text = "不通过";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tbListXPath.Text = "//div[@id='content']";
            tbItemXPath.Text = "descendant::ul[@class='list']/li";
            tbTitleXPath.Text = "";
            tbPagingXPath.Text = "";
            tbPassStatus.Text = "";
            tbNoPassStatus.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            tbListXPath.Text = "//div[@class='ls']";
            tbItemXPath.Text = "descendant::tr";
            tbTitleXPath.Text = "";
            tbPagingXPath.Text = "";
            tbPassStatus.Text = "";
            tbNoPassStatus.Text = "";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            tbListXPath.Text = "//table[@id='map_art']";
            tbItemXPath.Text = "descendant::td[@class='middle']/div";
            tbTitleXPath.Text = "";
            tbPagingXPath.Text = "";
            tbPassStatus.Text = "";
            tbNoPassStatus.Text = "";
        }
    }
}
