/*------------------------------------------------------------------------------
 * Unit Name:CheckBoxAndIndex.cs
 * Description: 用于检测SPGridView中CheckBox选中项的索引
 * Author: ZhengPing
 * Created Date: 2010-6-25
 * ----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
namespace MMSPro.WebApp
{
    class CheckBoxAndIndex
    {
        private CheckBox cBox; 
        private int index;

        public CheckBoxAndIndex(CheckBox box, int idx)

        {
            this.cBox = box;
            this.index = idx;

        }

        public CheckBox checkBox
        {
            get { return cBox; }
            set { cBox = value; }
        }

        public int checkIdx
        {
            get { return index; }
            set { index = value; }
        }
    }
}
