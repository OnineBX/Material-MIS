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
    public class FileUploadTemplate:ITemplate
    {
        #region Fields
        private string strBindColumn;
        private DataControlRowType dcrtColumnType;
        private string fuID;
        #endregion

        #region Methods
        public FileUploadTemplate()
        {

        }

        public FileUploadTemplate(DataControlRowType dcrtColumnType,string strID)
        {
            this.dcrtColumnType = dcrtColumnType;
            fuID = strID;
            strBindColumn = string.Empty;
        }

        public FileUploadTemplate(DataControlRowType dcrtColumnType, string strID, string strBindName)
        {
            this.dcrtColumnType = dcrtColumnType;
            fuID = strID;
            strBindColumn = strBindName;
        }
        
        public void InstantiateIn(Control ctlContainer)
        {
            switch (dcrtColumnType)
            {
                case DataControlRowType.DataRow: //TemplateContent
                    FileUpload fu = new FileUpload();
                    fu.ID = fuID;                                        
                    ctlContainer.Controls.Add(fu);
                    break;
                default:
                    break;
            }
        }             

        #endregion
    }
}
