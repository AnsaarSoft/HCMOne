using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;


namespace ACHR.Screen
{
    class frm_Calander:HRMSBaseForm
    {

        #region Variable

        SAPbouiCOM.Matrix mtCal;
        SAPbouiCOM.Column id, isNew, Code, Descr, StartDt, EndDt, Active;

        private SAPbouiCOM.DataTable dtCalander;
        public IEnumerable<MstCalendar> calanders;

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
                case "btn_cal":
                    //updateDbWithMat();
                    //OpenNewSearchForm();
                    break;
                case "4":
                    //VarifyPeriodDates();
                    VerifyPeriodDatesActiveCalendar();
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
            dtCalander = oForm.DataSources.DataTables.Item("dtCalander");
            dtCalander.Rows.Clear();
            fillMat();


            oForm.Freeze(false);

        }
        
        private void fillMat()
        {
            dtCalander.Rows.Clear();
            calanders = from p in dbHrPayroll.MstCalendar select p;
            dtCalander.Rows.Clear();
            dtCalander.Rows.Add(calanders.Count());
            int i = 0;
            foreach (MstCalendar cal in calanders)
            {
                dtCalander.SetValue("isNew", i, "N");
                dtCalander.SetValue("id", i, cal.Id);
                dtCalander.SetValue("Code", i, cal.Code.ToString());
                dtCalander.SetValue("Descr", i, cal.Description);
                dtCalander.SetValue("StartDt", i, Convert.ToDateTime(cal.StartDate).ToString("yyyyMMdd"));
                dtCalander.SetValue("EndDt", i, Convert.ToDateTime(cal.EndDate).ToString("yyyyMMdd"));
                dtCalander.SetValue("Active", i, cal.FlgActive == true ? "Y" : "N");

                i++;

            }
            if (i > 0) StartDt.Editable = false; else StartDt.Editable = true;
            EndDt.Editable = true;
            
            addEmptyRow();
            mtCal.LoadFromDataSource();

        }
        
