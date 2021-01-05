/*------------------------------------------------------------------------------
 * Unit Name：LabelTemplate.cs
 * Description: 用于SPGridVie的Label模板列
 * Author: Xu Chun Lei
 * Created Date: 2010-06-06
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
    public class LabelTemplate : ITemplate
    {
        #region Fields
        private string strColumnName;
        private DataControlRowType dcrtColumnType;
        public string BindRow { get; set; }
        #endregion

        #region Methods
        public LabelTemplate()
        {

        }

        /// <summary>
        /// 添加模板
        /// </summary>
        /// <param name="strColumnName">列名</param>
        /// <param name="dcrtColumnType">列类型</param>        
        public LabelTemplate(string strColumnName, DataControlRowType dcrtColumnType)
        {
            this.strColumnName = strColumnName;
            this.dcrtColumnType = dcrtColumnType;
        }
        /// <summary>
        /// 添加模板
        /// </summary>
        /// <param name="strColumnName">列名</param>
        /// <param name="dcrtColumnType">列类型</param>
        /// <param name="strBindRow">需要绑定到这个Label上面的数据字段名
        ///                         如果没有则传空字符串
        ///                         绑定的值存放在Label的tooltip属性上</param>
        public LabelTemplate(string strColumnName, DataControlRowType dcrtColumnType, string strBindRow)
        {
            BindRow = strBindRow;
            this.strColumnName = strColumnName;
            this.dcrtColumnType = dcrtColumnType;
        }
        public void InstantiateIn(Control ctlContainer)
        {
            switch (dcrtColumnType)
            {                
                case DataControlRowType.DataRow: //TemplateContent
                    Label lb = new Label();
                    lb.ID = "LBItem";
                    lb.Load += new EventHandler(lb_Load);
                    lb.DataBinding += new EventHandler(lb_DataBinding);                                                         
                    ctlContainer.Controls.Add(lb);
                    break;
                default:
                    break;
            }
        }

        void lb_DataBinding(object sender, EventArgs e)
        {
            Label lb = (Label)sender;
            SPGridViewRow fc = (SPGridViewRow)lb.NamingContainer;
            if (!string.IsNullOrEmpty(BindRow))
            {
                lb.Text = DataBinder.Eval(fc.DataItem, BindRow).ToString();
            }
        }

        void lb_Load(object sender, EventArgs e)
        {
            
        }       
        
        #endregion
    }
}
