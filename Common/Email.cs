using System.Configuration;
using System.Net.Mail;
using System.Text;

namespace Common
{
    public class Email
    {
        /// <summary>
        /// SMTP服务器
        /// </summary>
        public static readonly string SmtpServer = ConfigurationManager.AppSettings["SmtpServer"];
        /// <summary>
        /// 登陆用户名
        /// </summary>
        public static readonly string From = ConfigurationManager.AppSettings["From"];
        /// <summary>
        /// 登陆密码
        /// </summary>
        public static readonly string Password = ConfigurationManager.AppSettings["Password"];
        /// <summary>
        /// 收件人
        /// </summary>
        public static readonly string To = ConfigurationManager.AppSettings["To"];
        public static void SendEmail(string mailSubject, string mailContent)
        {
            // 设置发送方的邮件信息,例如使用网易的smtp
            string smtpServer = SmtpServer;
            string mailFrom = From;
            string userPassword = Password;

            // 邮件服务设置
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;//指定电子邮件发送方式
            smtpClient.EnableSsl = true;   //启动SSL加密
            smtpClient.Host = smtpServer; //指定SMTP服务器
            smtpClient.Credentials = new System.Net.NetworkCredential(mailFrom, userPassword);//用户名和密码

            // 发送邮件设置        
            MailMessage mailMessage = new MailMessage(mailFrom, To); // 发送人和收件人
            mailMessage.Subject = mailSubject;//主题
            mailMessage.Body = mailContent;//内容
            mailMessage.BodyEncoding = Encoding.UTF8;//正文编码
            mailMessage.IsBodyHtml = true;//设置为HTML格式
            mailMessage.Priority = MailPriority.Low;//优先级
            smtpClient.Send(mailMessage); // 发送邮件

        }
    }
}
