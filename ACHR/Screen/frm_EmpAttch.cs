using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIHRMS;
using DIHRMS.Custom;
using System.Data;
using System.IO;
using System.Drawing;

namespace ACHR.Screen
{
    class frm_EmpAttch : HRMSBaseForm
    {
        #region "Global Variable Area"


        SAPbouiCOM.Button btSave, btCancel;
        SAPbouiCOM.EditText txtReqBy, txtEmpCode, txtManager, txtdoj, txtdesig, txtSalary, txtPath1, txtPath2, txtPath3;
        SAPbouiCOM.Button btId, btnPath1, btnPath2, btnPath3;
        SAPbouiCOM.PictureBox picBox1, picBoX2, picBoX3 ;
        String FilePath,FilePath2,FilePath3, picPath;

        #endregion

        #region "B1 Events"

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            try
            {
                oForm.Freeze(true);
                oForm.EnableMenu("1290", false);  // First Record
                oForm.EnableMenu("1291", false);  // Last record 
                InitiallizeForm();               
                oForm.Freeze(false);
                if (!string.IsNullOrEmpty(Program.EmpID))
                {
                    LoadSelectedData(Program.EmpID);
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AdvncReq Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            try
            {
                switch (pVal.ItemUID)
                {
                    case "1":
                        if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                            InsertAttachmentRequest();
                        break;
                    case "btId":
                        OpenNewSearchForm();
                        break;
                    case "btnPath1":
                        LoadPath1File();
                        break;
                    case "btnPath2":
                        LoadPath2File();
                        break;
                    case "btnPath3":
                        LoadPath3File();
                        break;
                    case "btnCanc":
                        Program.EmpID = "";
                        oForm.Close();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: Frm_AdvncReq Function: CreateForm Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        public override void etFormAfterActivate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etFormAfterActivate(ref pVal, ref BubbleEvent);
            if (txtEmpCode == null)
            {
                Program.EmpID = string.Empty;
                return;
            }
            if (Program.EmpID == string.Empty)
            {
                return;
            }
            if (Program.EmpID == txtEmpCode.Value)
            {
                return;
            }
            else
            {
                SetEmpValues();
            }


        }

        public override void FindRecordMode()
        {
            base.FindRecordMode();

            OpenNewSearchForm();

        }
        #endregion

        #region "Local Methods"

        public void InitiallizeForm()
        {
            try
            {
                btSave = oForm.Items.Item("1").Specific;
                btCancel = oForm.Items.Item("btnCanc").Specific;
                btnPath1 = oForm.Items.Item("btnPath1").Specific;
                btnPath2 = oForm.Items.Item("btnPath2").Specific;
                btnPath3 = oForm.Items.Item("btnPath3").Specific;


                picBox1 = oForm.Items.Item("picBoX1").Specific;
                picBoX2 = oForm.Items.Item("picBoX2").Specific;
                picBoX3 = oForm.Items.Item("picBoX3").Specific;

                //Initializing Textboxes
                txtReqBy = oForm.Items.Item("txtRby").Specific;
                txtEmpCode = oForm.Items.Item("txtEmpC").Specific;               
                txtManager = oForm.Items.Item("txtManagr").Specific;

                oForm.DataSources.UserDataSources.Add("dtJoin", SAPbouiCOM.BoDataType.dt_DATE, 30);
                txtdoj = oForm.Items.Item("dtJoin").Specific;
                txtdoj.DataBind.SetBound(true, "", "dtJoin");

                txtdesig = oForm.Items.Item("txtDesig").Specific;
                txtSalary = oForm.Items.Item("txtSalry").Specific;

                txtPath1 = oForm.Items.Item("txtPath1").Specific;
                txtPath2 = oForm.Items.Item("txtPath2").Specific;
                txtPath3 = oForm.Items.Item("txtPath3").Specific;

                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                InitiallizeDocument();
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("InitializeFrom Error : " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        public void InitiallizeDocument()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.AttachEmpID))
                {
                    Program.EmpID = Program.AttachEmpID;
                    SetEmpValues();
                }
            }
            catch (Exception Ex)
            {
            }
        }
        
        private void OpenNewSearchForm()
        {
            try
            {
                Program.EmpID = "";
                string comName = "MstSearch";
                Program.sqlString = "empPick";
                string strLang = "ln_English";
                try
                {
                    oApplication.Forms.Item("frm_" + comName).Select();
                }
                catch
                {
                    //this.oForm.Visible = false;
                    Type oFormType = Type.GetType("ACHR.Screen." + "frm_" + comName);
                    Screen.HRMSBaseForm objScr = ((Screen.HRMSBaseForm)oFormType.GetConstructor(System.Type.EmptyTypes).Invoke(null));
                    objScr.CreateForm(oApplication, "ACHR.XMLScreen." + strLang + ".xml_" + comName + ".xml", oCompany, "frm_" + comName);
                    //this.oForm.Visible = true;
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                }

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void SetEmpValues()
        {
            try
            {
                if (!string.IsNullOrEmpty(Program.EmpID))
                {
                    txtEmpCode.Value = Program.EmpID;
                    LoadSelectedData(txtEmpCode.Value);
                }
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
                if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
                {                   
                    if (!String.IsNullOrEmpty(pCode))
                    {
                        var getEmp = (from a in dbHrPayroll.MstEmployee
                                      where a.EmpID.Contains(pCode)
                                      select a).FirstOrDefault();                       
                        if (getEmp != null)
                        {                                                      
                            txtReqBy.Value = getEmp.FirstName + " " + getEmp.MiddleName + " " + getEmp.LastName;
                            txtManager.Value = (from e in dbHrPayroll.MstEmployee where e.ID == getEmp.Manager select (e.FirstName + " " + e.MiddleName + " " + e.LastName)).FirstOrDefault();
                            txtdoj.Value = getEmp.JoiningDate == null ? "" : Convert.ToDateTime(getEmp.JoiningDate).ToString("yyyyMMdd");
                            txtdesig.Value = getEmp.DesignationName;
                            txtSalary.Value = String.Format("{0:0.00}", ds.getEmpGross(getEmp));
                            //txtSalary.Value = String.Format("{0:0.00}", getEmp.BasicSalary);  
                            LoadSelectedEmpAttachments(getEmp.ID);                                                                                                         
                        }

                    }
                }
                else
                {

                }
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_EmpAttch Function: LoadSelectedData Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void LoadPath1File()
        {
            try
            {

                FilePath = Program.objHrmsUI.FindFile();
                if (!String.IsNullOrEmpty(FilePath))
                {
                    picBox1.Picture = FilePath;
                    txtPath1.Value = FilePath;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Function : LoadImageFile Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void LoadPath2File()
        {
            try
            {

                FilePath2 = Program.objHrmsUI.FindFile();
                if (!String.IsNullOrEmpty(FilePath2))
                {
                    picBoX2.Picture = FilePath2;
                    txtPath2.Value = FilePath2;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Function : LoadImageFile Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void LoadPath3File()
        {
            try
            {

                FilePath3 = Program.objHrmsUI.FindFile();
                if (!String.IsNullOrEmpty(FilePath3))
                {
                    picBoX3.Picture = FilePath3;
                    txtPath3.Value = FilePath3;
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("Function : LoadImageFile Error : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        public static byte[] GetBytesFromFile(string fullFilePath)
        {
            // this method is limited to 2^32 byte files (4.2 GB)

            FileStream fs = File.OpenRead(fullFilePath);

            try
            {
                byte[] bytes = new byte[fs.Length];
                fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
                fs.Close();
                return bytes;
            }
            finally
            {
                fs.Close();
            }

        }
        
        private void InsertAttachmentRequest()
        {
            MstAttachments objAttachment = null;
            try
            {
                int EmpID;
                String pCode = txtEmpCode.Value;
                var getEmp = (from a in dbHrPayroll.MstEmployee
                              where a.EmpID.Contains(pCode)
                              select a).FirstOrDefault();
                if (getEmp == null)
                {
                    oApplication.StatusBar.SetText("Please select Valid Employee Code", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
                    return;
                }
                objAttachment = dbHrPayroll.MstAttachments.Where(a => a.EmpId == getEmp.ID).FirstOrDefault();
                if (objAttachment == null)
                {
                    objAttachment = new MstAttachments();
                    dbHrPayroll.MstAttachments.InsertOnSubmit(objAttachment);
                    objAttachment.EmpId = getEmp.ID;
                    objAttachment.CreatedBy = oCompany.UserName;
                    objAttachment.CreatedDate = DateTime.Now;
                    
                }
                objAttachment.Path1Link = txtPath1.Value.Trim();
                if (!string.IsNullOrEmpty(txtPath1.Value))
                {
                    objAttachment.Path1Image = GetBytesFromFile(txtPath1.Value.Trim());
                    
                }
                objAttachment.Path2Link = txtPath2.Value.Trim();
                if (!string.IsNullOrEmpty(txtPath2.Value))
                {
                    objAttachment.Path2Image = GetBytesFromFile(txtPath2.Value.Trim());
                    
                }
                objAttachment.Path3Link = txtPath3.Value.Trim();
                if (!string.IsNullOrEmpty(txtPath3.Value))
                {
                    objAttachment.Path3Image = GetBytesFromFile(txtPath3.Value.Trim());
                    
                }
                objAttachment.UpdatedBy = oCompany.UserName;
                objAttachment.UpdatedDate = DateTime.Now;
                dbHrPayroll.SubmitChanges();
                oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                oApplication.StatusBar.SetText(Program.objHrmsUI.getStrMsg("RecordSavedSuccessfully"), SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                oApplication.StatusBar.SetText("Form: frm_EmpAttch Function: InsertAttachmentRequest Msg: " + Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        private void LoadSelectedEmpAttachments(int EmpId)
        {
            try
            {
                var objAttachmentRecord = dbHrPayroll.MstAttachments.Where(a => a.EmpId == EmpId).FirstOrDefault();
                if (objAttachmentRecord != null)
                {
                    if (!string.IsNullOrEmpty(objAttachmentRecord.Path1Link))
                    {
                        txtPath1.Value = objAttachmentRecord.Path1Link.Trim();
                        picBox1.Picture = txtPath1.Value.Trim();
                    }
                    if (!string.IsNullOrEmpty(objAttachmentRecord.Path2Link))
                    {
                        txtPath2.Value = objAttachmentRecord.Path2Link.Trim();
                        picBoX2.Picture = txtPath2.Value.Trim();
                    }
                    if (!string.IsNullOrEmpty(objAttachmentRecord.Path3Link))
                    {
                        txtPath3.Value = objAttachmentRecord.Path3Link.Trim();
                        picBoX3.Picture = txtPath3.Value.Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText(ex.Message + " Trace: " + ex.StackTrace, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }
        
        public static Bitmap ByteToImage(byte[] blob)
        {
            MemoryStream mStream = new MemoryStream();
            byte[] pData = blob;
            mStream.Write(pData, 0, Convert.ToInt32(pData.Length));
            Bitmap bm = new Bitmap(mStream, false);
            mStream.Dispose();
            return bm;

        }

        #endregion
    }
}
