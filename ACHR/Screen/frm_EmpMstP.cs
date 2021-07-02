using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_EmpMstP:HRMSBaseForm
    {
        #region Variable

        SAPbouiCOM.EditText txtEmployeeiD;
        SAPbouiCOM.Matrix mtPastExperiance;
        SAPbouiCOM.DataTable dtPastExperiance;
        SAPbouiCOM.Column pIsNew, pId, pCompany, pFromdt, pTodt, pPosition;
        SAPbouiCOM.Column pDuties, pNotes, pLastSalary;
        SAPbouiCOM.Button btnMain;
        SAPbouiCOM.Item ibtnMain;

        #endregion

        #region Sap B1 Event
        
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
                case "btmain":
                    if (btnMain.Caption == "Update")
                    {
                        SubmitDocument();
                    }
                    if (btnMain.Caption == "Ok")
                    {
                        oForm.Close();
                    }
                    break;
            }
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            if (pVal.ItemUID == "mtpstexp" && pVal.ColUID == "lsalary")
            {
                oForm.Freeze(true);
                mtPastExperiance.FlushToDataSource();
                AddEmptyRowPastExperiance();
                oForm.Freeze(false);
            }
            if (pVal.ItemUID == "mtpstexp" && pVal.ColUID == "company")
            {
                oForm.Freeze(true);
                btnMain.Caption = "Update";
                oForm.Freeze(false);
            }
        }

        #endregion

        #region Functions

        private void InitiallizeForm()
        {
            try
            {
                txtEmployeeiD = oForm.Items.Item("txEmpID").Specific;
                oForm.DataSources.UserDataSources.Add("txEmpID", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50);
                txtEmployeeiD.DataBind.SetBound(true, "", "txEmpID");

                btnMain = oForm.Items.Item("btmain").Specific;
                ibtnMain = oForm.Items.Item("btmain");
                
                mtPastExperiance = oForm.Items.Item("mtpstexp").Specific;
                dtPastExperiance = oForm.DataSources.DataTables.Item("dtpstexp");
                pId = mtPastExperiance.Columns.Item("id");
                pId.Visible = false;
                pIsNew = mtPastExperiance.Columns.Item("isnew");
                pIsNew.Visible = false;
                pCompany = mtPastExperiance.Columns.Item("company");
                pFromdt = mtPastExperiance.Columns.Item("fromdt");
                pTodt = mtPastExperiance.Columns.Item("todt");
                pPosition = mtPastExperiance.Columns.Item("position");
                pDuties = mtPastExperiance.Columns.Item("duties");
                pNotes = mtPastExperiance.Columns.Item("note");
                pLastSalary = mtPastExperiance.Columns.Item("lsalary");

                FillDocument();
                btnMain.Caption = "Update";
                txtEmployeeiD.Value = Program.ExtendendEmpID;

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("InitiallizeForm Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillDocument()
        {
            try
            {
                MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee
                                    where a.EmpID == Program.ExtendendEmpID
                                    select a).FirstOrDefault();
                dtPastExperiance.Rows.Clear();
                foreach (MstEmployeeExperience One in oEmp.MstEmployeeExperience)
                {
                    dtPastExperiance.Rows.Add(1);
                    dtPastExperiance.SetValue(pIsNew.DataBind.Alias, dtPastExperiance.Rows.Count - 1, "N");
                    dtPastExperiance.SetValue(pId.DataBind.Alias, dtPastExperiance.Rows.Count - 1, One.Id);
                    dtPastExperiance.SetValue(pCompany.DataBind.Alias, dtPastExperiance.Rows.Count - 1, One.CompanyName);
                    dtPastExperiance.SetValue(pFromdt.DataBind.Alias, dtPastExperiance.Rows.Count - 1, One.FromDate);
                    dtPastExperiance.SetValue(pTodt.DataBind.Alias, dtPastExperiance.Rows.Count - 1, One.ToDate);
                    dtPastExperiance.SetValue(pPosition.DataBind.Alias, dtPastExperiance.Rows.Count - 1, One.Position);
                    dtPastExperiance.SetValue(pDuties.DataBind.Alias, dtPastExperiance.Rows.Count - 1, One.Duties);
                    dtPastExperiance.SetValue(pNotes.DataBind.Alias, dtPastExperiance.Rows.Count - 1, One.Notes);
                    dtPastExperiance.SetValue(pLastSalary.DataBind.Alias, dtPastExperiance.Rows.Count - 1, One.LastSalary);
                }
                mtPastExperiance.LoadFromDataSource();
                AddEmptyRowPastExperiance();

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillDocument Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SubmitDocument()
        {
            try
            {
                MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmployeeiD.Value.Trim() select a).FirstOrDefault();
                mtPastExperiance.FlushToDataSource();
                if (dtPastExperiance.Rows.Count > 0)
                {
                    for (int i = 0; i < dtPastExperiance.Rows.Count; i++)
                    {
                        int pid;
                        string companay, position, duties, notes, lastsalary, pisnew;
                        DateTime dtfrom, dtto;
                        pid = Convert.ToInt32(dtPastExperiance.GetValue(pId.DataBind.Alias, i));
                        pisnew = dtPastExperiance.GetValue(pIsNew.DataBind.Alias, i);
                        companay = dtPastExperiance.GetValue(pCompany.DataBind.Alias, i);
                        if (pisnew == "Y" && !string.IsNullOrEmpty(companay))
                        {
                            if (string.IsNullOrEmpty(companay)) continue;
                            position = dtPastExperiance.GetValue(pPosition.DataBind.Alias, i);
                            duties = dtPastExperiance.GetValue(pDuties.DataBind.Alias, i);
                            notes = dtPastExperiance.GetValue(pNotes.DataBind.Alias, i);
                            lastsalary = dtPastExperiance.GetValue(pLastSalary.DataBind.Alias, i);
                            dtfrom = Convert.ToDateTime(dtPastExperiance.GetValue(pFromdt.DataBind.Alias, i));
                            dtto = Convert.ToDateTime(dtPastExperiance.GetValue(pTodt.DataBind.Alias, i));
                            MstEmployeeExperience oNew = new MstEmployeeExperience();
                            oNew.CompanyName = companay;
                            oNew.Position = position;
                            oNew.Duties = duties;
                            oNew.Notes = notes;
                            oNew.LastSalary = lastsalary;
                            oNew.FromDate = dtfrom;
                            oNew.ToDate = dtto;
                            oEmp.MstEmployeeExperience.Add(oNew);
                        }
                        else if (pisnew == "N")
                        {
                            position = dtPastExperiance.GetValue(pPosition.DataBind.Alias, i);
                            duties = dtPastExperiance.GetValue(pDuties.DataBind.Alias, i);
                            notes = dtPastExperiance.GetValue(pNotes.DataBind.Alias, i);
                            lastsalary = dtPastExperiance.GetValue(pLastSalary.DataBind.Alias, i);
                            dtfrom = Convert.ToDateTime(dtPastExperiance.GetValue(pFromdt.DataBind.Alias, i));
                            dtto = Convert.ToDateTime(dtPastExperiance.GetValue(pTodt.DataBind.Alias, i));
                            MstEmployeeExperience oNew = (from a in dbHrPayroll.MstEmployeeExperience where a.Id == pid select a).FirstOrDefault();
                            oNew.CompanyName = companay;
                            oNew.Position = position;
                            oNew.Duties = duties;
                            oNew.Notes = notes;
                            oNew.LastSalary = lastsalary;
                            oNew.FromDate = dtfrom;
                            oNew.ToDate = dtto;
                        }
                    }
                }
                dbHrPayroll.SubmitChanges();
                btnMain.Caption = "Ok";
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("submitdocument Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AddEmptyRowPastExperiance()
        {
            try
            {
                Int32 RowValue = 0;
                if (dtPastExperiance.Rows.Count == 0)
                {
                    dtPastExperiance.Rows.Add(1);
                    RowValue = dtPastExperiance.Rows.Count;
                    dtPastExperiance.SetValue(pId.DataBind.Alias, RowValue - 1, 0);
                    dtPastExperiance.SetValue(pIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtPastExperiance.SetValue(pCompany.DataBind.Alias, RowValue - 1, "");
                    dtPastExperiance.SetValue(pFromdt.DataBind.Alias, RowValue - 1, DateTime.Now);
                    dtPastExperiance.SetValue(pTodt.DataBind.Alias, RowValue - 1, DateTime.Now);
                    dtPastExperiance.SetValue(pPosition.DataBind.Alias, RowValue - 1, "");
                    dtPastExperiance.SetValue(pDuties.DataBind.Alias, RowValue - 1, "");
                    dtPastExperiance.SetValue(pNotes.DataBind.Alias, RowValue - 1, "");
                    dtPastExperiance.SetValue(pLastSalary.DataBind.Alias, RowValue - 1, "");
                    mtPastExperiance.AddRow(1, RowValue + 1);
                }
                else
                {
                    if (dtPastExperiance.GetValue(pCompany.DataBind.Alias, dtPastExperiance.Rows.Count - 1) == "")
                    {
                    }
                    else
                    {
                        dtPastExperiance.Rows.Add(1);
                        RowValue = dtPastExperiance.Rows.Count;
                        dtPastExperiance.SetValue(pId.DataBind.Alias, RowValue - 1, 0);
                        dtPastExperiance.SetValue(pIsNew.DataBind.Alias, RowValue - 1, "Y");
                        dtPastExperiance.SetValue(pCompany.DataBind.Alias, RowValue - 1, "");
                        dtPastExperiance.SetValue(pFromdt.DataBind.Alias, RowValue - 1, DateTime.Now);
                        dtPastExperiance.SetValue(pTodt.DataBind.Alias, RowValue - 1, DateTime.Now);
                        dtPastExperiance.SetValue(pPosition.DataBind.Alias, RowValue - 1, "");
                        dtPastExperiance.SetValue(pDuties.DataBind.Alias, RowValue - 1, "");
                        dtPastExperiance.SetValue(pNotes.DataBind.Alias, RowValue - 1, "");
                        dtPastExperiance.SetValue(pLastSalary.DataBind.Alias, RowValue - 1, "");
                        mtPastExperiance.AddRow(1, mtPastExperiance.RowCount + 1);
                    }
                }
                mtPastExperiance.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("AddEmptyRowPastExperiance Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error); 
            }
        }

        #endregion
    }
}
