
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    partial class frm_Loan:HRMSBaseForm
    {
        #region Variables
        
        /* Form Items Objects */
        SAPbouiCOM.Matrix mtLoan;
        SAPbouiCOM.Column coCode, coDesc, coMarkup, coActive,isNew, id;
        private SAPbouiCOM.DataTable dtLoan;

        SAPbouiCOM.Item ImtAdv, IcoAdvCode, IcoAdvDesc, IcoAdvAct;
        //**********************************

        public IEnumerable<MstLoans> loan;

        #endregion

        #region SAP B1 Events

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

            mtLoan = oForm.Items.Item("mtLoan").Specific;
            isNew = mtLoan.Columns.Item("isNew");
            id = mtLoan.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;
            coCode = mtLoan.Columns.Item("coCode");
            coDesc = mtLoan.Columns.Item("coDesc");
            coMarkup = mtLoan.Columns.Item("coMarkup");
            coMarkup.Visible = false;
            coActive = mtLoan.Columns.Item("coActive");

            dtLoan = oForm.DataSources.DataTables.Item("dtLoan");
            dtLoan.Rows.Clear();
            fillMat();

            mtLoan.AutoResizeColumns();
            oForm.Freeze(false);

        }
        
        private void fillMat()
        {
            dtLoan.Rows.Clear();
            loan = from p in dbHrPayroll.MstLoans select p;
            dtLoan.Rows.Clear();
            dtLoan.Rows.Add(loan.Count());
            int i = 0;
            foreach (MstLoans lon in loan)
            {
                dtLoan.SetValue("isNew", i, "N");
                dtLoan.SetValue("id", i, lon.Id);
                dtLoan.SetValue("LoanCode", i, lon.Code.ToString());
                dtLoan.SetValue("Descr", i, lon.Description.ToString());               
                dtLoan.SetValue("Active", i, lon.FlgActive == true ? "Y" : "N");

                i++;

            }
            addEmptyRow();
            
            mtLoan.LoadFromDataSource();
           
        }
        
        private void addEmptyRow()
        {


            if (dtLoan.Rows.Count == 0)
            {
                dtLoan.Rows.Add(1);
                
                dtLoan.SetValue("isNew", 0, "Y");
                dtLoan.SetValue("id", 0, 0);
                dtLoan.SetValue("LoanCode", 0, "");
                dtLoan.SetValue("Descr", 0, "");
                dtLoan.SetValue("Markup", 0, "0.00");
                dtLoan.SetValue("Active", 0, "N");
                mtLoan.AddRow(1, mtLoan.RowCount + 1);
            }
            else
            {
                if (dtLoan.GetValue("LoanCode", dtLoan.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtLoan.Rows.Add(1);
                    dtLoan.SetValue("isNew", dtLoan.Rows.Count - 1, "Y");
                    dtLoan.SetValue("id", dtLoan.Rows.Count - 1, 0);
                    dtLoan.SetValue("LoanCode", dtLoan.Rows.Count - 1, "");
                    dtLoan.SetValue("Descr", dtLoan.Rows.Count - 1, "");
                    dtLoan.SetValue("Markup", dtLoan.Rows.Count - 1, "0.00");

                    dtLoan.SetValue("Active", dtLoan.Rows.Count - 1, "N");
                    mtLoan.AddRow(1, mtLoan.RowCount + 1);
                }

            }
            mtLoan.LoadFromDataSource();
           
        }
        
        private void updateDbWithMat()
        {
            try
            {
                mtLoan.FlushToDataSource();
                string id = "";
                string code = "";
                string isnew = "";
                for (int i = 0; i < dtLoan.Rows.Count; i++)
                {
                    code = Convert.ToString(dtLoan.GetValue("LoanCode", i));
                    isnew = Convert.ToString(dtLoan.GetValue("isNew", i));
                    isnew = isnew.Trim();
                    code = code.Trim();
                    if (code != "")
                    {
                        MstLoans objLoan;
                        id = Convert.ToString(dtLoan.GetValue("id", i));
                        if (isnew == "Y")
                        {
                            objLoan = new MstLoans();
                            dbHrPayroll.MstLoans.InsertOnSubmit(objLoan);
                            objLoan.CreateDate = DateTime.Now;
                            objLoan.UserId = oCompany.UserName;
                            var OoldChehkCode = dbHrPayroll.MstLoans.Where(a => a.Code == code).FirstOrDefault();
                            if (OoldChehkCode != null)
                            {
                                oApplication.StatusBar.SetText(code + " code already Exist.Please Provide valid Code for Record # " + i.ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                return;
                            }
                        }
                        else
                        {
                            objLoan = (from p in dbHrPayroll.MstLoans where p.Id.ToString() == id.Trim() select p).Single();
                        }
                        objLoan.Code = code;
                        objLoan.Description = dtLoan.GetValue("Descr", i);                      
                        objLoan.FlgActive = Convert.ToString(dtLoan.GetValue("Active", i)) == "Y" ? true : false;
                        
                        objLoan.UpdateDate = DateTime.Now;
                        objLoan.UpdatedBy = oCompany.UserName;
                    }
                }
                dbHrPayroll.SubmitChanges();
                addEmptyRow();
            }
            catch (Exception Ex)
            {
                oApplication.SetStatusBarMessage(Ex.Message);
            }
        }

        private Boolean ValidateForm()
        {
            try
            {
                //chek loan activity.
                mtLoan.FlushToDataSource();
                for (int i = 0; i < dtLoan.Rows.Count; i++)
                {
                    string loancode = dtLoan.GetValue(coCode.DataBind.Alias, i);
                    string loanstatus = dtLoan.GetValue(coActive.DataBind.Alias, i);
                    string isnewcheck = dtLoan.GetValue(isNew.DataBind.Alias, i);
                    if (!string.IsNullOrEmpty(loancode) && !string.IsNullOrEmpty(loanstatus))
                    {
                        Boolean flgActive = false;
                        if (loanstatus.Trim().ToLower() == "y")
                        {
                            flgActive = true;
                        }
                        else
                        {
                            flgActive = false;
                        }
                        //for active loan types can't deactivate.
                        var ocheck = (from a in dbHrPayroll.TrnsLoanDetail where a.MstLoans.Code == loancode.Trim() select a).Count();
                        var odata = (from a in dbHrPayroll.MstLoans where a.Code == loancode select a).FirstOrDefault();
                        if (ocheck > 0 && flgActive != Convert.ToBoolean(odata.FlgActive))
                        {
                            oApplication.StatusBar.SetText("Loan Type in use can't deactivate. Line : " + (i+1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        //for duplicate code
                        var codecheck = (from a in dbHrPayroll.MstLoans where a.Code == loancode select a).Count();
                        if (codecheck > 0 && isnewcheck.Trim().ToLower() == "y")
                        {
                            oApplication.StatusBar.SetText("Duplication of code not allowed. Line : " + (i+1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
                            return false;
                        }
                        else if (codecheck > 1 && isnewcheck.Trim().ToLower() == "n")
                        {
                            oApplication.StatusBar.SetText("Duplication of code not allowed. Line : " + (i+1).ToString(), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Warning);
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
