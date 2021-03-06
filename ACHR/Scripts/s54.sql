/*
Run this script on:

        (local).HCMTest    -  This database will be modified

to synchronize it with:

        MFM-PC.HCMEmpty

You are recommended to back up your database before running this script

Script created by SQL Compare version 10.2.0 from Red Gate Software Ltd at 1/30/2014 11:30:46 AM

*/
SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
IF EXISTS (SELECT * FROM tempdb..sysobjects WHERE id=OBJECT_ID('tempdb..#tmpErrors')) DROP TABLE #tmpErrors
GO
CREATE TABLE #tmpErrors (Error int)
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
GO
BEGIN TRANSACTION
GO
PRINT N'Refreshing [dbo].[ViewApprovalTemplate]'
GO
EXEC sp_refreshview N'[dbo].[ViewApprovalTemplate]'
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Altering [dbo].[MstEmployee]'
GO
ALTER TABLE [dbo].[MstEmployee] ADD
[EnglishName] [nvarchar] (35) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ArabicName] [nvarchar] (35) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[PassportExpiryDt] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[IDExpiryDt] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[MedicalCardExpDt] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[DrvLicCompletionDt] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[DrvLicReleaseDt] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[DrvLicLastDt] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[VisaNo] [nvarchar] (30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[IqamaProfessional] [nvarchar] (30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[BankCardExpiryDt] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Refreshing [dbo].[EmployeeDetail]'
GO
EXEC sp_refreshview N'[dbo].[EmployeeDetail]'
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Refreshing [dbo].[SalaryDeductions]'
GO
EXEC sp_refreshview N'[dbo].[SalaryDeductions]'
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Refreshing [dbo].[SalaryEarnings]'
GO
EXEC sp_refreshview N'[dbo].[SalaryEarnings]'
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Refreshing [dbo].[SalaryEmployerContrbutions]'
GO
EXEC sp_refreshview N'[dbo].[SalaryEmployerContrbutions]'
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
IF EXISTS (SELECT * FROM #tmpErrors) ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT>0 BEGIN
PRINT 'The database update succeeded'
COMMIT TRANSACTION
END
ELSE PRINT 'The database update failed'
GO
DROP TABLE #tmpErrors
GO
