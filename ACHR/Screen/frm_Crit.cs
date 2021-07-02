using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_Crit: HRMSBaseForm
    {
        #region Variable

        SAPbouiCOM.Button btnMain;
        SAPbouiCOM.Matrix mtCritaria;
        SAPbouiCOM.Item ibtnMain, imtCritaria, icbAssestment;
        SAPbouiCOM.DataTable dtCritaria;
        SAPbouiCOM.Column cIsNew, cID, cCode, cDescription, cMinMarks, cMarks;
        SAPbouiCOM.ComboBox cbAssestment;


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

        public override void etAfterCmbSelect(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterCmbSelect(ref pVal, ref BubbleEvent);
            if (pVal.ItemUID == icbAssestment.UniqueID)
            {
                FillCritaria(cbAssestment.Selected.Value);
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

                cbAssestment = oForm.Items.Item("cbassest").Specific;
                icbAssestment = oForm.Items.Item("cbassest");
                oForm.DataSources.UserDataSources.Add("cbassest", SAPbouiCOM.BoDataType.dt_SHORT_TEXT);
                cbAssestment.DataBind.SetBound(true, "", "cbassest");

                mtCritaria = oForm.Items.Item("mtmain").Specific;
                imtCritaria = oForm.Items.Item("mtmain");
                dtCritaria = oForm.DataSources.DataTables.Item("dtcrit");
                cIsNew = mtCritaria.Columns.Item("isnew");
                cIsNew.Visible = false;
                cID = mtCritaria.Columns.Item("id");
                cID.Visible = false;
                cCode = mtCritaria.Columns.Item("code");
                cDescription = mtCritaria.Columns.Item("desc");
                cMinMarks = mtCritaria.Columns.Item("mmark");
                cMarks = mtCritaria.Columns.Item("mark");

                btnMain.Caption = "Ok";

                FillAssetmentCombo(cbAssestment);
                dtCritaria.Rows.Clear();
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
                imtCritaria.AffectsFormMode = true;
            }
            catch (Exception)
            {
            }
        }

        private void FillCritaria(String pParentAssistmentID)
        {
            try
            {
                IEnumerable<MstAssestmentCriteria> Criterion = from a in dbHrPayroll.MstAssestmentCriteria
                                                               where a.AssestmentID.ToString() == pParentAssistmentID
                                                               select a;
                UInt16 i = 0;
                if (Criterion.Count() == 0)
                {
                    return;
                }
                dtCritaria.Rows.Clear();
                dtCritaria.Rows.Add(Criterion.Count());
                foreach (MstAssestmentCriteria Critaria in Criterion)
                {
                    dtCritaria.SetValue(cIsNew.DataBind.Alias, i, "N");
                    dtCritaria.SetValue(cID.DataBind.Alias, i, Critaria.ID);
                    dtCritaria.SetValue(cCode.DataBind.Alias, i, Critaria.Criteria);
                    dtCritaria.SetValue(cDescription.DataBind.Alias, i, Critaria.Description);
                    dtCritaria.SetValue(cMarks.DataBind.Alias, i, String.Format( "{0:0.00}", Critaria.Marks));
                    dtCritaria.SetValue(cMinMarks.DataBind.Alias, i, String.Format("{0:0.00}",Critaria.MinMarks));
                    i++;
                }
                AddEmptyRow();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("All Data can't load successfully" + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void FillAssetmentCombo(SAPbouiCOM.ComboBox pCombo)
        {
            try
            {
                IEnumerable<MstAssestment> Assestment = from a in dbHrPayroll.MstAssestment select a;
                pCombo.ValidValues.Add("0", "Select Assestment");
                foreach (MstAssestment One in Assestment)
                {
                    pCombo.ValidValues.Add(One.ID.ToString(), One.Code);
                }
                pCombo.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
            }
            catch (Exception)
            {
            }
        }

        private void AddEmptyRow()
        {
            Int32 RowValue = 0;

            if (dtCritaria.Rows.Count == 0)
            {
                dtCritaria.Rows.Add(1);
                RowValue = dtCritaria.Rows.Count;
                dtCritaria.SetValue(cIsNew.DataBind.Alias, RowValue - 1, "Y");
                dtCritaria.SetValue(cID.DataBind.Alias, RowValue - 1, "0");
                dtCritaria.SetValue(cCode.DataBind.Alias, RowValue - 1, "");
                dtCritaria.SetValue(cDescription.DataBind.Alias, RowValue - 1, "");
                dtCritaria.SetValue(cMarks.DataBind.Alias, RowValue - 1, "0");
                dtCritaria.SetValue(cMinMarks.DataBind.Alias, RowValue - 1, "0");
                mtCritaria.AddRow(1, RowValue + 1);
            }
            else
            {
                if (dtCritaria.GetValue(cCode.DataBind.Alias, dtCritaria.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtCritaria.Rows.Add(1);
                    RowValue = dtCritaria.Rows.Count;
                    dtCritaria.SetValue(cIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtCritaria.SetValue(cID.DataBind.Alias, RowValue - 1, "0");
                    dtCritaria.SetValue(cCode.DataBind.Alias, RowValue - 1, "");
                    dtCritaria.SetValue(cDescription.DataBind.Alias, RowValue - 1, "");
                    dtCritaria.SetValue(cMarks.DataBind.Alias, RowValue - 1, "0");
                    dtCritaria.SetValue(cMinMarks.DataBind.Alias, RowValue - 1, "0");
                    mtCritaria.AddRow(1, mtCritaria.RowCount + 1);
                }
            }
            mtCritaria.LoadFromDataSource();
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
                dtCritaria.Rows.Clear();
                mtCritaria.FlushToDataSource();
                String IsNew, Code, Description;
                Int32 Id = 0, ParentAssestmentID = 0;
                Decimal Marks = 0, MinMarks = 0;
                for (Int32 i = 0; i < dtCritaria.Rows.Count; i++)
                {
                    IsNew = Convert.ToString(dtCritaria.GetValue(cIsNew.DataBind.Alias, i));
                    Id = Convert.ToInt32(dtCritaria.GetValue(cID.DataBind.Alias, i));
                    Code = Convert.ToString(dtCritaria.GetValue(cCode.DataBind.Alias, i));
                    Description = Convert.ToString(dtCritaria.GetValue(cDescription.DataBind.Alias, i));
                    Marks = Convert.ToDecimal(dtCritaria.GetValue(cMarks.DataBind.Alias, i));
                    MinMarks = Convert.ToDecimal(dtCritaria.GetValue(cMinMarks.DataBind.Alias, i));
                    ParentAssestmentID = Convert.ToInt32(cbAssestment.Value.Trim());
                    MstAssestment ParentObject = (from a in dbHrPayroll.MstAssestment where a.ID == ParentAssestmentID select a).FirstOrDefault();
                    if (!String.IsNullOrEmpty(Code))
                    {
                        if (IsNew == "Y")
                        {
                            MstAssestmentCriteria oNew = new MstAssestmentCriteria();
                            oNew.Criteria = Code;
                            oNew.Description = Description;
                            oNew.Marks = Marks;
                            oNew.MinMarks = MinMarks;
                            oNew.CreateDt = DateTime.Now;
                            oNew.UserID = oCompany.UserName;
                            oNew.UpdateDt = DateTime.Now;
                            oNew.UpdatedBy = oCompany.UserName;
                            //dbHrPayroll.MstAssestmentCriteria.InsertOnSubmit(oNew);
                            ParentObject.MstAssestmentCriteria.Add(oNew);
                        }
                        else if (IsNew == "N")
                        {
                            MstAssestmentCriteria oOld = (from a in dbHrPayroll.MstAssestmentCriteria where a.ID == Id select a).FirstOrDefault();
                            if (oOld != null)
                            {
                                oOld.Criteria = Code;
                                oOld.Description = Description;
                                oOld.Marks = Marks;
                                oOld.MinMarks = MinMarks;
                                oOld.UpdatedBy = oCompany.UserName;
                                oOld.UpdateDt = DateTime.Now;
                            }
                        }
                        dbHrPayroll.SubmitChanges();
                    }
                }
                FillCritaria(cbAssestment.Selected.Value);
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
