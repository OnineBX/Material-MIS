/*------------------------------------------------------------------------------
 * Unit Name:CheckBoxAid.cs
 * Description: 用于检测SPGridView中CheckBox模板列的辅助类
 * Author: Li Tao
 * Created Date: 2009-08-17
 * ----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMSPro.WebApp
{
    class CheckBoxAid
    {
        #region Fields
        private int _CheckBoxItemID;
        private bool _CheckBoxItemState;
        #endregion

     
        public CheckBoxAid(int chbItemID, bool chbItemState)
        {
            this.CheckBoxItemID = chbItemID;
            this.CheckBoxItemState = chbItemState;
        }
        #region Properties
        public int CheckBoxItemID
        {
            get { return _CheckBoxItemID; }
            set { _CheckBoxItemID = value; }
        }

        public bool CheckBoxItemState
        {
            get { return _CheckBoxItemState; }
            set { _CheckBoxItemState = value; }
        }
        #endregion
    }
}
