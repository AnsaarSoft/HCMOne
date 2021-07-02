using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using System.Data;

namespace ACHR.Screen
{
    class frm_ApprTamp:HRMSBaseForm
    {
        SAPbouiCOM.Matrix mtOrig, mtStages;
        SAPbouiCOM.EditText txName, txDesc, txId;
        SAPbouiCOM.CheckBox chActive, chVacCre, chCanSrtLs, chEmpHir, chEmpLeav, chEmpTrns, chResTerm, chApraisal ,chLoan,chAdvance;
        SAPbouiCOM.Button cmdPrev, cmdNext, cmdNew;
        SAPbouiCOM.Column cUsr, cDept, OrigIsNew, Origid , StagId,StagIsNew;
        private SAPbouiCOM.DataTable dtOrig, dtStage;

        SAPbouiCOM.Item ItxName, ItxDesc, ItxReqApp, ItxReqRej, IcmdPrev, IcmdNext, IcmdNew, ItxId, IchActive, IchVacCre, IchCanSrtLs, IchEmpHir, IchEmpLeav, IchEmpTrns, IchResTerm, IchApraisal, IchLoan, IchAdvance ;
        //**********************************
        public IEnumerable<CfgApprovalTemplate> apprTemplates;
       

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            InitiallizeForm();
        }
        public override void fillFields()
        {
            base.fillFields();
            addEmptyRowStage();
            addEmptyRowOrig();
            _fillFields();
        }
        private void InitiallizeForm()
        {
            oForm.DefButton = "1";
            oForm.Freeze(true);
            oForm.PaneLevel = 1;

            //EachItemshould be initiallized in this sequence

            /*
             * Add datasource for the item
             * Initiallize the controll object with same ID
             * Initiallize the Item Object with same ID
             * Data Bind the controll with data source
             * 
             * */

            mtOrig = oForm.Items.Item("mtOrig").Specific;
            mtStages = oForm.Items.Item("mtStages").Specific;

            OrigIsNew = mtOrig.Columns.Item("isNew");
            Origid = mtOrig.Columns.Item("id");
            OrigIsNew.Visible = false;
            Origid.Visible = false;
            dtOrig = oForm.DataSources.DataTables.Item("dtOrig");
            dtOrig.Rows.Clear();

            StagIsNew = mtStages.Columns.Item("isNew");
            StagId = mtStages.Columns.Item("id");
            StagIsNew.Visible = false;
            StagId.Visible = false;
            dtStage = oForm.DataSources.DataTables.Item("dtStage");
            dtStage.Rows.Clear();

            oForm.DataSources.UserDataSources.Add("txId", SAPbouiCOM.BoDataType.dt_LONG_NUMBER); // Days of Month
            txId = oForm.Items.Item("txId").Specific;
            ItxId = oForm.Items.Item("txId");
            txId.DataBind.SetBound(true, "", "txId");

            oForm.DataSources.UserDataSources.Add("txDesc", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100); // Days of Month
            txDesc = oForm.Items.Item("txDesc").Specific;
            ItxDesc = oForm.Items.Item("txDesc");
            txDesc.DataBind.SetBound(true, "", "txDesc");

            oForm.DataSources.UserDataSources.Add("txName", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Days of Month
            txName = oForm.Items.Item("txName").Specific;
            ItxName = oForm.Items.Item("txName");
            txName.DataBind.SetBound(true, "", "txName");

            oForm.DataSources.UserDataSources.Add("chActive", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
            chActive = oForm.Items.Item("chActive").Specific;
            IchActive = oForm.Items.Item("chActive");
            chActive.DataBind.SetBound(true, "", "chActive");

            oForm.DataSources.UserDataSources.Add("chVacCre", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
            chVacCre = oForm.Items.Item("chVacCre").Specific;
            IchVacCre = oForm.Items.Item("chVacCre");
            chVacCre.DataBind.SetBound(true, "", "chVacCre");
            oForm.DataSources.UserDataSources.Add("chCanSrtLs", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
            chCanSrtLs = oForm.Items.Item("chCanSrtLs").Specific;
            IchCanSrtLs = oForm.Items.Item("chCanSrtLs");
            chCanSrtLs.DataBind.SetBound(true, "", "chCanSrtLs");

            oForm.DataSources.UserDataSources.Add("chEmpHir", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
            chEmpHir = oForm.Items.Item("chEmpHir").Specific;
            IchEmpHir = oForm.Items.Item("chEmpHir");
            chEmpHir.DataBind.SetBound(true, "", "chEmpHir");
            oForm.DataSources.UserDataSources.Add("chEmpLeav", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
            chEmpLeav = oForm.Items.Item("chEmpLeav").Specific;
            IchEmpLeav = oForm.Items.Item("chEmpLeav");
            chEmpLeav.DataBind.SetBound(true, "", "chEmpLeav");
            oForm.DataSources.UserDataSources.Add("chEmpTrns", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
            chEmpTrns = oForm.Items.Item("chEmpTrns").Specific;
            IchEmpTrns = oForm.Items.Item("chEmpTrns");
            chEmpTrns.DataBind.SetBound(true, "", "chEmpTrns");
            oForm.DataSources.UserDataSources.Add("chResTerm", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
            chResTerm = oForm.Items.Item("chResTerm").Specific;
            IchResTerm = oForm.Items.Item("chResTerm");
            chResTerm.DataBind.SetBound(true, "", "chResTerm");
            
            oForm.DataSources.UserDataSources.Add("chApraisal", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
            chApraisal = oForm.Items.Item("chApraisal").Specific;
            IchApraisal = oForm.Items.Item("chApraisal");
            chApraisal.DataBind.SetBound(true, "", "chApraisal");

            oForm.DataSources.UserDataSources.Add("chLoan", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
            chLoan = oForm.Items.Item("chLoan").Specific;
            IchLoan = oForm.Items.Item("chLoan");
            chLoan.DataBind.SetBound(true, "", "chLoan");


            oForm.DataSources.UserDataSources.Add("chAdvance", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1); // Days of Month
            chAdvance = oForm.Items.Item("chAdvance").Specific;
            IchAdvance = oForm.Items.Item("chAdvance");
            chAdvance.DataBind.SetBound(true, "", "chAdvance");


            getData();
            oForm.Freeze(false);
            addNew();
         
        }
        public void setDept(string usrCode, int i)
        {
            
                MstUsers usr = (from p in dbHrPayroll.MstUsers where p.UserCode == usrCode select p).Single();

                dtOrig.SetValue("id", i, usr.ID);
                dtOrig.SetValue("usercode", i, usr.UserCode);
                dtOrig.SetValue("pick", i,strCfl);
                dtOrig.SetValue("dept", i, usr.MstEmployee.MstDepartment.DeptName);
                mtOrig.SetLineData(i + 1);
                addEmptyRowOrig();

           
        }
        public void setDeptX(string usrCode, int i)
        {


            //string strSql = "select USERID,user_code , U_NAME from " + oCompany.CompanyDB + ".dbo.ousr Where USERID=" + usrCode;
            //DataTable dt = ds.getDataTable(strSql, "2");
            
                dtOrig.SetValue("id", i, usrCode);
                dtOrig.SetValue("usercode", i, usrCode);
                dtOrig.SetValue("pick", i, strCfl);
                //dtOrig.SetValue("dept", i, usr.MstEmployee.MstDepartment.DeptName);
                mtOrig.SetLineData(i + 1);
                addEmptyRowOrig();         


        }

        public void setDept(string usrCode, string username, int i)
        {
            //MstUsers usr = (from p in dbHrPayroll.MstUsers where p.UserCode == usrCode select p).Single();
            dtOrig.SetValue("id", i, usrCode);
            dtOrig.SetValue("usercode", i, username);
            dtOrig.SetValue("pick", i, strCfl);
            //dtAuth.SetValue("Dept", i, usr.MstEmployee.MstDepartment.DeptName);
            // mtAuth.SetLineData(i+1 );
            if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_OK_MODE) oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
            mtOrig.SetLineData(i + 1);
            addEmptyRowOrig();

        }
        public void setStage(string stageName, int i)
        {
            int cnt = (from p in dbHrPayroll.CfgApprovalStage where p.StageName == stageName select p).Count();
            if (cnt > 0)
            {
                CfgApprovalStage stage = (from p in dbHrPayroll.CfgApprovalStage where p.StageName == stageName select p).Single();
                dtStage.SetValue("id", i, stage.ID.ToString());
                dtStage.SetValue("name", i, stage.StageName);
                dtStage.SetValue("descr", i, stage.StageDescription);
                mtStages.SetLineData(i + 1);
                addEmptyRowStage();
            }
        }
        private void addEmptyRowOrig()
        {


            if (dtOrig.Rows.Count == 0)
            {
                dtOrig.Rows.Add(1);

                dtOrig.SetValue("isNew", 0, "Y");
                dtOrig.SetValue("id", 0, 0);
                dtOrig.SetValue("usercode", 0, "");
                dtOrig.SetValue("pick", 0, strCfl);
                //dtOrig.SetValue("dept", 0, "");
                mtOrig.AddRow(1, mtOrig.RowCount + 1);
            }
            else
            {
                if (dtOrig.GetValue("usercode", dtOrig.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtOrig.Rows.Add(1);
                    dtOrig.SetValue("isNew", dtOrig.Rows.Count - 1, "Y");
                    dtOrig.SetValue("id", dtOrig.Rows.Count - 1, 0);
                    dtOrig.SetValue("usercode", dtOrig.Rows.Count - 1, "");
                    dtOrig.SetValue("pick", dtOrig.Rows.Count - 1, strCfl);
                    //dtOrig.SetValue("dept", dtOrig.Rows.Count - 1, "");
                    mtOrig.AddRow(1, mtOrig.RowCount + 1);
                }

            }
            // mtAdv.FlushToDataSource();
            mtOrig.LoadFromDataSource();

        }

        private void addEmptyRowStage()
        {


            if (dtStage.Rows.Count == 0)
            {
                dtStage.Rows.Add(1);

                dtStage.SetValue("isNew", 0, "Y");
                dtStage.SetValue("id", 0, 0);
                dtStage.SetValue("name", 0, "");
                dtStage.SetValue("descr", 0, "");
                mtStages.AddRow(1, mtStages.RowCount + 1);
            }
            else
            {
                if (dtStage.GetValue("name", dtStage.Rows.Count - 1) == "")
                {
                }
                else
                {
                    dtStage.Rows.Add(1);
                    dtStage.SetValue("isNew", dtStage.Rows.Count - 1, "Y");
                    dtStage.SetValue("id", dtStage.Rows.Count - 1, 0);
                    dtStage.SetValue("name", dtStage.Rows.Count - 1, "");
                    dtStage.SetValue("descr", dtStage.Rows.Count - 1, "");
                    mtStages.AddRow(1, mtStages.RowCount + 1);
                }

            }
            // mtAdv.FlushToDataSource();
            mtStages.LoadFromDataSource();

        }
        private void IniContrls()
        {
            txId.Value = "0";
            txName.Value = "";
            txDesc.Value = "";
            dtOrig.Rows.Clear();
            dtStage.Rows.Clear();
            oForm.DataSources.UserDataSources.Item("chActive").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chVacCre").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chCanSrtLs").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chEmpHir").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chEmpLeav").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chEmpTrns").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chResTerm").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chApraisal").ValueEx = "N";

            oForm.DataSources.UserDataSources.Item("chLoan").ValueEx = "N";
            oForm.DataSources.UserDataSources.Item("chAdvance").ValueEx = "N";



            ItxId.Visible = false;
            txName.Active = true;

           
        }
        public override void AddNewRecord()
        {
            base.AddNewRecord();
            addNew();
        }
        private void addNew()
        {
            ItxName.Enabled = true;
            IniContrls();
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
            addEmptyRowOrig();
            addEmptyRowStage();

        }
       
      
      
       
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                    
                case "1":
                    doSubmit();
                    break;
                case "mtOrig":
                    if (pVal.ColUID == "pick")
                    {
                        int rowNum = pVal.Row;
                        if (rowNum <= dtOrig.Rows.Count)
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
               //setDept(strUserId, rowNum - 1);


            //}
        }
        public override void etAfterValidate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterValidate(ref pVal, ref BubbleEvent);
            int rowNum = pVal.Row;
            switch (pVal.ItemUID)
            {
                case "mtOrig":
                    
                    SAPbouiCOM.EditText oitm = mtOrig.GetCellSpecific("cUsr", rowNum);
                    string strUserCode = oitm.Value;
                    setDept(strUserCode, rowNum - 1);
                    break;
                case "mtStages":
                    
                     rowNum = pVal.Row;
                     SAPbouiCOM.EditText stItem = mtStages.GetCellSpecific("cStage", rowNum);
                     string strSgate = stItem.Value;
                    setStage(strSgate, rowNum - 1);
                    break;

            }
        }
        public override void FindRecordMode()
        {
            base.FindRecordMode();
            IniContrls();
            ItxName.Enabled = true;
            txName.Active = true;
        }
        private void fillMat(int recordId)
        {
            IEnumerable<CfgApprovalTemplateStages> apprStages;
            IEnumerable<CfgApprovalTemplateOriginator> apprOriginator;

            dtOrig.Rows.Clear();
            dtStage.Rows.Clear();
            apprStages = from p in dbHrPayroll.CfgApprovalTemplateStages where p.ATID.ToString() == recordId.ToString() select p;
            apprOriginator = from p in dbHrPayroll.CfgApprovalTemplateOriginator where p.ATID.ToString() == recordId.ToString() select p;

            int i = 0;
            foreach (CfgApprovalTemplateStages loc in apprStages)
            {
                dtStage.Rows.Add(1);
                dtStage.SetValue("isNew", i, "N");
                setStage(loc.CfgApprovalStage.StageName, i);
                // dtAuth.SetValue("Dept", i, sd.);

                i++;

            }
            addEmptyRowStage();
            mtStages.LoadFromDataSource();

            i = 0;
            foreach (CfgApprovalTemplateOriginator loc in apprOriginator)
            {
                dtOrig.Rows.Add(1);
                dtOrig.SetValue("isNew", i, "N");
                setDeptX(loc.Originator.ToString(), i);
                //setDept(loc.MstUsers.UserCode, i);
               // setDept(loc.Originator.ToString(), strUserName, i);
                // dtAuth.SetValue("Dept", i, sd.);
                
                i++;

            }
            addEmptyRowOrig();
            mtOrig.LoadFromDataSource();

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

                    CfgApprovalTemplate record = apprTemplates.ElementAt<CfgApprovalTemplate>(currentRecord);
                    txId.Value = record.ID.ToString();
                    txName.Value = record.Name.ToString();
                    txDesc.Value = record.Description;
                    chActive.Checked = (bool)record.FlgActive;

                    oForm.DataSources.UserDataSources.Item("chApraisal").ValueEx = (bool)record.CfgApprovalTemplateDocuments.ElementAt(0).FlgAppraisal == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("chCanSrtLs").ValueEx = (bool)record.CfgApprovalTemplateDocuments.ElementAt(0).FlgCandidate == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("chEmpHir").ValueEx = (bool)record.CfgApprovalTemplateDocuments.ElementAt(0).FlgEmpHiring == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("chEmpLeav").ValueEx = (bool)record.CfgApprovalTemplateDocuments.ElementAt(0).FlgEmpLeave == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("chResTerm").ValueEx = (bool)record.CfgApprovalTemplateDocuments.ElementAt(0).FlgResignation == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("chVacCre").ValueEx = (bool)record.CfgApprovalTemplateDocuments.ElementAt(0).FlgJobRequisition == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("chLoan").ValueEx = (bool)record.CfgApprovalTemplateDocuments.ElementAt(0).FlgLoan == true ? "Y" : "N";
                    oForm.DataSources.UserDataSources.Item("chAdvance").ValueEx = (bool)record.CfgApprovalTemplateDocuments.ElementAt(0).FlgAdvance == true ? "Y" : "N";

                    fillMat(record.ID);
                }


                oForm.Freeze(false);
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage("Error in loading Record!" + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, true);
                oForm.Freeze(false);

            }
        }
        private void getData()
        {
            CodeIndex.Clear();
            apprTemplates = from p in dbHrPayroll.CfgApprovalTemplate select p;
            int i = 0;
            foreach (CfgApprovalTemplate ele in apprTemplates)
            {
                CodeIndex.Add(ele.ID.ToString(), i);

                i++;
            }
            totalRecord = i;
        }
        public override void PrepareSearchKeyHash()
        {
            base.PrepareSearchKeyHash();
            SearchKeyVal.Clear();
            SearchKeyVal.Add("Name", txName.Value);
            SearchKeyVal.Add("Description", txDesc.Value);

        }
        private void doFind()
        {
            PrepareSearchKeyHash();
            string strSql = sqlString.getSql("apprTemp", SearchKeyVal);
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

            return result;
        }
        private void submitChanges()
        {
            try
            {
                string id = "";
                string code = "";
                string isnew = "";
                CfgApprovalTemplate aprTemplate;
                CfgApprovalTemplateDocuments ApprDocs;

                int cnt = (from p in dbHrPayroll.CfgApprovalTemplate where p.Name == txName.Value select p).Count();
                if (cnt > 0)
                {
                    aprTemplate = (from p in dbHrPayroll.CfgApprovalTemplate where p.Name == txName.Value select p).Single();
                    ApprDocs = aprTemplate.CfgApprovalTemplateDocuments.ElementAt(0);

                }
                else
                {
                    aprTemplate = new CfgApprovalTemplate();
                    ApprDocs = new CfgApprovalTemplateDocuments();
                    dbHrPayroll.CfgApprovalTemplate.InsertOnSubmit(aprTemplate);
                }
                aprTemplate.Name = txName.Value;
                aprTemplate.Description = txDesc.Value;
                aprTemplate.FlgActive = chActive.Checked;
                aprTemplate.CreateDate = DateTime.Now;
                aprTemplate.UpdateDate = DateTime.Now;
                aprTemplate.UserID = oApplication.Company.UserName;



                for (int i = 0; i < dtOrig.Rows.Count; i++)
                {
                    code = Convert.ToString(dtOrig.GetValue("usercode", i));
                    int userid=Convert.ToInt32(dtOrig.GetValue("id", i));
                    isnew = Convert.ToString(dtOrig.GetValue("isNew", i));
                    isnew = isnew.Trim();
                    code = code.Trim();
                    if (code != "" && isnew == "Y")
                    {
                        CfgApprovalTemplateOriginator apprOrig = new CfgApprovalTemplateOriginator();
                        // apprOrig.Originator = dtOrig.GetValue("usercode", i);
                        apprOrig.Originator = Convert.ToInt32(userid);
                        //apprOrig.MstUsers = (from p in dbHrPayroll.MstUsers where p.UserCode == code select p).Single();
                        //apprOrig.Department = dtOrig.GetValue("dept", i);

                        aprTemplate.CfgApprovalTemplateOriginator.Add(apprOrig);
                    }
                }

                for (int i = 0; i < dtStage.Rows.Count; i++)
                {
                    string stageId = Convert.ToString(dtStage.GetValue("id", i));

                    code = Convert.ToString(dtStage.GetValue("name", i));
                    isnew = Convert.ToString(dtStage.GetValue("isNew", i));
                    isnew = isnew.Trim();
                    code = code.Trim();
                    if (code != "" && isnew == "Y")
                    {
                        CfgApprovalStage apprStage = (from p in dbHrPayroll.CfgApprovalStage where p.StageName.ToString() == code select p).Single();
                        CfgApprovalTemplateStages nStag = new CfgApprovalTemplateStages();
                        nStag.CfgApprovalStage = apprStage;
                        nStag.Priorty = 1;
                        //nStag.de
                        aprTemplate.CfgApprovalTemplateStages.Add(nStag);
                    }


                }


                ApprDocs.FlgAppraisal = chApraisal.Checked;
                ApprDocs.FlgCandidate = chCanSrtLs.Checked;
                ApprDocs.FlgEmpHiring = chEmpHir.Checked;
                ApprDocs.FlgEmpLeave = chEmpLeav.Checked;
                ApprDocs.FlgResignation = chResTerm.Checked;
                ApprDocs.FlgJobRequisition = chVacCre.Checked;
                ApprDocs.FlgAdvance = chAdvance.Checked;
                ApprDocs.FlgLoan = chLoan.Checked;
                // ApprDocs.FlgTermination = chEmpTrns.Checked;

                aprTemplate.CfgApprovalTemplateDocuments.Add(ApprDocs);
                dbHrPayroll.SubmitChanges();
                getData();
                if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                {
                    IniContrls();
                }
            }
            catch(Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);
            }
        }
    }
}
