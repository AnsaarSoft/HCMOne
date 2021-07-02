using DIHRMS;
using DIHRMS.Custom;
using System;
using System.Data;
using System.Linq;

namespace ACHR.Screen
{
    class frm_RepotViewerSett : HRMSBaseForm
    {

        #region "Global Variable & objects"
        //Form all Element Objects
        SAPbouiCOM.Button btSave;        
        SAPbouiCOM.Item btnCanceli;     

        SAPbouiCOM.EditText txtUrl;
        SAPbouiCOM.CheckBox chkFlg;        
        // Variables
        Boolean HaveRecord = false;
        #endregion

        #region "Events"

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "btSave":
                    FormState();
                    break;
                case "2":
                    
                    break;                
                default:
                    break;
            }
        }

        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);            
            try
            {
                btSave = oForm.Items.Item("btSave").Specific;                
                btnCanceli = oForm.Items.Item("2");
                oForm.DataSources.UserDataSources.Add("txtUrl", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 100);
                txtUrl = oForm.Items.Item("txtUrl").Specific;
                txtUrl.DataBind.SetBound(true, "", "txtUrl");

                oForm.DataSources.UserDataSources.Add("chkFlg", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 1);
                chkFlg = oForm.Items.Item("chkFlg").Specific;
                chkFlg.DataBind.SetBound(true, "", "chkFlg");
                GetRecordFromDB();                
            }
            catch (Exception Ex)
            {
                oApplication.SetStatusBarMessage(Ex.Message, SAPbouiCOM.BoMessageTime.bmt_Medium, true);
                //Program.objHrmsUI.getStrMsg("xception");
            }
        }

        #endregion 

        #region "Local Methods"
        
        private void AddSetting()
        {
            
            try
            {
                if (!HaveRecord)
                {
                    CfgReportViewer oRpt = new CfgReportViewer();
                    oRpt.WebReportServerURL = txtUrl.Value;
                    oRpt.EnableWebReport = chkFlg.Checked;                     
                    dbHrPayroll.CfgReportViewer.InsertOnSubmit(oRpt);
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

        private void UpdateSetting()
        {
            try
            {
                if (HaveRecord)
                {
                    var oRec = (from a in dbHrPayroll.CfgReportViewer
                                       where a.Id==1
                                       select a).FirstOrDefault();
                    oRec.WebReportServerURL = txtUrl.Value;
                    oRec.EnableWebReport = chkFlg.Checked;

                    dbHrPayroll.SubmitChanges();
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                    oApplication.StatusBar.SetText("Configration Updated Successfully.", SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Success);
                }
            }
            catch (Exception Ex)
            {
            }
        }

       

        private void GetRecordFromDB()
        {
            
            try
            {
                var oRec = (from a in dbHrPayroll.CfgReportViewer
                                          where a.Id == 1
                                          select a
                                          ).FirstOrDefault();
                if (oRec!= null)
                {
                    oForm.Freeze(true);
                    txtUrl.Value = oRec.WebReportServerURL;
                    chkFlg.Checked = oRec.EnableWebReport.GetValueOrDefault();
                    oForm.Freeze(false);
                    HaveRecord = true;
                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;
                }
                else
                {                    
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
            //if (oForm.Mode == SAPbouiCOM.BoFormMode.fm_ADD_MODE)
            if(!HaveRecord)
            {
                AddSetting();
            }else
            //else if ( oForm.Mode == SAPbouiCOM.BoFormMode.fm_UPDATE_MODE )
            {
                UpdateSetting();
            }
        }

        #endregion
    }
}
