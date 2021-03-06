/*
Run this script on:

ITS-MME-DES-11.HamidDB    -  This database will be modified

to synchronize it with:

ITS-MME-DES-11.HRMS_EMPTY

You are recommended to back up your database before running this script

Script created by SQL Data Compare version 10.1.0 from Red Gate Software Ltd at 3/4/2014 12:01:37 PM

*/
		
SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS, NOCOUNT ON
GO
SET DATEFORMAT YMD
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO
BEGIN TRANSACTION
-- Pointer used for text / image updates. This might not be needed, but is declared here just in case
DECLARE @pv binary(16)

-- Add 83 rows to [dbo].[MstUserFunctions]
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'360 Degree Performance Appraisal', N'360 Degree Performance Appraisal', 1, N'mnu_360DegPerf')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Accruals View', N'Accruals View', 1, N'mnu_AccrView')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Advance Approval', N'Advance Approval', 1, N'mnu_AdvncApr')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Advance Request', N'Advance Request', 1, N'mnu_AdvncReq')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Advances', N'Advances', 1, N'mnu_Advance')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Approval Decision', N'Approval Decision', 1, N'mnu_ApprDesc')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Approval Stages', N'Approval Stages', 1, N'mnu_ApprStage')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Approval Template', N'Approval Template', 1, N'mnu_ApprTamp')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Attendance Processing', N'Attendance Processing', 1, N'mnu_AttProcess')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Attendance Rules', N'Attendance Rules', 1, N'mnu_GracePerRules')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Batch Creation', N'Batch Creation', 1, N'mnu_BtchCrea')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Branches', N'Branches', 1, N'mnu_Branches')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Budget and Head Count', N'Budget and Head Count', 1, N'mnu_BHcount')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Certification', N'Certification', 1, N'mnu_Certif')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Competency Group', N'Competency Group', 1, N'mnu_CmptncyGrp')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Competency Profile', N'Competency Profile', 1, N'mnu_CmptncyProf')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Connection and Mapping', N'Connection and Mapping', 1, N'mnu_DBConn')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Data Import', N'Data Import', 1, N'mnu_DT')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Deduction Rules', N'Deduction Rules', 1, N'mnu_DeductionRules')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Define Performance Plan', N'Define Performance Plan', 1, N'mnu_PerfPlan')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Department', N'Department', 1, N'mnu_Dept')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Designations', N'Designations', 1, N'mnu_Desg')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Element Setup', N'Element Setup', 1, N'mnu_EleSetup')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Email Setting', N'Email Setting', 1, N'mnu_eMailSett')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Employee Elements', N'Employee Elements', 1, N'mnu_EmpElem')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Employee Leave Assignment', N'Employee Leave Assignment', 1, N'mnu_EmpLev')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Employee Master', N'Employee Master', 1, N'mnu_EmpMst')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Employee Over Time', N'Employee Over Time', 1, N'mnu_empOverTime')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Employee Positions', N'Employee Positions', 1, N'mnu_EmpPosition')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Employee Work Days', N'Employee Work Days', 1, N'mnu_EWD')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Final Settlement / End of Services', N'Final Settlement / End of Services', 1, N'mnu_FS')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Final Settlement Approval', N'Final Settlement Approval', 1, N'mnu_FSAppr')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Formula Builder', N'Formula Builder', 1, N'mnu_FBuilder')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'GL Determination', N'GL Determination', 1, N'mnu_GLAcctDetLoc')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Gratuity Setup', N'Gratuity Setup', 1, N'mnu_Gratuity')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Increments / Promotions', N'Increments / Promotions', 1, N'mnu_Increment')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Institute Master', N'Institute Master', 1, N'mnu_InstMstr')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Interview Call', N'Interview Call', 1, N'mnu_InterviewCall')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Interview Evaluation And Selection', N'Interview Evaluation And Selection', 1, N'mnu_IntEvlSelection')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Job Posting', N'Job Posting', 1, N'mnu_JobPosting')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Job Titles', N'Job Titles', 1, N'mnu_JobTitle')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Key Performance Indicators', N'Key Performance Indicators', 1, N'mnu_KPI')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Languages', N'Languages', 1, N'mnu_Language')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Leave Deduction', N'Leave Deduction', 1, N'mnu_LeaveDed')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Leave Request', N'Leave Request', 1, N'mnu_LeaveRequest')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Leave Types', N'Leave Types', 1, N'mnu_LevType')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Loan', N'Loan', 1, N'mnu_Loan')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Loan Approval', N'Loan Approval', 1, N'mnu_LoanApr')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Loan Request', N'Loan Request', 1, N'mnu_LoanRequest')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Locations', N'Locations', 1, N'mnu_Locations')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Occupation Type', N'Occupation Type', 1, N'mnu_OccpType')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Opening Balance', N'Opening Balance', 1, N'mnu_OpnBal')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Over Time', N'Over Time', 1, N'mnu_OTSetup')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Payroll Calendar', N'Payroll Calendar', 1, N'mnu_Calander')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Payroll Initialization', N'Payroll Initialization', 1, N'mnu_PayrollIni')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Payroll Processing', N'Payroll Processing', 1, N'mnu_Processing')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Payroll Setup', N'Payroll Setup', 1, N'mnu_PayrollSetup')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Performance Appraisal', N'Performance Appraisal', 1, N'mnu_PerfAprsl')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Performance Assesment', N'Performance Assesment', 1, N'mnu_PerfAsmnt')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Performance Period Configuration', N'Performance Period Configuration', 1, N'mnu_PerfPrdConf')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Processing Candidate', N'Processing Candidate', 1, N'mnu_ProcCan')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Promotion Advice', N'Promotion Advice', 1, N'mnu_PromAdvc')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Qualification', N'Qualification', 1, N'mnu_Qualif')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Re Hire Employee', N'Re Hire Employee', 1, N'mnu_ReHireEmp')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Recognition Master', N'Recognition Master', 1, N'mnu_RecgMstr')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Relation Master', N'Relation Master', 1, N'mnu_RelMstr')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Report Setup', N'Report Setup', 1, N'mnu_Reports')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Report Viewer', N'Report Viewer', 1, N'mnu_ShowReports')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Resignation Approval', N'Resignation Approval', 1, N'mnu_ResAprv')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Resignation Request', N'Resignation Request', 1, N'mnu_ResReq')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Retro Element Set', N'Retro Element Set', 1, N'mnu_RetroSet')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Retro-Pay', N'Retro-Pay', 1, N'mnu_RetroPay')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Role', N'Role', 1, N'mnu_Role')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Shift Managment', N'Shift Managment', 1, N'mnu_MstShift')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Shift Schedular', N'Shift Schedular', 1, N'mnu_ShftSch')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Short Listing Candidate', N'Short Listing Candidate', 1, N'mnu_SLCan')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Tax Setup', N'Tax Setup', 1, N'mnu_TxSetup')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'User(s) Authentification', N'User(s) Authentification', 1, N'mnu_userAuth')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Vacancy Requisition', N'Vacancy Requisition', 1, N'mnu_VacReq')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Vacancy Status', N'Vacancy Status', 1, N'mnu_VacStatus')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'Vacancy Types', N'Vacancy Types', 1, N'mnu_VacType')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'View Leave Request', N'View Leave Request', 1, N'mnu_ViewLeaveReq')
INSERT INTO [dbo].[MstUserFunctions] ([FunctionName], [Description], [IsActive], [MenuID]) VALUES (N'View Shift Schedular', N'View Shift Schedular', 1, N'mnu_ViewShiftSch')
COMMIT TRANSACTION
GO
