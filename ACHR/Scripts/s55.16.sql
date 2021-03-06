/*
Run this script on:

        MFM-PC.HCMTestHamidTIL    -  This database will be modified

to synchronize it with:

        MFM-PC.HCMEmpty

You are recommended to back up your database before running this script

Script created by SQL Compare version 10.2.0 from Red Gate Software Ltd at 4/1/2014 4:53:38 PM

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
PRINT N'Dropping foreign keys from [dbo].[TrnsPerformanceAppraisalDetail]'
GO
ALTER TABLE [dbo].[TrnsPerformanceAppraisalDetail] DROP CONSTRAINT[FK_TrnsPerformanceAppraisalDetail_TrnsPerformanceAppraisal]
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Altering [dbo].[TrnsPerformanceAppraisal]'
GO
ALTER TABLE [dbo].[TrnsPerformanceAppraisal] ADD
[DocStatus] [nvarchar] (20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[PlanNumber] [int] NULL
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Altering [dbo].[TrnsPromotionAdvice]'
GO
ALTER TABLE [dbo].[TrnsPromotionAdvice] ADD
[Remarks] [nvarchar] (200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Adding foreign keys to [dbo].[TrnsPerformanceAppraisal]'
GO
ALTER TABLE [dbo].[TrnsPerformanceAppraisal] ADD CONSTRAINT [FK_TrnsPerformanceAppraisal_TrnsPerformancePlan] FOREIGN KEY ([PlanNumber]) REFERENCES [dbo].[TrnsPerformancePlan] ([Id])
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Adding foreign keys to [dbo].[TrnsPerformanceAppraisalDetail]'
GO
ALTER TABLE [dbo].[TrnsPerformanceAppraisalDetail] ADD CONSTRAINT [FK_TrnsPerformanceAppraisalDetail_TrnsPerformanceAppraisal] FOREIGN KEY ([PAID]) REFERENCES [dbo].[TrnsPerformanceAppraisal] ([ID]) ON DELETE CASCADE
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
