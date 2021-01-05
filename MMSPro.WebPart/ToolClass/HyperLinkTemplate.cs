using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace MMSPro.WebPart
{
    class HyperLinkTemplate : ITemplate
    {
        #region Fields
        private string strColumnName;
        private DataControlRowType dcrtColumnType;
        public string TaskType { get; set; }
        public string TaskTitle { get; set; }
        #endregion

        #region Methods
        public HyperLinkTemplate()
        {

        }

        /// <summary>
        /// 添加模板
        /// </summary>
        /// <param name="strColumnName">列名</param>
        /// <param name="dcrtColumnType">列类型</param>
        public HyperLinkTemplate(string strColumnName, DataControlRowType dcrtColumnType)
        {
            this.strColumnName = strColumnName;
            this.dcrtColumnType = dcrtColumnType;
        }

        /// <summary>
        /// 添加模板
        /// </summary>
        /// <param name="strColumnName">列名</param>
        /// <param name="dcrtColumnType">列类型</param>
        /// <param name="strTaskType">任务类型</param>
        /// <param name="strTaskTitle">任务名</param>
        public HyperLinkTemplate(string strColumnName, DataControlRowType dcrtColumnType, string strTaskType, string strTaskTitle)
        {
            this.TaskType = strTaskType;
            this.TaskTitle = strTaskTitle;
            this.strColumnName = strColumnName;
            this.dcrtColumnType = dcrtColumnType;

        }
        public void InstantiateIn(Control ctlContainer)
        {
            switch (dcrtColumnType)
            {
                case DataControlRowType.Header: //ColumnHeader
                    Label lab = new Label();
                    lab.Text = strColumnName;
                    ctlContainer.Controls.Add(lab);
                    break;
                case DataControlRowType.DataRow: //TemplateContent
                    HyperLink hl = new HyperLink();
                    hl.ID = "hlItem";
                    hl.Load += new EventHandler(hl_Load);
                    hl.DataBinding += new EventHandler(hl_DataBinding);
                    ctlContainer.Controls.Add(hl);
                    break;
            }
        }

        void hl_DataBinding(object sender, EventArgs e)
        {
            HyperLink hl = (HyperLink)sender;
            SPGridViewRow fc = (SPGridViewRow)hl.NamingContainer;
            if (!string.IsNullOrEmpty(TaskType))
            {
                hl.ToolTip = DataBinder.Eval(fc.DataItem, TaskType).ToString();
                hl.Text = DataBinder.Eval(fc.DataItem, TaskTitle).ToString();
                //hl.NavigateUrl = "";//NavigateUrl放到SPGirdView的RowDataBound事件中赋值
            }
        }

        void hl_Load(object sender, EventArgs e)
        {

        }
        #endregion
    }
}