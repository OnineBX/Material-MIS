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
using Utility;
using System.Reflection;
namespace MMSPro.WebApp
{
    public class CommitStorageDetailedCreate : System.Web.UI.Page
    {
        MMSProDBDataContext db;
        TextBox txtStorageinNum;
        

        //TextBox txtMaterialName;
        //TextBox txtMaterialmodel;
        TextBox txtMaterialCode;
        TextBox txtMaterialMod;
        TextBox txtID;
        TextBox txtFinanceCode;
        TextBox txtCommitMaterial;
        TextBox txtcommitID;

        TextBox txtGTJ;
        TextBox txtMetre;
        TextBox txtTon;

        DropDownList ddlproject;
        DateTimeControl DateTimeStorageIn;

        DropDownList ddlbatch;
        TextBox txtRemark;

        Button btnSave;
        Button btnQuit;

        Literal L1;
        Literal L2;
        Literal L3;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                InitControl();
                selectUser(this.txtMaterialCode, this.txtID, this.txtMaterialMod, this.txtFinanceCode, "../StorageCommit/SelectMaterial.aspx");
                selectCommitMaterial(this.txtCommitMaterial, this.txtcommitID, "../StorageCommit/SelectCommitMaterial.aspx");
                //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello ')</script>");

               

                if (!IsPostBack)
                {
                    BindProject();
                    BindDefaultDate();

                    using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {
                        var bi = from a in db.BatchOfIndex
                                 select a;
                        if (bi.ToArray().Length < 1)
                        {
                            batchCount();
                        }
                    }


                    BindBatch();
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }

            
     
           


        }



        private void selUser(TextBox tbox_M,TextBox tbox_W)
        {
           
            
            L1 = (Literal)GetControltByMaster("L1");
            L1.Text = JSDialogAid.GetJSForDialog(tbox_M.ClientID,tbox_W.ClientID,"", "../StorageAndPile/SelectUser.aspx");
        }


        /// <summary>
        /// 接受模式窗体返回数据
        /// </summary>
        /// <param name="tb">textbox对象</param>
        /// <param name="lb">lable对象</param>
        /// <param name="url">url</param>
        private void selectUser(TextBox tb, TextBox txtid, TextBox tmod, TextBox tfid, string url)
        {

            L3 = (Literal)GetControltByMaster("L3");
            L3.Text = JSDialogAid.GetDialogInfo(tb.ClientID, txtid.ClientID,tmod.ClientID,tfid.ClientID, url);
        }
        
        /// <summary>
        /// 接受模式窗体返回数据
        /// </summary>
        /// <param name="tb">textbox对象</param>
        /// <param name="lb">lable对象</param>
        /// <param name="url">url</param>
        private void selectCommitMaterial(TextBox Material, TextBox txtcommitID, string url)
        {

            L2 = (Literal)GetControltByMaster("L2");
            L2.Text = JSDialogAid.GetDialogInfo(Material.ClientID, txtcommitID.ClientID,url);
        }

