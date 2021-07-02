using System;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;

namespace ACHR.Screen
{
    class frm_RetroSet : HRMSBaseForm
    {
        SAPbouiCOM.Matrix mtElement;
        SAPbouiCOM.EditText txCode, txDescr, txEffe, txId;
        // SAPbouiCOM.ComboBox cbPayroll, cbPeriod, cbDept, cbLoc;
        SAPbouiCOM.Button cmdPrev, cmdNext, cmdNew;
        SAPbouiCOM.Item ItxCode, ItxDescr, ItxEffe, IcmdPrev, IcmdNext, IcmdNew, ItxId;
        SAPbouiCOM.DataTable dtElement;
        public int currentRecord = 0;
        public int totalRecord = 0;
        public IEnumerable<MstRetroElementSet> retroElement;
        public Hashtable CodeIndex = new Hashtable();
        
       
        public override void etAfterValidate(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterValidate(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "mtElement":
                    int rowNum = pVal.Row;
                    SAPbouiCOM.EditText oitm = mtElement.GetCellSpecific("Code", rowNum);
                    string strElementCode = oitm.Value;

                    setElementInfo(strElementCode, rowNum - 1);

                    break;

            }

        }
        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {

                case "cmdNext":
                    getNextRecord();
                    break;
                case "cmdPrev":
                    getPreviouRecord();
                    break;

                case "cmdNew":

                     addNew();
                     break;
                case "1":
                    submitForm();
                    break;
            }
        }
        private void addNew()
        {
            IniContrls();
            ItxCode.Enabled = true;
            oForm.Refresh();
            txDescr.Active = true;
            txCode.Active = true;
            //oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;
        }
        private void getData()
        {
            CodeIndex.Clear();
            retroElement = from p in dbHrPayroll.MstRetroElementSet select p;
            int i = 0;
            foreach (MstRetroElementSet ele in retroElement)
            {
                CodeIndex.Add(ele.Id.ToString(), i);
                i++;
            }
            totalRecord = i;
        }
        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            InitiallizeForm();
        }

        private void InitiallizeForm()
        {
            oForm.Freeze(true);

           

            oForm.DataSources.UserDataSources.Add("txCode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); // Days of Month
            txCode = oForm.Items.Item("txCode").Specific;
            ItxCode = oForm.Items.Item("txCode");
            txCode.DataBind.SetBound(true, "", "txCode");

            oForm.DataSources.UserDataSources.Add("txDescr", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 50); // Hours Per Day
            txDescr = oForm.Items.Item("txDescr").Specific;
            ItxDescr = oForm.Items.Item("txDescr");
            txDescr.DataBind.SetBound(true, "", "txDescr");

            oForm.DataSources.UserDataSources.Add("txEffe", SAPbouiCOM.BoDataType.dt_DATE); // Hours Per Day
            txEffe = oForm.Items.Item("txEffe").Specific;
            ItxEffe = oForm.Items.Item("txEffe");
            txEffe.DataBind.SetBound(true, "", "txEffe");

            oForm.DataSources.UserDataSources.Add("txId", SAPbouiCOM.BoDataType.dt_SHORT_TEXT,30); // Hours Per Day
            txId = oForm.Items.Item("txId").Specific;
            ItxId = oForm.Items.Item("txId");
            txId.DataBind.SetBound(true, "", "txId");

            cmdPrev = oForm.Items.Item("cmdPrev").Specific;
            IcmdPrev = oForm.Items.Item("cmdPrev");

            cmdNext = oForm.Items.Item("cmdNext").Specific;
            IcmdNext = oForm.Items.Item("cmdNext");

            cmdNew = oForm.Items.Item("cmdNew").Specific;
            IcmdNew = oForm.Items.Item("cmdNew");

            mtElement = oForm.Items.Item("mtElement").Specific;
            mtElement.Columns.Item("isNew").Visible = false;
            mtElement.Columns.Item("id").Visible = false;
            dtElement = oForm.DataSources.DataTables.Item("dtElement");
            dtElement.Rows.Clear();



            oForm.Freeze(false);



           
            oForm.PaneLevel = 1;
            IniContrls();
        }
        private void IniContrls()
        {
            getData();
            ItxId.Visible = false;
            txId.Value = "0";
            txCode.Value = "";
            txEffe.Value = "";
            txDescr.Value = "";
            txCode.Active = true;
            oForm.Mode = SAPbouiCOM.BoFormMode.fm_ADD_MODE;

            dtElement.Rows.Clear();
            addEmptyRowElement();

        }
        private void addEmptyRowElement()
        {
            try
            {
                // mtElement.FlushToDataSource();
                if (dtElement.Rows.Count == 0)
                {
                    dtElement.Rows.Add(1);
                    dtElement.SetValue("isNew", 0, "Y");
                    dtElement.SetValue("id", 0, 0);
                    dtElement.SetValue("Code", 0, "");
                    dtElement.SetValue("Descr", 0, "");
                    mtElement.AddRow(1, 0);
                    // mtElement.SetLineData(1);
                }
                else
                {
                    if (dtElement.GetValue("Code", dtElement.Rows.Count - 1) == "")
                    {
                    }
                    else
                    {
                        dtElement.Rows.Add(1);
                        dtElement.SetValue("isNew", dtElement.Rows.Count - 1, "Y");
                        dtElement.SetValue("id", dtElement.Rows.Count - 1, 0);
                        dtElement.SetValue("Code", dtElement.Rows.Count - 1, "");
                        dtElement.SetValue("Descr", dtElement.Rows.Count - 1, "");
                        mtElement.AddRow(1, mtElement.RowCount);


                    }

                }

                mtElement.LoadFromDataSourceEx();
                // mtElement.LoadFromDataSource();

            }
            catch (Exception ex)
            {
                string errmsg = ex.Message;

            }
        }
        private void setElementInfo(string ele, int rowNum)
        {
            int cnt = (from p in dbHrPayroll.MstElements where p.ElementName == ele select p).Count();
            if (cnt > 0)
            {
                MstElements element = (from p in dbHrPayroll.MstElements where p.ElementName == ele select p).Single();
                dtElement.SetValue("id", rowNum, element.Id);

                dtElement.SetValue("Code", rowNum, element.ElementName);
                dtElement.SetValue("Descr", rowNum, element.Description);
               
                mtElement.SetLineData(rowNum + 1);
                addEmptyRowElement();

            }

        }
        
        private bool submitForm()
        {
            bool submitResult = true;
            try
            {
                MstRetroElementSet retroSet;
               
               
                int cnt = (from p in dbHrPayroll.MstRetroElementSet where p.Id.ToString() == txId.Value select p).Count();
                if (cnt > 0)
                {
                    retroSet = (from p in dbHrPayroll.MstRetroElementSet where p.Id == Convert.ToInt16(txId.Value) select p).Single();
                   

                }
                else
                {

                    retroSet = new MstRetroElementSet();
                    dbHrPayroll.MstRetroElementSet.InsertOnSubmit(retroSet);
                    retroSet.CreateDate = DateTime.Now;
                    retroSet.UserId = oCompany.UserName;
               
                }

                retroSet.UpdateDate = DateTime.Now;
                retroSet.UpdatedBy = oCompany.UserName;
                retroSet.RetroSetCode = txCode.Value;
                retroSet.RetroSetName = txDescr.Value;
                retroSet.EffectiveDate = DateTime.ParseExact(txEffe.Value, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                for (int i = 0; i < dtElement.Rows.Count; i++)
                {
                    string isNew = Convert.ToString(dtElement.GetValue("isNew", i));
                    string elId = Convert.ToString(dtElement.GetValue("id", i));
                    if (elId != "0" && isNew == "Y")
                    {
                        MstElements mstEle = (from p in dbHrPayroll.MstElements where p.Id.ToString() == elId select p).Single();
                        MstRetroElementDetail retroEle = new MstRetroElementDetail();
                        retroEle.MstElements = mstEle;
                        // retroSet.mst .Add(PayrollElements);o

                        retroSet.MstRetroElementDetail.Add(retroEle);
                    }
                    else
                    {
                    }

                }



                dbHrPayroll.SubmitChanges();
                IniContrls();

            }
            catch (Exception ex)
            {
                oApplication.SetStatusBarMessage(ex.Message);
              
                submitResult = false;
            }
            return submitResult;
        }
         
      
        private void _fillFields()
        {
            oForm.Freeze(true);
            try
            {
                if (currentRecord >= 0)
                {
                    MstRetroElementSet record;
                    record = retroElement.ElementAt<MstRetroElementSet>(currentRecord);
                    txId.Value = record.Id.ToString();

                    txCode.Value = record.RetroSetCode.ToString();
                    txDescr.Value = record.RetroSetName;
                    oForm.DataSources.UserDataSources.Item("txEffe").ValueEx = Convert.ToDateTime(record.EffectiveDate).ToString("yyyyMMdd");
                    dtElement.Rows.Clear();
                    int i = 0;
                    foreach (MstRetroElementDetail pe in record.MstRetroElementDetail)
                    {
                        dtElement.Rows.Add(1);
                        dtElement.SetValue("isNew", dtElement.Rows.Count - 1, "N");
                        dtElement.SetValue("id", dtElement.Rows.Count - 1, pe.Id.ToString());
                        dtElement.SetValue("Code", dtElement.Rows.Count - 1, pe.MstElements.ElementName);
                        dtElement.SetValue("Descr", dtElement.Rows.Count - 1, pe.MstElements.Description);

                        i++;

                    }
                    mtElement.LoadFromDataSource();
                    addEmptyRowElement();
                    ItxCode.Enabled = false;

                    oForm.Mode = SAPbouiCOM.BoFormMode.fm_OK_MODE;


                }
            }
            catch (Exception ex)
            {
                oForm.Freeze(false);
            }
            oForm.Freeze(false);
        }
        public override void getNextRecord()
        {
            base.getNextRecord();
            if (currentRecord + 1 == totalRecord)
            {
                currentRecord = 0;
                oApplication.SetStatusBarMessage(Program.objHrmsUI.getStrMsg("Nev_Rec_Last"), SAPbouiCOM.BoMessageTime.bmt_Short, false);

            }
            else
            {
                currentRecord = currentRecord + 1;
            }
            _fillFields();
        }
        public override void getPreviouRecord()
        {
            base.getPreviouRecord();
            if (currentRecord <= 0)
            {
                currentRecord = totalRecord - 1;
            }
            else
            {
                currentRecord = currentRecord - 1;
            }
            _fillFields();
        }
       
    }
}
