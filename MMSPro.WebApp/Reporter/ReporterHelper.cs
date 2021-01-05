using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.in2bits.MyXls;
using org.in2bits.MyOle2;
using System.Data;
using System.Reflection;
namespace MMSPro.WebApp
{
    public static class ReporterHelper
    {
        public static void SetXF(ref Worksheet ws, ushort LeftRowIndex, ushort LeftColIndex, ushort RightRowIndex, ushort RightColIndex, XF xf)
        {

            Cell c;
            for (ushort i = LeftRowIndex; i <= RightRowIndex; i++)
            {
                for (ushort n = LeftColIndex; n <= RightColIndex; n++)
                {
                    if (ws.Rows[i].CellExists(n))
                        c = ws.Rows[i].CellAtCol(n);
                    else
                        c = ws.Cells.Add(i, n, "", xf);


                    //c.Style = xf.Style;
                    //c.Format = xf.Format;


                }
            }

        }
        public static void ReadFromDataTable(ref DataTable dt, ref Worksheet ws, int StartRow, int StartCol, XF xf)
        {
            int row = StartRow;
            int col = StartCol;
            foreach (DataRow dataRow in dt.Rows)
            {

                col = StartCol;
                foreach (object dataItem in dataRow.ItemArray)
                {
                    object value = dataItem;

                    if (dataItem == DBNull.Value)
                        value = null;
                    if (dataRow.Table.Columns[col - StartCol].DataType == typeof(byte[]))
                        value = string.Format("[ByteArray({0})]", ((byte[])value).Length);

                    ws.Cells.Add(row, col++, value, xf);
                }
                row++;


            }
        }
        /// <summary>
        /// linq结果转为datatable
        /// </summary>
        /// <typeparam name="T">linq查询类型</typeparam>
        /// <param name="query">linq查询结果</param>
        /// <returns>datatable</returns>
        public static DataTable LinqQueryToDataTable<T>(IEnumerable<T> query)
        {
            DataTable tbl = new DataTable();
            PropertyInfo[] props = null;
            foreach (T item in query)
            {
                if (props == null)
                //尚未初始化              
                {
                    Type t = item.GetType();
                    props = t.GetProperties();
                    foreach (PropertyInfo pi in props)
                    {
                        Type colType = pi.PropertyType;
                        //針對Nullable<>特別處理                     
                        if (colType.IsGenericType
                            && colType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            colType = colType.GetGenericArguments()[0];
                        //建立欄位             
                        tbl.Columns.Add(pi.Name, colType);
                    }
                }
                DataRow row = tbl.NewRow();
                foreach (PropertyInfo pi in props)
                    row[pi.Name] = pi.GetValue(item, null) ?? DBNull.Value;
                tbl.Rows.Add(row);
            }
            return tbl;
        }

    }
}
