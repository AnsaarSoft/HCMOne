
CREATE VIEW [dbo].[SalaryEmployerContrbutions]
AS
SELECT     salary.Id, ISNULL(SUM(Alw_1.LineValue), 0) AS Contribution1, ISNULL(SUM(Alw_2.LineValue), 0) AS Contribution2, ISNULL(SUM(Alw_3.LineValue), 0) AS Contribution3, 
                      ISNULL(SUM(Alw_4.LineValue), 0) AS Contribution4, ISNULL(SUM(Alw_5.LineValue), 0) AS Contribution5
FROM         dbo.TrnsSalaryProcessRegister AS salary LEFT OUTER JOIN
                      dbo.TrnsSalaryProcessRegisterDetail AS Alw_5 ON salary.Id = Alw_5.SRID AND Alw_5.LineMemo = 'X5' LEFT OUTER JOIN
                      dbo.TrnsSalaryProcessRegisterDetail AS Alw_4 ON salary.Id = Alw_4.SRID AND Alw_4.LineMemo = 'X4' LEFT OUTER JOIN
                      dbo.TrnsSalaryProcessRegisterDetail AS Alw_3 ON salary.Id = Alw_3.SRID AND Alw_3.LineMemo = 'Empr Contribution Gratuity' LEFT OUTER JOIN
                      dbo.TrnsSalaryProcessRegisterDetail AS Alw_2 ON salary.Id = Alw_2.SRID AND Alw_2.LineMemo = 'Empr Contribution EOBI' LEFT OUTER JOIN
                      dbo.TrnsSalaryProcessRegisterDetail AS Alw_1 ON salary.Id = Alw_1.SRID AND Alw_1.LineMemo = 'Empr Contribution Prov Fund'
GROUP BY salary.Id