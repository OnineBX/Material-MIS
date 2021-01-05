/*------------------------------------------------------------------------------
 * Unit Name：DateTimeControlTemplate.cs
 * Description: 用于SPGridVie的DateTimeControl模板列
 * Author: Xu Chun Lei
 * Created Date: 2010-08-12
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
    public class DateTimeTemplate : ITemplate
    {        
        private DataControlRowType dcrtColumnType;
        private string strBindColumn = string.Empty;        

        public DateTimeTemplate()
        {
 
        }

        /// <summary>
        /// 用于选择日期时间的构造函数，设置rowtype为DataControlRowType.DataRow即可
        /// </summary>
        /// <param name="rowtype"></param>
        public DateTimeTemplate(DataControlRowType rowtype)
        {
            dcrtColumnType = rowtype;            
        }

        public DateTimeTemplate(string bindcolumn)
        {
            dcrtColumnType = DataControlRowType.DataRow;
            strBindColumn = bindcolumn;
        }       
   
        public void InstantiateIn(Control ctlContainer)
        {
            switch (dcrtColumnType)
            {                
                case DataControlRowType.DataRow: //TemplateContent                    
                    DateTimeControl dtc = new DateTimeControl();
                    dtc.ID = strBindColumn;                     
                    dtc.DateOnly = true;                    
                    dtc.DataBinding += new EventHandler(dtc_DataBinding);
                    ctlContainer.Controls.Add(dtc);
                    break;
            }
        }

        void dtc_DataBinding(object sender, EventArgs e)
        {
            DateTimeControl dtc = (DateTimeControl)sender;
            SPGridViewRow fc = (SPGridViewRow)dtc.NamingContainer;
            if (!string.IsNullOrEmpty(strBindColumn))
            {
                dtc.SelectedDate = Convert.ToDateTime(DataBinder.Eval(fc.DataItem, strBindColumn));                
            }
        }
        

    }
}
