
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;

namespace ACHR.Screen
{
    partial class frm_ApprStage:HRMSBaseForm
    {


        /* Form Items Objects */
        SAPbouiCOM.Matrix mtAuth;
        SAPbouiCOM.EditText txName, txDesc, txReqApp, txReqRej, txId;
        SAPbouiCOM.Column cUsr, cDept, isNew, id;
        private SAPbouiCOM.DataTable dtAuth;

        SAPbouiCOM.Item ItxName, ItxDesc, ItxReqApp, ItxReqRej, IcmdPrev, IcmdNext, IcmdNew, ItxId;
        //**********************************
        public IEnumerable<CfgApprovalStage> appStages;
        

        private void IniContrls()
        {
            txDesc.Value = "";
            txName.Value = "";
            txReqApp.Value = "";
            txReqRej.Value = "";
            
            txName.Active = true;
            dtAuth.Rows.Clear();
            mtAuth.LoadFromDataSource();
            
            

        }
        private void getData()
        {
            CodeIndex.Clear();
            appStages = from p in dbHrPayroll.CfgApprovalStage select p;
            int i = 0;
            foreach (CfgApprovalStage ele in appStages)
            {
                CodeIndex.Add(ele.ID.ToString(), i);

                i++;
            }
            totalRecord = i;
        }
        
        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            oForm.Freeze(true);
            InitiallizeForm();
            oForm.Freeze(false);
            oForm.DefButton = "1";
        }
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

            mtAuth = oForm.Items.Item("mtAuth").Specific;
            isNew = mtAuth.Columns.Item("isNew");
            id = mtAuth.Columns.Item("id");
            isNew.Visible = false;
            id.Visible = false;
            dtAuth = oForm.DataSources.DataTables.Item("dtAuth");
            dtAuth.Rows.Clear();
            
            oForm.DataSources.UserDataSources.Add("txName", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            txName = oForm.Items.Item("txName").Specific;
            ItxName = oForm.Items.Item("txName");
            txName.DataBind.SetBound(true, "", "txName");

            oForm.DataSources.UserDataSources.Add("txDesc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txDesc = oForm.Items.Item("txDesc").Specific;
            ItxDesc = oForm.Items.Item("txDesc");
            txDesc.DataBind.SetBound(true, "", "txDesc");

            oForm.DataSources.UserDataSources.Add("txReqApp", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER); // Days of Month
            txReqApp = oForm.Items.Item("txReqApp").Specific;
            ItxReqApp = oForm.Items.Item("txReqApp");
            txReqApp.DataBind.SetBound(true, "", "txReqApp");

            oForm.DataSources.UserDataSources.Add("txId", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER); // Days of Month
            txId = oForm.Items.Item("txId").Specific;
            ItxId = oForm.Items.Item("txId");
            txId.DataBind.SetBound(true, "", "txId");

            oForm.DataSources.UserDataSources.Add("txReqRej", SAPbouiCOM.BoDataType.dt_SHORT_NUMBER); // Days of Month
            txReqRej = oForm.Items.Item("txReqRej").Specific;
            ItxReqRej = oForm.Items.Item("txReqRej");
            txReqRej.DataBind.SetBound(true, "", "txReqRej");
            getData();

            ItxId.Visible = false;
            // fillMat();
           
            IniContrls();
            oForm.Freeze(false);
            AddNewRecord();

        }
        public override void AddNewRecord()
        {
            base.AddNewRecord();
            addNew();
        }
        public override void etAfterCfl(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterCfl(ref pVal, ref BubbleEvent);
            oApplication.SetStatusBarMessage("selected fms");
        }
        private void fillMat(int stageId )
        {
            IEnumerable<CfgApprovalStageDetail> apprDetail;
            dtAuth.Rows.Clear();
            apprDetail = from p in dbHrPayroll.CfgApprovalStageDetail where p.ASID.ToString()==stageId.ToString() select p;
            dtAuth.Rows.Clear();
            int i = 0;
            foreach (CfgApprovalStageDetail sd in apprDetail)
            {
                dtAuth.Rows.Add(1);
                dtAuth.SetValue("isNew", i, "N");
                setDept(sd.AuthorizerName, i);
               // dtAuth.SetValue("Dept", i, sd.);

                i++;

            }
            addEmptyRow();

            mtAuth.LoadFromDataSource();
           
        }
        public override void etAfterValidate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterValidate(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "mtAuth":
                    int rowNum = pVal.Row;
                    SAPbouiCOM.EditText oitm = mtAuth.GetCellSpecific("cUsr", rowNum);
                    string strUserCode = oitm.Value;
                    setDept(strUserCode, rowNum - 1);
                    break;

            }
        }

