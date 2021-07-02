using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIHRMS.Custom
{
    public class sqlString
    {
        //MFM No Change Just Update Yes..
        //HCMONE = "920.02.01.01.014"
        public string dbName = "";

        public sqlString(string pdbName)
        {
            dbName = pdbName;
        }

        public string getSql(string SqlKey, Hashtable searchKeyVal)
        {
            string strSql = "";
            string strCondition = " where 1=1 ";
            foreach (string str in searchKeyVal.Keys)
            {
                if (searchKeyVal[str].ToString() == "" || searchKeyVal[str].ToString() == "*")
                {

                }
                else
                {
                    strCondition += " and convert(varchar," + str + ") like '" + searchKeyVal[str].ToString() + "%' ";
                }
            }
            switch (SqlKey)
            {
                case "empElement":
                    strSql = @"
                                SELECT  A1.ID, A1.EmpID, A1.FirstName, A1.MiddleName, A1.LastName, ISNULL(A4.DeptName,'') AS DepartmentName, A1.EmployeeContractType, ISNULL(A1.PayrollName,'') AS PayrollName,ISNULL(A1.PayrollID,'') AS PayrollID, ISNULL(A2.Description,'') AS DesignationName, ISNULL(A3.Description,'') AS PositionName, A5.Name As LocationName, A1.IDNo, A1.PassportNo 
                                FROM    dbo.MstEmployee A1 LEFT OUTER JOIN dbo.MstDesignation A2 ON A1.DesignationID = A2.Id
                                LEFT OUTER JOIN dbo.MstPosition AS A3 ON A1.PositionID = A3.Id
                                LEFT OUTER JOIN dbo.MstDepartment AS A4 ON A1.DepartmentID = A4.ID
                                LEFT OUTER JOIN dbo.MstLocation AS A5	ON A1.Location = A5.Id
                                ";
                    strCondition += " AND ISNULL(A1.flgActive,0) <> 0";
                    strCondition += " AND A1.ResignDate IS NULL";
                    strCondition += " AND ISNULL(A1.PayrollID,'') <> ''";
                    break;
                case "empForOvertime":

                    strSql = "SELECT    ID, EmpID, FirstName, MiddleName,LastName,FirstName+' '+MiddleName+' '+LastName AS FULLNAME,DepartmentName FROM         " + dbName + ".dbo.MstEmployee    ";
                    strCondition += " and  isnull(PayrollID,'')<>'' ";
                    strCondition += " and  isnull(flgOTApplicable,'0')<>'0' ";
                    strCondition += " And ResignDate IS NULL";
                    strCondition += " And isnull(flgactive,0) <> 0";
                    strCondition += " ORDER BY SortOrder";
                    break;
                case "empElementOvertime":

                    strSql = "SELECT     EmpID,ID, FirstName, MiddleName,LastName,DepartmentName,FirstName+' '+MiddleName+' '+LastName AS FULLNAME FROM         " + dbName + ".dbo.MstEmployee    ";
                    strCondition += " and  isnull(PayrollID,'')<>'' ";
                    strCondition += " ORDER BY SortOrder";
                    break;
                case "attMachines":

                    strSql = "SELECT     Code, Value, Type FROM         " + dbName + ".dbo.MstLOVE    ";
                    strCondition += " and  Type='MachineType' ";
                    break;
                case "AdvLoanPayment":
                    strSql = "SELECT DocNum,EmpID,EmpName,RequestedAmount,ApprovedAmount  FROM " + dbName + ".dbo.TrnsAdvance";
                    //strSql = "SELECT     Code, Value, Type FROM         " + dbName + ".dbo.MstLOVE    ";
                    //strCondition += " and  Type='MachineType' ";

                    break;
                case "empAdvance":

                    strSql = "SELECT ID,EmpID,FirstName+' '+MiddleName+' '+LastName AS FULLNAME, FirstName, MiddleName,LastName,DepartmentName FROM       " + dbName + ".dbo.MstEmployee    ";
                    strCondition += " and  isnull(PayrollID,'')<>'' ";
                    strCondition += " and  flgActive='1' ";
                    strCondition += " and ResignDate IS NULL";
                    strCondition += " ORDER BY SortOrder";
                    break;
                case "empAttendanceFrom":

                    strSql = "SELECT EmpID, FirstName, MiddleName,LastName,FirstName+' '+MiddleName+' '+LastName AS FULLNAME FROM " + dbName + ".dbo.MstEmployee    ";
                    strCondition += " and  isnull(PayrollID,'')<>'' ";
                    strCondition += " and  flgActive='1' ";
                    //strCondition += " Order by EmpID";
                    break;
                case "empAttendanceTo":

                    strSql = "SELECT EmpID, FirstName, MiddleName,LastName,FirstName+' '+MiddleName+' '+LastName AS FULLNAME FROM " + dbName + ".dbo.MstEmployee    ";
                    strCondition += " and  isnull(PayrollID,'')<>'' ";
                    strCondition += " and  flgActive='1' ";
                    //strCondition += " Order by EmpID";
                    break;
                case "MstShifts":
                    strSql = "SELECT    Id, Code, Description FROM         " + dbName + ".dbo.MstShifts    ";
                    strCondition += " and  StatusShift='1' ";
                    break;
                case "MstShiftsMaster":
                    strSql = "SELECT    Id, Code, Description FROM         " + dbName + ".dbo.MstShifts    ";
                    break;
                //AR
                case "BatchMaster":
                    strSql = "SELECT  id,BatchName,PayrollName,PayrollPeriod FROM         " + dbName + ".dbo.TrnsBatches ";
                    break;
                //
                case "empResign":
                    strSql = @"
                                SELECT  A1.ID, A1.EmpID, A1.FirstName, A1.MiddleName, A1.LastName, ISNULL(A4.DeptName,'') AS DepartmentName, A1.EmployeeContractType, ISNULL(A1.PayrollName,'') AS PayrollName,ISNULL(A1.PayrollID,'') AS PayrollID, ISNULL(A2.Description,'') AS DesignationName, ISNULL(A3.Description,'') AS PositionName, A5.Name As LocationName, A1.IDNo, A1.PassportNo 
                                FROM    dbo.MstEmployee A1 LEFT OUTER JOIN dbo.MstDesignation A2 ON A1.DesignationID = A2.Id
                                LEFT OUTER JOIN dbo.MstPosition AS A3 ON A1.PositionID = A3.Id
                                LEFT OUTER JOIN dbo.MstDepartment AS A4 ON A1.DepartmentID = A4.ID
                                LEFT OUTER JOIN dbo.MstLocation AS A5	ON A1.Location = A5.Id
                              ";
                    strCondition += " AND ISNULL(A1.flgActive,0) <> 0";
                    strCondition += " AND ResignDate IS NULL";
                    strCondition += " AND TerminationDate IS NULL";
                    break;
                case "empFSN":
                    strSql = @"
                                SELECT  A1.ID, A1.EmpID, A1.FirstName, A1.MiddleName, A1.LastName, ISNULL(A4.DeptName,'') AS DepartmentName, A1.EmployeeContractType, ISNULL(A1.PayrollName,'') AS PayrollName,ISNULL(A1.PayrollID,'') AS PayrollID, ISNULL(A2.Description,'') AS DesignationName, ISNULL(A3.Description,'') AS PositionName, A5.Name As LocationName, A1.IDNo, A1.PassportNo 
                                FROM    dbo.MstEmployee A1 LEFT OUTER JOIN dbo.MstDesignation A2 ON A1.DesignationID = A2.Id
                                LEFT OUTER JOIN dbo.MstPosition AS A3 ON A1.PositionID = A3.Id
                                LEFT OUTER JOIN dbo.MstDepartment AS A4 ON A1.DepartmentID = A4.ID
                                LEFT OUTER JOIN dbo.MstLocation AS A5	ON A1.Location = A5.Id";

                    strCondition += " AND A1.ResignDate IS NOT NULL";
                    strCondition += " AND A1.TerminationDate IS NOT NULL";
                    strCondition += " AND A1.PaymentMode LIKE 'HOLD%'";

                    break;
                case "empFSApr":
                    strSql = @"                                
                                SELECT M2.ID, M2.EmpID, M2.FirstName, M2.MiddleName, M2.LastName, M2.DepartmentName,M2.FirstName+' '+m2.MiddleName+' '+m2.LastName AS FullName
                                FROM dbo.TrnsFSHead M1 INNER JOIN dbo.MstEmployee M2 ON M1.internalEmpID = M2.ID
                                ORDER BY M2.SortOrder
                               ";
                    strCondition = "";
                    break;

                case "DailyWagers":

                    strSql = "SELECT     ID,EmpID, FirstName,MiddleName, LastName,DepartmentName,FirstName+' '+MiddleName+' '+LastName AS FULLNAME FROM         " + dbName + ".dbo.MstEmployee    ";
                    strCondition += " and  isnull(PayrollID,'')<>'' ";
                    strCondition += " and  flgActive='1' ";
                    strCondition += " and  EmployeeContractType='DWGS' ";
                    strCondition += " Order by EmpID";
                    break;
                case "empLoan":

                    strSql = "SELECT      ID,EmpID, FirstName,MiddleName, LastName,BasicSalary,DepartmentName,FirstName+' '+MiddleName+' '+LastName AS FULLNAME FROM         " + dbName + ".dbo.MstEmployee    ";
                    strCondition += " and  isnull(PayrollID,'')<>'' ";
                    strCondition += " and  flgActive='1' ";
                    strCondition += " and ResignDate IS NULL";
                    strCondition += " ORDER BY SortOrder";
                    break;

                case "empLoanbGuarantors":

                    strSql = "SELECT      ID,EmpID, FirstName,MiddleName, LastName,BasicSalary,DepartmentName,FirstName+' '+MiddleName+' '+LastName AS FULLNAME FROM         " + dbName + ".dbo.MstEmployee    ";
                    strCondition += " and  isnull(PayrollID,'')<>'' ";
                    strCondition += " and  flgActive='1' ";
                    strCondition += " and ResignDate IS NULL";
                    strCondition += " ORDER BY SortOrder";
                    break;

                case "empLeaveReq":
                    strSql = "SELECT     ID,EmpID, FirstName,MiddleName, LastName,DepartmentName,FirstName+' '+MiddleName+' '+LastName AS FULLNAME FROM         " + dbName + ".dbo.MstEmployee    ";
                    strCondition += " and  flgActive='1' ";
                    //strCondition += " Order by EmpID";//Change due to sort id mechanism.
                    strCondition += " ORDER BY SortOrder";
                    break;
                case "ActiveUsers":

                    strSql = "SELECT     UserCode, UserID AS UserName FROM         " + dbName + ".dbo.MstUsers    ";
                    strCondition += " and  flgActiveUser='1' ";

                    break;
                case "AdvApprovalDoc":

                    strSql = "SELECT     DocNum, EmpName  FROM         " + dbName + ".dbo.TrnsAdvance    ";
                    strCondition += " and   DocStatus='LV0001' and DocAprStatus='LV0005' ";

                    break;
                case "LoanApprovalDoc":

                    strSql = "SELECT     DocNum, EmpName  FROM         " + dbName + ".dbo.TrnsLoan    ";
                    strCondition += " and   DocStatus='LV0001' and DocAprStatus='LV0005' ";

                    break;
                case "elementSetup":
                    strSql = "SELECT Id, ElementName, Description, StartDate, EndDate, ElmtType, flgProcessInPayroll, flgStandardElement FROM         " + dbName + ".dbo.MstElements ";
                    break;
                case "PayrollShifts":
                    strSql = "SELECT [Code], [Description] FROM [dbo].[MstShifts] WHERE ISNULL([StatusShift], 0) = 1";
                    strCondition = "";
                    break;
                case "empMaster":
                    //strSql = "SELECT    ID, EmpID, FirstName,MiddleName, LastName,DepartmentName,EmployeeContractType,PayrollName,DesignationName,PositionName,LocationName,IDNo,PassportNo FROM         " + dbName + ".dbo.MstEmployee   ";
                    strSql = @"SELECT  A1.ID, A1.EmpID, ISNULL(A1.FirstName,'') as FirstName, ISNULL(A1.MiddleName,'') as MiddleName, ISNULL(A1.LastName,'') as LastName, ISNULL(A4.DeptName,'') AS DepartmentName, A1.EmployeeContractType, ISNULL(A1.PayrollName,'') AS PayrollName,ISNULL(A1.PayrollID,'') AS PayrollID, ISNULL(A2.Description,'') AS DesignationName, ISNULL(A3.Description,'') AS PositionName, A5.Name As LocationName, A1.IDNo, A1.PassportNo 
                               FROM    dbo.MstEmployee A1 LEFT OUTER JOIN dbo.MstDesignation A2 ON A1.DesignationID = A2.Id
			                   LEFT OUTER JOIN dbo.MstPosition AS A3 ON A1.PositionID = A3.Id
			                   LEFT OUTER JOIN dbo.MstDepartment AS A4 ON A1.DepartmentID = A4.ID
			                   LEFT OUTER JOIN dbo.MstLocation AS A5 ON A1.Location = A5.Id
                               ";
                    //strCondition += " and  flgActive='1' ";
                    break;
                case "empPick":
                    strSql = @"
                                SELECT  A1.ID, A1.EmpID, ISNULL(A1.FirstName,'') as FirstName, ISNULL(A1.MiddleName,'') as MiddleName, ISNULL(A1.LastName,'') as LastName, ISNULL(A4.DeptName,'') AS DepartmentName, A1.EmployeeContractType, ISNULL(A1.PayrollName,'') AS PayrollName,ISNULL(A1.PayrollID,'') AS PayrollID, ISNULL(A2.Description,'') AS DesignationName, ISNULL(A3.Description,'') AS PositionName, A5.Name As LocationName, A1.IDNo, A1.PassportNo 
                                FROM    dbo.MstEmployee A1 LEFT OUTER JOIN dbo.MstDesignation A2 ON A1.DesignationID = A2.Id
                                LEFT OUTER JOIN dbo.MstPosition AS A3 ON A1.PositionID = A3.Id
                                LEFT OUTER JOIN dbo.MstDepartment AS A4 ON A1.DepartmentID = A4.ID
                                LEFT OUTER JOIN dbo.MstLocation AS A5	ON A1.Location = A5.Id
                              ";
                    strCondition += " AND ISNULL(A1.flgActive,0) <> 0 AND ResignDate IS NULL";
                    break;
                case "empMasterPP":
                    strSql = @"SELECT  A1.ID, A1.EmpID, A1.FirstName, A1.MiddleName, A1.LastName, ISNULL(A4.DeptName,'') AS DepartmentName, A1.EmployeeContractType, ISNULL(A1.PayrollName,'') AS PayrollName,ISNULL(A1.PayrollID,'') AS PayrollID, ISNULL(A2.Description,'') AS DesignationName, ISNULL(A3.Description,'') AS PositionName, A5.Name As LocationName, A1.IDNo, A1.PassportNo 
                               FROM    dbo.MstEmployee A1 LEFT OUTER JOIN dbo.MstDesignation A2 ON A1.DesignationID = A2.Id
			                   LEFT OUTER JOIN dbo.MstPosition AS A3 ON A1.PositionID = A3.Id
			                   LEFT OUTER JOIN dbo.MstDepartment AS A4 ON A1.DepartmentID = A4.ID
			                   LEFT OUTER JOIN dbo.MstLocation AS A5	ON A1.Location = A5.Id
                               WHERE ISNULL(A1.flgPerPiece,0) = 1 AND ResignDate IS NULL";
                    strCondition = "";
                    break;
                case "PayrollSetup":

                    strSql = "SELECT    pr.ID, pr.PayrollName, prt.Value AS [Processing Type], pr.GLType ";
                    strSql += " FROM         " + dbName + ".dbo.CfgPayrollDefination AS pr INNER JOIN ";
                    strSql += "    " + dbName + ".dbo.MstLOVE AS prt ON pr.PayrollType = prt.Code AND prt.Type = pr.PayrollTypeLOVType";
                    break;
                case "PayrollEmps":
                    strSql = "SELECT     EmpID, FirstName, MiddleName,LastName FROM   " + dbName + ".dbo.MstEmployee as emp  inner join  " + dbName + ".dbo.CfgPayrollDefination AS pr  ";
                    strSql += " on pr.ID = emp.PayrollID ";
                    break;
                case "GetPayrollName":
                    strSql = "SELECT * FROM   " + dbName + ".dbo.CfgPayrollDefination";
                    strCondition = "";
                    break;
                case "mstEmployee":
                    strSql = "SELECT    emp.ID, emp.EmpID, ISNULL(emp.FirstName,'') AS [First Name], ISNULL(emp.MiddleName,'') AS [Middle Name], ISNULL(emp.LastName,'') AS [Last Name], ISNULL(emp.JobTitle,'') AS [Job Title],  ";
                    strSql += " ISNULL(emp.DepartmentName,'') AS Department, ISNULL(emp.LocationName,'') AS Location ";
                    strSql += "     FROM         " + dbName + ".dbo.MstEmployee AS emp";
                    break;
                case "apprStages":
                    strSql = "SELECT  [ID]   ,[StageName] as [Stage] ,[StageDescription] as [Description]  FROM  " + dbName + ".[dbo].[CfgApprovalStage] ";
                    break;
                case "authourizer":
                    //strSql = "select USERID,user_code , U_NAME from ousr";
                    strSql = "SELECT   t0.UserID,t1.FirstName ,dept.DeptName FROM     " + dbName + ".dbo.MstUsers t0 INNER JOIN " + dbName + ".dbo.MstEmployee t1 ON t0.empid = t1.id inner join " + dbName + ".dbo.MstDepartment dept on dept.ID = t1.DepartmentID";
                    break;

                case "apprTemp":
                    strSql = "SELECT ID, Name [Template], Description , flgActive as [Active]  FROM        " + dbName + ".dbo.CfgApprovalTemplate";
                    break;
                case "otmst":
                    strSql = "SELECT     Code,Description FROM      " + dbName + ".dbo.MstOverTime";
                    strCondition += " and  flgActive='1' ";
                    break;
                case "glDept":
                    strSql = @" SELECT     dept.ID, dept.DeptName AS Department, gl.Id AS [GL File]
                               FROM         " + dbName + @".dbo.MstGLDetermination as gl RIGHT OUTER JOIN
                      " + dbName + @".dbo.MstDepartment as dept ON gl.GLValue = dept.ID AND gl.GLType = 'DEPT'";
                    strCondition = "";

                    break;
                case "glLoc":
                    strSql = @" SELECT     loc.Id, loc.Name AS Location, gl.Id AS [GL File]
                                FROM         " + dbName + @".dbo.MstGLDetermination as gl RIGHT OUTER JOIN
                                 " + dbName + @".dbo.MstLocation as loc ON gl.GLValue = loc.Id AND gl.GLType = 'LOC' ";
                    strCondition = "";

                    break;
                case "OTSlabSearch":
                    strSql = @"SELECT internalID AS ID, SlabCode AS Code FROM dbo.TrnsOTSlab";
                    strCondition = "";

                    break;
                case "shortlistsearch":
                    strSql = @"SELECT     Can.ID, Can.CandidateNo AS [Candidate No], C.Name AS Branch, B.Code AS Department, A.Name AS Position, Can.JobRequisitionNo AS [Vacancy No] 
			                            , Can.ValidFrom AS [Valid From],Can.ValidTo AS [Valid To]
                               FROM         " + dbName + @".dbo.MstCandidate AS Can LEFT OUTER JOIN
                                            " + dbName + @".dbo.MstPosition AS A ON Can.Position = A.Id LEFT OUTER JOIN
                                            " + dbName + @".dbo.MstBranches AS C ON Can.Branch = C.Id LEFT OUTER JOIN
                                            " + dbName + @".dbo.MstDepartment AS B ON Can.Department = B.ID";
                    String candidatefrom, candidateto, jrfrom, jrto, validto, validfrom;
                    if (searchKeyVal["candidatefrom"].ToString() == "")
                    {
                        candidatefrom = "0";
                    }
                    else
                    {
                        candidatefrom = searchKeyVal["candidatefrom"].ToString();
                    }
                    if (searchKeyVal["candidateto"].ToString() == "")
                    {
                        candidateto = "1000000";
                    }
                    else
                    {
                        candidateto = searchKeyVal["candidateto"].ToString();
                    }
                    if (searchKeyVal["jrfrom"].ToString() == "")
                    {
                        jrfrom = "0";
                    }
                    else
                    {
                        jrfrom = searchKeyVal["jrfrom"].ToString();
                    }
                    if (searchKeyVal["jrto"].ToString() == "")
                    {
                        jrto = "1000000";
                    }
                    else
                    {
                        jrto = searchKeyVal["jrto"].ToString();
                    }
                    if (searchKeyVal["validfrom"].ToString() == "")
                    {
                        validfrom = "01/01/2000";
                    }
                    else
                    {
                        validfrom = searchKeyVal["validfrom"].ToString();
                    }
                    if (searchKeyVal["validto"].ToString() == "")
                    {
                        validto = "01/01/2030";
                    }
                    else
                    {
                        validto = searchKeyVal["validto"].ToString();
                    }
                    strCondition = " WHERE  Can.CandidateNo BETWEEN '" + candidatefrom + "' AND '" + candidateto + "' AND Can.JobRequisitionNo BETWEEN '" + jrfrom + "' AND '" + jrto + "' AND Isnull(c.Name,'') LIKE '" + searchKeyVal["branch"].ToString() + "%'";
                    strCondition += " AND Isnull(B.Code,'') LIKE '" + searchKeyVal["department"].ToString() + "%' AND Isnull(A.Name,'') LIKE '" + searchKeyVal["position"].ToString() + "%' AND Can.StaffingStatus LIKE '" + searchKeyVal["flag"].ToString() + "%' ";
                    break;

                case "SLSearch":
                    strSql = @"
                                SELECT     
	                                dbo.MstCandidate.ID, dbo.MstCandidate.CandidateNo, dbo.MstCandidate.FirstName, dbo.MstCandidate.MiddleName, dbo.MstCandidate.LastName, 
	                                dbo.MstCandidate.ValidFrom, dbo.MstCandidate.ValidTo, dbo.MstCandidate.JobRequisitionNo, dbo.MstPosition.Name AS Position, 
	                                dbo.MstDepartment.DeptName AS Department, dbo.MstBranches.Name AS Branches, dbo.MstLocation.Name AS Location
                                FROM        
                                    dbo.MstCandidate LEFT OUTER JOIN
                                    dbo.MstDesignation ON dbo.MstCandidate.Designation = dbo.MstDesignation.Id LEFT OUTER JOIN
                                    dbo.MstLocation ON dbo.MstCandidate.Location = dbo.MstLocation.Id LEFT OUTER JOIN
                                    dbo.MstPosition ON dbo.MstCandidate.Position = dbo.MstPosition.Id LEFT OUTER JOIN
                                    dbo.MstBranches ON dbo.MstCandidate.Branch = dbo.MstBranches.Id LEFT OUTER JOIN
                                    dbo.MstDepartment ON dbo.MstCandidate.Department = dbo.MstDepartment.ID
                                WHERE 
	                                ISNULL(dbo.MstDepartment.DeptName,'') LIKE '" + searchKeyVal["Department"].ToString() + @"%' AND
	                                ISNULL(dbo.MstBranches.Name,'') LIKE '" + searchKeyVal["Branches"].ToString() + @"%' AND
	                                ISNULL(dbo.MstLocation.Name,'') LIKE '" + searchKeyVal["Location"].ToString() + @"%' AND
	                                ISNULL(dbo.MstDesignation.Name,'') LIKE '" + searchKeyVal["Designation"].ToString() + @"%' AND
                                    ISNULL(dbo.MstCandidate.StaffingStatus,'OPEN') LIKE '" + searchKeyVal["Status"].ToString() + @"%' AND
	                                dbo.MstCandidate.CandidateNo BETWEEN '" + searchKeyVal["CanFrom"].ToString() + @"' AND '" + searchKeyVal["CanTo"].ToString() + @"' AND
	                                dbo.MstCandidate.JobRequisitionNo BETWEEN '" + searchKeyVal["JRFrom"].ToString() + @"' AND '" + searchKeyVal["JRTo"].ToString() + @"' AND
	                                dbo.MstCandidate.ValidFrom BETWEEN '" + searchKeyVal["ValidFrom"].ToString() + @"' AND '" + searchKeyVal["ValidTo"].ToString() + @"' AND
	                                dbo.MstCandidate.ValidTo BETWEEN '" + searchKeyVal["ValidFrom"].ToString() + @"' AND '" + searchKeyVal["ValidTo"].ToString() + @"'
                               ";
                    strCondition = "";
                    break;
                case "JEQuery":
                    strSql = @"
                        SELECT   dbo.TrnsSalaryProcessRegisterDetail.DebitAccount AcctCode,  dbo.TrnsSalaryProcessRegisterDetail.DebitAccountName AcctName, SUM(ABS(dbo.TrnsSalaryProcessRegisterDetail.LineValue)) AS Debit, 0 AS Credit , ISNULL(CostCenter,'') AS CostCenter , ISNULL(Project,'') AS Project, ISNULL(SalaryCurrency,'') AS EmpCurr
                        FROM    dbo.TrnsSalaryProcessRegister INNER JOIN
                        dbo.TrnsSalaryProcessRegisterDetail ON dbo.TrnsSalaryProcessRegister.Id = dbo.TrnsSalaryProcessRegisterDetail.SRID inner join 
                        MstEmployee on MstEmployee.ID = TrnsSalaryProcessRegister.EmpID
                        Where TrnsSalaryProcessRegister.ID in (" + searchKeyVal["spIds"].ToString() + @")
                        GROUP BY dbo.TrnsSalaryProcessRegisterDetail.DebitAccount,dbo.TrnsSalaryProcessRegisterDetail.DebitAccountName,costcenter,Project,SalaryCurrency

                        UNION

                        SELECT  dbo.TrnsSalaryProcessRegisterDetail.creditAccount AcctCode, dbo.TrnsSalaryProcessRegisterDetail.creditAccountName AcctName, 0 as Debit, SUM(ABS(dbo.TrnsSalaryProcessRegisterDetail.LineValue)) AS Credit , ISNULL(CostCenter,'') AS CostCenter , ISNULL(Project,'') AS Project, ISNULL(SalaryCurrency,'') AS EmpCurr
                        FROM    dbo.TrnsSalaryProcessRegister INNER JOIN
                        dbo.TrnsSalaryProcessRegisterDetail ON dbo.TrnsSalaryProcessRegister.Id = dbo.TrnsSalaryProcessRegisterDetail.SRID inner join 
                        MstEmployee on MstEmployee.ID = TrnsSalaryProcessRegister.EmpID
                        Where TrnsSalaryProcessRegister.ID in (" + searchKeyVal["spIds"].ToString() + @")
                        GROUP BY dbo.TrnsSalaryProcessRegisterDetail.creditAccount,dbo.TrnsSalaryProcessRegisterDetail.creditAccountName,costcenter,Project,SalaryCurrency

                        ORDER BY Project, CostCenter, Credit";

                    strCondition = "";
                    break;
                case "JEQueryClass":
                    strSql = @"
                                SELECT
	                                ISNULL(dAccountCode, '') AS AcctCode,
	                                ISNULL(dAccountDesc, '') AS AcctName,
	                                SUM(ISNULL(LineValue,0)) AS Debit,
	                                0 AS Credit,
	                                ISNULL(A2.CostCenter,'') AS CostCenter,
                                    ISNULL(A1.Location,0) AS Location
	
                                FROM
	                                dbo.TrnsSalaryClassification A1
	                                INNER JOIN dbo.MstEmployee A2 ON A1.EmpID = A2.ID 
                                WHERE
	                                A2.EmpID IN (" + searchKeyVal["EmpID"].ToString() + @")
	                                AND A1.PeriodID = " + searchKeyVal["PeriodID"].ToString() + @"
                                GROUP BY A1.Location, dAccountCode, dAccountDesc, CostCenter

                                UNION

                                SELECT
	                                ISNULL(cAccountCode, '') AS AcctCode,
	                                ISNULL(cAccountDesc, '') AS AcctName,
	                                0 AS Debit,
	                                SUM(ISNULL(LineValue,0)) AS Credit,
	                                ISNULL(A2.CostCenter,'') AS CostCenter,
                                    ISNULL(A1.Location,0) AS Location
	
                                FROM
	                                dbo.TrnsSalaryClassification A1
	                                INNER JOIN dbo.MstEmployee A2 ON A1.EmpID = A2.ID 
                                WHERE
	                                A2.EmpID IN (" + searchKeyVal["EmpID"].ToString() + @")
	                                AND A1.PeriodID = " + searchKeyVal["PeriodID"].ToString() + @"
                                GROUP BY A1.Location, cAccountCode, cAccountDesc, CostCenter";
                    strCondition = "";
                    break;
                case "JEQueryMFM":
                    strSql = @"SELECT        TrnsSalaryProcessRegisterDetail.DebitAccount AS AcctCode, TrnsSalaryProcessRegisterDetail.DebitAccountName AS AcctName, 
                                                         SUM(ABS(TrnsSalaryProcessRegisterDetail.LineValue)) AS Debit, 0 AS Credit, MstEmployee.CostCenter
                                FROM            TrnsSalaryProcessRegister INNER JOIN
                                                         TrnsSalaryProcessRegisterDetail ON TrnsSalaryProcessRegister.Id = TrnsSalaryProcessRegisterDetail.SRID INNER JOIN
                                                         MstEmployee ON MstEmployee.ID = TrnsSalaryProcessRegister.EmpID
                                WHERE        (TrnsSalaryProcessRegister.Id IN (" + searchKeyVal["spIds"].ToString() + @")) AND (TrnsSalaryProcessRegisterDetail.LineValue > 0)
                                GROUP BY TrnsSalaryProcessRegisterDetail.DebitAccount, TrnsSalaryProcessRegisterDetail.DebitAccountName, MstEmployee.CostCenter
                                UNION
                                SELECT        TrnsSalaryProcessRegisterDetail.CreditAccount AS AcctCode, TrnsSalaryProcessRegisterDetail.CreditAccountName AS AcctName, 0 AS Debit, 
                                                         SUM(ABS(TrnsSalaryProcessRegisterDetail.LineValue)) AS Credit, MstEmployee.CostCenter
                                FROM            TrnsSalaryProcessRegister INNER JOIN
                                                         TrnsSalaryProcessRegisterDetail ON TrnsSalaryProcessRegister.Id = TrnsSalaryProcessRegisterDetail.SRID INNER JOIN
                                                         MstEmployee ON MstEmployee.ID = TrnsSalaryProcessRegister.EmpID
                                WHERE        (TrnsSalaryProcessRegister.Id IN (" + searchKeyVal["spIds"].ToString() + @")) AND (TrnsSalaryProcessRegisterDetail.LineValue < 0)
                                GROUP BY TrnsSalaryProcessRegisterDetail.CreditAccount, TrnsSalaryProcessRegisterDetail.CreditAccountName, MstEmployee.CostCenter
                                UNION
                                SELECT        TOP (1) TrnsSalaryProcessRegisterDetail.CreditAccount AS AcctCode, TrnsSalaryProcessRegisterDetail.CreditAccountName AS AcctName, 0 AS Debit,
                                                             ISNULL((SELECT        SUM(ABS(A2.LineValue)) AS Expr1
                                                               FROM            TrnsSalaryProcessRegister AS A1 INNER JOIN
                                                                                         TrnsSalaryProcessRegisterDetail AS A2 ON A1.Id = A2.SRID
                                                               WHERE        (A1.Id IN (" + searchKeyVal["spIds"].ToString() + @")) AND (A2.LineValue > 0)) -
                                                             (SELECT        SUM(ABS(A2.LineValue)) AS Expr1
                                                               FROM            TrnsSalaryProcessRegister AS A1 INNER JOIN
                                                                                         TrnsSalaryProcessRegisterDetail AS A2 ON A1.Id = A2.SRID
                                                               WHERE        (A1.Id IN (" + searchKeyVal["spIds"].ToString() + @")) AND (A2.LineValue < 0)),0) AS Credit, ISNULL(MstEmployee.CostCenter, '') As CostCenter
                                FROM            TrnsSalaryProcessRegister INNER JOIN
                                                         TrnsSalaryProcessRegisterDetail ON TrnsSalaryProcessRegister.Id = TrnsSalaryProcessRegisterDetail.SRID INNER JOIN
                                                         MstEmployee ON MstEmployee.ID = TrnsSalaryProcessRegister.EmpID
                                WHERE        (TrnsSalaryProcessRegister.Id IN (" + searchKeyVal["spIds"].ToString() + @")) AND (TrnsSalaryProcessRegisterDetail.LineType = 'BS')
                                GROUP BY TrnsSalaryProcessRegisterDetail.CreditAccount, TrnsSalaryProcessRegisterDetail.CreditAccountName, MstEmployee.CostCenter
                                ORDER BY MstEmployee.CostCenter, Credit";

                    strCondition = "";
                    break;
                case "JEQueryA1":
                    strSql = @"
                                SELECT 
	                                A4.DeptName Reference,
	                                A3.OrganizationalUnit CompCode,
	                                A3.SalaryCurrency Currency,
                                    'S' AcctType,
	                                A2.DebitAccount AcctCode,
	                                '' SpecialGLIndicator,
	                                A2.DebitAccountName AcctName,
	                                'D' GLAcctType,
	                                SUM(ABS(A2.LineValue)) AS Amount,
	                                ISNULL(A3.CostCenter,'') CostCenter,
	                                A3.ProfitCenter
                                FROM 
	                                dbo.TrnsSalaryProcessRegister A1 INNER JOIN dbo.TrnsSalaryProcessRegisterDetail A2 ON A1.Id = A2.SRID
	                                INNER JOIN dbo.MstEmployee A3 ON A1.EmpID = A3.ID
	                                INNER JOIN dbo.MstDepartment A4 ON A3.DepartmentID = A4.ID
                                WHERE
	                                A1.Id IN (" + searchKeyVal["spIds"].ToString() + @")
                                GROUP BY A2.DebitAccount, A4.DeptName, A2.DebitAccountName, A3.OrganizationalUnit, A3.SalaryCurrency, A3.CostCenter, A3.ProfitCenter

                                UNION ALL

                                SELECT 
	                                	A4.DeptName Reference,
	                                    A3.OrganizationalUnit CompCode,
	                                    A3.SalaryCurrency Currency,
	                                    'AcctType' = CASE WHEN A2.LineType = 'Loan Recovery' OR A2.LineType = 'Advance Recovery'
	                                    THEN 'K' ELSE 'S' END,
	                                    'AcctCode' = CASE  WHEN A2.LineType = 'Loan Recovery' OR A2.LineType = 'Advance Recovery'
	                                    THEN A3.EmpID ELSE 	A2.CreditAccount END ,
	                                    'SpecialGLIndicator' = CASE WHEN A2.LineType = 'Loan Recovery' OR A2.LineType = 'Advance Recovery'
	                                    THEN ISNULL(A2.A1Indicators ,'') ELSE '' END ,
	                                    A2.CreditAccountName AcctName,
	                                    'C' GLAcctType,
	                                    SUM(ABS(A2.LineValue)) AS Amount,
	                                    ISNULL(A3.CostCenter,'') CostCenter,
	                                    A3.ProfitCenter
                                FROM 
	                                dbo.TrnsSalaryProcessRegister A1 INNER JOIN dbo.TrnsSalaryProcessRegisterDetail A2 ON A1.Id = A2.SRID
	                                INNER JOIN dbo.MstEmployee A3 ON A1.EmpID = A3.ID
	                                INNER JOIN dbo.MstDepartment A4 ON A3.DepartmentID = A4.ID
                                WHERE
	                                A1.Id IN (" + searchKeyVal["spIds"].ToString() + @")
                                GROUP BY A2.CreditAccount, A4.DeptName, A2.CreditAccountName, A3.OrganizationalUnit, A3.SalaryCurrency, A3.CostCenter, A3.ProfitCenter, A2.LineType, A3.EmpID, A2.A1Indicators

                                ORDER BY Reference, GLAcctType DESC
                            ";
                    strCondition = "";
                    break;
                case "JEQueryPP":
                    strSql = @"
                              SELECT 
	                                A1.DebitAccount AS AcctCode, A1.DebitName AS AcctName, ISNULL(SUM(ISNULL(NetPayable,0)),0) AS Debit, 0 AS Credit
                                FROM 
	                                dbo.TrnsEmployeePerPieceProcessing A1 
	                                INNER JOIN dbo.MstEmployee A3 ON A1.EmpID = A3.ID
                                WHERE
	                                A1.internalID IN (" + searchKeyVal["spIds"].ToString() + @") AND ISNULL(A1.flgPosted,0) = 0 
                                GROUP BY A1.DebitAccount, A1.DebitName

                                UNION ALL

                                SELECT 
	                                A1.CreditAccount AS AcctCode, A1.CreditName AS AcctName, 0 AS Debit,   ISNULL(SUM(ISNULL(NetPayable,0)),0) AS Credit	
                                FROM 
	                                dbo.TrnsEmployeePerPieceProcessing A1 
	                                INNER JOIN dbo.MstEmployee A3 ON A1.EmpID = A3.ID
                                WHERE
	                                A1.internalID IN (" + searchKeyVal["spIds"].ToString() + @") AND ISNULL(A1.flgPosted,0) = 0 
                                GROUP BY A1.CreditAccount, A1.CreditName
                                ORDER BY Credit
                              ";
                    strCondition = "";
                    break;
                case "JEQueryDimension":
                    strSql = @"
                                SELECT   dbo.TrnsSalaryProcessRegisterDetail.DebitAccount AcctCode,  dbo.TrnsSalaryProcessRegisterDetail.DebitAccountName AcctName, SUM(ABS(dbo.TrnsSalaryProcessRegisterDetail.LineValue)) AS Debit, 0 AS Credit , ISNULL(Dimension1,'') AS Dimension1,ISNULL(Dimension2,'') AS Dimension2,ISNULL(Dimension3,'') AS Dimension3,ISNULL(Dimension4,'') AS Dimension4,ISNULL(Dimension5,'') AS Dimension5, ISNULL(SalaryCurrency,'') AS EmpCurr
                                FROM    dbo.TrnsSalaryProcessRegister INNER JOIN
                                dbo.TrnsSalaryProcessRegisterDetail ON dbo.TrnsSalaryProcessRegister.Id = dbo.TrnsSalaryProcessRegisterDetail.SRID inner join 
                                MstEmployee on MstEmployee.ID = TrnsSalaryProcessRegister.EmpID
                                Where TrnsSalaryProcessRegister.ID in (" + searchKeyVal["spIds"].ToString() + @")
                                GROUP BY dbo.TrnsSalaryProcessRegisterDetail.DebitAccount,dbo.TrnsSalaryProcessRegisterDetail.DebitAccountName,Dimension1, Dimension2, Dimension3, Dimension4, Dimension5, SalaryCurrency

                                union

                                SELECT  dbo.TrnsSalaryProcessRegisterDetail.creditAccount AcctCode, dbo.TrnsSalaryProcessRegisterDetail.creditAccountName AcctName, 0 as Debit, SUM(ABS(dbo.TrnsSalaryProcessRegisterDetail.LineValue)) AS Credit , ISNULL(Dimension1,'') AS Dimension1,ISNULL(Dimension2,'') AS Dimension2,ISNULL(Dimension3,'') AS Dimension3,ISNULL(Dimension4,'') AS Dimension4,ISNULL(Dimension5,'') AS Dimension5, ISNULL(SalaryCurrency,'') AS EmpCurr
                                FROM    dbo.TrnsSalaryProcessRegister INNER JOIN
                                dbo.TrnsSalaryProcessRegisterDetail ON dbo.TrnsSalaryProcessRegister.Id = dbo.TrnsSalaryProcessRegisterDetail.SRID inner join 
                                MstEmployee on MstEmployee.ID = TrnsSalaryProcessRegister.EmpID
                                Where TrnsSalaryProcessRegister.ID in (" + searchKeyVal["spIds"].ToString() + @")
                                GROUP BY dbo.TrnsSalaryProcessRegisterDetail.creditAccount,dbo.TrnsSalaryProcessRegisterDetail.creditAccountName,Dimension1, Dimension2, Dimension3, Dimension4, Dimension5, SalaryCurrency
                            
                                ORDER BY Credit
                              ";
                    strCondition = "";
                    break;
                //JEBranches
                case "JEBranches":
                    strSql = @"SELECT dbo.TrnsSalaryProcessRegisterDetail.DebitAccount AcctCode,  dbo.TrnsSalaryProcessRegisterDetail.DebitAccountName AcctName, SUM(ABS(dbo.TrnsSalaryProcessRegisterDetail.LineValue)) AS Debit, 0 AS Credit ,CostCenter,EmpBranch As BranchName, ISNULL(SalaryCurrency,'') AS EmpCurr 
                                FROM  dbo.TrnsSalaryProcessRegister INNER JOIN
                                dbo.TrnsSalaryProcessRegisterDetail ON dbo.TrnsSalaryProcessRegister.Id = dbo.TrnsSalaryProcessRegisterDetail.SRID inner join 
                                MstEmployee on MstEmployee.ID = TrnsSalaryProcessRegister.EmpID
                                Where TrnsSalaryProcessRegister.ID in (" + searchKeyVal["spIds"].ToString() + @")
                                GROUP BY dbo.TrnsSalaryProcessRegisterDetail.DebitAccount,dbo.TrnsSalaryProcessRegisterDetail.DebitAccountName,costcenter,EmpBranch , SalaryCurrency

                                union

                                SELECT dbo.TrnsSalaryProcessRegisterDetail.creditAccount AcctCode, dbo.TrnsSalaryProcessRegisterDetail.creditAccountName AcctName, 0 as Debit, SUM(ABS(dbo.TrnsSalaryProcessRegisterDetail.LineValue)) AS Credit ,CostCenter,EmpBranch As BranchName, ISNULL(SalaryCurrency,'') AS EmpCurr
                                FROM dbo.TrnsSalaryProcessRegister INNER JOIN
                                dbo.TrnsSalaryProcessRegisterDetail ON dbo.TrnsSalaryProcessRegister.Id = dbo.TrnsSalaryProcessRegisterDetail.SRID inner join 
                                MstEmployee on MstEmployee.ID = TrnsSalaryProcessRegister.EmpID
                                Where TrnsSalaryProcessRegister.ID in (" + searchKeyVal["spIds"].ToString() + @")
                                GROUP BY dbo.TrnsSalaryProcessRegisterDetail.creditAccount,dbo.TrnsSalaryProcessRegisterDetail.creditAccountName,costcenter,EmpBranch , SalaryCurrency

                                order by CostCenter , credit";

                    strCondition = "";
                    break;
                //==========
                case "FSJEQuery":
                    strSql = @"
                                
                            SELECT 
                                FS3.DebitAccount AS AcctCode, FS3.DebitAccountName AS AcctName, SUM(ABS(ISNULL(FS3.LineValue,0))) AS Debit, 0 AS Credit,
                                ISNULL(FS4.CostCenter,'') AS CostCenter, ISNULL(FS4.BranchName,'') AS Branches, ISNULL(FS4.Project,'') AS Project
                            FROM 
                                dbo.TrnsFSHead AS FS1 INNER JOIN dbo.TrnsFinalSettelmentRegister AS FS2 ON FS1.ID = FS2.FSHeadID
                                INNER JOIN dbo.TrnsFinalSettelmentRegisterDetail AS FS3 ON FS2.Id = FS3.FSID
                                INNER JOIN dbo.MstEmployee AS FS4 ON FS1.internalEmpID = FS4.ID
                            WHERE FS1.ID = '" + searchKeyVal["FSID"].ToString() + @"'
                            GROUP BY FS3.DebitAccount, FS3.DebitAccountName, FS4.CostCenter, FS4.BranchName, FS4.Project

                                UNION ALL

                            SELECT 
	                            FS3.CreditAccount AS AcctCode, FS3.CreditAccountName AS AcctName, 0 AS Debit, SUM(ABS(ISNULL(FS3.LineValue,0))) AS Credit,
                                ISNULL(FS4.CostCenter,'') AS CostCenter, ISNULL(FS4.BranchName,'') AS Branches, ISNULL(FS4.Project,'') AS Project
                            FROM 
	                            dbo.TrnsFSHead AS FS1 INNER JOIN dbo.TrnsFinalSettelmentRegister AS FS2 ON FS1.ID = FS2.FSHeadID
	                            INNER JOIN dbo.TrnsFinalSettelmentRegisterDetail AS FS3 ON FS2.Id = FS3.FSID
	                            INNER JOIN dbo.MstEmployee AS FS4 ON FS1.internalEmpID = FS4.ID
                            WHERE FS1.ID = '" + searchKeyVal["FSID"].ToString() + @"'
                            GROUP BY FS3.CreditAccount,FS3.CreditAccountName,  FS4.CostCenter, FS4.BranchName, FS4.Project
                                ";
                    strCondition = "";
                    break;
                case "FSJEQueryATC":
                    strSql = @"
                                WITH mfm AS 
                                (
	                                SELECT 
		                                FS3.DebitAccount AS AcctCode, FS3.DebitAccountName AS AcctName, SUM(ABS(ISNULL(FS3.LineValue,0))) AS Debit, 0 AS Credit, ISNULL(FS4.CostCenter,'') AS CostCenter 
	                                FROM 
		                                dbo.TrnsFSHead AS FS1 INNER JOIN dbo.TrnsFinalSettelmentRegister AS FS2 ON FS1.ID = FS2.FSHeadID
		                                INNER JOIN dbo.TrnsFinalSettelmentRegisterDetail AS FS3 ON FS2.Id = FS3.FSID
		                                INNER JOIN dbo.MstEmployee AS FS4 ON FS1.internalEmpID = FS4.ID
	                                WHERE FS4.EmpID = '" + searchKeyVal["FSEmpID"].ToString() + @"' AND FS3.LineType IN ('BS','Element','FSAdjustment') AND FS3.LineValue > 0
	                                GROUP BY FS3.DebitAccount, FS3.DebitAccountName, FS4.CostCenter

	                                UNION ALL

	                                SELECT 
		                                FS3.CreditAccount AS AcctCode, FS3.CreditAccountName AS AcctName, 0 AS Debit, SUM(ABS(ISNULL(FS3.LineValue,0))) AS Credit, ISNULL(FS4.CostCenter,'') AS CostCenter
	                                FROM 
		                                dbo.TrnsFSHead AS FS1 INNER JOIN dbo.TrnsFinalSettelmentRegister AS FS2 ON FS1.ID = FS2.FSHeadID
		                                INNER JOIN dbo.TrnsFinalSettelmentRegisterDetail AS FS3 ON FS2.Id = FS3.FSID
		                                INNER JOIN dbo.MstEmployee AS FS4 ON FS1.internalEmpID = FS4.ID
	                                WHERE FS4.EmpID = '" + searchKeyVal["FSEmpID"].ToString() + @"' AND FS3.LineType NOT IN ('PFEmployeeOB', 'PFEmployerOB', 'FSContribution' )
	                                GROUP BY FS3.CreditAccount, FS3.CreditAccountName, FS4.CostCenter

	                                UNION ALL

	                                SELECT 
		                                FS3.DebitAccount AS AcctCode, FS3.DebitAccountName AS AcctName, 0 AS Debit, SUM(ISNULL(FS3.LineValue,0)) AS Credit, ISNULL(FS4.CostCenter,'') AS CostCenter
	                                FROM 
		                                dbo.TrnsFSHead AS FS1 INNER JOIN dbo.TrnsFinalSettelmentRegister AS FS2 ON FS1.ID = FS2.FSHeadID
		                                INNER JOIN dbo.TrnsFinalSettelmentRegisterDetail AS FS3 ON FS2.Id = FS3.FSID
		                                INNER JOIN dbo.MstEmployee AS FS4 ON FS1.internalEmpID = FS4.ID
	                                WHERE FS4.EmpID = '" + searchKeyVal["FSEmpID"].ToString() + @"' AND FS3.LineSubType IN ('PF', 'AdvanceRecovery','Loan RECOVERY','Tax') AND FS3.LineType IN ('Element','FSAdvanceRecovery', 'FSLoanRecovery','Tax')
	                                GROUP BY FS3.DebitAccount, FS3.DebitAccountName, FS4.CostCenter
                                    
                                )
                                SELECT AcctCode, AcctName, SUM(Debit) AS Debit, SUM(Credit) AS Credit, CostCenter 
                                FROM mfm
                                GROUP BY AcctCode, AcctName, CostCenter
                                ";
                    strCondition = "";
                    break;
                case "FSUAEJEQuery":
                    strSql = @"
                                DECLARE	@JETable TABLE
                                (
	                                AcctCode NVARCHAR(100),
	                                AcctName NVARCHAR(200),
	                                Debit NUMERIC(19,6),
	                                Credit NUMERIC(19,6),
	                                CostCenter NVARCHAR(100)
                                )
                                INSERT INTO @JETable
                                SELECT 
	                                FS3.DebitAccount AS AcctCode, FS3.DebitAccountName AS AcctName, SUM(ABS(ISNULL(FS3.LineValue,0))) AS Debit, 0 AS Credit, ISNULL(FS4.CostCenter,'') AS CostCenter
                                FROM 
	                                dbo.TrnsFSHead AS FS1 INNER JOIN dbo.TrnsFinalSettelmentRegister AS FS2 ON FS1.ID = FS2.FSHeadID
	                                INNER JOIN dbo.TrnsFinalSettelmentRegisterDetail AS FS3 ON FS2.Id = FS3.FSID
	                                INNER JOIN dbo.MstEmployee AS FS4 ON FS1.internalEmpID = FS4.ID
                                WHERE FS4.EmpID = '" + searchKeyVal["FSEmpID"].ToString() + @"' AND FS3.LineType <> 'FSGratuity'
                                GROUP BY FS3.DebitAccount, FS3.DebitAccountName, FS4.CostCenter
                                UNION ALL	
                                SELECT	
	                                A2.DebitAccount AS AcctCode, A2.DebitAccountName AS AcctName, ROUND(SUM(ABS(ISNULL(A2.LineValue,0))),0) AS Debit, 0 AS Credit, ISNULL(A3.CostCenter,'') AS CostCenter
                                FROM 
	                                dbo.TrnsSalaryProcessRegister A1
	                                INNER JOIN dbo.TrnsSalaryProcessRegisterDetail A2 ON A2.SRID = A1.Id
	                                INNER JOIN dbo.MstEmployee A3 ON A1.EmpID = A3.ID
                                WHERE 
	                                A3.EmpID = '" + searchKeyVal["FSEmpID"].ToString() + @"'
	                                AND A2.LineSubType = 'Gratuity Provision'
                                GROUP BY A2.DebitAccount, A2.DebitAccountName, A3.CostCenter
                                UNION ALL
                                SELECT 
	                                FS3.CreditAccount AS AcctCode, FS3.CreditAccountName AS AcctName, 0 AS Debit, SUM(ABS(ISNULL(FS3.LineValue,0))) AS Credit, ISNULL(FS4.CostCenter,'') AS CostCenter
                                FROM 
	                                dbo.TrnsFSHead AS FS1 INNER JOIN dbo.TrnsFinalSettelmentRegister AS FS2 ON FS1.ID = FS2.FSHeadID
	                                INNER JOIN dbo.TrnsFinalSettelmentRegisterDetail AS FS3 ON FS2.Id = FS3.FSID
	                                INNER JOIN dbo.MstEmployee AS FS4 ON FS1.internalEmpID = FS4.ID
                                WHERE FS4.EmpID = '" + searchKeyVal["FSEmpID"].ToString() + @"'
                                GROUP BY FS3.DebitAccount, FS3.DebitAccountName,FS3.CreditAccount,FS3.CreditAccountName,  FS4.CostCenter
                                DECLARE @Credit NUMERIC(19,6), @Debit NUMERIC(19,6)
                                DECLARE @AcctCode NVARCHAR(100), @AcctName NVARCHAR(200), @CostCenter NVARCHAR(200)
                                SELECT 
	                                @Credit = ROUND(SUM(ABS(ISNULL(FS3.LineValue,0))),0)
                                FROM 
	                                dbo.TrnsFSHead AS FS1 INNER JOIN dbo.TrnsFinalSettelmentRegister AS FS2 ON FS1.ID = FS2.FSHeadID
	                                INNER JOIN dbo.TrnsFinalSettelmentRegisterDetail AS FS3 ON FS2.Id = FS3.FSID
	                                INNER JOIN dbo.MstEmployee AS FS4 ON FS1.internalEmpID = FS4.ID
                                WHERE FS4.EmpID = '" + searchKeyVal["FSEmpID"].ToString() + @"' AND FS3.LineType = 'FSGratuity'
                                GROUP BY FS3.DebitAccount, FS3.DebitAccountName,FS3.CreditAccount,FS3.CreditAccountName,  FS4.CostCenter
                                SELECT	
	                                @AcctCode = A2.DebitAccount , @AcctName = A2.DebitAccountName, @Debit = ROUND(SUM(ABS(ISNULL(A2.LineValue,0))),0), @CostCenter= ISNULL(A3.CostCenter,'')
                                FROM 
	                                dbo.TrnsSalaryProcessRegister A1
	                                INNER JOIN dbo.TrnsSalaryProcessRegisterDetail A2 ON A2.SRID = A1.Id
	                                INNER JOIN dbo.MstEmployee A3 ON A1.EmpID = A3.ID
                                WHERE 
	                                A3.EmpID = '" + searchKeyVal["FSEmpID"].ToString() + @"'
	                                AND A2.LineSubType = 'Gratuity Provision'
                                GROUP BY A2.DebitAccount, A2.DebitAccountName, A3.CostCenter
                                IF @Debit > 0 AND @Credit > 0
                                BEGIN
	                                INSERT	INTO @JETable
	                                SELECT @AcctCode, @AcctName, ABS(@Debit - @Credit) AS Debit, 0 AS Credit, @CostCenter
                                END
                                SELECT * FROM @JETable	
";
                    strCondition = "";
                    break;
                case "JEQueryCC":
                    strSql = @"
                                SELECT 
	                                A1.DebitAccountCode AcctCode,
	                                A1.DebitAccountName AcctName,
	                                SUM(ABS(A1.NewLineValue)) Debit,
	                                0 Credit,
	                                A1.CostCenter
                                FROM 
	                                dbo.TrnsJECCRegister AS A1
                                WHERE A1.SalaryID IN (" + searchKeyVal["spIds"].ToString() + @")
                                GROUP BY A1.DebitAccountCode, A1.DebitAccountName, A1.CostCenter

                                UNION

                                SELECT 
	                                A1.CreditAccountCode AcctCode,
	                                A1.CreditAccountName AcctName,
	                                0 Debit,
	                                SUM(ABS(A1.NewLineValue)) Credit,
	                                A1.CostCenter
                                FROM 
	                                dbo.TrnsJECCRegister AS A1
                                WHERE A1.SalaryID IN (" + searchKeyVal["spIds"].ToString() + @")
                                GROUP BY A1.CreditAccountCode, A1.CreditAccountName, A1.CostCenter
                              ";
                    strCondition = "";
                    break;
                case "MstCandidate":
                    strSql = @"
                                SELECT     
	                                dbo.MstCandidate.ID, dbo.MstCandidate.CandidateNo, dbo.MstCandidate.FirstName, dbo.MstCandidate.MiddleName, dbo.MstCandidate.LastName, 
	                                dbo.MstBranches.Name AS Branch, dbo.MstPosition.Name AS Position, dbo.MstLocation.Name AS Location, dbo.MstDepartment.Code AS Department, 
	                                dbo.MstCandidate.ValidFrom, dbo.MstCandidate.ValidTo
                                FROM     
	                                dbo.MstCandidate LEFT OUTER JOIN
	                                dbo.MstPosition ON dbo.MstCandidate.Position = dbo.MstPosition.Id LEFT OUTER JOIN
	                                dbo.MstDepartment ON dbo.MstCandidate.Department = dbo.MstDepartment.ID LEFT OUTER JOIN
	                                dbo.MstLocation ON dbo.MstCandidate.Location = dbo.MstLocation.Id LEFT OUTER JOIN
	                                dbo.MstBranches ON dbo.MstCandidate.Branch = dbo.MstBranches.Id
                                WHERE
	                                CandidateNo LIKE '" + searchKeyVal["CandidateNo"].ToString() + @"' AND FirstName LIKE '" + searchKeyVal["FirstName"].ToString() + @"' AND Isnull(MiddleName,'') LIKE '" + searchKeyVal["MiddleName"].ToString() + @"' AND Isnull(LastName,'') LIKE '" + searchKeyVal["LastName"].ToString() + @"' AND
	                                Isnull(DeptName,'') LIKE '" + searchKeyVal["Department"].ToString() + @"' AND Isnull(dbo.MstBranches.Name,'') LIKE '" + searchKeyVal["Branch"].ToString() + @"' AND Isnull(dbo.MstPosition.Name,'') LIKE '" + searchKeyVal["Position"].ToString() + @"' AND
	                                Isnull(dbo.MstLocation.Name,'') LIKE '" + searchKeyVal["Location"].ToString() + @"'
                                ";
                    strCondition = "";
                    break;
                case "FinalSettlement":
                    strSql = @"
                            SELECT one.Id, one.DocNum, ISNULL(two.FirstName,'') +' '+ ISNULL(two.MiddleName,'') +' '+ ISNULL(two.LastName,'') AS [Employee Name], one.DateOfJoining, one.ResignDate
                            FROM dbo.TrnsResignation AS one INNER JOIN dbo.MstEmployee AS two ON one.EmpID = two.ID
	                        LEFT OUTER JOIN dbo.TrnsFinalSettelmentRegister AS three ON two.ID = three.EmpID
                            WHERE one.DocStatus = 'LV0002' AND ISNULL(three.FSStatus,0) = 0 
                              ";
                    strCondition = "";
                    break;
                case "FinalSettlementAppr":
                    strSql = @"
                            SELECT FS.Id AS DocNum, Emp.EmpID AS EmployeeID ,  ISNULL(Emp.FirstName,'') AS FirstName, ISNULL(Emp.MiddleName,'') AS MiddleName, ISNULL(Emp.LastName,'') AS LastName
                            FROM dbo.TrnsFinalSettelmentRegister AS FS INNER JOIN dbo.MstEmployee AS Emp ON FS.EmpID = Emp.ID
                            WHERE ISNULL(FS.FSStatus,0) = 0
                              ";
                    strCondition = "";
                    break;
                case "InterviewCall":
                    strSql = @"
                                SELECT     
	                                dbo.TrnsInterviewCall.ID, dbo.TrnsInterviewCall.DocNum, dbo.MstCandidate.CandidateNo, dbo.TrnsInterviewCall.ScheduleDate
                                FROM
	                                dbo.TrnsInterviewCall INNER JOIN
	                                dbo.MstCandidate ON dbo.TrnsInterviewCall.CandidateID = dbo.MstCandidate.ID LEFT OUTER JOIN
	                                dbo.MstBranches ON dbo.MstCandidate.Branch = dbo.MstBranches.Id LEFT OUTER JOIN
	                                dbo.MstDesignation ON dbo.MstCandidate.Designation = dbo.MstDesignation.Id LEFT OUTER JOIN
	                                dbo.MstDepartment ON dbo.MstCandidate.Department = dbo.MstDepartment.ID
                                WHERE     
	                                CONVERT(VARCHAR(50), dbo.TrnsInterviewCall.DocNum) LIKE '" + searchKeyVal["DocNum"].ToString() + @"' AND 
	                                CONVERT(VARCHAR(50), dbo.MstCandidate.CandidateNo) LIKE '" + searchKeyVal["CanNo"].ToString() + @"' AND
	                                CONVERT(DATE, dbo.TrnsInterviewCall.ScheduleDate, 101) BETWEEN CONVERT(DATE, '" + searchKeyVal["dtFrom"].ToString() + @"',101) AND CONVERT(DATE, '" + searchKeyVal["dtTo"].ToString() + @"',101) AND
                                    CONVERT(VARCHAR(50), dbo.MstDepartment.Code) LIKE '" + searchKeyVal["DeptName"].ToString() + @"%' AND
	                                CONVERT(VARCHAR(50), dbo.MstDesignation.Name) LIKE '" + searchKeyVal["DesigName"].ToString() + @"%' AND
	                                CONVERT(VARBINARY(50), dbo.MstBranches.Name) LIKE '" + searchKeyVal["BranchName"].ToString() + @"%'
                               ";
                    strCondition = "";
                    break;
                case "InterviewEvaluation":
                    strSql = @"
                                SELECT     
	                                dbo.TrnsInterviewEAS.ID, dbo.TrnsInterviewEAS.DocNum, dbo.TrnsInterviewCall.DocNum AS IntCallDocNum, dbo.TrnsInterviewCall.ScheduleDate, 
	                                dbo.MstCandidate.CandidateNo, dbo.TrnsInterviewEAS.DocStatus
                                FROM   
	                                dbo.TrnsInterviewEAS INNER JOIN
	                                dbo.TrnsInterviewCall ON dbo.TrnsInterviewEAS.InterviewID = dbo.TrnsInterviewCall.ID INNER JOIN
	                                dbo.MstCandidate ON dbo.TrnsInterviewCall.CandidateID = dbo.MstCandidate.ID
                                WHERE    
	                                CONVERT(VARCHAR(50),dbo.TrnsInterviewEAS.DocStatus) LIKE '" + searchKeyVal["DocStatus"].ToString() + @"%' AND 
	                                CONVERT(VARCHAR(50),dbo.MstCandidate.CandidateNo) LIKE '" + searchKeyVal["CanNo"].ToString() + @"' AND 
	                                CONVERT(VARCHAR(50), dbo.TrnsInterviewCall.DocNum) LIKE '" + searchKeyVal["DocNum"].ToString() + @"' AND 
	                                CONVERT(VARCHAR(50), dbo.TrnsInterviewEAS.DocNum) LIKE '" + searchKeyVal["IntEAS"].ToString() + @"' AND
	                                CONVERT(DATE, dbo.TrnsInterviewCall.ScheduleDate,101) BETWEEN CONVERT(DATE,'" + searchKeyVal["dtFrom"].ToString() + @"',101) AND CONVERT(DATE, '" + searchKeyVal["dtTo"].ToString() + @"',101)
                              ";
                    strCondition = "";
                    break;
                case "TaxSlab":
                    strSql = @"
                                SELECT 
                                   A1.ID ,A1.SalaryYear AS SalaryID,A2.Code,A2.Description
                                FROM dbo.CfgTaxSetup A1 INNER JOIN dbo.MstCalendar A2 
                                    ON A1.SalaryYear = A2.Id";
                    strCondition = "";
                    break;
            }

            //setting the default condition
            strSql += strCondition;

            return strSql;
        }

        public string getImportSql(string sqlKey)
        {

            string strsql = "";

            switch (sqlKey)
            {
                case "Department":
                    strsql = "Select [Name] as [Code],Remarks as [DeptName] from " + dbName + ".dbo.oudp ";
                    break;
            }

            return strsql;

        }

    }
}
