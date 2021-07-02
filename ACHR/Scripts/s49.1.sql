CREATE VIEW [dbo].[EmployeeDetail]
AS
SELECT     dbo.MstEmployee.ID, dbo.MstEmployee.EmpID, dbo.MstLocation.Name AS Location, dbo.MstDesignation.Name AS Designation, 
                      dbo.MstDepartment.DeptName AS Department, dbo.MstBranches.Name AS Branch, dbo.MstJobTitle.Name AS JobTitle
FROM         dbo.MstEmployee LEFT OUTER JOIN
                      dbo.MstDesignation ON dbo.MstEmployee.DesignationID = dbo.MstDesignation.Id LEFT OUTER JOIN
                      dbo.MstDepartment ON dbo.MstEmployee.DepartmentID = dbo.MstDepartment.ID LEFT OUTER JOIN
                      dbo.MstLocation ON dbo.MstEmployee.Location = dbo.MstLocation.Id LEFT OUTER JOIN
                      dbo.MstJobTitle ON dbo.MstEmployee.ID = dbo.MstJobTitle.Id LEFT OUTER JOIN
                      dbo.MstBranches ON dbo.MstEmployee.BranchID = dbo.MstBranches.Id