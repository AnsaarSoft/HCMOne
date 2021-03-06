/*
Run this script on:

        pk-khi-sap-126\sqlexpress.HRMS    -  This database will be modified

to synchronize it with:

        pk-khi-sap-126\sqlexpress.HRMSSMOLL

You are recommended to back up your database before running this script

Script created by SQL Compare version 10.2.0 from Red Gate Software Ltd at 2014-01-16 3:13:49 PM

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
PRINT N'Dropping foreign keys from [dbo].[TrnsIncrementPromotion]'
GO
ALTER TABLE [dbo].[TrnsIncrementPromotion] DROP CONSTRAINT[FK_TrnsIncrementPromotion_CfgPayrollDefination]
ALTER TABLE [dbo].[TrnsIncrementPromotion] DROP CONSTRAINT[FK_TrnsIncrementPromotion_MstEmployee]
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Dropping constraints from [dbo].[TrnsIncrementPromotion]'
GO
ALTER TABLE [dbo].[TrnsIncrementPromotion] DROP CONSTRAINT [PK_TrnsIncrementPromotion]
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[trnsIncDetail]'
GO
CREATE TABLE [dbo].[trnsIncDetail]
(
[Id] [bigint] NOT NULL IDENTITY(1, 1),
[IncrId] [int] NULL,
[empId] [int] NULL,
[empCode] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[empName] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[cBasic] [decimal] (18, 4) NULL,
[cGross] [decimal] (18, 4) NULL,
[applOn] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[incType] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[incValue] [decimal] (18, 4) NULL,
[newBasic] [decimal] (18, 4) NULL,
[newGross] [decimal] (18, 4) NULL,
[arear] [decimal] (18, 4) NULL
)
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating primary key [PK_trnsIncDetail] on [dbo].[trnsIncDetail]'
GO
ALTER TABLE [dbo].[trnsIncDetail] ADD CONSTRAINT [PK_trnsIncDetail] PRIMARY KEY CLUSTERED  ([Id])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Rebuilding [dbo].[TrnsIncrementPromotion]'
GO
CREATE TABLE [dbo].[tmp_rg_xx_TrnsIncrementPromotion]
(
[Id] [int] NOT NULL IDENTITY(1, 1),
[PayrollID] [int] NULL,
[IncreamentValue] [numeric] (18, 6) NULL,
[IncreamentType] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ApplicableDate] [datetime] NULL,
[ApplyOn] [int] NULL,
[payIn] [int] NULL,
[arearElementId] [int] NULL,
[StatusRec] [int] NULL,
[transId] [bigint] NULL,
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
SET IDENTITY_INSERT [dbo].[tmp_rg_xx_TrnsIncrementPromotion] ON
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
INSERT INTO [dbo].[tmp_rg_xx_TrnsIncrementPromotion]([Id], [PayrollID], [IncreamentValue], [IncreamentType], [ApplicableDate], [ApplyOn], [payIn], [StatusRec], [transId], [CreateDate], [UpdateDate], [UserId], [UpdatedBy]) SELECT [Id], [PayrollID], [IncreamentValue], [IncreamentType], [ApplicableDate], [ApplyOn], [EmployeeID], [StatusRec], [Designation], [CreateDate], [UpdateDate], [UserId], [UpdatedBy] FROM [dbo].[TrnsIncrementPromotion]
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
SET IDENTITY_INSERT [dbo].[tmp_rg_xx_TrnsIncrementPromotion] OFF
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
DECLARE @idVal BIGINT
SELECT @idVal = IDENT_CURRENT(N'[dbo].[TrnsIncrementPromotion]')
IF @idVal IS NOT NULL
    DBCC CHECKIDENT(N'[dbo].[tmp_rg_xx_TrnsIncrementPromotion]', RESEED, @idVal)
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
DROP TABLE [dbo].[TrnsIncrementPromotion]
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
EXEC sp_rename N'[dbo].[tmp_rg_xx_TrnsIncrementPromotion]', N'TrnsIncrementPromotion'
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating primary key [PK_TrnsIncrementPromotion] on [dbo].[TrnsIncrementPromotion]'
GO
ALTER TABLE [dbo].[TrnsIncrementPromotion] ADD CONSTRAINT [PK_TrnsIncrementPromotion] PRIMARY KEY CLUSTERED  ([Id])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Adding foreign keys to [dbo].[trnsIncDetail]'
GO
ALTER TABLE [dbo].[trnsIncDetail] ADD CONSTRAINT [FK_trnsIncDetail_TrnsIncrementPromotion] FOREIGN KEY ([IncrId]) REFERENCES [dbo].[TrnsIncrementPromotion] ([Id])
ALTER TABLE [dbo].[trnsIncDetail] ADD CONSTRAINT [FK_trnsIncDetail_MstEmployee] FOREIGN KEY ([empId]) REFERENCES [dbo].[MstEmployee] ([ID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Adding foreign keys to [dbo].[TrnsIncrementPromotion]'
GO
ALTER TABLE [dbo].[TrnsIncrementPromotion] ADD CONSTRAINT [FK_TrnsIncrementPromotion_CfgPayrollDefination] FOREIGN KEY ([PayrollID]) REFERENCES [dbo].[CfgPayrollDefination] ([ID])
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
