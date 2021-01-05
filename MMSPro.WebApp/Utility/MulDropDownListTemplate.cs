/*------------------------------------------------------------------------------
 * Unit Name：DropDownListTemplate.cs
 * Description: 用于SPGridVie的DropDownList模板列
 * Author: Xu Chun Lei
 * Created Date: 2010-08-20 by Xu Chun Lei
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
    public class MulDropDownListTemplate : ITemplate
    {
        #region Fields
        private string strBindColumn;
        private DataControlRowType dcrtColumnType;
        private string ddlID;
        private int iWidth = 120;
        #endregion

        #region Methods
        public MulDropDownListTemplate()
        {

        }

        public MulDropDownListTemplate(DataControlRowType dcrtColumnType,string strID)
        {
            this.dcrtColumnType = dcrtColumnType;
            ddlID = strID;
            strBindColumn = string.Empty;
        }

        public MulDropDownListTemplate(DataControlRowType dcrtColumnType, string strID,int width)
        {
            this.dcrtColumnType = dcrtColumnType;
            ddlID = strID;
            strBindColumn = string.Empty;
            iWidth = width;
        }

        public MulDropDownListTemplate(DataControlRowType dcrtColumnType, string strID,string strBindName)
        {
            this.dcrtColumnType = dcrtColumnType;
            ddlID = strID;
            strBindColumn = strBindName;
        }

        public MulDropDownListTemplate(DataControlRowType dcrtColumnType, string strID, string strBindName,int width)
        {
            this.dcrtColumnType = dcrtColumnType;
            ddlID = strID;
            strBindColumn = strBindName;
            iWidth = width;
        }
        
        public void InstantiateIn(Control ctlContainer)
        {
            switch (dcrtColumnType)
            {
                case DataControlRowType.DataRow: //TemplateContent
                    DropDownList ddl = new DropDownList();                     
                    ddl.ID = ddlID;                                        
                    ddl.AutoPostBack = true;
                    ddl.Width = iWidth;
                    ddl.DataBound += new EventHandler(ddl_DataBound);
                    ctlContainer.Controls.Add(ddl);
                    break;
                default:
                    break;
            }
        }

        void ddl_DataBound(object sender, EventArgs e)
        {            
            DropDownList ddl = (DropDownList)sender;
            SPGridViewRow fc = (SPGridViewRow)ddl.NamingContainer;
            if (!string.IsNullOrEmpty(strBindColumn))
            {                
                ddl.SelectedValue = DataBinder.Eval(fc.DataItem, strBindColumn).ToString();
            }
        }       

        #endregion
    }
}
