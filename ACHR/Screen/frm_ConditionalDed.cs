using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
namespace ACHR.Screen
{
    class frm_ConditionalDed:HRMSBaseForm
    {

        #region Variables
        SAPbouiCOM.Matrix oMat;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo, clID, clleaveCount, clNonDeductableLeaveType, clDeductableLeaveType, clPeriorty, clActive, isNew, id, clLevType;
        private SAPbouiCOM.DataTable dtLeaveConditionalDeduction;

        public IEnumerable<MstLeaveConditionalDeduction> LeaveType;
        #endregion

        #region B1 Events
        
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
                case "1":
                    AddUpdateRecord();
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
                        if (!ValidateRecord())
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

            oMat = oForm.Items.Item("mtLCD").Specific;
            isNew = oMat.Columns.Item("isNew");
            id = oMat.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;


            clleaveCount = oMat.Columns.Item("LC");

            clNonDeductableLeaveType = oMat.Columns.Item("LT");
            clDeductableLeaveType = oMat.Columns.Item("LDT");
            clActive = oMat.Columns.Item("Active");
            clPeriorty = oMat.Columns.Item("Periorty");
            dtLeaveConditionalDeduction = oForm.DataSources.DataTables.Item("dtLCD");
            dtLeaveConditionalDeduction.Rows.Clear();
           
            
            FillNonDeductableLeave(clNonDeductableLeaveType);
            FillDeductableLeave(clDeductableLeaveType);


            fillMat();


            oForm.Freeze(false);


        }
        
        private void fillMat()
        {

            dtLeaveConditionalDeduction.Rows.Clear();
            LeaveType = from p in dbHrPayroll.MstLeaveConditionalDeduction select p;
            dtLeaveConditionalDeduction.Rows.Clear();
            dtLeaveConditionalDeduction.Rows.Add(LeaveType.Count());
            int i = 0;
            foreach (MstLeaveConditionalDeduction LType in LeaveType)
            {
                dtLeaveConditionalDeduction.SetValue("isNew", i, "N");
                dtLeaveConditionalDeduction.SetValue("id", i, LType.ID);
                dtLeaveConditionalDeduction.SetValue("LC", i, LType.LeaveCount.ToString());
                dtLeaveConditionalDeduction.SetValue("LT", i, LType.NonDeductableLeave.ToString());
                dtLeaveConditionalDeduction.SetValue("LDT", i, LType.DeductableLeave.ToString());

                if (LType.Periorty != null)
                {
                    dtLeaveConditionalDeduction.SetValue("Periorty", i, LType.Periorty.ToString());
                }
                else
                {
                    dtLeaveConditionalDeduction.SetValue("Periorty", i, 0);
                }
                dtLeaveConditionalDeduction.SetValue("Active", i, LType.FlgActive == true ? "Y" : "N");

                i++;

            }
            addEmptyRow();

            oMat.LoadFromDataSource();

        }
        
