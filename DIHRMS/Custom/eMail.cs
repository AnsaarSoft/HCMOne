using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;

namespace DIHRMS.Custom
{
    public class eMail
    {
        
        dbHRMS oDB = null;

        public eMail( dbHRMS pDB )
        {
            oDB = pDB;
        }

        public void SendEmail(String pToEmail,String pToName, String pSubject, String peBody)
        {
            try
            {
                //Variable 
                
                
                //Logic
                var EmailRecord = (from a in oDB.MstEmailConfig
                                   select a).FirstOrDefault();

                SmtpClient oClient = new SmtpClient();
                oClient.Credentials = new NetworkCredential(EmailRecord.FromEmail, EmailRecord.Password);
                oClient.Host = EmailRecord.SMTPServer;
                oClient.Port = Convert.ToInt32(EmailRecord.SMTPort);
                oClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                oClient.UseDefaultCredentials = false;
                oClient.EnableSsl = true;

                MailAddress oFrom = new MailAddress(EmailRecord.FromEmail, "HRMS Solution");
                MailAddress oTo = new MailAddress(pToEmail, pToName);
                MailMessage MsgMail = new MailMessage(oFrom, oTo);

                MsgMail.Subject = pSubject;
                MsgMail.Body = peBody;
                MsgMail.IsBodyHtml = false;

                oClient.Send(MsgMail);
                //Assignment
                
            }
            catch (Exception Ex)
            {
                  
                
            }
        }
    }
}
