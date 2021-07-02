using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DIHRMS;
using SAPbobsCOM;
using SAPbouiCOM;

namespace ACHR.Screen
{
    class frm_userAuth : HRMSBaseForm
    {

        #region "Global Variable Area"

        SAPbouiCOM.Button btCancel, btnSave;
        SAPbouiCOM.EditText txtUserCode, txtUserName;
        SAPbouiCOM.DataTable dtAuthentic;
        SAPbouiCOM.Matrix grdAuthentic;
        SAPbouiCOM.Columns oColumns;
        SAPbouiCOM.Column oColumn, clNo,ID, Fucntions, Rights;
        SAPbouiCOM.Button btId;

        #endregion

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                oForm.EnableMenu("1288", false);  // Next Record
                oForm.EnableMenu("1289", false);  // Pevious Record
                oForm.EnableMenu("1290", false);  // First Record
                oForm.EnableMenu("1291", false);  // Last record 
                InitiallizeForm();
                GetFunctions();
                FillRightsinCombo();

                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_userAuth Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }       
        
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {

            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "btId":
                    picUser();
                    break;
                case "1":
                    SaveUserAuth2nd();
                    break;                
            }
        }

        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            try
            {
                btCancel = oForm.Items.Item("2").Specific;
                btnSave = oForm.Items.Item("1").Specific;
                //Initializing TextBoxes
                txtUserCode = oForm.Items.Item("txtUCode").Specific;
                txtUserName = oForm.Items.Item("txtUName").Specific;
                InitiallizegridMatrix();                
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void InitiallizegridMatrix()
        {
            try
            {
                dtAuthentic = oForm.DataSources.DataTables.Add("UserAuth");
                dtAuthentic.Columns.Add("No", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtAuthentic.Columns.Add("ID", SAPbouiCOM.BoFieldsType.ft_Integer);
                dtAuthentic.Columns.Add("Functions", SAPbouiCOM.BoFieldsType.ft_Text);
                dtAuthentic.Columns.Add("Rights", SAPbouiCOM.BoFieldsType.ft_Text);

                grdAuthentic = (SAPbouiCOM.Matrix)oForm.Items.Item("grd_Auth").Specific;
                oColumns = (SAPbouiCOM.Columns)grdAuthentic.Columns;

                oColumn = oColumns.Item("clNo");
                clNo = oColumn;
                oColumn.DataBind.Bind("UserAuth", "No");

                oColumn = oColumns.Item("Id");
                ID = oColumn;
                oColumn.DataBind.Bind("UserAuth", "ID");

                oColumn = oColumns.Item("func");
                Fucntions = oColumn;
                oColumn.DataBind.Bind("UserAuth", "Functions");

                oColumn = oColumns.Item("rights");
                Rights = oColumn;
                oColumn.DataBind.Bind("UserAuth", "Rights");
               
      

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void LoadSelectedData(String pCode)
        {

            try
            {
                                          
                if (!String.IsNullOrEmpty(pCode))
                {
                    var getUser = (from a in dbHrPayroll.MstUsers
                                  where a.UserCode.Contains(pCode)
                                  select a).FirstOrDefault();                   
                    txtUserName.Value = getUser.UserID;                        
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_userAuth Function: LoadSelectedData Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }       
        
        private void GetFunctions()
        {
            Int16 i = 0;
            var LstUserRights = dbHrPayroll.MstLOVE.Where(l => l.Type == "UserRights" && l.Code == "2").FirstOrDefault();
            //string userRight = "2";
            try
            {
                var FunctionRecords = dbHrPayroll.MstUserFunctions.Where(f => f.IsActive == true).ToList();
                if (FunctionRecords != null && FunctionRecords.Count > 0)
                {
                    dtAuthentic.Rows.Clear();
                    dtAuthentic.Rows.Add(FunctionRecords.Count());
                    foreach (var WD in FunctionRecords)
                    {
                        dtAuthentic.SetValue("No", i, i + 1);
                        dtAuthentic.SetValue("ID", i, WD.ID);
                        dtAuthentic.SetValue("Functions", i, WD.FunctionName);
                        if (LstUserRights != null)
                        {
                            dtAuthentic.SetValue("Rights", i, LstUserRights.Code);
                        }
                        i++;
                    }
                    grdAuthentic.LoadFromDataSource();
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_userAuth Function: GetFunctions Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void FillRightsinCombo()
        {
            try
            {
                var Data = dbHrPayroll.MstLOVE.Where(l => l.Type == "UserRights").ToList();
                foreach (var v in Data)
                {
                    Rights.ValidValues.Add(v.Code, v.Value);
                }                            
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void SaveUserAuth()
        {
            try
            {
                string strRights = "";
                string strFunctionID = "";
                int FunctionID = 0;
                int? userId = 0;

                if (!string.IsNullOrEmpty(txtUserCode.Value))
                {
                    var getUser = (from a in dbHrPayroll.MstUsers
                                   where a.UserCode.Contains(txtUserCode.Value)
                                   select a).FirstOrDefault();
                    if (getUser != null)
                    {
                        for (int i = 1; i <= grdAuthentic.RowCount; i++)
                        {
                            strRights = (grdAuthentic.Columns.Item("rights").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                            strFunctionID = (grdAuthentic.Columns.Item("Id").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                            if (!string.IsNullOrEmpty(strFunctionID) && !string.IsNullOrEmpty(strRights))
                            {
                                FunctionID = Convert.ToInt32(strFunctionID);
                                userId = getUser.ID;
                                var OldData = dbHrPayroll.MstUsersAuth.Where(u => u.UserID == userId && u.FunctionID == FunctionID).FirstOrDefault();
                                if (OldData != null)
                                {
                                    OldData.UserRights = strRights;
                                }
                                else
                                {
                                    MstUsersAuth objUserAuth = new MstUsersAuth();
                                    objUserAuth.FunctionID = FunctionID;
                                    objUserAuth.UserRights = strRights;
                                    objUserAuth.UserID = getUser.ID;
                                    dbHrPayroll.MstUsersAuth.InsertOnSubmit(objUserAuth);
                                }
                            }
                            else
                            {
                                oApplication.StatusBar.SetText("Please Provide Valid User Rights", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                return;
                            }                                                       
                        }
                        dbHrPayroll.SubmitChanges();
                    }
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_userAuth Function: SaveUserAuth Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void GetUserRights(String pCode)
        {
            int? userId = 0;
            int? intFunctionID = 0;
            try
            {
                if (!string.IsNullOrEmpty(pCode))
                {

                    var getUser = (from a in dbHrPayroll.MstUsers
                                   where a.UserCode.Contains(pCode)
                                   select a).FirstOrDefault();
                    if (getUser != null)
                    {
                        userId = getUser.ID;
                        //grdAuthentic.Clear();
                        //dtAuthentic.Rows.Clear();                
                        for (int K = 0; K < dtAuthentic.Rows.Count; K++)
                        {
                            string FunctionID = (grdAuthentic.Columns.Item("Id").Cells.Item(K + 1).Specific as SAPbouiCOM.EditText).Value;
                            string FunctionName = (grdAuthentic.Columns.Item("func").Cells.Item(K + 1).Specific as SAPbouiCOM.EditText).Value;
                            if (!string.IsNullOrEmpty(FunctionID))
                            {
                                intFunctionID = Convert.ToInt32(FunctionID);
                                var OldData = dbHrPayroll.MstUsersAuth.Where(u => u.UserID == userId && u.FunctionID == intFunctionID).FirstOrDefault();
                                if (OldData != null)
                                {
                                    dtAuthentic.SetValue("Rights", K, OldData.UserRights);
                                }
                            }
                        }
                        grdAuthentic.LoadFromDataSource();
                    }
                    else
                    {
                        GetFunctions();
                    }

                }

            }
            catch (Exception ex)
            {
            }
        }
        
        private void picUser()
        {
            string strSql = "select USERID,user_code , U_NAME from " + oCompany.CompanyDB + ".dbo.ousr";
            //DataTable dtUsr = ds.getDataTable(strSql);
            //string strSql = sqlString.getSql("ActiveUsers", SearchKeyVal);

            string strQuery = "SELECT \"USERID\" , \"USER_CODE\" , \"U_NAME\" FROM OUSR";
            SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
            oRecSet.DoQuery(strQuery);
            System.Data.DataTable dtSapUsers = new System.Data.DataTable();
            dtSapUsers.Columns.Add("USERID");
            dtSapUsers.Columns.Add("USER_CODE");
            dtSapUsers.Columns.Add("U_NAME");
            while (oRecSet.EoF == false)
            {
                string userid, usercode, uname;
                userid = Convert.ToString(oRecSet.Fields.Item("USERID").Value);
                usercode = Convert.ToString(oRecSet.Fields.Item("USER_CODE").Value);
                uname = Convert.ToString(oRecSet.Fields.Item("U_NAME").Value);
                DataRow dtrow = dtSapUsers.NewRow();
                dtrow["USERID"] = userid;
                dtrow["USER_CODE"] = usercode;
                dtrow["U_NAME"] = uname;
                dtSapUsers.Rows.Add(dtrow);
                oRecSet.MoveNext();
            }
            

            //picker pic = new picker(oApplication, ds.getDataTable(strSql));
            picker pic = new picker(oApplication, dtSapUsers);
            System.Data.DataTable st = pic.ShowInput("Select User", "Select User for User Authorization");
            pic = null;
            if (st.Rows.Count > 0)
            {
                txtUserCode.Value = st.Rows[0][0].ToString();
                txtUserName.Value = st.Rows[0][1].ToString();
                //LoadSelectedData(txtUserCode.Value);
                GetUserRights2nd(txtUserCode.Value);
            }
        }
        
        private void GetUserRights2nd(String pCode)
        {
            int? userId = 0;
            int? intFunctionID = 0;
            try
            {
                if (!string.IsNullOrEmpty(pCode))
                {
                    userId = Convert.ToInt32(pCode);
                    for (int K = 0; K < dtAuthentic.Rows.Count; K++)
                    {
                        string FunctionID = (grdAuthentic.Columns.Item("Id").Cells.Item(K + 1).Specific as SAPbouiCOM.EditText).Value;
                        string FunctionName = (grdAuthentic.Columns.Item("func").Cells.Item(K + 1).Specific as SAPbouiCOM.EditText).Value;
                        if (!string.IsNullOrEmpty(FunctionID))
                        {
                            intFunctionID = Convert.ToInt32(FunctionID);
                            var OldData = dbHrPayroll.MstUsersAuth.Where(u => u.UserID == userId && u.FunctionID == intFunctionID).FirstOrDefault();
                            if (OldData != null)
                            {
                                dtAuthentic.SetValue("Rights", K, OldData.UserRights);
                            }
                        }
                    }
                    grdAuthentic.LoadFromDataSource();
                }
                else
                {
                    GetFunctions();
                }
            }
            catch (Exception ex)
            {
            }
        }
        
        private void SaveUserAuth2nd()
        {
            try
            {
                string strRights = "";
                string strFunctionID = "";
                int FunctionID = 0;
                int? userId = 0;
                int confirm = oApplication.MessageBox("Are you sure you want to Update Authorization for Selected User? ", 3, "Yes", "No", "Cancel");
                if (confirm == 2 || confirm == 3)
                {
                    return;
                } 
                if (!string.IsNullOrEmpty(txtUserCode.Value))
                {
                    userId = Convert.ToInt32(txtUserCode.Value);
                    for (int i = 1; i <= grdAuthentic.RowCount; i++)
                    {
                        strRights = (grdAuthentic.Columns.Item("rights").Cells.Item(i).Specific as SAPbouiCOM.ComboBox).Value;
                        strFunctionID = (grdAuthentic.Columns.Item("Id").Cells.Item(i).Specific as SAPbouiCOM.EditText).Value;
                        if (!string.IsNullOrEmpty(strFunctionID) && !string.IsNullOrEmpty(strRights))
                        {
                            FunctionID = Convert.ToInt32(strFunctionID);

                            var OldData = dbHrPayroll.MstUsersAuth.Where(u => u.UserID == userId && u.FunctionID == FunctionID).FirstOrDefault();
                            if (OldData != null)
                            {
                                OldData.UserRights = strRights;
                            }
                            else
                            {
                                MstUsersAuth objUserAuth = new MstUsersAuth();
                                objUserAuth.FunctionID = FunctionID;
                                objUserAuth.UserRights = strRights;
                                objUserAuth.UserID = userId;
                                dbHrPayroll.MstUsersAuth.InsertOnSubmit(objUserAuth);
                            }
                        }
                        else
                        {
                            oApplication.StatusBar.SetText("Please Provide Valid User Rights", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                            return;
                        }
                    }
                    dbHrPayroll.SubmitChanges();
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_userAuth Function: SaveUserAuth Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion

    }
}
