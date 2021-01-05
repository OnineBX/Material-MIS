/*------------------------------------------------------------------------------
 * Unit Name:ExcelHelper.cs
 * Description: 用于读取Excel中的工作表(97-2003,2007)
 * Author: Li Tao
 * Created Date: 2010-05-04
 * 
 * Update Date:2010-07-22
 * Update Content:添加读取Excel2007的功能
 * ----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;

namespace MMSPro.WebApp
{
    internal class TMSProExcelUtility
    {
        #region Fields
        private static LogHelper _log = LogHelper.GetInstance(); //_log.Log(LogSeverity.Error, ex, "GetExcelSheet异常001", string.Empty);
        #endregion

        #region Properties
        #endregion

        #region Methods
        /// <summary>
        /// 返回指定Excel版本的DataSet
        /// </summary>
        /// <param name="filePath">Excel文件全路径</param>
        /// <param name="sheetName">工作表名</param>
        /// <param name="excelVertion">Excel版本,请指定"97-2003"或"2007"等Excel版本</param>
        /// <returns></returns>
        public DataSet GetExcelSheet(string filePath, string sheetName, string excelVertion)
        {
            DataSet ds = null;
            string conString = string.Empty;
            try
            {
                ds = new DataSet();
                if (excelVertion == "97-2003")
                {
                    //Yes表示用第一个有数据的行生成列标题，空白字段用F+列索引生成列标题，NO表示用F1,F2...生成列标题。IMEX=1表示启用混合数据类型。'表示启用"HDR=Yes;IMEX=1"后可以避免读取错误。
                    conString = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + filePath + ";Extended Properties='Excel 8.0;HDR=Yes;IMEX=1'";
                }
                else if (excelVertion == "2007")
                {
                    conString = "Provider=Microsoft.Ace.OleDb.12.0;" + "data source=" + filePath + ";Extended Properties='Excel 12.0; HDR=Yes;IMEX=1'";
                }
                using (OleDbConnection con = new OleDbConnection(conString))
                {
                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
                    }
                    OleDbDataAdapter oda = new OleDbDataAdapter("SELECT * FROM [" + sheetName + "$]", con);
                    oda.Fill(ds);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ds;
        }

        /// <summary>
        /// 返回Excel 97-2003版本的DataSet
        /// </summary>
        /// <param name="filePath">Excel文件全路径</param>
        /// <param name="sheetName">工作表名</param>
        /// <returns></returns>
        public DataSet GetExcelSheet(string filePath, string sheetName)
        {
            return GetExcelSheet(filePath, sheetName, "97-2003");
        }
        #endregion
    }
}
