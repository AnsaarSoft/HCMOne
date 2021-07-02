using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACHR.Common
{
    class UIMessages
    {
        public enum MessageType
        {
            SuccessMessage, UpdatedMessage, ErrorMessage
        }

        public MessageType MsgType { get; set; }

        public string GetMessageWithID(MessageType pValue)
        {
            string retValue = string.Empty;
            try
            {
                switch(Convert.ToInt32(pValue))
                {
                    case 0:
                        retValue = "Document has been added successfully.";
                        break;
                    case 1:
                        retValue = "Document has been updated successfully.";
                        break;
                    case 2:
                        retValue = "Operation is unsuccessfull, Kindly check logs.";
                        break;
                }
            }
            catch(Exception ex)
            {
                retValue = ex.Message;
            }
            return retValue;
        }
    }
}
