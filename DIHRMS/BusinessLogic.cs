using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using UFFU;
using System.Reflection;

namespace DIHRMS
{
    partial class dbHRMS : System.Data.Linq.DataContext
    {
        //Fi...
        //Class Level Objects & Variables..
        //AppVersion = "920.03.81.10.26";

        public string AppVersion = "920.03.84.10.26";

        #region "General Functions"

        partial void UpdateCfgApprovalDecisionRegister(CfgApprovalDecisionRegister instance)
        {
            //Identify Which DocType is Updated Call its appropiate function.
            switch (instance.DocType)
            {
                case 15:
                    this.ExecuteDynamicUpdate(instance);
                    CheckStageStatusJobRequisition(instance);
                    break;
                case 12:
                    this.ExecuteDynamicUpdate(instance);
                    CheckStageStatusResignation(instance);
                    break;
                case 11:
                    this.ExecuteDynamicUpdate(instance);
                    CheckStageStatusLoan(instance);
                    break;
                case 20:
                    this.ExecuteDynamicUpdate(instance);
                    CheckStageStatusAdvance(instance);
                    break;
                case 13:
                    this.ExecuteDynamicUpdate(instance);
                    CheckStageStatusLeaveRequest(instance);
                    break;
                default:
                    this.ExecuteDynamicUpdate(instance);
                    break;
            }
        }

        partial void UpdateNesk_CfgApprovalDecisionRegister(Nesk_CfgApprovalDecisionRegister instance)
        {
            switch (instance.DocType)
            {
                //case 13:
                //    this.ExecuteDynamicUpdate(instance);
                //    CheckStageStatusLeaveRequest(instance);
                //    break;
                //case 22:
                //    this.ExecuteDynamicUpdate(instance);
                //    CheckStageStatusSalaryChangeRequest(instance);
                //    break;
                //case 23:
                //    this.ExecuteDynamicUpdate(instance);
                //    CheckStageStatusViolationRequest(instance);
                //    break;
                case 24:
                    this.ExecuteDynamicUpdate(instance);
                    CheckStageStatusOverTimeRequest(instance);
                    break;
                default:
                    this.ExecuteDynamicUpdate(instance);
                    break;
            }
        }

        private void InsertLinesStageRegister(Int16 Priorty, Int32 DocNum, Int32 Series, Int16 DocType,
                                               Int32 StageID, Int32 ApprovalTemplateID)
        {
            String StageStatusInAppTemp;
            if (Priorty == 1)
            {
                StageStatusInAppTemp = @"INSERT dbo.CfgDocumentStageRegister
                                                                    ( DocNum ,
                                                                      DocType ,
                                                                      Series ,
                                                                      TempStages ,
                                                                      ApprovalTemp ,
                                                                      StageDecision ,
                                                                      flgCurrentStage
                                                                    )
                                                            VALUES  ( " + DocNum + @" , -- DocNum - int
                                                                      " + DocType + @" , -- DocType - tinyint
                                                                      " + Series + @" , -- Series - int
                                                                      " + StageID + @" , -- TempStages - int
                                                                      " + ApprovalTemplateID + @" , -- ApprovalTemp - int
                                                                      N'P' , -- StageDecision - nvarchar(2)
                                                                      1  -- flgCurrentStage - bit
                                                                    )";
                this.ExecuteCommand(StageStatusInAppTemp);
            }
            else
            {
                StageStatusInAppTemp = @"INSERT dbo.CfgDocumentStageRegister
                                                                    ( DocNum ,
                                                                      DocType ,
                                                                      Series ,
                                                                      TempStages ,
                                                                      ApprovalTemp ,
                                                                      StageDecision ,
                                                                      flgCurrentStage
                                                                    )
                                                            VALUES  ( " + DocNum + @" , -- DocNum - int
                                                                      " + DocType + @" , -- DocType - tinyint
                                                                      " + Series + @" , -- Series - int
                                                                      " + StageID + @" , -- TempStages - int
                                                                      " + ApprovalTemplateID + @" , -- ApprovalTemp - int
                                                                      N'P' , -- StageDecision - nvarchar(2)
                                                                      0  -- flgCurrentStage - bit
                                                                    )";
                this.ExecuteCommand(StageStatusInAppTemp);
            }
        }

        private void InsertLinesInApprovalDecisionRegister(String EmpID, String EmpName, Int32 DocNum, String StageName,
                                                           String ApprovalTempName, Int32 Series, Int32 DocType)
        {
            String InsertInCfgApprovalDecisionRegister = @"INSERT INTO dbo.CfgApprovalDecisionRegister
                                                                                ( EmpID ,
                                                                                    EmployeeName ,
                                                                                    TimeStamp ,
                                                                                    flgActive ,
                                                                                    DocType ,
                                                                                    DocNum ,
                                                                                    LineStatusID ,
                                                                                    LineStatusLOVType ,
                                                                                    StageName ,
                                                                                    ApprovalTempName ,
                                                                                    Series
                                                                                )
                                                                        VALUES  ( '" + EmpID + @"' , -- EmpID - nvarchar(10)
                                                                                  '" + EmpName + @"' , -- EmployeeName - nvarchar(150)
                                                                                  GetDate()   , -- TimeStamp - datetime
                                                                                    1 , -- flgActive - bit
                                                                                    " + DocType + @" , -- DocType - tinyint
                                                                                   " + DocNum + @" , -- DocNum - int
                                                                                  N'LV0005' , -- LineStatusID - nvarchar(10)
                                                                                  N'ApprovalStatus' , -- LineStatusLOVType - nvarchar(20)
                                                                                  N'" + StageName + @"' , -- StageName - nvarchar(20)
                                                                                  N'" + ApprovalTempName + @"' , -- ApprovalTempName - nvarchar(20)
                                                                                   " + Series + @"  -- Series - int
                                                                                )";
            this.ExecuteCommand(InsertInCfgApprovalDecisionRegister);
        }
        private void NESK_InsertLinesInApprovalDecisionRegister(int docNum, int docType, int EmpID, int ApproverID, string ApproverName, int DocHirerchyID, string LineStatusID,
                                                        string LineStatusLOVType, int LevelID, string LevelDesc, string EmailID)
        {
            String InsertInNESK_CfgApprovalDecisionRegister = @"INSERT INTO dbo.Nesk_CfgApprovalDecisionRegister
                                                                                ([DocNum]
                                                                               ,[DocType]
                                                                               ,[EmpID]
                                                                               ,[ApproverID]
                                                                               ,[ApproverEmailID]
                                                                               ,[ApproverName]
                                                                               ,[DocHirerchyID]
                                                                               ,[LineStatusID]
                                                                               ,[LineStatusLOVType]
                                                                               ,[PendingAtLevelID]
                                                                               ,[LevelID]
                                                                               ,[LevelDesc]                                                                          
                                                                               ,[TimeStamp]
                                                                               ,[flgActive])
                                                                        VALUES  ( '" + docNum + @"' ,
                                                                                  '" + docType + @"' ,
                                                                                  '" + EmpID + @"' ,
                                                                                  '" + ApproverID + @"' ,
                                                                                  '" + EmailID + @"' ,
                                                                                  '" + ApproverName + @"' ,
                                                                                  '" + DocHirerchyID + @"' ,
                                                                                  '" + LineStatusID + @"' ,
                                                                                  '" + LineStatusLOVType + @"' ,
                                                                                  1 ,
                                                                                  '" + LevelID + @"' ,
                                                                                  '" + LevelDesc + @"' ,
                                                                                  GetDate()   ,
                                                                                   1 )";
            this.ExecuteCommand(InsertInNESK_CfgApprovalDecisionRegister);
        }

        private void UpdateApprovalDocRegister(Int32 LineID, String StageDecision, UInt16 Status)
        {
            String UpdateCurrentStageLine = @"UPDATE dbo.CfgDocumentStageRegister
                                                        SET StageDecision = '" + StageDecision + @"', flgCurrentStage = " + Status + @"
                                                        WHERE ID = " + LineID;
            this.ExecuteCommand(UpdateCurrentStageLine);
        }

        #endregion

        #region "Job Requisition"

        /*Implementing JobRequisition Business Logic
         * DocType = 15
         * Table Name = TrnsJobRequisition, TrnsJobRequisitionDetail, TrnsJobRequisitionQualification, TrnsJobRequisitionSkills
         * 
         */