        private void addEmptyRow()
        {


            if (dtLeaveConditionalDeduction.Rows.Count == 0)
            {
                dtLeaveConditionalDeduction.Rows.Add(1);

                dtLeaveConditionalDeduction.SetValue("isNew", 0, "Y");
                dtLeaveConditionalDeduction.SetValue("id", 0, 0);
                dtLeaveConditionalDeduction.SetValue("LC", 0, 0);
                dtLeaveConditionalDeduction.SetValue("LT", 0, "");
                dtLeaveConditionalDeduction.SetValue("LDT", 0, "");

                dtLeaveConditionalDeduction.SetValue("Periorty", 0, 0);               
                dtLeaveConditionalDeduction.SetValue("Active", 0, "N");
               


                oMat.AddRow(1, oMat.RowCount + 1);
            }
            else
            {
                if (dtLeaveConditionalDeduction.GetValue("LT", dtLeaveConditionalDeduction.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtLeaveConditionalDeduction.Rows.Add(1);
                    dtLeaveConditionalDeduction.SetValue("isNew", dtLeaveConditionalDeduction.Rows.Count - 1, "Y");
                    dtLeaveConditionalDeduction.SetValue("id", dtLeaveConditionalDeduction.Rows.Count - 1, 0);
                    dtLeaveConditionalDeduction.SetValue("LC", dtLeaveConditionalDeduction.Rows.Count - 1, "0");
                    dtLeaveConditionalDeduction.SetValue("LT", dtLeaveConditionalDeduction.Rows.Count - 1, "");
                    dtLeaveConditionalDeduction.SetValue("LDT", dtLeaveConditionalDeduction.Rows.Count - 1, "");
                    
                    dtLeaveConditionalDeduction.SetValue("Periorty", dtLeaveConditionalDeduction.Rows.Count - 1, 0);
                   
                   
                    dtLeaveConditionalDeduction.SetValue("Active", dtLeaveConditionalDeduction.Rows.Count - 1, "N");
                   
                    oMat.AddRow(1, oMat.RowCount + 1);
                }

            }
            oMat.LoadFromDataSource();

        }
  
        private void FillNonDeductableLeave(SAPbouiCOM.Column Pcombo)
        {
            try
            {
                var oCollection = (from a in dbHrPayroll.MstLeaveType where a.LeaveType == "NonDed" && a.Active == true select a).ToList();
                //Pcombo.ValidValues.Add(Convert.ToString(0), Convert.ToString("Non Deduction"));
                if (oCollection.Count > 0)
                { 
                    foreach(var oneline in oCollection)
                    {
                        Pcombo.ValidValues.Add(oneline.Code, oneline.Description);

                    }
                }
            }
            catch (Exception ex)
            {

                oApplication.StatusBar.SetText("FillcolumnCombo : "+ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillDeductableLeave(SAPbouiCOM.Column Pcombo)
        {
            try
            {
                var oCollection = (from a in dbHrPayroll.MstLeaveType where a.LeaveType == "Ded" && a.Active == true select a).ToList();
                Pcombo.ValidValues.Add(Convert.ToString(0), Convert.ToString("No Deduction"));
                if (oCollection.Count > 0)
                {
                    foreach (var oneline in oCollection)
                    {
                        Pcombo.ValidValues.Add(oneline.Code, oneline.Description);

                    }
                }
            }
            catch (Exception ex)
            {

                oApplication.StatusBar.SetText("FillcolumnCombo : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void AddUpdateRecord()
        {
            oMat.FlushToDataSource();
            string id = "";
            string LType = "";
            string isnew = "";
            string NonDeductableLeave = "";
            string DeductableLeave = "";
            for (int i = 0; i < dtLeaveConditionalDeduction.Rows.Count; i++)
            {
                LType = Convert.ToString(dtLeaveConditionalDeduction.GetValue("LT", i));
                isnew = Convert.ToString(dtLeaveConditionalDeduction.GetValue("isNew", i));
                NonDeductableLeave = Convert.ToString(dtLeaveConditionalDeduction.GetValue("LT", i));
                DeductableLeave = Convert.ToString(dtLeaveConditionalDeduction.GetValue("LDT", i));
                isnew = isnew.Trim();
                LType = LType.Trim();
                if (LType != "")
                {
                    MstLeaveConditionalDeduction obj;
                    id = Convert.ToString(dtLeaveConditionalDeduction.GetValue("id", i));
                    if (isnew == "Y")
                    {
                        obj = new MstLeaveConditionalDeduction();
                        dbHrPayroll.MstLeaveConditionalDeduction.InsertOnSubmit(obj);
                    }
                    else
                    {
                        obj = (from p in dbHrPayroll.MstLeaveConditionalDeduction where p.ID.ToString() == id.Trim() select p).Single();
                    }
                    string va = Convert.ToString(dtLeaveConditionalDeduction.GetValue("LC", i));
                    obj.LeaveCount = Convert.ToInt32(va);


                    obj.NonDeductableLeave = dtLeaveConditionalDeduction.GetValue("LT", i);
                    obj.NonDeductableLeave= NonDeductableLeave; //Convert.ToString( dtOT.GetValue("ValType", i));
                    obj.DeductableLeave = dtLeaveConditionalDeduction.GetValue("LDT", i);
                    obj.DeductableLeave = DeductableLeave;

                    obj.Periorty = Convert.ToInt32(dtLeaveConditionalDeduction.GetValue("Periorty", i));
                    
                    obj.FlgActive = Convert.ToString(dtLeaveConditionalDeduction.GetValue("Active", i)) == "Y" ? true : false;
                    
                    obj.CreatedDate = DateTime.Now;
                    obj.CreatedBy = oCompany.UserName;


                    obj.UpdatedDate = DateTime.Now;
                    obj.UpdatedBy = oCompany.UserName;

                }
            }
            dbHrPayroll.SubmitChanges();
            fillMat();
            //addEmptyRow();
        }

        private bool ValidateRecord()
        {
            try
            {
                string strid, strisnew, strisactive;
                oMat.FlushToDataSource();
                for (int i = 0; i < dtLeaveConditionalDeduction.Rows.Count; i++)
                {
                    strid = Convert.ToString(dtLeaveConditionalDeduction.GetValue(id.DataBind.Alias, i));
                    strisnew = Convert.ToString(dtLeaveConditionalDeduction.GetValue(isNew.DataBind.Alias, i));
                    strisactive = Convert.ToString(dtLeaveConditionalDeduction.GetValue(clActive.DataBind.Alias, i));
                    var oRecord = (from a in dbHrPayroll.MstLeaveConditionalDeduction where a.ID.ToString() == strid select a).FirstOrDefault();
                    if (oRecord == null) continue;
                    bool flgActive = false;
                    if (strisactive.Trim().ToLower() == "y")
                    {
                        flgActive = true;
                    }
                    if (flgActive != Convert.ToBoolean(oRecord.FlgActive) && strisnew.Trim().ToLower() == "n")
                    {
                        int result = oApplication.MessageBox("Are you sure! you want to toggle condition status.", 1, "Yes", "No");
                        if (result == 2)
                        {
                            oApplication.StatusBar.SetText("Action canceled by user.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
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
