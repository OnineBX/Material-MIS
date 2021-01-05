/*------------------------------------------------------------------------------
 * Unit Name：EmailHelpercs
 * Description: Send E-mail
 * Author: Li Tao
 * Created Date: 2010-05-04
 *  ----------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;

namespace MMSPro.WebApp
{
    class EmailHelper
    {
        #region Fields
        private static LogHelper _log = LogHelper.GetInstance();
        #endregion

        #region Methods
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="from">源邮件地址</param>
        /// <param name="to">目的邮件地址</param>
        /// <param name="subject">邮件主题</param>
        /// <param name="body">邮件正文</param>
        /// <param name="smptServer">SMTP主机</param>
        /// <param name="userName">用户名</param>
        /// 
        /// <param name="userPwd">用户密码</param>
        /// <param name="mPriority">邮件优先级</param>
        /// <param name="emailEncoder">邮件编码</param>
        public void SendEmail(string from, string to, string subject, string body, string smptServer, string userName, string userPwd, MailPriority mPriority, Encoding emailEncoder)
        {
            try
            {
                using (MailMessage message = new MailMessage(from, to))
                {
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;
                    message.Priority = mPriority;
                    message.SubjectEncoding = emailEncoder;
                    message.BodyEncoding = emailEncoder;

                    SmtpClient client = new SmtpClient();
                    client.Host = smptServer;
                    client.UseDefaultCredentials = true;
                    //client.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                    client.Credentials = new NetworkCredential(userName, userPwd);
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Send(message);
                }
            }
            catch (Exception ex)
            {
                _log.Log(LogSeverity.Error, ex, "SendEmail异常001", string.Empty);
            }
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="from">源邮件地址</param>
        /// <param name="to">目的邮件地址</param>
        /// <param name="subject">邮件主题</param>
        /// <param name="body">邮件正文</param>
        /// <param name="smptServer">SMTP主机</param>
        /// <param name="mPriority">邮件优先级</param>
        /// <param name="emailEncoder">邮件编码</param>
        public void SendEmail(string from, string to, string subject, string body, string smptServer, MailPriority mPriority, Encoding emailEncoder)
        {
            try
            {
                using (MailMessage message = new MailMessage(from, to))
                {
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;
                    message.Priority = mPriority;
                    message.SubjectEncoding = emailEncoder;
                    message.BodyEncoding = emailEncoder;

                    SmtpClient client = new SmtpClient();
                    client.Host = smptServer;
                    client.Send(message);
                }
            }
            catch (Exception ex)
            {
                _log.Log(LogSeverity.Error, ex, "SendEmail异常002", string.Empty);
            }
        }
        #endregion
    }
}
