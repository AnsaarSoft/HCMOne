using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_Competancy: HRMSBaseForm
    {
        #region Variable
        
        SAPbouiCOM.Button btnMain;
        SAPbouiCOM.Matrix mtCompetancy;
        SAPbouiCOM.Item ibtnMain, imtCompetancy;
        SAPbouiCOM.DataTable dtCompetancy;
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

                mtCompetancy = oForm.Items.Item("mtmain").Specific;
                imtCompetancy = oForm.Items.Item("mtmain");
                dtCompetancy = oForm.DataSources.DataTables.Item("dtcom");
                cIsNew = mtCompetancy.Columns.Item("isnew");
                cIsNew.Visible = false;
                cID = mtCompetancy.Columns.Item("id");
                cID.Visible = false;
                cCode = mtCompetancy.Columns.Item("code");
                cCode.TitleObject.Sortable = false;
                cDescription = mtCompetancy.Columns.Item("desc");
                cDescription.TitleObject.Sortable = false;
                btnMain.Caption = "Ok";

                FillCompetency();
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
                imtCompetancy.AffectsFormMode = true;
            }
            catch (Exception)
            {
            }
        }

        private void FillCompetency()
        {
            try
            {
                //IEnumerable<MstCompetancy> Competancies = from a in dbHrPayroll.MstCompetancy select a;
                //UInt16 i = 0;
                //if (Competancies.Count() == 0)
                //{
                //    return;
                //}
                //dtCompetancy.Rows.Clear();
                //dtCompetancy.Rows.Add(Competancies.Count());
                //foreach (MstCompetancy Competancy in Competancies)
                //{
                //    dtCompetancy.SetValue(cIsNew.DataBind.Alias, i, "N");
                //    dtCompetancy.SetValue(cID.DataBind.Alias, i, Competancy.ID);
                //    dtCompetancy.SetValue(cCode.DataBind.Alias, i, Competancy.Code);
                //    dtCompetancy.SetValue(cDescription.DataBind.Alias, i, Competancy.Description);
                //    i++;
                //}

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("All Data can't load successfully" + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void AddEmptyRow()
        {
            Int32 RowValue = 0;

            if (dtCompetancy.Rows.Count == 0)
            {
                dtCompetancy.Rows.Add(1);
                RowValue = dtCompetancy.Rows.Count;
                dtCompetancy.SetValue(cIsNew.DataBind.Alias, RowValue - 1, "Y");
                dtCompetancy.SetValue(cID.DataBind.Alias, RowValue - 1, "0");
                dtCompetancy.SetValue(cCode.DataBind.Alias, RowValue - 1, "");
                dtCompetancy.SetValue(cDescription.DataBind.Alias, RowValue - 1, "");
                mtCompetancy.AddRow(1, RowValue + 1);
            }
            else
            {
                if (dtCompetancy.GetValue(cCode.DataBind.Alias, dtCompetancy.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtCompetancy.Rows.Add(1);
                    RowValue = dtCompetancy.Rows.Count;
                    dtCompetancy.SetValue(cIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtCompetancy.SetValue(cID.DataBind.Alias, RowValue - 1, "0");
                    dtCompetancy.SetValue(cCode.DataBind.Alias, RowValue - 1, "");
                    dtCompetancy.SetValue(cDescription.DataBind.Alias, RowValue - 1, "");
                    mtCompetancy.AddRow(1, mtCompetancy.RowCount + 1);
                }
            }
            mtCompetancy.LoadFromDataSource();
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
                //dtCompetancy.Rows.Clear();
                //mtCompetancy.FlushToDataSource();
                //String IsNew, Code, Description;
                //Int32 Id = 0;
                //for (Int32 i = 0; i < dtCompetancy.Rows.Count; i++)
                //{
                //    IsNew = Convert.ToString(dtCompetancy.GetValue(cIsNew.DataBind.Alias, i));
                //    Id = Convert.ToInt32(dtCompetancy.GetValue(cID.DataBind.Alias, i));
                //    Code = Convert.ToString(dtCompetancy.GetValue(cCode.DataBind.Alias, i));
                //    Description = Convert.ToString(dtCompetancy.GetValue(cDescription.DataBind.Alias, i));
                //    if (!String.IsNullOrEmpty(Code))
                //    {
                //        if (IsNew == "Y")
                //        {
                //            MstCompetancy oNew = new MstCompetancy();
                //            oNew.Code = Code;
                //            oNew.Description = Description;
                //            oNew.CreateDate = DateTime.Now;
                //            oNew.UserID = oCompany.UserName;
                //            oNew.UpdateDate = DateTime.Now;
                //            oNew.UpdatedBy = oCompany.UserName;
                //            dbHrPayroll.MstCompetancy.InsertOnSubmit(oNew);
                //        }
                //        else if (IsNew == "N")
                //        {
                //            MstCompetancy oOld = (from a in dbHrPayroll.MstCompetancy where a.ID == Id select a).FirstOrDefault();
                //            if (oOld != null)
                //            {
                //                oOld.Code = Code;
                //                oOld.Description = Description;
                //                oOld.UpdatedBy = oCompany.UserName;
                //                oOld.UpdateDate = DateTime.Now;
                //            }
                //        }
                //        dbHrPayroll.SubmitChanges();
                //    }
                //}
                //FillCompetency();
                //AddEmptyRow();
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
