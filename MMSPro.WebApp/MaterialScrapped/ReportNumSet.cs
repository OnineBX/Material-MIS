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
using System.Reflection;

namespace MMSPro.WebApp
{
    public class ReportNumSet:System.Web.UI.Page
    {
        MMSProDBDataContext db;
        SPGridView gv;
        ImageButton dataset;
        ImageButton dataclean;
        TextBox txtReportNum;
        Button btnSave;
        TextBox itembox;
        TextBox tboxQualified;
        CheckBox chb;


        //搜索
        DropDownList ddlStorage;
        DropDownList ddlPile;
        TextBox txtMaterialCode;
        TextBox txtMaterialName;
        private string storage;
        private string f_Pile;

        static string[] Titlelist = {
                                     "物料名称:MaterialName",
                                     "财务编码:FinanceCode",
                                     "物料规格:SpecificationModel", 
                                     "报废数量:Gentaojian",
                                     "所属仓库:StorageName",
                                     "所属垛位:PileCode",
                                     "回收项目:ProjectName",
                                     "生产厂家:ManufacturerName",
                                     "报废状态:state",
                                     "ID:AwaitScrapID"
                                    };



        protected void Page_Load(object sender, EventArgs e)
        {
            this.gv = new SPGridView();
            this.gv.AutoGenerateColumns = false;
            try
            {
                toolbarInit();
                initControl();
                BindGridView();
               
                LoadDefaultData();

                if (!IsPostBack)
                {

                    BindStorage();

                }


            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
        }

      

        protected void tbarbtnAdd_Click(object sender, EventArgs e)
        {

            Response.Redirect("AwaitScrapManage.aspx", false);
        }
        /// <summary>
        /// 初始化报废报告号
        /// </summary>
        private void LoadDefaultData()
        {
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //初始化报告号
                var cg = from u in db.AwaitScrap
                         where u.State=="待报废"
                         orderby u.AwaitScrapID ascending
                         select new { u.ScrapReportNum };

                var li = cg.ToList();
                for (int i = 0; i < this.gv.Rows.Count; i++)
                {
                    TextBox Num = (TextBox)(this.gv.Rows[i].Cells[4].Controls[0]);
                    if (li[i].ScrapReportNum == "未填写")
                    {
                        Num.Text = string.Empty;
                    }
                    else
                    {
                        Num.Text = li[i].ScrapReportNum;
                    }
                    

                }
            }
        }

        private void toolbarInit()
        {

            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");
            //批量设定
            ToolBarButton tbarbtnAdd = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            tbarbtnAdd.ID = "AddNewRow";
            tbarbtnAdd.Text = "返回";
            tbarbtnAdd.ImageUrl = "/_layouts/images/newitem.gif";
            tbarbtnAdd.Click += new EventHandler(tbarbtnAdd_Click);
            tbarTop.Buttons.Controls.Add(tbarbtnAdd);


            //刷新

            ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnRefresh.ID = "btnRefresh";
            btnRefresh.Text = "刷新";
            btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
            btnRefresh.Padding = "0,5,0,0";
            btnRefresh.Click += new EventHandler(btnRefresh_Click);
            tbarTop.RightButtons.Controls.Add(btnRefresh);
        }

        private void initControl()
        {
            this.txtReportNum = (TextBox)GetControltByMaster("txtReportNum");
            this.btnSave = (Button)GetControltByMaster("btnSave");
            btnSave.Attributes.Add("onclick", "return confirm('确认设定所选物资的待报废报告号？');");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.dataset = (ImageButton)GetControltByMaster("dataset");
            this.dataset.Click += new ImageClickEventHandler(dataset_Click);
            this.dataclean = (ImageButton)GetControltByMaster("dataclean");
            this.dataclean.Click += new ImageClickEventHandler(dataclean_Click);

            //搜索
            this.txtMaterialCode = (TextBox)GetControltByMaster("txtMaterialCode");
            this.txtMaterialName = (TextBox)GetControltByMaster("txtMaterialName");
            this.ddlStorage = (DropDownList)GetControltByMaster("ddlStorage");
            this.ddlStorage.SelectedIndexChanged += new EventHandler(ddlStorage_SelectedIndexChanged);
            this.ddlPile = (DropDownList)GetControltByMaster("ddlPile");

        }

        void ddlStorage_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindPile();
        }

        //绑定仓库
        private void BindStorage()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = (from a in db.AwaitScrap
                            select new
                            {
                                Key = a.PileInfo.StorageInfo.StorageName,
                                Value = a.PileInfo.StorageInfo.StorageID
                            }).Distinct();

