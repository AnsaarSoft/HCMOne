using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DIHRMS;
using DIHRMS.Custom;

namespace ACHR.Screen
{
    class frm_EmpMstR:HRMSBaseForm
    {
        #region Variable

        SAPbouiCOM.EditText txtEmployeeiD;
        SAPbouiCOM.Matrix mtRelatives;
        SAPbouiCOM.DataTable dtRelatives;
        SAPbouiCOM.Column rIsNew, rId, rType, rName, rIDNo, rTelephone, rEmail;
        SAPbouiCOM.Column rMCStartDate, rMCExpiryDate, rDOB, rDepencdent, rMCNo;
        SAPbouiCOM.Button btnMain;
        SAPbouiCOM.Item ibtnMain;

        #endregion

        #region B1 Event

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
                case "btnMain":
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
            if (pVal.ItemUID == "mtrelative" && pVal.ColUID == "mcenddt")
            {
                oForm.Freeze(true);
                mtRelatives.FlushToDataSource();
                AddEmptyRowRelatives();
                oForm.Freeze(false);
            }
            if (pVal.ItemUID == "mtrelative" && pVal.ColUID == "type")
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

                btnMain = oForm.Items.Item("btnMain").Specific;
                ibtnMain = oForm.Items.Item("btnMain");
                
                //Relatives

                mtRelatives = oForm.Items.Item("mtrelative").Specific;
                dtRelatives = oForm.DataSources.DataTables.Item("dtrelative");
                rIsNew = mtRelatives.Columns.Item("isnew");
                rIsNew.Visible = false;
                rId = mtRelatives.Columns.Item("id");
                rId.Visible = false;
                rType = mtRelatives.Columns.Item("type");
                rName = mtRelatives.Columns.Item("name");
                rIDNo = mtRelatives.Columns.Item("idno");
                rTelephone = mtRelatives.Columns.Item("phone");
                rEmail = mtRelatives.Columns.Item("email");
                rDOB = mtRelatives.Columns.Item("dob");
                rDepencdent = mtRelatives.Columns.Item("depend");
                rMCNo = mtRelatives.Columns.Item("mcno");
                rMCStartDate = mtRelatives.Columns.Item("mcstartdt");
                rMCExpiryDate = mtRelatives.Columns.Item("mcenddt");

