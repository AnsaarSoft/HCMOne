using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_Skills:HRMSBaseForm
    {
        #region Variable

        SAPbouiCOM.Button btnMain;
        SAPbouiCOM.Matrix mtSkills;
        SAPbouiCOM.Item ibtnMain, imtSkills;
        SAPbouiCOM.DataTable dtSkills;
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

                mtSkills = oForm.Items.Item("mtmain").Specific;
                imtSkills = oForm.Items.Item("mtmain");
                dtSkills = oForm.DataSources.DataTables.Item("dtskill");
                cIsNew = mtSkills.Columns.Item("isnew");
                cIsNew.Visible = false;
                cID = mtSkills.Columns.Item("id");
                cID.Visible = false;
                cCode = mtSkills.Columns.Item("code");
                cCode.TitleObject.Sortable = false;
                cDescription = mtSkills.Columns.Item("desc");
                cDescription.TitleObject.Sortable = false;
                btnMain.Caption = "Ok";

                FillSkills();
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
                imtSkills.AffectsFormMode = true;
            }
            catch (Exception)
            {
            }
        }

        private void FillSkills()
        {
            try
            {
                IEnumerable<MstSkills> Skills = from a in dbHrPayroll.MstSkills select a;
                UInt16 i = 0;
                if (Skills.Count() == 0)
                {
                    return;
                }
                dtSkills.Rows.Clear();
                dtSkills.Rows.Add(Skills.Count());
                foreach (MstSkills Skill in Skills)
                {
                    dtSkills.SetValue(cIsNew.DataBind.Alias, i, "N");
                    dtSkills.SetValue(cID.DataBind.Alias, i, Skill.ID);
                    dtSkills.SetValue(cCode.DataBind.Alias, i, Skill.Code);
                    dtSkills.SetValue(cDescription.DataBind.Alias, i, Skill.Description);
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

            if (dtSkills.Rows.Count == 0)
            {
                dtSkills.Rows.Add(1);
                RowValue = dtSkills.Rows.Count;
                dtSkills.SetValue(cIsNew.DataBind.Alias, RowValue - 1, "Y");
                dtSkills.SetValue(cID.DataBind.Alias, RowValue - 1, "0");
                dtSkills.SetValue(cCode.DataBind.Alias, RowValue - 1, "");
                dtSkills.SetValue(cDescription.DataBind.Alias, RowValue - 1, "");
                mtSkills.AddRow(1, RowValue + 1);
            }
            else
            {
                if (dtSkills.GetValue(cCode.DataBind.Alias, dtSkills.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtSkills.Rows.Add(1);
                    RowValue = dtSkills.Rows.Count;
                    dtSkills.SetValue(cIsNew.DataBind.Alias, RowValue - 1, "Y");
                    dtSkills.SetValue(cID.DataBind.Alias, RowValue - 1, "0");
                    dtSkills.SetValue(cCode.DataBind.Alias, RowValue - 1, "");
                    dtSkills.SetValue(cDescription.DataBind.Alias, RowValue - 1, "");
                    mtSkills.AddRow(1, mtSkills.RowCount + 1);
                }
            }
            mtSkills.LoadFromDataSource();
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
                dtSkills.Rows.Clear();
                mtSkills.FlushToDataSource();
                String IsNew, Code, Description;
                Int32 Id = 0;
                for (Int32 i = 0; i < dtSkills.Rows.Count; i++)
                {
                    IsNew = Convert.ToString(dtSkills.GetValue(cIsNew.DataBind.Alias, i));
                    Id = Convert.ToInt32(dtSkills.GetValue(cID.DataBind.Alias, i));
                    Code = Convert.ToString(dtSkills.GetValue(cCode.DataBind.Alias, i));
                    Description = Convert.ToString(dtSkills.GetValue(cDescription.DataBind.Alias, i));
                    if (!String.IsNullOrEmpty(Code))
                    {
                        if (IsNew == "Y")
                        {
                            MstSkills oNew = new MstSkills();
                            oNew.Code = Code;
                            oNew.Description = Description;
                            oNew.CreateDate = DateTime.Now;
                            oNew.UserID = oCompany.UserName;
                            oNew.UpdateDate = DateTime.Now;
                            oNew.UpdatedBy = oCompany.UserName;
                            dbHrPayroll.MstSkills.InsertOnSubmit(oNew);
                        }
                        else if (IsNew == "N")
                        {
                            MstSkills oOld = (from a in dbHrPayroll.MstSkills where a.ID == Id select a).FirstOrDefault();
                            if (oOld != null)
                            {
                                oOld.Code = Code;
                                oOld.Description = Description;
                                oOld.UpdatedBy = oCompany.UserName;
                                oOld.UpdateDate = DateTime.Now;
                            }
                        }
                        dbHrPayroll.SubmitChanges();
                    }
                }
                FillSkills();
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
