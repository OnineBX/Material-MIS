using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
namespace MMSPro.WebApp
{
    public class UploadExcel : System.Web.UI.Page
    {
        //TextBox txtSupplierID;
        FileUpload fu;
        Button btnSave;
        Button btnQuit;
        protected void Page_Load(object sender, EventArgs e)
        {
            //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello ')</script>");
            InvtControl();
          
        }

        private void InvtControl()
        {
            //this.txtSupplierID = (TextBox)GetControltByMaster("txtSupplierID");
            //this.txtMaterialName = (TextBox)GetControltByMaster("txtMaterialName");
            //this.txtMaterialCode = (TextBox)GetControltByMaster("txtMaterialCode");
            //this.txtMeasuringUnit = (TextBox)GetControltByMaster("txtMeasuringUnit");
            //this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
            //this.ddlMaterialType = (DropDownList)GetControltByMaster("ddlMaterialType");
            this.fu = (FileUpload)GetControltByMaster("FileUpload1");
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
        }


        void btnQuit_Click(object sender, EventArgs e)
        {
            Response.Redirect("MatManager.aspx");
        }

        void btnSave_Click(object sender, EventArgs e)
        {

            StringBuilder sb = new StringBuilder();
            try
            {
                    if (fu.FileBytes.Length > 0)
                    {
                        Net.SourceForge.Koogra.Excel.Workbook wb = new Net.SourceForge.Koogra.Excel.Workbook(fu.FileContent);
                        for (int i = 1; i <= wb.Sheets[0].Rows.MaxRow; i++)
                        {
                            sb.Append(InsertData(wb.Sheets[0].Rows[(uint)i]));
                        }
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('导入数据完毕')</script>");
                    }
            
            }
            catch
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('导入数据错误,请检查导入文件,仅2003格式可以导入 ')</script>");
            }
        
        }

        private string  InsertData(Net.SourceForge.Koogra.Excel.Row row)
        {
            string strResult = "";
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //分段获取4种值,父类无值直接跳出
                //获取主类
                if (row.Cells[0] == null || row.Cells[1] == null)
                    return "主类信息不完整";
                MaterialType Temp;
                //检查大类是否存在
                if (!db.MaterialType.Any(a => a.MaterialTypeCode == row.Cells[0].Value.ToString()))
                {
                    Temp = new MaterialType();
                    //MT.MaterialMainTypeCode =     
                    Temp.MaterialTypeCode = row.Cells[0].Value.ToString();
                    Temp.MaterialTypeName = row.Cells[1].Value.ToString();
                    db.MaterialType.InsertOnSubmit(Temp);
                    db.SubmitChanges();
                }
                else
                {
                    Temp = db.MaterialType.SingleOrDefault(a => a.MaterialTypeCode == row.Cells[0].Value.ToString());
                    Temp.MaterialTypeCode = row.Cells[0].Value.ToString();
                    Temp.MaterialTypeName = row.Cells[1].Value.ToString();
                    db.SubmitChanges();
                }
                //获取大类
                if (row.Cells[2] == null ||row.Cells[3] == null)
                    return "大类信息不完整";
                 MaterialMainType MT;
                //检查大类是否存在
                 if (!db.MaterialMainType.Any(a => a.MaterialMainTypeCode == row.Cells[2].Value.ToString()))
                 {
                     MT = new MaterialMainType();
                     //MT.MaterialMainTypeCode =     
                     MT.MaterialMainTypeCode = row.Cells[2].Value.ToString();
                     MT.MaterialMainTypeName = row.Cells[3].Value.ToString();
                     MT.MaterialTypeID = Temp.MaterialTypeID;
                     db.MaterialMainType.InsertOnSubmit(MT);
                     db.SubmitChanges();
                 }
                 else
                 {
                     MT = db.MaterialMainType.SingleOrDefault(a => a.MaterialMainTypeCode == row.Cells[2].Value.ToString());
                     MT.MaterialMainTypeCode = row.Cells[2].Value.ToString();
                     MT.MaterialMainTypeName = row.Cells[3].Value.ToString();
                     MT.MaterialTypeID = Temp.MaterialTypeID;
                     db.SubmitChanges();
                 }
                //获取中类
                 if (row.Cells[4] == null || row.Cells[5] == null)
                     return "";
                 MaterialChildType mct;
                 if (!db.MaterialChildType.Any(a => a.MaterialChildTypeCode == row.Cells[4].Value.ToString()))
                 {
                     mct = new MaterialChildType();
                     mct.MaterialChildTypeCode = row.Cells[4].Value.ToString();
                     mct.MaterialChildTypeName = row.Cells[5].Value.ToString();
                     mct.MaterialMainTypeID = MT.MaterialMainTypeID;
                     db.MaterialChildType.InsertOnSubmit(mct);
                     db.SubmitChanges();
                 }
                 else
                 {
                     mct = db.MaterialChildType.SingleOrDefault(a => a.MaterialChildTypeCode == row.Cells[4].Value.ToString());
                     mct.MaterialChildTypeCode = row.Cells[4].Value.ToString();
                     mct.MaterialChildTypeName = row.Cells[5].Value.ToString();
                     mct.MaterialMainTypeID = MT.MaterialMainTypeID;
                     db.SubmitChanges();
                 }
                //获取小类
                 if (row.Cells[6] == null || row.Cells[7] == null)
                     return "";
                 MaterialInfo mi;
                 if (!db.MaterialInfo.Any(a => a.FinanceCode == row.Cells[6].Value.ToString()))
                 {
                     mi = new MaterialInfo();
                     mi.MaterialchildTypeID = mct.MaterialChildTypeID;
                     mi.FinanceCode = row.Cells[6].Value.ToString();
                     mi.MaterialName = row.Cells[7].Value.ToString();
                     mi.SpecificationModel = row.Cells[8].Value.ToString() ?? "";
                     mi.Remark = row.Cells[9].Value.ToString() ?? "";
                     db.MaterialInfo.InsertOnSubmit(mi);
                     db.SubmitChanges();
                 }
                 else
                 {
                     mi = db.MaterialInfo.SingleOrDefault(a => a.FinanceCode == row.Cells[6].Value.ToString());
                     mi.MaterialchildTypeID = mct.MaterialChildTypeID;
                     mi.FinanceCode = row.Cells[6].Value.ToString();
                     mi.MaterialName = row.Cells[7].Value.ToString();
                     mi.SpecificationModel = row.Cells[8].Value.ToString() ?? "";
                     mi.Remark = row.Cells[9].Value.ToString() ?? "";
                     db.SubmitChanges();
                 }

            }
            return strResult;
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

