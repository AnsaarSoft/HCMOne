
CREATE VIEW [dbo].[SalaryEarnings]
AS
SELECT     salary.Id, ISNULL(SUM(Alw_1.LineValue), 0) AS Earning1, ISNULL(SUM(Alw_2.LineValue), 0) AS Earning2, ISNULL(SUM(Alw_3.LineValue), 0) AS Earning3, 
                      ISNULL(SUM(Alw_4.LineValue), 0) AS Earning4, ISNULL(SUM(Alw_5.LineValue), 0) AS Earning5
FROM         dbo.TrnsSalaryProcessRegister AS salary LEFT OUTER JOIN
                      dbo.TrnsSalaryProcessRegisterDetail AS Alw_5 ON salary.Id = Alw_5.SRID AND Alw_5.LineSubType = 'X5' LEFT OUTER JOIN
                      dbo.TrnsSalaryProcessRegisterDetail AS Alw_4 ON salary.Id = Alw_4.SRID AND Alw_4.LineSubType = 'X4' LEFT OUTER JOIN
                      dbo.TrnsSalaryProcessRegisterDetail AS Alw_3 ON salary.Id = Alw_3.SRID AND Alw_3.LineSubType = 'X3' LEFT OUTER JOIN
                      dbo.TrnsSalaryProcessRegisterDetail AS Alw_2 ON salary.Id = Alw_2.SRID AND Alw_2.LineSubType = 'PerkCar' LEFT OUTER JOIN
                      dbo.TrnsSalaryProcessRegisterDetail AS Alw_1 ON salary.Id = Alw_1.SRID AND Alw_1.LineSubType = 'SPALW'
GROUP BY salary.Id