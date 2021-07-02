using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_EmpMstQ:HRMSBaseForm
    {
        #region Variable

        SAPbouiCOM.EditText txtEmployeeiD;
        SAPbouiCOM.Matrix mtCertification;
        SAPbouiCOM.DataTable dtCertification;
        SAPbouiCOM.Column cIsNew, cId, cCertification, cAwardedBy, cAwardStatus, cDescription;
        SAPbouiCOM.Column cNotes, cValidated;
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
            if (pVal.ItemUID == "mtqlf" && pVal.ColUID == "validated")
            {
                oForm.Freeze(true);
                mtCertification.FlushToDataSource();
                AddEmptyRowCertification();
                oForm.Freeze(false);
            }
            if (pVal.ItemUID == "mtqlf" && pVal.ColUID == "cert")
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

                mtCertification = oForm.Items.Item("mtqlf").Specific;
                dtCertification = oForm.DataSources.DataTables.Item("dtcert");
                cId = mtCertification.Columns.Item("id");
                cId.Visible = false;
                cIsNew = mtCertification.Columns.Item("isnew");
                cIsNew.Visible = false;
                cCertification = mtCertification.Columns.Item("cert");
                cAwardedBy = mtCertification.Columns.Item("awdby");
                cAwardStatus = mtCertification.Columns.Item("awdstatus");
                cDescription = mtCertification.Columns.Item("desc");
                cNotes = mtCertification.Columns.Item("notes");
                cValidated = mtCertification.Columns.Item("validated");

                FillCertificationCombo(cCertification);
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
                dtCertification.Rows.Clear();
                foreach (MstEmployeeCertifications One in oEmp.MstEmployeeCertifications)
                {
                    dtCertification.Rows.Add(1);
                    dtCertification.SetValue(cIsNew.DataBind.Alias, dtCertification.Rows.Count - 1, "N");
                    dtCertification.SetValue(cId.DataBind.Alias, dtCertification.Rows.Count - 1, One.Id);
                    dtCertification.SetValue(cCertification.DataBind.Alias, dtCertification.Rows.Count - 1, One.CertificationID);
                    dtCertification.SetValue(cAwardedBy.DataBind.Alias, dtCertification.Rows.Count - 1, One.AwardedBy);
                    dtCertification.SetValue(cAwardStatus.DataBind.Alias, dtCertification.Rows.Count - 1, One.AwardStatus);
                    dtCertification.SetValue(cDescription.DataBind.Alias, dtCertification.Rows.Count - 1, One.Description);
                    dtCertification.SetValue(cNotes.DataBind.Alias, dtCertification.Rows.Count - 1, One.Notes);
                    dtCertification.SetValue(cValidated.DataBind.Alias, dtCertification.Rows.Count - 1, One.Validated);
                }
                mtCertification.LoadFromDataSource();
                AddEmptyRowCertification();

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
                mtCertification.FlushToDataSource();
                if (dtCertification.Rows.Count > 0)
                {
                    for (int i = 0; i < dtCertification.Rows.Count; i++)
                    {
                        int cid;
                        string certification, description, notes, awardstatus, awardby, validation, cisnew;
                        cid = Convert.ToInt32(dtCertification.GetValue(cId.DataBind.Alias, i));
                        cisnew = dtCertification.GetValue(cIsNew.DataBind.Alias, i);
                        certification = dtCertification.GetValue(cCertification.DataBind.Alias, i);
                        if (cisnew == "Y" && !string.IsNullOrEmpty(certification))
                        {
                            if (string.IsNullOrEmpty(certification)) continue;
                            description = dtCertification.GetValue(cDescription.DataBind.Alias, i);
                            notes = dtCertification.GetValue(cNotes.DataBind.Alias, i);
                            awardby = dtCertification.GetValue(cAwardedBy.DataBind.Alias, i);
                            awardstatus = dtCertification.GetValue(cAwardStatus.DataBind.Alias, i);
                            validation = dtCertification.GetValue(cValidated.DataBind.Alias, i);
                            MstEmployeeCertifications oNew = new MstEmployeeCertifications();
                            oNew.CertificationID = Convert.ToInt32(certification);
                            oNew.Description = description;
                            oNew.AwardedBy = awardby;
                            oNew.AwardStatus = awardstatus;
                            oNew.Notes = notes;
                            oNew.Validated = validation;
                            oEmp.MstEmployeeCertifications.Add(oNew);
                        }
                        else if (cisnew == "N")
                        {
                            description = dtCertification.GetValue(cDescription.DataBind.Alias, i);
                            notes = dtCertification.GetValue(cNotes.DataBind.Alias, i);
                            awardby = dtCertification.GetValue(cAwardedBy.DataBind.Alias, i);
                            awardstatus = dtCertification.GetValue(cAwardStatus.DataBind.Alias, i);
                            validation = dtCertification.GetValue(cValidated.DataBind.Alias, i);
                            MstEmployeeCertifications oNew = (from a in dbHrPayroll.MstEmployeeCertifications where a.Id == cid select a).FirstOrDefault();
                            oNew.CertificationID = Convert.ToInt32(certification);
                            oNew.Description = description;
                            oNew.AwardedBy = awardby;
                            oNew.AwardStatus = awardstatus;
                            oNew.Notes = notes;
                            oNew.Validated = validation;
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

        private void AddEmptyRowCertification()
        {
            try
            {
                Int32 RowValue = 0;
                if (dtCertification.Rows.Count == 0)
                {
                    dtCertification.Rows.Add(1);
                    RowValue = dtCertification.Rows.Count;
                    dtCertification.SetValue(cIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtCertification.SetValue(cId.DataBind.Alias, RowValue - 1, "0");
                    dtCertification.SetValue(cCertification.DataBind.Alias, RowValue - 1, "");
                    dtCertification.SetValue(cAwardedBy.DataBind.Alias, RowValue - 1, "");
                    dtCertification.SetValue(cAwardStatus.DataBind.Alias, RowValue - 1, "");
                    dtCertification.SetValue(cDescription.DataBind.Alias, RowValue - 1, "");
                    dtCertification.SetValue(cNotes.DataBind.Alias, RowValue - 1, "");
                    dtCertification.SetValue(cValidated.DataBind.Alias, RowValue - 1, "");
                    mtCertification.AddRow(1, RowValue + 1);
                }
                else
                {
                    if (dtCertification.GetValue(cCertification.DataBind.Alias, dtCertification.Rows.Count - 1) == "")
                    {
                    }
                    else
                    {
                        dtCertification.Rows.Add(1);
                        RowValue = dtCertification.Rows.Count;
                        dtCertification.SetValue(cIsNew.DataBind.Alias, RowValue - 1, "Y");
                        dtCertification.SetValue(cId.DataBind.Alias, RowValue - 1, "0");
                        dtCertification.SetValue(cCertification.DataBind.Alias, RowValue - 1, "");
                        dtCertification.SetValue(cAwardedBy.DataBind.Alias, RowValue - 1, "");
                        dtCertification.SetValue(cAwardStatus.DataBind.Alias, RowValue - 1, "");
                        dtCertification.SetValue(cDescription.DataBind.Alias, RowValue - 1, "");
                        dtCertification.SetValue(cNotes.DataBind.Alias, RowValue - 1, "");
                        dtCertification.SetValue(cValidated.DataBind.Alias, RowValue - 1, "");
                        mtCertification.AddRow(1, mtCertification.RowCount + 1);
                    }
                }
                mtCertification.LoadFromDataSource();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("AddEmptyRowCertification Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillCertificationCombo(SAPbouiCOM.Column pCombo)
        {
            try
            {
                IEnumerable<MstCertification> Collection = from a in dbHrPayroll.MstCertification select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (MstCertification One in Collection)
                {
                    pCombo.ValidValues.Add(One.Id.ToString(), One.Name);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillCertificationCombo Exception : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion
    }
}
