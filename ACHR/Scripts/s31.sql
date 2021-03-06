/*
Run this script on:

        pk-khi-sap-126\sqlexpress.dbHrmsEmpty    -  This database will be modified

to synchronize it with:

        pk-khi-sap-126\sqlexpress.ASAPayroll

You are recommended to back up your database before running this script

Script created by SQL Compare version 10.2.0 from Red Gate Software Ltd at 2013-12-02 7:36:08 AM

*/
SET NUMERIC_ROUNDABORT OFF

SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON

IF EXISTS (SELECT * FROM tempdb..sysobjects WHERE id=OBJECT_ID('tempdb..#tmpErrors')) DROP TABLE #tmpErrors

CREATE TABLE #tmpErrors (Error int)

SET XACT_ABORT ON

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

BEGIN TRANSACTION

PRINT N'Creating [dbo].[tblRpts]'

CREATE TABLE [dbo].[tblRpts]
(
[ReportId] [bigint] NOT NULL IDENTITY(1, 1),
[rptCode] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ReportName] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[rptFileStr] [varbinary] (max) NULL,
[reportIn] [varchar] (200) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

PRINT N'Creating primary key [PK_tblRpts] on [dbo].[tblRpts]'

ALTER TABLE [dbo].[tblRpts] ADD CONSTRAINT [PK_tblRpts] PRIMARY KEY CLUSTERED  ([ReportId])

IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION

IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END

IF EXISTS (SELECT * FROM #tmpErrors) ROLLBACK TRANSACTION

IF @@TRANCOUNT>0 BEGIN
PRINT 'The database update succeeded'
COMMIT TRANSACTION
END
ELSE PRINT 'The database update failed'

DROP TABLE #tmpErrors

