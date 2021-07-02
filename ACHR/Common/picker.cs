using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using SAPbobsCOM;
using SAPbouiCOM;
namespace ACHR
{
    public class picker
    {
        private SAPbouiCOM.Application oApplication;
        private string sInput = "";
        private string sTitle = "";
        public bool flgMultiple = false;
        private bool bLoadInputEvents;
        private System.Data.DataTable dtTable;
        private System.Data.DataTable dtOut = new System.Data.DataTable() ;
        private SAPbouiCOM.DataTable dtSearch;
        SAPbouiCOM.Matrix mtSearch;
        SAPbouiCOM.Form oform;
        SAPbouiCOM.EditText SearchField;
        SAPbouiCOM.Item IbtChoos, ISearchField;
        SAPbouiCOM.Button btChoos;
        
        public picker(SAPbouiCOM.Application app, System.Data.DataTable dt)
        {
            oApplication = app;
            oApplication.ItemEvent += new _IApplicationEvents_ItemEventEventHandler(ItemEvents);
            dtTable = dt;
        }
        
        public System.Data.DataTable ShowInput(string Title, string Message)
        {

            try
            {
                bLoadInputEvents = true;
                sTitle = Title;
                oApplication.MessageBox(Message);
                
            }
            catch
            {
                bLoadInputEvents = false;
            }
            return dtOut;
        }
        
        // handles Form event trafic for the form associated to this class instance
        public virtual void ItemEvents(string FormUID, ref SAPbouiCOM.ItemEvent pVal, out bool BubbleEvent)
        {
            BubbleEvent = true;
            try
            {


                switch (pVal.EventType)
                {

                    case BoEventTypes.et_ITEM_PRESSED:

                        e_ItemPressed(ref pVal, ref BubbleEvent);

                        break;
                    case BoEventTypes.et_FORM_LOAD:

                        e_FormLoad(ref pVal, ref BubbleEvent);

                        break;
                    case BoEventTypes.et_FORM_CLOSE:
                        bLoadInputEvents = false;
                        break;

                }




            }
            catch (Exception ex)
            {
                //oApplication.MessageBox(ex.Message);
                bLoadInputEvents = false;
            }



        }

        protected virtual void e_FormLoad(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {

            try
            {
                if (pVal.BeforeAction == false && bLoadInputEvents)
                {
                    oform = oApplication.Forms.Item(pVal.FormUID);
                    oform.ClientWidth = 400;
                    oform.ClientHeight = 400;
                    dtSearch = oform.DataSources.DataTables.Add("dtSearch");
                    // oform.Width = 600;
                    oform.Title = sTitle;

                    oform.State = BoFormStateEnum.fs_Maximized;
                    SAPbouiCOM.Item oItem;
                    IbtChoos = oform.Items.Item("1");                   
                    IbtChoos.Top = 350;
                    btChoos = IbtChoos.Specific;
                    btChoos.Caption = "Choose";
                    ISearchField = oform.Items.Item("2");
                    ISearchField.Top = 350;
                    
                    SAPbouiCOM.Columns oColumns;
                    SAPbouiCOM.DataColumns dtCols;
                    dtCols = dtSearch.Columns;
                    SAPbouiCOM.Column oColumn;
                    SAPbouiCOM.DataColumn dtCol;
                    oItem = oform.Items.Add("mtSearch", BoFormItemTypes.it_MATRIX);
                    oItem.Width = 400;
                    oItem.Height = 300;
                    oItem.Top = 40;


                    mtSearch = oItem.Specific;
                    if (flgMultiple)
                    {
                    }
                    else
                    {
                        mtSearch.SelectionMode = BoMatrixSelect.ms_Single;
                    }
                    oColumns = mtSearch.Columns;
                    //mtSearch.Layout = BoMatrixLayoutType.mlt_Vertical;
                    oColumn = oColumns.Add("vFix", SAPbouiCOM.BoFormItemTypes.it_EDIT);
                    oColumn.Editable = false;
                    oColumn.Width = 40;
                    //oColumn.Visible = false;
                    int i = 0;
                    int j = 0;
                    foreach (System.Data.DataColumn cl in dtTable.Columns)
                    {
                        dtOut.Columns.Add(cl.ColumnName);
                        oColumn = oColumns.Add("v_" + i.ToString(), SAPbouiCOM.BoFormItemTypes.it_EDIT);
                        oColumn.TitleObject.Caption = cl.Caption;
                        oColumn.Width = 100;
                        oColumn.Editable = false;
                        dtCol = dtCols.Add("cd" + i.ToString(), BoFieldsType.ft_AlphaNumeric);
                        oColumn.DataBind.Bind("dtSearch", "cd" + i.ToString());

                        i++;
                        // col.DataBind.TableName = "dtSearch";
                        //col.DataBind

                    }
                    dtSearch.Rows.Clear();
                    i = 0;
                    j = 0;
                    foreach (DataRow dr in dtTable.Rows)
                    {
                        dtSearch.Rows.Add(1);
                        j = 0;
                        foreach (System.Data.DataColumn col in dtTable.Columns)
                        {
                            dtSearch.SetValue("cd" + j.ToString(), i, dr[j].ToString());
                            j++;
                        }
                        i++;
                    }
                    mtSearch.LoadFromDataSource();
                    mtSearch.AutoResizeColumns();
                    oform = null;

                }
            }
            catch (Exception ex)
            {
                bLoadInputEvents = false;
            }
            

        }

        protected virtual void e_ItemPressed(ref SAPbouiCOM.ItemEvent pVal, ref bool BubbleEvent)
        {

            if (pVal.ItemUID == "1" & pVal.BeforeAction)
            {
                /*

                sInput = "Success";
                if (string.IsNullOrEmpty(sInput))
                {
                    BubbleEvent = false;
                }
                else
                {
                    bLoadInputEvents = false;
                }
                 * */
                //int confirm = oApplication.MessageBox("Are you sure you want to Login into " + dtOut.Rows[0][1].ToString(), 3, "Yes", "No", "Cancel");
                //if (confirm == 2 || confirm == 3)
                //{
                //    dtOut.Rows.Clear();
                //    BubbleEvent = true;
                //}
                //else
                //{

                //}
            }

            if (pVal.ItemUID == "mtSearch" && pVal.BeforeAction)
            {
                int rowNum = pVal.Row;
                System.Data.DataRow dr = dtOut.NewRow();

                for (int i=0;  i< dtOut.Columns.Count ;i++)
                {
                    dr[i] = dtTable.Rows[rowNum - 1][i].ToString();
                }
                dtOut.Rows.Add(dr);
               // BubbleEvent = false;
                //bLoadInputEvents = false;
                IbtChoos.Click();
            }

        }

    }
}