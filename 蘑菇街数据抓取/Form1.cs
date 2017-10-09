using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ivony.Html.Parser;
using Ivony.Html;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skay.WebBot;
using System.Data.SqlClient;
using System.Threading;

namespace 蘑菇街数据抓取
{
    public partial class Form1 : Form
    {

        /// <summary>
        /// ///
        /// 
        /// 
        /// 每次抓取是到数据库修改链接对应的页数页数
        /// 尝试100页
        /// 
        /// 
        /// </summary>
        bool falg = false;
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if(falg)
            {
                MessageBox.Show("正在运行 请勿重复点击");
            }
            else
            {
                Thread th = new Thread(Start_go);
                th.Start();
            }
            

        }

        public void Start_go()
        {
            falg = true;
            DataTable dt = new DataTable();//mogujie  E_COMMERCE
            SqlConnection conn = new SqlConnection("Data Source=10.1.56.31;Initial Catalog=mogujie;Persist Security Info=True;User ID=sa;Password=123456");
            SqlConnection conn_E = new SqlConnection("Data Source=10.1.56.31;Initial Catalog=E_COMMERCE;Persist Security Info=True;User ID=sa;Password=123456");
            conn.Open();
            conn_E.Open();
            string sql = "select * from mogujieurl";//每次抓取是到数据库修改链接对应的页数页数
            SqlDataAdapter adap = new SqlDataAdapter(sql, conn);
            adap.Fill(dt);

            //一级分类的编号    ------------------------------------------------------
            string INDUSTRY_ID = "内衣配饰";
            //二级分类的编号  ------------------------------------------------------
            string PRODUCTTYPE_ID = null;
            //购物平台的来源
            string PLATFORM_ID = "蘑菇街";
            //string month = textBox1.Text;
            //if(month=="")
            //{
            //    MessageBox.Show("请输入月份");
            //    return ;
            //}
            DateTime sy = new DateTime();//sy为datetime型 
            sy = System.DateTime.Today;//取当前日期给sy 
            string year = sy.Year.ToString();//取年份 
            string month = sy.Month.ToString();//取月份

            for (int i = 0; i < dt.Rows.Count; i++) //i = 14 count = 22
            {
                int BRAND_ID = 177156;
                string tradeItemId = null;//--------贸易项目ID
                decimal DISCOUNT = 0.00M;//---------促销价
                string url = null;//----------------详情页URL
                string url_ = null;//---------------分类连接
                int pagenum = 0;//------------------页数
                string TITLE = "无数据";//----------标题
                int perPage = 0;//------------------每页的条数
                object[] a = dt.Rows[i].ItemArray;

                url_ = (string)a[3];
                pagenum = (Int32)a[6];
                //尝试100页，不到的可以跳出；
                pagenum = 100;
                PRODUCTTYPE_ID = (string)a[7];

                url_ = url_.Split('?')[0].ToString();
                int urlnum = int.Parse(url_.Split('/')[5].ToString());

                HttpUtility http = new HttpUtility();

                for (int ii = 0; ii < pagenum; ii++) // 
                {
                    int page = ii + 1;
                    string url_and = "http://list.mogujie.com/search?cKey=pc-wall-v1&page=" + page + "&fcid=" + urlnum + "&ad=2";
                    string Area_Html = http.GetHtmlText(url_and, "utf-8", "text/html;charset=utf-8", "");
                    var dom = (JObject)JsonConvert.DeserializeObject(Area_Html);
                    try
                    {
                        perPage = dom["result"]["wall"]["docs"].Count();
                    }
                    catch{
                        break;
                    }

                    for (int iii = 0; iii < perPage; iii++)//
                    {
                        try
                        {
                            //url_and = "http://list.mogujie.com/search?cKey=pc-wall-v1&page=" + page + "&fcid=" + urlnum + "&ad=2";
                            //Area_Html = http.GetHtmlText(url_and, "UTF-8", "text/html;charset=utf-8", "");
                            //var dom = (JObject)JsonConvert.DeserializeObject(Area_Html);
                            url = dom["result"]["wall"]["docs"][iii]["link"].ToString();//商品编号
                            DISCOUNT = Convert.ToDecimal(dom["result"]["wall"]["docs"][iii]["price"].ToString());
                            tradeItemId = dom["result"]["wall"]["docs"][iii]["tradeItemId"].ToString();
                            TITLE = dom["result"]["wall"]["docs"][iii]["title"].ToString();
                        }
                        catch { }

                        try
                        {//-----------------------------------------------------------修改月份---------------
                            string url_Ishave = "'" + url + "'";
                            DataTable dt3 = new DataTable();
                            sql = @"select * from DATE where TITLE = '" + TITLE + "' and GOODSID =  '" + tradeItemId + "' and MONTH =  '" + month + "'  and  YEAR = '" + year+"'";
                            adap = new SqlDataAdapter(sql, conn);
                            adap.Fill(dt3);
                            if (dt3.Rows.Count >= 1)
                            {
                                richTextBox1.Text = "";
                                richTextBox1.Text = "重复数据";
                                continue;
                            }
                        }
                        catch
                        {
                            continue;
                        }

                        string html = null;
                        try
                        {
                            html = http.GetHtmlText(url, "utf-8", "text/html; charset=utf-8");
                        }
                        catch
                        {
                            continue;
                        }

                        var documenthtml = new JumonyParser().Parse(html);

                        //商品编号-----------------------------------------------------------------------------------------------
                        string GOODSID = "无数据";
                        //店铺名称--------------------------------------------------------------------------------------------------
                        string SHOPNAME = "无数据";
                        //收藏数------------------------------------------------------------------------------------------------------
                        string COLLECTION = "无数据";

                        string COLLECTION_NUM = "0";
                        //省市---------------------------------------------------------------------------------------------------
                        string area = "无数据";
                        string PROVINCE = "其他";
                        string CITY = "其他";
                        int PROVINCE_ID = 0;
                        int CITY_ID = 0;
                        GOODSID = tradeItemId;

                        try
                        {
                            var shopId1 = documenthtml.FindFirst("#shopId");
                            string shopId = shopId1.Attribute("value").Value();

                            string url_shop = "http://www.mogujie.com/trade/shopweb_index/asyShopHead?&shopId=" + shopId;
                            Area_Html = http.GetHtmlText(url_shop, "GBK", "text/html;charset=GBK", "");
                            try
                            {
                                var dom_SS = (JObject)JsonConvert.DeserializeObject(Area_Html);

                                SHOPNAME = dom_SS["data"]["shopInfo"]["name"].ToString();//店铺名称
                                COLLECTION = dom_SS["data"]["shop_befaved_num"].ToString();
                                COLLECTION_NUM = COLLECTION;               //收藏数
                                area = dom_SS["data"]["shopInfo"]["area"].ToString();    //省市
                                if (area.IndexOf("省") > -1 && area.Length == 6)
                                {
                                    PROVINCE = area.Substring(0, 2);
                                    CITY = area.Substring(3, 3);
                                }
                                //省市编号查询
                                CITY = '\'' + CITY + '\'';
                                try
                                {
                                    DataTable dt1 = new DataTable();
                                    sql = "select CITY_CODE from CITY where CITY_NAME = " + CITY;
                                    adap = new SqlDataAdapter(sql, conn_E);
                                    adap.Fill(dt1);
                                    object[] a1 = dt1.Rows[0].ItemArray;
                                    CITY_ID = (int)a1[0];
                                }
                                catch { }
                                //省编号查询
                                PROVINCE = '\'' + PROVINCE + '\'';
                                try
                                {
                                    DataTable dt2 = new DataTable();
                                    sql = "select PROVINCE_CODE from PROVINCE where PROVINCE_NAME = " + PROVINCE;
                                    adap = new SqlDataAdapter(sql, conn_E);
                                    adap.Fill(dt2);
                                    object[] a2 = dt2.Rows[0].ItemArray;
                                    PROVINCE_ID = (int)a2[0];
                                }
                                catch { }
                            }

                            catch { }
                        }
                        catch { }

                        //价格----------------------------------------------------------------------------------------------------
                        decimal PRICE = 0.00M;
                        try
                        {
                            var text = documenthtml.FindFirst("#J_OriginPrice");
                            string PRICE1 = text.InnerHtml();
                            int count = PRICE1.Length - PRICE1.Replace("¥", "").Length;
                            if (count == 1)
                            {
                                PRICE = Convert.ToDecimal(PRICE1.Split('¥')[1]);
                            }
                            else
                            {
                                PRICE = DISCOUNT;
                            }
                        }
                        catch {
                            PRICE = DISCOUNT;
                        }

                        //总销量------------------------------------------------------------------------------------------
                        int SALE_VOLUME = 0;
                        try
                        {
                            var text = documenthtml.FindFirst(".property-extra");
                            var text1 = text.FindFirst(".J_SaleNum");
                            string text2 = text1.InnerHtml();
                            SALE_VOLUME = int.Parse(text2);
                        }
                        catch { }

                        //销售额---------------------------------------------------------------------------------------------
                        decimal SALE_AMOUNT = 0.00M;
                        try
                        {
                            SALE_AMOUNT = DISCOUNT * SALE_VOLUME;
                        }
                        catch { }

                        //总评价----------------------------------------------------------------------------------------------
                        int COMMEN_NUM = 0;
                        try
                        {
                            var text = documenthtml.FindFirst(".property-extra");
                            var text1 = text.FindFirst(".num");
                            string text2 = text1.InnerHtml();
                            COMMEN_NUM = int.Parse(text2);
                        }
                        catch { }

                        //好评中评差评--------------------------------------------------------------------------------------------
                        int POSITIVE_COMMEN = 0;  //好
                        int MODERATE_COMMEN = 0;  //中
                        int NEGATIVE_COMMEN = 0;  //差

                        try                       //好
                        {
                            var text = documenthtml.FindFirst(".comment-content");
                            var text1 = text.FindFirst(".list");
                            var text2 = text1.Find(".best");
                            POSITIVE_COMMEN = text2.Count();
                        }
                        catch { }
                        try                       //差
                        {
                            var text = documenthtml.FindFirst(".comment-content");
                            var text1 = text.FindFirst(".list");
                            var text2 = text1.Find("a");
                            NEGATIVE_COMMEN = text2.Count() - POSITIVE_COMMEN;
                        }
                        catch { }
                        try                       //中
                        {
                            MODERATE_COMMEN = COMMEN_NUM - POSITIVE_COMMEN - NEGATIVE_COMMEN;
                        }
                        catch { }

                        string strstr = null;
                        string REPUTATION = "无数据";
                        string SHOPTYPE = "无数据";
                        string SHELVES_TIME = "无数据";
                        int YEAR =int.Parse(year);
                        int MONTH = int.Parse(month);
                        string DETAIL_URL = url;

                        strstr = "商品编号 = " + GOODSID + "\n" + "一级分类的编号 = " + INDUSTRY_ID + "\n" + "二级分类的编号  = " + PRODUCTTYPE_ID + "\n" + "购物平台的来源 = " + PLATFORM_ID + "\n" + "品牌ID = " + BRAND_ID + "\n" + "省份 = " + PROVINCE_ID + "\n" + "店铺名称 = " + SHOPNAME + "\n" + "城市 = " + CITY_ID + "\n" + "title = " + TITLE + "\n" + "价格 = " + PRICE + "\n" + "促销价 = " + DISCOUNT + "\n" + "销售总量 = " + SALE_VOLUME + "\n" + "销售额 = " + SALE_AMOUNT + "\n" + "总评 = " + COMMEN_NUM + "\n" + "好评 = " + POSITIVE_COMMEN + "\n" + "中评 = " + MODERATE_COMMEN + "\n" + "差评  = " + NEGATIVE_COMMEN + "\n" + "收藏人数 = " + COLLECTION_NUM + "\n" + "名誉 = " + REPUTATION + "\n" + "shoptype = " + SHOPTYPE + "\n" + "上市时间 = " + SHELVES_TIME + "\n" + "年 = " + YEAR + "\n" + "月 = " + MONTH + "\n" + "详细地址 = " + url;
                        richTextBox1.Text = "   ";
                        TITLE = TITLE.Replace("'", "-");
                        SHOPNAME = SHOPNAME.Replace("'", "-");
                        sql = string.Format("INSERT INTO DATE VALUES ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}','{20}','{21}','{22}','{23}')", GOODSID, INDUSTRY_ID, PRODUCTTYPE_ID, PLATFORM_ID, BRAND_ID, PROVINCE_ID, SHOPNAME, CITY_ID, TITLE, PRICE, DISCOUNT, SALE_VOLUME, SALE_AMOUNT, COMMEN_NUM, POSITIVE_COMMEN, MODERATE_COMMEN, NEGATIVE_COMMEN, COLLECTION_NUM, REPUTATION, SHOPTYPE, SHELVES_TIME, YEAR, MONTH, url);
                        SqlCommand com = new SqlCommand(sql, conn);
                        com.ExecuteNonQuery();
                        richTextBox1.Text = "--------插入成功---------\n" + strstr;
                        //Thread.Sleep(750);

                    }//for循环 每个页面商品的个数

                }//for循环 每个分类的页数
            }//for循环 URL表
            conn.Close();
            MessageBox.Show("蘑菇街抓取完成");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        { }

        private void Form1_Load(object sender, EventArgs e)
        { }

        private void textBox2_TextChanged(object sender, EventArgs e)
        { }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
