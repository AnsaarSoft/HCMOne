using System;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;
using DIHRMS.Custom;


namespace ACHR.Screen
{
    partial class frm_AccrView : HRMSBaseForm
    {
        public override void etAfterCfl(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterCfl(ref pVal, ref BubbleEvent);
            SAPbouiCOM.ChooseFromListEvent ocfl = (SAPbouiCOM.ChooseFromListEvent)pVal;
            string itemId = pVal.ItemUID;
            SAPbouiCOM.Item cflItem = oForm.Items.Item(itemId);
            SAPbouiCOM.DataTable oDT = ocfl.SelectedObjects;
            if (oDT != null)
            {
                if (itemId == "txEmpId")
                {

                    oForm.DataSources.UserDataSources.Item("txEmpId").ValueEx = Convert.ToString(oDT.GetValue("empID", 0));
                    oForm.DataSources.UserDataSources.Item("txEmpName").ValueEx = Convert.ToString(oDT.GetValue("firstName", 0));
                    oForm.DataSources.UserDataSources.Item("txHrmsId").ValueEx = Convert.ToString(oDT.GetValue("U_HrmsEmpId", 0));
                    getEmpAccrual();
                }
            }
        }
        //public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        //{
        //    base.etAfterClick(ref pVal, ref BubbleEvent);
        //    try
        //    {
        //        switch (pVal.ItemUID)
        //        {
        //            case "btId":
        //                OpenNewSearchForm();
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    catch (Exception Ex)
        //    {
        //        oApplication.StatusBar.SetText("Form: frm_AccrView Function: etAfterClick Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
        //    }
        //}
        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            InitiallizeForm();
            IniContrls();
            fillLeaveType();

        }
        private void IniContrls()
        {

            oForm.DataSources.UserDataSources.Item("txAccrDate").ValueEx = DateTime.Now.ToString("yyyyMMdd");

        }
        private void fillLeaveType()
        {
            IEnumerable<MstLeaveType> lts = from p in dbHrPayroll.MstLeaveType select p;
            int i = 0;
            foreach (MstLeaveType lt in lts)
            {
                cbLType.ValidValues.Add(lt.ID.ToString(), lt.Description);
                i++;
            }
            cbLType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
           

        }
        private void getEmpAccrual()
        {
            dtAccrual.Rows.Clear();
            if (txHrmsId.Value.ToString() != "")
            {
                int cnt = (from p in dbHrPayroll.MstEmployeeLeaves where p.MstEmployee.EmpID.ToString() == txHrmsId.Value.ToString() && p.LeaveType.ToString() == cbLType.Value.ToString() && Convert.ToBoolean(p.FlgActive) select p).Count();

                if (cnt > 0)
                {
                    dtAccrual.Rows.Add(1);
                    MstEmployeeLeaves empleaves = (from p in dbHrPayroll.MstEmployeeLeaves where p.MstEmployee.EmpID.ToString() == txHrmsId.Value.ToString() && p.LeaveType.ToString() == cbLType.Value.ToString() select p).Single();
                    dtAccrual.SetValue("CarryFwd", 0, empleaves.LeavesCarryForward.ToString());
                    dtAccrual.SetValue("Allowed", 0, empleaves.LeavesEntitled.ToString());
                    dtAccrual.SetValue("Consumed", 0, empleaves.LeavesUsed.ToString());
                    dtAccrual.SetValue("Permonth", 0, empleaves.LeavesEntitled == null ? "0" : Convert.ToString(Math.Round(Convert.ToDouble(empleaves.LeavesEntitled) / 12, 2)));

                    DateTime dtFrom = Convert.ToDateTime(empleaves.FromDt);
                    DateTime dtTo = Convert.ToDateTime(empleaves.ToDt);
                    string strValue = oForm.DataSources.UserDataSources.Item("txAccrDate").ValueEx;
                    DateTime dt = DateTime.ParseExact(strValue, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    int totalDays = (dtTo - dtFrom).Days;
                    int daysTillAccrDate = (dt - dtFrom).Days;
                    double daysEntitled = Convert.ToDouble(empleaves.LeavesEntitled);
                    double accrDays = 0.00;

                    if (totalDays > 0)
                    {
                        accrDays = daysEntitled * daysTillAccrDate / totalDays;
                    }
                    dtAccrual.SetValue("atAccrDate", 0, Math.Round( accrDays ,2).ToString());
                    dtAccrual.SetValue("Balance", 0, Math.Round(accrDays - Convert.ToDouble( empleaves.LeavesUsed) , 2).ToString());

                }
                else
                {
                }
                mtAccrual.LoadFromDataSource();

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
                    oApplication.Forms.Item("frm_" + comName).Select();
                }
                catch
                {
                    //this.oForm.Visible = false;
                    Type oFormType = Type.GetType("ACHR.Screen." + "frm_" + comName);
                    Screen.HRMSBaseForm objScr = ((Screen.HRMSBaseForm)oFormType.GetConstructor(System.Type.EmptyTypes).Invoke(null));
                    objScr.CreateForm(oApplication, "ACHR.XMLScreen." + strLang + ".xml_" + comName + ".xml", oCompany, "frm_" + comName);
                    //this.oForm.Visible = true;

                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        private void SetEmpValues()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.EmpID))
                {
                    //txEmpId.Value = Program.EmpID;
                    //txHrmsId.Value = Program.EmpID;
                    var EmpRecord = dbHrPayroll.MstEmployee.Where(e => e.EmpID == Program.EmpID).ToList();
                    if (EmpRecord != null && EmpRecord.Count > 0)
                    {
                        txHrmsId.Value = EmpRecord.FirstOrDefault().EmpID;
                        txEmpName.Value = EmpRecord.FirstOrDefault().FirstName + " " + EmpRecord.FirstOrDefault().MiddleName + " " + EmpRecord.FirstOrDefault().LastName;
                        getEmpAccrual();
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_AccrView Function: SetEmpValues Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            SetEmpValues();
        }
    }
}
