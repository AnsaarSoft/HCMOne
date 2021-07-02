using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using DIHRMS;



namespace ACHR.WinForms
{
    public partial class frmRptViewer : Form
    {

        public dbHRMS dbHrPayroll;
        public bool isSystem = false;
        public string Critaria = "";
        public string DateParameter = "";
        public string mfmcp = "";
        public string rptCode = "";

        public frmRptViewer()
        {
            InitializeComponent();
        }

        private void frmRptViewer_Load(object sender, EventArgs e)
        {
            dbHrPayroll = new dbHRMS(Program.objHrmsUI.hrConstr);
            int cnt = (from p in dbHrPayroll.TblRpts where p.RptCode == rptCode select p).Count();
            if (cnt > 0)
            {
                //TblRpts rpt = (from p in dbHrPayroll.TblRpts where p.RptCode == rptCode select p).Single();
                TblRpts rpt = (from p in dbHrPayroll.TblRpts where p.RptCode == rptCode select p).FirstOrDefault();
                byte[] rptBytes = rpt.RptFileStr.ToArray();
                this.Text = rpt.ReportName;
                FileStream fs = new FileStream(Application.StartupPath + "\\test.rpt", System.IO.FileMode.Create);
                int len = rptBytes.Length;
                fs.Write(rptBytes, 0, len);
                fs.Flush();
                fs.Close();

                ReportDocument report = new ReportDocument();
                report.Load(Application.StartupPath + "\\test.rpt");
                //report.SetDatabaseLogon(Program.objHrmsUI.HRMSDBuid, Program.objHrmsUI.HRMSDbPwd, Program.objHrmsUI.HRMSDbServer, Program.objHrmsUI.HRMSDbName);
                Program.SetReport(report);
                if (isSystem)
                {
                    ParameterFieldDefinitions fielDef = report.DataDefinition.ParameterFields;
                    ParameterDiscreteValue discVal1 = new ParameterDiscreteValue();
                    ParameterFieldDefinition fielLoc1 = fielDef["Critaria"];
                    ParameterValues paraVals1 = new ParameterValues();
                    paraVals1 = fielLoc1.CurrentValues;
                    discVal1.Value = Critaria;
                    paraVals1.Add(discVal1);
                    fielLoc1.ApplyCurrentValues(paraVals1);

                }
                ParameterFieldDefinitions crParameterdef = report.DataDefinition.ParameterFields;
                foreach (CrystalDecisions.CrystalReports.Engine.ParameterFieldDefinition def in crParameterdef)
                {
                    if (def.Name.Equals("CP"))    // check if parameter exists in report
                    {
                        ParameterFields paramFields = new ParameterFields();
                        ParameterField CurrentPeriod = new ParameterField();
                        CurrentPeriod.ParameterFieldName = "CP"; //Crystal Report Parameter name.
                        ParameterDiscreteValue dcBpCode = new ParameterDiscreteValue();
                        dcBpCode.Value = mfmcp;
                        CurrentPeriod.CurrentValues.Add(dcBpCode);
                        paramFields.Add(CurrentPeriod);
                        crystalReportViewer1.ParameterFieldInfo = paramFields;
                    }
                    if (def.Name.Equals("DateParameter"))    // check if parameter exists in report
                    {
                        ParameterFields paramFields = new ParameterFields();
                        ParameterField dtParamerter = new ParameterField();
                        dtParamerter.ParameterFieldName = "DateParameter"; //Crystal Report Parameter name.
                        ParameterDiscreteValue dtParameterValue = new ParameterDiscreteValue();
                        dtParameterValue.Value = DateParameter;
                        dtParamerter.CurrentValues.Add(dtParameterValue);
                        paramFields.Add(dtParamerter);
                        crystalReportViewer1.ParameterFieldInfo = paramFields;
                    }
                }
                System.Drawing.Printing.PrintDocument doctoprint = new System.Drawing.Printing.PrintDocument();
                report.PrintOptions.PrinterName = doctoprint.DefaultPageSettings.PrinterSettings.PrinterName;
                crystalReportViewer1.ReportSource = report;
            }
            else
            {
                MessageBox.Show("Invalid Report");
            }


        }
    }
}
