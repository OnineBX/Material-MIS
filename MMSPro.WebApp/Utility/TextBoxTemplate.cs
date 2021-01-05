/*------------------------------------------------------------------------------
 * Unit Name：TextBoxTemplate.cs
 * Description: 用于SPGridVie的TextBox模板列
 * Author: Xu ChunLei
 * Created Date: 2010-07-15
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
    public class TextBoxTemplate : ITemplate
    {
        #region Fields
        private string strColumnName;
        private DataControlRowType dcrtColumnType;
        private string Regular;
        private string DefaultValue;
        private bool Enabled;
        public string BindRow { get; set; }
        #endregion
        private int iWith;
        #region Methods
        public TextBoxTemplate()
        {

        }

        /// <summary>
        /// 添加模板
        /// </summary>
        /// <param name="strColumnName">列名</param>
        /// <param name="dcrtColumnType">列类型</param>
        public TextBoxTemplate(string strColumnName, DataControlRowType dcrtColumnType)
        {
            this.strColumnName = strColumnName;
            this.dcrtColumnType = dcrtColumnType;
            Regular = string.Empty;
            Enabled = true;            
        }      

        public TextBoxTemplate(string strColumnName, DataControlRowType dcrtColumnType,bool enabled)
        {
            this.strColumnName = strColumnName;
            this.dcrtColumnType = dcrtColumnType;
            Regular = string.Empty;
            Enabled = enabled;
        }

        /// <summary>
        /// 添加DataRow类型模板
        /// </summary>
        /// <param name="strColumnName">列名</param>
        /// <param name="strRegular">验证正则表达式</param>
        /// <param name="strBindRow">需要绑定的数据列</param>
        public TextBoxTemplate(string strColumnName, string strBindRow, string strRegular)
        {
            this.strColumnName = strColumnName;
            BindRow = strBindRow; 
            this.dcrtColumnType = DataControlRowType.DataRow;
            Regular = strRegular;
            Enabled = true;
        }

        /// <summary>
        /// 添加带默认值和验证正则表达式的模板
        /// </summary>
        /// <param name="strColumnName"></param>
        /// <param name="strBindRow"></param>
        /// <param name="strRegular"></param>
        public TextBoxTemplate(string strColumnName, string strBindRow, string strRegular,string strdefault)
        {
            this.strColumnName = strColumnName;
            BindRow = strBindRow;
            this.dcrtColumnType = DataControlRowType.DataRow;
            Regular = strRegular;
            DefaultValue = strdefault;
            Enabled = true;
        }

        public TextBoxTemplate(string strColumnName, string strBindRow, string strRegular, string strdefault, int iWidth)
        {
            this.strColumnName = strColumnName;
            BindRow = strBindRow;
            this.dcrtColumnType = DataControlRowType.DataRow;
            Regular = strRegular;
            DefaultValue = strdefault;
            Enabled = true;
            iWith = iWidth;
        }

        public TextBoxTemplate(string strColumnName, string strBindRow, string strRegular, string strdefault, int iWidth, bool iboll)//edit by adonis
        {
            this.strColumnName = strColumnName;
            BindRow = strBindRow;
            this.dcrtColumnType = DataControlRowType.DataRow;
            Regular = strRegular;
            DefaultValue = strdefault;
            Enabled = iboll;
            iWith = iWidth;
        }

        /// <summary>
        /// 添加模板
        /// </summary>
        /// <param name="strColumnName">列名</param>
        /// <param name="dcrtColumnType">列类型</param>
        /// <param name="strBindRow">需要绑定到这个TextBox上面的数据字段名
        ///                         如果没有则传空字符串
        ///                         </param>
        public TextBoxTemplate(string strColumnName, DataControlRowType dcrtColumnType, string strBindRow)
        {
            BindRow = strBindRow;            
            this.strColumnName = strColumnName;
            this.dcrtColumnType = dcrtColumnType;
            Regular = string.Empty;
            Enabled = true;
        }

        /// <summary>
        /// 添加指定tbox宽度的模板
        /// </summary>
        /// <param name="strColumnName">列名</param>
        /// <param name="bValidate">是否加入验证</param>
        /// <param name="strBindRow">需要绑定的数据列</param>
        /// <param name="intWith">列宽</param>
        public TextBoxTemplate(string strColumnName, string strBindRow, string strRegular, int intWith)
        {
            this.iWith = intWith;
            this.strColumnName = strColumnName;
            BindRow = strBindRow;
            this.dcrtColumnType = DataControlRowType.DataRow;
            Regular = strRegular;
            Enabled = true;
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
                    TextBox tb = new TextBox();
                    if (iWith > 0)
                        tb.Width = iWith;
                    tb.ID = strColumnName;
                    tb.Load += new EventHandler(cb_Load);
                    tb.DataBinding += new EventHandler(cb_DataBinding);
                   
                    tb.Enabled = Enabled;
                 
                    tb.Text = DefaultValue;
                    ctlContainer.Controls.Add(tb);
                    if (!string.IsNullOrEmpty(Regular))
                    {
                        RegularExpressionValidator rev = new RegularExpressionValidator();
                        rev.ID = string.Format("Validate{0}", strColumnName);
                        rev.ControlToValidate = strColumnName;
                        rev.Text = "*";
                        rev.ValidationExpression = Regular;
                        ctlContainer.Controls.Add(rev);
                    }
                    
                    break;
            }
        }

        void cb_DataBinding(object sender, EventArgs e)
        {
            TextBox cb = (TextBox)sender;
            SPGridViewRow fc = (SPGridViewRow)cb.NamingContainer;
            if (!string.IsNullOrEmpty(BindRow))
            {
               if(DataBinder.Eval(fc.DataItem, BindRow)!=null)
                cb.Text = DataBinder.Eval(fc.DataItem, BindRow).ToString();
            }


        }

        void cb_Load(object sender, EventArgs e)
        {

        }
        #endregion

        //文本框的验证控件类型
//        public enum ValidateType{Required,Regular,Range,None};
    }
}
