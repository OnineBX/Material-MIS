using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMSPro.WebApp
{
    public class DataCom
    {
        /// <summary>
        /// 生成query筛选字段的方法
        /// </summary>
        /// <param name="strconlection">一个stringcollection包含所有需要显示的字段</param>
        /// <returns>返回xml格式的筛选字段</returns>
        public static string getVileFieldXML(System.Collections.Specialized.StringCollection strconlection)
        {

            StringBuilder sbResult = new StringBuilder();
            foreach (string strT in strconlection)
            {
                sbResult.Append("<FieldRef Name='" + strT + "'/>");
            }
            return sbResult.ToString();
        }
    }
}
