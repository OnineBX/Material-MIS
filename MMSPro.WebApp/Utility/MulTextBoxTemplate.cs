/*------------------------------------------------------------------------------
 * Unit Name：TextBoxTemplate.cs
 * Description: 用于SPGridVie的TextBox模板列
 * Author: Li Tao
 * Created Date: 2010-06-01
 * ----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace MMSPro.WebApp
{
    public class MulTextBoxTemplate : ITemplate
    {
        #region Fields
        private string strColumnName;
        private DataControlRowType dcrtColumnType;
        public string BindRow { get; set; }
        public string BindTag { get; set; }
        public string TextBoxID { get; set; }
        #endregion

        #region Methods
        public MulTextBoxTemplate()
        {

        }

        /// <summary>
        /// 添加模板
        /// </summary>
        /// <param name="strColumnName">列名</param>
        /// <param name="dcrtColumnType">列类型</param>
        public MulTextBoxTemplate(string strColumnName, DataControlRowType dcrtColumnType)
        {
            this.strColumnName = strColumnName;
            this.dcrtColumnType = dcrtColumnType;
        }
        /// <summary>
        /// 添加模板
        /// </summary>
        /// <param name="strColumnName">列名</param>
        /// <param name="dcrtColumnType">列类型</param>
        /// <param name="strBindRow">需要绑定到这个checkbox上面的数据字段名
        ///                         如果没有则传空字符串
        ///                         绑定的值存放在checkbox的tooltip属性上</param>
        public MulTextBoxTemplate(string strColumnName, DataControlRowType dcrtColumnType, string strBindRow, string strBindTag,string txtFieldID)
        {
            BindRow = strBindRow;
            BindTag = strBindTag;
            this.strColumnName = strColumnName;
            this.dcrtColumnType = dcrtColumnType;
            this.TextBoxID = txtFieldID;
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
                    TextBox cb = new TextBox();
                    cb.ID = this.TextBoxID;
                    cb.Load += new EventHandler(cb_Load);
                    //cb.InputAttributes.Add("onclick", "javascript:SmtGridSelectItem(this);");
                    cb.DataBinding += new EventHandler(cb_DataBinding);
                    cb.Width = 100;
                    //cb.Checked = false;
                    ctlContainer.Controls.Add(cb);
                    break;
            }
        }

        void cb_DataBinding(object sender, EventArgs e)
        {
            TextBox cb = (TextBox)sender;
            SPGridViewRow fc = (SPGridViewRow)cb.NamingContainer;
            if (!string.IsNullOrEmpty(BindRow))
            {
                cb.Text = DataBinder.Eval(fc.DataItem, BindRow).ToString();
                cb.ToolTip = DataBinder.Eval(fc.DataItem, BindTag).ToString();
            }


        }

        void cb_Load(object sender, EventArgs e)
        {

        }
        #endregion
    }
}
