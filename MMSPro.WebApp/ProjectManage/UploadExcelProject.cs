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
    public class UploadExcelProject : System.Web.UI.Page
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
            Response.Redirect("ManufacturerManager.aspx");
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
                //检查往来单位类别
                if (row.Cells[0] == null || row.Cells[1] == null)
                    return "往来单位类别信息不完整";
                //写入往来单位类别信息
                BusinessUnitType but;
                //检查往来单位类别是否存在
                if (!db.BusinessUnitType.Any(a => a.BusinessUnitTypeName == row.Cells[1].Value.ToString()))
                {
                    but = new BusinessUnitType();
                    //MT.MaterialMainTypeCode =     
                    but.BusinessUnitTypeCode = row.Cells[0].Value.ToString();
                    but.BusinessUnitTypeName = row.Cells[1].Value.ToString();
                    but.Remark = row.Cells[2] == null ? "" : row.Cells[2].Value.ToString();
                    db.BusinessUnitType.InsertOnSubmit(but);
                    db.SubmitChanges();
                }
                else
                {
                    but = db.BusinessUnitType.SingleOrDefault(a => a.BusinessUnitTypeName == row.Cells[1].Value.ToString());
                    but.BusinessUnitTypeCode = row.Cells[0].Value.ToString();
                    but.BusinessUnitTypeName = row.Cells[1].Value.ToString();
                    but.Remark = row.Cells[2] == null ? "" : row.Cells[2].Value.ToString();
                    db.SubmitChanges();
                }
//***************************************************************************
                //写入单位信息
                if (row.Cells[3] == null || row.Cells[4] == null)
                    return "单位信息不完整";
                //写入单位信息
                BusinessUnitInfo bui;
                //检查单位是否存在
                if (!db.BusinessUnitInfo.Any(a => a.BusinessUnitCode == row.Cells[3].Value.ToString()))
                {
                    bui = new BusinessUnitInfo();
                    //MT.MaterialMainTypeCode =     
                    bui.BusinessUnitCode = row.Cells[3].Value.ToString();
                    bui.BusinessUnitName = row.Cells[4].Value.ToString();
                    bui.Remark = row.Cells[5] == null ? "" : row.Cells[5].Value.ToString();
                    bui.BusinessUnitTypeID = but.BusinessUnitTypeID;
                    db.BusinessUnitInfo.InsertOnSubmit(bui);
                    db.SubmitChanges();
                }
                else
                {
                    bui = db.BusinessUnitInfo.SingleOrDefault(a => a.BusinessUnitCode == row.Cells[3].Value.ToString());
                    bui.BusinessUnitCode = row.Cells[3].Value.ToString();
                    bui.BusinessUnitName = row.Cells[4].Value.ToString();
                    bui.BusinessUnitTypeID = but.BusinessUnitTypeID;
                    bui.Remark = row.Cells[5] == null ? "" : row.Cells[5].Value.ToString();
                    db.SubmitChanges();
                }
//***************************************************************************
                //写项目信息
                if (row.Cells[6] == null || row.Cells[7] == null)
                    return "项目信息不完整";
                ProjectInfo pi;                             
                //检查生产厂商类别是否存在
                if (!db.ProjectInfo.Any(a => a.ProjectCode == row.Cells[6].Value.ToString()))
                {
                    pi = new ProjectInfo();
                    //MT.MaterialMainTypeCode =     
                    pi.ProjectCode = row.Cells[6].Value.ToString();
                    pi.ProjectName = row.Cells[7].Value.ToString();
                    pi.Remark = row.Cells[8] == null ? "" : row.Cells[8].Value.ToString();
                    pi.Owner = bui.BusinessUnitID;
                    db.ProjectInfo.InsertOnSubmit(pi);
                    db.SubmitChanges();
                }
                else
                {
                    pi = db.ProjectInfo.SingleOrDefault(a => a.ProjectCode == row.Cells[6].Value.ToString());
                    pi.ProjectCode = row.Cells[6].Value.ToString();
                    pi.ProjectName = row.Cells[7].Value.ToString();
                    pi.Remark = row.Cells[8] == null ? "" : row.Cells[8].Value.ToString();
                    pi.Owner = bui.BusinessUnitID;
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

