using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Text.RegularExpressions;

namespace Utility
{
    public enum CodeValideType
    {
        编号_只有数字 = 1,
        正整数,
        数字_正负整数或者小数,
        带两位小数的正实数,
        零或者非零开头的整数,
        数字或字母或下划线,
        用户名称_1到12个大小写英文字母或下划线开头_后面可以接数字,
        用户真实姓名_0到4个汉字组成_右面可以接0到10位的数字编号,
        输入项只能在50个汉字以内,
        输入项只能在100个汉字以内,
        密码_6到20位的长度,
        用户组或文件类别_只能为汉字_大小写字母_数字,
        时间格式
        
    }

    
    public class Security
    {
        //加密用的盐值


        private const string salt = "!@#$%";
        //加密用的密钥和向量


        static private byte[] bytKey = { 0x21, 0x6e, 0x33, 0x4e, 0x65, 0x35, 0x5f, 0xa2 };
        static private byte[] bytV = { 0x41, 0xbe, 0x27, 0xac, 0x42, 0x29, 0x5d, 0xa3 };
        static Security()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        /**************************************************/
        //                                               //
        //               加密，解密                     //
        //                                              //
        /*************************************************/

        /// <summary>
        /// des加密
        /// </summary>
        /// <param name="values">要加密的字符串</param>
        /// <returns>加密结果</returns>
        public static  string funDesEncrypt(string values)
        {
            byte[] strValue;

            SymmetricAlgorithm des;
            des = new DESCryptoServiceProvider();
            strValue = System.Text.Encoding.UTF8.GetBytes(values);
            ICryptoTransform icryptotransform = des.CreateEncryptor(bytKey, bytV);
            MemoryStream memorystream = new MemoryStream();
            CryptoStream cryptostream = new CryptoStream(memorystream, icryptotransform, CryptoStreamMode.Write);
            cryptostream.Write(strValue, 0, strValue.Length);
            cryptostream.FlushFinalBlock();
            cryptostream.Close();
            des.Clear();
            return System.Convert.ToBase64String(memorystream.ToArray());

        }


        /// <summary>
        ///des解密 
        /// </summary>
        /// <param name="values">要解密的字符串</param>
        /// <returns>解密结果</returns>
        public static  string funDesDecrypt(string values)
        {
            byte[] strValue;
            SymmetricAlgorithm des;
            des = new DESCryptoServiceProvider();
            ICryptoTransform icryptotransform = des.CreateDecryptor(bytKey, bytV);
            strValue = System.Convert.FromBase64String(values);
            MemoryStream memorystream = new MemoryStream();
            CryptoStream cryptostream = new CryptoStream(memorystream, icryptotransform, CryptoStreamMode.Write);
            cryptostream.Write(strValue, 0, strValue.Length);
            cryptostream.FlushFinalBlock();
            cryptostream.Close();
            des.Clear();
            return System.Text.Encoding.UTF8.GetString(memorystream.ToArray());
        }


        /// <summary>
        /// md5加密
        /// </summary>
        /// <param name="strValue">要加密的字符串</param>
        /// <returns>加密结果</returns>
        public static  string funMD5Encrypt(string strValue)
        {
            string strTemp = salt + strValue;
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytValue;
            byte[] bythash;
            bytValue = System.Text.Encoding.UTF8.GetBytes(strTemp);
            bythash = md5.ComputeHash(bytValue);
            md5.Clear();
            return Convert.ToBase64String(bythash);
        }



        /**************************************************/
        //                                               //
        //               正则表达试相关                  //
        //                                              //
        /*************************************************/

        /// <summary>
        /// 用正则表达式检查传入的参数是否为数字

