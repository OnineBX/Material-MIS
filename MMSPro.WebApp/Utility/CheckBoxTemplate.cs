﻿/*------------------------------------------------------------------------------
 * Unit Name：CheckBoxTemplate.cs
 * Description: 用于SPGridVie的CheckBox模板列
 * Author: Li Tao
 * Created Date: 2009-08-17
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
    public class CheckBoxTemplate : ITemplate
    {
        #region Fields
        private string strColumnName;
        private DataControlRowType dcrtColumnType;
        private bool bInitStatus;
        public string BindRow{get;set;}
        #endregion

        #region Methods
        public CheckBoxTemplate()
        {

        }

        /// <summary>
        /// 添加模板
        /// </summary>
        /// <param name="strColumnName">列名</param>
        /// <param name="dcrtColumnType">列类型</param>
        public CheckBoxTemplate(string strColumnName, DataControlRowType dcrtColumnType)
        {
            this.strColumnName = strColumnName;
            this.dcrtColumnType = dcrtColumnType;
            this.bInitStatus = false;
        }
        /// <summary>
        /// 添加模板
        /// </summary>
        /// <param name="strColumnName">列名</param>
        /// <param name="dcrtColumnType">列类型</param>
        /// <param name="strBindRow">需要绑定到这个checkbox上面的数据字段名
        ///                         如果没有则传空字符串
        ///                         绑定的值存放在checkbox的tooltip属性上</param>
        public CheckBoxTemplate(string strColumnName, DataControlRowType dcrtColumnType,string strBindRow)
        {
            BindRow = strBindRow;
            this.strColumnName = strColumnName;
            this.dcrtColumnType = dcrtColumnType;
            this.bInitStatus = false;
        }

        /// <summary>
        /// 添加模板
        /// </summary>
        /// <param name="strColumnName">列名</param>
        /// <param name="dcrtColumnType">列类型</param>
        /// <param name="bStatus">初始选中状态</param>
        public CheckBoxTemplate(string strColumnName, DataControlRowType dcrtColumnType, bool bStatus)
        {
            this.strColumnName = strColumnName;
            this.dcrtColumnType = dcrtColumnType;
            this.bInitStatus = bStatus;
        }

        public void InstantiateIn(Control ctlContainer)
        {
            switch (dcrtColumnType)
            {
                case DataControlRowType.Header: //ColumnHeader
                    Literal ltr = new Literal();
                    ltr.Text = "<img src='" + SPContext.Current.Web.Url + "/_layouts/images/unchecka.gif' title='select all/unselect all' style='cursor:pointer' onclick='javascript:SmtGridToggleSelectAll();return false;' />";
                    ctlContainer.Controls.Add(ltr);
                    break;
                case DataControlRowType.DataRow: //TemplateContent
                    CheckBox cb = new CheckBox();                    
                    cb.ID = "SMItem";
                    cb.Load += new EventHandler(cb_Load);
                    cb.InputAttributes.Add("onclick", "javascript:SmtGridSelectItem(this);");                   
                    cb.DataBinding += new EventHandler(cb_DataBinding);
                    cb.Checked = bInitStatus;
                    ctlContainer.Controls.Add(cb);
                    break;
            }
        }

        void cb_DataBinding(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            SPGridViewRow fc = (SPGridViewRow)cb.NamingContainer;            
            if (!string.IsNullOrEmpty(BindRow))
            {
                cb.ToolTip = DataBinder.Eval(fc.DataItem, BindRow).ToString();
            }
            
           
        }

        void cb_Load(object sender, EventArgs e)
        {

        }
        #endregion
    }
}
