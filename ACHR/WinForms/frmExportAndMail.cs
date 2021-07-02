using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using DIHRMS;
using System.IO;
using System.Net.Mail;

namespace ACHR.WinForms
{
    public partial class frmExportAndMail : Form
    {
        public dbHRMS dbHrPayroll;
        public bool isSystem = false;
        public string Critaria = "";
        public string rptCode = "";
        public bool exportAndMail = true;
        public string salaySlipIDs = "";
        public bool mailSent = false;
        int totalReceipents = 0;
        int sentEmail = 1;
        public frmExportAndMail()
        {
            InitializeComponent();
        }

        private void frmExportAndMail_Load(object sender, EventArgs e)
        {

            //  button1_Click(sender, e);
          //  this.ShowInTaskbar = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
            dbHrPayroll = new dbHRMS(Program.objHrmsUI.hrConstr);
            salaySlipIDs = salaySlipIDs.Replace(", ", ",");
            string[] slipIDs = salaySlipIDs.Split(',');
            var selectedSlips = dbHrPayroll.TrnsSalaryProcessRegister.Where(o => slipIDs.Contains(o.Id.ToString())).ToList();
            totalReceipents = selectedSlips.Count;
            progressBar1.Maximum = totalReceipents;            
            foreach (TrnsSalaryProcessRegister slip in selectedSlips)
            {
                try
                {

                    int cnt = (from p in dbHrPayroll.TblRpts where p.RptCode == rptCode select p).Count();
                    if (cnt > 0)
                    {
                        TblRpts rpt = (from p in dbHrPayroll.TblRpts where p.RptCode == rptCode select p).Single();
                        byte[] rptBytes = rpt.RptFileStr.ToArray();
                        this.Text = rpt.ReportName;
                        FileStream fs = new FileStream(Application.StartupPath + "\\test.rpt", System.IO.FileMode.Create);
                        int len = rptBytes.Length;
                        fs.Write(rptBytes, 0, len);
                        fs.Flush();
                        fs.Close();
                        File.Create(Application.StartupPath + "\\test.pdf").Dispose();
                        ReportDocument report = new ReportDocument();
                        report.Load(Application.StartupPath + "\\test.rpt");
                        Program.SetReport(report);
                        if (isSystem)
                        {
                            ParameterFieldDefinitions fielDef = report.DataDefinition.ParameterFields;
                            ParameterDiscreteValue discVal1 = new ParameterDiscreteValue();
                            ParameterFieldDefinition fielLoc1 = fielDef["Critaria"];
                            ParameterValues paraVals1 = new ParameterValues();
                            paraVals1 = fielLoc1.CurrentValues;
                            discVal1.Value = string.Format("Where TrnsSalaryProcessRegister.Id in({0})", slip.Id);
                            //discVal1.Value = Critaria;                            
                            paraVals1.Add(discVal1);
                            fielLoc1.ApplyCurrentValues(paraVals1);
                        }
                        System.Drawing.Printing.PrintDocument doctoprint = new System.Drawing.Printing.PrintDocument();
                        report.PrintOptions.PrinterName = doctoprint.DefaultPageSettings.PrinterSettings.PrinterName;
                        crystalReportViewer1.ReportSource = report;
                        //  report.Dispose();
                        ExportOptions CrExportOptions;
                        DiskFileDestinationOptions CrDiskFileDestinationOptions = new DiskFileDestinationOptions();
                        PdfRtfWordFormatOptions CrFormatTypeOptions = new PdfRtfWordFormatOptions();
                        CrDiskFileDestinationOptions.DiskFileName = Application.StartupPath + "\\test.pdf";
                        CrExportOptions = report.ExportOptions;
                        CrExportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
                        CrExportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
                        CrExportOptions.DestinationOptions = CrDiskFileDestinationOptions;
                        CrExportOptions.FormatOptions = CrFormatTypeOptions;
                        report.Export();
                        var email = dbHrPayroll.MstEmailConfig.FirstOrDefault();
                        string MonthName = Convert.ToDateTime(slip.CfgPeriodDates.StartDate)
                            .ToString("MMMM", CultureInfo.InvariantCulture);
                        string YearName = Convert.ToDateTime(slip.CfgPeriodDates.StartDate)
                            .ToString("yyyy", CultureInfo.InvariantCulture);
                        string EmpName = slip.MstEmployee.FirstName + " " + (!string.IsNullOrEmpty(slip.MstEmployee.MiddleName) ? slip.MstEmployee.MiddleName + " " : "") + slip.MstEmployee.LastName;
                        MailMessage mail = new MailMessage(email.FromEmail, slip.MstEmployee.OfficeEmail,
                        string.Format("Salary Slip for month of {0}", MonthName),
                        string.Format(@"<b>Dear {0}</b>,
                                        <br> 
                                        <br> 
                                        Please find attached your pay-slip, for the month of {1} {2}.<br><br>
                                        In case of any ambiguity, please consult HR Department for resolution of your queries.<br><br>
                                        <b>This is a SAP generated pay-slip and does not require any signature.</b><br>
                                        <br>
                                        <b>Best Regards,</b><br>
                                        <br>
                                        <b>HR Department.</b>", EmpName, MonthName, YearName));
                        mail.IsBodyHtml = true;
                        try
                        {
                            Attachment attachment = new Attachment(Application.StartupPath + "\\test.pdf");
                            attachment.Name = slip.PeriodName + " " + EmpName + ".pdf";  // set name here
                            mail.Attachments.Add(attachment);
                            //mail.Attachments.Add(new Attachment(Application.StartupPath + "\\test.pdf"));
                            SmtpClient client = new SmtpClient(email.SMTPServer);
                            client.Port = Convert.ToInt32(email.SMTPort);
                            client.Credentials = new System.Net.NetworkCredential(email.FromEmail, email.Password);
                            //if (email.TestEmail.ToLower().Trim() == "y")
                            if (Convert.ToBoolean(email.SSL) == true)
                            {
                                client.EnableSsl = true;
                            }
                            else
                            {
                                client.EnableSsl = false;
                            }
                            client.Send(mail);
                            mail.Dispose();
                            slip.FlgEmailed = true;
                        }
                        catch (Exception ex)
                        {
                            mail.Dispose();
                            
                        }
                        //progressBar1.PerformStep();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                
            }
            dbHrPayroll.SubmitChanges();                
            this.Close();
        }

        private void frmExportAndMail_Shown(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }
    }
}
