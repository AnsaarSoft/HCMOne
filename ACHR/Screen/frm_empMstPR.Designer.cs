using System;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;


namespace ACHR.Screen
{
    partial class frm_empMstPR : HRMSBaseForm
    {
        Hashtable editBoxes = new  Hashtable();
        Hashtable comboBoxes = new Hashtable();
        Hashtable chkBoxes = new Hashtable();

        SAPbouiCOM.EditText txFname, txMName, txLname, txJobT, txLoginId, txPwd, txEmpCode, txIni, txNamePre, txOffPhone, txExt, txMobPhone, txPager, txHomPhone, txFax, txEmail, txid;
        SAPbouiCOM.EditText txBasic, txCal, txShift, txDoj, txAcctTitl, txBnkNam, txBrnch, txAcctNam, txEffDate, txPercent, txFather, txMother, txSSN, txUninMem, txUninNum, txNatnlty;
        SAPbouiCOM.EditText txPssprt, txPsprtDt, txPsprtExp, txITaxNum, txIdNum, txIDIsDate, txIdPlace, txIdIssBy, txIdExpDt;
        SAPbouiCOM.ComboBox cbLoc, cbPos, cbDept, cbBranch, cbMgr, cbDesign, cbCurr, cbPmtMode, cbAccType, cbReligion, cbMarStat, cbPayroll, cbSBOUsr;
        SAPbouiCOM.CheckBox chActive;
        SAPbouiCOM.Item ItxEmpCode, ItxLoginId;
        SAPbouiCOM.DataTable dtHead, dtpayroll, dtpersonal;

        SAPbouiCOM.DataTable dtAccrual;

