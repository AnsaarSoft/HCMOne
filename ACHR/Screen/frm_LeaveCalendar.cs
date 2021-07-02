using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;


namespace ACHR.Screen
{
    class frm_LeaveCalendar : HRMSBaseForm
    {

        #region Variable

        SAPbouiCOM.Matrix mtCal;
        SAPbouiCOM.Column id, isNew, Code, Descr, StartDt, EndDt, Active;

        private SAPbouiCOM.DataTable dtLeaveCalendar;
        public IEnumerable<MstLeaveCalendar> LeaveCalendar;

        #endregion 

        #region SAP B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);

            InitiallizeForm();
            oForm.Freeze(false);

        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    AddUpdateRecord();
                    break;
                
            }
        }

        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    if (!ValidateRecord())
                    {
                        BubbleEvent = false;
                    }
                    break;
            }
        }

        #endregion 

        #region Functions

        private void InitiallizeForm()
        {
            oForm.Freeze(true);

            //EachItemshould be initiallized in this sequence

            /*
             * Add datasource for the item
             * Initiallize the controll object with same ID
             * Initiallize the Item Object with same ID
             * Data Bind the controll with data source
             * 
             * */

            mtCal = oForm.Items.Item("mtCal").Specific;
            isNew = mtCal.Columns.Item("isNew");
            id = mtCal.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;

            Code = mtCal.Columns.Item("Code");
            Descr = mtCal.Columns.Item("Descr");
            StartDt = mtCal.Columns.Item("StartDt");
            EndDt = mtCal.Columns.Item("EndDt");
            Active = mtCal.Columns.Item("Active");
            dtLeaveCalendar = oForm.DataSources.DataTables.Item("dtCalander");
            dtLeaveCalendar.Rows.Clear();
            fillMat();


            oForm.Freeze(false);

        }
        
        private void fillMat()
        {
            dtLeaveCalendar.Rows.Clear();
            LeaveCalendar = from p in dbHrPayroll.MstLeaveCalendar select p;
            dtLeaveCalendar.Rows.Clear();
            dtLeaveCalendar.Rows.Add(LeaveCalendar.Count());
            int i = 0;
            foreach (MstLeaveCalendar cal in LeaveCalendar)
            {
                dtLeaveCalendar.SetValue("isNew", i, "N");
                dtLeaveCalendar.SetValue("id", i, cal.Id);
                dtLeaveCalendar.SetValue("Code", i, cal.Code.ToString());
                dtLeaveCalendar.SetValue("Descr", i, cal.Description);
                dtLeaveCalendar.SetValue("StartDt", i, Convert.ToDateTime(cal.StartDate).ToString("yyyyMMdd"));
                dtLeaveCalendar.SetValue("EndDt", i, Convert.ToDateTime(cal.EndDate).ToString("yyyyMMdd"));
                dtLeaveCalendar.SetValue("Active", i, cal.FlgActive == true ? "Y" : "N");

                i++;

            }
            if (i > 0) StartDt.Editable = false; else StartDt.Editable = true;
            EndDt.Editable = true;
            //StartDt.Editable = true;
            addEmptyRow();
            mtCal.LoadFromDataSource();

        }
        
        private void addEmptyRow()
        {


            if (dtLeaveCalendar.Rows.Count == 0)
            {
                dtLeaveCalendar.Rows.Add(1);

                dtLeaveCalendar.SetValue("isNew", 0, "Y");
                dtLeaveCalendar.SetValue("id", 0, 0);
                dtLeaveCalendar.SetValue("Code", 0, "");
                dtLeaveCalendar.SetValue("Descr", 0, "");
                dtLeaveCalendar.SetValue("StartDt", 0, "");
                dtLeaveCalendar.SetValue("EndDt", 0, "");
                dtLeaveCalendar.SetValue("Active", 0, "N");



                mtCal.AddRow(1, mtCal.RowCount + 1);
            }
            else
            {
                if (dtLeaveCalendar.GetValue("Code", dtLeaveCalendar.Rows.Count - 1) == "")
                {
                }
                else
                {
                    DateTime strDt = Convert.ToDateTime(dtLeaveCalendar.GetValue("EndDt", dtLeaveCalendar.Rows.Count - 1));
                    dtLeaveCalendar.Rows.Add(1);
                    dtLeaveCalendar.SetValue("isNew", dtLeaveCalendar.Rows.Count - 1, "Y");
                    dtLeaveCalendar.SetValue("id", dtLeaveCalendar.Rows.Count - 1, 0);
                    dtLeaveCalendar.SetValue("Code", dtLeaveCalendar.Rows.Count - 1, "");
                    dtLeaveCalendar.SetValue("Descr", dtLeaveCalendar.Rows.Count - 1, "");
                    dtLeaveCalendar.SetValue("StartDt", dtLeaveCalendar.Rows.Count - 1, strDt.AddDays(1).ToString("yyyyMMdd"));
                    dtLeaveCalendar.SetValue("EndDt", dtLeaveCalendar.Rows.Count - 1, "");

                    dtLeaveCalendar.SetValue("Active", dtLeaveCalendar.Rows.Count - 1, "N");
                    mtCal.AddRow(1, mtCal.RowCount + 1);
                }

            }
            // mtAdv.FlushToDataSource();

        }

        private Boolean ValidateRecord()
        {
            try
            {
                string id, code, desc, isnew,active, startdt, enddt;
                List<string> oCode = new List<string>();
                int CountActive = 0;
                mtCal.FlushToDataSource();
                for (int i = 0; i < dtLeaveCalendar.Rows.Count; i++)
                {
                    id = ""; code = ""; desc = ""; isnew = ""; startdt = "";
                    enddt = ""; active = "";

                    id = Convert.ToString(dtLeaveCalendar.GetValue("id", i));
                    code = Convert.ToString(dtLeaveCalendar.GetValue("Code", i));
                    desc = Convert.ToString(dtLeaveCalendar.GetValue("Descr", i));
                    isnew = Convert.ToString(dtLeaveCalendar.GetValue("isNew", i));
                    startdt = Convert.ToString(dtLeaveCalendar.GetValue("StartDt", i));
                    enddt = Convert.ToString(dtLeaveCalendar.GetValue("EndDt", i));
                    active = Convert.ToString(dtLeaveCalendar.GetValue("Active", i));

                    if (string.IsNullOrEmpty(code) && isnew == "Y" && !string.IsNullOrEmpty(desc))
                    {
                        oApplication.StatusBar.SetText("Code field is madatory. @ Line : " + (i + 1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(code))
                        {
                            oCode.Add(code);
                        }
                    }
                    if (string.IsNullOrEmpty(desc) && isnew == "Y" && !string.IsNullOrEmpty(code))
                    {
                        oApplication.StatusBar.SetText("Description field is madatory. @ Line : " + (i + 1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }

                    if (!string.IsNullOrEmpty(active))
                    {
                        if (active.ToLower() == "y")
                        {
                            CountActive++;
                        }
                    }

                    if (CountActive > 1)
                    {
                        oApplication.StatusBar.SetText("Only one Leave year can be active at a time.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                        return false;
                    }
                    if (!string.IsNullOrEmpty(startdt) && !string.IsNullOrEmpty(enddt) && active.ToLower() == "y")
                    {
                        DateTime FromDate, ToDate;                       
                        FromDate = Convert.ToDateTime(startdt);
                        ToDate = Convert.ToDateTime(enddt);
                        int monthCount = (ToDate.Year - FromDate.Year) * 12 + ToDate.Month - FromDate.Month + 1;
                        double TotalYearDays = (ToDate - FromDate).TotalDays + 1;
                        var CurrentCalendar = (from a in dbHrPayroll.MstCalendar where a.FlgActive == true select a).FirstOrDefault();                        

                        string sdt = CurrentCalendar.StartDate.Value.Date.ToString("dd");
                        string edt = FromDate.ToString("dd");
                        
                        if (sdt != edt)
                        {
                            oApplication.StatusBar.SetText("Leave Calendar Start date can't be greater or Less then Fiscal Year start date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        if (FromDate >= ToDate)
                        {
                            oApplication.StatusBar.SetText("Leave Start date can't be greater then End Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }

                        if (TotalYearDays > 367 || TotalYearDays < 367 && TotalYearDays > 366 || TotalYearDays < 366 && TotalYearDays > 365 || TotalYearDays < 365)
                        {
                            oApplication.StatusBar.SetText("Leave Year must have 12 Months, Adjust End Date to achive desired result.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        
                        var oCal = (from a in dbHrPayroll.MstLeaveCalendar where a.Code == code select a).FirstOrDefault();
                        if (oCode.Count != oCode.Distinct().Count())
                        {
                            oApplication.StatusBar.SetText("Code duplication not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                    }
                    
                }
                //if (oCode.Count != oCode.Distinct().Count())
                //{
                //    oApplication.StatusBar.SetText("Code duplication not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                //    return false;
                //}
                //if (CountActive > 1)
                //{
                //    oApplication.StatusBar.SetText("Only one Leave year can be active at a time.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                //    return false;
                //}
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }       
        
        
        private void OpenNewSearchForm()
        {
            try
            {
                Program.EmpID = "";
                string comName = "Search";
                string strLang = "ln_English";
                try
                {                   
                    //oApplication.Forms.Item("60506").Select();
                    oApplication.OpenForm(SAPbouiCOM.BoFormObjectEnum.fo_UserDefinedObject, "60506", "ObjectKey");
                }
                catch
                {
                    //this.oForm.Visible = false;
                    Type oFormType = Type.GetType("ACHR.Screen." + "60506");
                    Screen.HRMSBaseForm objScr = ((Screen.HRMSBaseForm)oFormType.GetConstructor(System.Type.EmptyTypes).Invoke(null));
                    objScr.CreateForm(oApplication, "ACHR.XMLScreen." + strLang + ".xml_" + comName + ".xml", oCompany, "60506");
                    //this.oForm.Visible = true;

                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void AddUpdateRecord()
        {
            try
            {
                mtCal.FlushToDataSource();
                string id = "";
                string code = "";
                string isnew,active = "";                
                for (int i = 0; i < dtLeaveCalendar.Rows.Count; i++)
                {
                    code = Convert.ToString(dtLeaveCalendar.GetValue("Code", i));
                    isnew = Convert.ToString(dtLeaveCalendar.GetValue("isNew", i));
                    active = Convert.ToString(dtLeaveCalendar.GetValue("Active", i));
                    string strStDate = ""; //dtCalander.GetValue("StartDt", i).ToString();
                    string strEndDate = "";  //dtCalander.GetValue("EndDt", i).ToString();
                    try
                    {
                        strStDate = dtLeaveCalendar.GetValue("StartDt", i).ToString();
                        strEndDate = dtLeaveCalendar.GetValue("EndDt", i).ToString();
                    }
                    catch (Exception ex)
                    {

                    }
                    isnew = isnew.Trim();
                    code = code.Trim();
                    if (active.ToLower() == "y")
                    {
                        if (code != "")
                        {
                            MstLeaveCalendar objUp;
                            id = Convert.ToString(dtLeaveCalendar.GetValue("id", i));
                            if (isnew == "Y" && code != "" && strEndDate != "" && dtLeaveCalendar.GetValue("Descr", i) != "")
                            {
                                objUp = new MstLeaveCalendar();
                                dbHrPayroll.MstLeaveCalendar.InsertOnSubmit(objUp);
                                objUp.Code = dtLeaveCalendar.GetValue("Code", i);
                                objUp.Description = dtLeaveCalendar.GetValue("Descr", i);
                                objUp.StartDate = Convert.ToDateTime(dtLeaveCalendar.GetValue("StartDt", i));// DateTime.ParseExact(dtCalander.GetValue("StartDt", i), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                objUp.EndDate = Convert.ToDateTime(dtLeaveCalendar.GetValue("EndDt", i));// DateTime.ParseExact(dtCalander.GetValue("EndDt", i), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                objUp.FlgActive = Convert.ToString(dtLeaveCalendar.GetValue("Active", i)) == "Y" ? true : false;
                                objUp.CreateDate = DateTime.Now;
                                objUp.UserId = oCompany.UserName; //to be changed;

                                dbHrPayroll.SubmitChanges();
                                ds.addPeriodDates(Convert.ToDateTime(dtLeaveCalendar.GetValue("StartDt", i)), Convert.ToDateTime(dtLeaveCalendar.GetValue("EndDt", i)), dtLeaveCalendar.GetValue("Code", i));
                                return;
                            }
                            else
                            {
                                if (isnew == "N")
                                {
                                    
                                        objUp = (from a in dbHrPayroll.MstLeaveCalendar where a.Id.ToString() == id select a).FirstOrDefault();
                                        objUp.FlgActive = Convert.ToString(dtLeaveCalendar.GetValue("Active", i)) == "Y" ? true : false;
                                        objUp.StartDate = Convert.ToDateTime(dtLeaveCalendar.GetValue("StartDt", i));// DateTime.ParseExact(dtCalander.GetValue("StartDt", i), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                        objUp.EndDate = Convert.ToDateTime(dtLeaveCalendar.GetValue("EndDt", i));// DateTime.ParseExact(dtCalander.GetValue("EndDt", i), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                                        objUp.UpdateDate = DateTime.Now;
                                        objUp.UpdatedBy = oCompany.UserName;

                                        dbHrPayroll.SubmitChanges();
                                        oApplication.StatusBar.SetText("Successfully Updated Records.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                                   
                                }
                            }
                        }
                    }
                    
                }
                fillMat();
            }
            catch (Exception ex)
            {
            }
        }

        #endregion
    }
}
