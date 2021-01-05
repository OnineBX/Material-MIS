/*------------------------------------------------------------------------------
 * Unit Name：DropDownListTemplate.cs
 * Description: 用于SPGridVie的DropDownList模板列
 * Author: Xu Chun Lei
 * Created Date: 2010-06-07
 * Modified Date:2010-08-10 by Xu Chun Lei
 * ----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Collections.Specialized;

namespace MMSPro.WebApp
{
    public class DropDownListTemplate : ITemplate
    {
        #region Fields
        private string strBindRow;
        private DataControlRowType dcrtColumnType;
        private IEnumerable<string> Data;
        private bool bolEnable = true;
        #endregion

        #region Methods
        public DropDownListTemplate()
        {

        }

        public DropDownListTemplate(DataControlRowType dcrtColumnType)
        {            
            this.dcrtColumnType = dcrtColumnType;
        }
       

       /// <summary>
       /// 构造函数
       /// </summary>
       /// <param name="strBindRow">要绑定的数据列，无须数据绑定时赋值String.Empty</param>
       /// <param name="dcrtColumnType">模板的列类型</param>
       /// <param name="data">初始化列表项的可枚举型数据</param>
        public DropDownListTemplate(string strBindRow, DataControlRowType dcrtColumnType,IEnumerable<string> data)
        {
            this.strBindRow = strBindRow;
            this.dcrtColumnType = dcrtColumnType;
            this.Data = data;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="strBindRow">要绑定的数据列，无须数据绑定时赋值String.Empty</param>
        /// <param name="dcrtColumnType">模板的列类型</param>
        /// <param name="data">初始化列表项的可枚举型数据</param>
        /// <param name="bolEnable">是否可交互</param>
        public DropDownListTemplate(string strBindRow, DataControlRowType dcrtColumnType, IEnumerable<string> data,bool Enable)
        {
            bolEnable = Enable;
            this.strBindRow = strBindRow;
            this.dcrtColumnType = dcrtColumnType;
            this.Data = data;
        }     

        public void InstantiateIn(Control ctlContainer)
        {
            switch (dcrtColumnType)
            {                
                case DataControlRowType.DataRow: //TemplateContent
                    DropDownList ddl = new DropDownList();
                    ddl.Enabled = bolEnable;
                    ddl.ID = "DDLItem";
                    if (Data != null)
                    {
                        foreach (string item in Data)
                            ddl.Items.Add(item);
                    }
                    ddl.DataBinding += new EventHandler(ddl_DataBinding);
                    ddl.AutoPostBack = true;
                    ddl.Width = 150;                                 
                    ctlContainer.Controls.Add(ddl);
                    break;
                default:
                    break;
            }
        }

        void ddl_DataBinding(object sender, EventArgs e)
        {
            DropDownList ddl = (DropDownList)sender;
            SPGridViewRow fc = (SPGridViewRow)ddl.NamingContainer;
            if (!string.IsNullOrEmpty(strBindRow))
            {
                ddl.Text = DataBinder.Eval(fc.DataItem, strBindRow).ToString();
            }
        }                      
        
        #endregion
    }
}
