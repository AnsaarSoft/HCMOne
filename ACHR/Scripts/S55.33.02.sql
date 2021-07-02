/*
Run this script on:

ITS-MME-DES-11.HRMS_EMPTY    -  This database will be modified

to synchronize it with:

ITS-MME-DES-11.HamidDB

You are recommended to back up your database before running this script

Script created by SQL Data Compare version 10.1.0 from Red Gate Software Ltd at 8/7/2014 12:18:05 PM

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

-- Drop constraint FK_TrnsEmployeePenalty_MstPenaltyRules from [dbo].[TrnsEmployeePenalty]
ALTER TABLE [dbo].[TrnsEmployeePenalty] DROP CONSTRAINT [FK_TrnsEmployeePenalty_MstPenaltyRules]

-- Add 4 rows to [dbo].[MstPenaltyRules]
SET IDENTITY_INSERT [dbo].[MstPenaltyRules] ON
INSERT INTO [dbo].[MstPenaltyRules] ([ID], [Code], [Description], [Days], [PenaltyDays], [LeaveType]) VALUES (1, N'PR_01', N'Saturday/Sunday Off Penalty', 1, 2, 7)
INSERT INTO [dbo].[MstPenaltyRules] ([ID], [Code], [Description], [Days], [PenaltyDays], [LeaveType]) VALUES (2, N'PR_02', N'Consective Off Day Penalty', 1, 2, 7)
INSERT INTO [dbo].[MstPenaltyRules] ([ID], [Code], [Description], [Days], [PenaltyDays], [LeaveType]) VALUES (3, N'PR_03', N'Special Day Off Penalty', 1, 2, 7)
INSERT INTO [dbo].[MstPenaltyRules] ([ID], [Code], [Description], [Days], [PenaltyDays], [LeaveType]) VALUES (4, N'PR_04', N'Late Coming Penalty', 4, 1, 7)
SET IDENTITY_INSERT [dbo].[MstPenaltyRules] OFF

-- Add constraint FK_TrnsEmployeePenalty_MstPenaltyRules to [dbo].[TrnsEmployeePenalty]
ALTER TABLE [dbo].[TrnsEmployeePenalty] WITH NOCHECK ADD CONSTRAINT [FK_TrnsEmployeePenalty_MstPenaltyRules] FOREIGN KEY ([PenaltyId]) REFERENCES [dbo].[MstPenaltyRules] ([ID])
COMMIT TRANSACTION
GO
