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
    public class WellOwnerCreater : System.Web.UI.Page
    {
        //TextBox txtWellOwnerName;
        //TextBox txtWellOwnerCode;     
        ////TextBox txtRemark;
        //Button btnSave;
        //Button btnQuit;
        //protected void Page_Load(object sender, EventArgs e)
        //{
        //    //ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('hello ')</script>");
        //    InvtControl();
           
        //}

        //private void InvtControl()
        //{
        //    this.txtWellOwnerName = (TextBox)GetControltByMaster("txtWellOwnerName");
        //    this.txtWellOwnerCode = (TextBox)GetControltByMaster("txtWellOwnerCode");        
        //   // this.txtRemark = (TextBox)GetControltByMaster("txtRemark");
        //    this.btnSave = (Button)GetControltByMaster("btnSave");
        //    this.btnQuit = (Button)GetControltByMaster("btnQuit");
        //    this.btnSave.Click += new EventHandler(btnSave_Click);
        //    this.btnQuit.CausesValidation = false;
        //    this.btnQuit.Click += new EventHandler(btnQuit_Click);
        //}
    

        //void btnQuit_Click(object sender, EventArgs e)
        //{
        //    Response.Redirect("WellOwnerManager.aspx");
        //}

        //void btnSave_Click(object sender, EventArgs e)
        //{

        //    if (!string.IsNullOrEmpty(this.txtWellOwnerName.Text) && !string.IsNullOrEmpty(this.txtWellOwnerCode.Text))
        //    {
        //        using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))                
        //        {
        //           //检查唯一性
        //            var temp = db.WellOwner.SingleOrDefault(a => a.WellOwnerCode == this.txtWellOwnerCode.Text.Trim());
        //            if (temp == null)
        //            {
        //                WellOwner ei = new WellOwner();
        //                ei.WellOwnerName = this.txtWellOwnerName.Text.Trim();    
        //                ei.WellOwnerCode = this.txtWellOwnerCode.Text.Trim();
        //              //  ei.Remark = this.txtRemark.Text.Trim();
        //                db.WellOwner.InsertOnSubmit(ei);
        //                db.SubmitChanges();
        //                Response.Redirect("WellOwnerManager.aspx");
        //            }
        //            else
        //            {
        //                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('业主单位编码已存在')</script>");
        //            }
        //        }
        //    }
        //}
        ///// <summary>
        ///// 获取控件
        ///// </summary>
        ///// <param name="controlName">控件的ID名称</param>
        ///// <returns>返回Control，需要强制类型转换为对应控件</returns>
        //protected Control GetControltByMaster(string controlName)
        //{
        //    return this.Master.FindControl("PlaceHolderMain").FindControl(controlName);
        //}
    }
}