        private void BindDefaultDate()
        { 
            int id=0;
            if (!string.IsNullOrEmpty(Request.QueryString["StorageInID"]))
            {
                id = Convert.ToInt32(Request.QueryString["StorageInID"]);
                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    CommitInMain temp = db.CommitInMain.SingleOrDefault(u => u.StorageInID == id);
                    this.txtStorageinNum.Text = temp.StorageInCode.ToString();
                    this.txtStorageinNum.Enabled = false;
                }
            }
            else
            {
                Response.Redirect("StorageManage.aspx");
            }

            
        }
        private void InitControl()
        {

            this.txtStorageinNum = (TextBox)GetControltByMaster("txtStorageinNum");

            this.txtMaterialCode = (TextBox)GetControltByMaster("txtMaterialCode");
            this.txtMaterialMod = (TextBox)GetControltByMaster("txtMaterialMod");
            this.txtID = (TextBox)GetControltByMaster("txtID");
            this.txtFinanceCode = (TextBox)GetControltByMaster("txtFinanceCode");

            this.txtCommitMaterial = (TextBox)GetControltByMaster("txtCommitMaterial");
            this.txtcommitID = (TextBox)GetControltByMaster("txtcommitID");

            this.txtGTJ = (TextBox)GetControltByMaster("txtGTJ");
            this.txtMetre = (TextBox)GetControltByMaster("txtMetre");
            this.txtTon = (TextBox)GetControltByMaster("txtTon");

            this.ddlproject = (DropDownList)GetControltByMaster("ddlproject");
            this.DateTimeStorageIn = (DateTimeControl)GetControltByMaster("DateTimeStorageIn");


            this.ddlbatch = (DropDownList)GetControltByMaster("ddlbatch");

            this.txtRemark = (TextBox)GetControltByMaster("txtRemark");



            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);

        }







        private void BindBatch()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {

                Dictionary<string, string> dic = new Dictionary<string, string>();

                var list = from a in db.BatchOfIndex

                           select new { a.BatchOfIndexName, a.BatchOfIndexID };
                var templist = list.ToList();

                var tempBatch = from a in db.CommitInMaterials
                                where a.CommitProduce.StorageInID == Convert.ToInt32(Request.QueryString["StorageInID"].Trim())
                                select new { a.CommitProduce.BatchIndex };
                var le = tempBatch.ToList();
                for (int i = 0; i < le.Count; i++)
                {
                    for (int x = 0; x < templist.Count; x++)
                    {
                        if (templist[x].BatchOfIndexName.ToString() == le[i].BatchIndex.ToString())
                        {
                            templist.Remove(templist[x]);
                        }
                    }
                }


                for (int i = 0; i < templist.Count; i++)
                {
                    dic.Add(templist[i].BatchOfIndexID.ToString(), templist[i].BatchOfIndexName.ToString());
                }

                this.ddlbatch.DataSource = dic;
                this.ddlbatch.DataTextField = "Value";
                this.ddlbatch.DataValueField = "Key";
                this.ddlbatch.DataBind();
                this.ddlbatch.Items.Insert(0, "--请选择--");
            } 
        }




        private void BindProject()
        {
            using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var temp = from a in db.ProjectInfo
                           select new
                           {
                               Key = a.ProjectName,
                               Value = a.ProjectID
                           };

                this.ddlproject.DataSource = temp;
                this.ddlproject.DataTextField = "Key";
                this.ddlproject.DataValueField = "Value";
                this.ddlproject.DataBind();
                this.ddlproject.Items.Insert(0, "--请选择--");
            }
        }
        /// <summary>
        /// 初始化批次，系统第一次运行时将创建到数据库
        /// </summary>
        private void batchCount()
        {
            using (MMSProDBDataContext dk = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                List<string> li = new List<string>();
                li.Add("第一批");
                li.Add("第二批");
                li.Add("第三批");
                li.Add("第四批");
                li.Add("第五批");
                li.Add("第六批");
                li.Add("第七批");
                li.Add("第八批");
                li.Add("第九批");
                li.Add("第十批");


                
                for (int i = 0; i < 10; i++)
                {
                    BatchOfIndex bic = new BatchOfIndex();
                    bic.BatchOfIndexName = li[i].ToString();
                    dk.BatchOfIndex.InsertOnSubmit(bic);
                    dk.SubmitChanges();
                }
            }
        }


        public void btnSave_Click(object sender, EventArgs e)
        {

            try
            {

                using (db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {

                   

                    CommitProduce SID = new CommitProduce();
         

                    SID.StorageInID = Convert.ToInt32(Request.QueryString["StorageInID"]);
                    SID.MaterialID =Convert.ToInt32( this.txtID.Text.Trim());

                    SID.QuantityGentaojian = Convert.ToDecimal(this.txtGTJ.Text.Trim());
                    SID.QuantityMetre = Convert.ToDecimal(this.txtMetre.Text.Trim());
                    SID.QuantityTon = Convert.ToDecimal(this.txtTon.Text.Trim());




                    if (this.ddlproject.SelectedIndex == 0)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择预期使用项目！')</script>");
                        return;
                    }


                    SID.ExpectedProject = Convert.ToInt32(this.ddlproject.SelectedValue.Trim());

      
                    SID.ExpectedTime = this.DateTimeStorageIn.SelectedDate;


                    if (this.ddlbatch.SelectedItem.Text == "--请选择--")
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择入库批次')</script>");
                        return;
                    }

                    SID.BatchIndex = this.ddlbatch.SelectedItem.Text.ToString();
                    var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                    SID.CreateTime = SevTime.First();
                    SID.Creator = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);
                    SID.Remark = this.txtRemark.Text.Trim();

                    db.CommitProduce.InsertOnSubmit(SID);
                    db.SubmitChanges();
                   

                    //写入委外关系表
                    RelationCommitIn rci = new RelationCommitIn();
                    rci.CommitMaterial = SID.StorageInProduceID;
                    rci.CommitOutMaterial = this.txtcommitID.Text.Trim();
                    var STime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                    rci.CreateTime = STime.First();
                    db.RelationCommitIn.InsertOnSubmit(rci);
                    db.SubmitChanges();


                }
                if (!string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
                {
                    Response.Redirect("StorageDetailedManage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "");
                }
                else
                {
                    Response.Redirect("StorageDetailedManage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "");
                }
            }
            catch (Exception ex)
            {
                MethodBase mb = MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_INSERTERROR));
            }

        }
        private int reEmpId(string Emptbox)
        {
            int reID = 0;
            using ( MMSProDBDataContext dc= new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {

                EmpInfo ei = dc.EmpInfo.SingleOrDefault(u => u.Account == Emptbox);
                if (ei == null)
                {
                    return 0;
                }
                reID = ei.EmpID;

            }
            return reID;
        }
        public void btnQuit_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request.QueryString["TaskStorageID"]))
            {
                Response.Redirect("StorageDetailedManage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "&&TaskStorageID=" + Request.QueryString["TaskStorageID"] + "");
            }
            else
            {
                Response.Redirect("StorageDetailedManage.aspx?StorageInID=" + Request.QueryString["StorageInID"] + "");
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