                this.ddlStorage.DataSource = temp;
                this.ddlStorage.DataTextField = "Key";
                this.ddlStorage.DataValueField = "Value";
                this.ddlStorage.DataBind();
                this.ddlStorage.Items.Insert(0, "--请选择--");
            }
        }
        //绑定垛位
        private void BindPile()
        {
            this.ddlPile.Items.Clear();
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                if (this.ddlStorage.SelectedValue != "--请选择--" && this.ddlStorage.SelectedValue != "")
                {
                    var temp = (from a in db.AwaitScrap
                                where a.PileInfo.StorageInfo.StorageID == Convert.ToInt32(this.ddlStorage.SelectedValue)
                                select new
                                {
                                    Key = a.PileInfo.PileCode,
                                    Value = a.PileInfo.PileID
                                }).Distinct();

                    this.ddlPile.DataSource = temp;
                    this.ddlPile.DataTextField = "Key";
                    this.ddlPile.DataValueField = "Value";
                    this.ddlPile.DataBind();
                    this.ddlPile.Items.Insert(0, "--请选择--");
                }
                else
                {
                    if (this.ddlStorage.SelectedValue != "--请选择--")
                    {
                        this.ddlPile.Items.Insert(0, "--请选择--");
                        this.ddlPile.SelectedIndex = 0;
                    }
                }

            }
        }

        void dataclean_Click(object sender, ImageClickEventArgs e)
        {
            List<CheckBox> listString = GetCheckedID();
            CheckBox chb;
         

                if (listString.Count > 0)
                {
                    for (int i = 0; i < this.gv.Rows.Count; i++)
                    {
                        chb = (CheckBox)this.gv.Rows[i].Cells[0].Controls[0];
                        if (!chb.Checked)
                            continue;
                        if (this.gv.Rows[i].Cells[4].Controls[0] is TextBox)
                        {
                            itembox = (TextBox)this.gv.Rows[i].Cells[4].Controls[0];
                            itembox.Text = string.Empty;
                            this.txtReportNum.Text = string.Empty;
                        }


                    }
                }
            
        }

        void dataset_Click(object sender, ImageClickEventArgs e)
        {
            List<CheckBox> listString = GetCheckedID();
            CheckBox chb;


            if (this.txtReportNum.Text != string.Empty && this.txtReportNum.Text != "请输入...")
            {
                if (listString.Count > 0)
                {
                    for (int i = 0; i < this.gv.Rows.Count; i++)
                    {
                        chb = (CheckBox)this.gv.Rows[i].Cells[0].Controls[0];
                        if (!chb.Checked)
                            continue;
                        if (this.gv.Rows[i].Cells[4].Controls[0] is TextBox)
                        {
                            itembox = (TextBox)this.gv.Rows[i].Cells[4].Controls[0];
                            itembox.Text = this.txtReportNum.Text;
                        }


                    }
                }
                else
                {

                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要设定的物资!')</script>");
                    return;
                }


            }
            else
            {

                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请设定待报废报告号!')</script>");
                return;
            }
        }



        void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                List<CheckBox> listString = GetCheckedID();
           

                if (listString.Count > 0)
                {
                    if (CheckStringEmpty())
                    {

                        for (int i = 0; i < this.gv.Rows.Count; i++)
                        {
                            chb = (CheckBox)this.gv.Rows[i].Cells[0].Controls[0];
                            if (!chb.Checked)
                                continue;
                            if (this.gv.Rows[i].Cells[4].Controls[0] is TextBox)
                            {
                                itembox = (TextBox)this.gv.Rows[i].Cells[4].Controls[0];

                                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                                {
                                    AwaitScrap asp = db.AwaitScrap.SingleOrDefault(u => u.AwaitScrapID == int.Parse(this.gv.Rows[i].Cells[this.gv.Columns.Count-1].Text));
                                    asp.ScrapReportNum = itembox.Text.Trim();
                                    db.SubmitChanges();

                                }

                            }


                        }
                        Response.Redirect("AwaitScrapManage.aspx", false);
                    }
                    else
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请将选择的数据填写完整!')</script>");
                        return;
                    }

                }
                else
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择要提交的物资!')</script>");
                    return;
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_UPDATEERROR));
            }

           
        }


        protected void tbarbtnEdit_Click(object sender, EventArgs e)
        {
            Response.Redirect("QualityControlManage.aspx?TaskStorageID=" + Request.QueryString["TaskStorageID"] + "&&StorageInID=" + Request.QueryString["StorageInID"] + "");
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {

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

        /// <summary>
        /// 检查输入的合格数量和质检号码是否为空
        /// </summary>
        /// <returns></returns>
        private bool CheckStringEmpty()
        {

            for (int i = 0; i < this.gv.Rows.Count; i++)
            {
                chb = (CheckBox)this.gv.Rows[i].Cells[0].Controls[0];
                if (!chb.Checked)
                    continue;

                if (this.gv.Rows[i].Cells[4].Controls[0] is TextBox)
                {
                    tboxQualified = (TextBox)this.gv.Rows[i].Cells[4].Controls[0];
                    if (tboxQualified.Text == string.Empty)
                    {
                        return false;
                    }
                }

            }

            return true;
        }

        protected void tbarbtnDelte_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    List<CheckBox> listString = GetCheckedID();
            //    if (listString.Count > 0)
            //    {
            //        StorageInDetailed SID;
            //        using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            //        {
            //            foreach (var li in listString)
            //            {
            //                SID = db.StorageInDetailed.SingleOrDefault(a => a.StorageDetailedID == int.Parse(li.ToolTip));
            //                if (SID != null)
            //                {
            //                    db.StorageInDetailed.DeleteOnSubmit(SID);

            //                }
            //            }
            //            db.SubmitChanges();
            //        }
            //        Response.Redirect("StorageInDetailedManage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "");
            //    }
            //    else
            //    {
            //        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要删除的记录!')</script>");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MethodBase mb = MethodBase.GetCurrentMethod();
            //    LogToDBHelper lhelper = LogToDBHelper.Instance; lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
            //    ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_DELETEERROR));
            //}

        }

        /// <summary>
        /// 绑定Gridview
        /// </summary>
        private void BindGridView()
        {
            
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {

                if (this.ddlStorage.SelectedValue != "--请选择--" && this.ddlStorage.SelectedValue != "")
                {
                    storage = this.ddlStorage.SelectedItem.Text.ToString();

                }
                else
                {
                    storage = string.Empty;
                }
                if (this.ddlPile.SelectedValue != "--请选择--" && this.ddlPile.SelectedValue != "")
                {
                    f_Pile = this.ddlPile.SelectedItem.Text.ToString();
                }
                else
                {
                    f_Pile = string.Empty;
                }


                BoundField bfColumn;
                //添加选择列
                TemplateField tfieldCheckbox = new TemplateField();
                tfieldCheckbox.ItemTemplate = new CheckBoxTemplate("请选择", DataControlRowType.DataRow, "AwaitScrapID");
                tfieldCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
                this.gv.Columns.Insert(0, tfieldCheckbox);


                TemplateField reportNum = new TemplateField();
                reportNum.ItemTemplate = new MulTextBoxTemplate("请选择", DataControlRowType.DataRow, "", "AwaitScrapID", "txtReportNum");
                reportNum.HeaderTemplate = new MulTextBoxTemplate("待报废报告号", DataControlRowType.Header);
                reportNum.ItemStyle.Width = 150;
               
                foreach (var kvp in Titlelist)
                {
                    bfColumn = new BoundField();
                    bfColumn.HeaderText = kvp.Split(':')[0];
                    bfColumn.DataField = kvp.Split(':')[1];
                    this.gv.Columns.Add(bfColumn);
                }
                this.gv.Columns.Insert(4, reportNum);
               

                this.gv.DataSource = from a in db.AwaitScrap
                                     where a.State == "待报废"
                                     && a.MaterialInfo.FinanceCode.Contains(this.txtMaterialCode.Text.Trim())
                                     && a.MaterialInfo.MaterialName.Contains(this.txtMaterialName.Text.Trim())
                                     && a.StorageInfo.StorageName == (storage == "" ? a.StorageInfo.StorageName : storage)
                                     && a.PileInfo.PileCode == (f_Pile == "" ? a.PileInfo.PileCode : f_Pile)
                                     select new
                                     {
                                         a.AwaitScrapID,
                                         a.ScrapReportNum,
                                         a.MaterialInfo.MaterialName,
                                         a.MaterialInfo.FinanceCode,
                                         a.Gentaojian,
                                         a.MaterialInfo.SpecificationModel,
                                         a.StorageInfo.StorageName,
                                         a.PileInfo.PileCode,
                                         a.ProjectInfo.ProjectName,
                                         a.Manufacturer.ManufacturerName,
                                         a.State,
                                         a.Remark
                                     };
                this.gv.DataBind();
                this.gv.Columns[this.gv.Columns.Count - 1].Visible = false;
                Panel p1 = (Panel)GetControltByMaster("Panel1");

                p1.Controls.Add(this.gv);



            }

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
