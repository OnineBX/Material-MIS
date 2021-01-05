/*------------------------------------------------------------------------------
 * Unit Name：LogHelper.cs
 * Description: 用于向事件管理器创建日志
 * Author: Li Tao
 * Created Date: 2010-05-04
 * ----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;

namespace MMSPro.WebApp
{
    public enum LogSeverity
    {
        Error, Warning, Info, Debug
    }

    struct LogStruct
    {
        public int level;
        public int parcount;
        public string message;
    }

    public class LogHelper
    {
        #region Fields

        private static int logLevel = 3;

        private static EventLog _log;
        private static Hashtable _loght;

        private static bool _isDebug;
        private static bool _isInfo;
        private static bool _isWarn;
        private static bool _isError;

        private const int MAX_MSG_LENGTH = 32766;

        #endregion

        #region Properties
        #endregion

        #region Methods

        /// <summary>
        /// LogHelper
        /// </summary>
        static LogHelper()
        {
            try
            {
                _log = new EventLog("Material Management System", System.Environment.MachineName, "WebApp");
                if (logLevel < 0)
                {
                    try
                    {
                        logLevel = 3;
                    }
                    catch (Exception e)
                    {
                        _log.WriteEntry("Error when create Logger : " + e.Message, EventLogEntryType.Error);
                        logLevel = 3;
                    }
                }
                SetLogLevel();
            }
            catch (Exception e)
            {
            }
        }

        /// <summary>
        /// LogHelper
        /// </summary>
        /// <returns>LogHelper</returns>
        public static LogHelper GetInstance()
        {
            try
            {
                return new LogHelper();
            }
            catch (Exception e)
            {
                Console.WriteLine("Create LogHelper error." + e.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Set log level
        /// </summary>
        private static void SetLogLevel()
        {
            switch (logLevel)
            {
                case 4:
                    //m_LogToLogFile = true;
                    _isDebug = true;
                    _isInfo = true;
                    _isWarn = true;
                    _isError = true;
                    break;
                case 3:
                    _isDebug = true;
                    _isInfo = true;
                    _isWarn = true;
                    _isError = true;
                    break;
                case 2:
                    _isInfo = true;
                    _isWarn = true;
                    _isError = true;
                    break;
                case 1:
                    _isWarn = true;
                    _isError = true;
                    break;
                case 0:
                    _isError = true;
                    break;
            }
        }

        /// <summary>
        /// Log method
        /// </summary>
        /// <param name="severity"></param>
        /// <param name="formatStrId"></param>
        /// <param name="args"></param>
        public void Log(LogSeverity severity, string formatStrId, params object[] args)
        {
            string sMsg = string.Empty;
            try
            {
                if (args.Length != ((LogStruct)_loght[formatStrId]).parcount)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        sMsg += "\t" + args[i].ToString();
                    }
                    Error("Wrong Log Params Count:" + formatStrId + "\t" + sMsg);
                    return;
                }

                sMsg = "[" + formatStrId + "]\n" + string.Format(((LogStruct)_loght[formatStrId]).message, args);
            }
            catch (Exception)
            {
                sMsg = "[" + formatStrId + "]";
                for (int i = 0; i < args.Length; i++)
                {
                    sMsg += "\t" + args[i].ToString();
                }
            }
            try
            {
                switch (severity)
                {
                    case LogSeverity.Error:
                        ErrorInternal(sMsg);
                        break;
                    case LogSeverity.Warning:
                        WarnInternal(sMsg);
                        break;
                    case LogSeverity.Info:
                        InfoInternal(sMsg);
                        break;
                    case LogSeverity.Debug:
                        DebugInternal(sMsg);
                        break;
                }
            }
            catch { }
        }

        /// <summary>
        /// Log method
        /// </summary>
        /// <param name="formatStrId"></param>
        /// <param name="args"></param>
        public void Log(string formatStrId, params object[] args)
        {
            string sMsg = string.Empty;
            bool bExist = true;
            try
            {
                if (args.Length != ((LogStruct)_loght[formatStrId]).parcount)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        sMsg += "\t" + args[i].ToString();
                    }
                    Error("Wrong Log Params Count:" + formatStrId + "\t" + sMsg);
                    return;
                }

                sMsg = "[" + formatStrId + "]\n" + string.Format(((LogStruct)_loght[formatStrId]).message, args);
            }
            catch
            {
                sMsg = "[" + formatStrId + "]";
                for (int i = 0; i < args.Length; i++)
                {
                    sMsg += "\t" + args[i].ToString();
                }
                bExist = false;
            }

            try
            {
                if (bExist)
                {
                    switch (((LogStruct)_loght[formatStrId]).level)
                    {
                        case 0:
                            ErrorInternal(sMsg);
                            break;
                        case 1:
                            WarnInternal(sMsg);
                            break;
                        case 2:
                            InfoInternal(sMsg);
                            break;
                        case 3:
                            DebugInternal(sMsg);
                            break;
                    }
                }
                else
                {
                    WarnInternal(sMsg);
                }
            }
            catch { }
        }


        /// <summary>
        /// Log method
        /// </summary>
        /// <param name="severity"></param>
        /// <param name="e"></param>
        /// <param name="formatStrId"></param>
        /// <param name="args"></param>
        public void Log(LogSeverity severity, Exception e, string formatStrId, params object[] args)
        {
            string sMsg = string.Empty;
            try
            {
                sMsg = "[" + formatStrId + "]\n" + string.Format(((LogStruct)_loght[formatStrId]).message, args);
            }
            catch (Exception)
            {
                sMsg = "[" + formatStrId + "]";
                for (int i = 0; i < args.Length; i++)
                {
                    sMsg += "\t" + args[i].ToString();
                }
            }

            try
            {
                switch (severity)
                {
                    case LogSeverity.Error:
                        ErrorInternal(sMsg, e);
                        break;
                    case LogSeverity.Warning:
                        WarnInternal(sMsg, e);
                        break;
                    case LogSeverity.Info:
                        InfoInternal(sMsg, e);
                        break;
                    case LogSeverity.Debug:
                        DebugInternal(sMsg, e);
                        break;
                }
            }
            catch { }
        }


        /// <summary>
        /// Write entry, real write event log into eventviewer
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="type"></param>
        private void WriteEntry(string msg, EventLogEntryType type)
        {
            try
            {
                msg = ChangeMessage(msg);
                if (msg.Length > MAX_MSG_LENGTH)
                {
                    int nextLength = MAX_MSG_LENGTH;
                    int offset = 0;
                    while (nextLength > 0)
                    {
                        _log.WriteEntry(msg.Substring(offset, nextLength), type);
                        offset += MAX_MSG_LENGTH;
                        nextLength = msg.Length - offset;
                        if (nextLength > MAX_MSG_LENGTH)
                            nextLength = MAX_MSG_LENGTH;
                    }
                }
                else
                {
                    _log.WriteEntry(msg, type);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Change messaeg
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private string ChangeMessage(string msg)
        {
            try
            {
                string[] keys = new string[] { "pass", "pwd" };
                msg = msg.ToLower();
                string msgall = "";
                msg = Replace(msg, "pass");
                msg = Replace(msg, "pwd");
            }
            catch (Exception ex)
            { }
            return msg;
        }

        /// <summary>
        /// Replace message string
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string Replace(string msg, string key)
        {
            string msgall = "";
            try
            {
                int k = 0;
                int len = msg.Length;
                int i = msg.IndexOf(key, k);
                bool flag = true;
                while (i > 0)
                {
                    if ((i + 18) > len)
                    {
                        flag = false;
                        string temp1 = msg.Substring(k);//,i+18-k);
                        msgall = msgall + ReplacePassword(temp1, key);
                        k = i;
                        break;
                    }
                    string temp = msg.Substring(k, i + 18 - k);
                    msgall = msgall + ReplacePassword(temp, key);
                    k = i + 18;
                    i = msg.IndexOf(key, k);
                }
                string aaa = "";
                if (flag)
                {
                    aaa = msg.Substring(k);
                    msgall = msgall + aaa;
                }
            }
            catch (Exception ex)
            {
                string ss = "";
            }
            return msgall;
        }


        /// <summary>
        /// Replace message password
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string ReplacePassword(string msg, string key)
        {
            int startIndex = msg.IndexOf(key);
            if (startIndex < 0)
            {
                return msg;
            }
            int leng = msg.Length - (startIndex + 18);
            if (leng > 0)
            {
                string temp = msg.Substring(startIndex + 1, 18);
                msg = msg.Replace(temp, key + "=******");
            }
            else
            {
                string temp = msg.Substring(startIndex);
                msg = msg.Replace(temp, key + "=******");
            }
            return msg;
        }

        /// <summary>
        /// Output debug level log
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        private void Debug(string msg, Exception e)
        {
            try
            {
                DebugInternal(msg, e);
            }
            catch { }
        }

        /// <summary>
        /// Real output debug level log
        /// </summary>
        /// <param name="msg"></param>
        private void DebugInternal(string msg)
        {
            if (!_isDebug)
            {
                return;
            }

            WriteEntry(string.Format("{0}", msg), EventLogEntryType.Information);

        }

        /// <summary>
        /// Output debug level log
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        private void DebugInternal(string msg, Exception e)
        {
            DebugInternal(string.Format("{0} : {1}\n{2}", msg, e.Message, e.StackTrace));
        }


        /// <summary>
        /// Output information level log
        /// </summary>
        /// <param name="msg"></param>
        private void Info(string msg)
        {
            try
            {
                InfoInternal(msg);
            }
            catch { }
        }

        /// <summary>
        /// Real output information level log
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        private void Info(string msg, Exception e)
        {
            try
            {
                InfoInternal(msg, e);
            }
            catch { }
        }


        /// <summary>
        /// Real output information level log
        /// </summary>
        /// <param name="msg"></param>
        private void InfoInternal(string msg)
        {
            if (!_isInfo)
            {
                return;
            }

            WriteEntry(string.Format("{0}\n", msg), EventLogEntryType.Information);

        }


        /// <summary>
        /// Real output information level log
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        private void InfoInternal(string msg, Exception e)
        {
            InfoInternal(string.Format("{0} : {1}\n{2}", msg, e.Message, e.StackTrace));
        }

        /// <summary>
        /// Output Warning level log
        /// </summary>
        /// <param name="msg"></param>
        private void Warn(string msg)
        {
            try
            {
                WarnInternal(msg);
            }
            catch { }
        }

        /// <summary>
        /// Real Output Warning level log
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        private void Warn(string msg, Exception e)
        {
            try
            {
                WarnInternal(msg, e);
            }
            catch { }
        }

        /// <summary>
        /// Output Warning level log
        /// </summary>
        /// <param name="msg"></param>
        private void WarnInternal(string msg)
        {
            if (!_isWarn)
            {
                return;
            }

            WriteEntry(string.Format("{0}", msg), EventLogEntryType.Warning);
        }

        /// <summary>
        /// Output Warning level log
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        private void WarnInternal(string msg, Exception e)
        {
            WarnInternal(string.Format("{0} : {1}\n{2}", msg, e.Message, e.StackTrace));
        }

        /// <summary>
        /// Output error level log
        /// </summary>
        /// <param name="msg"></param>
        private void Error(string msg)
        {
            try
            {
                ErrorInternal(msg);
            }
            catch { }
        }

        /// <summary>
        /// Output error level log
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        private void Error(string msg, Exception e)
        {
            try
            {
                ErrorInternal(msg, e);
            }
            catch { }
        }

        /// <summary>
        /// Real output Warning level log
        /// </summary>
        /// <param name="msg"></param>
        private void ErrorInternal(string msg)
        {
            if (!_isError)
            {
                return;
            }
            WriteEntry(string.Format("{0}", msg), EventLogEntryType.Error);
        }

        /// <summary>
        /// Output error level log
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="e"></param>
        private void ErrorInternal(string msg, Exception e)
        {
            ErrorInternal(string.Format("{0} : {1}\n{2}", msg, e.Message, e.StackTrace));
        }

        #endregion
    }
}