        partial void InsertTrnsJobRequisition(TrnsJobRequisition instance)
        {
            try
            {
                Byte Doctype = 15;
                IEnumerable<CfgApprovalTemplateStages> UserTemplateStage = null;
                IEnumerable<CfgApprovalStageDetail> UsersInStage = null;
                ViewApprovalTemplate Record = null;
                //Check Resignation Doc Emp has A temp

                //Record = (from a in this.ViewApprovalTemplate where a.FlgJobRequisition == true && a.UserID == instance.UserID select a).FirstOrDefault();
                Record = (from a in this.ViewApprovalTemplate where a.FlgJobRequisition == true select a).FirstOrDefault();
                if (Record != null)
                {
                    Console.WriteLine("Tempelate Detected");
                    UserTemplateStage = from a in this.CfgApprovalTemplateStages where a.ATID == Record.ID orderby a.Priorty ascending select a;
                    //Mark All Lines in current stage
                    UsersInStage = from a in this.CfgApprovalStageDetail where a.ASID == Record.StageID select a;
                    foreach (var Stage in UsersInStage)
                    {
                        InsertLinesInApprovalDecisionRegister(Stage.AuthorizerID, Stage.AuthorizerName, Convert.ToInt32(instance.DocNum),
                                                              Record.StageName, Record.Name,
                                                              Convert.ToInt32(instance.Series), Doctype);

                    }
                    //Mark all available stages in the given temp
                    foreach (var Lines in UserTemplateStage)
                    {
                        InsertLinesStageRegister(Convert.ToInt16(Lines.Priorty), Convert.ToInt32(instance.DocNum), Convert.ToInt32(instance.Series),
                                                 Doctype, Convert.ToInt32(Lines.StageID), Convert.ToInt32(Lines.CfgApprovalTemplate.ID));
                    }
                    //Prepare the document save in draft and pending status
                    instance.DocType = Doctype;
                    instance.DocStatus = "LV0001";
                    instance.DocStatusLOV = "DocStatus";
                    instance.DocAprStatus = "LV0005";
                    instance.DocAprStatusLOV = "ApprovalStatus";
                    this.ExecuteDynamicInsert(instance);


                }
                else
                {
                    //Prepare the document save in open and approved status
                    //when no template found
                    instance.DocType = Doctype;
                    instance.DocStatus = "LV0002";
                    instance.DocStatusLOV = "DocStatus";
                    instance.DocAprStatus = "LV0006";
                    instance.DocAprStatusLOV = "ApprovalStatus";
                    this.ExecuteDynamicInsert(instance);
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        public void CheckStageStatusJobRequisition(CfgApprovalDecisionRegister pints)
        {
            /*Variable Section */
            Byte DocumentType = 15;
            Int16 ApprovalCounts, RejectionCounts;
            Int16 TempApprovals, TempRejections;
            Boolean StageApproved = false, StageRejected = false;
            //Retrive stage lists
            try
            {
                //Retrive Current Stage
                var CStage = (from a in this.CfgApprovalStage
                              where a.StageName.Contains(pints.StageName)
                              select a).FirstOrDefault();
                TempApprovals = Convert.ToInt16(CStage.ApprovalsNo);
                TempRejections = Convert.ToInt16(CStage.RejectionsNo);

                ApprovalCounts = Convert.ToInt16((from a in this.CfgApprovalDecisionRegister
                                                  where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.StageName.Contains(pints.StageName) && a.LineStatusID.Contains("LV0006")
                                                  select a).Count());

                RejectionCounts = Convert.ToInt16((from a in this.CfgApprovalDecisionRegister
                                                   where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.StageName.Contains(pints.StageName) && a.LineStatusID.Contains("LV0007")
                                                   select a).Count());

                if (ApprovalCounts >= TempApprovals) StageApproved = true;
                if (RejectionCounts >= TempRejections) StageRejected = true;

                if (StageApproved)
                {
                    //Kill Lines of Current Stage in CfgApprovalDecisionReport.
                    var AllLinesInDoc = from a in this.CfgApprovalDecisionRegister
                                        where a.DocNum == pints.DocNum && a.Series == pints.Series && a.DocType == DocumentType && a.StageName.Contains(pints.StageName)
                                        select a;
                    foreach (var OneLine in AllLinesInDoc)
                    {
                        String KillExistingLines = @"UPDATE dbo.CfgApprovalDecisionRegister
                                                        SET flgActive = 0
                                                        WHERE ID = " + OneLine.ID.ToString();
                        this.ExecuteCommand(KillExistingLines);
                    }


                    //Add Update Current Stage as InActive and Approved Status in CfgDocumentStageRegister
                    var StageLines = (from a in this.CfgDocumentStageRegister
                                      where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.FlgCurrentStage == true
                                      select a).FirstOrDefault();

                    UpdateApprovalDocRegister(StageLines.ID, "A", 0);
                    var NextStage = from a in this.CfgDocumentStageRegister
                                    where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.StageDecision.Contains("P")
                                    select a;

                    if (NextStage.Count() > 0)
                    {
                        // Make Next Stage An Active Current Stage
                        var NextID = NextStage.Where(i => i.ID > StageLines.ID)
                                              .OrderBy(i => i.ID)
                                              .FirstOrDefault();
                        if (NextID != null)
                        {
                            String UpdateNextStagetoCurrentStage = @"UPDATE dbo.CfgDocumentStageRegister
                                                                    SET flgCurrentStage = 1
                                                                    WHERE ID = " + NextID.ID;
                            this.ExecuteCommand(UpdateNextStagetoCurrentStage);

                            //Enter Lines in CfgApprovalDecisionRegister for Next Stage

                            var Stages = from a in this.CfgApprovalStageDetail
                                         where a.ASID == NextID.TempStages
                                         select a;
                            foreach (var Stage in Stages)
                            {
                                InsertLinesInApprovalDecisionRegister(Stage.AuthorizerID, Stage.AuthorizerName, Convert.ToInt32(pints.DocNum), NextID.CfgApprovalStage.StageName, NextID.CfgApprovalTemplate.Name, Convert.ToInt32(pints.Series), DocumentType);
                            }

                        }
                    }
                    else
                    {
                        //Set The Document Approval Status as Approved and Document Status As Open
                        var TrnsDoc = (from a in this.TrnsJobRequisition
                                       where a.DocNum == pints.DocNum && a.DocType == pints.DocType && a.Series == pints.Series
                                       select a).FirstOrDefault();
                        String MainDocumentUpdate = @"UPDATE dbo.TrnsJobRequisition
                                                        SET DocStatus = 'LV0002', DocStatusLOV = 'DocStatus' , DocAprStatus = 'LV0006', DocAprStatusLOV = 'ApprovalStatus'
                                                        WHERE ID = " + TrnsDoc.ID.ToString();
                        this.ExecuteCommand(MainDocumentUpdate);

                    }
                }
                else if (StageRejected)
                {
                    //Mark the lines in CfgApprovalDecisionRegister
                    var KillLinesInCfgADR = from a in this.CfgApprovalDecisionRegister
                                            where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.StageName.Contains(pints.StageName)
                                            select a;
                    foreach (var OneLine in KillLinesInCfgADR)
                    {
                        String OneLineQuery = @"UPDATE dbo.CfgApprovalDecisionRegister
                                                SET flgActive = 0
                                                WHERE ID = '" + OneLine.ID.ToString() + @"'";
                        this.ExecuteCommand(OneLineQuery);
                    }
                    //Mark the Lines in CfgDocumentStageRegister

                    var LinesDSR = from a in this.CfgDocumentStageRegister
                                   where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.StageDecision.Contains("P")
                                   select a;
                    foreach (var OneLine in LinesDSR)
                    {
                        String UpdateDSRLine = @"UPDATE dbo.CfgDocumentStageRegister
                                                 SET StageDecision = 'R', flgCurrentStage = 0
                                                 WHERE ID = '" + OneLine.ID.ToString() + @"'";
                        this.ExecuteCommand(UpdateDSRLine);
                    }

                    //Set The Generated Document As Rejected In Approval Status & Document Status to Close

                    //Get Document ID
                    var DocID = (from a in this.TrnsJobRequisition
                                 where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series
                                 select a).FirstOrDefault();
                    String JobRequisitionClosed = @"UPDATE dbo.TrnsJobRequisition
                                                    SET DocStatus = 'LV0003', DocStatusLOV = 'DocStatus', DocAprStatus = 'LV0007', DocAprStatusLOV = 'ApprovalStatus'
                                                    WHERE ID = '" + DocID.ID + @"'";
                    this.ExecuteCommand(JobRequisitionClosed);

                }


            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }

        }

        #endregion

        #region "Resignation"

        /* Resignation
         * Doctype ==12
         */

        partial void InsertTrnsResignation(TrnsResignation instance)
        {
            try
            {
                Int16 Doctype = 12;
                String EmployeeID = "";
                DateTime TerminationDt, ResignDt;
                IEnumerable<CfgApprovalTemplateStages> UserTemplateStage = null;
                IEnumerable<CfgApprovalStageDetail> UsersInStage = null;
                ViewApprovalTemplate Record = null;
                //Check Resignation Doc Emp has A temp

                Record = (from a in this.ViewApprovalTemplate where a.FlgResignation == true select a).FirstOrDefault();
                if (Record != null)
                {
                    Console.WriteLine("Tempelate Detected");
                    UserTemplateStage = from a in this.CfgApprovalTemplateStages where a.ATID == Record.ID orderby a.Priorty ascending select a;
                    //Mark All Lines in current stage
                    UsersInStage = from a in this.CfgApprovalStageDetail where a.ASID == Record.StageID select a;
                    foreach (var Stage in UsersInStage)
                    {
                        InsertLinesInApprovalDecisionRegister(Stage.AuthorizerID, Stage.AuthorizerName, Convert.ToInt32(instance.DocNum),
                                                              Record.StageName, Record.Name,
                                                              Convert.ToInt32(instance.Series), Doctype);

                    }
                    //Mark all available stages in the given temp
                    foreach (var Lines in UserTemplateStage)
                    {
                        InsertLinesStageRegister(Convert.ToInt16(Lines.Priorty), Convert.ToInt32(instance.DocNum), Convert.ToInt32(instance.Series),
                                                 Doctype, Convert.ToInt32(Lines.StageID), Convert.ToInt32(Lines.CfgApprovalTemplate.ID));
                    }
                    //Prepare the document save in draft and pending status
                    instance.DocType = Doctype;
                    instance.DocStatus = "LV0001";
                    instance.DocStatusLOV = "DocStatus";
                    instance.DocAprStatus = "LV0005";
                    instance.DocAprStatusLOV = "ApprovalStatus";
                    this.ExecuteDynamicInsert(instance);


                }
                else
                {
                    //Prepare the document save in open and approved status
                    //when no template found
                    instance.DocType = Doctype;
                    instance.DocStatus = "LV0002";
                    instance.DocStatusLOV = "DocStatus";
                    instance.DocAprStatus = "LV0006";
                    instance.DocAprStatusLOV = "ApprovalStatus";
                    EmployeeID = instance.MstEmployee.EmpID;
                    TerminationDt = Convert.ToDateTime(instance.TerminationDate);
                    ResignDt = Convert.ToDateTime(instance.ResignDate);
                    this.ExecuteDynamicInsert(instance);
                    InActiveEmployee(EmployeeID, TerminationDt, ResignDt);
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }

        }

        private void CheckStageStatusResignation(CfgApprovalDecisionRegister pints)
        {
            /*Variable Section */
            Byte DocumentType = 12;
            Int16 ApprovalCounts, RejectionCounts;
            Int16 TempApprovals, TempRejections;
            Boolean StageApproved = false, StageRejected = false;
            String EmployeeID;
            DateTime TerminationDt, ResignDt;
            //Retrive stage lists
            try
            {
                //Retrive Current Stage
                var CStage = (from a in this.CfgApprovalStage
                              where a.StageName.Contains(pints.StageName)
                              select a).FirstOrDefault();
                TempApprovals = Convert.ToInt16(CStage.ApprovalsNo);
                TempRejections = Convert.ToInt16(CStage.RejectionsNo);

                ApprovalCounts = Convert.ToInt16((from a in this.CfgApprovalDecisionRegister
                                                  where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.StageName.Contains(pints.StageName) && a.LineStatusID.Contains("LV0006")
                                                  select a).Count());

                RejectionCounts = Convert.ToInt16((from a in this.CfgApprovalDecisionRegister
                                                   where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.StageName.Contains(pints.StageName) && a.LineStatusID.Contains("LV0007")
                                                   select a).Count());

                if (ApprovalCounts >= TempApprovals) StageApproved = true;
                if (RejectionCounts >= TempRejections) StageRejected = true;

                if (StageApproved)
                {
                    //Kill Lines of Current Stage in CfgApprovalDecisionReport.
                    var AllLinesInDoc = from a in this.CfgApprovalDecisionRegister
                                        where a.DocNum == pints.DocNum && a.Series == pints.Series && a.DocType == DocumentType && a.StageName.Contains(pints.StageName)
                                        select a;
                    foreach (var OneLine in AllLinesInDoc)
                    {
                        String KillExistingLines = @"UPDATE dbo.CfgApprovalDecisionRegister
                                                        SET flgActive = 0
                                                        WHERE ID = " + OneLine.ID.ToString();
                        this.ExecuteCommand(KillExistingLines);
                    }


                    //Add Update Current Stage as InActive and Approved Status in CfgDocumentStageRegister
                    var StageLines = (from a in this.CfgDocumentStageRegister
                                      where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.FlgCurrentStage == true
                                      select a).FirstOrDefault();

                    UpdateApprovalDocRegister(StageLines.ID, "A", 0);
                    var NextStage = from a in this.CfgDocumentStageRegister
                                    where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.StageDecision.Contains("P")
                                    select a;

                    if (NextStage.Count() > 0)
                    {
                        // Make Next Stage An Active Current Stage
                        var NextID = NextStage.Where(i => i.ID > StageLines.ID)
                                              .OrderBy(i => i.ID)
                                              .FirstOrDefault();
                        if (NextID != null)
                        {
                            String UpdateNextStagetoCurrentStage = @"UPDATE dbo.CfgDocumentStageRegister
                                                                    SET flgCurrentStage = 1
                                                                    WHERE ID = " + NextID.ID;
                            this.ExecuteCommand(UpdateNextStagetoCurrentStage);

                            //Enter Lines in CfgApprovalDecisionRegister for Next Stage

                            var Stages = from a in this.CfgApprovalStageDetail
                                         where a.ASID == NextID.TempStages
                                         select a;
                            foreach (var Stage in Stages)
                            {
                                InsertLinesInApprovalDecisionRegister(Stage.AuthorizerID, Stage.AuthorizerName, Convert.ToInt32(pints.DocNum), NextID.CfgApprovalStage.StageName, NextID.CfgApprovalTemplate.Name, Convert.ToInt32(pints.Series), DocumentType);
                            }

                        }
                    }
                    else
                    {
                        //Set The Document Approval Status as Approved and Document Status As Open
                        var TrnsDoc = (from a in this.TrnsResignation
                                       where a.DocNum == pints.DocNum && a.DocType == pints.DocType && a.Series == pints.Series
                                       select a).FirstOrDefault();
                        EmployeeID = Convert.ToString(TrnsDoc.EmpID);
                        TerminationDt = Convert.ToDateTime(TrnsDoc.TerminationDate);
                        ResignDt = Convert.ToDateTime(TrnsDoc.ResignDate);
                        String MainDocumentUpdate = @"UPDATE dbo.TrnsResignation
                                                        SET DocStatus = 'LV0002', DocStatusLOV = 'DocStatus' , DocAprStatus = 'LV0006', DocAprStatusLOV = 'ApprovalStatus'
                                                        WHERE ID = " + TrnsDoc.Id.ToString();
                        this.ExecuteCommand(MainDocumentUpdate);
                        InActiveEmployee(EmployeeID, TerminationDt, ResignDt);

                    }
                }
                else if (StageRejected)
                {
                    //Mark the lines in CfgApprovalDecisionRegister
                    var KillLinesInCfgADR = from a in this.CfgApprovalDecisionRegister
                                            where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.StageName.Contains(pints.StageName)
                                            select a;
                    foreach (var OneLine in KillLinesInCfgADR)
                    {
                        String OneLineQuery = @"UPDATE dbo.CfgApprovalDecisionRegister
                                                SET flgActive = 0
                                                WHERE ID = '" + OneLine.ID.ToString() + @"'";
                        this.ExecuteCommand(OneLineQuery);
                    }
                    //Mark the Lines in CfgDocumentStageRegister

                    var LinesDSR = from a in this.CfgDocumentStageRegister
                                   where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.StageDecision.Contains("P")
                                   select a;
                    foreach (var OneLine in LinesDSR)
                    {
                        String UpdateDSRLine = @"UPDATE dbo.CfgDocumentStageRegister
                                                 SET StageDecision = 'R', flgCurrentStage = 0
                                                 WHERE ID = '" + OneLine.ID.ToString() + @"'";
                        this.ExecuteCommand(UpdateDSRLine);
                    }

                    //Set The Generated Document As Rejected In Approval Status & Document Status to Close

                    //Get Document ID
                    var DocID = (from a in this.TrnsResignation
                                 where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series
                                 select a).FirstOrDefault();
                    String JobRequisitionClosed = @"UPDATE dbo.TrnsResignation
                                                    SET DocStatus = 'LV0003', DocStatusLOV = 'DocStatus', DocAprStatus = 'LV0007', DocAprStatusLOV = 'ApprovalStatus'
                                                    WHERE ID = '" + DocID.Id + @"'";
                    this.ExecuteCommand(JobRequisitionClosed);

                }


            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void InActiveEmployee(String EmpID, DateTime pTerminationDt, DateTime pResignationDt)
        {
            try
            {
                String InActiveEmployee = @"UPDATE dbo.MstEmployee
                                            SET PaymentMode = 'HOLD',
	                                            TerminationDate = '" + pTerminationDt + @"',
	                                            ResignDate = '" + pResignationDt + @"'
                                            WHERE EmpID = '" + EmpID + @"'";
                this.ExecuteCommand(InActiveEmployee);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }

        }

        #endregion

        #region "Loan Request"

        /* Loan & Advance Document
         * DocType == 11
         */

        partial void InsertTrnsLoan(TrnsLoan instance)
        {
            try
            {
                Byte Doctype = 11;
                string strDocStatus = "LV0003", strApprovalStatus = "LV0006";
                IEnumerable<CfgApprovalTemplateStages> UserTemplateStage = null;
                IEnumerable<CfgApprovalStageDetail> UsersInStage = null;
                ViewApprovalTemplate Record = null;
                //Check Resignation Doc Emp has A temp

                if (instance.DocAprStatus == strApprovalStatus && instance.DocStatus == strDocStatus)
                {
                    instance.DocType = Doctype;
                    this.ExecuteDynamicInsert(instance);
                }
                else
                {

                    Record = (from a in this.ViewApprovalTemplate where a.FlgLoan == true select a).FirstOrDefault();
                    if (Record != null)
                    {
                        Console.WriteLine("Tempelate Detected");
                        UserTemplateStage = from a in this.CfgApprovalTemplateStages where a.ATID == Record.ID orderby a.Priorty ascending select a;
                        //Mark All Lines in current stage
                        UsersInStage = from a in this.CfgApprovalStageDetail where a.ASID == Record.StageID select a;
                        foreach (var Stage in UsersInStage)
                        {
                            InsertLinesInApprovalDecisionRegister(Stage.AuthorizerID, Stage.AuthorizerName, Convert.ToInt32(instance.DocNum),
                                                                  Record.StageName, Record.Name,
                                                                  Convert.ToInt32(instance.Series), Doctype);

                        }
                        //Mark all available stages in the given temp
                        foreach (var Lines in UserTemplateStage)
                        {
                            InsertLinesStageRegister(Convert.ToInt16(Lines.Priorty), Convert.ToInt32(instance.DocNum), Convert.ToInt32(instance.Series),
                                                     Doctype, Convert.ToInt32(Lines.StageID), Convert.ToInt32(Lines.CfgApprovalTemplate.ID));
                        }
                        //Prepare the document save in draft and pending status
                        instance.DocType = Doctype;
                        instance.DocStatus = "LV0001";
                        instance.DocStatusLOV = "DocStatus";
                        instance.DocAprStatus = "LV0005";
                        instance.DocAprStatusLOV = "ApprovalStatus";
                        this.ExecuteDynamicInsert(instance);


                    }
                    else
                    {
                        //Prepare the document save in open and approved status
                        //when no template found
                        instance.DocType = Doctype;
                        instance.TrnsLoanDetail.ElementAt(0).ApprovedAmount = instance.TrnsLoanDetail.ElementAt(0).RequestedAmount;
                        instance.TrnsLoanDetail.ElementAt(0).ApprovedInstallment = instance.TrnsLoanDetail.ElementAt(0).Installments;
                        instance.TrnsLoanDetail.ElementAt(0).RecoveredAmount = 0.0M;
                        instance.TrnsLoanDetail.ElementAt(0).FlgActive = true;
                        instance.TrnsLoanDetail.ElementAt(0).FlgStopRecovery = false;

                        instance.DocStatus = "LV0002";
                        instance.DocStatusLOV = "DocStatus";
                        instance.DocAprStatus = "LV0006";
                        instance.DocAprStatusLOV = "ApprovalStatus";
                        this.ExecuteDynamicInsert(instance);
                    }
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void CheckStageStatusLoan(CfgApprovalDecisionRegister pints)
        {
            /*Variable Section */
            Byte DocumentType = 11;
            Int16 ApprovalCounts, RejectionCounts;
            Int16 TempApprovals, TempRejections;
            Boolean StageApproved = false, StageRejected = false;
            //Retrive stage lists
            try
            {
                //Retrive Current Stage
                var CStage = (from a in this.CfgApprovalStage
                              where a.StageName.Contains(pints.StageName)
                              select a).FirstOrDefault();
                TempApprovals = Convert.ToInt16(CStage.ApprovalsNo);
                TempRejections = Convert.ToInt16(CStage.RejectionsNo);

                ApprovalCounts = Convert.ToInt16((from a in this.CfgApprovalDecisionRegister
                                                  where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.StageName.Contains(pints.StageName) && a.LineStatusID.Contains("LV0006")
                                                  select a).Count());

                RejectionCounts = Convert.ToInt16((from a in this.CfgApprovalDecisionRegister
                                                   where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.StageName.Contains(pints.StageName) && a.LineStatusID.Contains("LV0007")
                                                   select a).Count());

                if (ApprovalCounts >= TempApprovals) StageApproved = true;
                if (RejectionCounts >= TempRejections) StageRejected = true;

                if (StageApproved)
                {
                    //Kill Lines of Current Stage in CfgApprovalDecisionReport.
                    var AllLinesInDoc = from a in this.CfgApprovalDecisionRegister
                                        where a.DocNum == pints.DocNum && a.Series == pints.Series && a.DocType == DocumentType && a.StageName.Contains(pints.StageName)
                                        select a;
                    foreach (var OneLine in AllLinesInDoc)
                    {
                        String KillExistingLines = @"UPDATE dbo.CfgApprovalDecisionRegister
                                                        SET flgActive = 0
                                                        WHERE ID = " + OneLine.ID.ToString();
                        this.ExecuteCommand(KillExistingLines);
                    }


                    //Add Update Current Stage as InActive and Approved Status in CfgDocumentStageRegister
                    var StageLines = (from a in this.CfgDocumentStageRegister
                                      where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.FlgCurrentStage == true
                                      select a).FirstOrDefault();

                    UpdateApprovalDocRegister(StageLines.ID, "A", 0);
                    var NextStage = from a in this.CfgDocumentStageRegister
                                    where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.StageDecision.Contains("P")
                                    select a;

                    if (NextStage.Count() > 0)
                    {
                        // Make Next Stage An Active Current Stage
                        var NextID = NextStage.Where(i => i.ID > StageLines.ID)
                                              .OrderBy(i => i.ID)
                                              .FirstOrDefault();
                        if (NextID != null)
                        {
                            String UpdateNextStagetoCurrentStage = @"UPDATE dbo.CfgDocumentStageRegister
                                                                    SET flgCurrentStage = 1
                                                                    WHERE ID = " + NextID.ID;
                            this.ExecuteCommand(UpdateNextStagetoCurrentStage);

                            //Enter Lines in CfgApprovalDecisionRegister for Next Stage

                            var Stages = from a in this.CfgApprovalStageDetail
                                         where a.ASID == NextID.TempStages
                                         select a;
                            foreach (var Stage in Stages)
                            {
                                InsertLinesInApprovalDecisionRegister(Stage.AuthorizerID, Stage.AuthorizerName, Convert.ToInt32(pints.DocNum), NextID.CfgApprovalStage.StageName, NextID.CfgApprovalTemplate.Name, Convert.ToInt32(pints.Series), DocumentType);
                            }

                        }
                    }
                    else
                    {
                        //Set The Document Approval Status as Approved and Document Status As Open
                        var TrnsDoc = (from a in this.TrnsLoan
                                       where a.DocNum == pints.DocNum && a.DocType == pints.DocType && a.Series == pints.Series
                                       select a).FirstOrDefault();
                        String MainDocumentUpdate = @"UPDATE dbo.TrnsLoan
                                                        SET DocStatus = 'LV0002', DocStatusLOV = 'DocStatus' , DocAprStatus = 'LV0006', DocAprStatusLOV = 'ApprovalStatus'
                                                        WHERE ID = " + TrnsDoc.ID.ToString();
                        this.ExecuteCommand(MainDocumentUpdate);

                    }
                }
                else if (StageRejected)
                {
                    //Mark the lines in CfgApprovalDecisionRegister
                    var KillLinesInCfgADR = from a in this.CfgApprovalDecisionRegister
                                            where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.StageName.Contains(pints.StageName)
                                            select a;
                    foreach (var OneLine in KillLinesInCfgADR)
                    {
                        String OneLineQuery = @"UPDATE dbo.CfgApprovalDecisionRegister
                                                SET flgActive = 0
                                                WHERE ID = '" + OneLine.ID.ToString() + @"'";
                        this.ExecuteCommand(OneLineQuery);
                    }
                    //Mark the Lines in CfgDocumentStageRegister

                    var LinesDSR = from a in this.CfgDocumentStageRegister
                                   where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.StageDecision.Contains("P")
                                   select a;
                    foreach (var OneLine in LinesDSR)
                    {
                        String UpdateDSRLine = @"UPDATE dbo.CfgDocumentStageRegister
                                                 SET StageDecision = 'R', flgCurrentStage = 0
                                                 WHERE ID = '" + OneLine.ID.ToString() + @"'";
                        this.ExecuteCommand(UpdateDSRLine);
                    }

                    //Set The Generated Document As Rejected In Approval Status & Document Status to Close

                    //Get Document ID
                    var DocID = (from a in this.TrnsLoan
                                 where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series
                                 select a).FirstOrDefault();
                    String JobRequisitionClosed = @"UPDATE dbo.TrnsLoan
                                                    SET DocStatus = 'LV0003', DocStatusLOV = 'DocStatus', DocAprStatus = 'LV0007', DocAprStatusLOV = 'ApprovalStatus'
                                                    WHERE ID = '" + DocID.ID + @"'";
                    this.ExecuteCommand(JobRequisitionClosed);

                }


            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        #endregion

        #region " Standared Element in Payroll"

        /* Standard Element
         * DocType == 20
         */
        //partial void MstElements(MstElements instance)
        //{
        //    if (instance.FlgEffectOnGross == true)
        //    {

        //        IEnumerable <CfgPayrollDefination> elementPrs = from p in CfgPayrollDefination where p.ID 
        //    }
        //}

        #endregion

        #region " Employee Master"

        /* Employee Master Standard Element
       
         */


        #endregion

        #region " Advance Transaction"

        /* Advance Document
         * DocType == 20
         */

        partial void InsertTrnsAdvance(TrnsAdvance instance)
        {
            try
            {
                Byte Doctype = 20;
                IEnumerable<CfgApprovalTemplateStages> UserTemplateStage = null;
                IEnumerable<CfgApprovalStageDetail> UsersInStage = null;
                ViewApprovalTemplate Record = null;
                //Check Resignation Doc Emp has A temp

                Record = (from a in this.ViewApprovalTemplate where a.FlgAdvance == true select a).FirstOrDefault();
                if (Record != null)
                {
                    Console.WriteLine("Tempelate Detected");
                    UserTemplateStage = from a in this.CfgApprovalTemplateStages where a.ATID == Record.ID orderby a.Priorty ascending select a;
                    //Mark All Lines in current stage
                    UsersInStage = from a in this.CfgApprovalStageDetail where a.ASID == Record.StageID select a;
                    foreach (var Stage in UsersInStage)
                    {
                        InsertLinesInApprovalDecisionRegister(Stage.AuthorizerID, Stage.AuthorizerName, Convert.ToInt32(instance.DocNum),
                                                              Record.StageName, Record.Name,
                                                              Convert.ToInt32(instance.Series), Doctype);

                    }
                    //Mark all available stages in the given temp
                    foreach (var Lines in UserTemplateStage)
                    {
                        InsertLinesStageRegister(Convert.ToInt16(Lines.Priorty), Convert.ToInt32(instance.DocNum), Convert.ToInt32(instance.Series),
                                                 Doctype, Convert.ToInt32(Lines.StageID), Convert.ToInt32(Lines.CfgApprovalTemplate.ID));
                    }
                    //Prepare the document save in draft and pending status
                    instance.DocType = Doctype;
                    instance.DocStatus = "LV0001";
                    instance.DocStatusLOV = "DocStatus";
                    instance.DocAprStatus = "LV0005";
                    instance.DocAprStatusLOV = "ApprovalStatus";
                    this.ExecuteDynamicInsert(instance);


                }
                else
                {
                    //Prepare the document save in open and approved status
                    //when no template found
                    instance.DocType = Doctype;
                    instance.ApprovedAmount = instance.RequestedAmount;
                    instance.RemainingAmount = instance.RequestedAmount;
                    instance.DocStatus = "LV0002";
                    instance.DocStatusLOV = "DocStatus";
                    instance.DocAprStatus = "LV0006";
                    instance.DocAprStatusLOV = "ApprovalStatus";
                    this.ExecuteDynamicInsert(instance);
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void CheckStageStatusAdvance(CfgApprovalDecisionRegister pints)
        {
            /*Variable Section */
            Byte DocumentType = 20;
            Int16 ApprovalCounts, RejectionCounts;
            Int16 TempApprovals, TempRejections;
            Boolean StageApproved = false, StageRejected = false;
            //Retrive stage lists
            try
            {
                //Retrive Current Stage
                var CStage = (from a in this.CfgApprovalStage
                              where a.StageName.Contains(pints.StageName)
                              select a).FirstOrDefault();
                TempApprovals = Convert.ToInt16(CStage.ApprovalsNo);
                TempRejections = Convert.ToInt16(CStage.RejectionsNo);

                ApprovalCounts = Convert.ToInt16((from a in this.CfgApprovalDecisionRegister
                                                  where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.StageName.Contains(pints.StageName) && a.LineStatusID.Contains("LV0006")
                                                  select a).Count());

                RejectionCounts = Convert.ToInt16((from a in this.CfgApprovalDecisionRegister
                                                   where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.StageName.Contains(pints.StageName) && a.LineStatusID.Contains("LV0007")
                                                   select a).Count());

                if (ApprovalCounts >= TempApprovals) StageApproved = true;
                if (RejectionCounts >= TempRejections) StageRejected = true;

                if (StageApproved)
                {
                    //Kill Lines of Current Stage in CfgApprovalDecisionReport.
                    var AllLinesInDoc = from a in this.CfgApprovalDecisionRegister
                                        where a.DocNum == pints.DocNum && a.Series == pints.Series && a.DocType == DocumentType && a.StageName.Contains(pints.StageName)
                                        select a;
                    foreach (var OneLine in AllLinesInDoc)
                    {
                        String KillExistingLines = @"UPDATE dbo.CfgApprovalDecisionRegister
                                                        SET flgActive = 0
                                                        WHERE ID = " + OneLine.ID.ToString();
                        this.ExecuteCommand(KillExistingLines);
                    }


                    //Add Update Current Stage as InActive and Approved Status in CfgDocumentStageRegister
                    var StageLines = (from a in this.CfgDocumentStageRegister
                                      where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.FlgCurrentStage == true
                                      select a).FirstOrDefault();

                    UpdateApprovalDocRegister(StageLines.ID, "A", 0);
                    var NextStage = from a in this.CfgDocumentStageRegister
                                    where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.StageDecision.Contains("P")
                                    select a;

                    if (NextStage.Count() > 0)
                    {
                        // Make Next Stage An Active Current Stage
                        var NextID = NextStage.Where(i => i.ID > StageLines.ID)
                                              .OrderBy(i => i.ID)
                                              .FirstOrDefault();
                        if (NextID != null)
                        {
                            String UpdateNextStagetoCurrentStage = @"UPDATE dbo.CfgDocumentStageRegister
                                                                    SET flgCurrentStage = 1
                                                                    WHERE ID = " + NextID.ID;
                            this.ExecuteCommand(UpdateNextStagetoCurrentStage);

                            //Enter Lines in CfgApprovalDecisionRegister for Next Stage

                            var Stages = from a in this.CfgApprovalStageDetail
                                         where a.ASID == NextID.TempStages
                                         select a;
                            foreach (var Stage in Stages)
                            {
                                InsertLinesInApprovalDecisionRegister(Stage.AuthorizerID, Stage.AuthorizerName, Convert.ToInt32(pints.DocNum), NextID.CfgApprovalStage.StageName, NextID.CfgApprovalTemplate.Name, Convert.ToInt32(pints.Series), DocumentType);
                            }

                        }
                    }
                    else
                    {
                        //Set The Document Approval Status as Approved and Document Status As Open
                        var TrnsDoc = (from a in this.TrnsAdvance
                                       where a.DocNum == pints.DocNum && a.DocType == pints.DocType && a.Series == pints.Series
                                       select a).FirstOrDefault();
                        String MainDocumentUpdate = @"UPDATE dbo.TrnsAdvance
                                                        SET DocStatus = 'LV0002', DocStatusLOV = 'DocStatus' , DocAprStatus = 'LV0006', DocAprStatusLOV = 'ApprovalStatus' , RemainingAmount = ApprovedAmount
                                                        WHERE ID = " + TrnsDoc.ID.ToString();
                        this.ExecuteCommand(MainDocumentUpdate);

                    }
                }
                else if (StageRejected)
                {
                    //Mark the lines in CfgApprovalDecisionRegister
                    var KillLinesInCfgADR = from a in this.CfgApprovalDecisionRegister
                                            where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.StageName.Contains(pints.StageName)
                                            select a;
                    foreach (var OneLine in KillLinesInCfgADR)
                    {
                        String OneLineQuery = @"UPDATE dbo.CfgApprovalDecisionRegister
                                                SET flgActive = 0
                                                WHERE ID = '" + OneLine.ID.ToString() + @"'";
                        this.ExecuteCommand(OneLineQuery);
                    }
                    //Mark the Lines in CfgDocumentStageRegister

                    var LinesDSR = from a in this.CfgDocumentStageRegister
                                   where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.StageDecision.Contains("P")
                                   select a;
                    foreach (var OneLine in LinesDSR)
                    {
                        String UpdateDSRLine = @"UPDATE dbo.CfgDocumentStageRegister
                                                 SET StageDecision = 'R', flgCurrentStage = 0
                                                 WHERE ID = '" + OneLine.ID.ToString() + @"'";
                        this.ExecuteCommand(UpdateDSRLine);
                    }

                    //Set The Generated Document As Rejected In Approval Status & Document Status to Close

                    //Get Document ID
                    var DocID = (from a in this.TrnsAdvance
                                 where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series
                                 select a).FirstOrDefault();
                    String JobRequisitionClosed = @"UPDATE dbo.TrnsAdvance
                                                    SET DocStatus = 'LV0003', DocStatusLOV = 'DocStatus', DocAprStatus = 'LV0007', DocAprStatusLOV = 'ApprovalStatus' , RemainingAmount = ApprovedAmount
                                                    WHERE ID = '" + DocID.ID + @"'";
                    this.ExecuteCommand(JobRequisitionClosed);

                }


            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }

        }

        #endregion

        #region "Leave Approval DI"
        /* Leave Requeset
         * DocType == 13
         */

        partial void InsertTrnsLeavesRequest(TrnsLeavesRequest instance)
        {
            try
            {
                Byte Doctype = 13;
                IEnumerable<CfgApprovalTemplateStages> UserTemplateStage = null;
                IEnumerable<CfgApprovalStageDetail> UsersInStage = null;
                ViewApprovalTemplate Record = null;

                //Check Resignation Doc Emp has A temp
                //Record = (from a in this.ViewApprovalTemplate where a.FlgEmpLeave == true && a.UserID == instance.UserId select a).FirstOrDefault();
                Record = (from a in this.ViewApprovalTemplate where a.FlgEmpLeave == true select a).FirstOrDefault();
                if (Record != null)
                {
                    Console.WriteLine("Tempelate Detected");
                    UserTemplateStage = from a in this.CfgApprovalTemplateStages where a.ATID == Record.ID orderby a.Priorty ascending select a;
                    //Mark All Lines in current stage
                    UsersInStage = from a in this.CfgApprovalStageDetail where a.ASID == Record.StageID select a;
                    foreach (var Stage in UsersInStage)
                    {
                        InsertLinesInApprovalDecisionRegister(Stage.AuthorizerID, Stage.AuthorizerName, Convert.ToInt32(instance.DocNum),
                                                              Record.StageName, Record.Name,
                                                              Convert.ToInt32(instance.Series), Doctype);

                    }
                    //Mark all available stages in the given temp
                    foreach (var Lines in UserTemplateStage)
                    {
                        InsertLinesStageRegister(Convert.ToInt16(Lines.Priorty), Convert.ToInt32(instance.DocNum), Convert.ToInt32(instance.Series),
                                                 Doctype, Convert.ToInt32(Lines.StageID), Convert.ToInt32(Lines.CfgApprovalTemplate.ID));
                    }
                    //Prepare the document save in draft and pending status
                    instance.DocType = Doctype;
                    instance.DocStatus = "LV0001";
                    instance.DocStatusLOV = "DocStatus";
                    instance.DocAprStatus = "LV0005";
                    instance.DocAprStatusLOV = "ApprovalStatus";


                }
                else
                {
                    //Prepare the document save in open and approved status
                    //when no template found
                    instance.DocType = Doctype;
                    instance.DocStatus = "LV0002";
                    instance.DocStatusLOV = "DocStatus";
                    instance.DocAprStatus = "LV0006";
                    instance.DocAprStatusLOV = "ApprovalStatus";

                }
                try
                {
                    mFm log = new mFm(Assembly.GetExecutingAssembly().Location, true, false);
                    Custom.DataServices ds = new Custom.DataServices(this, this.Connection.Database, "BusinessLoagic", log);
                    MstLeaveType lt = (from p in this.MstLeaveType where p.ID == instance.LeaveType select p).Single();
                    string leaveDed = lt.DeductionCode;
                    string tmpLeaveDeduction = lt.PendingApprDedCode;

                    instance.DeductId = leaveDed;
                    if ((instance.DeductAmnt != null ? instance.DeductAmnt : 0) <= 0)
                        instance.DeductAmnt = 0.00M;
                    instance.DeductedAmnt = 0.00M;
                    instance.DeductedUnit = 0;
                    instance.PendingDedAmnt = 0.00M;
                    instance.RembAmnt = 0.00M;
                    instance.PendingDedId = tmpLeaveDeduction;
                    if (instance.DeductAmnt == 0)
                    {
                        #region Leave Request
                        if (instance.DeductId != null && instance.DeductId != "")
                        {
                            var oCompany = (from a in this.MstCompany select a).FirstOrDefault();
                            string CompanyName = string.IsNullOrEmpty(oCompany.CompanyName) ? "" : oCompany.CompanyName.Trim();
                            DateTime dtGetMonthDays;
                            Int32 intGetMonthDays;
                            decimal decHalfMonthSalary = 0;
                            
                            //var PeriodId = dbHrPayroll.CfgPeriodDates.Where(pd => pd.StartDate <= x && x <= pd.EndDate && pd.PayrollId == instance.MstEmployee.PayrollID).FirstOrDefault();
                            CfgPeriodDates LeaveFromPeriod = (from pd in this.CfgPeriodDates where pd.StartDate <= instance.LeaveFrom.Value && instance.LeaveFrom.Value <= pd.EndDate && pd.PayrollId == instance.MstEmployee.PayrollID select pd).FirstOrDefault();
                            MstLeaveDeduction ld = (from p in this.MstLeaveDeduction where p.Code == leaveDed.Trim() select p).FirstOrDefault();
                            dtGetMonthDays = Convert.ToDateTime(LeaveFromPeriod.StartDate);
                            intGetMonthDays = DateTime.DaysInMonth(dtGetMonthDays.Year, dtGetMonthDays.Month);
                            if (ld != null && ld.TypeofDeduction == "POG")
                            {
                                decimal grossAmnt = ds.getEmpGross(instance.MstEmployee);
                                short days = (short)instance.MstEmployee.CfgPayrollDefination.WorkDays;
                                if (days < 1)
                                {
                                    days = instance.MstLeaveType.Months != null ? Convert.ToInt16(instance.MstLeaveType.Months) : Convert.ToInt16(0);
                                    //days = (short)instance.MstLeaveType.Months;
                                    if (days < 1)
                                    {
                                        if (LeaveFromPeriod != null)
                                        {
                                            if (LeaveFromPeriod.CfgPayrollDefination.PayrollType.Trim() == "HMNT")
                                            {
                                                days = Convert.ToInt16((Convert.ToDateTime(LeaveFromPeriod.EndDate) - Convert.ToDateTime(LeaveFromPeriod.StartDate)).Days + 1);
                                                //days = Convert.ToInt16(System.DateTime.DaysInMonth(LeaveFromPeriod.StartDate.Value.Date.Year, LeaveFromPeriod.StartDate.Value.Date.Month));
                                            }
                                            else
                                            {
                                                days = Convert.ToInt16(System.DateTime.DaysInMonth(LeaveFromPeriod.StartDate.Value.Date.Year, LeaveFromPeriod.StartDate.Value.Date.Month));
                                            }
                                        }
                                        else
                                        {
                                            days = Convert.ToInt16(System.DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month));
                                        }
                                    }
                                    //days = (Convert.ToDateTime(leavePeriod.EndDate) - Convert.ToDateTime(leavePeriod.StartDate)).Days;
                                    //days = workDays + 1;
                                }
                                if (days != null && days > 0)
                                {
                                    //instance.DeductAmnt = grossAmnt / days * instance.TotalCount;                                    
                                    instance.DeductAmnt = (grossAmnt * (ld.DeductionValue / 100)) / days * instance.TotalCount;

                                }
                            }
                            if (ld != null && ld.TypeofDeduction == "FIX")
                            {
                                decimal grossAmnt = ds.getEmpGross(instance.MstEmployee);
                                short days = (short)instance.MstEmployee.CfgPayrollDefination.WorkDays;
                                if (days < 1)
                                {
                                    days = (short)instance.MstLeaveType.Months;
                                    if (days < 1)
                                    {
                                        if (LeaveFromPeriod != null)
                                        {
                                            days = Convert.ToInt16(System.DateTime.DaysInMonth(LeaveFromPeriod.StartDate.Value.Date.Year, LeaveFromPeriod.StartDate.Value.Date.Month));
                                        }
                                        else
                                        {
                                            days = Convert.ToInt16(System.DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month));
                                        }
                                    }
                                    //days = (Convert.ToDateTime(leavePeriod.EndDate) - Convert.ToDateTime(leavePeriod.StartDate)).Days;
                                    //days = workDays + 1;
                                    //days = Convert.ToInt16(System.DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month));

                                }
                                if (days != null && days > 0)
                                {
                                    instance.DeductAmnt = ld.DeductionValue;
                                    // instance.DeductAmnt = grossAmnt / days * instance.TotalCount;
                                }
                            }
                            if (ld != null && ld.TypeofDeduction == "POB")
                            {
                                short days = (short)instance.MstEmployee.CfgPayrollDefination.WorkDays;
                                if (days < 1)
                                {
                                    days = (short)instance.MstLeaveType.Months;
                                    if (days < 1)
                                    {
                                        if (LeaveFromPeriod != null)
                                        {
                                            if (LeaveFromPeriod.CfgPayrollDefination.PayrollType.Trim() == "HMNT")
                                            {
                                                days = Convert.ToInt16((Convert.ToDateTime(LeaveFromPeriod.EndDate) - Convert.ToDateTime(LeaveFromPeriod.StartDate)).Days + 1);
                                                //days = Convert.ToInt16(System.DateTime.DaysInMonth(LeaveFromPeriod.StartDate.Value.Date.Year, LeaveFromPeriod.StartDate.Value.Date.Month));
                                            }
                                            else
                                            {
                                                days = Convert.ToInt16(System.DateTime.DaysInMonth(LeaveFromPeriod.StartDate.Value.Date.Year, LeaveFromPeriod.StartDate.Value.Date.Month));
                                            }
                                        }
                                        else
                                        {
                                            days = Convert.ToInt16(System.DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month));
                                        }
                                    }
                                    //days = (Convert.ToDateTime(leavePeriod.EndDate) - Convert.ToDateTime(leavePeriod.StartDate)).Days;
                                    //days = workDays + 1;
                                    //days = Convert.ToInt16(System.DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month));

                                }
                                if (days != null && days > 0)
                                {
                                    //instance.DeductAmnt = instance.MstEmployee.BasicSalary / days * instance.TotalCount;
                                    if (LeaveFromPeriod.CfgPayrollDefination.PayrollType.Trim() == "HMNT")
                                    {
                                        if (CompanyName.ToLower() == "pakola")
                                        {
                                            instance.DeductAmnt = ((instance.MstEmployee.BasicSalary / 2) * (ld.DeductionValue / 100)) / days * instance.TotalCount;
                                        }
                                        else
                                        {
                                            //instance.DeductAmnt = ((instance.MstEmployee.BasicSalary / intGetMonthDays) * (ld.DeductionValue / 100)) / days * instance.TotalCount;
                                            decHalfMonthSalary = Convert.ToDecimal((instance.MstEmployee.BasicSalary / intGetMonthDays) * days);
                                            instance.DeductAmnt = ((decHalfMonthSalary) * (ld.DeductionValue / 100)) / days * instance.TotalCount;
                                        }

                                    }
                                    else
                                    {
                                        instance.DeductAmnt = (instance.MstEmployee.BasicSalary * (ld.DeductionValue / 100)) / days * instance.TotalCount;
                                    }
                                }
                            }

                        }
                        #endregion

                        #region Pending region
                        if (instance.PendingDedId != null && instance.PendingDedId != "")
                        {
                            MstLeaveDeduction ld = this.MstLeaveDeduction.Where(p => p.Code == tmpLeaveDeduction.Trim()).FirstOrDefault();
                            CfgPeriodDates LeaveFromPeriod = (from pd in this.CfgPeriodDates where pd.StartDate <= instance.LeaveFrom.Value && instance.LeaveFrom.Value <= pd.EndDate && pd.PayrollId == instance.MstEmployee.PayrollID select pd).FirstOrDefault();
                            // MstLeaveDeduction ld = (from p in this.MstLeaveDeduction where p.Code == tmpLeaveDeduction.Trim() select p).FirstOrDefault();
                            if (ld != null && ld.TypeofDeduction == "POG")
                            {
                                decimal grossAmnt = ds.getEmpGross(instance.MstEmployee);
                                short days = (short)instance.MstEmployee.CfgPayrollDefination.WorkDays;
                                if (days < 1)
                                {
                                    days = (short)instance.MstLeaveType.Months;
                                    if (days < 1)
                                    {
                                        if (LeaveFromPeriod != null)
                                        {
                                            days = Convert.ToInt16(System.DateTime.DaysInMonth(LeaveFromPeriod.StartDate.Value.Date.Year, LeaveFromPeriod.StartDate.Value.Date.Month));
                                        }
                                        else
                                        {
                                            days = Convert.ToInt16(System.DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month));
                                        }
                                    }
                                    //days = (Convert.ToDateTime(leavePeriod.EndDate) - Convert.ToDateTime(leavePeriod.StartDate)).Days;
                                    //days = workDays + 1;
                                    //days = Convert.ToInt16(System.DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month));

                                }
                                if (days != null && days > 0)
                                {
                                    // instance.PendingDedAmnt = grossAmnt / days * instance.TotalCount;
                                    instance.DeductAmnt = (grossAmnt * (ld.DeductionValue / 100)) / days * instance.TotalCount;
                                }
                            }
                            if (ld != null && ld.TypeofDeduction == "POB")
                            {
                                short days = (short)instance.MstEmployee.CfgPayrollDefination.WorkDays;
                                if (days < 1)
                                {
                                    days = (short)instance.MstLeaveType.Months;
                                    if (days < 1)
                                    {
                                        if (LeaveFromPeriod != null)
                                        {
                                            days = Convert.ToInt16(System.DateTime.DaysInMonth(LeaveFromPeriod.StartDate.Value.Date.Year, LeaveFromPeriod.StartDate.Value.Date.Month));
                                        }
                                        else
                                        {
                                            days = Convert.ToInt16(System.DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month));
                                        }
                                    }
                                    //days = (Convert.ToDateTime(leavePeriod.EndDate) - Convert.ToDateTime(leavePeriod.StartDate)).Days;
                                    //days = workDays + 1;
                                    //days = Convert.ToInt16(System.DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month));

                                }
                                if (days != null && days > 0)
                                {
                                    //instance.PendingDedAmnt = instance.MstEmployee.BasicSalary / days * instance.TotalCount;
                                    instance.DeductAmnt = (instance.MstEmployee.BasicSalary * (ld.DeductionValue / 100)) / days * instance.TotalCount;
                                }
                            }
                            if (ld != null && ld.TypeofDeduction == "FIX")
                            {
                                decimal grossAmnt = ds.getEmpGross(instance.MstEmployee);
                                short days = (short)instance.MstEmployee.CfgPayrollDefination.WorkDays;
                                if (days < 1)
                                {
                                    days = (short)instance.MstLeaveType.Months;
                                    if (days < 1)
                                    {
                                        if (LeaveFromPeriod != null)
                                        {
                                            days = Convert.ToInt16(System.DateTime.DaysInMonth(LeaveFromPeriod.StartDate.Value.Date.Year, LeaveFromPeriod.StartDate.Value.Date.Month));
                                        }
                                        else
                                        {
                                            days = Convert.ToInt16(System.DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month));
                                        }
                                    }
                                    //days = (Convert.ToDateTime(leavePeriod.EndDate) - Convert.ToDateTime(leavePeriod.StartDate)).Days;
                                    //days = workDays + 1;
                                    //days = Convert.ToInt16(System.DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month));

                                }
                                if (days != null && days > 0)
                                {
                                    instance.PendingDedAmnt = ld.DeductionValue;
                                    // instance.DeductAmnt = grossAmnt / days * instance.TotalCount;
                                }
                            }

                        }
                        #endregion
                    }
                    this.ExecuteDynamicInsert(instance);
                }
                catch
                { }

            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        private void CheckStageStatusLeaveRequest(CfgApprovalDecisionRegister pints)
        {
            /*Variable Section */
            Byte DocumentType = 13;
            Int16 ApprovalCounts, RejectionCounts;
            Int16 TempApprovals, TempRejections;
            Boolean StageApproved = false, StageRejected = false;
            //Retrive stage lists
            try
            {
                //Retrive Current Stage
                var CStage = (from a in this.CfgApprovalStage
                              where a.StageName.Contains(pints.StageName)
                              select a).FirstOrDefault();
                TempApprovals = Convert.ToInt16(CStage.ApprovalsNo);
                TempRejections = Convert.ToInt16(CStage.RejectionsNo);

                ApprovalCounts = Convert.ToInt16((from a in this.CfgApprovalDecisionRegister
                                                  where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.StageName.Contains(pints.StageName) && a.LineStatusID.Contains("LV0006")
                                                  select a).Count());

                RejectionCounts = Convert.ToInt16((from a in this.CfgApprovalDecisionRegister
                                                   where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.StageName.Contains(pints.StageName) && a.LineStatusID.Contains("LV0007")
                                                   select a).Count());

                if (ApprovalCounts >= TempApprovals) StageApproved = true;
                if (RejectionCounts >= TempRejections) StageRejected = true;

                if (StageApproved)
                {
                    //Kill Lines of Current Stage in CfgApprovalDecisionReport.
                    var AllLinesInDoc = from a in this.CfgApprovalDecisionRegister
                                        where a.DocNum == pints.DocNum && a.Series == pints.Series && a.DocType == DocumentType && a.StageName.Contains(pints.StageName)
                                        select a;
                    foreach (var OneLine in AllLinesInDoc)
                    {
                        String KillExistingLines = @"UPDATE dbo.CfgApprovalDecisionRegister
                                                        SET flgActive = 0
                                                        WHERE ID = " + OneLine.ID.ToString();
                        this.ExecuteCommand(KillExistingLines);
                    }


                    //Add Update Current Stage as InActive and Approved Status in CfgDocumentStageRegister
                    var StageLines = (from a in this.CfgDocumentStageRegister
                                      where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.FlgCurrentStage == true
                                      select a).FirstOrDefault();

                    UpdateApprovalDocRegister(StageLines.ID, "A", 0);
                    var NextStage = from a in this.CfgDocumentStageRegister
                                    where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.StageDecision.Contains("P")
                                    select a;

                    if (NextStage.Count() > 0)
                    {
                        // Make Next Stage An Active Current Stage
                        var NextID = NextStage.Where(i => i.ID > StageLines.ID)
                                              .OrderBy(i => i.ID)
                                              .FirstOrDefault();
                        if (NextID != null)
                        {
                            String UpdateNextStagetoCurrentStage = @"UPDATE dbo.CfgDocumentStageRegister
                                                                    SET flgCurrentStage = 1
                                                                    WHERE ID = " + NextID.ID;
                            this.ExecuteCommand(UpdateNextStagetoCurrentStage);

                            //Enter Lines in CfgApprovalDecisionRegister for Next Stage

                            var Stages = from a in this.CfgApprovalStageDetail
                                         where a.ASID == NextID.TempStages
                                         select a;
                            foreach (var Stage in Stages)
                            {
                                InsertLinesInApprovalDecisionRegister(Stage.AuthorizerID, Stage.AuthorizerName, Convert.ToInt32(pints.DocNum), NextID.CfgApprovalStage.StageName, NextID.CfgApprovalTemplate.Name, Convert.ToInt32(pints.Series), DocumentType);
                            }

                        }
                    }
                    else
                    {
                        //Set The Document Approval Status as Approved and Document Status As Open
                        var TrnsDoc = (from a in this.TrnsLeavesRequest
                                       where a.DocNum == pints.DocNum && a.DocType == pints.DocType && a.Series == pints.Series
                                       select a).FirstOrDefault();
                        String MainDocumentUpdate = @"UPDATE dbo.TrnsLeavesRequest
                                                        SET DocStatus = 'LV0002', DocStatusLOV = 'DocStatus' , DocAprStatus = 'LV0006', DocAprStatusLOV = 'ApprovalStatus'
                                                        WHERE ID = " + TrnsDoc.ID.ToString();
                        this.ExecuteCommand(MainDocumentUpdate);

                    }
                }
                else if (StageRejected)
                {
                    //Mark the lines in CfgApprovalDecisionRegister
                    var KillLinesInCfgADR = from a in this.CfgApprovalDecisionRegister
                                            where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.StageName.Contains(pints.StageName)
                                            select a;
                    foreach (var OneLine in KillLinesInCfgADR)
                    {
                        String OneLineQuery = @"UPDATE dbo.CfgApprovalDecisionRegister
                                                SET flgActive = 0
                                                WHERE ID = '" + OneLine.ID.ToString() + @"'";
                        this.ExecuteCommand(OneLineQuery);
                    }
                    //Mark the Lines in CfgDocumentStageRegister

                    var LinesDSR = from a in this.CfgDocumentStageRegister
                                   where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series && a.StageDecision.Contains("P")
                                   select a;
                    foreach (var OneLine in LinesDSR)
                    {
                        String UpdateDSRLine = @"UPDATE dbo.CfgDocumentStageRegister
                                                 SET StageDecision = 'R', flgCurrentStage = 0
                                                 WHERE ID = '" + OneLine.ID.ToString() + @"'";
                        this.ExecuteCommand(UpdateDSRLine);
                    }

                    //Set The Generated Document As Rejected In Approval Status & Document Status to Close

                    //Get Document ID
                    var DocID = (from a in this.TrnsLeavesRequest
                                 where a.DocNum == pints.DocNum && a.DocType == DocumentType && a.Series == pints.Series
                                 select a).FirstOrDefault();
                    String JobRequisitionClosed = @"UPDATE dbo.TrnsLeavesRequest
                                                    SET DocStatus = 'LV0003', DocStatusLOV = 'DocStatus', DocAprStatus = 'LV0007', DocAprStatusLOV = 'ApprovalStatus'
                                                    WHERE ID = '" + DocID.ID + @"'";
                    this.ExecuteCommand(JobRequisitionClosed);

                }


            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        #endregion

        #region AdvancePaymentLogic

        partial void InsertTrnsLoanAndAdvancePayment(TrnsLoanAndAdvancePayment instance)
        {
            try
            {

            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        #endregion

        #region "Employee Overtime Approval DI"
        /* Employee OverTime
         * DocType == 24 */

        partial void InsertTrnsEmployeeOvertimeDetail(TrnsEmployeeOvertimeDetail instance)
        {
            try
            {
                Byte Doctype = 24;
                IEnumerable<Nesk_CfgDocHierarchyDetail> DocHirerchyDetail = null;
                int EmpDepartID = instance.TrnsEmployeeOvertime.MstEmployee.DepartmentID.Value;
                //Check Whether Doc Hirerchy/Approval Template Exist for Leave Approval Procedure
                Nesk_CfgDocHierarchy DocTemplateRecord = null;
                DocTemplateRecord = this.Nesk_CfgDocHierarchy.Where(d => d.DepartID == EmpDepartID && d.DocType == Doctype).FirstOrDefault();
                if (DocTemplateRecord != null)
                {

                    Console.WriteLine("Tempelate Detected");
                    DocHirerchyDetail = this.Nesk_CfgDocHierarchyDetail.Where(dt => dt.DocHirerchyID == DocTemplateRecord.ID).ToList();
                    //Mark All Lines in ApprovalDecisionRegister
                    foreach (var Level in DocHirerchyDetail)
                    {
                        NESK_InsertLinesInApprovalDecisionRegister(instance.DocNum.Value, Doctype, instance.TrnsEmployeeOvertime.EmployeeId.Value, Level.EmpID.Value,
                                                                    Level.MstEmployee.FirstName + " " + Level.MstEmployee.MiddleName + " " + Level.MstEmployee.LastName,
                                                                    Level.DocHirerchyID.Value,
                                                                    "LV0005", "ApprovalStatus",
                                                                    Level.HirerchyLevel.Value,
                                                                    Level.HirerchyLevelDesc, Level.MstEmployee.OfficeEmail);
                    }
                    //Prepare the document save in draft and pending status
                    instance.DocType = Doctype;
                    instance.DocStatus = "LV0001";
                    instance.DocStatusLOV = "DocStatus";
                    instance.DocAprStatus = "LV0005";
                    instance.DocAprStatusLOV = "ApprovalStatus";
                    var ApprDesLine = this.Nesk_CfgApprovalDecisionRegister.Where(l => l.DocNum == instance.DocNum && l.DocType == instance.DocType && l.LevelID == l.PendingAtLevelID).FirstOrDefault();
                    if (ApprDesLine != null)
                    {
                        if (!string.IsNullOrEmpty(ApprDesLine.ApproverEmailID))
                        {
                            //SendEmail(ApprDesLine.ApproverEmailID, "Hi " + ApprDesLine.ApproverName + " \n You have One OverTime Request Pending for Approval.");
                        }
                        MstEmployee empManager = this.MstEmployee.Where(e => e.ID == instance.ManagerId).FirstOrDefault();
                        if (empManager != null && !string.IsNullOrEmpty(empManager.OfficeEmail))
                        {
                            //SendEmail(empManager.OfficeEmail, "Hi " + empManager.FirstName + " \n Your OverTime Request has been Submited and Pending for Approval.");
                        }
                    }
                    else
                    {
                        //Prepare the document save in open and approved status
                        //when no template found
                        instance.DocType = Doctype;
                        instance.DocStatus = "LV0002";
                        instance.DocStatusLOV = "DocStatus";
                        instance.DocAprStatus = "LV0006";
                        instance.DocAprStatusLOV = "ApprovalStatus";
                    }
                }
                else
                {
                    //Prepare the document save in open and approved status
                    //when no template found
                    instance.DocType = Doctype;
                    instance.DocStatus = "LV0002";
                    instance.DocStatusLOV = "DocStatus";
                    instance.DocAprStatus = "LV0006";
                    instance.DocAprStatusLOV = "ApprovalStatus";
                }
                this.ExecuteDynamicInsert(instance);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }
        private void CheckStageStatusOverTimeRequest(Nesk_CfgApprovalDecisionRegister pints)
        {
            /*Variable Section */
            Byte DocumentType = 24;
            Int32 ApprovalCounts, RejectionCounts;
            Int32 TempApprovals, TempRejections;
            Boolean StageApproved = false, StageRejected = false;
            //Retrive stage lists
            try
            {
                var cStage = pints.PendingAtLevelID;

                //Retrive Current Stage               
                TempApprovals = Convert.ToInt16(pints.PendingAtLevelID);
                TempRejections = Convert.ToInt32(pints.PendingAtLevelID);
                //Retrive Total Number of Stages for Approval
                ApprovalCounts = Convert.ToInt32((from a in this.Nesk_CfgApprovalDecisionRegister
                                                  where a.DocNum == pints.DocNum && a.DocType == DocumentType
                                                  select a).Count());
                if (ApprovalCounts <= TempApprovals && pints.LineStatusID == "LV0006")
                {
                    StageApproved = true;
                }
                //if (RejectionCounts >= TempRejections) StageRejected = true;
                if (!StageApproved && pints.LineStatusID == "LV0006")
                {
                    int CurentStage = 0;
                    CurentStage = pints.PendingAtLevelID.Value;
                    CurentStage = CurentStage + 1;
                    var AllLinesInDoc = from a in this.Nesk_CfgApprovalDecisionRegister
                                        where a.DocNum == pints.DocNum && a.DocType == DocumentType
                                        select a;
                    foreach (var OneLine in AllLinesInDoc)
                    {
                        String UpdateExistingLines = @"UPDATE dbo.Nesk_CfgApprovalDecisionRegister
                                                        SET PendingAtLevelID = " + CurentStage + " WHERE ID = " + OneLine.ID.ToString();
                        this.ExecuteCommand(UpdateExistingLines);
                    }
                    var ApprDesLine = this.Nesk_CfgApprovalDecisionRegister.Where(l => l.DocNum == pints.DocNum && l.LevelID == l.PendingAtLevelID).FirstOrDefault();
                    if (ApprDesLine != null)
                    {
                        if (!string.IsNullOrEmpty(ApprDesLine.ApproverEmailID))
                        {
                            //SendEmail(ApprDesLine.ApproverEmailID, "Hi " + ApprDesLine.ApproverName + " \n You have OverTime Request Pending for Approval.");
                        }
                    }
                }
                if (!StageApproved && pints.LineStatusID == "LV0007")
                {
                    //Kill Lines of Current Stage in CfgApprovalDecisionReport.
                    var AllLinesInDoc = from a in this.Nesk_CfgApprovalDecisionRegister
                                        where a.DocNum == pints.DocNum && a.DocType == DocumentType
                                        select a;
                    foreach (var OneLine in AllLinesInDoc)
                    {
                        String KillExistingLines = @"UPDATE dbo.Nesk_CfgApprovalDecisionRegister
                                                        SET flgActive = 0
                                                        WHERE ID = " + OneLine.ID.ToString();
                        this.ExecuteCommand(KillExistingLines);
                    }
                    //Set The Document Approval Status as Approved and Document Status As Open
                    var TrnsDoc = (from a in this.TrnsEmployeeOvertimeDetail
                                   where a.DocNum == pints.DocNum && a.DocType == pints.DocType
                                   select a).FirstOrDefault();
                    if (TrnsDoc != null)
                    {
                        if (pints.DocType == 24)
                        {
                            String MainDocumentUpdate = @"UPDATE dbo.TrnsEmployeeOvertimeDetail
                                                        SET DocStatus = 'LV0003', DocStatusLOV = 'DocStatus' , DocAprStatus = 'LV0007', DocAprStatusLOV = 'ApprovalStatus'
                                                        WHERE ID = " + TrnsDoc.Id.ToString();
                            this.ExecuteCommand(MainDocumentUpdate);
                        }
                    }
                    TrnsEmployeeOvertimeDetail objEmployeeOTDetail = this.TrnsEmployeeOvertimeDetail.Where(s => s.DocNum == pints.DocNum).FirstOrDefault();
                    if (objEmployeeOTDetail != null)
                    {
                        MstEmployee empManager = this.MstEmployee.Where(e => e.ID == objEmployeeOTDetail.ManagerId).FirstOrDefault();
                        if (empManager != null && !string.IsNullOrEmpty(empManager.OfficeEmail))
                        {
                            // SendEmail(empManager.OfficeEmail, "Hi " + empManager.FirstName + " \n Your OverTime Request has been Rejected By " + pints.ApproverName);
                        }
                    }
                }
                if (!StageApproved && pints.LineStatusID == "LV0030")
                {
                    TrnsEmployeeOvertimeDetail objOT = this.TrnsEmployeeOvertimeDetail.Where(s => s.DocNum == pints.DocNum).FirstOrDefault();
                    if (objOT != null)
                    {
                        MstEmployee empManager = this.MstEmployee.Where(e => e.ID == objOT.ManagerId).FirstOrDefault();
                        if (empManager != null && !string.IsNullOrEmpty(empManager.OfficeEmail))
                        {
                            // SendEmail(empManager.OfficeEmail, "Hi " + empManager.FirstName + " \n Your OverTime Request has been Suspended By " + pints.ApproverName);
                        }
                    }
                }
                if (StageApproved)
                {
                    //Kill Lines of Current Stage in CfgApprovalDecisionReport.
                    var AllLinesInDoc = from a in this.Nesk_CfgApprovalDecisionRegister
                                        where a.DocNum == pints.DocNum && a.DocType == DocumentType
                                        select a;
                    foreach (var OneLine in AllLinesInDoc)
                    {
                        String KillExistingLines = @"UPDATE dbo.Nesk_CfgApprovalDecisionRegister
                                                        SET flgActive = 0
                                                        WHERE ID = " + OneLine.ID.ToString();
                        this.ExecuteCommand(KillExistingLines);
                    }
                    //Set The Document Approval Status as Approved and Document Status As Open
                    var TrnsDoc = (from a in this.TrnsEmployeeOvertimeDetail
                                   where a.DocNum == pints.DocNum && a.DocType == pints.DocType
                                   select a).FirstOrDefault();
                    if (TrnsDoc != null)
                    {
                        if (pints.DocType == 24)
                        {
                            String MainDocumentUpdate = @"UPDATE dbo.TrnsEmployeeOvertimeDetail
                                                        SET DocStatus = 'LV0002', DocStatusLOV = 'DocStatus' , DocAprStatus = 'LV0006', DocAprStatusLOV = 'ApprovalStatus'
                                                        WHERE ID = " + TrnsDoc.Id.ToString();
                            this.ExecuteCommand(MainDocumentUpdate);
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine("{0}", Ex.Message);
            }
        }

        #endregion

        #region Email Send Code

        public static string SendEmail1(string toEmail, string mailBody)
        {
            try
            {
                //string fromEmail = ConfigurationManager.AppSettings["FromEmail"].ToString();
                ////string password = ConfigurationManager.AppSettings["Password"].ToString();
                //string fromEmail = "mznawaz87@gmail.com";
                //string password = "XYZ";

                //smtpClient.Send(mail);
                // Gmail Address from where you send the mail
                var fromAddress = "mznawaz87@gmail.com";
                //Password of your gmail address
                const string fromPassword = ".4710834";
                // any address where the email will be sending
                var toAddress = toEmail;
                // Passing the values and make a email formate to display
                string subject = "AC Payroll Web";
                string body = mailBody;
                // smtp settings
                var smtp = new System.Net.Mail.SmtpClient();
                {
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                    //smtp.Credentials = new NetworkCredential(fromAddress, fromPassword);
                    smtp.Timeout = 20000;
                }
                // Passing values to smtp object
                smtp.Send(fromAddress, toAddress, subject, body);
                return "sent";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion

        #region Logging Employee Transaction



        #endregion

    }
}
