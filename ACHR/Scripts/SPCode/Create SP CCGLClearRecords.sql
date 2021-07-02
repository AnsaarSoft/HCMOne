-- ================================================
-- 
--  Muhammad Faisal Maqsood
-- 
-- ================================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		MFM
-- Create date: Aj ki Tareekh
-- Description:	Behtareen kaam
-- =============================================
ALTER PROCEDURE CCGLPerEmployee 
	-- Add the parameters for the stored procedure here
	@SalaryID int = 0
	
WITH ENCRYPTION
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @EmpData TABLE
	(
		ID INT IDENTITY,
		SalaryID INT,
		JEID INT,	
		EmpID INT,
		ElementID INT,
		ElementDesc VARCHAR(100),
		LineValue NUMERIC(18,6),
		AccountCodeDebit VARCHAR(100),
		AccountNameDebit VARCHAR(100),
		AccountCodeCredit VARCHAR(100),
		AccountNameCredit VARCHAR(100)
	)
    
    DECLARE @EmpCCData TABLE
    (
		ID INT IDENTITY,
		EmpID INT,
		TotalHour NUMERIC(18,6),
		HourRatio NUMERIC(18,6),
		CostCenterHour NUMERIC(18,6),
		CostCenter VARCHAR(100)
    )
	
	--Fill Both tables
	
	INSERT INTO @EmpData
	SELECT T1.Id, 0 AS JEID, T1.EmpID, T2.LineBaseEntry, T2.LineMemo, T2.LineValue, T2.DebitAccount, T2.DebitAccountName, T2.CreditAccount, T2.CreditAccountName
	FROM dbo.TrnsSalaryProcessRegister T1 INNER JOIN dbo.TrnsSalaryProcessRegisterDetail T2 ON	T1.Id = T2.SRID
	WHERE T1.Id = @SalaryID
	
	INSERT INTO @EmpCCData
	SELECT
		A3.EmpID,
		0 AS TotalHour,
		0 AS HourRatio,
		SUM(A2.TotalHours) AS TotalHours,
		A2.CostCenter 
	FROM 
		dbo.TrnsAttendanceRegister A1 INNER JOIN dbo.TrnsAttendanceRegisterDetail A2 ON A1.Id = A2.FKID
		INNER JOIN dbo.TrnsSalaryProcessRegister A3 ON A1.EmpID = A3.EmpID AND A1.PeriodID = A3.PayrollPeriodID
	WHERE A3.Id = @SalaryID
	GROUP BY A2.CostCenter, A3.EmpID
	
	UPDATE @EmpCCData
	SET TotalHour = (SELECT CASE WHEN SUM(ISNULL(CostCenterHour,0)) = 0 THEN 1 ELSE SUM(ISNULL(CostCenterHour,0)) END FROM @EmpCCData)	        
	
	UPDATE @EmpCCData 
	SET HourRatio = ISNULL((ISNULL(CostCenterHour,0) / ISNULL(TotalHour,1)),1)
	
	INSERT dbo.TrnsJECCRegister
	SELECT 
		B1.SalaryID, 0 AS JeID, B1.AccountCodeDebit, B1.AccountNameDebit, B1.AccountCodeCredit, B1.AccountNameCredit ,
		B1.LineValue, B1.LineValue * B2.HourRatio AS NewLineValue, B2.CostCenter
	FROM 
		@EmpData B1 INNER JOIN @EmpCCData B2 ON B1.EmpID = B2.EmpID
	
	
END
GO
