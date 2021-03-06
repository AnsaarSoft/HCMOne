/*
Run this script on:

        MFM-PC.ZeeshanDB2    -  This database will be modified

to synchronize it with:

        MFM-PC.HCMEmpty

You are recommended to back up your database before running this script

Script created by SQL Compare version 10.2.0 from Red Gate Software Ltd at 1/9/2014 10:20:13 AM

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
PRINT N'Dropping foreign keys from [dbo].[TrnsFinalSettelmentRegisterDetail]'
GO
ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegisterDetail_TrnsFinalSettelmentRegister]
ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegisterDetail_MstAdvance]
ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegisterDetail_MstArrears]
ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegisterDetail_MstBonus]
ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegisterDetail_MstElements]
ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegisterDetail_MstExpense]
ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegisterDetail_MstLoans]
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Dropping foreign keys from [dbo].[TrnsFinalSettelmentRegister]'
GO
ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegister_CfgPayrollDefination]
ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegister_CfgPeriodDates]
ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegister_MstEmployee]
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Dropping constraints from [dbo].[TrnsFinalSettelmentRegister]'
GO
ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] DROP CONSTRAINT [PK_TrnsFinalSettelmentRegister]
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Dropping constraints from [dbo].[TrnsFinalSettelmentRegisterDetail]'
GO
ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] DROP CONSTRAINT [PK_TrnsFinalSettelmentRegisterDetail]
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
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
ALTER TABLE [dbo].[MstEmployee] ALTER COLUMN [OfficeEmail] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
ALTER TABLE [dbo].[MstEmployee] ALTER COLUMN [BankBranch] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
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
PRINT N'Altering [dbo].[MstElements]'
GO
ALTER TABLE [dbo].[MstElements] ALTER COLUMN [ElementName] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Rebuilding [dbo].[TrnsFinalSettelmentRegister]'
GO
CREATE TABLE [dbo].[tmp_rg_xx_TrnsFinalSettelmentRegister]
(
[Id] [int] NOT NULL IDENTITY(1, 1),
[PayrollID] [int] NULL,
[PayrollPeriodID] [int] NULL,
[PayrollName] [nvarchar] (30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[PeriodName] [nvarchar] (30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[EmpID] [int] NULL,
[EmpName] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[EmpGross] [numeric] (18, 6) NULL,
[EmpBasic] [numeric] (18, 6) NULL,
[EmpOT] [numeric] (18, 6) NULL,
[EmpElementTotal] [numeric] (18, 6) NULL,
[EmpAdvance] [numeric] (18, 6) NULL,
[EmpBonus] [numeric] (18, 6) NULL,
[EmpLoan] [numeric] (18, 6) NULL,
[EmpExpense] [numeric] (18, 6) NULL,
[EmpArrears] [numeric] (18, 6) NULL,
[EmpRetroElement] [numeric] (18, 6) NULL,
[JENum] [int] NULL,
[FinalSettlementStatus] [int] NOT NULL,
[EmpTaxblTotal] [numeric] (18, 6) NULL,
[EmpTotalTax] [numeric] (18, 6) NULL,
[DaysPaid] [smallint] NULL,
[FSStatus] [tinyint] NULL,
[CreateDate] [datetime] NULL,
[UpdateDate] [datetime] NULL,
[UserId] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[UpdateBy] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
SET IDENTITY_INSERT [dbo].[tmp_rg_xx_TrnsFinalSettelmentRegister] ON
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
INSERT INTO [dbo].[tmp_rg_xx_TrnsFinalSettelmentRegister]([Id], [PayrollID], [PayrollPeriodID], [PayrollName], [PeriodName], [EmpID], [EmpName], [EmpGross], [EmpBasic], [EmpOT], [EmpElementTotal], [EmpAdvance], [EmpBonus], [EmpLoan], [EmpExpense], [EmpArrears], [EmpRetroElement], [JENum], [FinalSettlementStatus], [CreateDate], [UpdateDate], [UserId], [UpdateBy]) SELECT [Id], [PayrollID], [PayrollPeriodID], [PayrollName], [PeriodName], [EmpID], [EmpName], [EmpGross], [EmpBasic], [EmpOT], [EmpElementTotal], [EmpAdvance], [EmpBonus], [EmpLoan], [EmpExpense], [EmpArrears], [EmpRetroElement], [JENum], [FinalSettlementStatus], [CreateDate], [UpdateDate], [UserId], [UpdateBy] FROM [dbo].[TrnsFinalSettelmentRegister]
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
SET IDENTITY_INSERT [dbo].[tmp_rg_xx_TrnsFinalSettelmentRegister] OFF
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
DROP TABLE [dbo].[TrnsFinalSettelmentRegister]
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
EXEC sp_rename N'[dbo].[tmp_rg_xx_TrnsFinalSettelmentRegister]', N'TrnsFinalSettelmentRegister'
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating primary key [PK_TrnsFinalSettelmentRegister] on [dbo].[TrnsFinalSettelmentRegister]'
GO
ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] ADD CONSTRAINT [PK_TrnsFinalSettelmentRegister] PRIMARY KEY CLUSTERED  ([Id])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Rebuilding [dbo].[TrnsFinalSettelmentRegisterDetail]'
GO
CREATE TABLE [dbo].[tmp_rg_xx_TrnsFinalSettelmentRegisterDetail]
(
[Id] [int] NOT NULL IDENTITY(1, 1),
[FSID] [int] NOT NULL,
[LineType] [nvarchar] (30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LineSubType] [nvarchar] (30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LineValue] [numeric] (18, 6) NULL,
[LineMemo] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LineBaseEntry] [int] NULL,
[BaseValue] [numeric] (18, 6) NULL,
[BaseValueType] [nvarchar] (20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[BaseValueCalculatedOn] [numeric] (18, 6) NULL,
[NoOfDay] [smallint] NULL,
[OTHours] [numeric] (18, 6) NULL,
[CreditAccount] [nvarchar] (16) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[CreditAccountName] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[DebitAccount] [nvarchar] (16) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[DebitAccountName] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[CreditValue] [numeric] (18, 6) NULL,
[DebitValue] [numeric] (18, 6) NULL,
[flgGross] [bit] NULL,
[flgStandard] [bit] NULL,
[SortOrder] [smallint] NULL,
[TaxableAmount] [numeric] (18, 6) NULL,
[CreateDate] [datetime] NULL,
[UpdateDate] [datetime] NULL,
[UserId] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[UpdatedBy] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
SET IDENTITY_INSERT [dbo].[tmp_rg_xx_TrnsFinalSettelmentRegisterDetail] ON
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
INSERT INTO [dbo].[tmp_rg_xx_TrnsFinalSettelmentRegisterDetail]([Id], [FSID], [LineType], [LineValue], [DebitValue], [CreditAccount], [DebitAccount], [CreditValue], [CreateDate], [UpdateDate], [UserId], [UpdatedBy]) SELECT [Id], [FSID], [LineType], [LineValue], [DebitValue], [CreditAccount], [DebitAccount], [CreditValue], [CreateDate], [UpdateDate], [UserId], [UpdatedBy] FROM [dbo].[TrnsFinalSettelmentRegisterDetail]
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
SET IDENTITY_INSERT [dbo].[tmp_rg_xx_TrnsFinalSettelmentRegisterDetail] OFF
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
DROP TABLE [dbo].[TrnsFinalSettelmentRegisterDetail]
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
EXEC sp_rename N'[dbo].[tmp_rg_xx_TrnsFinalSettelmentRegisterDetail]', N'TrnsFinalSettelmentRegisterDetail'
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating primary key [PK_TrnsFinalSettelmentRegisterDetail] on [dbo].[TrnsFinalSettelmentRegisterDetail]'
GO
ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] ADD CONSTRAINT [PK_TrnsFinalSettelmentRegisterDetail] PRIMARY KEY CLUSTERED  ([Id])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Altering [dbo].[tblRpts]'
GO
ALTER TABLE [dbo].[tblRpts] ADD
[Critaria] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Adding foreign keys to [dbo].[TrnsFinalSettelmentRegisterDetail]'
GO
ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] ADD CONSTRAINT [FK_TrnsFinalSettelmentRegisterDetail_TrnsFinalSettelmentRegister] FOREIGN KEY ([FSID]) REFERENCES [dbo].[TrnsFinalSettelmentRegister] ([Id]) ON DELETE CASCADE
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Adding foreign keys to [dbo].[TrnsFinalSettelmentRegister]'
GO
ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] ADD CONSTRAINT [FK_TrnsFinalSettelmentRegister_CfgPayrollDefination] FOREIGN KEY ([PayrollID]) REFERENCES [dbo].[CfgPayrollDefination] ([ID])
ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] ADD CONSTRAINT [FK_TrnsFinalSettelmentRegister_CfgPeriodDates] FOREIGN KEY ([PayrollPeriodID]) REFERENCES [dbo].[CfgPeriodDates] ([ID])
ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] ADD CONSTRAINT [FK_TrnsFinalSettelmentRegister_MstEmployee] FOREIGN KEY ([EmpID]) REFERENCES [dbo].[MstEmployee] ([ID])
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