        private void addEmptyRow()
        {


            if (dtCalander.Rows.Count == 0)
            {
                dtCalander.Rows.Add(1);

                dtCalander.SetValue("isNew", 0, "Y");
                dtCalander.SetValue("id", 0, 0);
                dtCalander.SetValue("Code", 0, "");
                dtCalander.SetValue("Descr", 0, "");
                dtCalander.SetValue("StartDt", 0, "");
                dtCalander.SetValue("EndDt", 0, "");
                dtCalander.SetValue("Active", 0, "N");



                mtCal.AddRow(1, mtCal.RowCount + 1);
            }
            else
            {
                if (dtCalander.GetValue("Code", dtCalander.Rows.Count - 1) == "")
                {
                }
                else
                {
                    DateTime strDt = Convert.ToDateTime(dtCalander.GetValue("EndDt", dtCalander.Rows.Count - 1));
                    dtCalander.Rows.Add(1);
                    dtCalander.SetValue("isNew", dtCalander.Rows.Count - 1, "Y");
                    dtCalander.SetValue("id", dtCalander.Rows.Count - 1, 0);
                    dtCalander.SetValue("Code", dtCalander.Rows.Count - 1, "");
                    dtCalander.SetValue("Descr", dtCalander.Rows.Count - 1, "");
                    dtCalander.SetValue("StartDt", dtCalander.Rows.Count - 1, strDt.AddDays(1).ToString("yyyyMMdd"));
                    dtCalander.SetValue("EndDt", dtCalander.Rows.Count - 1, "");

                    dtCalander.SetValue("Active", dtCalander.Rows.Count - 1, "N");
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
                for (int i = 0; i < dtCalander.Rows.Count; i++)
                {
                    id = ""; code = ""; desc = ""; isnew = ""; startdt = "";
                    enddt = ""; active = "";

                    id = Convert.ToString(dtCalander.GetValue("id", i));
                    code = Convert.ToString(dtCalander.GetValue("Code", i));
                    desc = Convert.ToString(dtCalander.GetValue("Descr", i));
                    isnew = Convert.ToString(dtCalander.GetValue("isNew", i));
                    startdt = Convert.ToString(dtCalander.GetValue("StartDt", i));
                    enddt = Convert.ToString(dtCalander.GetValue("EndDt", i));
                    active = Convert.ToString(dtCalander.GetValue("Active", i));

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
                    if (!string.IsNullOrEmpty(startdt) && !string.IsNullOrEmpty(enddt))
                    {
                        DateTime FromDate, ToDate;
                        //FromDate = DateTime.ParseExact(startdt, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        //ToDate = DateTime.ParseExact(enddt, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                        FromDate = Convert.ToDateTime(startdt);
                        ToDate = Convert.ToDateTime(enddt);
                        if (FromDate >= ToDate)
                        {
                            oApplication.StatusBar.SetText("Fiscal Start date can't be greater then End Date", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        double oFY = (ToDate - FromDate).TotalDays + 1;
                        if (oFY > 367)
                        {
                            oApplication.StatusBar.SetText("Fiscal Year must have 12 Periods, Adjust End Date to achive desired result.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                    }
                    if (!string.IsNullOrEmpty(active))
                    {
                        if (active.ToLower() == "y")
                        {
                            CountActive++;
                        }
                    }
                    var oCal = (from a in dbHrPayroll.MstCalendar where a.Code == code select a).FirstOrDefault();
                }
                if (oCode.Count != oCode.Distinct().Count())
                {
                    oApplication.StatusBar.SetText("Code duplication not allowed.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                if (CountActive > 1)
                {
                    oApplication.StatusBar.SetText("Only one fiscal year can be active at a time.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                    return false;
                }
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        
        private void VarifyPeriodDates()
        {
            string id = "";
            string code = "";
            string isnew = "";
            for (int i = 0; i < dtCalander.Rows.Count; i++)
            {
                code = Convert.ToString(dtCalander.GetValue("Code", i));
                isnew = Convert.ToString(dtCalander.GetValue("isNew", i));
                string strStDate = ""; //dtCalander.GetValue("StartDt", i).ToString();
                string strEndDate = "";  //dtCalander.GetValue("EndDt", i).ToString();
                try
                {
                    strStDate = dtCalander.GetValue("StartDt", i).ToString();
                    strEndDate = dtCalander.GetValue("EndDt", i).ToString();
                }
                catch
                {

                }
                isnew = isnew.Trim();
                code = code.Trim();
                try
                {
                    if (code != "")
                    {
                        id = Convert.ToString(dtCalander.GetValue("id", i));
                        if (isnew == "N" && code != "" && strEndDate != "" && dtCalander.GetValue("Descr", i) != "")
                        {
                            ds.AddPeriodDates(Convert.ToDateTime(dtCalander.GetValue("StartDt", i)), Convert.ToDateTime(dtCalander.GetValue("EndDt", i)), dtCalander.GetValue("Code", i));
                        }

                    }
                    oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("Gen_ChangeSuccess"), SAPbouiCOM.BoMessageTime.bmt_Short, false);
                }
                catch (Exception ex)
                {
                    oApplication.SetStatusBarMessage("Verification Failed Error : " +ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, false);
                }

            }
        }

        private void VerifyPeriodDatesActiveCalendar()
        {
            try
            {
                string id = "", code = "", cActive = "";
                for (int i = 0; i < dtCalander.Rows.Count; i++)
                {
                    code = Convert.ToString(dtCalander.GetValue("Code", i));
                    cActive = Convert.ToString(dtCalander.GetValue(Active.DataBind.Alias, i));
                    string strStDate = ""; //dtCalander.GetValue("StartDt", i).ToString();
                    string strEndDate = "";  //dtCalander.GetValue("EndDt", i).ToString();
                    try
                    {
                        strStDate = dtCalander.GetValue("StartDt", i).ToString();
                        strEndDate = dtCalander.GetValue("EndDt", i).ToString();
                    }
                    catch
                    {

                    }
                    cActive = cActive.Trim();
                    code = code.Trim();
                    try
                    {
                        if (code != "")
                        {
                            if (cActive == "Y" && code != "" && strEndDate != "" && dtCalander.GetValue("Descr", i) != "")
                            {
                                ds.AddPeriodDates(Convert.ToDateTime(dtCalander.GetValue("StartDt", i)), Convert.ToDateTime(dtCalander.GetValue("EndDt", i)), dtCalander.GetValue("Code", i));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MsgWarning("Unable to verify periods.");
                        logger(ex);
                    }
                }
            }
            catch(Exception ex)
            {
                logger(ex);
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
                string isnew = "";
                for (int i = 0; i < dtCalander.Rows.Count; i++)
                {
                    code = Convert.ToString(dtCalander.GetValue("Code", i));
                    isnew = Convert.ToString(dtCalander.GetValue("isNew", i));
                    string strStDate = ""; //dtCalander.GetValue("StartDt", i).ToString();
                    string strEndDate = "";  //dtCalander.GetValue("EndDt", i).ToString();
                    try
                    {
                        strStDate = dtCalander.GetValue("StartDt", i).ToString();
                        strEndDate = dtCalander.GetValue("EndDt", i).ToString();
                    }
                    catch (Exception ex)
                    {

                    }
                    isnew = isnew.Trim();
                    code = code.Trim();
                    if (code != "")
                    {
                        MstCalendar objUp;
                        id = Convert.ToString(dtCalander.GetValue("id", i));
                        if (isnew == "Y" && code != "" && strEndDate != "" && dtCalander.GetValue("Descr", i) != "")
                        {
                            objUp = new MstCalendar();
                            dbHrPayroll.MstCalendar.InsertOnSubmit(objUp);
                            objUp.Code = dtCalander.GetValue("Code", i);
                            objUp.Description = dtCalander.GetValue("Descr", i);
                            objUp.StartDate = Convert.ToDateTime(dtCalander.GetValue("StartDt", i));// DateTime.ParseExact(dtCalander.GetValue("StartDt", i), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            objUp.EndDate = Convert.ToDateTime(dtCalander.GetValue("EndDt", i));// DateTime.ParseExact(dtCalander.GetValue("EndDt", i), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                            objUp.FlgActive = Convert.ToString(dtCalander.GetValue("Active", i)) == "Y" ? true : false;
                            objUp.CreateDate = DateTime.Now;
                            objUp.UserId = oCompany.UserName; //to be changed;

                            dbHrPayroll.SubmitChanges();
                            //fillMat();
                            //ds.AddPeriodDates(Convert.ToDateTime(dtCalander.GetValue("StartDt", i)), Convert.ToDateTime(dtCalander.GetValue("EndDt", i)), dtCalander.GetValue("Code", i));
                            return;
                        }
                        else
                        {
                            if (isnew == "N")
                            {
                                objUp = (from a in dbHrPayroll.MstCalendar where a.Id.ToString() == id select a).FirstOrDefault();
                                objUp.FlgActive = Convert.ToString(dtCalander.GetValue("Active", i)) == "Y" ? true : false;
                                objUp.UpdateDate = DateTime.Now;
                                objUp.UpdatedBy = oCompany.UserName;

                                dbHrPayroll.SubmitChanges();
                                //fillMat();
                                //oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("Err_Required"));
                                oApplication.StatusBar.SetText("Successfully Updated Records.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
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
