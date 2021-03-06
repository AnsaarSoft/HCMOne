/*
Run this script on:

        mfm-pc.HCMTestHamidTIL    -  This database will be modified

to synchronize it with:

        mfm-pc.HCMEmpty

You are recommended to back up your database before running this script

Script created by SQL Compare version 10.2.0 from Red Gate Software Ltd at 5/16/2014 9:45:22 AM

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
PRINT N'Dropping foreign keys from [dbo].[MstUsersAuth]'
GO
ALTER TABLE [dbo].[MstUsersAuth] DROP CONSTRAINT[FK_MstUsersAuth_MstUsersAuth]
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Altering [dbo].[TrnsSalaryProcessRegisterDetail]'
GO
ALTER TABLE [dbo].[TrnsSalaryProcessRegisterDetail] ALTER COLUMN [NoOfDay] [numeric] (18, 6) NULL
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
PRINT N'Altering [dbo].[TrnsResignation]'
GO
ALTER TABLE [dbo].[TrnsResignation] ADD
[TerminationDate] [datetime] NULL
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Altering [dbo].[TrnsFinalSettelmentRegister]'
GO
ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] ADD
[FSHeadID] [int] NULL,
[MonthDays] [int] NULL
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] DROP
COLUMN [FSStatus]
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] ALTER COLUMN [DaysPaid] [numeric] (18, 6) NULL
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[TrnsFSHead]'
GO
CREATE TABLE [dbo].[TrnsFSHead]
(
[ID] [int] NOT NULL IDENTITY(1, 1),
[internalEmpID] [int] NULL,
[ResignDt] [datetime] NULL,
[TerminationDt] [datetime] NULL,
[PeriodCounts] [int] NULL,
[PayrollID] [int] NULL,
[JournalEntry] [int] NULL,
[DocType] [smallint] NULL,
[DocNum] [int] NULL,
[DocStatus] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[CreateDt] [datetime] NULL,
[UpdateDt] [datetime] NULL,
[CreatedBy] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[UpdatedBy] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating primary key [PK_TrnsFSHead] on [dbo].[TrnsFSHead]'
GO
ALTER TABLE [dbo].[TrnsFSHead] ADD CONSTRAINT [PK_TrnsFSHead] PRIMARY KEY CLUSTERED  ([ID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Altering [dbo].[TrnsFinalSettelmentRegisterDetail]'
GO
ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] ADD
[CostType] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] ALTER COLUMN [NoOfDay] [numeric] (18, 6) NULL
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Adding foreign keys to [dbo].[MstUsersAuth]'
GO
ALTER TABLE [dbo].[MstUsersAuth] ADD CONSTRAINT [FK_MstUsersAuth_MstUsersAuth] FOREIGN KEY ([FunctionID]) REFERENCES [dbo].[MstUserFunctions] ([ID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Adding foreign keys to [dbo].[TrnsFinalSettelmentRegister]'
GO
ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] ADD CONSTRAINT [FK_TrnsFinalSettelmentRegister_TrnsFSHead] FOREIGN KEY ([FSHeadID]) REFERENCES [dbo].[TrnsFSHead] ([ID]) ON DELETE CASCADE
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Adding foreign keys to [dbo].[TrnsFSHead]'
GO
ALTER TABLE [dbo].[TrnsFSHead] ADD CONSTRAINT [FK_TrnsFSHead_MstEmployee] FOREIGN KEY ([internalEmpID]) REFERENCES [dbo].[MstEmployee] ([ID])
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