        /// </summary>
        /// <param name="strValue">要检查的字符</param>
        /// <returns>符合true,否则返回false</returns>
        public static  bool funCheckNum(string strValue)
        {

            if (Regex.Match(strValue, @"^\d*$").Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 验证传入参数是否满足为几位要求

        /// </summary>
        /// <param name="strValue">传入参数</param>
        /// <param name="digit">验证的位数</param>
        /// <returns>符合true,否则返回false</returns>
        public static  bool funCheckNum(string strValue, int digit)
        {

            if (Regex.Match(strValue, @"^\d{" + digit + "}$").Success)
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        /// <summary>
        /// 验证字符串

        /// </summary>
        /// <param name="value">需要验证的字符串</param>
        /// <param name="type">验证类型</param>
        /// <returns>验证通过返回true,失败返回false</returns>
        public static bool ValidString(string value, CodeValideType type)
        {
            bool isValid = false;
            System.Text.RegularExpressions.Regex reg;
            switch (type)
            {
                case CodeValideType.编号_只有数字:
                    reg = new Regex("^[0-9]+$");
                    isValid = reg.IsMatch(value);
                    break;
                case CodeValideType.正整数:
                    reg = new Regex("^[0-9 ]*$");
                    isValid = reg.IsMatch(value);
                    break;

                case CodeValideType.数字_正负整数或者小数:
                    reg = new Regex("^[-]?[0-9]+(.[0-9]+)?$");
                    isValid = reg.IsMatch(value);
                    break;
                case CodeValideType.带两位小数的正实数:
                    reg = new Regex("^[0-9]+(.[0-9]{2})?$");
                    isValid = reg.IsMatch(value);
                    break;
                case CodeValideType.数字或字母或下划线:
                    reg = new Regex(@"/^\w+$/");
                    isValid =reg.IsMatch(value);
                    break;
                case CodeValideType.零或者非零开头的整数:
                    reg = new Regex("^(0|[1-9][0-9]*)$");
                    isValid = reg.IsMatch(value);
                    break;
                case CodeValideType.输入项只能在50个汉字以内:
                    if (value.Length <= 50 && value.Length > 0)
                    {
                        isValid = true;
                    }
                    break;
                case CodeValideType.输入项只能在100个汉字以内:
                    if (value.Length <= 100 && value.Length > 0)
                    {
                        isValid = true;
                    }
                    break;
                case CodeValideType.用户名称_1到12个大小写英文字母或下划线开头_后面可以接数字:
                    if (value.Length <= 12 && value.Length > 0)
                    {
                        reg = new Regex("^(_*[A-Za-z]){2}[_A-Za-z0-9]*$");
                        isValid = reg.IsMatch(value);
                    }
                    break;
                case CodeValideType.用户真实姓名_0到4个汉字组成_右面可以接0到10位的数字编号:
                    reg = new Regex("^[\u4e00-\u9fa5]{2,4}[0-9]{0,10}$");
                    isValid = reg.IsMatch(value);
                    break;
                case CodeValideType.密码_6到20位的长度:
                    if (value.Length > 0 && value.Length <= 20)
                    {
                        isValid = true;
                    }
                    break;
                case CodeValideType.用户组或文件类别_只能为汉字_大小写字母_数字:
                    reg = new Regex("^[\u4e00-\u9fa5|A-Z|a-z|0-9]+$");
                    isValid = reg.IsMatch(value);
                    break;
                case CodeValideType.时间格式:
                    isValid = true;
                    try
                    {
                        Convert.ToDateTime(value);
                    }
                    catch
                    {
                        isValid = false;
                    }

                    break;

            }

            return isValid;
        }

        /// <summary>
        /// 验证传入参数是否满足是从第lower位到upper位的数字
        /// </summary>
        /// <param name="strValue">传入参数</param>
        /// <param name="lower">最小位数</param>
        /// <param name="upper">最大位数</param>
        public static  bool funCheckNum(string strValue, int lower, int upper)
        {

            if (Regex.Match(strValue, @"^\d{" + lower + "," + upper + "}$").Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 检查参数是否是否包含特殊符号

        /// </summary>
        /// <param name="strValue">要检查的字符串</param>
        /// <returns></returns>
        public static bool funCheckNumChar(string strValue)
        {
            // if (Regex.Match(strValue, @"^[!.,，?！？。、”“‘’:：() \-{}_><~a-zA-Z0-9\u4e00-\u9fa5]*$").Success)
            //  if (Regex.Match(strValue, @"^[!.,，?！？。、”\[\]“‘’'" + "\"" + @"\\:：() \-{}_><~a-zA-Z0-9\u4e00-\u9fa5]*$").Success)
            if (Regex.Match(strValue, @"[!，?！？。、”\[\]“‘’'" + "\"" + @"\\:： \-{}><~]").Success)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// 检查参数是否是否仅有数字与字母
        /// </summary>
        /// <param name="strValue">要检查的字符串</param>
        /// <returns></returns>
        public static  bool funCheckJustNumChar(string strValue)
        {
            if (Regex.Match(strValue, @"^[!.,，?！？。、”“‘’:：() \-{}_><~\u4e00-\u9fa5]*$").Success)
            //  if (Regex.Match(strValue, @"^[!.,，?！？。、”\[\]“‘’'" + "\"" + @"\\:：() \-{}_><~a-zA-Z0-9\u4e00-\u9fa5]*$").Success)
            // if (Regex.Match(strValue, @"[!，?！？。、”\[\]“‘’'" + "\"" + @"\\:： \-{}><~]").Success)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        /// <summary>
        /// 检查备注字串是否自有非法字符

        /// </summary>
        /// <param name="strValue">要检查的字符串</param>
        /// <returns></returns>
        public static  bool funCheckRemarkChar(string strValue)
        {
            if (System.Text.RegularExpressions.Regex.Match(strValue, @"['<>" + "\"" + @"]").Success)
            {
                return false;
            }
            else
            {
                return true;
            }
        }



        /// <summary>
        /// 判断目录存在否，如果不存在，就创建该目录
        /// </summary>
        /// <param name="strFile">文件路径（包含有完整的文件扩展名）</param>
        /// <returns></returns>
        public static void CheckHaveDirectory(string strPath)
        {
            if (Directory.Exists(strPath) == false)
            {
                Directory.CreateDirectory(strPath);
            }
        }




        /// <summary>
        /// 取得文件的扩展名
        /// </summary>
        /// <param name="strFile">文件路径（包含有完整的文件扩展名）</param>
        /// <returns></returns>
        public static string GetExtendName(string strFile)
        {
            string typeName = null;

            int pos = strFile.LastIndexOf('.');

            if (pos > 0)
            {
                typeName = strFile.Substring(pos + 1, strFile.Length - (pos + 1));
            }
            if (typeName != null)
            {
                typeName = typeName.ToLower();
            }
            return typeName;
        }

        /// <summary>
        /// 取得文件的名
        /// </summary>
        /// <param name="strFile">文件路径（包含有完整的文件扩展名）</param>
        /// <returns></returns>
        public static string GetFileName(string strFileFullName)
        {
            int I, j;
            string infilename, outFileName, tempFile, strFileName;

            strFileName = "";
            infilename = strFileFullName;
            outFileName = infilename;

            I = outFileName.LastIndexOf(".");

            if (I > 1)
            {
                tempFile = outFileName.Substring(0, I);
                j = tempFile.LastIndexOf("\\");
                strFileName = tempFile.Substring(j + 1, tempFile.Length - j - 1);

                //outFileName = outFileName.Substring(I,infilename.Length - I);
            }

            return strFileName;
        }


        public static void CheckDirectiory(string file_Path)
        {
            if (Directory.Exists(file_Path) == true)
            {
                DirectoryInfo tempDir = new DirectoryInfo(file_Path);
                foreach (FileInfo tempFile in tempDir.GetFiles())
                {
                    if (tempFile != null)
                    {
                        try
                        {
                            tempFile.Delete();
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="file_Path"></param>
        public static void DeleteFile(string strFile_Path)
        {            
            if (File.Exists(strFile_Path) == true)
            {
                try
                {
                    File.Delete(strFile_Path);
                }
                catch
                {
                }
            }
        }


        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="file_Path"></param>
        public static void SaveFile(string file_Path,string strSaveFile_Path)
        { 
            try
            {
                File.Copy(file_Path, strSaveFile_Path,true);
            }
            catch
            {
            }           
        }

    }
}
