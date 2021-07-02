using System;
using System.IO;
using System.Data;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;
using DIHRMS.Custom;

namespace ACHR.Screen
{
    partial class frm_Reports : HRMSBaseForm
    {
        public IEnumerable<TblRpts> reports;

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);

            InitiallizeForm();
            
            AddNewRecord();

            getData();

        }
        
        private void getData()
        {
            dtMat.Rows.Clear();
            CodeIndex.Clear();
            reports = from p in dbHrPayroll.TblRpts select p;
            int i = 0;
            foreach (TblRpts ele in reports)
            {
                CodeIndex.Add(ele.ReportId.ToString(), i);
                dtMat.Rows.Add(1);
                dtMat.SetValue("Code", i, ele.RptCode);
                dtMat.SetValue("id", i, ele.ReportId.ToString());
                dtMat.SetValue("Name", i, ele.ReportName);
                dtMat.SetValue("Menu", i, ele.ReportIn);
                if (ele.FlgEmployee != null)
                {
                    if (ele.FlgEmployee == true) dtMat.SetValue(mEmployee.DataBind.Alias, i, "Y");
                    if (ele.FlgEmployee == false) dtMat.SetValue(mEmployee.DataBind.Alias, i, "N");
                }
                else
                {
                    dtMat.SetValue(mEmployee.DataBind.Alias, i, "N");
                }
                if (ele.FlgDept != null)
                {
                    if (ele.FlgDept == true) dtMat.SetValue(mDepartment.DataBind.Alias, i, "Y");
                    if (ele.FlgDept == false) dtMat.SetValue(mDepartment.DataBind.Alias, i, "N");
                }
                else
                {
                    dtMat.SetValue(mDepartment.DataBind.Alias, i, "N");
                }
                if (ele.FlgLocation != null)
                {
                    if (ele.FlgLocation == true) dtMat.SetValue(mLocation.DataBind.Alias, i, "Y");
                    if (ele.FlgLocation == false) dtMat.SetValue(mLocation.DataBind.Alias, i, "N");
                }
                else
                {
                    dtMat.SetValue(mLocation.DataBind.Alias, i, "N");
                }
                if (ele.FlgDateFrom != null)
                {
                    if (ele.FlgDateFrom == true) dtMat.SetValue(mDateFrom.DataBind.Alias, i, "Y");
                    if (ele.FlgDateFrom == false) dtMat.SetValue(mDateFrom.DataBind.Alias, i, "N");
                }
                else
                {
                    dtMat.SetValue(mDateFrom.DataBind.Alias, i, "N");
                }
                if (ele.FlgDateTo != null)
                {
                    if (ele.FlgDateTo == true) dtMat.SetValue(mDateTo.DataBind.Alias, i, "Y");
                    if (ele.FlgDateTo == false) dtMat.SetValue(mDateTo.DataBind.Alias, i, "N");
                }
                else
                {
                    dtMat.SetValue(mDateTo.DataBind.Alias, i, "N");
                }
                if (ele.FlgPreviousPeriod != null)
                {
                    if (ele.FlgPreviousPeriod == true) dtMat.SetValue(mPreviousPeriod.DataBind.Alias, i, "Y");
                    if (ele.FlgPreviousPeriod == false) dtMat.SetValue(mPreviousPeriod.DataBind.Alias, i, "N");
                }
                else
                {
                    dtMat.SetValue(mPreviousPeriod.DataBind.Alias, i, "N");
                }
                if (ele.FlgPeriod != null)
                {
                    if (ele.FlgPeriod == true) dtMat.SetValue(mPeriod.DataBind.Alias, i, "Y");
                    if (ele.FlgPeriod == false) dtMat.SetValue(mPeriod.DataBind.Alias, i, "N");
                }
                else
                {
                    dtMat.SetValue(mPeriod.DataBind.Alias, i, "N");
                }
                if (ele.FlgCritaria != null)
                {
                    if (ele.FlgCritaria == true) dtMat.SetValue(mCritaria.DataBind.Alias, i, "Y");
                    if (ele.FlgCritaria == false) dtMat.SetValue(mCritaria.DataBind.Alias, i, "N");
                }
                else
                {
                    dtMat.SetValue(mCritaria.DataBind.Alias, i, "N");
                }
                mtReports.AddRow(1);

                i++;
            }
            totalRecord = i;
            mtReports.LoadFromDataSource();
        }
        
        public override void fillFields()
        {
            base.fillFields();

            if (currentRecord >= 0)
            {

                txName.Active = true;

               ItxCode .Enabled = false;

                TblRpts record = reports.ElementAt<TblRpts>(currentRecord);

                txCode.Value = record.RptCode;
                txName.Value = record.ReportName;
                txMenu.Value = record.ReportIn;
                chkEmployee.Checked = record.FlgEmployee != null ? Convert.ToBoolean(record.FlgEmployee) : false;
                chkDepartment.Checked = record.FlgDept != null ? Convert.ToBoolean(record.FlgDept) : false;
                chkLocation.Checked = record.FlgLocation != null ? Convert.ToBoolean(record.FlgLocation) : false;
                chkDateFrom.Checked = record.FlgDateFrom != null ? Convert.ToBoolean(record.FlgDateFrom) : false;
                chkDateTo.Checked = record.FlgDateTo != null ? Convert.ToBoolean(record.FlgDateTo) : false;
                chkPreviousPeriod.Checked = record.FlgPreviousPeriod != null ? Convert.ToBoolean(record.FlgPreviousPeriod) : false;
                chkPeriod.Checked = record.FlgPeriod != null ? Convert.ToBoolean(record.FlgPeriod) : false;
                chkCritaria.Checked = record.FlgCritaria != null ? Convert.ToBoolean(record.FlgCritaria) : false;
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;

            }

        }
        
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                 case "btPick":
                    getFileName();
                    break;
                 case "1":
                    SubmitForm();
                    break;

                case "mtReports":

                        if (pVal.Row >= 1 && pVal.Row <= mtReports.RowCount)
                        {
                            try
                            {
                                string id = Convert.ToString(dtMat.GetValue("id", pVal.Row - 1));
                                getRecord(id.ToString());
                                //switch (pVal.ColUID)
                                //{
                                //    case "ID":
                                //        string id = Convert.ToString(dtMat.GetValue("id", pVal.Row - 1));
                                //        getRecord(id.ToString());
                                //        break;
                                //}
                            }
                            catch
                            {

                                // iniSalaryDetail();
                            }
                        }
                        break;
                    


            }
        }
        
        private void getFileName()
        {
            string fileName = Program.objHrmsUI.FindFile();
            txFilenam.Value = fileName;

        }

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            _iniControls();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;

        }
        
        private void _iniControls()
        {
            ItxCode.Enabled = true;
            txCode.Value = "";
            txName.Value = "";
            txFilenam.Value = "";
            txMenu.Value = "";
            txCode.Active = true;
        }
        
        private void pullDepartmentsFromFile()
        {

        }
        
        private void SubmitForm()
        {
            if (validateform())
            {
                TblRpts rpt;
                int cnt = (from p in dbHrPayroll.TblRpts where p.RptCode == txCode.Value.Trim() select p).Count();
                if (cnt > 0)
                {
                    rpt = (from p in dbHrPayroll.TblRpts where p.RptCode == txCode.Value.Trim() select p).Single();
                }
                else
                {
                    rpt = new TblRpts();
                    dbHrPayroll.TblRpts.InsertOnSubmit(rpt);
                }
                rpt.RptCode = txCode.Value.Trim();

                rpt.ReportName = txName.Value.Trim();
                rpt.ReportIn = txMenu.Value.Trim();
                if (!String.IsNullOrEmpty(txFilenam.Value))
                {
                    rpt.RptFileStr = GetBytesFromFile(txFilenam.Value.Trim());
                }
                if (txMenu.Value.Trim() != "System")
                {
                rpt.FlgEmployee = chkEmployee.Checked;
                rpt.FlgDept = chkDepartment.Checked;
                rpt.FlgLocation = chkLocation.Checked;
                rpt.FlgDateFrom = chkDateFrom.Checked;
                rpt.FlgDateTo = chkDateTo.Checked;
                rpt.FlgPreviousPeriod = chkPreviousPeriod.Checked;
                rpt.FlgPeriod = chkPeriod.Checked;
                rpt.FlgCritaria = chkCritaria.Checked;
                }
                if (!String.IsNullOrEmpty(txMenu.Value))
                {
                    if (txMenu.Value.Trim() == "System")
                    {
                        rpt.FlgSystem = true;
                        rpt.FlgPeriod = chkPeriod.Checked;
                    }
                    else
                    {
                        rpt.FlgSystem = false;
                    }
                }
                dbHrPayroll.SubmitChanges();
                getData();
                if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                {
                    AddNewRecord();
                }

            }
        }
        
        private bool validateform()
        {
            bool outResult = true;

            return outResult;

        }
        
        public static byte[] GetBytesFromFile(string fullFilePath)
        {
            // this method is limited to 2^32 byte files (4.2 GB)

            FileStream fs = File.OpenRead(fullFilePath);
           
            try
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                fs.Close();
                return bytes;
            }
            finally
            {
                fs.Close();
            }

        }
       
       
    }

}
