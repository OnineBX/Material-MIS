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
    public class UploadExcelPile : System.Web.UI.Page
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
            Response.Redirect("PileManager.aspx");
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
        private int funGetUserIDbyAcc(string strAcc)
        {
            int n=0;
            using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                var tempEmp = dc.EmpInfo.FirstOrDefault(u => u.Account.Contains( strAcc));
                if (tempEmp == null)
                {
                    tempEmp = dc.EmpInfo.FirstOrDefault();
                   // ClientScript.RegisterClientScriptBlock(typeof(string), "Fail", "<script>alert('系统中不存在该员工，请添加该员工或者选择其他的员工')</script>");
                }
                if (tempEmp != null)
                {
                    n = tempEmp.EmpID;    
                }                
            }
            return n;
        }
        private string  InsertData(Net.SourceForge.Koogra.Excel.Row row)
        {
            string strResult = "";
            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //检查仓库
                if (row.Cells[0] == null || row.Cells[1] == null || row.Cells[2] ==null)
                    return "仓库信息不完整";
                //写入仓库信息
                StorageInfo si;
                int empid = 0;
                //检查仓库是否存在
                if (!db.StorageInfo.Any(a => a.StorageCode == row.Cells[0].Value.ToString()))
                {
                    si = new  StorageInfo();
                    //MT.MaterialMainTypeCode =     
                    si.StorageCode = row.Cells[0].Value.ToString();
                    si.StorageName = row.Cells[1].Value.ToString();
                    si.Remark = row.Cells[3] == null ? "" : row.Cells[3].Value.ToString();
                    empid = funGetUserIDbyAcc(row.Cells[2].Value.ToString());
                    if (empid == 0)
                    {
                        throw new Exception();
                    }
                    si.EmpID = empid;
                    db.StorageInfo.InsertOnSubmit(si);
                    db.SubmitChanges();
                }
                else
                {
                    si = db.StorageInfo.SingleOrDefault(a => a.StorageCode == row.Cells[0].Value.ToString());
                    si.StorageCode = row.Cells[0].Value.ToString();
                    si.StorageName = row.Cells[1].Value.ToString();
                    si.Remark = row.Cells[3] == null ? "" : row.Cells[3].Value.ToString();
                    empid = funGetUserIDbyAcc(row.Cells[2].Value.ToString());
                    if (empid == 0)
                    {
                        throw new Exception();
                    }
                    si.EmpID = empid;
                    db.SubmitChanges();
                }
//***************************************************************************
                //写入垛位信息
                if (row.Cells[4] == null || row.Cells[5] == null)
                    return "垛位信息不完整";
                //写入垛位信息
                PileInfo pi;
                //检查垛位是否存在
                if (!db.PileInfo.Any(a => a.PileCode == row.Cells[4].Value.ToString()))
                {
                    pi = new PileInfo();
                    //MT.MaterialMainTypeCode =     
                    pi.PileCode = row.Cells[4].Value.ToString();
                    pi.PileName = row.Cells[5].Value.ToString();
                    pi.PileSize = row.Cells[6] == null ? "" : row.Cells[6].Value.ToString();
                    pi.Remark = row.Cells[7] == null ? "" : row.Cells[7].Value.ToString();
                    pi.StorageID = si.StorageID;
                    db.PileInfo.InsertOnSubmit(pi);
                    db.SubmitChanges();
                }
                else
                {
                    pi = db.PileInfo.SingleOrDefault(a => a.PileCode== row.Cells[4].Value.ToString());
                    pi.PileCode = row.Cells[4].Value.ToString();
                    pi.PileName = row.Cells[5].Value.ToString();
                    pi.PileSize = row.Cells[6] == null ? "" : row.Cells[6].Value.ToString();
                    pi.Remark = row.Cells[7] == null ? "" : row.Cells[7].Value.ToString();
                    pi.StorageID = si.StorageID;
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

