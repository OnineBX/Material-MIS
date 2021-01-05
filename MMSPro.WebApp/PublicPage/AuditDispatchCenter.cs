/*------------------------------------------------------------------------------
 * Unit Name：AuditDispatchCenter.cs
 * Description: 公用页面--审批处理中心页面
 * Author: Xu Chun Lei
 * Created Date: 2010-07-16
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

namespace MMSPro.WebApp
{
    public class AuditDispatchCenter:System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            using (MMSProDBDataContext db = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {
                string strBackUrl,strDisposeUrl;
                int noticeid = Convert.ToInt32(Request.QueryString["NoticeID"]);
                int commitinid = Convert.ToInt32(Request.QueryString["CommitInID"]);//统一用NoticeID，待修改                
                switch (Request.QueryString["Process"])
                {
                    case "委外入库"://edit by adonis
                        strBackUrl = "../StorageCommitIn/CommitInManage.aspx";

                        //已经发送物资调拨审核的情况
                        int count = db.TaskStorageIn.Count(u => u.StorageInID == commitinid && u.StorageInType.Equals("委外入库"));

                        //没有物资明细的情况
                        if (db.CommitInDetailed.Count(u => u.CommitInID == commitinid) == 0)
                        {
                            strDisposeUrl = string.Format("../StorageCommitIn/CommitInDetailedManage.aspx?CommitInID={0}", commitinid);
                            Response.Redirect(string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=该调拨单没有物资明细，不能发送审核！&DisposeUrl={0}&BackUrl={1}", strDisposeUrl, strBackUrl), false);
                            return;
                        }

                        if (count > 0)
                        {
                            strDisposeUrl = strBackUrl;
                            Response.Redirect(string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=该入库单已经进入流程，不能重复发送！&DisposeUrl={0}&BackUrl={1}", strDisposeUrl, strBackUrl));
                            return;
                        }
                        Response.Redirect("../StorageCommitIn/TaskCommitIn.aspx?CommitInID=" + commitinid + "&&state=质检&&storageInType=委外入库");


                        break;

                    case "委外出库":
                        strBackUrl = "../StorageCommitOut/ManageCommitOutNotice.aspx";
                        //调拨单中没有物资明细的情况
                        if (db.StorageCommitOutDetails.Count(u => u.StorageCommitOutNoticeID == noticeid) == 0)
                        {
                            strDisposeUrl = string.Format("../StorageCommitOut/SelectCommitOutDetails.aspx?NoticeID={0}", noticeid);                            
                            Response.Redirect(string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=该调拨单没有物资明细，不能发送审核！&DisposeUrl={0}&BackUrl={1}", strDisposeUrl, strBackUrl),false);
                            return;
                        }

                        //已经发送物资调拨审核的情况                        
                        if (db.StorageOutTask.Count(u => u.NoticeID.Equals(noticeid) && u.TaskType.Equals("物资调拨审核") && u.Process.Equals("委外出库")) != 0)
                        {
                            strDisposeUrl = strBackUrl;                           
                            Response.Redirect(string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=该调拨单已经发送生产组长审核，不能重复发送！&DisposeUrl={0}&BackUrl={1}", strDisposeUrl, strBackUrl));
                            return;
                        }
                        Response.Redirect(string.Format("../StorageCommitOut/CreateCommitOutTask.aspx?NoticeID={0}&TaskType=物资调拨审核",noticeid));
                    break;

                    case "正常出库":
                    strBackUrl = "../StorageOut/ManageStorageOutNotice.aspx";
                    //调拨单中没有物资明细的情况
                    if (db.StorageOutDetails.Count(u => u.StorageOutNoticeID == noticeid) == 0)
                    {
                        strDisposeUrl = string.Format("../StorageOut/SelectStorageOutDetails.aspx?NoticeID={0}", noticeid);
                        Response.Redirect(string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=该调拨单没有物资明细，不能发送审核！&DisposeUrl={0}&BackUrl={1}", strDisposeUrl, strBackUrl), false);
                        return;
                    }

                    //已经发送物资调拨审核的情况                        
                    if (db.StorageOutTask.Count(u => u.NoticeID.Equals(noticeid) && u.TaskType.Equals("物资调拨审核") && u.Process.Equals("正常出库")) != 0)
                    {
                        strDisposeUrl = strBackUrl;
                        Response.Redirect(string.Format("../PublicPage/ErrorInfo.aspx?ErrorInfo=该调拨单已经发送生产组长审核，不能重复发送！&DisposeUrl={0}&BackUrl={1}", strDisposeUrl, strBackUrl));
                        return;
                    }
                    Response.Redirect(string.Format("../StorageOut/CreateStorageOutTask.aspx?NoticeID={0}&TaskType=物资调拨审核", noticeid));
                    break;                   
                }
            }            
        }        
    }
}
