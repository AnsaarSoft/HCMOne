/*
Run this script on:

        ITS-MME-DES-11.HRMS_EMPTY    -  This database will be modified

to synchronize it with:

        ITS-MME-DES-11.HamidDB

You are recommended to back up your database before running this script

Script created by SQL Compare version 10.2.0 from Red Gate Software Ltd at 8/7/2014 12:09:57 PM

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
PRINT N'Altering [dbo].[MstElementDeduction]'
GO
ALTER TABLE [dbo].[MstElementDeduction] ADD
[flgPropotionate] [bit] NULL
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Altering [dbo].[MstElementEarning]'
GO
ALTER TABLE [dbo].[MstElementEarning] ADD
[flgPropotionate] [bit] NULL
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[TrnsEmployeePenalty]'
GO
CREATE TABLE [dbo].[TrnsEmployeePenalty]
(
[ID] [int] NOT NULL IDENTITY(1, 1),
[EmpId] [int] NULL,
[PenaltyId] [int] NULL,
[Days] [int] NULL,
[PenaltyDays] [int] NULL,
[FromDate] [datetime] NULL,
[ToDate] [datetime] NULL
)
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating primary key [PK_TrnsEmployeePenalty] on [dbo].[TrnsEmployeePenalty]'
GO
ALTER TABLE [dbo].[TrnsEmployeePenalty] ADD CONSTRAINT [PK_TrnsEmployeePenalty] PRIMARY KEY CLUSTERED  ([ID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[MstPenaltyRules]'
GO
CREATE TABLE [dbo].[MstPenaltyRules]
(
[ID] [int] NOT NULL IDENTITY(1, 1),
[Code] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Description] [nvarchar] (150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Days] [int] NULL,
[PenaltyDays] [int] NULL,
[LeaveType] [int] NULL
)
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating primary key [PK_MstPenaltyRules] on [dbo].[MstPenaltyRules]'
GO
ALTER TABLE [dbo].[MstPenaltyRules] ADD CONSTRAINT [PK_MstPenaltyRules] PRIMARY KEY CLUSTERED  ([ID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[TrnsNaheedAttRegister]'
GO
CREATE TABLE [dbo].[TrnsNaheedAttRegister]
(
[ID] [int] NOT NULL IDENTITY(1, 1),
[EmpID] [int] NULL,
[PeriodID] [int] NULL,
[Date] [datetime] NULL,
[DayName] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ShiftID] [int] NULL,
[ShiftHours] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[TimeIn] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[TimeOut] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[OTCount] [decimal] (18, 2) NULL,
[OTHours] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[WorkHours] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[OnLeave] [bit] NULL,
[flgOffDay] [bit] NULL,
[flgOnSpecialDayLeave] [bit] NULL,
[OnAbsent] [bit] NULL,
[LateInMin] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Processed] [bit] NULL,
[Posted] [bit] NULL,
[CreatedDate] [datetime] NULL,
[UpdatedDate] [datetime] NULL,
[ProcessedBy] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[PostedBy] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[CreatedBy] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[UpdatedBy] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating primary key [PK_TrnsNaheedAttRegister] on [dbo].[TrnsNaheedAttRegister]'
GO
ALTER TABLE [dbo].[TrnsNaheedAttRegister] ADD CONSTRAINT [PK_TrnsNaheedAttRegister] PRIMARY KEY CLUSTERED  ([ID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating [dbo].[MstSpecialDays]'
GO
CREATE TABLE [dbo].[MstSpecialDays]
(
[ID] [int] NOT NULL IDENTITY(1, 1),
[Code] [nvarchar] (20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Description] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[Date] [datetime] NULL,
[CreateDate] [datetime] NULL,
[UpdateDate] [datetime] NULL,
[UserID] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[UpdatedBy] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Creating primary key [PK_MstSpecialDays] on [dbo].[MstSpecialDays]'
GO
ALTER TABLE [dbo].[MstSpecialDays] ADD CONSTRAINT [PK_MstSpecialDays] PRIMARY KEY CLUSTERED  ([ID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Adding foreign keys to [dbo].[TrnsEmployeePenalty]'
GO
ALTER TABLE [dbo].[TrnsEmployeePenalty] ADD CONSTRAINT [FK_TrnsEmployeePenalty_MstPenaltyRules] FOREIGN KEY ([PenaltyId]) REFERENCES [dbo].[MstPenaltyRules] ([ID])
ALTER TABLE [dbo].[TrnsEmployeePenalty] ADD CONSTRAINT [FK_TrnsEmployeePenalty_MstEmployee] FOREIGN KEY ([EmpId]) REFERENCES [dbo].[MstEmployee] ([ID])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Adding foreign keys to [dbo].[TrnsNaheedAttRegister]'
GO
ALTER TABLE [dbo].[TrnsNaheedAttRegister] ADD CONSTRAINT [FK_TrnsNaheedAttRegister_MstEmployee] FOREIGN KEY ([EmpID]) REFERENCES [dbo].[MstEmployee] ([ID])
ALTER TABLE [dbo].[TrnsNaheedAttRegister] ADD CONSTRAINT [FK_TrnsNaheedAttRegister_MstShifts] FOREIGN KEY ([ShiftID]) REFERENCES [dbo].[MstShifts] ([Id])
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
