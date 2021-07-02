using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_EmpMstE:HRMSBaseForm
    {
        #region Variable

        SAPbouiCOM.EditText txtEmployeeiD;
        SAPbouiCOM.Matrix mtEducation;
        SAPbouiCOM.DataTable dtEducation;
        SAPbouiCOM.Column eIsNew, eId, eInstituteName, eFromDate, eToDate, eSubject, eQualification, eAwardedQlf, eMark, eNotes;
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
            if (pVal.ItemUID == "mtedu" && pVal.ColUID == "notes")
            {
                oForm.Freeze(true);
                mtEducation.FlushToDataSource();
                AddEmptyRowEducation();
                oForm.Freeze(false);
            }
            if (pVal.ItemUID == "mtedu" && pVal.ColUID == "insname")
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

                mtEducation = oForm.Items.Item("mtedu").Specific;
                dtEducation = oForm.DataSources.DataTables.Item("dtins");
                eId = mtEducation.Columns.Item("id");
                eId.Visible = false;
                eIsNew = mtEducation.Columns.Item("isnew");
                eIsNew.Visible = false;
                eInstituteName = mtEducation.Columns.Item("insname");
                eFromDate = mtEducation.Columns.Item("fromdt");
                eToDate = mtEducation.Columns.Item("todt");
                eSubject = mtEducation.Columns.Item("subject");
                eQualification = mtEducation.Columns.Item("qlft");
                eAwardedQlf = mtEducation.Columns.Item("aqlft");
                eMark = mtEducation.Columns.Item("mark");
                eNotes = mtEducation.Columns.Item("notes");

                FillInstituteCombo(eInstituteName);
                FillQualificationCombo(eQualification);
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
                dtEducation.Rows.Clear();
                foreach (MstEmployeeEducation One in oEmp.MstEmployeeEducation)
                {
                    dtEducation.Rows.Add(1);
                    dtEducation.SetValue(eIsNew.DataBind.Alias, dtEducation.Rows.Count - 1, "N");
                    dtEducation.SetValue(eId.DataBind.Alias, dtEducation.Rows.Count - 1, One.Id);
                    dtEducation.SetValue(eInstituteName.DataBind.Alias, dtEducation.Rows.Count - 1, One.InstituteID);
                    dtEducation.SetValue(eFromDate.DataBind.Alias, dtEducation.Rows.Count - 1, One.FromDate);
                    dtEducation.SetValue(eToDate.DataBind.Alias, dtEducation.Rows.Count - 1, One.ToDate);
                    dtEducation.SetValue(eSubject.DataBind.Alias, dtEducation.Rows.Count - 1, One.Subject);
                    dtEducation.SetValue(eQualification.DataBind.Alias, dtEducation.Rows.Count - 1, One.QualificationID);
                    dtEducation.SetValue(eAwardedQlf.DataBind.Alias, dtEducation.Rows.Count - 1, One.AwardedQualification);
                    dtEducation.SetValue(eMark.DataBind.Alias, dtEducation.Rows.Count - 1, One.MarkGrade);
                    dtEducation.SetValue(eNotes.DataBind.Alias, dtEducation.Rows.Count - 1, One.Notes);

                }
                mtEducation.LoadFromDataSource();
                AddEmptyRowEducation();

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
                mtEducation.FlushToDataSource();
                if (dtEducation.Rows.Count > 0)
                {
                    for (int i = 0; i < dtEducation.Rows.Count; i++)
                    {
                        int eid = 0, institute = 0, qualification = 0;
                        string eisnew, subject, awardqlfy, marks, notes;
                        DateTime dtfrom, dtto;
                        eid = Convert.ToInt32(dtEducation.GetValue(eId.DataBind.Alias, i));
                        eisnew = dtEducation.GetValue(eIsNew.DataBind.Alias, i);
                        institute = dtEducation.GetValue(eInstituteName.DataBind.Alias, i);
                        qualification = dtEducation.GetValue(eQualification.DataBind.Alias, i);
                        if (eisnew == "Y")
                        {
                            if (institute == -1) continue;
                            if (qualification == -1) continue;
                            subject = dtEducation.GetValue(eSubject.DataBind.Alias, i);
                            awardqlfy = dtEducation.GetValue(eAwardedQlf.DataBind.Alias, i);
                            marks = dtEducation.GetValue(eMark.DataBind.Alias, i);
                            notes = dtEducation.GetValue(eNotes.DataBind.Alias, i);
                            dtfrom = Convert.ToDateTime(dtEducation.GetValue(eFromDate.DataBind.Alias, i));
                            dtto = Convert.ToDateTime(dtEducation.GetValue(eToDate.DataBind.Alias, i));
                            MstEmployeeEducation oNew = new MstEmployeeEducation();
                            oNew.InstituteID = institute;
                            oNew.QualificationID = qualification;
                            oNew.Subject = subject;
                            oNew.AwardedQualification = awardqlfy;
                            oNew.MarkGrade = marks;
                            oNew.Notes = notes;
                            oNew.FromDate = dtfrom;
                            oNew.ToDate = dtto;
                            oNew.CreateDate = DateTime.Now;
                            oNew.UpdateDate = DateTime.Now;
                            oNew.UserId = oCompany.UserName;
                            oNew.UpdatedBy = oCompany.UserName;
                            oEmp.MstEmployeeEducation.Add(oNew);
                        }
                        else if (eisnew == "N")
                        {
                            subject = dtEducation.GetValue(eSubject.DataBind.Alias, i);
                            awardqlfy = dtEducation.GetValue(eAwardedQlf.DataBind.Alias, i);
                            marks = dtEducation.GetValue(eMark.DataBind.Alias, i);
                            notes = dtEducation.GetValue(eNotes.DataBind.Alias, i);
                            dtfrom = Convert.ToDateTime(dtEducation.GetValue(eFromDate.DataBind.Alias, i));
                            dtto = Convert.ToDateTime(dtEducation.GetValue(eToDate.DataBind.Alias, i));
                            MstEmployeeEducation oNew = (from a in dbHrPayroll.MstEmployeeEducation where a.Id == eid select a).FirstOrDefault();
                            oNew.InstituteID = institute;
                            oNew.QualificationID = qualification;
                            oNew.Subject = subject;
                            oNew.AwardedQualification = awardqlfy;
                            oNew.MarkGrade = marks;
                            oNew.Notes = notes;
                            oNew.FromDate = dtfrom;
                            oNew.ToDate = dtto;
                            oNew.CreateDate = DateTime.Now;
                            oNew.UpdateDate = DateTime.Now;
                            oNew.UserId = oCompany.UserName;
                            oNew.UpdatedBy = oCompany.UserName;
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

        private void AddEmptyRowEducation()
        {
            try
            {
                Int32 RowValue = 0;
                if (dtEducation.Rows.Count == 0)
                {
                    dtEducation.Rows.Add(1);
                    RowValue = dtEducation.Rows.Count;
                    dtEducation.SetValue(eIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtEducation.SetValue(eId.DataBind.Alias, RowValue - 1, 0);
                    dtEducation.SetValue(eInstituteName.DataBind.Alias, RowValue - 1, -1);
                    dtEducation.SetValue(eFromDate.DataBind.Alias, RowValue - 1, DateTime.Now);
                    dtEducation.SetValue(eToDate.DataBind.Alias, RowValue - 1, DateTime.Now);
                    dtEducation.SetValue(eSubject.DataBind.Alias, RowValue - 1, "");
                    dtEducation.SetValue(eQualification.DataBind.Alias, RowValue - 1, -1);
                    dtEducation.SetValue(eAwardedQlf.DataBind.Alias, RowValue - 1, "");
                    dtEducation.SetValue(eMark.DataBind.Alias, RowValue - 1, "");
                    dtEducation.SetValue(eNotes.DataBind.Alias, RowValue - 1, "");
                    mtEducation.AddRow(1, RowValue + 1);
                }
                else
                {
                    if (dtEducation.GetValue(eInstituteName.DataBind.Alias, dtEducation.Rows.Count - 1) == -1)
                    {
                    }
                    else
                    {
                        dtEducation.Rows.Add(1);
                        RowValue = dtEducation.Rows.Count;
                        dtEducation.SetValue(eIsNew.DataBind.Alias, RowValue - 1, "Y");
                        dtEducation.SetValue(eId.DataBind.Alias, RowValue - 1, 0);
                        dtEducation.SetValue(eInstituteName.DataBind.Alias, RowValue - 1, -1);
                        dtEducation.SetValue(eFromDate.DataBind.Alias, RowValue - 1, DateTime.Now);
                        dtEducation.SetValue(eToDate.DataBind.Alias, RowValue - 1, DateTime.Now);
                        dtEducation.SetValue(eSubject.DataBind.Alias, RowValue - 1, "");
                        dtEducation.SetValue(eQualification.DataBind.Alias, RowValue - 1, -1);
                        dtEducation.SetValue(eAwardedQlf.DataBind.Alias, RowValue - 1, "");
                        dtEducation.SetValue(eMark.DataBind.Alias, RowValue - 1, "");
                        dtEducation.SetValue(eNotes.DataBind.Alias, RowValue - 1, "");
                        mtEducation.AddRow(1, mtEducation.RowCount + 1);
                    }
                }
                mtEducation.LoadFromDataSource();
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }

        }

        private void FillInstituteCombo(SAPbouiCOM.Column pCombo)
        {
            try
            {
                IEnumerable<MstInstitute> Collection = from a in dbHrPayroll.MstInstitute select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (MstInstitute One in Collection)
                {
                    pCombo.ValidValues.Add(One.Id.ToString(), One.Name);
                }

            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillInstituteCombo exception : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillQualificationCombo(SAPbouiCOM.Column pCombo)
        {
            try
            {
                IEnumerable<MstQualification> Collection = from a in dbHrPayroll.MstQualification select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (MstQualification One in Collection)
                {
                    pCombo.ValidValues.Add(One.Id.ToString(), One.Code);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("FillQualificationCombo exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion
    }
}
