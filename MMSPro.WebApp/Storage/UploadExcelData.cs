/*------------------------------------------------------------------------------
 * Unit Name：UploadExcelData.cs
 * Description: 正常入库--Excel数据上传页
 * Author: Zheng Ping
 * Created Date: 2011-2-22
 * ----------------------------------------------------------------------------*/
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
    public class UploadExcelData:System.Web.UI.Page
    {
        FileUpload fu;
        Button btnSave;
        Button btnQuit;
        Image img;
        
        protected void Page_Load(object sender, EventArgs e)
        {
          
            InvtControl();

        }

        private void InvtControl()
        {
           
            this.fu = (FileUpload)GetControltByMaster("FileUpload1");
            this.btnSave = (Button)GetControltByMaster("btnSave");
            this.btnQuit = (Button)GetControltByMaster("btnQuit");
            this.img = (Image)GetControltByMaster("loading");
            this.btnSave.Click += new EventHandler(btnSave_Click);
            this.btnQuit.CausesValidation = false;
            this.btnQuit.Click += new EventHandler(btnQuit_Click);
        }

        void btnQuit_Click(object sender, EventArgs e)
        {
            
        }

        void btnSave_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                if (fu.FileBytes.Length > 0)
                {
                    Net.SourceForge.Koogra.Excel.Workbook wb = new Net.SourceForge.Koogra.Excel.Workbook(fu.FileContent);
                  
                    for (int i = 1; i <= wb.Sheets[1].Rows.MaxRow; i++)
                    {
                        InsertData(wb.Sheets[1].Rows[(uint)i]);
                    }
                    this.img.Visible = false;
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('导入数据完毕');var s = new Object();s.type='';window.returnValue=s;window.close();</script>");
                }

            }
            catch
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('导入数据错误,请检查导入文件,仅2003格式可以导入 ')</script>");
            }
        }


        //数据导入
        private void InsertData(Net.SourceForge.Koogra.Excel.Row row)
        {

            //loading图标
            //this.img.Visible = true;
            //Response.AddHeader("Refresh", "0");

            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                //判断必填字段是否有空数据
                for (int i = 0; i < 8; i++)
                {
                    if (row.Cells[(uint)i] == null)
                    {
                        ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('数据中有必填字段为空')</script>");
                        return;
                    }
                }

                //判断是否有此入库单
                StorageInMain si = db.StorageInMain.SingleOrDefault(u => u.StorageInCode == row.Cells[0].Value.ToString().Trim());
                if (si == null)
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('待导入的入库单号[ " + row.Cells[0].Value.ToString().Trim() + " ] 不存在')</script>");
                    return;
                }

                //判断是否有物料编码
                MaterialInfo mi = db.MaterialInfo.SingleOrDefault(u => u.FinanceCode == row.Cells[1].Value.ToString().Trim());
                if (mi == null)
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('待导入的物料编码[ " + row.Cells[1].Value.ToString().Trim() + " ] 不存在,请先建立物料后再尝试导入')</script>");
                    return;
                }

                //判断数据格式
                //根/套/件
                if (!Utility.Security.ValidString(row.Cells[2].Value.ToString().Trim(), Utility.CodeValideType.零或者非零开头的整数))
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('根/套/件数量只能是整数')</script>");
                    return;
                }
                //米
                if (!Utility.Security.ValidString(row.Cells[3].Value.ToString().Trim(), Utility.CodeValideType.带两位小数的正实数))
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('米的数量只能是整数或者带两位小数的正实数')</script>");
                    return;
                }

                //吨
                if (!Utility.Security.ValidString(row.Cells[4].Value.ToString().Trim(), Utility.CodeValideType.带两位小数的正实数))
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('吨的数量只能是整数或者带两位小数的正实数')</script>");
                    return;
                }



                //判断有无项目信息
                ProjectInfo pi = db.ProjectInfo.SingleOrDefault(u => u.ProjectCode == row.Cells[5].Value.ToString().Trim());
                if (pi == null)
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('待导入的项目编码[ " + row.Cells[5].Value.ToString().Trim() + " ] 不存在,请先建立项目信息后再尝试导入')</script>");
                    return;
                }
                //预计到库时间

                if (ConvData(row.Cells[6].Value.ToString().Trim()).ToString() != "1900-1-1 0:00:00")
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('入库时间不和规范,请按年-月-日格式，如：2010-10-10')</script>");
                    return;
                }

                //判断批次信息
                if (!ValideBatch(row.Cells[7].Value.ToString()))
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('" + row.Cells[7].Value.ToString() + "处 格式错误！请按此格式输入，如：第一批,第二批,最多十批')</script>");
                    return;
                }

                //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('"+row.Cells[7].Value.ToString()+" 检查完毕一切正常')</script>");


               
                
    


                //开始数据导入
                StorageProduce SID = new StorageProduce();


                SID.StorageInID = Convert.ToInt32(Request.QueryString["storageInID"]);

                //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('" + SID.StorageInID + "')</script>");

                SID.MaterialID = (db.MaterialInfo.SingleOrDefault(u => u.FinanceCode == row.Cells[1].Value.ToString().Trim())).MaterialID;

                SID.QuantityGentaojian = Convert.ToDecimal(row.Cells[2].Value.ToString().Trim());
                SID.QuantityMetre = Convert.ToDecimal(row.Cells[3].Value.ToString().Trim());
                SID.QuantityTon = Convert.ToDecimal(row.Cells[4].Value.ToString().Trim());

                SID.ExpectedProject = (db.ProjectInfo.SingleOrDefault(u => u.ProjectCode == row.Cells[5].Value.ToString().Trim())).ProjectID;
                SID.ExpectedTime = ConvData(row.Cells[6].Value.ToString().Trim());

                SID.BatchIndex = row.Cells[7].Value.ToString();
                var SevTime = db.ExecuteQuery<DateTime>("select  getdate()", new object[] { });
                SID.CreateTime = SevTime.First();
                SID.Creator = reEmpId(SPContext.Current.Web.CurrentUser.LoginName);


                if (row.Cells.MaxCol>7)
                {
                    SID.Remark = row.Cells[8].Value.ToString();
                    
                }
                else
                {
                    SID.Remark = string.Empty;
                }
              

                db.StorageProduce.InsertOnSubmit(SID);
                db.SubmitChanges();


            }
        }

        private bool ValideBatch(string batch)
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

           return li.Exists(u => u == batch);
           

        }

        private DateTime ConvData(string strData)
        {
            DateTime data = Convert.ToDateTime("1900-1-1");
            try
            {
                data = Convert.ToDateTime(strData);
                return data;
            }
            catch
            {
               
                return data;
            }
        }

        private int reEmpId(string Emptbox)
        {
            int reID = 0;
            using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
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
