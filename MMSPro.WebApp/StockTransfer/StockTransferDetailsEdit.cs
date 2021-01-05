using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Collections.Specialized;
using System.Configuration;

namespace MMSPro.WebApp
{
    public class StockTransferDetailsEdit : System.Web.UI.Page
    {
        MMSProDBDataContext db;
        SPGridView gv;
        TextBox tboxQualified;
        Button subMit;
        private int listCount = 0;


        static string[] Titlelist = {
                                     "调拨通知单编号:StorageOutNoticeCode",
                                     "物料名称:MaterialName",
                                     "物料规格:SpecificationModel",
                                     "物料编码:MaterialCode",
                                     "单价:UnitPrice",
                                     "金额:Amount",
                                     "所属仓库:StorageName",
                                     "所属垛位:PileCode",
                                     "到库时间:StorageTime",
                                     "供应商:SupplierName",
                                     "创建者:Creator",
                                     "创建日期:CreateTime",
                                     
                                    };



        protected void Page_Load(object sender, EventArgs e)
        {
            //if (!string.IsNullOrEmpty(Request.QueryString["StorageOutDetailsID"]))
            //{ 

            //}
            this.gv = new SPGridView();
            this.gv.AutoGenerateColumns = false;
            init();
            BindGridView();
            //初始化质检合格表
            if (!string.IsNullOrEmpty(Request.QueryString["StorageOutDetailsID"]))
            {
                using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    var cg = from u in dc.StorageOutDetails
                             where u.StorageOutDetailsID == Convert.ToInt32(Request.QueryString["StorageOutDetailsID"].ToString())
                             orderby u.StorageOutDetailsID ascending
                             select new { u.FinanceCode, u.Gentaojian};

                    var li = cg.ToList();
                    for (int i = 0; i < this.gv.Rows.Count; i++)
                    {
                        TextBox tb = (TextBox)(this.gv.Rows[i].Cells[5].Controls[0]);
                        tb.Text = li[i].FinanceCode.ToString();
                        TextBox ti = (TextBox)(this.gv.Rows[i].Cells[6].Controls[0]);
                        ti.Text = li[i].Gentaojian.ToString();
                    }
                }
            }


            if (!IsPostBack)
            {

            }


            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");
            //返回
            ToolBarButton tbarbtnBack = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnBack.ID = "backRow";
            tbarbtnBack.Text = "返回";
            tbarbtnBack.ImageUrl = "/_layouts/images/BACK.GIF";
            tbarbtnBack.Click += new EventHandler(tbarbtnBack_Click);
            tbarTop.Buttons.Controls.Add(tbarbtnBack);

            ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnRefresh.ID = "btnRefresh";
            btnRefresh.Text = "刷新";
            btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
            btnRefresh.Padding = "0,5,0,0";
            btnRefresh.Click += new EventHandler(btnRefresh_Click);
            tbarTop.RightButtons.Controls.Add(btnRefresh);
        }



