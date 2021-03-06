/*
Run this script on:

        MFM-PC.HCMTestDummy    -  This database will be modified

to synchronize it with:

        MFM-PC.HCMEmpty

You are recommended to back up your database before running this script

Script created by SQL Compare version 10.2.0 from Red Gate Software Ltd at 1/15/2014 10:35:07 AM

*/
SET NUMERIC_ROUNDABORT OFF

SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON

IF EXISTS (SELECT * FROM tempdb..sysobjects WHERE id=OBJECT_ID('tempdb..#tmpErrors')) DROP TABLE #tmpErrors

CREATE TABLE #tmpErrors (Error int)

SET XACT_ABORT ON

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

BEGIN TRANSACTION

PRINT N'Dropping foreign keys from [dbo].[MstGLDAdvanceDetail]'

ALTER TABLE [dbo].[MstGLDAdvanceDetail] DROP CONSTRAINT[FK_MstGLDAdvanceDetail_MstGLDetermination]

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Dropping foreign keys from [dbo].[MstGLDBonusDetail]'

ALTER TABLE [dbo].[MstGLDBonusDetail] DROP CONSTRAINT[FK_MstGLDBonusDetail_MstGLDetermination]

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Dropping foreign keys from [dbo].[MstGLDContribution]'

ALTER TABLE [dbo].[MstGLDContribution] DROP CONSTRAINT[FK_MstGLDContribution_MstGLDetermination]

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Dropping foreign keys from [dbo].[MstGLDDeductionDetail]'

ALTER TABLE [dbo].[MstGLDDeductionDetail] DROP CONSTRAINT[FK_MstGLDDeductionDetail_MstGLDetermination]

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Dropping foreign keys from [dbo].[MstGLDEarningDetail]'

ALTER TABLE [dbo].[MstGLDEarningDetail] DROP CONSTRAINT[FK_MstGLDEarningDetail_MstGLDetermination]

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Dropping foreign keys from [dbo].[mstGLDExpDetails]'

ALTER TABLE [dbo].[mstGLDExpDetails] DROP CONSTRAINT[FK_mstGLDExpDetails_mstGLDExpDetails]

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Dropping foreign keys from [dbo].[mstGLDLeaveDedDetails]'

ALTER TABLE [dbo].[mstGLDLeaveDedDetails] DROP CONSTRAINT[FK_mstGLDLeaveDedDetails_mstGLDLeaveDedDetails]

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Dropping foreign keys from [dbo].[MstGLDLoansDetails]'

ALTER TABLE [dbo].[MstGLDLoansDetails] DROP CONSTRAINT[FK_MstGLDLoansDetails_MstGLDetermination]

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Dropping foreign keys from [dbo].[MstGLDOverTimeDetail]'

ALTER TABLE [dbo].[MstGLDOverTimeDetail] DROP CONSTRAINT[FK_MstGLDOverTimeDetail_MstGLDetermination]

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Dropping foreign keys from [dbo].[trnsJEDetail]'

ALTER TABLE [dbo].[trnsJEDetail] DROP CONSTRAINT[FK_trnsJEDetail_trnsJE]

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Altering [dbo].[MstElementContribution]'

ALTER TABLE [dbo].[MstElementContribution] ADD
[flgEOS] [bit] NULL

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Altering [dbo].[MstElementDeduction]'

ALTER TABLE [dbo].[MstElementDeduction] ADD
[flgEOS] [bit] NULL

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Creating primary key [PK_TrnsFinalSettelmentRegister] on [dbo].[TrnsFinalSettelmentRegister]'

ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] ADD CONSTRAINT [PK_TrnsFinalSettelmentRegister] PRIMARY KEY CLUSTERED  ([Id])

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Adding foreign keys to [dbo].[MstGLDAdvanceDetail]'

