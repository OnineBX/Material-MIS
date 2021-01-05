using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.WebControls;
using System.Configuration;

namespace MMSPro.WebApp
{
    public class UploadFile : System.Web.UI.Page
    {
        FileUpload fuploadTemp;
        Button btnUploadFile;
        Label lblInfo;
        SPGridView gviewFiles;
        Panel p1;
        string strDetailsID;
        string strProcessType;
        string strReportNum;

        protected void Page_Load(object sender, EventArgs e)
        {
            this.strDetailsID = Request.QueryString["detailsID"].ToString();
            this.strProcessType = Request.QueryString["Type"].ToString();
            this.strReportNum = Request.QueryString["ReportNum"].ToString();

            this.btnUploadFile = (Button)GetControltByMaster("btnUploadFile");
            this.btnUploadFile.Click +=new EventHandler(btnUploadFile_Click);
            this.fuploadTemp = (FileUpload)GetControltByMaster("fuploadTemp");
            this.lblInfo = (Label)GetControltByMaster("lblInfo");

            ToolBar tbarTop = (ToolBar)GetControltByMaster("tbarFileOfQC");
            ToolBarButton btnDelte = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnDelte.ID = "DeleteRow";
            btnDelte.Text = "删除";
            btnDelte.ImageUrl = "/_layouts/images/delete.gif";
            btnDelte.Click += new EventHandler(btnDelte_Click);
            StringBuilder sbScript = new StringBuilder();
            sbScript.Append("var aa= window.confirm('确认删除所选项?');");
            sbScript.Append("if(aa == false){");
            sbScript.Append("return false;}");
            btnDelte.OnClientClick = sbScript.ToString();
            tbarTop.Buttons.Controls.Add(btnDelte);

            ToolBarButton btnDownLoad = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnDownLoad.ID = "DownLoadRow";
            btnDownLoad.Text = "下载";
            btnDownLoad.ImageUrl = "/_layouts/images/edit.gif";
            btnDownLoad.Click += new EventHandler(btnDownLoad_Click);
            tbarTop.Buttons.Controls.Add(btnDownLoad);

            ToolBarButton btnRefresh = (ToolBarButton)this.Page.LoadControl("~/_controltemplates/ToolBarButton.ascx");
            btnRefresh.ID = "btnRefresh";
            btnRefresh.Text = "刷新";
            btnRefresh.ImageUrl = "/_layouts/images/refresh.GIF";
            btnRefresh.Padding = "0,5,0,0";
            btnRefresh.Click += new EventHandler(btnRefresh_Click);
            tbarTop.RightButtons.Controls.Add(btnRefresh);

            //if (!IsPostBack)
            //{
            BindGridView(this.strDetailsID, this.strProcessType, this.strReportNum);
            this.p1 = (Panel)GetControltByMaster("Panel1");
            this.p1.Controls.Add(this.gviewFiles);
            //}
        }

