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
    public class ScrappedSet: System.Web.UI.Page
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
        int AapId;

        //搜索
        DropDownList ddlStorage;
        DropDownList ddlPile;
        TextBox txtMaterialCode;
        TextBox txtMaterialName;
        TextBox txtMaterials;
        TextBox txtID;
        TextBox txtType;
        private string storage;
        private string f_Pile;

        Literal L2;

        static string[] Titlelist = {
                                     "物料名称:MaterialName",
                                     "财务编码:FinanceCode",
                                     "物料规格:SpecificationModel",
                                     "待报废报告号:ScrapReportNum",
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
                selectUser(this.txtMaterials, this.txtID,this.txtType, "../MaterialScrapped/SelectMaterial.aspx");

                BindGridView();
                

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
        /// 接受模式窗体返回数据
        /// </summary>
        /// <param name="tb">textbox对象</param>
        /// <param name="lb">lable对象</param>
        /// <param name="url">url</param>
        private void selectUser(TextBox TM, TextBox Tid, TextBox TT, string url)
        {

            L2 = (Literal)GetControltByMaster("L2");
            L2.Text = JSDialogAid.GetMaterialInfo(TM.ClientID, Tid.ClientID,TT.ClientID, url);
        }

        private void toolbarInit()
        {

            //添加按钮到toolbar
            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarbusiness");
            //返回
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
            this.txtMaterials = (TextBox)GetControltByMaster("txtMaterials");
            this.txtID = (TextBox)GetControltByMaster("txtID");
            this.txtType = (TextBox)GetControltByMaster("txtType");

            this.btnSave = (Button)GetControltByMaster("btnSave");
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

             
                    if (listString.Count != 1)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择一条且仅能有一条记录进行报废!')</script>");
                        return;
                    }
                   

                    if (CheckReportNum())
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
                                        AapId = int.Parse(this.gv.Rows[i].Cells[this.gv.Columns.Count - 1].Text);

                                        AwaitScrap asp = db.AwaitScrap.SingleOrDefault(u => u.AwaitScrapID == AapId);
                                        asp.State = "已报废";

                                        Scrapped sp = new Scrapped();
                                        sp.AwaitScrapID = AapId;
                                        sp.StockID = Convert.ToInt32( this.txtID.Text.Trim());
                                        sp.StockType = this.txtType.Text.Trim();
                                        sp.ScrappedNum = itembox.Text.Trim();
                                        var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                                        sp.ScrappedTime = SevTime.First();
                                        var Time = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                                        sp.CreateTime = Time.First();
                                        sp.Creator = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
                                        db.Scrapped.InsertOnSubmit(sp);
                                        db.SubmitChanges();

                                    }

                                }


                            }
                            Response.Redirect("ScrappedInfo.aspx", false);
                        }
                        else
                        {
                            ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请将选择的数据填写完整!')</script>");
                            return;
                        }
                    }
                    else
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('所选物资中存在未设定待报废报告号的物资!')</script>");
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

        private int reEmpId(string Emp)
        {
            int valueEmp = 0;
            using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                EmpInfo EI = dc.EmpInfo.SingleOrDefault(u => u.Account == Emp);
                if (EI != null)
                {
                    valueEmp = EI.EmpID;
                }

            }

            return valueEmp;
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
        /// <summary>
        /// 检查是否设定了待报废报告号
        /// </summary>
        /// <returns></returns>
        private bool CheckReportNum()
        {
            for (int i = 0; i < this.gv.Rows.Count; i++)
            { 
                 chb = (CheckBox)this.gv.Rows[i].Cells[0].Controls[0];
                 if (!chb.Checked)
                     continue;
                 if (this.gv.Rows[i].Cells[5].Text == "未填写")
                 {
                     return false;
                 }
            }


                return true;
        }
        

        protected void tbarbtnDelte_Click(object sender, EventArgs e)
        {
           
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
                reportNum.HeaderTemplate = new MulTextBoxTemplate("报废文件号", DataControlRowType.Header);
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