ALTER TABLE [dbo].[MstGLDAdvanceDetail] ADD CONSTRAINT [FK_MstGLDAdvanceDetail_MstGLDetermination] FOREIGN KEY ([GLDId]) REFERENCES [dbo].[MstGLDetermination] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Adding foreign keys to [dbo].[MstGLDBonusDetail]'

ALTER TABLE [dbo].[MstGLDBonusDetail] ADD CONSTRAINT [FK_MstGLDBonusDetail_MstGLDetermination] FOREIGN KEY ([GLDId]) REFERENCES [dbo].[MstGLDetermination] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Adding foreign keys to [dbo].[MstGLDContribution]'

ALTER TABLE [dbo].[MstGLDContribution] ADD CONSTRAINT [FK_MstGLDContribution_MstGLDetermination] FOREIGN KEY ([GLDId]) REFERENCES [dbo].[MstGLDetermination] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Adding foreign keys to [dbo].[MstGLDDeductionDetail]'

ALTER TABLE [dbo].[MstGLDDeductionDetail] ADD CONSTRAINT [FK_MstGLDDeductionDetail_MstGLDetermination] FOREIGN KEY ([GLDId]) REFERENCES [dbo].[MstGLDetermination] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Adding foreign keys to [dbo].[MstGLDEarningDetail]'

ALTER TABLE [dbo].[MstGLDEarningDetail] ADD CONSTRAINT [FK_MstGLDEarningDetail_MstGLDetermination] FOREIGN KEY ([GLDId]) REFERENCES [dbo].[MstGLDetermination] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Adding foreign keys to [dbo].[mstGLDExpDetails]'

ALTER TABLE [dbo].[mstGLDExpDetails] ADD CONSTRAINT [FK_mstGLDExpDetails_mstGLDExpDetails] FOREIGN KEY ([GLDId]) REFERENCES [dbo].[MstGLDetermination] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Adding foreign keys to [dbo].[mstGLDLeaveDedDetails]'

ALTER TABLE [dbo].[mstGLDLeaveDedDetails] ADD CONSTRAINT [FK_mstGLDLeaveDedDetails_mstGLDLeaveDedDetails] FOREIGN KEY ([GLDId]) REFERENCES [dbo].[MstGLDetermination] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Adding foreign keys to [dbo].[MstGLDLoansDetails]'

ALTER TABLE [dbo].[MstGLDLoansDetails] ADD CONSTRAINT [FK_MstGLDLoansDetails_MstGLDetermination] FOREIGN KEY ([GLDId]) REFERENCES [dbo].[MstGLDetermination] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Adding foreign keys to [dbo].[MstGLDOverTimeDetail]'

ALTER TABLE [dbo].[MstGLDOverTimeDetail] ADD CONSTRAINT [FK_MstGLDOverTimeDetail_MstGLDetermination] FOREIGN KEY ([GLDId]) REFERENCES [dbo].[MstGLDetermination] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Adding foreign keys to [dbo].[TrnsFinalSettelmentRegisterDetail]'

ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] ADD CONSTRAINT [FK_TrnsFinalSettelmentRegisterDetail_TrnsFinalSettelmentRegister] FOREIGN KEY ([FSID]) REFERENCES [dbo].[TrnsFinalSettelmentRegister] ([Id]) ON DELETE CASCADE

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Adding foreign keys to [dbo].[trnsJEDetail]'

ALTER TABLE [dbo].[trnsJEDetail] ADD CONSTRAINT [FK_trnsJEDetail_trnsJE] FOREIGN KEY ([JEID]) REFERENCES [dbo].[trnsJE] ([ID]) ON DELETE CASCADE ON UPDATE CASCADE

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

IF EXISTS (SELECT * FROM #tmpErrors) ROLLBACK TRANSACTION

IF @@TRANCOUNT>0 BEGIN
PRINT 'The database update succeeded'
COMMIT TRANSACTION
END
ELSE PRINT 'The database update failed'

DROP TABLE #tmpErrors

