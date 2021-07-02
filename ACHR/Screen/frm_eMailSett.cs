using DIHRMS;
using DIHRMS.Custom;
using System;
using System.Data;
using System.Linq;
using SAPbouiCOM;

namespace ACHR.Screen
{
    class frm_eMailSett:HRMSBaseForm
    {

        #region "Global Variable & objects"
        //Form all Element Objects
        SAPbouiCOM.Button btnAdd;
        SAPbouiCOM.Button btnCancel;
        SAPbouiCOM.Item btnCanceli;
        SAPbouiCOM.Button btnTest;
        SAPbouiCOM.Item btnTesti;
        SAPbouiCOM.CheckBox chkSSL;

        SAPbouiCOM.EditText txtSmtpServer;
        SAPbouiCOM.EditText txtSmtpPort;
        SAPbouiCOM.EditText txtFromEmail;
        SAPbouiCOM.EditText txtPsw;
        SAPbouiCOM.EditText txtTestEmail;
        // Variables
        Boolean HaveRecord = false;
        #endregion

        #region "Events"

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "1":
                    //FormState();
                    break;
                case "2":
                    
                    break;
                case "btnTest":
                    TestEmail();
                    break;
                default:
                    break;

            }
        }

        public override void etBeforeClick(ref ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etBeforeClick(ref pVal, ref BubbleEvent);
            switch(pVal.ItemUID)
            {
                case "1":
                    FormState();
                    break;
            }
        }

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            //Assign Object to Form Elements.
            try
            {
                btnAdd = oForm.Items.Item("1").Specific;
                //btnAddi = oForm.Items.Item("btnAdd");
                //btnCancel = oForm.Items.Item("btnCancel").Specific;
                btnCanceli = oForm.Items.Item("2");
                //btnTest = oForm.Items.Item("btnTest").Specific;
                btnTesti = oForm.Items.Item("btnTest");
                txtSmtpServer = oForm.Items.Item("txtSmtpSer").Specific;
                txtSmtpPort = oForm.Items.Item("txtPort").Specific;
                txtFromEmail = oForm.Items.Item("txtFromEm").Specific;
                txtPsw = oForm.Items.Item("txtPsw").Specific;
                txtTestEmail = oForm.Items.Item("txtToEmail").Specific;
                chkSSL = oForm.Items.Item("chssl").Specific;
                oForm.DataSources.UserDataSources.Add("chssl", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkSSL.DataBind.SetBound(true, "", "chssl");

                GetRecordFromDB();
                FormState();
            }
            catch (Exception Ex)
            {
                oApplication.SetStatusBarMessage(Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, true);
                //Program.objHrmsUI.getStrMsg("xception");
            }
        }

        #endregion 

        #region "Local Methods"
        
        private void AddEmailSetting()
        {
            
            try
            {
                if (!HaveRecord)
                {
                    MstEmailConfig oEmail = new MstEmailConfig();
                    oEmail.SMTPServer = txtSmtpServer.Value;
                    oEmail.SMTPort = Convert.ToInt16(txtSmtpPort.Value);
                    oEmail.FromEmail = txtFromEmail.Value;
                    oEmail.Password = txtPsw.Value;
                    oEmail.TestEmail = txtTestEmail.Value;
                    oEmail.SSL = chkSSL.Checked;

                    dbHrPayroll.MstEmailConfig.InsertOnSubmit(oEmail);
                    dbHrPayroll.SubmitChanges();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    oApplication.StatusBar.SetText("Configration Saved Successfully", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void UpdateEmailSetting()
        {
            try
            {
                if (HaveRecord)
                {
                    var EmailRecord = (from a in dbHrPayroll.MstEmailConfig
                                       select a).FirstOrDefault();
                    EmailRecord.SMTPServer = txtSmtpServer.Value;
                    EmailRecord.SMTPort = Convert.ToInt16(txtSmtpPort.Value);
                    EmailRecord.FromEmail = txtFromEmail.Value;
                    EmailRecord.Password = txtPsw.Value;
                    EmailRecord.TestEmail = txtTestEmail.Value;
                    EmailRecord.SSL = chkSSL.Checked;

                    dbHrPayroll.SubmitChanges();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    oApplication.StatusBar.SetText("Configration Updated Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
            }
            catch (Exception Ex)
            {
            }
        }

        private void TestEmail()
        {
            try
            {
                var EmailRecord = (from a in dbHrPayroll.MstEmailConfig
                                   select a).FirstOrDefault();
                //DIHRMS.Custom.eMail.SendEmail(EmailRecord.TestEmail, "ToEmail Name", "Hello Test email hai", "same as message subject");
                ds.SendEmail(EmailRecord.TestEmail, "ToEmail Name", "Hello Test Email Hai", "Same message as subject");
                oApplication.StatusBar.SetText("Test Email sent", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}",Ex.Message);
            }

        }

        private void GetRecordFromDB()
        {
            //Variable & Object
            
            //Logic
            try
            {

                Int32 AvailableRecords = (from a in dbHrPayroll.MstEmailConfig
                                          select a).Count();

                if (AvailableRecords > 0)
                {
                    var Records = (from b in dbHrPayroll.MstEmailConfig
                                   select b).FirstOrDefault();

                    oForm.Freeze(true);
                    txtSmtpServer.Value = Records.SMTPServer;
                    txtSmtpPort.Value = Convert.ToString(Records.SMTPort);
                    txtFromEmail.Value = Records.FromEmail;
                    txtPsw.Value = Records.Password;
                    txtTestEmail.Value = Records.TestEmail;
                    chkSSL.Checked = Convert.ToBoolean(Records.SSL);

                    oForm.Freeze(false);
                    HaveRecord = true;
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                }
                else
                {
                    //Program.objHrmsUI.getStrMsg("ConError");
                    HaveRecord = false;
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
                }
            }
            catch ( Exception Ex)
            {
            }
            //Assignments
        }

        private void FormState()
        {
            if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
            {
                AddEmailSetting();
            }
            else if ( oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE )
            {
                UpdateEmailSetting();
            }
        }

        #endregion
    }
}
