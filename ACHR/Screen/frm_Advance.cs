using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    class frm_Advance:HRMSBaseForm
    {

        #region Variable
        
        /* Form Items Objects */
        SAPbouiCOM.Matrix mtAdv;
        SAPbouiCOM.Column coAdvCode, coAdvDesc, coAdvAct, coAdvDef,isNew, id;
        private SAPbouiCOM.DataTable dtAdvance;

        SAPbouiCOM.Item ImtAdv, IcoAdvCode, IcoAdvDesc, IcoAdvAct;
        //**********************************

        public IEnumerable<MstAdvance> advances;

        #endregion

        #region B1 Events

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            oForm.EnableMenu("1282", false); //Add Disable
            oForm.EnableMenu("1281", false); //Find Disable
            oForm.EnableMenu("1290", false); //First Record Disable
            oForm.EnableMenu("1289", false); //Previos Record Disable
            oForm.EnableMenu("1288", false); //Next Record Disable
            oForm.EnableMenu("1291", false); //Last Record Disable
            InitiallizeForm();
            oForm.Freeze(false);

        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    updateDbWithMat();
                    break;
            }
        }

        public override void etBeforeClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE || oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE)
                    {
                        if (!ValidateForm())
                        {
                            BubbleEvent = false;
                        }
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

            mtAdv = oForm.Items.Item("mtAdv").Specific;
            isNew = mtAdv.Columns.Item("isNew");
            id = mtAdv.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;

            coAdvCode = mtAdv.Columns.Item("coAdvCode");
            coAdvDesc = mtAdv.Columns.Item("coAdvDesc");
            coAdvAct = mtAdv.Columns.Item("coAdvAct");
            coAdvDef = mtAdv.Columns.Item("coAdvDef");

            dtAdvance = oForm.DataSources.DataTables.Item("dtAdv");
            dtAdvance.Rows.Clear();
            fillMat();                      
            oForm.Freeze(false);

        }
        
        private void fillMat()
        {
            dtAdvance.Rows.Clear();
            advances = from p in dbHrPayroll.MstAdvance select p;
            dtAdvance.Rows.Clear();
            dtAdvance.Rows.Add(advances.Count());
            int i = 0;
            foreach (MstAdvance adv in advances)
            {
                dtAdvance.SetValue("isNew", i, "N");
                dtAdvance.SetValue("id", i, adv.Id);
                dtAdvance.SetValue("advCode", i, adv.AllowanceId.ToString());
                dtAdvance.SetValue("Desc", i, adv.Description.ToString());
                dtAdvance.SetValue("Active", i, adv.FlgActive == true ? "Y" : "N");
                if (adv.FlgDefault != null)
                {
                    dtAdvance.SetValue("Deflt", i, adv.FlgDefault == true ? "Y" : "N");
                }
                else
                {
                    dtAdvance.SetValue("Deflt", i, "N");
                }

                i++;

            }
            addEmptyRow();
            
           
           
        }
        
        private void addEmptyRow()
        {


            if (dtAdvance.Rows.Count == 0)
            {
                dtAdvance.Rows.Add(1);
                dtAdvance.SetValue("isNew", 0, "Y");
                dtAdvance.SetValue("id", 0, 0);
                dtAdvance.SetValue("advCode", 0, "");
                dtAdvance.SetValue("Desc", 0, "");
                dtAdvance.SetValue("Active", 0, "N");
                dtAdvance.SetValue("Deflt", 0, "N");
                mtAdv.AddRow(1, mtAdv.RowCount + 1);
            }
            else
            {
                if (dtAdvance.GetValue("advCode", dtAdvance.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtAdvance.Rows.Add(1);
                    dtAdvance.SetValue("isNew", dtAdvance.Rows.Count - 1, "Y");
                    dtAdvance.SetValue("advCode", dtAdvance.Rows.Count - 1, "");
                    dtAdvance.SetValue("Desc", dtAdvance.Rows.Count - 1, "");
                    dtAdvance.SetValue("Active", dtAdvance.Rows.Count - 1, "N");
                    dtAdvance.SetValue("Deflt", dtAdvance.Rows.Count - 1, "N");
                    mtAdv.AddRow(1, mtAdv.RowCount + 1);
                }

            }
            mtAdv.LoadFromDataSource();
        }
        
        private void updateDbWithMat()
        {
            try
            {
                //oDtAdv.Rows.Clear();
                mtAdv.FlushToDataSource();
                string id = "";
                string code = "";
                string isnew = "";
                for (int i = 0; i < dtAdvance.Rows.Count; i++)
                {
                    code = Convert.ToString(dtAdvance.GetValue("advCode", i));
                    isnew = Convert.ToString(dtAdvance.GetValue("isNew", i));
                    isnew = isnew.Trim();
                    code = code.Trim();
                    if (code != "")
                    {
                        MstAdvance objAdv;
                        id = Convert.ToString(dtAdvance.GetValue("id", i));
                        if (isnew == "Y")
                        {
                            objAdv = new MstAdvance();
                            dbHrPayroll.MstAdvance.InsertOnSubmit(objAdv);
                            objAdv.UserId = oCompany.UserName;
                            objAdv.CreateDate = DateTime.Now;
                            var OoldChehkCode = dbHrPayroll.MstAdvance.Where(a => a.AllowanceId == code).FirstOrDefault();
                            if (OoldChehkCode != null)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            objAdv = (from p in dbHrPayroll.MstAdvance where p.Id.ToString() == id.Trim() select p).Single();
                        }
                        objAdv.AllowanceId = code;
                        objAdv.Description = dtAdvance.GetValue("Desc", i);
                        objAdv.FlgActive = Convert.ToString(dtAdvance.GetValue("Active", i)) == "Y" ? true : false;
                        objAdv.FlgDefault = Convert.ToString(dtAdvance.GetValue("Deflt", i)) == "Y" ? true : false;
                        
                        objAdv.UpdateDate = DateTime.Now;
                        objAdv.UpdatedBy = oCompany.UserName;
                    }
                }
                dbHrPayroll.SubmitChanges();
                addEmptyRow();
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);
            }
        }

        private Boolean ValidateForm()
        {
            try
            {
                //chek loan activity.
                mtAdv.FlushToDataSource();
                for (int i = 0; i < dtAdvance.Rows.Count; i++)
                {
                    string advancecode = dtAdvance.GetValue(coAdvCode.DataBind.Alias, i);
                    string advancestatus = dtAdvance.GetValue(coAdvAct.DataBind.Alias, i);
                    string isnewcheck = dtAdvance.GetValue(isNew.DataBind.Alias, i);
                    if (!string.IsNullOrEmpty(advancecode) && !string.IsNullOrEmpty(advancestatus))
                    {
                        Boolean flgActive = false;
                        if (advancestatus.Trim().ToLower() == "y")
                        {
                            flgActive = true;
                        }
                        else
                        {
                            flgActive = false;
                        }
                        //for active advance types can't deactivate.
                        var ocheck = (from a in dbHrPayroll.TrnsAdvance where a.MstAdvance.AllowanceId == advancecode.Trim() select a).Count();
                        var odata = (from a in dbHrPayroll.MstAdvance where a.AllowanceId == advancecode select a).FirstOrDefault();
                        if (ocheck > 0 && flgActive != Convert.ToBoolean(odata.FlgActive))
                        {
                            oApplication.StatusBar.SetText("Advance Type in use can't deactivate. Line : " + (i + 1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        //for duplicate code
                        var codecheck = (from a in dbHrPayroll.MstAdvance where a.AllowanceId == advancecode select a).Count();
                        if (codecheck > 0 && isnewcheck.Trim().ToLower() == "y")
                        {
                            oApplication.StatusBar.SetText("Duplication of code not allowed. Line : " + (i + 1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        else if (codecheck > 1 && isnewcheck.Trim().ToLower() == "n")
                        {
                            oApplication.StatusBar.SetText("Duplication of code not allowed. Line : " + (i + 1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

    }
}
