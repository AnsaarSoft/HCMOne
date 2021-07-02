using System;
using System.Data;
using System.Linq;
using DIHRMS;
using System.Collections;
using System.Collections.Generic;
using SAPbobsCOM;

namespace ACHR.Screen
{
    class frm_ItemsSearch : HRMSBaseForm
    {
       
        #region Variable

        SAPbouiCOM.EditText txtItemCode, txtItemName;
        SAPbouiCOM.Item ItxtItemCode, ItxtItemName;
        SAPbouiCOM.Button btnMain, btnCancel, btnSearch;
        SAPbouiCOM.Matrix grdItems;
        SAPbouiCOM.DataTable dtItems;
        SAPbouiCOM.Column cItemCode, cItemName, cSelect;
       
        #endregion

        #region B1 Events


        public override void CreateForm(SAPbouiCOM.Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId)
        {
            base.CreateForm(SboApp, strXml, cmp, frmId);
            InitiallizeForm();
        }

        public override void etAfterClick(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {
            base.etAfterClick(ref pVal, ref BubbleEvent);
            switch (pVal.ItemUID)
            {
                case "btMain":
                    GiveItemsBack();
                    break;
                
            }
        }

        #endregion

        #region Function
        
        private void InitiallizeForm()
        {
            oForm.Freeze(true);
            try
            {
                btnSearch = oForm.Items.Item("btSearch").Specific;
                btnMain = oForm.Items.Item("btMain").Specific;
                btnCancel = oForm.Items.Item("2").Specific;

                oForm.DataSources.UserDataSources.Add("txCode", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30); 
                txtItemCode = oForm.Items.Item("txCode").Specific;
                ItxtItemCode = oForm.Items.Item("txCode");
                txtItemCode.DataBind.SetBound(true, "", "txCode");

                oForm.DataSources.UserDataSources.Add("txName", SAPbouiCOM.BoDataType.dt_SHORT_TEXT, 30);
                txtItemName = oForm.Items.Item("txName").Specific;
                ItxtItemName = oForm.Items.Item("txName");
                txtItemName.DataBind.SetBound(true, "", "txName");

                grdItems = oForm.Items.Item("mtItem").Specific;
                dtItems = oForm.DataSources.DataTables.Item("dtItem");
                cItemCode = grdItems.Columns.Item("clicode"); 
                cItemName = grdItems.Columns.Item("cliname");
                cSelect = grdItems.Columns.Item("clselect");

                LoadAllItemsPerPeice();

            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("InitiallizeForm Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
            oForm.Freeze(false);
        }

        private void LoadAllItemsPerPeice()
        {
            try
            {
                string strItemCode, strItemName;
                DataTable dtItemsInSAP = new DataTable();
                dtItemsInSAP.Columns.Add("ItemCode");
                dtItemsInSAP.Columns.Add("ItemName");
                string strSql = "SELECT ItemCode, ItemName FROM dbo.OITM WHERE ISNULL(U_PerPieceItem,'N') = 'Y'";
                //string strSql1 = "SELECT \"DimActive\" FROM \"ODIM\" WHERE \"DimCode\" = 3";
                SAPbobsCOM.Recordset oRecSet = (SAPbobsCOM.Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);
                oRecSet.DoQuery(strSql);
                while (oRecSet.EoF == false)
                {
                    strItemCode = Convert.ToString(oRecSet.Fields.Item("ItemCode").Value);
                    strItemName = Convert.ToString(oRecSet.Fields.Item("ItemName").Value);
                    if (!string.IsNullOrEmpty(strItemName) && !string.IsNullOrEmpty(strItemCode))
                    {
                        DataRow drOne = dtItemsInSAP.NewRow();
                        drOne["ItemCode"] = strItemCode;
                        drOne["ItemName"] = strItemName;
                        dtItemsInSAP.Rows.Add(drOne);
                    }
                    strItemCode = string.Empty;
                    strItemName = string.Empty;
                    oRecSet.MoveNext();
                }
                if (dtItemsInSAP.Rows.Count > 0)
                {
                    dtItems.Rows.Clear();
                    dtItems.Rows.Add(dtItemsInSAP.Rows.Count);
                    int i = 0;
                    foreach (DataRow One in dtItemsInSAP.Rows)
                    {
                        dtItems.SetValue(cItemCode.DataBind.Alias, i, Convert.ToString(One["ItemCode"]));
                        dtItems.SetValue(cItemName.DataBind.Alias, i, Convert.ToString(One["ItemName"]));
                        dtItems.SetValue(cSelect.DataBind.Alias, i, "Y");
                        i++;
                    }
                    grdItems.LoadFromDataSource();
                }
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("LoadAllItemsPerPeice Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        private void GiveItemsBack()
        {
            try
            {
                if (Program.oSapItems.Count > 0)
                {
                    Program.oSapItems.Clear();
                }
                grdItems.FlushToDataSource();
                for(int i = 0 ; i < dtItems.Rows.Count ; i++)
                {
                    string ItemCode, ItemName, isSelect;
                    ItemCode = dtItems.GetValue(cItemCode.DataBind.Alias, i);
                    ItemName = dtItems.GetValue(cItemName.DataBind.Alias, i);
                    isSelect = dtItems.GetValue(cSelect.DataBind.Alias, i);

                    if (!string.IsNullOrEmpty(isSelect) && isSelect == "Y")
                    {
                        var OneItem = new Program.SapItems();
                        OneItem.ItemCode = ItemCode;
                        OneItem.ItemName = ItemName;
                        Program.oSapItems.Add(OneItem);
                    }
                }
                oForm.Close();
            }
            catch (Exception ex)
            {
                oApplication.StatusBar.SetText("GiveItemsBack Exception : " + ex.Message, SAPbouiCOM.BoMessageTime.bmt_Short, SAPbouiCOM.BoStatusBarMessageType.smt_Error);
            }
        }

        #endregion

    }
}
