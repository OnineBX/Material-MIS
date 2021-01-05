/*------------------------------------------------------------------------------
 * Unit Name：PagerTemplate.cs
 * Description: 用于SPGridView的标准分页
 * Author: Li Tao
 * Created Date: 2010-05-04
 * ----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.WebControls;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace MMSPro.WebPart
{
    class  SPGridViewPagerTemplate: ITemplate
    {
        #region Fields
        private SPGridView _gview = null;

        private string _format = string.Empty;
        //private static LogHelper _log = LogHelper.GetInstance();
        #endregion

        #region Methods
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="format"></param>
        /// <param name="grid"></param>
        public SPGridViewPagerTemplate(string format, SPGridView grid)
        {
            this._format = format;
            this._gview = grid;
        }

        /// <summary>
        /// Instantiat in the control to container
        /// </summary>
        /// <param name="container"></param>
        public void InstantiateIn(Control container)
        {
            try
            {
                Table tbl = new System.Web.UI.WebControls.Table();
                container.Controls.Add(tbl);
                tbl.Width = Unit.Percentage(100);
                TableRow row = new TableRow();
                tbl.Rows.Add(row);
                TableCell cell = new TableCell();
                cell.Font.Size = new FontUnit(FontSize.Smaller);
                row.Cells.Add(cell);
                cell.HorizontalAlign = HorizontalAlign.Center;


                int currentPage = _gview.PageIndex + 1;
                int from = 0;
                int to = 0;
                if (currentPage > 1)
                {
                    ImageButton prevBtn = new ImageButton();
                    prevBtn.ImageUrl = "~/_layouts/images/prev.gif";
                    prevBtn.CommandName = "Page";
                    prevBtn.CommandArgument = "Prev";
                    cell.Controls.Add(prevBtn);

                }
                if (currentPage > 1)
                {
                    if (currentPage == _gview.PageCount)
                    {
                        from = (currentPage - 1) * _gview.PageSize + 1;
                        to = _gview.Rows.Count + (currentPage - 1) * _gview.PageSize;
                    }
                    else
                    {
                        from = (currentPage - 1) * _gview.PageSize + 1;
                        to = from + _gview.PageSize - 1;
                    }
                }
                else
                {
                    from = 1;
                    to = _gview.PageSize;
                }
                LiteralControl lControl = new LiteralControl(String.Format(_format, from, to));
                cell.Controls.Add(lControl);

                if (currentPage < _gview.PageCount)
                {
                    ImageButton nextBtn = new ImageButton();
                    nextBtn.ImageUrl = "~/_layouts/images/next.gif";
                    nextBtn.CommandName = "Page";
                    nextBtn.CommandArgument = "Next";
                    cell.Controls.Add(nextBtn);
                }
            }
            catch (Exception ex)
            {
                //_log.Log(LogSeverity.Error, ex, "gview分页异常", string.Empty);
            }
        }
        #endregion
    }
}