        public void setDept(string usrCode, int i)
        {
            //MstUsers usr = (from p in dbHrPayroll.MstUsers where p.UserCode == usrCode select p).Single();
            //dtAuth.SetValue("id", i, usr.ID);
            dtAuth.SetValue("username", i, usrCode);
            dtAuth.SetValue("pick", i, strCfl);
            //dtAuth.SetValue("Dept", i, usr.MstEmployee.MstDepartment.DeptName);
            // mtAuth.SetLineData(i+1 );
            if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_OK_MODE) oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
            addEmptyRow();

        }
        public void setDept(string usrCode,string username, int i)
        {
            //MstUsers usr = (from p in dbHrPayroll.MstUsers where p.UserCode == usrCode select p).Single();
            dtAuth.SetValue("id", i, usrCode);
            dtAuth.SetValue("username", i, username);
            dtAuth.SetValue("pick", i, strCfl);
            //dtAuth.SetValue("Dept", i, usr.MstEmployee.MstDepartment.DeptName);
            // mtAuth.SetLineData(i+1 );
            if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_OK_MODE) oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
            addEmptyRow();

        }
        private void addEmptyRow()
        {


            if (dtAuth.Rows.Count == 0)
            {
                dtAuth.Rows.Add(1);
                
                dtAuth.SetValue("isNew", 0, "Y");
                dtAuth.SetValue("id", 0, 0);
                dtAuth.SetValue("username", 0, "");
                dtAuth.SetValue("pick", 0, strCfl);
                //dtAuth.SetValue("Dept", 0, "");
                mtAuth.AddRow(1, mtAuth.RowCount + 1);
            }
            else
            {
                if (dtAuth.GetValue("username", dtAuth.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtAuth.Rows.Add(1);
                    dtAuth.SetValue("isNew", dtAuth.Rows.Count - 1, "Y");
                    dtAuth.SetValue("id", dtAuth.Rows.Count - 1, 0);
                    dtAuth.SetValue("username", dtAuth.Rows.Count - 1, "");
                    dtAuth.SetValue("pick", dtAuth.Rows.Count - 1,strCfl);
                    //dtAuth.SetValue("Dept", dtAuth.Rows.Count - 1, "");
                    mtAuth.AddRow(1, mtAuth.RowCount + 1);
                }

            }
           // mtAdv.FlushToDataSource();
           mtAuth.LoadFromDataSource();
           
        }
        private void addNew()
        {
            ItxName.Enabled = true;

            IniContrls();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            addEmptyRow();
        }
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    doSubmit();
                    break;

                case "mtAuth":
                    if (pVal.ColUID == "pick")
                    {
                        int rowNum = pVal.Row;
                        if (rowNum <= dtAuth.Rows.Count)
                        {
                            pickuser(rowNum);
                        }
                    }
                    break;
            }
        }
        private void pickuser(int rowNum)
        {
            string strSql = "select USERID,user_code , U_NAME from " + oCompany.CompanyDB + ".dbo.ousr";
            
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select User", "Select User for User Authorization");
            pic = null;
            if (st.Rows.Count > 0)
            {
                string strUserId = st.Rows[0][0].ToString();
                string strUserName = st.Rows[0][1].ToString();
                setDept(strUserId, strUserName, rowNum - 1);
            }

            //picker pic = new picker(oApplication, ds.getDataTable(sqlString.getSql("authourizer", SearchKeyVal)));
           
            //System.Data.DataTable st = pic.ShowInput("Select Authorizor", "Select Authorizer");
            //pic = null;
            //if (st.Rows.Count > 0)
            //{
            //    string strUserId = st.Rows[0][0].ToString();
            //    string elementName = st.Rows[0][0].ToString();
            //    setDept(strUserId, rowNum - 1);

             
            //}
        }
        public override void fillFields()
        {
            base.fillFields();
            _fillFields();
        }
        private void _fillFields()
        {
            oForm.Freeze(true);
            try
            {
                if (currentRecord >= 0)
                {
                    //txDescr.Active = true;
                    txDesc.Active = true;
                    ItxName.Enabled = false;

                    CfgApprovalStage record = appStages.ElementAt<CfgApprovalStage>(currentRecord);
                    txId.Value = record.ID.ToString();
                    txName.Value = record.StageName.ToString();
                    txDesc.Value = record.StageDescription;
                    txReqApp.Value = record.ApprovalsNo.ToString();
                    txReqRej.Value = record.RejectionsNo.ToString();
                    fillMat(record.ID);
                }


                oForm.Freeze(false);
                addEmptyRow();
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
              
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error in loading Record!" + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, true);
                oForm.Freeze(false);

            }
        }
        public override void FindRecordMode()
        {
            base.FindRecordMode();
            IniContrls();
            ItxName.Enabled = true;
            txName.Active = true;
        }
        private void doSubmit()
        {
            if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_FIND_MODE)
            {
                doFind();
            }
            else
            {
                if (validateForm())
                {
                    submitChanges();
                }
            }
        }
        private bool validateForm()
        {
            bool result = true;

            if (txName.Value == "")
            {
                oApplication.SetStatusBarMessage("Provide Name");
                return false;

            }
            if (txDesc.Value == "")
            {
                oApplication.SetStatusBarMessage("Provide Description");
                return false;

            }
            if (txReqApp.Value == "")
            {
                oApplication.SetStatusBarMessage("Atleast 1 approval required");
                return false;

            }
            
            return result;
        }
        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
            SearchKeyVal.Add("StageName", txName.Value);
            SearchKeyVal.Add("StageDescription", txDesc.Value);

        }
        private void doFind()
        {
            PrepareSearchKeyHash();
            string strSql = sqlString.getSql("apprStages", SearchKeyVal);
            picker pic = new picker(oApplication, ds.getDataTable(strSql));
            System.Data.DataTable st = pic.ShowInput("Select Stage", "Select Stage");
            pic = null;
            if (st.Rows.Count > 0)
            {
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                txId.Value = st.Rows[0][0].ToString();
                txName.Value = st.Rows[0][1].ToString();
                getRecord(txId.Value.ToString());
            }
        }
        private void submitChanges()
        {
            try
            {
                // mtAuth.FlushToDataSource();
                string id = "";
                string code = "";
                string isnew = "";
                CfgApprovalStage apprStage;
                int cnt = (from p in dbHrPayroll.CfgApprovalStage where p.StageName == txName.Value select p).Count();
                if (cnt > 0)
                {
                    apprStage = (from p in dbHrPayroll.CfgApprovalStage where p.StageName == txName.Value select p).Single();

                }
                else
                {
                    apprStage = new CfgApprovalStage();
                    dbHrPayroll.CfgApprovalStage.InsertOnSubmit(apprStage);
                }
                apprStage.StageName = txName.Value;
                apprStage.StageDescription = txDesc.Value;
                
                apprStage.ApprovalsNo = Convert.ToByte(txReqApp.Value);
                if (txReqRej.Value == "")
                {
                    apprStage.RejectionsNo = 0;
                }
                else
                {
                    apprStage.RejectionsNo = Convert.ToByte(txReqRej.Value);
                }

                for (int i = 0; i < dtAuth.Rows.Count; i++)
                {
                    code = Convert.ToString(dtAuth.GetValue("username", i));
                    isnew = Convert.ToString(dtAuth.GetValue("isNew", i));
                    isnew = isnew.Trim();
                    code = code.Trim();
                    if (code != "" && isnew == "Y")
                    {
                        CfgApprovalStageDetail apprUsr = new CfgApprovalStageDetail();
                        apprUsr.AuthorizerID = dtAuth.GetValue("username", i);
                        apprUsr.AuthorizerName = dtAuth.GetValue("username", i);
                        apprStage.CfgApprovalStageDetail.Add(apprUsr);
                    }
                }

                dbHrPayroll.SubmitChanges();
                getData();
                if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                {
                    IniContrls();
                }
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);
            }
        }
       
    
    } 
}