        private void setDataTables()
        {
            dtHead.SetValue("active", 0, "Y");
        }
        private void InitiallizeForm()
        {
            oForm.Freeze(true);
            oForm.PaneLevel = 1;
            oForm.DefButton = "1";
             dtHead = oForm.DataSources.DataTables.Item("dtHead");
             dtpayroll = oForm.DataSources.DataTables.Item("dtpayroll");
             dtpersonal = oForm.DataSources.DataTables.Item("dtpersonal");
             dtHead.Rows.Add(1);

             dtpayroll.Rows.Add(1);
             dtpersonal.Rows.Add(1);
             setDataTables();
             ItxEmpCode = oForm.Items.Item("txEmpCode");
            txFname = oForm.Items.Item("txFname").Specific;
            txMName = oForm.Items.Item("txMName").Specific;
            txLname = oForm.Items.Item("txLname").Specific;
            txJobT = oForm.Items.Item("txJobT").Specific;
            txLoginId = oForm.Items.Item("txLoginId").Specific;
            ItxLoginId = oForm.Items.Item("txLoginId");
            txPwd = oForm.Items.Item("txPwd").Specific;
            txEmpCode = oForm.Items.Item("txEmpCode").Specific;
            txIni = oForm.Items.Item("txIni").Specific;
            txNamePre = oForm.Items.Item("txNamePre").Specific;
            txOffPhone = oForm.Items.Item("txOffPhone").Specific;

            txExt = oForm.Items.Item("txExt").Specific;
            txMobPhone = oForm.Items.Item("txMobPhone").Specific;
            txPager = oForm.Items.Item("txPager").Specific;
            txHomPhone = oForm.Items.Item("txHomPhone").Specific;
            txFax = oForm.Items.Item("txFax").Specific;
            txEmail = oForm.Items.Item("txEmail").Specific;
            txid = oForm.Items.Item("txid").Specific;

            //, , , , , , , , , , , , , , , ;
            txBasic = oForm.Items.Item("txBasic").Specific;
            txCal = oForm.Items.Item("txCal").Specific;
            txShift = oForm.Items.Item("txShift").Specific;
            txDoj = oForm.Items.Item("txDoj").Specific;
            txAcctTitl = oForm.Items.Item("txAcctTitl").Specific;
            txBnkNam = oForm.Items.Item("txBnkNam").Specific;
            txBrnch = oForm.Items.Item("txBrnch").Specific;
            txAcctNam = oForm.Items.Item("txAcctNam").Specific;
            txEffDate = oForm.Items.Item("txEffDate").Specific;
            txPercent = oForm.Items.Item("txPercent").Specific;
            txFather = oForm.Items.Item("txFather").Specific;
            txMother = oForm.Items.Item("txMother").Specific;
            txSSN = oForm.Items.Item("txSSN").Specific;
            txUninMem = oForm.Items.Item("txUninMem").Specific;
            txUninNum = oForm.Items.Item("txUninNum").Specific;
            txNatnlty = oForm.Items.Item("txNatnlty").Specific;

            //, , , , , , , , 
            txPssprt = oForm.Items.Item("txPssprt").Specific;
            txPsprtDt = oForm.Items.Item("txPsprtDt").Specific;
            txPsprtExp = oForm.Items.Item("txPsprtExp").Specific;
            txITaxNum = oForm.Items.Item("txITaxNum").Specific;
            txIdNum = oForm.Items.Item("txIdNum").Specific;
            txIDIsDate = oForm.Items.Item("txIDIsDate").Specific;
            txIdPlace = oForm.Items.Item("txIdPlace").Specific;
            txIdIssBy = oForm.Items.Item("txIdIssBy").Specific;
            txIdExpDt = oForm.Items.Item("txIdExpDt").Specific;

            //, , , , , , , , , , ;
            cbLoc = oForm.Items.Item("cbLoc").Specific;
            cbPos = oForm.Items.Item("cbPos").Specific;
            cbDept = oForm.Items.Item("cbDept").Specific;
            cbBranch = oForm.Items.Item("cbBranch").Specific;
            cbMgr = oForm.Items.Item("cbMgr").Specific;
            cbDesign = oForm.Items.Item("cbDesign").Specific;
            cbCurr = oForm.Items.Item("cbCurr").Specific;
            cbPmtMode = oForm.Items.Item("cbPmtMode").Specific;
            cbAccType = oForm.Items.Item("cbAccType").Specific;
            cbReligion = oForm.Items.Item("cbReligion").Specific;
            cbMarStat = oForm.Items.Item("cbMarStat").Specific;
            cbPayroll = oForm.Items.Item("cbPayroll").Specific;
            cbSBOUsr = oForm.Items.Item("cbSBOUsr").Specific;
            chActive = oForm.Items.Item("chActive").Specific;


            oForm.Freeze(false);
          

        }
        public void iniControlls()
        {
            oForm.Freeze(true);
            try
            {
                txEmpCode.Value = "";
                txFname.Value = "";
                txLname.Value = "";
                txMName.Value = "";
                txJobT.Value = "";
                txLoginId.Value = "";
                cbDept.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                txid.Value = "0";
                txPwd.Value = "";
                cbLoc.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbPos.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbBranch.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbMgr.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbDesign.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbPayroll.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                cbSBOUsr.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                txIni.Value = "";
                txNamePre.Value = "";
                txOffPhone.Value = "";
                txExt.Value = "";
                txMobPhone.Value = "";
                txPager.Value = "";
                txHomPhone.Value = "";
                txFax.Value = "";
                txEmail.Value = "";
                chActive.Checked = false;

                txBasic.Value = "0.00";
                cbCurr.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                txCal.Value = "";
                txShift.Value = "";
                txDoj.Value = "";
                cbPmtMode.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                txAcctTitl.Value = "";
                txBnkNam.Value = "";
                txBrnch.Value = "";
                txAcctNam.Value = "";
                cbAccType.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                txEffDate.Value = "";
                txPercent.Value = "0.00";

                txFather.Value = "";
                txMother.Value = "";
                cbReligion.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                txSSN.Value = "";
                cbMarStat.Select(0, SAPbouiCOM.BoSearchKey.psk_Index);
                txUninMem.Value = "";
                txUninNum.Value = "";
                txNatnlty.Value = "";
                txPssprt.Value = "";
                txPsprtDt.Value = "";
                txPsprtExp.Value = "";
                txITaxNum.Value = "";
                txIdNum.Value = "";
                txIDIsDate.Value = "";
                txIdPlace.Value = "";
                txIdIssBy.Value = "";
                txIdExpDt.Value = "";
            }
            catch(Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);
            }
            oForm.Freeze(false);
        }
    }
}