                FillRelationShipCombo(rType);
                FillDocument();
                btnMain.Caption = "Update";
                txtEmployeeiD.Value = Program.ExtendendEmpID;
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("InitiallizeForm Exception : " + ex.Message , SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AddEmptyRowRelatives()
        {
            try
            {
                Int32 RowValue = 0;
                if (dtRelatives.Rows.Count == 0)
                {
                    dtRelatives.Rows.Add(1);
                    RowValue = dtRelatives.Rows.Count;
                    dtRelatives.SetValue(rIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtRelatives.SetValue(rId.DataBind.Alias, RowValue - 1, "0");
                    dtRelatives.SetValue(rName.DataBind.Alias, RowValue - 1, "");
                    dtRelatives.SetValue(rIDNo.DataBind.Alias, RowValue - 1, "");
                    dtRelatives.SetValue(rTelephone.DataBind.Alias, RowValue - 1, 0);
                    dtRelatives.SetValue(rEmail.DataBind.Alias, RowValue - 1, "");
                    dtRelatives.SetValue(rDOB.DataBind.Alias, RowValue - 1, DateTime.Now);
                    dtRelatives.SetValue(rDepencdent.DataBind.Alias, RowValue - 1, "");
                    dtRelatives.SetValue(rMCNo.DataBind.Alias, RowValue - 1, 0);
                    dtRelatives.SetValue(rMCStartDate.DataBind.Alias, RowValue - 1, DateTime.Now);
                    dtRelatives.SetValue(rMCExpiryDate.DataBind.Alias, RowValue - 1, DateTime.Now);
                    mtRelatives.AddRow(1, RowValue + 1);
                }
                else
                {
                    if (dtRelatives.GetValue(rType.DataBind.Alias, dtRelatives.Rows.Count - 1) == "")
                    {
                    }
                    else
                    {

                        dtRelatives.Rows.Add(1);
                        RowValue = dtRelatives.Rows.Count;
                        dtRelatives.SetValue(rIsNew.DataBind.Alias, RowValue - 1, "Y");
                        dtRelatives.SetValue(rId.DataBind.Alias, RowValue - 1, "0");
                        dtRelatives.SetValue(rName.DataBind.Alias, RowValue - 1, "");
                        dtRelatives.SetValue(rIDNo.DataBind.Alias, RowValue - 1, "0");
                        dtRelatives.SetValue(rTelephone.DataBind.Alias, RowValue - 1, 0);
                        dtRelatives.SetValue(rEmail.DataBind.Alias, RowValue - 1, "");
                        dtRelatives.SetValue(rDOB.DataBind.Alias, RowValue - 1, DateTime.Now);
                        dtRelatives.SetValue(rDepencdent.DataBind.Alias, RowValue - 1, "");
                        dtRelatives.SetValue(rMCNo.DataBind.Alias, RowValue - 1, 0);
                        dtRelatives.SetValue(rMCStartDate.DataBind.Alias, RowValue - 1, DateTime.Now);
                        dtRelatives.SetValue(rMCExpiryDate.DataBind.Alias, RowValue - 1, DateTime.Now);
                        mtRelatives.AddRow(1, mtRelatives.RowCount + 1);
                    }
                }
                mtRelatives.LoadFromDataSource();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_EmpMst Function: AddEmptyRowRelative Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        private void FillDocument()
        {
            try
            {
                if (Program.ExtendendEmpID == "" || Program.ExtendendEmpID == null)
                {
                    oApplication.SetStatusBarMessage("Please select employee to add relative information ", SAPbouiCOM.BoMessageTime.bmt_Short, true);
                    return;
                }
                else
                {
                    MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee
                                        where a.EmpID == Program.ExtendendEmpID
                                        select a).FirstOrDefault();

                    dtRelatives.Rows.Clear();
                    foreach (MstEmployeeRelatives One in oEmp.MstEmployeeRelatives)
                    {
                        dtRelatives.Rows.Add(1);
                        dtRelatives.SetValue(rIsNew.DataBind.Alias, dtRelatives.Rows.Count - 1, "N");
                        dtRelatives.SetValue(rId.DataBind.Alias, dtRelatives.Rows.Count - 1, One.Id);
                        dtRelatives.SetValue(rType.DataBind.Alias, dtRelatives.Rows.Count - 1, One.RelativeID);
                        dtRelatives.SetValue(rName.DataBind.Alias, dtRelatives.Rows.Count - 1, One.FirstName);
                        dtRelatives.SetValue(rIDNo.DataBind.Alias, dtRelatives.Rows.Count - 1, One.IDNoRelative);
                        dtRelatives.SetValue(rTelephone.DataBind.Alias, dtRelatives.Rows.Count - 1, One.TelephoneNo);
                        dtRelatives.SetValue(rDOB.DataBind.Alias, dtRelatives.Rows.Count - 1, One.BOD);
                        dtRelatives.SetValue(rDepencdent.DataBind.Alias, dtRelatives.Rows.Count - 1, One.FlgDependent == true ? "Y" : "N");
                        dtRelatives.SetValue(rMCNo.DataBind.Alias, dtRelatives.Rows.Count - 1, One.MedicalCardNo);
                        dtRelatives.SetValue(rMCStartDate.DataBind.Alias, dtRelatives.Rows.Count - 1, One.MedicalCardStartDate);
                        dtRelatives.SetValue(rMCExpiryDate.DataBind.Alias, dtRelatives.Rows.Count - 1, One.MedicalCardExpiryDate);
                    }
                    mtRelatives.LoadFromDataSource();

                    AddEmptyRowRelatives();
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Fill Document exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void SubmitDocument()
        {
            try
            {
                MstEmployee oEmp = (from a in dbHrPayroll.MstEmployee where a.EmpID == txtEmployeeiD.Value.Trim() select a).FirstOrDefault();
                mtRelatives.FlushToDataSource();

                if (dtRelatives.Rows.Count > 0)
                {
                    for (Int32 i = 0; i < dtRelatives.Rows.Count; i++)
                    {
                        int rid;
                        string RelativeCode, risnew, fname, lname, IDNoRelative, telephone, email, mccardno;
                        DateTime DateOfBirth, MCExpiryDate, MCStartDate;
                        RelativeCode = dtRelatives.GetValue(rType.DataBind.Alias, i);
                        risnew = Convert.ToString(dtRelatives.GetValue(rIsNew.DataBind.Alias, i));
                        if (risnew == "Y")
                        {

                            if (string.IsNullOrEmpty(RelativeCode)) continue;
                            rid = Convert.ToInt32(dtRelatives.GetValue(rId.DataBind.Alias, i));

                            fname = dtRelatives.GetValue(rName.DataBind.Alias, i);
                            IDNoRelative = dtRelatives.GetValue(rIDNo.DataBind.Alias, i);
                            telephone = dtRelatives.GetValue(rTelephone.DataBind.Alias, i);
                            email = dtRelatives.GetValue(rEmail.DataBind.Alias, i);
                            DateOfBirth = dtRelatives.GetValue(rDOB.DataBind.Alias, i);
                            MCStartDate = dtRelatives.GetValue(rMCStartDate.DataBind.Alias, i);
                            MCExpiryDate = dtRelatives.GetValue(rMCExpiryDate.DataBind.Alias, i);
                            mccardno = dtRelatives.GetValue(rMCNo.DataBind.Alias, i);

                            MstEmployeeRelatives oRelative = new MstEmployeeRelatives();
                            oRelative.RelativeID = RelativeCode;
                            oRelative.RelativeLOVType = "Relative";
                            oRelative.FirstName = fname;
                            oRelative.IDNoRelative = IDNoRelative;
                            oRelative.TelephoneNo = telephone;
                            oRelative.Email = email;
                            oRelative.MedicalCardNo = mccardno;
                            oRelative.MedicalCardStartDate = MCStartDate;
                            oRelative.MedicalCardExpiryDate = MCExpiryDate;
                            oRelative.BOD = DateOfBirth;
                            if (dtRelatives.GetValue(rDepencdent.DataBind.Alias, i) == "Y")
                            {
                                oRelative.FlgDependent = true;
                            }
                            else
                            {
                                oRelative.FlgDependent = false;
                            }

                            oEmp.MstEmployeeRelatives.Add(oRelative);
                        }
                        else if (risnew == "N")
                        {
                            rid = Convert.ToInt32(dtRelatives.GetValue(rId.DataBind.Alias, i));
                            fname = dtRelatives.GetValue(rName.DataBind.Alias, i);
                            IDNoRelative = dtRelatives.GetValue(rIDNo.DataBind.Alias, i);
                            telephone = dtRelatives.GetValue(rTelephone.DataBind.Alias, i);
                            email = dtRelatives.GetValue(rEmail.DataBind.Alias, i);
                            DateOfBirth = dtRelatives.GetValue(rDOB.DataBind.Alias, i);
                            MCStartDate = dtRelatives.GetValue(rMCStartDate.DataBind.Alias, i);
                            MCExpiryDate = dtRelatives.GetValue(rMCExpiryDate.DataBind.Alias, i);
                            mccardno = dtRelatives.GetValue(rMCNo.DataBind.Alias, i);
                            MstEmployeeRelatives oUpd = (from a in dbHrPayroll.MstEmployeeRelatives where a.Id == rid select a).FirstOrDefault();
                            oUpd.RelativeID = RelativeCode;
                            oUpd.RelativeLOVType = "Relative";
                            oUpd.FirstName = fname;
                            oUpd.IDNoRelative = IDNoRelative;
                            oUpd.TelephoneNo = telephone;
                            oUpd.Email = email;
                            oUpd.MedicalCardNo = mccardno;
                            oUpd.MedicalCardStartDate = MCStartDate;
                            oUpd.MedicalCardExpiryDate = MCExpiryDate;
                            oUpd.BOD = DateOfBirth;
                            if (dtRelatives.GetValue(rDepencdent.DataBind.Alias, i) == "Y")
                            {
                                oUpd.FlgDependent = true;
                            }
                            else
                            {
                                oUpd.FlgDependent = false;
                            }
                        }
                    }
                    dbHrPayroll.SubmitChanges();
                    btnMain.Caption = "Ok";
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("update employee relative exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillRelationShipCombo(SAPbouiCOM.Column pCombo)
        {
            try
            {
                IEnumerable<MstRelation> Collection = from a in dbHrPayroll.MstRelation select a;
                pCombo.ValidValues.Add("-1", "");
                foreach (MstRelation One in Collection)
                {
                    pCombo.ValidValues.Add(One.Id.ToString(), One.Code);
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }


        #endregion
    }
}
