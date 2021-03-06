/*
Run this script on:

ITS-MME-DES-11.HCM_TEST    -  This database will be modified

to synchronize it with:

ITS-MME-DES-11.HRMS_EMPTY

You are recommended to back up your database before running this script

Script created by SQL Data Compare version 10.1.0 from Red Gate Software Ltd at 2/13/2014 3:13:59 PM

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

-- Add rows to [dbo].[MstLOVE]
SET IDENTITY_INSERT [dbo].[MstLOVE] ON
INSERT INTO [dbo].[MstLOVE] ([Id], [Code], [Value], [Type], [Language]) VALUES (163, N'M1', N'Machine 1', N'MachineType', N'ln_English')
INSERT INTO [dbo].[MstLOVE] ([Id], [Code], [Value], [Type], [Language]) VALUES (164, N'M2', N'Machine 2', N'MachineType', N'ln_English')
INSERT INTO [dbo].[MstLOVE] ([Id], [Code], [Value], [Type], [Language]) VALUES (165, N'M3', N'Machine 3', N'MachineType', N'ln_English')
SET IDENTITY_INSERT [dbo].[MstLOVE] OFF
-- Operation applied to 3 rows out of 3
COMMIT TRANSACTION
GO
