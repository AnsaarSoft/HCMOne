using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPbouiCOM;
using SAPbobsCOM;
namespace ACHR.Common
{
    interface IntHrForm
    {
        
        // General Methods
        void CreateForm(Application SboApp, string strXml, SAPbobsCOM.Company cmp, string frmId);
        
        //Event Handlers
        void BeforeClick(ref ItemEvent pVal , ref bool BubbleEvent);
        void AfterClick(ref ItemEvent pVal, ref bool BubbleEvent);
        void BeforeCfl(ref ItemEvent pVal, ref bool BubbleEvent);
        void AfterCfl(ref ItemEvent pVal, ref bool BubbleEvent);
        void BeforeCmbSelect(ref ItemEvent pVal, ref bool BubbleEvent);
        void AfterCmbSelect(ref ItemEvent pVal, ref bool BubbleEvent);
        void BeforeLostFocus(ref ItemEvent pVal, ref bool BubbleEvent);
        void AfterLostFocus(ref ItemEvent pVal, ref bool BubbleEvent);
        void BeforeGetFocus(ref ItemEvent pVal, ref bool BubbleEvent);
        void AfterGotFocus(ref ItemEvent pVal, ref bool BubbleEvent);

        //Nevigation
        void getNextRecord();
        void getPreviouRecord();
        void AddNewRecord();
        void getLastRecord();
        void getFirstRecord();
    }
}