        void btnRefresh_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void btnDownLoad_Click(object sender, EventArgs e)
        {
            List<CheckBox> listCheckBoxs = GetCheckedID();
            if (listCheckBoxs.Count == 1)
            {
                FileOfQC fqc;
                using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    foreach (var chb in listCheckBoxs)
                    {
                        fqc = dc.FileOfQC.SingleOrDefault(qc => qc.FileID == int.Parse(chb.ToolTip));
                        if (fqc != null)
                        {
                            byte[] fileContent = fqc.FileContent.ToArray();
                            string fileExtension = System.IO.Path.GetExtension(fqc.NameOfFile);
                            Response.ClearHeaders();
                            //Response.AppendHeader("Content-Disposition", "attachment;filename=" + Server.UrlEncode(fqc.NameOfFile)); //把 attachment 改为 online 则在线打开
                            Response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(fqc.NameOfFile, System.Text.Encoding.UTF8)); //把 attachment 改为 online 则在线打开
                            Response.AddHeader("Content-Length", fqc.FileSize);
                            Response.AppendHeader("Last-Modified", fqc.FileCreateTime.ToFileTime().ToString());
                            Response.AppendHeader("Location", Request.Url.AbsoluteUri);

                            Response.ContentType = GetResponseContentType(fileExtension);
                            Response.BinaryWrite(fileContent);
                            Response.End();

                            //Response.Clear();
                            //Response.AppendHeader("Content-Disposition", "attachment;filename=" + HttpUtility.UrlEncode(fqc.NameOfFile, System.Text.Encoding.UTF8));//设置编码，解决下载文件名乱码
                            //Response.AppendHeader("Content-Length", fqc.FileSize.Length.ToString());
                            //Response.ContentType = "application/octet-stream";
                            //Response.OutputStream.Write(fileContent, 0, fileContent.Length);
                            //Response.End();
                        }
                    }
                }
                //Page.RegisterStartupScript("DeleteOk", "<script>alert('下载成功!          ');window.location.href='UploadFile.aspx?dt=" + DateTime.Now.ToString("yyyyMMddhhmmss") + "'</" + "script>");
                Page.RegisterStartupScript("DeleteOk", "<script>alert('下载成功!          ');window.location.href='UploadFile.aspx?dt=" + DateTime.Now.ToString("yyyyMMddhhmmss") + "&detailsID=" + this.strDetailsID + "&Type=" + this.strProcessType + "&ReportNum=" + this.strReportNum + "'</" + "script>");
            }
            else if (listCheckBoxs.Count == 0)
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择要下载的质检报告!')</script>");
                //Page.RegisterClientScriptBlock("ShowMessage", "<script>alert('请选择需要删除的记录!')</script>");
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择一个且仅能选择一个质检报告!')</script>");
            }
        }

        private string GetResponseContentType(string fileType)
        {
            string result;
            switch (fileType.ToLower())
            {
                case ".doc": 
                    result = "application/msword"; 
                    break;
                case ".docx":
                    result = "application/msword";
                    break;
                case ".xls": 
                    result = "application/msexcel"; 
                    break;
                case ".xlsx":
                    result = "application/msexcel";
                    break;
                case ".txt": 
                    result = "text/plain"; 
                    break;
                case ".pdf": 
                    result = "application/pdf";
                    break;
                case ".jpg":
                    result = "image/jpeg";
                    break;
                //case ".ppt": 
                //    result = "appication/powerpoint"; 
                //    break;
                default: 
                    result = "application/unknown"; 
                    break;
            }
            return result;
        }



        protected void btnDelte_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            List<CheckBox> listCheckBoxs = GetCheckedID();
            if (listCheckBoxs.Count > 0)
            {
                FileOfQC fqc;
                using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                {
                    foreach (var chb in listCheckBoxs)
                    {
                        fqc = dc.FileOfQC.SingleOrDefault(qc => qc.FileID == int.Parse(chb.ToolTip));
                        if (fqc != null)
                        {
                            dc.FileOfQC.DeleteOnSubmit(fqc);

                        }
                    }
                    dc.SubmitChanges();
                }
                Page.RegisterStartupScript("DeleteOk", "<script>alert('删除成功!          ');window.location.href='UploadFile.aspx?dt=" + DateTime.Now.ToString("yyyyMMddhhmmss") + "&detailsID=" + this.strDetailsID + "&Type=" + this.strProcessType + "&ReportNum=" + this.strReportNum + "'</" + "script>");
            }
            else
            {
                ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('请选择需要删除的记录!')</script>");
                //Page.RegisterClientScriptBlock("ShowMessage", "<script>alert('请选择需要删除的记录!')</script>");
            }

        }

        void btnUploadFile_Click(object sender, EventArgs e)
        {
            #region
            //try
            //{
            //    SPWeb web = SPContext.Current.Web;
            //    web.AllowUnsafeUpdates = true;
            //    SPList list = web.Lists["DocForQC"];
            //    SPFolder folder = list.RootFolder;
            //    //SPListItem item = list.Items.Add();
            //    //item["标题"] = this.fuploadTemp.FileName;
            //    //item.Update();
            //    ////item.Attachments.Add(this.fuploadTemp.FileName, this.fuploadTemp.FileBytes);
            //    //this.lblInfo.Text = "上传文件成功!";

            //    //System.IO.Stream fs = this.fuploadTemp.FileContent;
            //    //int length = (int)fs.Length;
            //    //byte[] content = new byte[length];
            //    //fs.Read(content, 0, length);
            //    //fs.Close();
            //    //folder.Files.Add(folder.Url + "/" + System.IO.Path.GetFileName(this.fuploadTemp..FileName), content);
            //    web.AllowUnsafeUpdates = true;
            //    folder.Files.Add(folder.Url + "/" + this.fuploadTemp.FileName, this.fuploadTemp.FileBytes);
            //    this.lblInfo.Text = "正在上传...";
            //    System.Threading.Thread.Sleep(2000);
            //    this.lblInfo.Text = "上传报告成功!";

            //}
            //catch (Exception ex)
            //{
            //    System.Reflection.MethodBase mb = System.Reflection.MethodBase.GetCurrentMethod();
            //    LogToDBHelper lhelper = LogToDBHelper.Instance;
            //    lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
            //    ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            //}
            #endregion
            try
            {
                string fileExtension = System.IO.Path.GetExtension(this.fuploadTemp.FileName);
                if (fileExtension == ".doc" || fileExtension == ".docx" || fileExtension == ".xls" || fileExtension == ".xlsx" || fileExtension == ".pdf" || fileExtension == ".txt" || fileExtension == ".jpg")
                {
                    this.lblInfo.Text = "正在上传...";
                    using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
                    {

                        FileOfQC fqc = new FileOfQC();
                        fqc.NameOfFile = this.fuploadTemp.FileName;
                        fqc.FileContent = new System.Data.Linq.Binary(this.fuploadTemp.FileBytes);
                        //fqc.FileSize = (this.fuploadTemp.FileContent.Length / (int)1024).ToString(); //转化后为KB
                        fqc.FileSize = this.fuploadTemp.FileContent.Length.ToString(); //转化后为KB
                        fqc.FileCreateTime = DateTime.Now;
                        fqc.FileCreateEmp = dc.EmpInfo.SingleOrDefault(em => em.Account.ToLower() == SPContext.Current.Web.CurrentUser.LoginName.ToLower()).EmpID;
                        fqc.Filed1 = this.strDetailsID;
                        fqc.Filed2 = this.strProcessType;
                        fqc.Filed3 = this.strReportNum;
                        dc.FileOfQC.InsertOnSubmit(fqc);
                        dc.SubmitChanges();
                        System.Threading.Thread.Sleep(2000);
                        this.lblInfo.Text = "上传报告成功!";
                        //Response.Redirect("UploadFile.aspx");
                        Response.Redirect("UploadFile.aspx?&detailsID=" + this.strDetailsID + "&Type=" + this.strProcessType + "&ReportNum=" + this.strReportNum);
                    }
                }
                else
                {
                    ClientScript.RegisterClientScriptBlock(typeof(string), "ShowMessage", "<script>alert('只能上传.doc,.docx,.xls,.xlsx,.pdf,.txt,.jpg等类型的文件!')</script>");
                }
            }
            catch (Exception ex)
            {
                System.Reflection.MethodBase mb = System.Reflection.MethodBase.GetCurrentMethod();
                LogToDBHelper lhelper = LogToDBHelper.Instance;
                lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
                ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
            }
            //finally
            //{
            //    BindGridView();
            //    this.p1 = (Panel)GetControltByMaster("Panel1");
            //    //this.p1.Controls.Clear();
            //    this.p1.Controls.Add(this.gviewFiles);
            //}


        }

        void BindGridView(string detailsID, string processType,string reportNum)
        {
            //throw new NotImplementedException();
            this.gviewFiles = new SPGridView(); ;

            TemplateField tfCheckbox = new TemplateField();
            tfCheckbox.ItemTemplate = new CheckBoxTemplate("选择所有/取消", DataControlRowType.DataRow, "FileID");
            tfCheckbox.HeaderTemplate = new CheckBoxTemplate("请选择", DataControlRowType.Header);
            this.gviewFiles.Columns.Add(tfCheckbox);

            BoundField bfFileName = new BoundField();
            bfFileName.HeaderText = "质检报告";
            bfFileName.DataField = "NameOfFile";
            this.gviewFiles.Columns.Add(bfFileName);

            BoundField bfCreateTime = new BoundField();
            bfCreateTime.HeaderText = "创建时间";
            bfCreateTime.DataField = "FileCreateTime";
            this.gviewFiles.Columns.Add(bfCreateTime);

            //CommandField cf = new CommandField();
            //cf.ButtonType = ButtonType.Link;
            //cf.ShowDeleteButton = true;
            //cf.HeaderText = "操作";
            //this.gviewFiles.Columns.Add(cf);



            this.gviewFiles.AutoGenerateColumns = false;
            this.gviewFiles.GridLines = GridLines.None;
            this.gviewFiles.CssClass = "ms-vh2 padded headingfont";
            //this.gviewFiles.DataKeyNames = new string[] { "FileID" };
            //this.gviewFiles.RowDeleting +=new GridViewDeleteEventHandler(gviewFiles_RowDeleting);
            //this.gviewFiles.RowDataBound += new GridViewRowEventHandler(gviewFiles_RowDataBound);

            //this.gviewMoreTaskForMyMsg.AllowPaging = true;
            //this.gviewMoreTaskForMyMsg.PageSize = 1;
            //this.gviewMoreTaskForMyMsg.PageIndexChanging +=new GridViewPageEventHandler(gviewMoreTaskForMyMsg_PageIndexChanging);
            //this.gviewMoreTaskForMyMsg.PagerTemplate = new SPGridViewPagerTemplate("{0} - {1}", gviewMoreTaskForMyMsg);

            using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
            {

                this.gviewFiles.DataSource = from fqc in dc.FileOfQC
                                             where fqc.Filed1 == detailsID && fqc.Filed2 == processType &&fqc.Filed3 == reportNum
                                             select new
                                             {
                                                 fqc.NameOfFile,
                                                 fqc.FileID,
                                                 fqc.FileCreateTime
                                             };

                this.gviewFiles.DataBind();
            }
        }

        #region
        //void gviewFiles_RowDataBound(object sender, GridViewRowEventArgs e)
        //{
        //    if (e.Row.RowType == DataControlRowType.DataRow)
        //    {
        //        if (e.Row.RowState == DataControlRowState.Normal || e.Row.RowState == DataControlRowState.Alternate)
        //        {
        //            ((LinkButton)e.Row.Cells[1].Controls[0]).Attributes.Add("onclick", "javascript:return confirm('你确定要删除\"" + e.Row.Cells[0].Text + "\"吗?')");
        //        }
        //    }
        //}

        //void gviewFiles_RowDeleting(object sender, GridViewDeleteEventArgs e)
        //{
        //    try
        //    {
        //        using (MMSProDBDataContext dc = new MMSProDBDataContext(ConfigurationManager.ConnectionStrings["mmsConString"].ConnectionString))
        //        {
        //            int curFileID = Convert.ToInt32(this.gviewFiles.DataKeys[e.RowIndex].Value);
        //            FileOfQC fqc = dc.FileOfQC.SingleOrDefault(qc => qc.FileID == curFileID);
        //            if (fqc != null)
        //            {
        //                dc.FileOfQC.DeleteOnSubmit(fqc);
        //            }
        //            dc.SubmitChanges();

        //            BindGridView();
        //            //this.p1 = (Panel)GetControltByMaster("Panel1");
        //            //this.p1.Controls.Add(this.gviewFiles);

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Reflection.MethodBase mb = System.Reflection.MethodBase.GetCurrentMethod();
        //        LogToDBHelper lhelper = LogToDBHelper.Instance;
        //        lhelper.WriteLog(ex.Message, "错误", string.Format("{0}.{1}", mb.ReflectedType.Name, mb.Name));
        //        ClientScript.RegisterClientScriptBlock(typeof(string), "提示", string.Format("<script>alert('{0}')</script>", LogToDBHelper.LOG_MSG_LOADERROR));
        //    }
        //}
        #endregion


        /// <summary>
        /// 返回选中的列表
        /// </summary>
        /// <returns>返回list构成的列表</returns>
        private List<CheckBox> GetCheckedID()
        {
            List<CheckBox> listCheckBoxs = new List<CheckBox>();

            foreach (GridViewRow row in this.gviewFiles.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    CheckBox ck = (CheckBox)row.Cells[0].FindControl("SMItem");
                    if (ck.Checked)
                    {
                        listCheckBoxs.Add(ck);
                    }
                }
            }
            return listCheckBoxs;
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