        void tbarbtnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("StorageOutDetailsManage.aspx?StorageOutNoticeID=" + Request.QueryString["StorageOutNoticeID"] + "&&StorageOutTaskID=" + Request.QueryString["StorageOutTaskID"] + "");
        }




        void btnRefresh_Click(object sender, EventArgs e)
        {


        }


        /// <summary>
        /// 检查输入选择行输入框值否为空,且财务编码是否唯一
        /// </summary>
        /// <returns></returns>
        private bool CheckStringEmpty()
        {
            using (MMSProDBDataContext dbc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                List<string> li = new List<string>();
                List<CheckBox> listString = GetCheckedID();
                if (listString.Count > 0)
                {
                    for (int s = 0; s < listString.Count; s++)
                    {


                        for (int k = 5; k < 7; k++)
                        {
                            if (this.gv.Rows[s].Cells[k].Controls[0] is TextBox)
                            {
                                tboxQualified = (TextBox)this.gv.Rows[s].Cells[k].Controls[0];
                                if (tboxQualified.Text == string.Empty)
                                {
                                    return false;
                                }
                                if (k == 5)
                                {


                                    TextBox f_code = (TextBox)this.gv.Rows[s].Cells[k].Controls[0];
                                    li.Add(f_code.Text.Trim());
                                    var tmp = from a in dbc.StorageOutDetails
                                              where a.FinanceCode == f_code.Text.Trim()
                                              select a;
                                    if (tmp.ToArray().Length > 0)
                                    {
                                        return false;
                                    }



                                }
                            }
                        }

                    }
                    //用户填写的值如有重复
                    listCount = li.Count;
                    if (listCount != li.Distinct().ToList().Count)
                    {
                        return false;
                    }


                }
            }

            return true;
        }

        /// <summary>
        /// 返回选中的列表
        /// </summary>
        /// <returns>返回list构成的列表</returns>
        private List<CheckBox> GetCheckedID()
        {
            List<CheckBox> list = new List<CheckBox>();

            foreach (GridViewRow row in this.gv.Rows)
            {

                CheckBox ck = (CheckBox)row.Cells[0].FindControl("SMItem");
                if (ck.Checked)
                {
                    list.Add(ck);
                }
            }
            return list;
        }

        private void init()
        {
            #region 初始化
            subMit = new Button();
            subMit.Text = "提交修改";
            subMit.Width = 120;
            subMit.Click += new EventHandler(subMit_Click);
            Panel plSubMit = (Panel)GetControltByMaster("plSubMit");
            plSubMit.Controls.Add(subMit);
            #endregion
        }

        void subMit_Click(object sender, EventArgs e)
        {
            if (CheckStringEmpty() != true)
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请将选择的数据填写完整且财务编码不能重复!')</script>");
                return;
            }


            List<CheckBox> listString = GetCheckedID();
            if (listString.Count > 0)
            {


                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {

                    for (int i = 0; i < listString.Count; i++)
                    {

                        StorageOutDetails SOD = db.StorageOutDetails.SingleOrDefault(u => u.StorageOutDetailsID == Convert.ToInt32(Request.QueryString["StorageOutDetailsID"].ToString()));
                        TextBox tboxCode = (TextBox)(this.gv.Rows[i].Cells[5].Controls[0]);
                        TextBox tboxNums = (TextBox)(this.gv.Rows[i].Cells[6].Controls[0]);
                        SOD.FinanceCode = tboxCode.Text.Trim();
                        SOD.Gentaojian = Convert.ToDecimal(tboxNums.Text.Trim());
                        db.SubmitChanges();


                    }

                }

                Response.Redirect("StorageOutDetailsManage.aspx?StorageOutNoticeID=" + Request.QueryString["StorageOutNoticeID"] + "&&StorageOutTaskID=" + Request.QueryString["StorageOutTaskID"] + "");
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择后提交修改!')</script>");
            }
        }




        /// <summary>
        /// 绑定Gridview
        /// </summary>
        private void BindGridView()
        {
            this.gv.Columns.Clear();
            //int sid = Convert.ToInt32(Request.QueryString["StorageInID"]);

            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {

                TemplateField tfieldTextBox = new TemplateField();
                tfieldTextBox.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "StorageOutDetailsID", "txtCount");
                tfieldTextBox.HeaderTemplate = new MulTextBoxTemplate("调拨数量", DataControlRowType.Header);
                tfieldTextBox.ItemStyle.Width = 150;

                TemplateField code_fieldTextBox = new TemplateField();
                code_fieldTextBox.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "StorageOutDetailsID", "txtCode");
                code_fieldTextBox.HeaderTemplate = new MulTextBoxTemplate("财务编号", DataControlRowType.Header);
                code_fieldTextBox.ItemStyle.Width = 150;



                BoundField bfColumn;
                //添加选择列
                TemplateField tfieldCheckbox = new TemplateField();
                tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "StorageOutDetailsID");
                tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
                this.gv.Columns.Add(tfieldCheckbox);

                foreach (var kvp in Titlelist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.gv.Columns.Add(bfColumn);
                }
                this.gv.Columns.Insert(5, code_fieldTextBox);
                this.gv.Columns.Insert(6, tfieldTextBox);
                this.gv.DataSource = from a in db.StorageOutDetails
                                     where a.StorageOutDetailsID == Convert.ToInt32(Request.QueryString["StorageOutDetailsID"])
                                     select new
                                     {

                                         a.StorageOutDetailsID,
                                         a.StorageOutNotice.StorageOutNoticeCode,
                                         a.TableOfStocks.MaterialInfo.MaterialName,
                                         a.TableOfStocks.MaterialInfo.MaterialCode,
                                         a.TableOfStocks.SpecificationModel,
                                         a.TableOfStocks.UnitPrice,
                                         a.Amount,
                                         a.TableOfStocks.PileInfo.StorageInfo.StorageName,
                                         a.TableOfStocks.PileInfo.PileCode,
                                         a.TableOfStocks.StorageTime,
                                         a.TableOfStocks.SupplierInfo.SupplierName,
                                         a.Creator,
                                         a.CreateTime,

                                     };
                this.gv.DataBind();
                this.gv.Columns[this.gv.Columns.Count - 1].Visible = false;
                Panel p1 = (Panel)GetControltByMaster("Panel1");
                p1.Controls.Add(this.gv);




            }

        }
   

        void btnPassNagetive_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void btnCannel_Click(object sender, EventArgs e)
        {
            //Response.Redirect("QualityControlManage.aspx?TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&StorageInID=" + Request.QueryString["StorageInID"] + "");
        }

        void btnSend_Click(object sender, EventArgs e)
        {

            Response.Redirect("../../default-old.aspx",false);

            //Response.Redirect("QualityControlMessage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&state=材料会计审核");
        }

        /// <summary>
        /// 获取控件
        /// </summary>
        /// <param name="controlName">控件的ID名称</param>
        /// <returns>返回Control，需要强制类型转换为对应控件</returns>
        protected Control GetControltByMaster(string controlName)
        {
            return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        }
    }
}
