/*
Run this script on:

        ITS-MME-011-PC\MSSQL12.HamidDB    -  This database will be modified

to synchronize it with:

        ITS-MME-DES-11.HamidDB

You are recommended to back up your database before running this script

Script created by SQL Compare version 10.2.0 from Red Gate Software Ltd at 5/28/2014 4:30:49 PM

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
PRINT N'Dropping foreign keys from [dbo].[CfgApprovalTemplateOriginator]'
GO
ALTER TABLE [dbo].[CfgApprovalTemplateOriginator] DROP CONSTRAINT[FK_CfgApprovalTemplateOriginator_CfgApprovalTemplateOriginator]
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Altering [dbo].[ViewApprovalTemplate]'
GO
ALTER VIEW dbo.ViewApprovalTemplate
AS
SELECT     TOP (100) PERCENT dbo.CfgApprovalTemplate.ID, dbo.CfgApprovalTemplate.Name, dbo.CfgApprovalTemplate.Description, 
                      dbo.CfgApprovalTemplateStages.StageID, dbo.CfgApprovalTemplateStages.Priorty, dbo.CfgApprovalTemplateDocuments.flgJobRequisition, 
                      dbo.CfgApprovalTemplateDocuments.flgEmpHiring, dbo.CfgApprovalTemplateDocuments.flgCandidate, 
                      dbo.CfgApprovalTemplateDocuments.flgEmpLeave, dbo.CfgApprovalTemplateDocuments.flgResignation, dbo.CfgApprovalTemplateDocuments.flgLoan, 
                      dbo.CfgApprovalTemplateDocuments.flgAppraisal, dbo.CfgApprovalTemplateDocuments.flgAdvance, dbo.CfgApprovalStage.StageName
FROM         dbo.CfgApprovalTemplate INNER JOIN
                      dbo.CfgApprovalTemplateOriginator ON dbo.CfgApprovalTemplate.ID = dbo.CfgApprovalTemplateOriginator.ATID INNER JOIN
                      dbo.CfgApprovalTemplateStages ON dbo.CfgApprovalTemplate.ID = dbo.CfgApprovalTemplateStages.ATID INNER JOIN
                      dbo.CfgApprovalTemplateDocuments ON dbo.CfgApprovalTemplate.ID = dbo.CfgApprovalTemplateDocuments.ATID INNER JOIN
                      dbo.CfgApprovalStage ON dbo.CfgApprovalTemplateStages.StageID = dbo.CfgApprovalStage.ID
WHERE     (dbo.CfgApprovalTemplate.flgActive = 1)
ORDER BY dbo.CfgApprovalTemplateStages.Priorty
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
PRINT N'Altering extended properties'
GO
EXEC sp_updateextendedproperty N'MS_DiagramPane1', N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1[50] 4[25] 3) )"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1[43] 4) )"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 1
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "CfgApprovalTemplate"
            Begin Extent = 
               Top = 11
               Left = 23
               Bottom = 119
               Right = 174
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "CfgApprovalTemplateOriginator"
            Begin Extent = 
               Top = 120
               Left = 467
               Bottom = 213
               Right = 618
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "CfgApprovalTemplateStages"
            Begin Extent = 
               Top = 5
               Left = 673
               Bottom = 113
               Right = 824
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "CfgApprovalTemplateDocuments"
            Begin Extent = 
               Top = 250
               Left = 224
               Bottom = 358
               Right = 492
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "CfgApprovalStage"
            Begin Extent = 
               Top = 6
               Left = 862
               Bottom = 144
               Right = 1023
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
      PaneHidden = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 16
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
   ', 'SCHEMA', N'dbo', 'VIEW', N'ViewApprovalTemplate', NULL, NULL
GO
IF @@ERROR<>0 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION
GO
IF @@TRANCOUNT=0 BEGIN INSERT INTO #tmpErrors (Error) SELECT 1 BEGIN TRANSACTION END
GO
EXEC sp_updateextendedproperty N'MS_DiagramPane2', N'      Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 4335
         Alias = 900
         Table = 3045
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
', 'SCHEMA', N'dbo', 'VIEW', N'ViewApprovalTemplate', NULL, NULL
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
