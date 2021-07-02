using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;

namespace ACHR.Screen
{
    class frm_Dept : HRMSBaseForm
    {
        #region "Global Variable Area"

        SAPbouiCOM.Button btnMain, btnViewHiarcy;
        SAPbouiCOM.Matrix mtMain;
        SAPbouiCOM.Item ibtnMain, ibtnViewHiarcy, IbtnMain;
        SAPbouiCOM.DataTable dtDept;
        SAPbouiCOM.Column Isnew, Id, Serial, Code, Description, ParentId, flgActive, DeptLevel;

        #endregion

        #region "B1 Events"
        
        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                //oApplication.FontHeight
                oForm.Freeze(true);
                InitiallizeForm();
                oForm.Freeze(false);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_Dept Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);

            if (pVal.Before_Action == false)
            {
                switch(pVal.ItemUID)
                {
                    case "1":
                        AddnUpdateDepartment();
                        break;
                    case "btview":
                        PrintDepartmentHiearcyReport();
                        break;
                    default:
                        break;
                }
            }
        }

        public override void AddNewRecord()
        {
            base.AddNewRecord();
            //AddEmptyRow();
            //oForm.Mode = SAPbouiCOM.BoFormMode.fm_UPDATE_MODE;
        }

        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            //Each Item should be initiallized in this sequence

            /*
             * Add datasource for the item
             * Initiallize the control object with same ID
             * Initiallize the Item Object with same ID
             * Data Bind the controll with data source
             * 
             * */
            try
            {
                btnMain = oForm.Items.Item("1").Specific;
                ibtnMain = oForm.Items.Item("1");

                mtMain = oForm.Items.Item("mtmain").Specific;
                dtDept = oForm.DataSources.DataTables.Item("dtmain");
                Isnew = mtMain.Columns.Item("isnew");
                Isnew.Visible = false;
                Id = mtMain.Columns.Item("id");
                Id.Visible = false;
                Serial = mtMain.Columns.Item("serial");
                Code = mtMain.Columns.Item("code");
                Code.TitleObject.Sortable = false;
                Description = mtMain.Columns.Item("desc");
                Description.TitleObject.Sortable = false;
                ParentId = mtMain.Columns.Item("parent");
                ParentId.TitleObject.Sortable = false;
                DeptLevel = mtMain.Columns.Item("level");
                DeptLevel.TitleObject.Sortable = false;
                flgActive = mtMain.Columns.Item("active");
                flgActive.TitleObject.Sortable = false;

                FillDepartmentColumn(ParentId);
                fillColumCombo("DeptLevel", DeptLevel);
                FillDepartment();
                AddEmptyRow();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        public void AddnUpdateDepartment()
        {
            try
            {
                oForm.Freeze(true);
                dtDept.Rows.Clear();
                mtMain.FlushToDataSource();
                String DeptCode, DeptDesc, IsNewValue, DeptId, StrActive, DeptParentId, Level;
                Boolean ActiveValue = false;
                for (int i = 0; i < dtDept.Rows.Count; i++)
                {
                    DeptCode = Convert.ToString(dtDept.GetValue(Code.DataBind.Alias, i));
                    DeptDesc = Convert.ToString(dtDept.GetValue(Description.DataBind.Alias, i));
                    IsNewValue = Convert.ToString(dtDept.GetValue(Isnew.DataBind.Alias, i));
                    DeptId = Convert.ToString(dtDept.GetValue(Id.DataBind.Alias, i));
                    DeptParentId = Convert.ToString(dtDept.GetValue(ParentId.DataBind.Alias, i));
                    Level = Convert.ToString(dtDept.GetValue(DeptLevel.DataBind.Alias, i));
                    StrActive = Convert.ToString(dtDept.GetValue(flgActive.DataBind.Alias, i));
                    if (StrActive == "Y")
                    {
                        ActiveValue = true;
                    }
                    else
                    {
                        ActiveValue = false;
                    }
                    if (DeptCode != "")
                    {
                        if (IsNewValue == "Y")
                        {
                            MstDepartment oNew = new MstDepartment();
                            oNew.Code = DeptCode;
                            oNew.DeptName = DeptDesc;
                            if (DeptParentId == "0")
                            {
                                oNew.ParentDepartment = null;
                            }
                            else
                            {
                                oNew.ParentDepartment = Convert.ToInt32(DeptParentId);
                            }
                            oNew.DeptLevel = Convert.ToByte(Level);
                            oNew.FlgActive = ActiveValue;
                            oNew.CreateDate = DateTime.Now;
                            oNew.UserId = oCompany.UserName;

                            dbHrPayroll.MstDepartment.InsertOnSubmit(oNew);
                        }
                        else if (IsNewValue == "N")
                        {                            
                            var oOld = (from a in dbHrPayroll.MstDepartment where a.ID == Convert.ToInt32(DeptId) select a).FirstOrDefault();
                            var EmpWithDepartmentID = dbHrPayroll.MstEmployee.Where(e => e.DepartmentID == Convert.ToInt32(DeptId) && e.FlgActive == true).ToList();
                            if (EmpWithDepartmentID != null && EmpWithDepartmentID.Count > 0 && oOld.Code != DeptCode)
                            {
                                oApplication.MessageBox(DeptDesc + " Department Can't be updated.It is attached with Employee(s)");
                                oApplication.StatusBar.SetText(DeptDesc + " Department Can't be updated.It is attached with Employee(s)", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                                continue;
                            }
                            oOld.Code = DeptCode;
                            oOld.DeptName = DeptDesc;
                            oOld.FlgActive = ActiveValue;
                            if (DeptParentId == "0")
                            {
                                oOld.ParentDepartment = null;
                            }
                            else
                            {
                                oOld.ParentDepartment = Convert.ToInt32(DeptParentId);
                            }
                            oOld.DeptLevel = Convert.ToByte(Level);
                            oOld.UpdateDate = DateTime.Now;
                            oOld.UpdatedBy = oCompany.UserName;
                        }

                        dbHrPayroll.SubmitChanges();
                    }
                    
                }
                FillDepartment();
                AddEmptyRow();
                oForm.Freeze(false);
                //if (btnMain.Caption == "Add")
                //{
                //    oApplication.StatusBar.SetText("Records Saved Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                //}
                

                //if (btnMain.Caption == "Update")
                //{
                //    int confirm = oApplication.MessageBox("Are you sure you want to post draft? ", 3, "Yes", "No", "Cancel");
                //    if (confirm == 2 || confirm == 3)
                //    {
                //        return;
                //    }
                //}
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Function: AddnUpdateDepartment Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                oForm.Freeze(false);
            }
        }

        public void CheckState()
        {
            try
            {
                if (btnMain.Caption == "Add")
                {
                }
                if (btnMain.Caption == "Update")
                {
                }
                if (btnMain.Caption == "Ok")
                {
                    oForm.Close();
                }

            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        public void FillDepartment()
        {
            try
            {
                IEnumerable<MstDepartment> Departments = from a in dbHrPayroll.MstDepartment select a;
                Int16 i = 0;
                if (Departments.Count() == 0)
                {
                    return;
                }
                dtDept.Rows.Clear();
                dtDept.Rows.Add(Departments.Count());
                foreach (MstDepartment Dept in Departments)
                {
                    dtDept.SetValue(Isnew.DataBind.Alias, i, "N");
                    dtDept.SetValue(Id.DataBind.Alias, i, Dept.ID);
                    dtDept.SetValue(Code.DataBind.Alias, i, Dept.Code);
                    dtDept.SetValue(Description.DataBind.Alias, i, Dept.DeptName);
                    if (Dept.ParentDepartment == null)
                    {
                        dtDept.SetValue(ParentId.DataBind.Alias, i, "0");
                    }
                    else
                    {
                        dtDept.SetValue(ParentId.DataBind.Alias, i, Dept.ParentDepartment.ToString());
                    }
                    dtDept.SetValue(DeptLevel.DataBind.Alias, i, Dept.DeptLevel.ToString());
                    if (Convert.ToBoolean(Dept.FlgActive))
                    {
                        dtDept.SetValue(flgActive.DataBind.Alias, i, "Y");
                    }
                    else
                    {
                        dtDept.SetValue(flgActive.DataBind.Alias, i, "N");
                    }
                    dtDept.SetValue(Serial.DataBind.Alias, i, i + 1);
                    i++;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("FillDepartmentGRid Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }

        }

        private void AddEmptyRow()
        {
            Int32 RowValue = 0;
            if (dtDept.Rows.Count == 0)
            {
                dtDept.Rows.Add(1);
                RowValue = dtDept.Rows.Count;
                dtDept.SetValue(Isnew.DataBind.Alias, RowValue - 1, "Y");
                dtDept.SetValue(Id.DataBind.Alias, RowValue - 1, "0");
                dtDept.SetValue(Code.DataBind.Alias, RowValue - 1, "");
                dtDept.SetValue(Description.DataBind.Alias, RowValue - 1, "");
                dtDept.SetValue(ParentId.DataBind.Alias, RowValue - 1, "0");
                dtDept.SetValue(DeptLevel.DataBind.Alias, RowValue - 1, "0");
                dtDept.SetValue(flgActive.DataBind.Alias, RowValue - 1, "Y");
                dtDept.SetValue(Serial.DataBind.Alias, RowValue - 1, RowValue);
                mtMain.AddRow(1, RowValue + 1);
            }
            else
            {
                if (dtDept.GetValue(Code.DataBind.Alias, dtDept.Rows.Count - 1) == "")
                {
                }
                else
                {

                    dtDept.Rows.Add(1);
                    RowValue = dtDept.Rows.Count;
                    dtDept.SetValue(Isnew.DataBind.Alias, RowValue - 1, "Y");
                    dtDept.SetValue(Id.DataBind.Alias, RowValue - 1, "0");
                    dtDept.SetValue(Code.DataBind.Alias, RowValue - 1, "");
                    dtDept.SetValue(Description.DataBind.Alias, RowValue - 1, "");
                    dtDept.SetValue(ParentId.DataBind.Alias, RowValue - 1, "0");
                    dtDept.SetValue(DeptLevel.DataBind.Alias, RowValue - 1, "0");
                    dtDept.SetValue(flgActive.DataBind.Alias, RowValue - 1, "Y");
                    dtDept.SetValue(Serial.DataBind.Alias, RowValue - 1, RowValue);
                    mtMain.AddRow(1, mtMain.RowCount + 1);
                }
            }
            mtMain.LoadFromDataSource();
        }

        private void FillDepartmentColumn(SAPbouiCOM.Column pCombo)
        {
            try
            {
                IEnumerable<MstDepartment> Departments = from a in dbHrPayroll.MstDepartment select a;
                pCombo.ValidValues.Add("0", "Parent Department");
                foreach (MstDepartment One in Departments)
                {
                    pCombo.ValidValues.Add(One.ID.ToString(), One.DeptName);
                }
                //dtDept.SetValue(pCombo.DataBind.Alias, 0, 1);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void PrintDepartmentHiearcyReport()
        {
            try
            {
                TblRpts oReport = (from a in dbHrPayroll.TblRpts where a.RptCode.Contains("DeptHV") select a).FirstOrDefault();
                if (oReport != null)
                {
                    Program.objHrmsUI.printRpt("DeptHV", true, "","");
                }
                else
                {
                    oApplication.StatusBar.SetText("Attach Department Reports First.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Exception @ PrintDepartmentHiearcyReport : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion
    }
}
