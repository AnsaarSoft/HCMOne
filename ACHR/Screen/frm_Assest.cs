using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_Assest : HRMSBaseForm
    {

        #region Variable

        SAPbouiCOM.Button btnMain;
        SAPbouiCOM.Matrix mtAssestment;
        SAPbouiCOM.Item ibtnMain, imtAssestment;
        SAPbouiCOM.DataTable dtAssestment;
        SAPbouiCOM.Column cIsNew, cID, cCode, cDescription;


        #endregion

        #region Business One Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            InitiallizeForm();
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "btmain":
                    CheckMainButton();
                    break;
                case "xsd":
                    break;
            }
        }

        public override void etAfterLostFocus(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterLostFocus(ref pVal, ref BubbleEvent);
            if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE && btnMain.Caption == "Ok")
            {
                btnMain.Caption = "Update";
            }
        }

        #endregion

        #region Local Methods

        private void InitiallizeForm()
        {
            oForm.Freeze(true);
            try
            {
                btnMain = oForm.Items.Item("btmain").Specific;
                ibtnMain = oForm.Items.Item("btmain");

                mtAssestment = oForm.Items.Item("mtmain").Specific;
                imtAssestment = oForm.Items.Item("mtmain");
                dtAssestment = oForm.DataSources.DataTables.Item("dtskill");
                cIsNew = mtAssestment.Columns.Item("isnew");
                cIsNew.Visible = false;
                cID = mtAssestment.Columns.Item("id");
                cID.Visible = false;
                cCode = mtAssestment.Columns.Item("code");
                cDescription = mtAssestment.Columns.Item("desc");
                btnMain.Caption = "Ok";

                FillAssestments();
                AddEmptyRow();

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception  InitiallizeForm Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            oForm.Freeze(false);
        }

        private void FormStatus()
        {
            try
            {
                imtAssestment.AffectsFormMode = true;
            }
            catch (Exception)
            {
            }
        }

        private void FillAssestments()
        {
            try
            {
                IEnumerable<MstAssestment> Assestments = from a in dbHrPayroll.MstAssestment select a;
                UInt16 i = 0;
                if (Assestments.Count() == 0)
                {
                    return;
                }
                dtAssestment.Rows.Clear();
                dtAssestment.Rows.Add(Assestments.Count());
                foreach (MstAssestment Assestment in Assestments)
                {
                    dtAssestment.SetValue(cIsNew.DataBind.Alias, i, "N");
                    dtAssestment.SetValue(cID.DataBind.Alias, i, Assestment.ID);
                    dtAssestment.SetValue(cCode.DataBind.Alias, i, Assestment.Code);
                    dtAssestment.SetValue(cDescription.DataBind.Alias, i, Assestment.Assestment);
                    i++;
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("All Data can't load successfully" + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AddEmptyRow()
        {
            Int32 RowValue = 0;

            if (dtAssestment.Rows.Count == 0)
            {
                dtAssestment.Rows.Add(1);
                RowValue = dtAssestment.Rows.Count;
                dtAssestment.SetValue(cIsNew.DataBind.Alias, RowValue - 1, "Y");
                dtAssestment.SetValue(cID.DataBind.Alias, RowValue - 1, "0");
                dtAssestment.SetValue(cCode.DataBind.Alias, RowValue - 1, "");
                dtAssestment.SetValue(cDescription.DataBind.Alias, RowValue - 1, "");
                mtAssestment.AddRow(1, RowValue + 1);
            }
            else
            {
                if (dtAssestment.GetValue(cCode.DataBind.Alias, dtAssestment.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtAssestment.Rows.Add(1);
                    RowValue = dtAssestment.Rows.Count;
                    dtAssestment.SetValue(cIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtAssestment.SetValue(cID.DataBind.Alias, RowValue - 1, "0");
                    dtAssestment.SetValue(cCode.DataBind.Alias, RowValue - 1, "");
                    dtAssestment.SetValue(cDescription.DataBind.Alias, RowValue - 1, "");
                    mtAssestment.AddRow(1, mtAssestment.RowCount + 1);
                }
            }
            mtAssestment.LoadFromDataSource();
        }

        private void CheckMainButton()
        {
            switch (btnMain.Caption)
            {
                case "Update":
                    SubmitChanges();
                    btnMain.Caption = "Ok";
                    break;
                case "Ok":
                    oForm.Close();
                    break;
            }
        }

        private void SubmitChanges()
        {
            oForm.Freeze(true);
            try
            {
                dtAssestment.Rows.Clear();
                mtAssestment.FlushToDataSource();
                String IsNew, Code, Description;
                Int32 Id = 0;
                for (Int32 i = 0; i < dtAssestment.Rows.Count; i++)
                {
                    IsNew = Convert.ToString(dtAssestment.GetValue(cIsNew.DataBind.Alias, i));
                    Id = Convert.ToInt32(dtAssestment.GetValue(cID.DataBind.Alias, i));
                    Code = Convert.ToString(dtAssestment.GetValue(cCode.DataBind.Alias, i));
                    Description = Convert.ToString(dtAssestment.GetValue(cDescription.DataBind.Alias, i));
                    if (!String.IsNullOrEmpty(Code))
                    {
                        if (IsNew == "Y")
                        {
                            MstAssestment oNew = new MstAssestment();
                            oNew.Code = Code;
                            oNew.Assestment = Description;
                            oNew.CreateDt = DateTime.Now;
                            oNew.UserId = oCompany.UserName;
                            oNew.UpdateDt = DateTime.Now;
                            oNew.UpdatedBy = oCompany.UserName;
                            dbHrPayroll.MstAssestment.InsertOnSubmit(oNew);
                        }
                        else if (IsNew == "N")
                        {
                            MstAssestment oOld = (from a in dbHrPayroll.MstAssestment where a.ID == Id select a).FirstOrDefault();
                            if (oOld != null)
                            {
                                oOld.Code = Code;
                                oOld.Assestment = Description;
                                oOld.UpdatedBy = oCompany.UserName;
                                oOld.UpdateDt = DateTime.Now;
                            }
                        }
                        dbHrPayroll.SubmitChanges();
                    }
                }
                FillAssestments();
                AddEmptyRow();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception @ SubmitChanges Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            oForm.Freeze(false);
        }

        #endregion
    }
}
