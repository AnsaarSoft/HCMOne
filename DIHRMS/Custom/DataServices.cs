using System;
using System.Globalization;

using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UFFU;

namespace DIHRMS.Custom
{
    public class DataServices : LOV
    {
        dbHRMS oDB = null;
        string hrmsDbname = "";
        string userId = "";
        mFm oLogger = null;

        public DataServices(dbHRMS pDB, string pDbName, string UserId, mFm log) : base(pDB)
        {
            // oDB = new dbHRMS( pDB .Connection);
            oDB = pDB;
            hrmsDbname = pDbName;
            userId = UserId;
            oLogger = log;
        }

        public void logger(Exception pEx)
        {
            try
            {
                oLogger.LogException(oDB.AppVersion, pEx);
            }
            catch
            {
            }
        }

        public void logger(string msg)
        {
            try
            {
                oLogger.LogEntry(oDB.AppVersion, msg);
            }
            catch
            {
            }
        }

        public decimal getDaysCnt(MstEmployee emp, CfgPeriodDates per, out decimal payDays, out decimal leaveDays, out decimal monthDays, out decimal varDayCount)
        {
            decimal outResult = 0.00M;

            monthDays = (decimal)emp.CfgPayrollDefination.WorkDays;
            if (monthDays == 0.00M)
            {
                monthDays = (Convert.ToDateTime(per.EndDate) - Convert.ToDateTime(per.StartDate)).Days;
                monthDays = monthDays + 1;
            }
            leaveDays = 0.00M;
            payDays = 0.00M;
            varDayCount = 0.0M;
            decimal lateJoiningDays = 0.00M;
            decimal earlyTermDays = 0.00M;
            decimal vlDays = 0.00M;
            if (emp.JoiningDate != null && emp.JoiningDate >= per.StartDate && emp.JoiningDate <= per.EndDate)
            {
                lateJoiningDays = (Convert.ToDateTime(emp.JoiningDate) - Convert.ToDateTime(per.StartDate)).Days;
            }

            if (emp.TerminationDate != null && emp.TerminationDate >= per.StartDate && emp.TerminationDate <= per.EndDate)
            {
                earlyTermDays = (Convert.ToDateTime(per.EndDate) - Convert.ToDateTime(emp.TerminationDate)).Days;
                //earlyTermDays = 0;
            }

            string strSql = @"
                                SELECT 
	                                A1.ID,
	                                ISNULL(A1.TotalCount,0) TotalCount,
	                                ISNULL(A1.DeductAmnt,0) DeductAmnt,
	                                A1.LeaveFrom, A1.LeaveTo,
	                                ISNULL(A2.flgVariableValue,0) flgVariableValue
                                FROM 
	                                dbo.TrnsLeavesRequest A1
	                                INNER JOIN dbo.MstLeaveType A2 ON A1.LeaveType = A2.ID
                                WHERE 
	                                A1.DocStatus = 'LV0002'
	                                AND A1.EmpID = '" + emp.ID + @"'
	                                AND ((A1.LeaveFrom BETWEEN '" + per.StartDate.ToString() + @"' AND '" + per.EndDate.ToString() + @"') OR (A1.LeaveTo BETWEEN '" + per.StartDate.ToString() + @"' AND '" + per.EndDate.ToString() + @"'))
                                ";

            DataTable dtLeaves = getDataTable(strSql);
            foreach (DataRow dr in dtLeaves.Rows)
            {
                Boolean flgVariableValue = false;
                DateTime fromDt = Convert.ToDateTime(dr["LeaveFrom"].ToString());
                DateTime toDt = Convert.ToDateTime(dr["LeaveTo"].ToString());
                decimal LeaveCnt = Convert.ToDecimal(dr["TotalCount"]);
                decimal deductedAmount = Convert.ToDecimal(dr["DeductAmnt"]);
                if (emp.ResignDate == null)
                {
                    flgVariableValue = Convert.ToBoolean(dr["flgVariableValue"]);
                }
                if (deductedAmount > 0)
                {
                    decimal leaveCount = 0.00M, vvdc = 0.0M;
                    if (fromDt >= per.StartDate && toDt <= per.EndDate)
                    {
                        leaveCount = (decimal)LeaveCnt;
                        if (flgVariableValue)
                            vvdc = leaveCount;
                    }
                    if (fromDt >= per.StartDate && toDt > per.EndDate)
                    {
                        leaveCount = (decimal)(Convert.ToDateTime(per.EndDate) - Convert.ToDateTime(fromDt)).Days + 1;
                        if (flgVariableValue)
                            vvdc = leaveCount;
                    }
                    if (fromDt < per.StartDate && toDt <= per.EndDate)
                    {
                        leaveCount = (decimal)(Convert.ToDateTime(toDt) - Convert.ToDateTime(per.StartDate)).Days + 1;
                        if (flgVariableValue)
                            vvdc = leaveCount;
                    }
                    if (fromDt < per.StartDate && toDt > per.EndDate)
                    {
                        leaveCount = monthDays;
                        if (flgVariableValue)
                            vvdc = leaveCount;
                    }
                    leaveDays += leaveCount;
                    varDayCount += vvdc;
                }

                //if(leavetype is VL)
                //{
                //  vlDays+= leaveCount;
                //}                
            }
            payDays = monthDays - lateJoiningDays - earlyTermDays - vlDays;
            outResult = monthDays - leaveDays - lateJoiningDays - earlyTermDays;
            varDayCount = monthDays - varDayCount;
            return outResult;
        }

        public decimal getEmpGross(MstEmployee emp, int pType = 1, int pRegular = 0, decimal percent = 1)
        {

            decimal outDecimal = 0.00M;
            var PayrollDefination = (from a in oDB.CfgPayrollBasicInitialization select a).FirstOrDefault();

            string CompanyName = string.IsNullOrEmpty(PayrollDefination.CompanyName) ? "" : PayrollDefination.CompanyName.Trim();
            if (!string.IsNullOrEmpty(emp.EmployeeContractType) && emp.EmployeeContractType != "DWGS")
            {
                outDecimal = (decimal)emp.BasicSalary.GetValueOrDefault();
            }
            else
            {

                if (CompanyName.ToLower() != "emco")
                {
                    var GetCurrentMonthsPeriod = oDB.CfgPeriodDates.Where(e => e.PayrollId == emp.PayrollID).FirstOrDefault();
                    if (GetCurrentMonthsPeriod != null)
                    {
                        DateTime startDate = GetCurrentMonthsPeriod.StartDate.Value;
                        DateTime EndDate = GetCurrentMonthsPeriod.EndDate.Value;
                        int CountTotalPeriodDays = (EndDate - startDate).Days + 1;
                        if (CountTotalPeriodDays > 0)
                        {
                            outDecimal = Convert.ToDecimal(emp.BasicSalary.GetValueOrDefault()) * CountTotalPeriodDays;
                        }
                    }
                }
                else
                {
                    outDecimal = (decimal)emp.BasicSalary.GetValueOrDefault();
                }

            }
            //IEnumerable<TrnsEmployeeElementDetail> empGrossElements = from p in oDB.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId.ToString() == emp.ID.ToString() && p.MstElements.FlgEffectOnGross == true && p.FlgActive == true select p;
            var empGrossElements = (from a in oDB.TrnsEmployeeElementDetail
                                    where a.TrnsEmployeeElement.EmployeeId == emp.ID
                                        && (a.MstElements.FlgEffectOnGross != null ? a.MstElements.FlgEffectOnGross : false) == true
                                        && (a.FlgActive != null ? a.FlgActive : false) == true
                                    select a).ToList();
            try
            {
                foreach (TrnsEmployeeElementDetail ele in empGrossElements)
                {
                    if (ele.MstElements.ElmtType.Trim() == "Ear")
                    {
                        Boolean? flgPayrollActive = false;
                        flgPayrollActive = (from a in oDB.MstElementLink where a.PayrollID == emp.PayrollID && a.MstElements.Id == ele.MstElements.Id select a.FlgActive).FirstOrDefault();
                        if ((flgPayrollActive != null ? Convert.ToBoolean(flgPayrollActive) : false))
                        {
                            DIHRMS.Custom.clsElement elementinfo = new DIHRMS.Custom.clsElement(oDB, ele, emp, emp.GrossSalary == null ? (decimal)0 : (decimal)emp.GrossSalary, pType);

                            if (ele.MstElements.Type.Trim().ToUpper() == "REC")
                            {
                                if ((bool)!ele.MstElements.FlgGradeDep)
                                {
                                    percent = 1;
                                }
                                outDecimal += (decimal)elementinfo.Amount * percent;
                            }
                            else if (ele.MstElements.Type.Trim().ToUpper() == "NON-REC")
                            {
                                var GetCurrentMonthsPeriod = oDB.CfgPeriodDates.Where(e => e.PayrollId == emp.PayrollID && e.FlgLocked == false).FirstOrDefault();

                                if (GetCurrentMonthsPeriod.ID == ele.PeriodId)
                                {
                                    if ((bool)!ele.MstElements.FlgGradeDep)
                                    {
                                        percent = 1;
                                    }
                                    outDecimal += (decimal)elementinfo.Amount * percent;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { }
            return outDecimal;
        }

        public decimal getEmpGross(MstEmployee emp, int payrollperiodId)
        {
            decimal outDecimal = 0.00M;
            try
            {
                outDecimal = (decimal)emp.BasicSalary;

                if (!string.IsNullOrEmpty(emp.EmployeeContractType) && emp.EmployeeContractType == "DWGS")
                {

                    TrnsEmployeeWorkDays ProcessedDays = (from a in oDB.TrnsEmployeeWorkDays
                                                          where a.PayrollPeriodID == payrollperiodId
                                                          select a).FirstOrDefault();
                    if (ProcessedDays != null)
                    {
                        var Days = (from a in oDB.TrnsEmployeeWDDetails
                                    where a.EmployeeID == emp.ID
                                    && a.EmpWDId == ProcessedDays.Id
                                    select a).FirstOrDefault();
                        if (Days != null)
                        {
                            outDecimal = (decimal)(emp.BasicSalary * Days.WorkDays.GetValueOrDefault());
                        }
                    }
                }
                else
                {
                    outDecimal = (decimal)emp.BasicSalary;
                }
                IEnumerable<TrnsEmployeeElementDetail> empGrossElements = from p in oDB.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId.ToString() == emp.ID.ToString() && p.MstElements.FlgEffectOnGross == true && p.FlgActive == true select p;

                foreach (TrnsEmployeeElementDetail ele in empGrossElements)
                {
                    if (ele.MstElements.ElmtType.Trim() == "Ear" && Convert.ToBoolean(ele.MstElements.FlgEffectOnGross) && ele.FlgActive == true)
                    {
                        DIHRMS.Custom.clsElement elementinfo = new DIHRMS.Custom.clsElement(oDB, ele, emp);
                        if (ele.MstElements.Type.Trim().ToUpper() == "REC")
                        {
                            outDecimal += (decimal)elementinfo.Amount;
                        }
                        else if (ele.MstElements.Type.Trim().ToUpper() == "NON-REC")
                        {
                            if (payrollperiodId == ele.PeriodId)
                            {
                                outDecimal += (decimal)elementinfo.Amount;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                outDecimal = 0;
                string error = ex.Message;
            }
            return outDecimal;
        }

        public decimal getEmpGross(MstEmployee emp, string pVerifyElements)
        {
            decimal outDecimal = 0.00M;
            try
            {
                outDecimal = (decimal)emp.GrossSalary;
            }
            catch (Exception ex)
            {
                outDecimal = 0M;
            }
            return outDecimal;
        }

        public MstGLDetermination getEmpGl(MstEmployee emp)
        {
            MstGLDetermination detr = null;
            string GlType = emp.CfgPayrollDefination.GLType.ToString().Trim();

            try
            {

                if (GlType == "LOC")
                {
                    detr = (from p in oDB.MstGLDetermination where p.GLType == "LOC" && p.GLValue == emp.Location select p).FirstOrDefault();
                }
                else if (GlType == "DEPT")
                {
                    detr = (from p in oDB.MstGLDetermination where p.GLType == "DEPT" && p.GLValue == emp.DepartmentID select p).FirstOrDefault();
                }
                else if (GlType == "COMP")
                {
                    detr = (from p in oDB.MstGLDetermination where p.GLType == "COMP" select p).FirstOrDefault();
                }
                else
                {
                    detr = (from p in oDB.MstGLDetermination where p.GLType == "COMP" select p).FirstOrDefault();
                }

            }
            catch (Exception ex)
            {

            }
            return detr;
        }

        public Hashtable getElementGL(MstEmployee emp, MstElements ele, MstGLDetermination emGl)
        {
            MstGLDetermination glDetr = emGl;
            Hashtable gls = new Hashtable();
            string StrElementName = ele.ElementName;
            int GlId = glDetr.Id;
            int cntGl = 0;
            switch (ele.ElmtType.ToString().Trim())
            {
                case "Ear":
                    cntGl = (from p in oDB.MstGLDEarningDetail where p.GLDId.ToString() == GlId.ToString() && p.ElementId.ToString() == ele.Id.ToString() select p).Count();
                    if (cntGl > 0)
                    {
                        MstGLDEarningDetail glEar = (from p in oDB.MstGLDEarningDetail where p.GLDId.ToString() == GlId.ToString() && p.ElementId.ToString() == ele.Id.ToString() select p).FirstOrDefault();
                        gls.Add("DrAcct", glEar.CostAccout);
                        gls.Add("CrAcct", glEar.BalancingAccount);
                        gls.Add("DrAcctName", glEar.CostAcctDisplay);
                        gls.Add("CrAcctName", glEar.BalancingAcctDisplay);
                        gls.Add("LocationID", glEar.MstGLDetermination.GLValue);
                    }
                    else
                    {
                        gls.Add("DrAcct", "Not Found");
                        gls.Add("CrAcct", "Not Found");
                        gls.Add("DrAcctName", "Not Found");
                        gls.Add("CrAcctName", "Not Found");
                        gls.Add("LocationID", "Not Found");
                    }
                    break;
                case "Ded":
                    cntGl = (from p in oDB.MstGLDDeductionDetail where p.GLDId.ToString() == GlId.ToString() && p.DeductionId.ToString() == ele.Id.ToString() select p).Count();
                    if (cntGl > 0)
                    {
                        MstGLDDeductionDetail glDed = (from p in oDB.MstGLDDeductionDetail where p.GLDId.ToString() == GlId.ToString() && p.DeductionId.ToString() == ele.Id.ToString() select p).FirstOrDefault();
                        gls.Add("DrAcct", glDed.CostAccount);
                        gls.Add("CrAcct", glDed.BalancingAccount);
                        gls.Add("DrAcctName", glDed.CostAcctDisplay);
                        gls.Add("CrAcctName", glDed.BalancingAcctDisplay);
                    }
                    else
                    {
                        gls.Add("DrAcct", "Not Found");
                        gls.Add("CrAcct", "Not Found");
                        gls.Add("DrAcctName", "Not Found");
                        gls.Add("CrAcctName", "Not Found");
                    }
                    break;
                case "Con":
                    cntGl = (from p in oDB.MstGLDContribution where p.GLDId.ToString() == GlId.ToString() && p.ContributionId.ToString() == ele.Id.ToString() select p).Count();
                    if (cntGl > 0)
                    {
                        MstGLDContribution glCont = (from p in oDB.MstGLDContribution where p.GLDId.ToString() == GlId.ToString() && p.ContributionId.ToString() == ele.Id.ToString() select p).FirstOrDefault();
                        gls.Add("DrAcct", glCont.CostAccount);
                        gls.Add("CrAcct", glCont.BalancingAccount);
                        gls.Add("DrAcctName", glCont.CostAcctDisplay);
                        gls.Add("CrAcctName", glCont.BalancingAcctDisplay);
                        gls.Add("EmprDrAcct", glCont.EmprCostAccount);
                        gls.Add("EmprCrAcct", glCont.EmprBalancingAccount);
                        gls.Add("EmprDrAcctName", glCont.EmprCostAcctDisplay);
                        gls.Add("EmprCrAcctName", glCont.EmprBalancingAcctDisplay);
                    }
                    else
                    {
                        gls.Add("DrAcct", "Not Found");
                        gls.Add("CrAcct", "Not Found");
                        gls.Add("DrAcctName", "Not Found");
                        gls.Add("CrAcctName", "Not Found");
                        gls.Add("EmprDrAcct", "Not Found");
                        gls.Add("EmprCrAcct", "Not Found");
                        gls.Add("EmprDrAcctName", "Not Found");
                        gls.Add("EmprCrAcctName", "Not Found");
                    }
                    break;
            }

            return gls;

        }

        public Hashtable getLoanGL(MstEmployee emp, MstLoans loan)
        {
            MstGLDetermination glDetr = getEmpGl(emp);
            Hashtable gls = new Hashtable();
            int GlId = glDetr.Id;
            int cntGl = 0;

            cntGl = (from p in oDB.MstGLDLoansDetails where p.GLDId.ToString() == GlId.ToString() && p.LoanId.ToString() == loan.Id.ToString() select p).Count();
            if (cntGl > 0)
            {
                MstGLDLoansDetails glloan = (from p in oDB.MstGLDLoansDetails where p.GLDId.ToString() == GlId.ToString() && p.LoanId.ToString() == loan.Id.ToString() select p).FirstOrDefault();
                gls.Add("DrAcct", glloan.CostAccount);
                gls.Add("CrAcct", glloan.BalancingAccount);
                gls.Add("DrAcctName", glloan.CostAcctDisplay);
                gls.Add("CrAcctName", glloan.BalancingAcctDisplay);
                gls.Add("Indicator", string.IsNullOrEmpty(glloan.A1Indicator) == true ? "Not Found" : glloan.A1Indicator);
            }
            else
            {
                gls.Add("DrAcct", "Not Found");
                gls.Add("CrAcct", "Not Found");
                gls.Add("DrAcctName", "Not Found");
                gls.Add("CrAcctName", "Not Found");
                gls.Add("Indicator", "Not Found");
            }

            return gls;

        }

        public Hashtable getGraduityGL(MstEmployee emp)
        {
            MstGLDetermination glDetr = getEmpGl(emp);
            Hashtable gls = new Hashtable();
            int GlId = glDetr.Id;
            int cntGl = 0;

            cntGl = (from p in oDB.MstGLDetermination where p.GLType.ToString() == glDetr.GLType.ToString() select p).Count();

            if (cntGl > 0)
            {
                MstGLDetermination glOt = (from p in oDB.MstGLDetermination where p.GLType.ToString() == glDetr.GLType.ToString() select p).Single();
                gls.Add("DrAcct", glOt.GratuityExpense);
                gls.Add("CrAcct", glOt.GratuityPayable);
                gls.Add("DrAcctName", "");
                gls.Add("CrAcctName", "");
            }
            else
            {
                gls.Add("DrAcct", "Not Found");
                gls.Add("CrAcct", "Not Found");
                gls.Add("DrAcctName", "Not Found");
                gls.Add("CrAcctName", "Not Found");
            }

            return gls;

        }

        public Hashtable getOverTimeGL(MstEmployee emp, MstOverTime overtim)
        {
            MstGLDetermination glDetr = getEmpGl(emp);
            Hashtable gls = new Hashtable();
            int GlId = glDetr.Id;
            int cntGl = 0;

            cntGl = (from p in oDB.MstGLDOverTimeDetail where p.GLDId.ToString() == GlId.ToString() && p.OvertimeId.ToString() == overtim.ID.ToString() select p).Count();

            if (cntGl > 0)
            {
                MstGLDOverTimeDetail glOt = (from p in oDB.MstGLDOverTimeDetail where p.GLDId.ToString() == GlId.ToString() && p.OvertimeId.ToString() == overtim.ID.ToString() select p).Single();
                gls.Add("DrAcct", glOt.CostAccount);
                gls.Add("CrAcct", glOt.BalancingAccount);
                gls.Add("DrAcctName", glOt.CostAcctDisplay);
                gls.Add("CrAcctName", glOt.BalancingAcctDisplay);
            }
            else
            {
                gls.Add("DrAcct", "Not Found");
                gls.Add("CrAcct", "Not Found");
                gls.Add("DrAcctName", "Not Found");
                gls.Add("CrAcctName", "Not Found");
            }

            return gls;

        }

        public Hashtable getLeaveDedGL(MstEmployee emp, int dedId)
        {
            MstGLDetermination glDetr = getEmpGl(emp);
            Hashtable gls = new Hashtable();
            int GlId = glDetr.Id;
            int cntGl = 0;
            cntGl = (from p in oDB.MstGLDLeaveDedDetails where p.GLDId.ToString() == GlId.ToString() && p.LeaveDedId.ToString() == dedId.ToString() select p).Count();

            if (cntGl > 0)
            {
                MstGLDLeaveDedDetails glOt = (from p in oDB.MstGLDLeaveDedDetails where p.GLDId.ToString() == GlId.ToString() && p.LeaveDedId.ToString() == dedId.ToString() select p).FirstOrDefault();
                gls.Add("DrAcct", glOt.CostAccount);
                gls.Add("CrAcct", glOt.BalancingAccount);
                gls.Add("DrAcctName", glOt.CostAcctDisplay);
                gls.Add("CrAcctName", glOt.BalancingAcctDisplay);
            }
            else
            {
                gls.Add("DrAcct", "Not Found");
                gls.Add("CrAcct", "Not Found");
                gls.Add("DrAcctName", "Not Found");
                gls.Add("CrAcctName", "Not Found");
            }

            return gls;

        }

        public Hashtable getAdvGL(MstEmployee emp, MstAdvance adv)
        {
            MstGLDetermination glDetr = getEmpGl(emp);
            Hashtable gls = new Hashtable();
            int GlId = glDetr.Id;
            int cntGl = 0;

            cntGl = (from p in oDB.MstGLDAdvanceDetail where p.GLDId.ToString() == GlId.ToString() && p.AdvancesId.ToString() == adv.Id.ToString() select p).Count();
            if (cntGl > 0)
            {
                MstGLDAdvanceDetail glAdv = (from p in oDB.MstGLDAdvanceDetail where p.GLDId.ToString() == GlId.ToString() && p.AdvancesId.ToString() == adv.Id.ToString() select p).FirstOrDefault();
                gls.Add("DrAcct", glAdv.CostAccount);
                gls.Add("CrAcct", glAdv.BalancingAccount);
                gls.Add("DrAcctName", glAdv.CostAcctDisplay);
                gls.Add("CrAcctName", glAdv.BalancingAcctDisplay);
                gls.Add("Indicator", string.IsNullOrEmpty(glAdv.A1Indicator) == true ? "Not Found" : glAdv.A1Indicator);
            }
            else
            {
                gls.Add("DrAcct", "Not Found");
                gls.Add("CrAcct", "Not Found");
                gls.Add("DrAcctName", "Not Found");
                gls.Add("CrAcctName", "Not Found");
                gls.Add("Indicator", "Not Found");
            }

            return gls;

        }

        public decimal getElementAmount(MstEmployee emp, string valType, decimal Value, int payrollId)
        {
            decimal outValue = Convert.ToDecimal(0.00);

            switch (valType.Trim())
            {

                case "POB":
                    outValue = Convert.ToDecimal(Value) / 100 * (decimal)emp.BasicSalary;

                    break;
                case "POG":

                    //outValue = Convert.ToDecimal(Value) / 100 * (decimal)getEmpGross(emp);
                    outValue = Convert.ToDecimal(Value) / 100 * (decimal)getEmpGross(emp, payrollId);

                    break;
                case "FIX":
                    outValue = Convert.ToDecimal(Value);

                    break;
            }
            return outValue;
        }

        public decimal getElementAmount(MstEmployee emp, string valType, decimal Value)
        {
            decimal outValue = Convert.ToDecimal(0.00);

            switch (valType.Trim())
            {

                case "POB":
                    outValue = Convert.ToDecimal(Value) / 100 * (decimal)emp.BasicSalary;

                    break;
                case "POG":

                    //outValue = Convert.ToDecimal(Value) / 100 * (decimal)getEmpGross(emp);
                    outValue = Convert.ToDecimal(Value) / 100 * (decimal)getEmpGross(emp);

                    break;
                case "FIX":
                    outValue = Convert.ToDecimal(Value);

                    break;
            }
            return outValue;
        }

        public object getScallerValue(string strSql)
        {
            object outResult = new object();
            SqlConnection con = (SqlConnection)oDB.Connection;
            try
            {

                if (con.State == ConnectionState.Closed) con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandText = strSql;
                outResult = cmd.ExecuteScalar();

            }
            catch
            {
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }

            return outResult;
        }

        public object ExecuteQueries(string strSql)
        {
            object outResult = new object();
            SqlConnection con = (SqlConnection)oDB.Connection;
            try
            {
                if (con.State == ConnectionState.Closed) con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandText = strSql;
                outResult = cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                outResult = 0;
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
            return outResult;
        }

        public long getNextId(string tblName, string idCol)
        {
            long nextId = 1;
            string strSql = " Select isnull(max(" + idCol + "),0) + 1 as nextId from " + hrmsDbname + ".dbo." + tblName;
            try
            {
                nextId = Convert.ToInt32(getScallerValue(strSql));
            }
            catch { }
            return nextId;
        }

        public DataTable getDataTable(string strsql)
        {
            DataTable dt = new DataTable();
            SqlConnection con = (SqlConnection)oDB.Connection;
            try
            {
                if (con.State == ConnectionState.Closed) con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandText = strsql;
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Clear();
                dt.Rows.Clear();
                dt.Load(dr);
                dr.Close();
            }
            catch (Exception ex)
            {
                string tempvalue = ex.Message;
            }
            finally
            {
                if (con.State == ConnectionState.Open) con.Close();
            }
            return dt;
        }

        public DataTable getDataTable(string strsql, string method2)
        {
            DataTable dt = new DataTable();
            SqlConnection con = (SqlConnection)oDB.Connection;
            try
            {


                if (con.State == ConnectionState.Closed) con.Open(); else con.Close();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;

                cmd.CommandText = strsql;

                SqlDataAdapter da = new SqlDataAdapter(strsql, con);
                da.Fill(dt);
                return dt;

                //SqlDataReader dr = cmd.ExecuteReader();


                //dt.Clear();
                //dt.Rows.Clear();
                //dt.Load(dr);

                //dr.Close();
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                if (con.State == ConnectionState.Closed) con.Open();
            }






        }

        public String getExecuteQueryScaller(string strquery)
        {
            string retvalue = "";
            SqlConnection con = (SqlConnection)oDB.Connection;
            try
            {
                if (con.State == ConnectionState.Closed) con.Open(); else con.Close();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;

                cmd.CommandText = strquery;

                retvalue = Convert.ToString(cmd.ExecuteScalar());
                if (string.IsNullOrEmpty(retvalue))
                    retvalue = "0";
            }
            catch (Exception ex)
            {
                retvalue = "0";
            }
            finally
            {
                con.Close();
            }
            return retvalue;
        }

        public decimal getDaysCnt(MstEmployee emp, CfgPeriodDates per, out decimal payDays, out decimal leaveDays, out decimal monthDays)
        {
            decimal outResult = 0.00M;
            monthDays = (decimal)emp.CfgPayrollDefination.WorkDays;
            if (monthDays == 0.00M)
            {
                monthDays = (Convert.ToDateTime(per.EndDate) - Convert.ToDateTime(per.StartDate)).Days;
                monthDays = monthDays + 1;
            }
            leaveDays = 0.00M;
            payDays = 0.00M;
            decimal lateJoiningDays = 0.00M;
            decimal earlyTermDays = 0.00M;
            decimal vlDays = 0.00M;
            decimal ShiftDaysOnOff = 0.0M;
            if (emp.JoiningDate != null && emp.JoiningDate >= per.StartDate && emp.JoiningDate <= per.EndDate)
            {
                lateJoiningDays = (Convert.ToDateTime(emp.JoiningDate) - Convert.ToDateTime(per.StartDate)).Days;
            }

            if (emp.TerminationDate != null && emp.TerminationDate >= per.StartDate && emp.TerminationDate <= per.EndDate)
            {
                earlyTermDays = (Convert.ToDateTime(per.EndDate) - Convert.ToDateTime(emp.TerminationDate)).Days;
                //earlyTermDays = 0;
            }

            // string strSql = "  SELECT     CASE unitsId WHEN 'Day' THEN totalcount ELSE totalcount / 8 END AS TotalLeaves, DeductAmnt,DocDate, LeaveFrom, LeaveTo, ID";
            //string strSql = "  SELECT     totalcount AS TotalLeaves, DeductAmnt,DocDate, LeaveFrom, LeaveTo, ID";
            //strSql += " FROM         dbo.TrnsLeavesRequest";
            //strSql += " WHERE     (DocStatus = N'LV0002') ";
            //strSql += " and empId = '" + emp.ID + "'  ";
            //strSql += " and ( (LeaveFrom between '" + per.StartDate.ToString() + "' and '" + per.EndDate + "') ";
            //strSql += " or  (LeaveTo between '" + per.StartDate.ToString() + "' and '" + per.EndDate + "') ) ";

            //string strSql = "  SELECT     CASE unitsId WHEN 'Day' THEN ISNULL(totalcount,0) ELSE ISNULL(totalcount,0)  END AS TotalLeaves, ISNULL(DeductAmnt,0) AS DeductAmnt,DocDate, LeaveFrom, LeaveTo, ID";
            //strSql += " FROM         dbo.TrnsLeavesRequest";
            //strSql += " WHERE     (DocStatus = N'LV0002') ";
            //strSql += " and empId = '" + emp.ID + "'";
            ////and TotalCount>=0.05";
            //strSql += " and ( (LeaveFrom between '" + per.StartDate.ToString() + "' and '" + per.EndDate + "') ";
            //strSql += " or  (LeaveTo between '" + per.StartDate.ToString() + "' and '" + per.EndDate + "') ) ";
            string strSql = @"
                                SELECT 
	                                A1.ID,
	                                ISNULL(A1.TotalCount,0) TotalLeaves,
	                                ISNULL(A1.DeductAmnt,0) DeductAmnt,
	                                A1.LeaveFrom, A1.LeaveTo,
	                                ISNULL(A2.flgVariableValue,0) flgVariableValue
                                FROM 
	                                dbo.TrnsLeavesRequest A1
	                                INNER JOIN dbo.MstLeaveType A2 ON A1.LeaveType = A2.ID
                                WHERE 
	                                A1.DocStatus = 'LV0002'
	                                AND A1.EmpID = '" + emp.ID + @"'
	                                AND ((A1.LeaveFrom BETWEEN '" + per.StartDate.ToString() + @"' AND '" + per.EndDate.ToString() + @"') OR (A1.LeaveTo BETWEEN '" + per.StartDate.ToString() + @"' AND '" + per.EndDate.ToString() + @"'))
                                ";

            DataTable dtLeaves = getDataTable(strSql);
            foreach (DataRow dr in dtLeaves.Rows)
            {
                DateTime fromDt = Convert.ToDateTime(dr["LeaveFrom"].ToString());
                DateTime toDt = Convert.ToDateTime(dr["LeaveTo"].ToString());
                decimal LeaveCnt = Convert.ToDecimal(dr["TotalLeaves"]);
                decimal deductedAmount = Convert.ToDecimal(dr["DeductAmnt"]);
                if (deductedAmount > 0)
                {
                    decimal leaveCount = 0.00M;
                    if (fromDt >= per.StartDate && toDt <= per.EndDate)
                    {
                        leaveCount = (decimal)LeaveCnt;

                    }
                    if (fromDt >= per.StartDate && toDt > per.EndDate)
                    {
                        leaveCount = (decimal)(Convert.ToDateTime(per.EndDate) - Convert.ToDateTime(fromDt)).Days + 1;

                    }
                    if (fromDt < per.StartDate && toDt <= per.EndDate)
                    {
                        leaveCount = (decimal)(Convert.ToDateTime(toDt) - Convert.ToDateTime(per.StartDate)).Days + 1;

                    }
                    if (fromDt < per.StartDate && toDt > per.EndDate)
                    {
                        leaveCount = monthDays;

                    }
                    leaveDays += leaveCount;
                }

                //if(leavetype is VL)
                //{
                //  vlDays+= leaveCount;
                //}                
            }
            if (!string.IsNullOrEmpty(emp.ShiftDaysCode))
            {
                var oCount = (from a in oDB.TrnsShiftsDaysRegister
                              where a.EmpCode == emp.EmpID
                              && a.RecordDate >= per.StartDate
                              && a.RecordDate <= per.EndDate
                              && a.DayStatus == 0
                              select a).Count();
                if (oCount > 0)
                {
                    ShiftDaysOnOff = oCount;
                }
            }
            payDays = monthDays - lateJoiningDays - earlyTermDays - vlDays;
            outResult = monthDays - (leaveDays + ShiftDaysOnOff) - lateJoiningDays - earlyTermDays;
            return outResult;
        }

        public decimal getDaysCntInMinutes(MstEmployee emp, CfgPeriodDates per, out decimal payDays, out decimal leaveDays, out decimal monthDays)
        {
            decimal outResult = 0.00M;
            leaveDays = 0.00M;
            payDays = 0.00M;
            monthDays = 0.0M;
            try
            {
                monthDays = (decimal)emp.CfgPayrollDefination.WorkDays;
                decimal MinutesInDay = 0.0M;
                MinutesInDay = Convert.ToDecimal(emp.CfgPayrollDefination.WorkHours) * 60;
                if (monthDays == 0.00M)
                {
                    monthDays = (Convert.ToDateTime(per.EndDate) - Convert.ToDateTime(per.StartDate)).Days;
                    monthDays = monthDays + 1;
                }
                decimal lateJoiningDays = 0.00M;
                decimal earlyTermDays = 0.00M;
                decimal vlDays = 0.00M;
                if (emp.JoiningDate != null && emp.JoiningDate >= per.StartDate && emp.JoiningDate <= per.EndDate)
                {
                    lateJoiningDays = (Convert.ToDateTime(emp.JoiningDate) - Convert.ToDateTime(per.StartDate)).Days;
                }

                if (emp.TerminationDate != null && emp.TerminationDate >= per.StartDate && emp.TerminationDate <= per.EndDate)
                {
                    earlyTermDays = (Convert.ToDateTime(per.EndDate) - Convert.ToDateTime(emp.TerminationDate)).Days;
                    //earlyTermDays = 0;
                }

                //string strSql = "  SELECT     CASE unitsId WHEN 'Day' THEN totalcount ELSE totalcount / 8 END AS TotalLeaves, DeductAmnt,DocDate, LeaveFrom, LeaveTo, ID";
                //strSql += " FROM         dbo.TrnsLeavesRequest";
                //strSql += " WHERE     (DocStatus = N'LV0002') ";
                //strSql += " and empId = '" + emp.ID + "'  ";
                //strSql += " and ( (LeaveFrom between '" + per.StartDate.ToString() + "' and '" + per.EndDate + "') ";
                //strSql += " or  (LeaveTo between '" + per.StartDate.ToString() + "' and '" + per.EndDate + "') ) ";

                string strSql = "  SELECT     CASE unitsId WHEN 'Day' THEN ISNULL(totalcount,0) ELSE ISNULL(totalcount,0)  END AS TotalLeaves, ISNULL(DeductAmnt,0) AS DeductAmnt,DocDate, LeaveFrom, LeaveTo, ID";
                strSql += " FROM         dbo.TrnsLeavesRequest";
                strSql += " WHERE     (DocStatus = N'LV0002') ";
                strSql += " and empId = '" + emp.ID + "'  ";
                strSql += " and ( (LeaveFrom between '" + per.StartDate.ToString() + "' and '" + per.EndDate + "') ";
                strSql += " or  (LeaveTo between '" + per.StartDate.ToString() + "' and '" + per.EndDate + "') ) ";

                DataTable dtLeaves = getDataTable(strSql);
                foreach (DataRow dr in dtLeaves.Rows)
                {
                    DateTime fromDt = Convert.ToDateTime(dr["LeaveFrom"].ToString());
                    DateTime toDt = Convert.ToDateTime(dr["LeaveTo"].ToString());
                    decimal LeaveCnt = Convert.ToDecimal(dr["TotalLeaves"]);
                    decimal deductedAmount = Convert.ToDecimal(dr["DeductAmnt"]);
                    if (deductedAmount > 0)
                    {
                        decimal leaveCount = 0.00M;
                        if (fromDt >= per.StartDate && toDt <= per.EndDate)
                        {
                            leaveCount = (decimal)LeaveCnt;

                        }
                        if (fromDt >= per.StartDate && toDt > per.EndDate)
                        {
                            leaveCount = (decimal)(Convert.ToDateTime(per.EndDate) - Convert.ToDateTime(fromDt)).Days + 1;

                        }
                        if (fromDt < per.StartDate && toDt <= per.EndDate)
                        {
                            leaveCount = (decimal)(Convert.ToDateTime(toDt) - Convert.ToDateTime(per.StartDate)).Days + 1;

                        }
                        if (fromDt < per.StartDate && toDt > per.EndDate)
                        {
                            leaveCount = monthDays;

                        }
                        leaveDays += leaveCount;
                    }

                    //if(leavetype is VL)
                    //{
                    //  vlDays+= leaveCount;
                    //}                
                }
                payDays = monthDays - lateJoiningDays - earlyTermDays - vlDays;
                outResult = monthDays - leaveDays - lateJoiningDays - earlyTermDays;

                //New Minute Work.
                decimal temp = 0.0M;
                temp = monthDays;
                monthDays = temp * MinutesInDay;
                temp = 0.0M;
                temp = payDays;
                payDays = payDays * MinutesInDay;
                temp = 0.0M;
                temp = outResult;
                outResult = temp * MinutesInDay;
            }
            catch (Exception ex)
            {
                outResult = 0;
            }
            return outResult;
        }

        public DataTable salaryProcessingElements(MstEmployee emp, CfgPeriodDates period, decimal days, decimal grossSalary, MstGLDetermination emGl, decimal payRatio, decimal payRatioWithLeaves, decimal LeaveCount, decimal WorkingDays)
        {
            decimal amnt = 0.0M;
            decimal DaysCnt = days;
            string empContractType = emp.EmployeeContractType;
            decimal EmployerContributionRatio = 1;
            Hashtable elementGls = new Hashtable();
            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");
            dtOut.Columns.Add("NRTaxbleAmnt");
            IEnumerable<TrnsEmployeeElementDetail> eles = from p in oDB.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == emp.ID && p.FlgActive == true && p.MstElements.FlgProcessInPayroll == true && (p.MstElements.Type == "Rec" || (p.PeriodId == period.ID && p.FlgOneTimeConsumed != true)) select p;
            //List<string> oList = new List<string>();
            //foreach (var one in eles)
            //{
            //    oList.Add(one.MstElements.ElementName);
            //}
            if (empContractType == "PROB")
            {
                eles = eles.Where(e => e.MstElements.FlgProbationApplicable == true).ToList();
            }
            foreach (TrnsEmployeeElementDetail ele in eles)
            {

                int cnt = (from p in oDB.MstElementLink where p.ElementID == ele.ElementId && p.PayrollID == emp.PayrollID && p.FlgActive == true select p).Count();
                if (cnt == 0) continue;
                DataRow dr = dtOut.NewRow();
                clsElement eleInfo = new clsElement(oDB, ele, emp, grossSalary, 1);

                decimal taxableAmnt = 0.00M;
                amnt = (decimal)eleInfo.Amount;
                //string elementName = ele.MstElements.ElementName;
                string eleName = string.Empty;
                eleName = ele.MstElements.ElementName;
                elementGls = getElementGL(emp, ele.MstElements, emGl);
                if (amnt != 0 || eleInfo.emprAmount != 0)
                {

                    string baseValType = "";
                    decimal baseValue = 0.0M;
                    decimal baseCalculatedOn = 0.0M;
                    baseValType = eleInfo.ValueType;
                    baseValue = eleInfo.Value;



                    if (baseValType == "POB")
                    {
                        baseCalculatedOn = (decimal)emp.BasicSalary;

                    }
                    if (baseValType == "POG")
                    {
                        if (!Convert.ToBoolean(ele.MstElements.FlgVGross))
                        {
                            baseCalculatedOn = grossSalary;
                        }
                        else
                        {
                            if (payRatioWithLeaves == 1)
                            {
                                baseCalculatedOn = (grossSalary / WorkingDays) * LeaveCount;
                            }
                            else
                            {
                                baseCalculatedOn = grossSalary * payRatioWithLeaves;
                            }
                        }
                    }
                    if (baseValType.ToUpper() == "FIX")
                    {
                        baseCalculatedOn = (decimal)eleInfo.Amount;
                        baseValue = 100.00M;

                    }


                    if (eleInfo.ElementType == "Ear" && ele.MstElements.MstElementEarning[0].FlgVariableValue == true)
                    {
                        if (emp.CfgPayrollDefination.WorkDays > 0)
                        {
                            amnt = Convert.ToDecimal(amnt * DaysCnt / emp.CfgPayrollDefination.WorkDays);
                        }
                        else
                        {
                            if ((Convert.ToDateTime(period.EndDate) - Convert.ToDateTime(period.StartDate)).Days > 0)
                            {
                                int daysxx = (Convert.ToDateTime(period.EndDate) - Convert.ToDateTime(period.StartDate)).Days + 1;
                                amnt = Convert.ToDecimal(amnt * DaysCnt / daysxx);
                            }
                        }
                    }
                    //Zeeshan Block
                    if (eleInfo.ElementType == "Ear" && ele.MstElements.MstElementEarning[0].FlgLeaveEncashment == true)
                    {
                        amnt = ele.Amount.Value;
                    }
                    if (eleInfo.ElementType == "Con")
                    {
                        decimal maxAppAmount = 0.0M;
                        MstElementContribution eleContr = ele.MstElements.MstElementContribution.FirstOrDefault();
                        if (eleContr != null)
                        {
                            int elementID = eleContr.ElementId.Value;
                            maxAppAmount = eleContr.MaxAppAmount == null ? 0.0M : eleContr.MaxAppAmount.Value;
                        }
                        if (maxAppAmount > 0 && maxAppAmount <= baseCalculatedOn)
                        {
                            amnt = ele.MstElements.MstElementContribution[0].MaxEmployeeContribution.Value;
                        }
                        else if (Convert.ToBoolean(ele.MstElements.FlgVGross))
                        {

                            amnt = baseCalculatedOn * (Convert.ToDecimal(ele.Value) / 100);

                        }
                        else if (ele.MstElements.FlgConBatch != null && Convert.ToBoolean(ele.MstElements.FlgConBatch))
                        {
                            if (baseValType.ToUpper() == "FIX")
                            {
                                amnt = baseCalculatedOn;
                            }
                            else
                            {
                                amnt = baseCalculatedOn * (Convert.ToDecimal(ele.EmpContr) / 100);
                            }
                        }
                    }
                    //End of Zeeshan Block 


                    //Mahan Kaam by Mahan Insan
                    if (ele.MstElements.Type == "Rec" && eleInfo.ElementType == "Ear" && ele.MstElements.MstElementEarning[0].FlgPropotionate == true)
                    {
                        //amnt = amnt * payRatio;
                        if (emp.CfgPayrollDefination.WorkDays > 0)
                        {
                            amnt = Convert.ToDecimal(amnt * DaysCnt / emp.CfgPayrollDefination.WorkDays);
                        }
                        else
                        {
                            if ((Convert.ToDateTime(period.EndDate) - Convert.ToDateTime(period.StartDate)).Days > 0)
                            {
                                int daysxx = (Convert.ToDateTime(period.EndDate) - Convert.ToDateTime(period.StartDate)).Days + 1;
                                amnt = Convert.ToDecimal(amnt * DaysCnt / daysxx);
                            }
                        }
                    }
                    if (ele.MstElements.Type == "Rec" && eleInfo.ElementType == "Ded" && ele.MstElements.MstElementDeduction[0].FlgPropotionate == true)
                    {
                        amnt = amnt * payRatio;
                    }
                    //End of Mahan kaam 
                    if (eleInfo.ElementType == "Ear" && ele.MstElements.MstElementEarning[0].FlgNotTaxable == true)
                    {
                        taxableAmnt = 0.00M;
                    }
                    else
                    {
                        taxableAmnt = amnt;
                    }

                    if (eleInfo.ElementType == "Con" && ele.MstElements.MstElementContribution[0].FlgVariableValue == true)
                    {
                        if (emp.CfgPayrollDefination.WorkDays > 0)
                        {
                            amnt = Convert.ToDecimal(amnt * DaysCnt / emp.CfgPayrollDefination.WorkDays);
                        }
                        else
                        {
                            if ((Convert.ToDateTime(period.EndDate) - Convert.ToDateTime(period.StartDate)).Days > 0)
                            {
                                int daysxx = (Convert.ToDateTime(period.EndDate) - Convert.ToDateTime(period.StartDate)).Days + 1;
                                EmployerContributionRatio = Convert.ToDecimal(DaysCnt / daysxx);
                                amnt = Convert.ToDecimal(amnt * DaysCnt / daysxx);
                            }
                        }
                    }

                    if (eleInfo.ElementType == "Ded" || eleInfo.ElementType == "Con")
                    {
                        amnt = -amnt;
                        taxableAmnt = 0.00M;


                    }

                    ele.FlgOneTimeConsumed = true;
                    dr["LineType"] = "Element";
                    dr["LineSubType"] = ele.MstElements.ElementName;
                    dr["LineValue"] = amnt;
                    dr["LineMemo"] = ele.MstElements.Description;
                    dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                    dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                    dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                    dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();


                    dr["LineBaseEntry"] = ele.Id;
                    dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                    dr["BaseValue"] = baseValue;
                    dr["BaseValueType"] = baseValType;
                    dr["TaxbleAmnt"] = taxableAmnt;
                    dr["NRTaxbleAmnt"] = 0.00M;
                    if (ele.MstElements.Type == "Non-Rec")
                    {
                        dr["NRTaxbleAmnt"] = taxableAmnt;
                    }
                    dtOut.Rows.Add(dr);
                    if (eleInfo.ElementType == "Con")
                    {

                        decimal emprTaxDiscount = 0.00M;
                        if (ele.MstElements.MstElementContribution[0].ContTaxDiscount != null)
                        {
                            emprTaxDiscount = (decimal)ele.MstElements.MstElementContribution[0].ContTaxDiscount;

                            if (emprTaxDiscount > 0 && eleInfo.emprAmount * 12 > emprTaxDiscount)
                            {
                                taxableAmnt = eleInfo.emprAmount - emprTaxDiscount / 12;
                            }
                            else
                            {

                            }
                            //Modified BY Zeeshan
                            if (ele.MstElements.MstElementContribution[0].MaxAppAmount <= baseCalculatedOn)
                            {
                                taxableAmnt = 500 - emprTaxDiscount / 12;
                            }
                            //End Of Modification
                        }
                        if (taxableAmnt < 0) taxableAmnt = 0.00M;
                        dr = dtOut.NewRow();
                        dr["LineType"] = "Element";
                        dr["LineSubType"] = "Empr Cont";
                        if (ele.MstElements.MstElementContribution[0].MaxAppAmount > 0 && ele.MstElements.MstElementContribution[0].MaxAppAmount <= baseCalculatedOn)
                        {
                            dr["LineValue"] = ele.MstElements.MstElementContribution[0].MaxEmployerContribution.Value;
                        }
                        else
                        {
                            if (ele.MstElements.FlgConBatch != null && Convert.ToBoolean(ele.MstElements.FlgConBatch))
                            {
                                if (baseValType.ToUpper() == "FIX")
                                {
                                    dr["LineValue"] = baseCalculatedOn;
                                }
                                else
                                {
                                    dr["LineValue"] = baseCalculatedOn * (Convert.ToDecimal(ele.EmplrContr) / 100);
                                }
                            }
                            else
                            {
                                //dr["LineValue"] = eleInfo.emprAmount * EmployerContributionRatio;
                                if (!Convert.ToBoolean(ele.MstElements.FlgVGross))
                                {
                                    //baseCalculatedOn = grossSalary;
                                    dr["LineValue"] = eleInfo.emprAmount * EmployerContributionRatio;
                                }
                                else
                                {
                                    if (payRatioWithLeaves == 1)
                                    {
                                        //baseCalculatedOn = (grossSalary / WorkingDays) * LeaveCount;
                                        dr["LineValue"] = (eleInfo.emprAmount / WorkingDays) * LeaveCount;

                                    }
                                    else
                                    {
                                        //baseCalculatedOn = grossSalary * payRatioWithLeaves;
                                        dr["LineValue"] = eleInfo.emprAmount * payRatioWithLeaves;
                                    }
                                }
                            }
                        }
                        dr["LineMemo"] = "Empr Contribution " + ele.MstElements.Description;
                        dr["DebitAccount"] = elementGls["EmprDrAcct"].ToString();
                        dr["CreditAccount"] = elementGls["EmprCrAcct"].ToString();
                        dr["DebitAccountName"] = elementGls["EmprDrAcctName"].ToString();
                        dr["CreditAccountName"] = elementGls["EmprCrAcctName"].ToString();
                        dr["LineBaseEntry"] = ele.Id;
                        dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                        dr["BaseValue"] = baseValue;
                        dr["BaseValueType"] = baseValType;
                        dr["TaxbleAmnt"] = taxableAmnt;
                        dr["NRTaxbleAmnt"] = 0.00;
                        dtOut.Rows.Add(dr);
                    }
                }
            }
            return dtOut;
        }

        public DataTable ElementsProcessionEarnings(MstEmployee oEmp, CfgPeriodDates period, decimal days, decimal grossSalary, MstGLDetermination emGl, decimal payRatio, decimal payRatioWithLeaves, decimal LeaveCount, decimal WorkingDays, int pType, decimal percent = 1)
        {
            decimal amnt = 0.0M;
            decimal DaysCnt = days;
            decimal taxableAmnt = 0.00M;
            string empContractType = oEmp.EmployeeContractType;
            string eleName = string.Empty;
            decimal EmployerContributionRatio = 1;
            Hashtable elementGls = new Hashtable();
            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");
            dtOut.Columns.Add("NRTaxbleAmnt");
            var oElementEarnings = (from p in oDB.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == oEmp.ID && p.FlgActive == true && p.MstElements.FlgProcessInPayroll == true && p.MstElements.ElmtType == "Ear" && (p.MstElements.Type == "Rec" || (p.PeriodId == period.ID && (p.FlgOneTimeConsumed == null ? false : Convert.ToBoolean(p.FlgOneTimeConsumed)) != true)) select p).ToList();
            if (oElementEarnings.Count > 0)
            {
                if (empContractType == "PROB")
                {
                    oElementEarnings = oElementEarnings.Where(a => a.MstElements.FlgProbationApplicable == true).ToList();
                }
                foreach (var OneElement in oElementEarnings)
                {
                    Int32 ElementLinkCheck = (from a in oDB.MstElementLink where a.ElementID == OneElement.ElementId && a.PayrollID == oEmp.PayrollID && a.FlgActive == true select a).Count();
                    if (ElementLinkCheck == 0) continue;

                    clsElement ElementInfo = new clsElement(oDB, OneElement, oEmp, grossSalary, pType);
                    if (!OneElement.MstElements.FlgGradeDep.GetValueOrDefault())
                    {
                        //percent = 1;
                        amnt = (decimal)ElementInfo.Amount;
                    }
                    else
                    {
                        amnt = (decimal)ElementInfo.Amount * percent;
                        logger("Element name: " + OneElement.MstElements.ElementName + " and amount: " + amnt.ToString() + " percent value: " + percent.ToString());
                    }
                    
                    eleName = OneElement.MstElements.ElementName;
                    elementGls = getElementGL(oEmp, OneElement.MstElements, emGl);

                    if (amnt != 0 || ElementInfo.emprAmount != 0)
                    {
                        string baseValType = "";
                        decimal baseValue = 0.0M;
                        decimal baseCalculatedOn = 0.0M;
                        baseValType = ElementInfo.ValueType;
                        baseValue = ElementInfo.Value;
                        if (baseValType == "POB")
                        {
                            baseCalculatedOn = (decimal)oEmp.BasicSalary;
                        }
                        if (baseValType == "POG")
                        {
                            if (!Convert.ToBoolean(OneElement.MstElements.FlgVGross))
                            {
                                //baseCalculatedOn = grossSalary;
                                baseCalculatedOn = getEmpGross(oEmp);
                            }
                            else
                            {
                                if (payRatioWithLeaves == 1)
                                {
                                    //baseCalculatedOn = (grossSalary / WorkingDays) * LeaveCount;
                                    baseCalculatedOn = (getEmpGross(oEmp) / WorkingDays) * LeaveCount;
                                }
                                else
                                {
                                    //baseCalculatedOn = grossSalary * payRatioWithLeaves;
                                    baseCalculatedOn = getEmpGross(oEmp) * payRatioWithLeaves;
                                }
                            }
                        }
                        if (baseValType == "FIX")
                        {
                            baseCalculatedOn = amnt;
                            baseValue = 100.00M;
                        }
                        if (OneElement.MstElements.MstElementEarning[0].FlgVariableValue == true)
                        {
                            if (oEmp.CfgPayrollDefination.WorkDays > 0)
                            {
                                amnt = Convert.ToDecimal(amnt * DaysCnt / oEmp.CfgPayrollDefination.WorkDays);
                            }
                            else
                            {
                                Int32 ActualDays = (Convert.ToDateTime(period.EndDate) - Convert.ToDateTime(period.StartDate)).Days + 1;
                                if (ActualDays > 0)
                                {
                                    amnt = Convert.ToDecimal(amnt * DaysCnt / ActualDays);
                                }
                            }
                        }
                        if (OneElement.MstElements.MstElementEarning[0].FlgLeaveEncashment == true)
                        {
                            amnt = OneElement.Amount.Value;
                        }
                        if (OneElement.MstElements.Type == "Rec" && OneElement.MstElements.MstElementEarning[0].FlgPropotionate == true)
                        {
                            amnt = amnt * payRatio;
                        }
                        //Shift On Off Working
                        if (Convert.ToBoolean(OneElement.MstElements.FlgShiftDays) && !string.IsNullOrEmpty(oEmp.ShiftDaysCode))
                        {
                            amnt = amnt * DaysCnt / WorkingDays;
                        }
                        if (OneElement.MstElements.MstElementEarning[0].FlgNotTaxable == true)
                        {
                            taxableAmnt = 0.00M;
                        }
                        else
                        {
                            taxableAmnt = amnt;
                        }
                        OneElement.FlgOneTimeConsumed = true;
                        DataRow dr = dtOut.NewRow();
                        dr["LineType"] = "Element";
                        dr["LineSubType"] = OneElement.MstElements.ElementName;
                        dr["LineValue"] = amnt;
                        dr["LineMemo"] = OneElement.MstElements.Description;
                        dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                        dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                        dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                        dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();
                        dr["LineBaseEntry"] = OneElement.Id;
                        dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                        dr["BaseValue"] = baseValue;
                        dr["BaseValueType"] = baseValType;
                        dr["TaxbleAmnt"] = taxableAmnt;
                        dr["NRTaxbleAmnt"] = 0.00M;
                        if (OneElement.MstElements.Type == "Non-Rec")
                        {
                            dr["NRTaxbleAmnt"] = taxableAmnt;
                        }
                        dtOut.Rows.Add(dr);
                    }
                }
            }
            return dtOut;
        }

        public DataTable ElementsProcessionEarnings_ForHalfMonthPakola(MstEmployee oEmp, CfgPeriodDates period, decimal days, decimal grossSalary, MstGLDetermination emGl, decimal payRatio, decimal payRatioWithLeaves, decimal LeaveCount, decimal WorkingDays, int pType)
        {
            decimal amnt = 0.0M;
            decimal DaysCnt = days;
            decimal taxableAmnt = 0.00M;
            string empContractType = oEmp.EmployeeContractType;
            string eleName = string.Empty;
            decimal EmployerContributionRatio = 1;
            Hashtable elementGls = new Hashtable();
            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");
            dtOut.Columns.Add("NRTaxbleAmnt");
            var oElementEarnings = (from p in oDB.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == oEmp.ID && p.FlgActive == true && p.MstElements.FlgProcessInPayroll == true && p.MstElements.ElmtType == "Ear" && (p.MstElements.Type == "Rec" || (p.PeriodId == period.ID && (p.FlgOneTimeConsumed == null ? false : Convert.ToBoolean(p.FlgOneTimeConsumed)) != true)) select p).ToList();
            if (oElementEarnings.Count > 0)
            {
                if (empContractType == "PROB")
                {
                    oElementEarnings = oElementEarnings.Where(a => a.MstElements.FlgProbationApplicable == true).ToList();
                }
                foreach (var OneElement in oElementEarnings)
                {
                    Int32 ElementLinkCheck = (from a in oDB.MstElementLink where a.ElementID == OneElement.ElementId && a.PayrollID == oEmp.PayrollID && a.FlgActive == true select a).Count();
                    if (ElementLinkCheck == 0) continue;

                    clsElement ElementInfo = new clsElement(oDB, OneElement, oEmp, grossSalary, pType);
                    amnt = (decimal)ElementInfo.Amount / 2;
                    eleName = OneElement.MstElements.ElementName;
                    elementGls = getElementGL(oEmp, OneElement.MstElements, emGl);

                    #region If Deduction Leave Then No Attendance Allownace Calculate
                    if (OneElement.MstElements.FlgAttendanceAllowance == true)
                    {
                        var CheckEnteredLeaves = (from a in oDB.TrnsLeavesRequest

                                                  where a.MstEmployee.EmpID == oEmp.EmpID
                                                  && period.StartDate <= a.LeaveFrom
                                                  && period.EndDate >= a.LeaveFrom
                                                   && a.LeaveType == a.MstLeaveType.ID
                                               && a.MstLeaveType.LeaveType == "Ded"
                                                  select a).FirstOrDefault();
                        if (CheckEnteredLeaves != null)
                        {
                            amnt = 0;
                        }
                        else
                        {
                            #region Calculate Allowance Amount
                            decimal TotalLeaveUsed = 0;
                            TotalLeaveUsed = (from a in oDB.TrnsLeavesRequest
                                              where a.MstEmployee.EmpID == oEmp.EmpID
                                              && period.StartDate <= a.LeaveFrom
                                                   && period.EndDate >= a.LeaveFrom
                                                   && a.LeaveType == a.MstLeaveType.ID
                                                   && a.MstLeaveType.LeaveType != "Ded"
                                              select a.TotalCount).Sum() ?? 0;
                            var oMasterAllowanceCollection = (from a in oDB.MstAttendanceAllowance
                                                              where a.FlgActive == true
                                                              && a.DocNo == oEmp.AttendanceAllowance
                                                              select a).OrderBy(o => o.LeaveCount).ToList();
                            string ElementCode = "";
                            decimal ElementAmount = 0M;

                            var maxLimit = oMasterAllowanceCollection.FirstOrDefault(o => o.LeaveCount == oMasterAllowanceCollection.Max(e => e.LeaveCount));

                            if (maxLimit != null)
                            {
                                foreach (var OneAllowance in oMasterAllowanceCollection)
                                {
                                    string AllowanceCode = OneAllowance.Code;
                                    if (TotalLeaveUsed >= maxLimit.LeaveCount)
                                    {
                                        ElementAmount = Convert.ToDecimal(maxLimit.Value);
                                        ElementCode = maxLimit.ElementType;
                                        break;
                                    }
                                    else if (TotalLeaveUsed <= OneAllowance.LeaveCount)
                                    {
                                        ElementAmount = Convert.ToDecimal(OneAllowance.Value);
                                        ElementCode = OneAllowance.ElementType;
                                        break;
                                    }
                                }
                                amnt = ElementAmount / 2;
                            }
                            #endregion
                        }
                    }
                    #endregion

                    if (amnt != 0 || ElementInfo.emprAmount != 0)
                    {
                        string baseValType = "";
                        decimal baseValue = 0.0M;
                        decimal baseCalculatedOn = 0.0M;
                        baseValType = ElementInfo.ValueType;
                        baseValue = ElementInfo.Value;
                        if (baseValType == "POB")
                        {
                            baseCalculatedOn = (decimal)oEmp.BasicSalary / 2;
                        }
                        if (baseValType == "POG")
                        {
                            if (!Convert.ToBoolean(OneElement.MstElements.FlgVGross))
                            {
                                baseCalculatedOn = grossSalary / 2;
                            }
                            else
                            {
                                if (payRatioWithLeaves == 1)
                                {
                                    baseCalculatedOn = ((grossSalary / 2) / WorkingDays) * LeaveCount;
                                }
                                else
                                {
                                    baseCalculatedOn = (grossSalary / 2) * payRatioWithLeaves;
                                }
                            }
                        }
                        if (baseValType == "FIX")
                        {
                            baseCalculatedOn = (decimal)ElementInfo.Amount;
                            baseValue = 100.00M;
                        }
                        if (OneElement.MstElements.MstElementEarning[0].FlgVariableValue == true)
                        {
                            if (oEmp.CfgPayrollDefination.WorkDays > 0)
                            {
                                amnt = Convert.ToDecimal(amnt * DaysCnt / oEmp.CfgPayrollDefination.WorkDays);
                            }
                            else
                            {
                                Int32 ActualDays = (Convert.ToDateTime(period.EndDate) - Convert.ToDateTime(period.StartDate)).Days + 1;
                                if (ActualDays > 0)
                                {
                                    amnt = Convert.ToDecimal(amnt * DaysCnt / ActualDays);
                                }
                            }
                        }
                        if (OneElement.MstElements.MstElementEarning[0].FlgLeaveEncashment == true)
                        {
                            amnt = OneElement.Amount.Value;
                        }
                        if (OneElement.MstElements.Type == "Rec" && OneElement.MstElements.MstElementEarning[0].FlgPropotionate == true)
                        {
                            amnt = amnt * payRatio;
                        }
                        //Shift On Off Working
                        if (Convert.ToBoolean(OneElement.MstElements.FlgShiftDays) && !string.IsNullOrEmpty(oEmp.ShiftDaysCode))
                        {
                            amnt = amnt * DaysCnt / WorkingDays;
                        }
                        if (OneElement.MstElements.MstElementEarning[0].FlgNotTaxable == true)
                        {
                            taxableAmnt = 0.00M;
                        }
                        else
                        {
                            taxableAmnt = amnt;
                        }
                        OneElement.FlgOneTimeConsumed = true;
                        DataRow dr = dtOut.NewRow();
                        dr["LineType"] = "Element";
                        dr["LineSubType"] = OneElement.MstElements.ElementName;
                        dr["LineValue"] = amnt;
                        dr["LineMemo"] = OneElement.MstElements.Description;
                        dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                        dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                        dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                        dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();
                        dr["LineBaseEntry"] = OneElement.Id;
                        dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                        dr["BaseValue"] = baseValue;
                        dr["BaseValueType"] = baseValType;
                        dr["TaxbleAmnt"] = taxableAmnt;
                        dr["NRTaxbleAmnt"] = 0.00M;
                        if (OneElement.MstElements.Type == "Non-Rec")
                        {
                            dr["NRTaxbleAmnt"] = taxableAmnt;
                        }
                        dtOut.Rows.Add(dr);
                    }
                }
            }
            return dtOut;
        }

        public DataTable ElementsProcessionEarnings_ForHalfMonth(MstEmployee oEmp, CfgPeriodDates period, decimal days, decimal grossSalary, MstGLDetermination emGl, decimal payRatio, decimal payRatioWithLeaves, decimal LeaveCount, decimal WorkingDays, int pType)
        {
            decimal amnt = 0.0M;
            decimal DaysCnt = days;
            DateTime dtGetMonthDays;
            Int32 intGetMonthDays;
            dtGetMonthDays = Convert.ToDateTime(period.StartDate);
            intGetMonthDays = DateTime.DaysInMonth(dtGetMonthDays.Year, dtGetMonthDays.Month);
            decimal taxableAmnt = 0.00M;
            string empContractType = oEmp.EmployeeContractType;
            string eleName = string.Empty;
            decimal EmployerContributionRatio = 1;
            Hashtable elementGls = new Hashtable();
            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");
            dtOut.Columns.Add("NRTaxbleAmnt");
            var oElementEarnings = (from p in oDB.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == oEmp.ID && p.FlgActive == true && p.MstElements.FlgProcessInPayroll == true && p.MstElements.ElmtType == "Ear" && (p.MstElements.Type == "Rec" || (p.PeriodId == period.ID && (p.FlgOneTimeConsumed == null ? false : Convert.ToBoolean(p.FlgOneTimeConsumed)) != true)) select p).ToList();
            if (oElementEarnings.Count > 0)
            {
                if (empContractType == "PROB")
                {
                    oElementEarnings = oElementEarnings.Where(a => a.MstElements.FlgProbationApplicable == true).ToList();
                }
                foreach (var OneElement in oElementEarnings)
                {
                    Int32 ElementLinkCheck = (from a in oDB.MstElementLink where a.ElementID == OneElement.ElementId && a.PayrollID == oEmp.PayrollID && a.FlgActive == true select a).Count();
                    if (ElementLinkCheck == 0) continue;

                    clsElement ElementInfo = new clsElement(oDB, OneElement, oEmp, grossSalary, pType);
                    if (OneElement.MstElements.Type.Trim() != "Non-Rec")
                    {
                        amnt = (decimal)(ElementInfo.Amount / intGetMonthDays) * WorkingDays;
                    }
                    else
                    {
                        amnt = (decimal)ElementInfo.Amount;
                    }
                    eleName = OneElement.MstElements.ElementName;
                    elementGls = getElementGL(oEmp, OneElement.MstElements, emGl);

                    #region If Deduction Leave Then No Attendance Allownace Calculate
                    if (OneElement.MstElements.FlgAttendanceAllowance == true)
                    {
                        var CheckEnteredLeaves = (from a in oDB.TrnsLeavesRequest

                                                  where a.MstEmployee.EmpID == oEmp.EmpID
                                                  && period.StartDate <= a.LeaveFrom
                                                  && period.EndDate >= a.LeaveFrom
                                                   && a.LeaveType == a.MstLeaveType.ID
                                               && a.MstLeaveType.LeaveType == "Ded"
                                                  select a).FirstOrDefault();
                        if (CheckEnteredLeaves != null)
                        {
                            amnt = 0;
                        }
                        else
                        {
                            #region Calculate Allowance Amount
                            decimal TotalLeaveUsed = 0;
                            TotalLeaveUsed = (from a in oDB.TrnsLeavesRequest
                                              where a.MstEmployee.EmpID == oEmp.EmpID
                                              && period.StartDate <= a.LeaveFrom
                                                   && period.EndDate >= a.LeaveFrom
                                                   && a.LeaveType == a.MstLeaveType.ID
                                                   && a.MstLeaveType.LeaveType != "Ded"
                                              select a.TotalCount).Sum() ?? 0;
                            var oMasterAllowanceCollection = (from a in oDB.MstAttendanceAllowance
                                                              where a.FlgActive == true
                                                              && a.DocNo == oEmp.AttendanceAllowance
                                                              select a).OrderBy(o => o.LeaveCount).ToList();
                            string ElementCode = "";
                            decimal ElementAmount = 0M;

                            var maxLimit = oMasterAllowanceCollection.FirstOrDefault(o => o.LeaveCount == oMasterAllowanceCollection.Max(e => e.LeaveCount));

                            if (maxLimit != null)
                            {
                                foreach (var OneAllowance in oMasterAllowanceCollection)
                                {
                                    string AllowanceCode = OneAllowance.Code;
                                    if (TotalLeaveUsed >= maxLimit.LeaveCount)
                                    {
                                        ElementAmount = Convert.ToDecimal(maxLimit.Value);
                                        ElementCode = maxLimit.ElementType;
                                        break;
                                    }
                                    else if (TotalLeaveUsed <= OneAllowance.LeaveCount)
                                    {
                                        ElementAmount = Convert.ToDecimal(OneAllowance.Value);
                                        ElementCode = OneAllowance.ElementType;
                                        break;
                                    }
                                }
                                amnt = ElementAmount / DaysCnt;
                            }
                            #endregion
                        }
                    }
                    #endregion

                    if (amnt != 0 || ElementInfo.emprAmount != 0)
                    {
                        string baseValType = "";
                        decimal baseValue = 0.0M;
                        decimal baseCalculatedOn = 0.0M;
                        baseValType = ElementInfo.ValueType;
                        baseValue = ElementInfo.Value;
                        if (baseValType == "POB")
                        {
                            baseCalculatedOn = (decimal)(oEmp.BasicSalary / intGetMonthDays) * WorkingDays;
                        }
                        if (baseValType == "POG")
                        {
                            if (!Convert.ToBoolean(OneElement.MstElements.FlgVGross))
                            {
                                baseCalculatedOn = (grossSalary / intGetMonthDays) * WorkingDays;
                            }
                            else
                            {
                                if (payRatioWithLeaves == 1)
                                {
                                    baseCalculatedOn = ((grossSalary / DaysCnt) / WorkingDays) * LeaveCount;
                                }
                                else
                                {
                                    baseCalculatedOn = (grossSalary / DaysCnt) * payRatioWithLeaves;
                                }
                            }

                        }
                        if (baseValType == "FIX")
                        {
                            if (OneElement.MstElements.Type.Trim() != "Non-Rec")
                            {
                                baseCalculatedOn = (((decimal)ElementInfo.Amount / intGetMonthDays) * WorkingDays); //(decimal)ElementInfo.Amount;
                                baseValue = 100.00M;
                            }
                        }
                        if (OneElement.MstElements.MstElementEarning[0].FlgVariableValue == true)
                        {
                            if (oEmp.CfgPayrollDefination.WorkDays > 0)
                            {

                                amnt = Convert.ToDecimal(amnt * DaysCnt / oEmp.CfgPayrollDefination.WorkDays);

                            }
                            else
                            {
                                Int32 ActualDays = (Convert.ToDateTime(period.EndDate) - Convert.ToDateTime(period.StartDate)).Days + 1;
                                if (ActualDays > 0)
                                {
                                    amnt = Convert.ToDecimal(amnt * DaysCnt / ActualDays);
                                }
                            }
                        }
                        if (OneElement.MstElements.MstElementEarning[0].FlgLeaveEncashment == true)
                        {
                            amnt = OneElement.Amount.Value;
                        }
                        if (OneElement.MstElements.Type == "Rec" && OneElement.MstElements.MstElementEarning[0].FlgPropotionate == true)
                        {
                            amnt = amnt * payRatio;
                        }
                        //Shift On Off Working
                        if (Convert.ToBoolean(OneElement.MstElements.FlgShiftDays) && !string.IsNullOrEmpty(oEmp.ShiftDaysCode))
                        {
                            amnt = amnt * DaysCnt / WorkingDays;
                        }
                        if (OneElement.MstElements.MstElementEarning[0].FlgNotTaxable == true)
                        {
                            taxableAmnt = 0.00M;
                        }
                        else
                        {
                            taxableAmnt = amnt;
                        }
                        OneElement.FlgOneTimeConsumed = true;
                        DataRow dr = dtOut.NewRow();
                        dr["LineType"] = "Element";
                        dr["LineSubType"] = OneElement.MstElements.ElementName;
                        dr["LineValue"] = amnt;
                        dr["LineMemo"] = OneElement.MstElements.Description;
                        dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                        dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                        dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                        dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();
                        dr["LineBaseEntry"] = OneElement.Id;
                        dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                        dr["BaseValue"] = baseValue;
                        dr["BaseValueType"] = baseValType;
                        dr["TaxbleAmnt"] = taxableAmnt;
                        dr["NRTaxbleAmnt"] = 0.00M;
                        if (OneElement.MstElements.Type == "Non-Rec")
                        {
                            dr["NRTaxbleAmnt"] = taxableAmnt;
                        }
                        dtOut.Rows.Add(dr);
                    }
                }
            }
            return dtOut;
        }

        public DataTable ElementsProcessingDeductions(MstEmployee oEmp, CfgPeriodDates period, decimal days, decimal grossSalary, MstGLDetermination emGl, decimal payRatio, decimal payRatioWithLeaves, decimal LeaveCount, decimal WorkingDays, int pType)
        {
            decimal amnt = 0.0M;
            decimal DaysCnt = days;
            decimal taxableAmnt = 0.00M;
            string empContractType = oEmp.EmployeeContractType;
            string eleName = string.Empty;
            decimal EmployerContributionRatio = 1;
            Hashtable elementGls = new Hashtable();
            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");
            dtOut.Columns.Add("NRTaxbleAmnt");
            var oElementEarnings = (from p in oDB.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == oEmp.ID && p.FlgActive == true && p.MstElements.FlgProcessInPayroll == true && p.MstElements.ElmtType == "Ded" && (p.MstElements.Type == "Rec" || (p.PeriodId == period.ID && (p.FlgOneTimeConsumed == null ? false : Convert.ToBoolean(p.FlgOneTimeConsumed)) != true)) select p).ToList();
            if (oElementEarnings.Count > 0)
            {
                if (empContractType == "PROB")
                {
                    oElementEarnings = oElementEarnings.Where(a => a.MstElements.FlgProbationApplicable == true).ToList();
                }
                foreach (var OneElement in oElementEarnings)
                {
                    Int32 ElementLinkCheck = (from a in oDB.MstElementLink where a.ElementID == OneElement.ElementId && a.PayrollID == oEmp.PayrollID && a.FlgActive == true select a).Count();
                    if (ElementLinkCheck == 0) continue;

                    clsElement ElementInfo = new clsElement(oDB, OneElement, oEmp, grossSalary, pType);
                    amnt = (decimal)ElementInfo.Amount;
                    eleName = OneElement.MstElements.ElementName;
                    elementGls = getElementGL(oEmp, OneElement.MstElements, emGl);
                    if (amnt != 0 || ElementInfo.emprAmount != 0)
                    {
                        string baseValType = "";
                        decimal baseValue = 0.0M;
                        decimal baseCalculatedOn = 0.0M;
                        baseValType = ElementInfo.ValueType;
                        baseValue = ElementInfo.Value;
                        if (baseValType == "POB")
                        {
                            baseCalculatedOn = (decimal)oEmp.BasicSalary;
                        }
                        if (baseValType == "POG")
                        {
                            if (!Convert.ToBoolean(OneElement.MstElements.FlgVGross))
                            {
                                baseCalculatedOn = grossSalary;
                            }
                            else
                            {
                                if (payRatioWithLeaves == 1)
                                {
                                    baseCalculatedOn = (grossSalary / WorkingDays) * LeaveCount;
                                }
                                else
                                {
                                    baseCalculatedOn = grossSalary * payRatioWithLeaves;
                                }
                            }
                        }
                        if (baseValType == "FIX")
                        {
                            baseCalculatedOn = (decimal)ElementInfo.Amount;
                            baseValue = 100.00M;
                        }
                        //Per Day Basis Deduction Variable Gross
                        if (OneElement.MstElements.FlgVGross == true)
                        //if (OneElement.MstElements.MstElementDeduction[0].FlgVariableValue == true)
                        {
                            if (oEmp.CfgPayrollDefination.WorkDays > 0)
                            {
                                amnt = Convert.ToDecimal(amnt * DaysCnt / oEmp.CfgPayrollDefination.WorkDays);
                            }
                            else
                            {
                                Int32 ActualDays = (Convert.ToDateTime(period.EndDate) - Convert.ToDateTime(period.StartDate)).Days + 1;
                                if (ActualDays > 0)
                                {
                                    amnt = Convert.ToDecimal(amnt * DaysCnt / ActualDays);
                                }
                            }
                        }
                        //Per Day Basis 
                        if (OneElement.MstElements.Type == "Rec" && OneElement.MstElements.MstElementDeduction[0].FlgPropotionate == true)
                        {
                            amnt = amnt * payRatio;
                        }
                        //Shift On Days Off Days
                        if (Convert.ToBoolean(OneElement.MstElements.FlgShiftDays) && !string.IsNullOrEmpty(oEmp.ShiftDaysCode))
                        {
                            amnt = amnt * DaysCnt / WorkingDays;
                        }
                        amnt = -amnt;
                        taxableAmnt = 0.00M;
                        OneElement.FlgOneTimeConsumed = true;
                        DataRow dr = dtOut.NewRow();
                        dr["LineType"] = "Element";
                        dr["LineSubType"] = OneElement.MstElements.ElementName;
                        dr["LineValue"] = amnt;
                        dr["LineMemo"] = OneElement.MstElements.Description;
                        dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                        dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                        dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                        dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();
                        dr["LineBaseEntry"] = OneElement.Id;
                        dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                        dr["BaseValue"] = baseValue;
                        dr["BaseValueType"] = baseValType;
                        dr["TaxbleAmnt"] = taxableAmnt;
                        dr["NRTaxbleAmnt"] = 0.00M;
                        if (OneElement.MstElements.Type == "Non-Rec")
                        {
                            dr["NRTaxbleAmnt"] = taxableAmnt;
                        }
                        dtOut.Rows.Add(dr);
                    }
                }
            }
            return dtOut;
        }

        public DataTable ElementsProcessingDeductions_ForHalfMonth(MstEmployee oEmp, CfgPeriodDates period, decimal days, decimal grossSalary, MstGLDetermination emGl, decimal payRatio, decimal payRatioWithLeaves, decimal LeaveCount, decimal WorkingDays, int pType)
        {
            decimal amnt = 0.0M;
            decimal DaysCnt = days;
            DateTime dtGetMonthDays;
            Int32 intGetMonthDays;
            dtGetMonthDays = Convert.ToDateTime(period.StartDate);
            intGetMonthDays = DateTime.DaysInMonth(dtGetMonthDays.Year, dtGetMonthDays.Month);
            decimal taxableAmnt = 0.00M;
            string empContractType = oEmp.EmployeeContractType;
            string eleName = string.Empty;
            decimal EmployerContributionRatio = 1;
            Hashtable elementGls = new Hashtable();
            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");
            dtOut.Columns.Add("NRTaxbleAmnt");
            var oElementEarnings = (from p in oDB.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == oEmp.ID && p.FlgActive == true && p.MstElements.FlgProcessInPayroll == true && p.MstElements.ElmtType == "Ded" && (p.MstElements.Type == "Rec" || (p.PeriodId == period.ID && (p.FlgOneTimeConsumed == null ? false : Convert.ToBoolean(p.FlgOneTimeConsumed)) != true)) select p).ToList();
            if (oElementEarnings.Count > 0)
            {
                if (empContractType == "PROB")
                {
                    oElementEarnings = oElementEarnings.Where(a => a.MstElements.FlgProbationApplicable == true).ToList();
                }
                foreach (var OneElement in oElementEarnings)
                {
                    Int32 ElementLinkCheck = (from a in oDB.MstElementLink where a.ElementID == OneElement.ElementId && a.PayrollID == oEmp.PayrollID && a.FlgActive == true select a).Count();
                    if (ElementLinkCheck == 0) continue;

                    clsElement ElementInfo = new clsElement(oDB, OneElement, oEmp, grossSalary, pType);
                    if (OneElement.MstElements.Type.Trim() != "Non-Rec")
                    {
                        amnt = (decimal)(ElementInfo.Amount / intGetMonthDays) * WorkingDays;
                    }
                    else
                    {
                        amnt = (decimal)(ElementInfo.Amount);
                    }
                    eleName = OneElement.MstElements.ElementName;
                    elementGls = getElementGL(oEmp, OneElement.MstElements, emGl);
                    if (amnt != 0 || ElementInfo.emprAmount != 0)
                    {
                        string baseValType = "";
                        decimal baseValue = 0.0M;
                        decimal baseCalculatedOn = 0.0M;
                        baseValType = ElementInfo.ValueType;
                        baseValue = ElementInfo.Value;
                        if (baseValType == "POB")
                        {
                            baseCalculatedOn = (decimal)(oEmp.BasicSalary / intGetMonthDays) * WorkingDays;
                        }
                        if (baseValType == "POG")
                        {
                            if (!Convert.ToBoolean(OneElement.MstElements.FlgVGross))
                            {
                                baseCalculatedOn = (grossSalary / intGetMonthDays) * WorkingDays;
                            }
                            else
                            {
                                if (payRatioWithLeaves == 1)
                                {
                                    baseCalculatedOn = ((grossSalary / DaysCnt) / WorkingDays) * LeaveCount;
                                }
                                else
                                {
                                    baseCalculatedOn = (grossSalary / DaysCnt) * payRatioWithLeaves;
                                }
                            }

                        }
                        if (baseValType == "FIX")
                        {
                            if (OneElement.MstElements.Type.Trim() != "Non-Rec")
                            {
                                baseCalculatedOn = (((decimal)ElementInfo.Amount / intGetMonthDays) * WorkingDays); //(decimal)ElementInfo.Amount;
                                baseValue = 100.00M;
                            }
                        }
                        //Per Day Basis Deduction Variable Gross
                        if (OneElement.MstElements.FlgVGross == true)
                        //if (OneElement.MstElements.MstElementDeduction[0].FlgVariableValue == true)
                        {
                            if (oEmp.CfgPayrollDefination.WorkDays > 0)
                            {
                                amnt = Convert.ToDecimal(amnt * DaysCnt / oEmp.CfgPayrollDefination.WorkDays);
                            }
                            else
                            {
                                Int32 ActualDays = (Convert.ToDateTime(period.EndDate) - Convert.ToDateTime(period.StartDate)).Days + 1;
                                if (ActualDays > 0)
                                {
                                    amnt = Convert.ToDecimal(amnt * DaysCnt / ActualDays);
                                }
                            }
                        }
                        //Per Day Basis 
                        if (OneElement.MstElements.Type == "Rec" && OneElement.MstElements.MstElementDeduction[0].FlgPropotionate == true)
                        {
                            amnt = amnt * payRatio;
                        }
                        //Shift On Days Off Days
                        if (Convert.ToBoolean(OneElement.MstElements.FlgShiftDays) && !string.IsNullOrEmpty(oEmp.ShiftDaysCode))
                        {
                            amnt = amnt * DaysCnt / WorkingDays;
                        }
                        amnt = -amnt;
                        taxableAmnt = 0.00M;
                        OneElement.FlgOneTimeConsumed = true;
                        DataRow dr = dtOut.NewRow();
                        dr["LineType"] = "Element";
                        dr["LineSubType"] = OneElement.MstElements.ElementName;
                        dr["LineValue"] = amnt;
                        dr["LineMemo"] = OneElement.MstElements.Description;
                        dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                        dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                        dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                        dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();
                        dr["LineBaseEntry"] = OneElement.Id;
                        dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                        dr["BaseValue"] = baseValue;
                        dr["BaseValueType"] = baseValType;
                        dr["TaxbleAmnt"] = taxableAmnt;
                        dr["NRTaxbleAmnt"] = 0.00M;
                        if (OneElement.MstElements.Type == "Non-Rec")
                        {
                            dr["NRTaxbleAmnt"] = taxableAmnt;
                        }
                        dtOut.Rows.Add(dr);
                    }
                }
            }
            return dtOut;
        }

        public DataTable ElementsProcessingContributions(MstEmployee oEmp, CfgPeriodDates period, decimal days, decimal grossSalary, MstGLDetermination emGl, decimal payRatio, decimal payRatioWithLeaves, decimal LeaveCount, decimal WorkingDays, int pType)
        {
            decimal amnt = 0.0M;
            decimal DaysCnt = days;
            decimal taxableAmnt = 0.00M;
            string empContractType = oEmp.EmployeeContractType;
            string eleName = string.Empty;
            decimal EmployerContributionRatio = 1;
            decimal maxAppAmount = 0.0M;
            decimal emprTaxDiscount = 0.00M;
            Hashtable elementGls = new Hashtable();
            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");
            dtOut.Columns.Add("NRTaxbleAmnt");
            var oElementEarnings = (from p in oDB.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == oEmp.ID && p.FlgActive == true && p.MstElements.FlgProcessInPayroll == true && p.MstElements.ElmtType == "Con" && (p.MstElements.Type == "Rec" || (p.PeriodId == period.ID && (p.FlgOneTimeConsumed == null ? false : Convert.ToBoolean(p.FlgOneTimeConsumed)) != true)) select p).ToList();
            if (oElementEarnings.Count > 0)
            {
                if (empContractType == "PROB")
                {
                    oElementEarnings = oElementEarnings.Where(a => a.MstElements.FlgProbationApplicable == true).ToList();
                }
                foreach (var OneElement in oElementEarnings)
                {
                    Int32 ElementLinkCheck = (from a in oDB.MstElementLink where a.ElementID == OneElement.ElementId && a.PayrollID == oEmp.PayrollID && a.FlgActive == true select a).Count();
                    if (ElementLinkCheck == 0) continue;
                    MstElementContribution ContributionElement = OneElement.MstElements.MstElementContribution.FirstOrDefault();
                    //Days Check Validation
                    Int32 EmployeeDays = (Convert.ToDateTime(period.StartDate) - Convert.ToDateTime(oEmp.JoiningDate)).Days;
                    if (ContributionElement.DaysRangeFrom >= 0 && ContributionElement.DaysRangeTo > 0)
                    {
                        if (EmployeeDays >= ContributionElement.DaysRangeFrom && EmployeeDays <= ContributionElement.DaysRangeTo)
                        {
                        }
                        else
                        {
                            continue;
                        }
                    }
                    //Salary Check Validation
                    if (ContributionElement.SalaryRangeFrom >= 0 && ContributionElement.SalaryRangeTo > 0)
                    {
                        if (ContributionElement.ValidOnSalary == "0")
                        {
                            if (oEmp.BasicSalary >= ContributionElement.SalaryRangeFrom && oEmp.BasicSalary <= ContributionElement.SalaryRangeTo)
                            {
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            decimal empGrossSalary = getEmpGross(oEmp);
                            if (empGrossSalary >= ContributionElement.SalaryRangeFrom && empGrossSalary <= ContributionElement.SalaryRangeTo)
                            {
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }

                    //Element Check Validation
                    clsElement ElementInfo = new clsElement(oDB, OneElement, oEmp, grossSalary, pType);
                    amnt = (decimal)ElementInfo.Amount;
                    eleName = OneElement.MstElements.ElementName;
                    elementGls = getElementGL(oEmp, OneElement.MstElements, emGl);
                    if (amnt != 0 || ElementInfo.emprAmount != 0)
                    {
                        string baseValType = "";
                        decimal baseValue = 0.0M;
                        decimal baseCalculatedOn = 0.0M;
                        baseValType = ElementInfo.ValueType;
                        baseValue = ElementInfo.Value;
                        if (baseValType == "POB")
                        {
                            baseCalculatedOn = (decimal)oEmp.BasicSalary;
                        }
                        if (baseValType == "POG")
                        {
                            if (!Convert.ToBoolean(OneElement.MstElements.FlgVGross))
                            {
                                baseCalculatedOn = grossSalary;
                            }
                            else
                            {
                                if (payRatioWithLeaves == 1)
                                {
                                    baseCalculatedOn = (grossSalary / WorkingDays) * LeaveCount;
                                }
                                else
                                {
                                    baseCalculatedOn = grossSalary * payRatioWithLeaves;
                                }
                            }
                        }
                        if (baseValType == "FIX")
                        {
                            baseCalculatedOn = (decimal)ElementInfo.Amount;
                            baseValue = 100.00M;
                        }
                        maxAppAmount = 0.0M;

                        if (ContributionElement != null)
                        {
                            int elementID = ContributionElement.ElementId.Value;
                            maxAppAmount = ContributionElement.MaxAppAmount == null ? 0.0M : ContributionElement.MaxAppAmount.Value;
                        }
                        if (maxAppAmount > 0 && maxAppAmount <= baseCalculatedOn)
                        {
                            amnt = OneElement.MstElements.MstElementContribution[0].MaxEmployeeContribution.Value;
                        }
                        else if (Convert.ToBoolean(OneElement.MstElements.FlgVGross))
                        {
                            amnt = baseCalculatedOn * (Convert.ToDecimal(OneElement.Value) / 100);
                        }
                        else if (OneElement.MstElements.FlgConBatch != null && Convert.ToBoolean(OneElement.MstElements.FlgConBatch))
                        {
                            if (baseValType == "FIX")
                            {
                                amnt = baseCalculatedOn;
                            }
                            else
                            {
                                amnt = baseCalculatedOn * (Convert.ToDecimal(OneElement.EmpContr) / 100);
                            }
                        }
                        if (OneElement.MstElements.MstElementContribution[0].FlgVariableValue == true)
                        {
                            if (oEmp.CfgPayrollDefination.WorkDays > 0)
                            {
                                amnt = Convert.ToDecimal(amnt * DaysCnt / oEmp.CfgPayrollDefination.WorkDays);
                            }
                            else
                            {
                                Int32 ActualDays = (Convert.ToDateTime(period.EndDate) - Convert.ToDateTime(period.StartDate)).Days + 1;
                                if (ActualDays > 0)
                                {
                                    EmployerContributionRatio = Convert.ToDecimal(DaysCnt / ActualDays);
                                    amnt = Convert.ToDecimal(amnt * DaysCnt / ActualDays);
                                }
                            }
                        }
                        //Shift On  Days Off Days
                        if (Convert.ToBoolean(OneElement.MstElements.FlgShiftDays) && !string.IsNullOrEmpty(oEmp.ShiftDaysCode))
                        {
                            amnt = amnt * DaysCnt / WorkingDays;
                        }
                        amnt = -amnt;
                        taxableAmnt = 0.00M;
                        //Tax Working On Employee Side
                        decimal ExemptedAmount = OneElement.MstElements.MstElementContribution[0].ContTaxDiscount == null ? 0M : (decimal)OneElement.MstElements.MstElementContribution[0].ContTaxDiscount;
                        if (ExemptedAmount > 0)
                        {
                            if(Math.Abs(amnt) > ExemptedAmount)
                            {
                                taxableAmnt = amnt + ExemptedAmount;
                            }
                            else
                            {
                                taxableAmnt = 0;
                            }
                        }
                        else
                        {
                            taxableAmnt = amnt;
                        }
                        //End of Tax Working.
                        OneElement.FlgOneTimeConsumed = true;
                        DataRow dr = dtOut.NewRow();
                        dr["LineType"] = "Element";
                        dr["LineSubType"] = OneElement.MstElements.ElementName;
                        dr["LineValue"] = amnt;
                        dr["LineMemo"] = OneElement.MstElements.Description;
                        dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                        dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                        dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                        dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();
                        dr["LineBaseEntry"] = OneElement.Id;
                        dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                        dr["BaseValue"] = baseValue;
                        dr["BaseValueType"] = baseValType;
                        dr["TaxbleAmnt"] = taxableAmnt;
                        dr["NRTaxbleAmnt"] = 0.00M;
                        if (OneElement.MstElements.Type == "Non-Rec")
                        {
                            dr["NRTaxbleAmnt"] = taxableAmnt;
                        }
                        dtOut.Rows.Add(dr);
                        emprTaxDiscount = 0.00M;
                        if (OneElement.MstElements.MstElementContribution[0].ContTaxDiscount != null)
                        {
                            emprTaxDiscount = (decimal)OneElement.MstElements.MstElementContribution[0].ContTaxDiscount;
                            if (emprTaxDiscount > 0 && ElementInfo.emprAmount > emprTaxDiscount)
                            {
                                taxableAmnt = ElementInfo.emprAmount - emprTaxDiscount;
                            }
                            else if (emprTaxDiscount == 0)
                            {
                                taxableAmnt = ElementInfo.emprAmount;
                            }
                            //Modified BY Zeeshan
                            //if (OneElement.MstElements.MstElementContribution[0].MaxAppAmount <= baseCalculatedOn)
                            //{
                            //    taxableAmnt = 500 - emprTaxDiscount / 12;
                            //}
                            //End Of Modification
                        }
                        //if (taxableAmnt < 0) taxableAmnt = 0.00M;
                        decimal contrAmnt = 0;
                        if (OneElement.MstElements.MstElementContribution[0].MaxAppAmount > 0 && OneElement.MstElements.MstElementContribution[0].MaxAppAmount <= baseCalculatedOn)
                        {
                            contrAmnt = OneElement.MstElements.MstElementContribution[0].MaxEmployerContribution.Value;
                        }
                        else
                        {
                            if (OneElement.MstElements.FlgConBatch != null && Convert.ToBoolean(OneElement.MstElements.FlgConBatch))
                            {
                                if (baseValType.ToUpper() == "FIX")
                                {
                                    //contrAmnt = baseCalculatedOn;
                                    contrAmnt = ElementInfo.emprAmount;
                                }
                                else
                                {
                                    contrAmnt = baseCalculatedOn * (Convert.ToDecimal(OneElement.EmplrContr) / 100);
                                }
                            }
                            else
                            {
                                //dr["LineValue"] = eleInfo.emprAmount * EmployerContributionRatio;
                                if (!Convert.ToBoolean(OneElement.MstElements.FlgVGross))
                                {
                                    //baseCalculatedOn = grossSalary;
                                    contrAmnt = ElementInfo.emprAmount * EmployerContributionRatio;
                                }
                                else
                                {
                                    if (payRatioWithLeaves == 1)
                                    {
                                        //baseCalculatedOn = (grossSalary / WorkingDays) * LeaveCount;
                                        contrAmnt = (ElementInfo.emprAmount / WorkingDays) * LeaveCount;

                                    }
                                    else
                                    {
                                        //baseCalculatedOn = grossSalary * payRatioWithLeaves;
                                        contrAmnt = ElementInfo.emprAmount * payRatioWithLeaves;
                                    }
                                }
                            }
                        }
                        //Shift On Days Off Days
                        if (Convert.ToBoolean(OneElement.MstElements.FlgShiftDays) && !string.IsNullOrEmpty(oEmp.ShiftDaysCode))
                        {
                            contrAmnt = contrAmnt * DaysCnt / WorkingDays;
                        }
                        dr = dtOut.NewRow();
                        dr["LineType"] = "Element";
                        dr["LineSubType"] = "Empr Cont";
                        dr["LineValue"] = Math.Round(contrAmnt, 0, MidpointRounding.AwayFromZero);
                        dr["LineMemo"] = "Empr Contribution " + OneElement.MstElements.Description;
                        dr["DebitAccount"] = elementGls["EmprDrAcct"].ToString();
                        dr["CreditAccount"] = elementGls["EmprCrAcct"].ToString();
                        dr["DebitAccountName"] = elementGls["EmprDrAcctName"].ToString();
                        dr["CreditAccountName"] = elementGls["EmprCrAcctName"].ToString();
                        dr["LineBaseEntry"] = OneElement.Id;
                        dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                        dr["BaseValue"] = baseValue;
                        dr["BaseValueType"] = baseValType;
                        dr["TaxbleAmnt"] = 0.0M;
                        dr["NRTaxbleAmnt"] = 0.00;
                        dtOut.Rows.Add(dr);

                    }
                }
            }
            return dtOut;
        }

        public DataTable ElementsContributionsCalculationBasedOnEarnedSalary(MstEmployee oEmp, CfgPeriodDates period, decimal days, decimal grossSalary, MstGLDetermination emGl, decimal payRatio, decimal payRatioWithLeaves, decimal LeaveCount, decimal WorkingDays, int pType)
        {
            decimal EmprValue = 0.0M;
            decimal EmpValue = 0.0M;
            decimal amnt = 0.0M;
            decimal DaysCnt = days;
            decimal taxableAmnt = 0.00M;
            string empContractType = oEmp.EmployeeContractType;
            string eleName = string.Empty;
            decimal EmployerContributionRatio = 1;
            decimal maxAppAmount = 0.0M;
            decimal emprTaxDiscount = 0.00M;
            Hashtable elementGls = new Hashtable();
            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");
            dtOut.Columns.Add("NRTaxbleAmnt");
            var oElementEarnings = (from p in oDB.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == oEmp.ID && p.FlgActive == true && p.MstElements.FlgProcessInPayroll == true && p.MstElements.ElmtType == "Con" && (p.MstElements.Type == "Rec" || (p.PeriodId == period.ID && (p.FlgOneTimeConsumed == null ? false : Convert.ToBoolean(p.FlgOneTimeConsumed)) != true)) select p).ToList();
            if (oElementEarnings.Count > 0)
            {
                if (empContractType == "PROB")
                {
                    oElementEarnings = oElementEarnings.Where(a => a.MstElements.FlgProbationApplicable == true).ToList();
                }
                foreach (var OneElement in oElementEarnings)
                {
                    Int32 ElementLinkCheck = (from a in oDB.MstElementLink where a.ElementID == OneElement.ElementId && a.PayrollID == oEmp.PayrollID && a.FlgActive == true select a).Count();
                    if (ElementLinkCheck == 0) continue;
                    MstElementContribution ContributionElement = OneElement.MstElements.MstElementContribution.FirstOrDefault();
                    //Days Check Validation
                    Int32 EmployeeDays = (Convert.ToDateTime(period.StartDate) - Convert.ToDateTime(oEmp.JoiningDate)).Days;
                    if (ContributionElement.DaysRangeFrom >= 0 && ContributionElement.DaysRangeTo > 0)
                    {
                        if (EmployeeDays >= ContributionElement.DaysRangeFrom && EmployeeDays <= ContributionElement.DaysRangeTo)
                        {
                        }
                        else
                        {
                            continue;
                        }
                    }
                    //Salary Check Validation
                    if (ContributionElement.SalaryRangeFrom >= 0 && ContributionElement.SalaryRangeTo > 0)
                    {
                        if (ContributionElement.ValidOnSalary == "0")
                        {
                            if (oEmp.BasicSalary >= ContributionElement.SalaryRangeFrom && oEmp.BasicSalary <= ContributionElement.SalaryRangeTo)
                            {
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            decimal empGrossSalary = getEmpGross(oEmp);
                            if (empGrossSalary >= ContributionElement.SalaryRangeFrom && empGrossSalary <= ContributionElement.SalaryRangeTo)
                            {
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }

                    //Element Check Validation
                    clsElement ElementInfo = new clsElement(oDB, OneElement, oEmp, grossSalary, pType);
                    amnt = (decimal)ElementInfo.Amount;
                    eleName = OneElement.MstElements.ElementName;
                    elementGls = getElementGL(oEmp, OneElement.MstElements, emGl);
                    //if (amnt != 0 || ElementInfo.emprAmount != 0)
                    //{
                    string baseValType = "";
                    decimal baseValue = 0.0M;
                    decimal baseCalculatedOn = 0.0M;
                    decimal EarnedSalary = 0.0M;
                    baseValType = ElementInfo.ValueType;
                    baseValue = ElementInfo.Value;

                    maxAppAmount = 0.0M;

                    if (ContributionElement != null)
                    {
                        int elementID = ContributionElement.ElementId.Value;
                        maxAppAmount = ContributionElement.MaxAppAmount == null ? 0.0M : ContributionElement.MaxAppAmount.Value;
                        EmprValue = ContributionElement.Employer == null ? 0.0M : ContributionElement.Employer.Value;
                        EmpValue = ContributionElement.Employee == null ? 0.0M : ContributionElement.Employee.Value;
                    }
                    if (baseValType == "POB")
                    {
                        baseCalculatedOn = (decimal)oEmp.BasicSalary;
                    }
                    if (baseValType == "POG")
                    {
                        if (!Convert.ToBoolean(OneElement.MstElements.FlgVGross))
                        {
                            baseCalculatedOn = grossSalary;
                        }
                        else
                        {
                            if (payRatioWithLeaves == 1)
                            {
                                baseCalculatedOn = (grossSalary / WorkingDays) * LeaveCount;
                            }
                            else
                            {
                                baseCalculatedOn = grossSalary * payRatioWithLeaves;
                            }
                        }
                    }
                    if (baseValType == "FIX")
                    {
                        EarnedSalary = (maxAppAmount / WorkingDays) * LeaveCount; ;//(decimal)ElementInfo.Amount;
                        baseValue = 100.00M;
                    }

                    if (maxAppAmount > 0 && maxAppAmount <= baseCalculatedOn)
                    {
                        if (OneElement.MstElements.MstElementContribution[0].FlgVariableValue == true)
                        {
                            if (oEmp.CfgPayrollDefination.WorkDays > 0)
                            {
                                EarnedSalary = Convert.ToDecimal(baseCalculatedOn * DaysCnt / oEmp.CfgPayrollDefination.WorkDays);
                            }
                            else
                            {
                                Int32 ActualDays = (Convert.ToDateTime(period.EndDate) - Convert.ToDateTime(period.StartDate)).Days + 1;
                                if (ActualDays > 0)
                                {
                                    EmployerContributionRatio = Convert.ToDecimal(DaysCnt / ActualDays);

                                    EarnedSalary = Convert.ToDecimal(baseCalculatedOn * DaysCnt / ActualDays);
                                }
                            }
                        }
                    }
                    if (EarnedSalary >= maxAppAmount)
                    {
                        amnt = (maxAppAmount / 100) * EmpValue;
                    }
                    else if (EarnedSalary < maxAppAmount)
                    {
                        amnt = (EarnedSalary / 100) * EmpValue;
                    }
                    amnt = -amnt;
                    taxableAmnt = 0.00M;
                    OneElement.FlgOneTimeConsumed = true;
                    DataRow dr = dtOut.NewRow();
                    dr["LineType"] = "Element";
                    dr["LineSubType"] = OneElement.MstElements.ElementName;
                    dr["LineValue"] = amnt;
                    dr["LineMemo"] = OneElement.MstElements.Description;
                    dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                    dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                    dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                    dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();
                    dr["LineBaseEntry"] = OneElement.Id;
                    dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                    dr["BaseValue"] = baseValue;
                    dr["BaseValueType"] = baseValType;
                    dr["TaxbleAmnt"] = taxableAmnt;
                    dr["NRTaxbleAmnt"] = 0.00M;
                    if (OneElement.MstElements.Type == "Non-Rec")
                    {
                        dr["NRTaxbleAmnt"] = taxableAmnt;
                    }
                    dtOut.Rows.Add(dr);
                    emprTaxDiscount = 0.00M;
                    if (OneElement.MstElements.MstElementContribution[0].ContTaxDiscount != null)
                    {
                        emprTaxDiscount = (decimal)OneElement.MstElements.MstElementContribution[0].ContTaxDiscount;
                        if (emprTaxDiscount > 0 && ElementInfo.emprAmount > emprTaxDiscount)
                        {
                            taxableAmnt = ElementInfo.emprAmount - emprTaxDiscount;
                        }
                        else if (emprTaxDiscount == 0)
                        {
                            taxableAmnt = ElementInfo.emprAmount;
                        }
                    }
                    decimal contrAmnt = 0;
                    if (OneElement.MstElements.MstElementContribution[0].MaxAppAmount > 0 && OneElement.MstElements.MstElementContribution[0].MaxAppAmount <= baseCalculatedOn)
                    {
                        contrAmnt = OneElement.MstElements.MstElementContribution[0].MaxEmployerContribution.Value;
                    }
                    else
                    {
                        if (OneElement.MstElements.FlgConBatch != null && Convert.ToBoolean(OneElement.MstElements.FlgConBatch))
                        {
                            if (baseValType.ToUpper() == "FIX")
                            {
                                contrAmnt = ElementInfo.emprAmount;
                            }
                            else
                            {
                                contrAmnt = baseCalculatedOn * (Convert.ToDecimal(OneElement.EmplrContr) / 100);
                            }
                        }
                        else
                        {
                            if (!Convert.ToBoolean(OneElement.MstElements.FlgVGross))
                            {
                                contrAmnt = ElementInfo.emprAmount * EmployerContributionRatio;
                            }
                            else
                            {
                                if (payRatioWithLeaves == 1)
                                {
                                    contrAmnt = (ElementInfo.emprAmount / WorkingDays) * LeaveCount;
                                }
                                else
                                {
                                    contrAmnt = ElementInfo.emprAmount * payRatioWithLeaves;
                                }
                            }
                        }
                    }
                    //Shift On Days Off Days
                    if (Convert.ToBoolean(OneElement.MstElements.FlgShiftDays) && !string.IsNullOrEmpty(oEmp.ShiftDaysCode))
                    {
                        contrAmnt = contrAmnt * DaysCnt / WorkingDays;
                    }
                    if (EarnedSalary >= maxAppAmount)
                    {
                        amnt = (maxAppAmount / 100) * EmprValue;
                    }
                    else if (EarnedSalary < maxAppAmount)
                    {
                        amnt = (EarnedSalary / 100) * EmprValue;
                    }
                    dr = dtOut.NewRow();
                    dr["LineType"] = "Element";
                    dr["LineSubType"] = "Empr Cont";
                    dr["LineValue"] = Math.Round(amnt, 0, MidpointRounding.AwayFromZero);
                    dr["LineMemo"] = "Empr Contribution " + OneElement.MstElements.Description;
                    dr["DebitAccount"] = elementGls["EmprDrAcct"].ToString();
                    dr["CreditAccount"] = elementGls["EmprCrAcct"].ToString();
                    dr["DebitAccountName"] = elementGls["EmprDrAcctName"].ToString();
                    dr["CreditAccountName"] = elementGls["EmprCrAcctName"].ToString();
                    dr["LineBaseEntry"] = OneElement.Id;
                    dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                    dr["BaseValue"] = baseValue;
                    dr["BaseValueType"] = baseValType;
                    dr["TaxbleAmnt"] = taxableAmnt;
                    dr["NRTaxbleAmnt"] = 0.00;
                    dtOut.Rows.Add(dr);

                    //}
                }
            }
            return dtOut;
        }

        public DataTable GratuityCalculations(MstEmployee oEmp, decimal MonthDays, CfgPeriodDates oPeriod)
        {
            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");
            dtOut.Columns.Add("NRTaxbleAmnt");
            DateTime GratuityPeriodStartDate, GratuityPeriodEndDate;
            TimeSpan DifferencTime;
            double DifferenceInDays = 0;
            decimal SlabRating = 0, PerValue = 0;
            decimal SlabDays = 0, CalculationDays = 0;
            decimal GratuityAmount = 0;
            decimal LeavesCount = 0;
            decimal EmpGross = 0, EmpBasic = 0;
            Hashtable elementGls = new Hashtable();
            decimal PerDayGross = 0, PerDayBasic = 0, PayrollDays = 0, PerMonthProvision = 0m;
            try
            {
                if (oEmp.GratuitySlabs != null)
                {
                    #region Gratuity Provision
                    //now check if employee is eligle or not
                    GratuityPeriodStartDate = Convert.ToDateTime(oEmp.JoiningDate);
                    GratuityPeriodEndDate = Convert.ToDateTime(oPeriod.EndDate);
                    DifferencTime = GratuityPeriodEndDate - GratuityPeriodStartDate;
                    DifferenceInDays = DifferencTime.TotalDays;
                    if ((oEmp.TrnsGratuitySlabs.FlgWOPLeaves != null ? oEmp.TrnsGratuitySlabs.FlgWOPLeaves : false) == true)
                    {
                        LeavesCount = Convert.ToDecimal((from a in oDB.TrnsLeavesRequest where a.MstEmployee.EmpID == oEmp.EmpID && a.MstLeaveType.LeaveType.ToLower() == "ded" select a.TotalCount).Sum() ?? 0M);
                    }
                    if ((oEmp.TrnsGratuitySlabs.FlgAbsoluteYears != null ? oEmp.TrnsGratuitySlabs.FlgAbsoluteYears : false) == true)
                    {
                        SlabRating = Math.Round(Convert.ToDecimal((((decimal)DifferenceInDays - LeavesCount) / 365)));
                    }
                    else
                    {
                        SlabRating = Convert.ToDecimal((((decimal)DifferenceInDays - LeavesCount) / 365));
                    }

                    //var oSlab = (from a in oDB.TrnsGratuitySlabsDetail 
                    //             where a.TrnsGratuitySlabs.MstEmployee[0].EmpID == pEmp.EmpID &&
                    //             a.FromPoints <= SlabRating && (a.ToPoints != 0 ? a.ToPoints : 100M) >= SlabRating
                    //             select a).FirstOrDefault();
                    var oSlab = (from Detail in oDB.TrnsGratuitySlabsDetail
                                 join Head in oDB.TrnsGratuitySlabs on Detail.FKID equals Head.InternalID
                                 where Head.InternalID == oEmp.GratuitySlabs &&
                                 Detail.FromPoints <= SlabRating && (Detail.ToPoints != 0 ? Detail.ToPoints : 100M) >= SlabRating
                                 select new { Description = Detail.Description, Days = Detail.DaysCount, HeaderDays = Head.CalculatedDays }).FirstOrDefault();

                    if (oSlab != null)
                    {
                        SlabDays = Convert.ToDecimal(oSlab.Days);
                        CalculationDays = Convert.ToDecimal(oSlab.HeaderDays);
                        PerValue = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.BasedOnValue);
                        if (oEmp.TrnsGratuitySlabs.BasedOn == "0") //Basic
                        {
                            EmpBasic = Convert.ToDecimal(oEmp.BasicSalary);
                            if (CalculationDays == 0)
                            {
                                if (oEmp.PayrollID != null)
                                {
                                    PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                }
                                if (PayrollDays == 0)
                                {
                                    TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                    PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                }
                            }
                            else
                            {
                                PayrollDays = CalculationDays;
                            }
                            if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                            {
                                PerDayBasic = ((EmpBasic / 100) * PerValue) * 12 / PayrollDays;
                                GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;
                                //PerMonthProvision = (GratuityAmount / PayrollDays) * MonthDays;
                                PerMonthProvision = ((PerDayBasic * SlabDays) / PayrollDays) * MonthDays;
                            }
                            else
                            {
                                PerDayBasic = ((EmpBasic / 100) * PerValue) / PayrollDays;
                                GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;
                                //PerMonthProvision = (GratuityAmount / 365) * MonthDays;
                                PerMonthProvision = ((PerDayBasic * SlabDays) / 365) * MonthDays;
                            }
                        }
                        else //Gross
                        {
                            if (CalculationDays == 0)
                            {
                                if (oEmp.PayrollID != null)
                                {
                                    PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                }
                                if (PayrollDays == 0)
                                {
                                    TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                    PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                }
                            }
                            else
                            {
                                PayrollDays = CalculationDays;
                            }
                            decimal empGrossValue = getEmpGross(oEmp);
                            if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                            {
                                PerDayGross = ((empGrossValue / 100) * PerValue) * 12 / PayrollDays;
                                GratuityAmount = (SlabRating * SlabDays) * PerDayGross;
                                //PerMonthProvision = (GratuityAmount / PayrollDays) * MonthDays;
                                PerMonthProvision = ((PerDayGross * SlabDays) / PayrollDays) * MonthDays;
                            }
                            else
                            {
                                PerDayGross = ((empGrossValue / 100) * PerValue) / PayrollDays;
                                GratuityAmount = (SlabRating * SlabDays) * PerDayGross;
                                //PerMonthProvision = (GratuityAmount / 365) * MonthDays;
                                PerMonthProvision = ((PerDayGross * SlabDays) / PayrollDays) * MonthDays;

                            }
                        }
                    }
                    DataRow dr = dtOut.NewRow();
                    dr["LineType"] = "Element";
                    dr["LineSubType"] = "Gratuity Provision";
                    dr["LineValue"] = PerMonthProvision;
                    dr["LineMemo"] = "Gratuity";
                    dr["DebitAccount"] = "";
                    dr["CreditAccount"] = "";
                    dr["DebitAccountName"] = "";
                    dr["CreditAccountName"] = "";
                    dr["LineBaseEntry"] = "";
                    dr["BaseValueCalculatedOn"] = oEmp.BasicSalary.ToString();
                    dr["BaseValue"] = GratuityAmount.ToString();
                    dr["BaseValueType"] = "";
                    dr["TaxbleAmnt"] = "";
                    dr["NRTaxbleAmnt"] = 0.00M;
                    dtOut.Rows.Add(dr);
                    //return dtOut;

                    #endregion

                    #region Provision Increment

                    var oLastSalary = (from a in oDB.TrnsSalaryProcessRegister
                                       where a.EmpID == oEmp.ID && a.PayrollPeriodID < oPeriod.ID
                                       orderby a.Id descending
                                       select a).FirstOrDefault(); // assuming last period salary.
                    if (oLastSalary == null) return dtOut; // when no last salary found
                    if (oEmp.BasicSalary != oLastSalary.EmpBasic)
                    {
                        //get previous gratuity amount & count
                        decimal PreviousProvisionGratuityValue = 0M;
                        foreach (var One in oLastSalary.TrnsSalaryProcessRegisterDetail)
                        {
                            if (One.LineMemo == "Gratuity")
                            {
                                PreviousProvisionGratuityValue = Convert.ToDecimal(One.BaseValue);
                            }
                        }
                        //get current gratuity amount.
                        decimal newValueProvision = GratuityAmount;
                        if (PreviousProvisionGratuityValue < newValueProvision)
                        {
                            DataRow dr1 = dtOut.NewRow();
                            dr1["LineType"] = "Element";
                            dr1["LineSubType"] = "Gratuity Provision";
                            dr1["LineValue"] = Math.Abs(newValueProvision - PreviousProvisionGratuityValue - PerMonthProvision);
                            dr1["LineMemo"] = "Gratuity Adj";
                            dr1["DebitAccount"] = "";
                            dr1["CreditAccount"] = "";
                            dr1["DebitAccountName"] = "";
                            dr1["CreditAccountName"] = "";
                            dr1["LineBaseEntry"] = "";
                            dr1["BaseValueCalculatedOn"] = newValueProvision.ToString();
                            dr1["BaseValue"] = PreviousProvisionGratuityValue.ToString();
                            dr1["BaseValueType"] = "FIX";
                            dr1["TaxbleAmnt"] = "0";
                            dr1["NRTaxbleAmnt"] = 0.00M;
                            dtOut.Rows.Add(dr1);
                        }
                    }
                    return dtOut;
                    #endregion
                }
                else
                {
                    return dtOut;
                }
            }
            catch (Exception ex)
            {
                return dtOut;
            }
        }

        public DataTable GratuitySlabWiseCalculations(MstEmployee oEmp, decimal MonthDays, CfgPeriodDates oPeriod, CfgPeriodDates oPrePeriod)
        {
            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");
            dtOut.Columns.Add("NRTaxbleAmnt");
            try
            {
                DateTime GratuityPeriodStartDate, GratuityPeriodEndDate;
                TimeSpan DifferencTime;
                double DifferenceInDays = 0;
                decimal SlabRating = 0, PerValue = 0;
                decimal SlabDays = 0, CalculationDays = 0;
                decimal GratuityAmount = 0;
                decimal LeavesCount = 0;
                decimal EmpGross = 0, EmpBasic = 0;
                decimal Slab1Calc = 0, Slab2Calc = 0, Slab3Calc = 0, Slab4Calc = 0, Slab5Calc = 0;
                DateTime FirstPeriodEndDate = DateTime.MinValue, SecondPeriodEndDate = DateTime.MinValue, ThirdPeriodEndDate = DateTime.MinValue, FourthPeriodEndDate = DateTime.MinValue, FifthPeriodEndDate = DateTime.MinValue;
                Hashtable elementGls = new Hashtable();
                decimal PerDayGross = 0, PerDayBasic = 0, PayrollDays = 0, PerMonthProvision = 0m;
                if (oEmp.GratuitySlabs != null)
                {

                    #region Gratuity Calculations
                    GratuityPeriodStartDate = Convert.ToDateTime(oEmp.JoiningDate);
                    GratuityPeriodEndDate = Convert.ToDateTime(oPeriod.EndDate);
                    DifferencTime = GratuityPeriodEndDate - GratuityPeriodStartDate;
                    DifferenceInDays = DifferencTime.TotalDays;
                    if ((oEmp.TrnsGratuitySlabs.FlgWOPLeaves != null ? oEmp.TrnsGratuitySlabs.FlgWOPLeaves : false) == true)
                    {
                        LeavesCount = Convert.ToDecimal((from a in oDB.TrnsLeavesRequest where a.MstEmployee.EmpID == oEmp.EmpID && a.MstLeaveType.LeaveType.ToLower() == "ded" select a.TotalCount).Sum() ?? 0M);
                    }
                    if ((oEmp.TrnsGratuitySlabs.FlgAbsoluteYears != null ? oEmp.TrnsGratuitySlabs.FlgAbsoluteYears : false) == true)
                    {
                        SlabRating = Math.Round(Convert.ToDecimal((((decimal)DifferenceInDays - LeavesCount) / 365)));
                    }
                    else
                    {
                        SlabRating = Convert.ToDecimal((((decimal)DifferenceInDays - LeavesCount) / 365));
                    }
                    var oSlab = (from a in oDB.TrnsGratuitySlabsDetail
                                 where a.TrnsGratuitySlabs.InternalID == oEmp.GratuitySlabs
                                 && a.FromPoints < SlabRating
                                 select a).ToList();
                    int i = 1;
                    foreach (var One in oSlab)
                    {

                        if (i == 1) //slab 1
                        {
                            #region Slab 1
                            double DaysDifference1stSlab = 0, ToValueSlab = Convert.ToDouble(One.ToPoints) * 365;
                            DateTime Gratuity1stPeriodStart, Gratuity1stPeriodEnd;
                            TimeSpan Gratuity1stSlabSpan;
                            DateTime FirstPeriodToFind = Convert.ToDateTime(oEmp.JoiningDate).AddDays(ToValueSlab);
                            CfgPeriodDates oFirstSlabEndDatePeriod = (from a in oDB.CfgPeriodDates
                                                                      where a.PayrollId == oEmp.PayrollID
                                                                      && a.StartDate <= FirstPeriodToFind && a.EndDate >= FirstPeriodToFind
                                                                      select a).FirstOrDefault();
                            if (oFirstSlabEndDatePeriod != null)
                            {
                                if ((DateTime)oPeriod.StartDate <= (DateTime)oFirstSlabEndDatePeriod.StartDate)
                                {
                                    Gratuity1stPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    Gratuity1stPeriodEnd = Convert.ToDateTime(oFirstSlabEndDatePeriod.EndDate);
                                }
                            }
                            else
                            {
                                if ((DateTime)oPeriod.StartDate < FirstPeriodToFind)
                                {
                                    Gratuity1stPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    Gratuity1stPeriodEnd = Convert.ToDateTime(oEmp.JoiningDate).AddDays((double)(One.ToPoints * 365));
                                }
                            }
                            Gratuity1stPeriodStart = Convert.ToDateTime(oEmp.JoiningDate);
                            Gratuity1stSlabSpan = Gratuity1stPeriodEnd - Gratuity1stPeriodStart;
                            DaysDifference1stSlab = Gratuity1stSlabSpan.TotalDays;
                            if ((oEmp.TrnsGratuitySlabs.FlgAbsoluteYears != null ? oEmp.TrnsGratuitySlabs.FlgAbsoluteYears : false) == true)
                            {
                                SlabRating = Math.Round(Convert.ToDecimal((((decimal)DaysDifference1stSlab - LeavesCount) / 365)));
                            }
                            else
                            {
                                SlabRating = Convert.ToDecimal((((decimal)DaysDifference1stSlab - LeavesCount) / 365));
                            }

                            #region Value Calculation
                            SlabDays = Convert.ToDecimal(One.DaysCount);
                            CalculationDays = Convert.ToDecimal(One.TrnsGratuitySlabs.CalculatedDays);
                            PerValue = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.BasedOnValue);
                            if (oEmp.TrnsGratuitySlabs.BasedOn == "0") //Basic
                            {
                                EmpBasic = Convert.ToDecimal(oEmp.BasicSalary);
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                                else
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                            }
                            else //Gross
                            {
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                decimal empGrossValue = getEmpGross(oEmp);
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                                else
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                            }
                            #endregion

                            Slab1Calc = GratuityAmount;
                            FirstPeriodEndDate = Gratuity1stPeriodEnd;
                            #endregion
                        }
                        else if (i == 2) //slab 2
                        {
                            #region Slab 2
                            double DaysDifference2ndSlab = 0, ToValueSlab = Convert.ToDouble(One.ToPoints) * 365;
                            DateTime Gratuity2ndPeriodStart, Gratuity2ndPeriodEnd;
                            TimeSpan Gratuity2ndSlabSpan;
                            DateTime FirstPeriodToFind = Convert.ToDateTime(oEmp.JoiningDate).AddDays(ToValueSlab);
                            CfgPeriodDates oSecondSlabEndDatePeriod = (from a in oDB.CfgPeriodDates
                                                                       where a.PayrollId == oEmp.PayrollID
                                                                       && a.StartDate <= FirstPeriodToFind && a.EndDate >= FirstPeriodToFind
                                                                       select a).FirstOrDefault();
                            if (oSecondSlabEndDatePeriod != null)
                            {
                                if ((DateTime)oPeriod.StartDate <= (DateTime)oSecondSlabEndDatePeriod.StartDate)
                                {
                                    Gratuity2ndPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    Gratuity2ndPeriodEnd = Convert.ToDateTime(oSecondSlabEndDatePeriod.EndDate);
                                }
                            }
                            else
                            {
                                if ((DateTime)oPeriod.StartDate < FirstPeriodToFind)
                                {
                                    Gratuity2ndPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    Gratuity2ndPeriodEnd = Convert.ToDateTime(oEmp.JoiningDate).AddDays((double)(One.ToPoints * 365));
                                }
                            }
                            Gratuity2ndPeriodStart = Convert.ToDateTime(FirstPeriodEndDate.AddDays(1));
                            Gratuity2ndSlabSpan = Gratuity2ndPeriodEnd - Gratuity2ndPeriodStart;
                            DaysDifference2ndSlab = Gratuity2ndSlabSpan.TotalDays;
                            if ((oEmp.TrnsGratuitySlabs.FlgAbsoluteYears != null ? oEmp.TrnsGratuitySlabs.FlgAbsoluteYears : false) == true)
                            {
                                SlabRating = Math.Round(Convert.ToDecimal((((decimal)DaysDifference2ndSlab - LeavesCount) / 365)));
                            }
                            else
                            {
                                SlabRating = Convert.ToDecimal((((decimal)DaysDifference2ndSlab - LeavesCount) / 365));
                            }

                            #region Value Calculation
                            SlabDays = Convert.ToDecimal(One.DaysCount);
                            CalculationDays = Convert.ToDecimal(One.TrnsGratuitySlabs.CalculatedDays);
                            PerValue = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.BasedOnValue);
                            if (oEmp.TrnsGratuitySlabs.BasedOn == "0") //Basic
                            {
                                EmpBasic = Convert.ToDecimal(oEmp.BasicSalary);
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                                else
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                            }
                            else //Gross
                            {
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                decimal empGrossValue = getEmpGross(oEmp);
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                                else
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                            }
                            #endregion

                            Slab2Calc = GratuityAmount;
                            SecondPeriodEndDate = Gratuity2ndPeriodEnd;
                            #endregion
                        }
                        else if (i == 3) //slab 3
                        {
                            #region Slab 3
                            double DaysDifference3rdSlab = 0, ToValueSlab = Convert.ToDouble(One.ToPoints) * 365;
                            DateTime Gratuity3rdPeriodStart, Gratuity3rdPeriodEnd;
                            TimeSpan Gratuity3rdSlabSpan;
                            DateTime FirstPeriodToFind = Convert.ToDateTime(oEmp.JoiningDate).AddDays(ToValueSlab);
                            CfgPeriodDates oThirdSlabEndDatePeriod = (from a in oDB.CfgPeriodDates
                                                                      where a.PayrollId == oEmp.PayrollID
                                                                      && a.StartDate <= FirstPeriodToFind && a.EndDate >= FirstPeriodToFind
                                                                      select a).FirstOrDefault();
                            if (oThirdSlabEndDatePeriod != null)
                            {
                                if ((DateTime)oPeriod.StartDate <= (DateTime)oThirdSlabEndDatePeriod.StartDate)
                                {
                                    Gratuity3rdPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    Gratuity3rdPeriodEnd = Convert.ToDateTime(oThirdSlabEndDatePeriod.EndDate);
                                }
                            }
                            else
                            {
                                if ((DateTime)oPeriod.StartDate < FirstPeriodToFind)
                                {
                                    Gratuity3rdPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    Gratuity3rdPeriodEnd = Convert.ToDateTime(oEmp.JoiningDate).AddDays((double)(One.ToPoints * 365));
                                }
                            }
                            Gratuity3rdPeriodStart = Convert.ToDateTime(FirstPeriodEndDate.AddDays(1));
                            Gratuity3rdSlabSpan = Gratuity3rdPeriodEnd - Gratuity3rdPeriodStart;
                            DaysDifference3rdSlab = Gratuity3rdSlabSpan.TotalDays;
                            if ((oEmp.TrnsGratuitySlabs.FlgAbsoluteYears != null ? oEmp.TrnsGratuitySlabs.FlgAbsoluteYears : false) == true)
                            {
                                SlabRating = Math.Round(Convert.ToDecimal((((decimal)DaysDifference3rdSlab - LeavesCount) / 365)));
                            }
                            else
                            {
                                SlabRating = Convert.ToDecimal((((decimal)DaysDifference3rdSlab - LeavesCount) / 365));
                            }

                            #region Value Calculation
                            SlabDays = Convert.ToDecimal(One.DaysCount);
                            CalculationDays = Convert.ToDecimal(One.TrnsGratuitySlabs.CalculatedDays);
                            PerValue = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.BasedOnValue);
                            if (oEmp.TrnsGratuitySlabs.BasedOn == "0") //Basic
                            {
                                EmpBasic = Convert.ToDecimal(oEmp.BasicSalary);
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                                else
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                            }
                            else //Gross
                            {
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                decimal empGrossValue = getEmpGross(oEmp);
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                                else
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                            }
                            #endregion

                            Slab3Calc = GratuityAmount;
                            ThirdPeriodEndDate = Gratuity3rdPeriodEnd;
                            #endregion
                        }
                        else if (i == 4) //slab 4
                        {
                            #region Slab 4
                            double DaysDifference4thSlab = 0, ToValueSlab = Convert.ToDouble(One.ToPoints) * 365;
                            DateTime Gratuity4thPeriodStart, Gratuity4thPeriodEnd;
                            TimeSpan Gratuity4thSlabSpan;
                            DateTime FirstPeriodToFind = Convert.ToDateTime(oEmp.JoiningDate).AddDays(ToValueSlab);
                            CfgPeriodDates oFourthSlabEndDatePeriod = (from a in oDB.CfgPeriodDates
                                                                       where a.PayrollId == oEmp.PayrollID
                                                                       && a.StartDate <= FirstPeriodToFind && a.EndDate >= FirstPeriodToFind
                                                                       select a).FirstOrDefault();
                            if (oFourthSlabEndDatePeriod != null)
                            {
                                if ((DateTime)oPeriod.StartDate <= (DateTime)oFourthSlabEndDatePeriod.StartDate)
                                {
                                    Gratuity4thPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    if ((DateTime)oPeriod.StartDate < FirstPeriodToFind)
                                    {
                                        Gratuity4thPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                    }
                                    else
                                    {
                                        Gratuity4thPeriodEnd = Convert.ToDateTime(oEmp.JoiningDate).AddDays((double)(One.ToPoints * 365));
                                    }
                                }
                            }
                            else
                            {
                                if ((DateTime)oPeriod.StartDate < FirstPeriodToFind)
                                {
                                    Gratuity4thPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    Gratuity4thPeriodEnd = Convert.ToDateTime(oEmp.JoiningDate).AddDays((double)(One.ToPoints * 365));
                                }
                            }
                            Gratuity4thPeriodStart = Convert.ToDateTime(FirstPeriodEndDate.AddDays(1));
                            Gratuity4thSlabSpan = Gratuity4thPeriodEnd - Gratuity4thPeriodStart;
                            DaysDifference4thSlab = Gratuity4thSlabSpan.TotalDays;
                            if ((oEmp.TrnsGratuitySlabs.FlgAbsoluteYears != null ? oEmp.TrnsGratuitySlabs.FlgAbsoluteYears : false) == true)
                            {
                                SlabRating = Math.Round(Convert.ToDecimal((((decimal)DaysDifference4thSlab - LeavesCount) / 365)));
                            }
                            else
                            {
                                SlabRating = Convert.ToDecimal((((decimal)DaysDifference4thSlab - LeavesCount) / 365));
                            }

                            #region Value Calculation
                            SlabDays = Convert.ToDecimal(One.DaysCount);
                            CalculationDays = Convert.ToDecimal(One.TrnsGratuitySlabs.CalculatedDays);
                            PerValue = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.BasedOnValue);
                            if (oEmp.TrnsGratuitySlabs.BasedOn == "0") //Basic
                            {
                                EmpBasic = Convert.ToDecimal(oEmp.BasicSalary);
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                                else
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                            }
                            else //Gross
                            {
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                decimal empGrossValue = getEmpGross(oEmp);
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                                else
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                            }
                            #endregion

                            Slab4Calc = PerMonthProvision;
                            FourthPeriodEndDate = Gratuity4thPeriodEnd;
                            #endregion
                        }
                        else if (i == 5) //slab 5
                        {
                            #region Slab 5
                            double DaysDifference5thSlab = 0, ToValueSlab = Convert.ToDouble(One.ToPoints) * 365;
                            DateTime Gratuity5thPeriodStart, Gratuity5thPeriodEnd;
                            TimeSpan Gratuity5thSlabSpan;
                            DateTime FirstPeriodToFind = Convert.ToDateTime(oEmp.JoiningDate).AddDays(ToValueSlab);
                            CfgPeriodDates oFifthSlabEndDatePeriod = (from a in oDB.CfgPeriodDates
                                                                      where a.PayrollId == oEmp.PayrollID
                                                                      && a.StartDate <= FirstPeriodToFind && a.EndDate >= FirstPeriodToFind
                                                                      select a).FirstOrDefault();
                            if (oFifthSlabEndDatePeriod != null)
                            {
                                if ((DateTime)oPeriod.StartDate <= (DateTime)oFifthSlabEndDatePeriod.StartDate)
                                {
                                    Gratuity5thPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    Gratuity5thPeriodEnd = Convert.ToDateTime(oFifthSlabEndDatePeriod.EndDate);
                                }
                            }
                            else
                            {
                                if ((DateTime)oPeriod.StartDate < FirstPeriodToFind)
                                {
                                    Gratuity5thPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    Gratuity5thPeriodEnd = Convert.ToDateTime(oEmp.JoiningDate).AddDays((double)(One.ToPoints * 365));
                                }
                            }
                            Gratuity5thPeriodStart = Convert.ToDateTime(FirstPeriodEndDate.AddDays(1));
                            Gratuity5thSlabSpan = Gratuity5thPeriodEnd - Gratuity5thPeriodStart;
                            DaysDifference5thSlab = Gratuity5thSlabSpan.TotalDays;
                            if ((oEmp.TrnsGratuitySlabs.FlgAbsoluteYears != null ? oEmp.TrnsGratuitySlabs.FlgAbsoluteYears : false) == true)
                            {
                                SlabRating = Math.Round(Convert.ToDecimal((((decimal)DaysDifference5thSlab - LeavesCount) / 365)));
                            }
                            else
                            {
                                SlabRating = Convert.ToDecimal((((decimal)DaysDifference5thSlab - LeavesCount) / 365));
                            }

                            #region Value Calculation
                            SlabDays = Convert.ToDecimal(One.DaysCount);
                            CalculationDays = Convert.ToDecimal(One.TrnsGratuitySlabs.CalculatedDays);
                            PerValue = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.BasedOnValue);
                            if (oEmp.TrnsGratuitySlabs.BasedOn == "0") //Basic
                            {
                                EmpBasic = Convert.ToDecimal(oEmp.BasicSalary);
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                                else
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                            }
                            else //Gross
                            {
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                decimal empGrossValue = getEmpGross(oEmp);
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                                else
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                            }
                            #endregion

                            Slab5Calc = PerMonthProvision;
                            FifthPeriodEndDate = Gratuity5thPeriodEnd;
                            #endregion
                        }
                        i++;
                    }
                    if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                    {
                        PerMonthProvision = ((Slab1Calc + Slab2Calc + Slab3Calc + Slab4Calc + Slab5Calc) / PayrollDays) * MonthDays;
                    }
                    else
                    {
                        PerMonthProvision = ((Slab1Calc + Slab2Calc + Slab3Calc + Slab4Calc + Slab5Calc) / 365) * MonthDays;
                    }

                    DataRow dr = dtOut.NewRow();
                    dr["LineType"] = "Element";
                    dr["LineSubType"] = "Gratuity Provision";
                    dr["LineValue"] = PerMonthProvision;
                    dr["LineMemo"] = "Gratuity";
                    dr["DebitAccount"] = "";
                    dr["CreditAccount"] = "";
                    dr["DebitAccountName"] = "";
                    dr["CreditAccountName"] = "";
                    dr["LineBaseEntry"] = "";
                    dr["BaseValueCalculatedOn"] = oEmp.BasicSalary.ToString();
                    dr["BaseValue"] = oEmp.BasicSalary.ToString();
                    dr["BaseValueType"] = "FIX";
                    dr["TaxbleAmnt"] = "";
                    dr["NRTaxbleAmnt"] = 0.00M;
                    dtOut.Rows.Add(dr);
                    #endregion

                    #region Gratuity Increment Provision Adjustment
                    var oLastSalary = (from a in oDB.TrnsSalaryProcessRegister
                                       where a.EmpID == oEmp.ID && a.PayrollPeriodID == oPrePeriod.ID
                                       select a).FirstOrDefault(); // assuming last period salary.
                    if (oLastSalary == null) return dtOut; // when no last salary found
                    if (oPrePeriod.ID == oPeriod.ID) return dtOut; // when company started and have no previos data.
                    if (oEmp.BasicSalary != oLastSalary.EmpBasic)
                    {
                        //get previous gratuity amount & count
                        Int32 oCounts = (from a in oDB.TrnsSalaryProcessRegisterDetail
                                         where a.TrnsSalaryProcessRegister.EmpID == oEmp.ID
                                         && a.TrnsSalaryProcessRegister.PayrollPeriodID <= oPrePeriod.ID
                                         && a.LineMemo == "Gratuity"
                                         select a).Count();
                        decimal PreviousProvisionGratuityValue = (from a in oDB.TrnsSalaryProcessRegisterDetail
                                                                  where a.TrnsSalaryProcessRegister.EmpID == oEmp.ID
                                                                  && a.TrnsSalaryProcessRegister.PayrollPeriodID <= oPrePeriod.ID
                                                                  && a.LineMemo == "Gratuity"
                                                                  select a.LineValue).Sum() ?? 0;
                        //get current gratuity amount.
                        decimal newValueProvision = PerMonthProvision * oCounts;
                        if (PreviousProvisionGratuityValue < newValueProvision)
                        {
                            DataRow dr1 = dtOut.NewRow();
                            dr1["LineType"] = "Element";
                            dr1["LineSubType"] = "Gratuity Provision Adj";
                            dr1["LineValue"] = Math.Abs(newValueProvision - PreviousProvisionGratuityValue);
                            dr1["LineMemo"] = "Gratuity";
                            dr1["DebitAccount"] = "";
                            dr1["CreditAccount"] = "";
                            dr1["DebitAccountName"] = "";
                            dr1["CreditAccountName"] = "";
                            dr1["LineBaseEntry"] = "";
                            dr1["BaseValueCalculatedOn"] = newValueProvision.ToString();
                            dr1["BaseValue"] = PreviousProvisionGratuityValue.ToString();
                            dr1["BaseValueType"] = "FIX";
                            dr1["TaxbleAmnt"] = "0";
                            dr1["NRTaxbleAmnt"] = 0.00M;
                            dtOut.Rows.Add(dr1);
                        }
                    }
                    #endregion

                }
            }
            catch (Exception ex)
            {
            }
            return dtOut;
        }

        public decimal getEmployeeTaxAmount(CfgPeriodDates period, MstEmployee emp, decimal PeriodTaxbleIncome, decimal NonRecurringPyable)
        {
            int remainingPeriodsCnt = 0;
            decimal outValue = 0.00M;
            decimal alreadyProcessedIncome = 0.00M;
            decimal alreadyProcessedTax = 0.00M;
            decimal projectedIncome = 0.00M;
            decimal currentYearExpectedIncome = 0.00M;
            decimal currentYearExpectedTax = 0.00M;
            decimal perYearTax = 0.00M;
            string strPrevious = "SELECT SUM(dbo.TrnsSalaryProcessRegister.EmpTaxblTotal) AS TaxbleAmount, SUM(ISNULL(dbo.TrnsSalaryProcessRegister.EmpTotalTax, 0)) AS PaidTax,  ";
            strPrevious += "                     dbo.TrnsSalaryProcessRegister.EmpID";
            strPrevious += " FROM dbo.TrnsSalaryProcessRegister INNER JOIN";
            strPrevious += " dbo.CfgPeriodDates ON dbo.TrnsSalaryProcessRegister.PayrollPeriodID = dbo.CfgPeriodDates.ID";
            strPrevious += " WHERE     (dbo.TrnsSalaryProcessRegister.EmpID = " + emp.ID.ToString() + ") AND (dbo.CfgPeriodDates.CalCode = '" + period.CalCode + "')";
            strPrevious += " GROUP BY dbo.TrnsSalaryProcessRegister.EmpID";


            DataTable dtPreviousInfo = getDataTable(strPrevious);

            remainingPeriodsCnt = (from p in oDB.CfgPeriodDates where p.ID >= period.ID && p.PayrollId == emp.PayrollID && p.CalCode == period.CalCode select p).Count();

            string strTaxOb = " select isnull(SUM(TaxBalance),0) as ObTax from TrnsOBTax where EmpID = '" + emp.ID.ToString() + "'";
            decimal obTax = Convert.ToDecimal(getScallerValue(strTaxOb));
            string strIncomeOb = " select isnull(SUM(SalaryBalance),0) as ObTax from TrnsOBSalary where EmpID = '" + emp.ID.ToString() + "'";
            decimal obIncome = Convert.ToDecimal(getScallerValue(strIncomeOb));
            alreadyProcessedIncome = obIncome;
            alreadyProcessedTax = obTax;
            foreach (DataRow dr in dtPreviousInfo.Rows)
            {
                alreadyProcessedIncome += Convert.ToDecimal(dtPreviousInfo.Rows[0]["TaxbleAmount"]);
                alreadyProcessedTax += Convert.ToDecimal(dtPreviousInfo.Rows[0]["PaidTax"]);

            }


            projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable) * remainingPeriodsCnt) + NonRecurringPyable;

            currentYearExpectedIncome = alreadyProcessedIncome + projectedIncome;

            int cnt = (from p in oDB.CfgTaxDetail where p.CfgTaxSetup.MstCalendar.Code == period.CalCode where p.MinAmount <= currentYearExpectedIncome && p.MaxAmount >= currentYearExpectedIncome select p).Count();
            if (cnt > 0)
            {
                CfgTaxDetail taxLine = (from p in oDB.CfgTaxDetail where p.CfgTaxSetup.MstCalendar.Code == period.CalCode where p.MinAmount <= currentYearExpectedIncome && p.MaxAmount >= currentYearExpectedIncome select p).Single();

                currentYearExpectedTax = (decimal)taxLine.FixTerm + (decimal)(currentYearExpectedIncome - taxLine.MinAmount) * (decimal)taxLine.TaxValue / 100;

                outValue = (currentYearExpectedTax - alreadyProcessedTax) / remainingPeriodsCnt;

            }



            return outValue;
        }

        public decimal getEmployeeTaxAmount(CfgPeriodDates period, MstEmployee emp, decimal PeriodTaxbleIncomeOT, decimal PeriodTaxbleIncome, decimal NonRecurringPyable)
        {
            int remainingPeriodsCnt = 0;
            decimal outValue = 0.00M;
            decimal alreadyProcessedIncome = 0.00M;
            decimal alreadyProcessedTax = 0.00M;
            decimal projectedIncome = 0.00M;
            decimal currentYearExpectedIncome = 0.00M;
            decimal currentYearExpectedTax = 0.00M;
            decimal perYearTax = 0.00M;
            string strPrevious = "SELECT SUM(dbo.TrnsSalaryProcessRegister.EmpTaxblTotal) AS TaxbleAmount, SUM(ISNULL(dbo.TrnsSalaryProcessRegister.EmpTotalTax, 0)) AS PaidTax,  ";
            strPrevious += "                     dbo.TrnsSalaryProcessRegister.EmpID";
            strPrevious += " FROM dbo.TrnsSalaryProcessRegister INNER JOIN";
            strPrevious += " dbo.CfgPeriodDates ON dbo.TrnsSalaryProcessRegister.PayrollPeriodID = dbo.CfgPeriodDates.ID";
            strPrevious += " WHERE     (dbo.TrnsSalaryProcessRegister.EmpID = " + emp.ID.ToString() + ") AND (dbo.CfgPeriodDates.CalCode = '" + period.CalCode + "')";
            strPrevious += " GROUP BY dbo.TrnsSalaryProcessRegister.EmpID";


            DataTable dtPreviousInfo = getDataTable(strPrevious);

            remainingPeriodsCnt = (from p in oDB.CfgPeriodDates where p.ID >= period.ID && p.PayrollId == emp.PayrollID && p.CalCode == period.CalCode select p).Count();

            string strTaxOb = " select isnull(SUM(TaxBalance),0) as ObTax from TrnsOBTax where EmpID = '" + emp.ID.ToString() + "'";
            decimal obTax = Convert.ToDecimal(getScallerValue(strTaxOb));
            string strIncomeOb = " select isnull(SUM(SalaryBalance),0) as ObTax from TrnsOBSalary where EmpID = '" + emp.ID.ToString() + "'";
            decimal obIncome = Convert.ToDecimal(getScallerValue(strIncomeOb));
            alreadyProcessedIncome = obIncome;
            alreadyProcessedTax = obTax;
            foreach (DataRow dr in dtPreviousInfo.Rows)
            {
                alreadyProcessedIncome += Convert.ToDecimal(dtPreviousInfo.Rows[0]["TaxbleAmount"]);
                alreadyProcessedTax += Convert.ToDecimal(dtPreviousInfo.Rows[0]["PaidTax"]);

            }


            projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT) * remainingPeriodsCnt) + NonRecurringPyable + PeriodTaxbleIncomeOT;

            currentYearExpectedIncome = alreadyProcessedIncome + projectedIncome;

            int cnt = (from p in oDB.CfgTaxDetail where p.CfgTaxSetup.MstCalendar.Code == period.CalCode where p.MinAmount <= currentYearExpectedIncome && p.MaxAmount >= currentYearExpectedIncome select p).Count();
            if (cnt > 0)
            {
                CfgTaxDetail taxLine = (from p in oDB.CfgTaxDetail where p.CfgTaxSetup.MstCalendar.Code == period.CalCode where p.MinAmount <= currentYearExpectedIncome && p.MaxAmount >= currentYearExpectedIncome select p).Single();

                currentYearExpectedTax = (decimal)taxLine.FixTerm + (decimal)(currentYearExpectedIncome - taxLine.MinAmount) * (decimal)taxLine.TaxValue / 100;

                outValue = (currentYearExpectedTax - alreadyProcessedTax) / remainingPeriodsCnt;

            }



            return outValue;
        }

        public decimal getEmployeeTaxAmount(CfgPeriodDates period, MstEmployee emp, decimal PeriodTaxbleIncomeOT, decimal PeriodTaxbleLWOP, decimal PeriodTaxbleIncome, decimal NonRecurringPyable, decimal empGrossSalary, decimal PayRatioCondition)
        {
            int remainingPeriodsCnt = 0;
            decimal outValue = 0.00M;
            decimal alreadyProcessedIncome = 0.00M;
            decimal alreadyProcessedTax = 0.00M;
            decimal projectedIncome = 0.00M;
            decimal currentYearExpectedIncome = 0.00M;
            decimal currentYearExpectedTax = 0.00M;
            decimal perYearTax = 0.00M;
            Decimal TaxDiscountYearly = 0.0M;
            Decimal TaxDiscountMonthly = 0.0M;
            Decimal TaxQuarterlyComplete = 0.0M;

            //New Section For Tax Adjustment

            String strQuery = @"SELECT ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS Value
                                FROM dbo.TrnsTaxAdjustment AS A1 INNER JOIN dbo.TrnsTaxAdjustmentDetails AS A2 ON A1.ID = A2.TAID
	                                 INNER JOIN dbo.MstEmployee A3 ON A1.empID = A3.ID
                                WHERE 
	                                ISNULL(A1.flgActive,'0') = '1' AND
	                                ISNULL(A2.flgActive,'0') = '1' AND
                                    ISNULL(A2.flgMonthly,'0') = '0' AND  
	                                A3.EmpID = '" + emp.EmpID + "'";


            TaxDiscountYearly = Convert.ToDecimal(getScallerValue(strQuery));

            //End Section
            //string strPrevious = "SELECT SUM (ISNULL(dbo.TrnsSalaryProcessRegister.EmpTaxblTotal,0)) AS TaxbleAmount, SUM(ISNULL(dbo.TrnsSalaryProcessRegister.EmpTotalTax, 0)) AS PaidTax,  ";
            //strPrevious += "                     dbo.TrnsSalaryProcessRegister.EmpID";
            //strPrevious += " FROM dbo.TrnsSalaryProcessRegister INNER JOIN";
            //strPrevious += " dbo.CfgPeriodDates ON dbo.TrnsSalaryProcessRegister.PayrollPeriodID = dbo.CfgPeriodDates.ID";
            //strPrevious += " WHERE     (dbo.TrnsSalaryProcessRegister.EmpID = " + emp.ID.ToString() + ") AND (dbo.CfgPeriodDates.CalCode = '" + period.CalCode + "')";
            //strPrevious += " GROUP BY dbo.TrnsSalaryProcessRegister.EmpID";

            string strPrevious = "SELECT SUM(dbo.TrnsSalaryProcessRegister.EmpTaxblTotal) AS TaxbleAmount, SUM(ISNULL(dbo.TrnsSalaryProcessRegister.EmpTotalTax, 0)) AS PaidTax,  ";
            strPrevious += "                     dbo.TrnsSalaryProcessRegister.EmpID";
            strPrevious += " FROM dbo.TrnsSalaryProcessRegister INNER JOIN";
            strPrevious += " dbo.CfgPeriodDates ON dbo.TrnsSalaryProcessRegister.PayrollPeriodID = dbo.CfgPeriodDates.ID";
            strPrevious += " WHERE     (dbo.TrnsSalaryProcessRegister.EmpID = " + emp.ID.ToString() + ") AND (dbo.CfgPeriodDates.CalCode = '" + period.CalCode + "')";
            strPrevious += " GROUP BY dbo.TrnsSalaryProcessRegister.EmpID";

            DataTable dtPreviousInfo = getDataTable(strPrevious);

            remainingPeriodsCnt = (from p in oDB.CfgPeriodDates where p.ID >= period.ID && p.PayrollId == emp.PayrollID && p.CalCode == period.CalCode select p).Count();

            string strTaxOb = " select isnull(SUM(TaxBalance),0) as ObTax from TrnsOBTax where EmpID = '" + emp.ID.ToString() + "'";
            decimal obTax = Convert.ToDecimal(getScallerValue(strTaxOb));
            string strIncomeOb = " select isnull(SUM(SalaryBalance),0) as ObTax from TrnsOBSalary where EmpID = '" + emp.ID.ToString() + "'";
            decimal obIncome = Convert.ToDecimal(getScallerValue(strIncomeOb));
            alreadyProcessedIncome = obIncome;
            alreadyProcessedTax = obTax;
            foreach (DataRow dr in dtPreviousInfo.Rows)
            {
                alreadyProcessedIncome += Convert.ToDecimal(dtPreviousInfo.Rows[0]["TaxbleAmount"]);
                alreadyProcessedTax += Convert.ToDecimal(dtPreviousInfo.Rows[0]["PaidTax"]);

            }
            //TODO: For 1st month of calendar year for specific days
            if (alreadyProcessedIncome == 0 && PayRatioCondition != 1)
            {
                //projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) * ((remainingPeriodsCnt-1)*empGrossSalary)) + NonRecurringPyable + PeriodTaxbleIncomeOT + PeriodTaxbleLWOP;
                projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) + ((remainingPeriodsCnt - 1) * empGrossSalary)) + NonRecurringPyable + PeriodTaxbleLWOP + PeriodTaxbleIncomeOT;
                projectedIncome += TaxQuarterlyComplete;
            }
            else
            {
                projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) * remainingPeriodsCnt) + NonRecurringPyable + PeriodTaxbleIncomeOT + PeriodTaxbleLWOP;
                projectedIncome += TaxQuarterlyComplete;
            }

            currentYearExpectedIncome = alreadyProcessedIncome + projectedIncome;

            int cnt = (from p in oDB.CfgTaxDetail where p.CfgTaxSetup.MstCalendar.Code == period.CalCode where p.MinAmount <= currentYearExpectedIncome && p.MaxAmount >= currentYearExpectedIncome select p).Count();
            if (cnt > 0)
            {
                CfgTaxDetail taxLine = (from p in oDB.CfgTaxDetail where p.CfgTaxSetup.MstCalendar.Code == period.CalCode where p.MinAmount <= currentYearExpectedIncome && p.MaxAmount >= currentYearExpectedIncome select p).Single();

                currentYearExpectedTax = (decimal)taxLine.FixTerm + (decimal)(currentYearExpectedIncome - taxLine.MinAmount) * (decimal)taxLine.TaxValue / 100;

                if (TaxDiscountYearly != 0)
                    currentYearExpectedTax -= TaxDiscountYearly;

                outValue = (currentYearExpectedTax - alreadyProcessedTax) / remainingPeriodsCnt;

                if (TaxDiscountMonthly != 0)
                    outValue -= TaxDiscountMonthly;
            }



            return outValue;
        }

        public decimal getEmployeeTaxAmount(CfgPeriodDates period, MstEmployee emp, decimal PeriodTaxbleIncomeOT, decimal PeriodTaxbleLWOP, decimal PeriodTaxbleIncome, decimal NonRecurringPyable, decimal empGrossSalary, decimal PayRatioCondition, out decimal QuarterlyValue)
        {
            int remainingPeriodsCnt = 0;
            decimal outValue = 0.00M;
            decimal salaryTaxValue = 0.0M;
            decimal alreadyProcessedIncome = 0.00M;
            decimal alreadyProcessedTax = 0.00M;
            decimal projectedIncome = 0.00M;
            decimal currentYearExpectedIncome = 0.00M;
            decimal currentYearExpectedIncomeI = 0.00M;
            decimal currentYearExpectedTax = 0.00M;
            decimal currentYearExpectedTaxI = 0.00M;
            decimal perYearTax = 0.00M;
            Decimal TaxDiscountYearly = 0.0M;
            Decimal TaxDiscountMonthly = 0.0M;
            Decimal TaxQuarterlyComplete = 0.0M;
            Decimal TaxValueIncentive = 0.0M;
            Decimal TaxIncentivePrevious = 0.0M;
            Int32 TaxDetailID = 0;
            decimal incentiveValue = 0.0M;
            decimal lastIncentiveValue = 0.0M;
            string TaxCode = "", TaxMin = "", TaxMax = "", TaxPer = "", TaxFix = "";
            string TaxCodeI = "", TaxMinI = "", TaxMaxI = "", TaxPerI = "", TaxFixI = "";

            //New Section For Tax Adjustment

            String strQuery = @"SELECT ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS Value
                                FROM dbo.TrnsTaxAdjustment AS A1 INNER JOIN dbo.TrnsTaxAdjustmentDetails AS A2 ON A1.ID = A2.TAID
	                                 INNER JOIN dbo.MstEmployee A3 ON A1.empID = A3.ID
                                WHERE 
	                                ISNULL(A1.flgActive,'0') = '1' AND
	                                ISNULL(A2.flgActive,'0') = '1' AND
                                    ISNULL(A2.flgMonthly,'0') = '0' AND  
	                                A3.EmpID = '" + emp.EmpID + "'";


            TaxDiscountYearly = Convert.ToDecimal(getScallerValue(strQuery));


            String strQuery2 = @"SELECT ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS Value
                                FROM dbo.TrnsTaxAdjustment AS A1 INNER JOIN dbo.TrnsTaxAdjustmentDetails AS A2 ON A1.ID = A2.TAID
	                                 INNER JOIN dbo.MstEmployee A3 ON A1.empID = A3.ID
                                WHERE 
	                                ISNULL(A1.flgActive,'0') = '1' AND
	                                ISNULL(A2.flgActive,'0') = '1' AND
                                    ISNULL(A2.flgMonthly,'0') = '1' AND  
	                                A3.EmpID = '" + emp.EmpID + "'";

            TaxDiscountMonthly = Convert.ToDecimal(getScallerValue(strQuery2));


            //            String strQuery3 = @"
            //                                SELECT 
            //	                                ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS Amount
            //                                FROM 
            //	                                dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID
            //                                WHERE
            //	                                A1.empID = '" + emp.ID + @"'
            //	                                AND A2.PayrollPeriodID = '"+ period.ID +"'";
            // period nikal diya ek case ki waja say jab
            String strQuery3 = @"
                                SELECT 
	                                ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS Amount
                                FROM 
	                                dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID
                                WHERE
	                                A1.empID = '" + emp.ID + @"'";

            TaxQuarterlyComplete = Convert.ToDecimal(getScallerValue(strQuery3));


            String strQuery4 = @"SELECT Top 1 A2.ID FROM dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID WHERE	A1.empID = '" + emp.ID + "' 	AND A2.PayrollPeriodID = '" + period.ID + "'";
            TaxDetailID = Convert.ToInt32(getScallerValue(strQuery4));

            String strQuery6 = @"
                                SELECT 
	                                ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS Amount
                                FROM 
	                                dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID
                                WHERE
	                                A1.empID = '" + emp.ID + @"'
                                    AND A2.ID <> '" + TaxDetailID + @"'
                                ";
            TaxIncentivePrevious = Convert.ToInt32(getScallerValue(strQuery6));

            //String strQuery5 = @"SELECT 	ISNULL(SUM(ISNULL(A2.TaxableAmount,0)),0) AS TaxableAmountI FROM dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID WHERE A1.empID = '"+emp.ID+"' AND A2.PayrollPeriodID < '" + period.ID + "'";
            string strQuery5 = @"SELECT 	ISNULL(SUM(ISNULL(A2.TaxableAmount,0)),0) AS TaxableAmountI FROM dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID INNER JOIN dbo.CfgPeriodDates A3 ON A2.PayrollPeriodID = A3.ID WHERE A1.empID = '" + emp.ID.ToString() + "' AND A3.ID < '" + period.ID.ToString() + "' AND A3.CalCode = '" + period.CalCode.ToString().Trim() + "'";
            TaxValueIncentive = Convert.ToInt32(getScallerValue(strQuery5));


            if (TaxQuarterlyComplete > 0)
            {
                QuarterlyValue = TaxQuarterlyComplete;
            }
            else
            {
                QuarterlyValue = 0.0M;
            }

            //End Section
            //string strPrevious = "SELECT SUM (ISNULL(dbo.TrnsSalaryProcessRegister.EmpTaxblTotal,0)) AS TaxbleAmount, SUM(ISNULL(dbo.TrnsSalaryProcessRegister.EmpTotalTax, 0)) AS PaidTax,  ";
            //strPrevious += "                     dbo.TrnsSalaryProcessRegister.EmpID";
            //strPrevious += " FROM dbo.TrnsSalaryProcessRegister INNER JOIN";
            //strPrevious += " dbo.CfgPeriodDates ON dbo.TrnsSalaryProcessRegister.PayrollPeriodID = dbo.CfgPeriodDates.ID";
            //strPrevious += " WHERE     (dbo.TrnsSalaryProcessRegister.EmpID = " + emp.ID.ToString() + ") AND (dbo.CfgPeriodDates.CalCode = '" + period.CalCode + "')";
            //strPrevious += " GROUP BY dbo.TrnsSalaryProcessRegister.EmpID";

            string strPrevious = "SELECT SUM(dbo.TrnsSalaryProcessRegister.EmpTaxblTotal) AS TaxbleAmount, SUM(ISNULL(dbo.TrnsSalaryProcessRegister.EmpTotalTax, 0)) AS PaidTax,  ";
            strPrevious += "                     dbo.TrnsSalaryProcessRegister.EmpID";
            strPrevious += " FROM dbo.TrnsSalaryProcessRegister INNER JOIN";
            strPrevious += " dbo.CfgPeriodDates ON dbo.TrnsSalaryProcessRegister.PayrollPeriodID = dbo.CfgPeriodDates.ID";
            strPrevious += " WHERE     (dbo.TrnsSalaryProcessRegister.EmpID = " + emp.ID.ToString() + ") AND (dbo.CfgPeriodDates.CalCode = '" + period.CalCode + "')";
            strPrevious += " GROUP BY dbo.TrnsSalaryProcessRegister.EmpID";

            DataTable dtPreviousInfo = getDataTable(strPrevious);

            remainingPeriodsCnt = (from p in oDB.CfgPeriodDates where p.ID >= period.ID && p.PayrollId == emp.PayrollID && p.CalCode == period.CalCode select p).Count();

            string strTaxOb = " select isnull(SUM(TaxBalance),0) as ObTax from TrnsOBTax where EmpID = '" + emp.ID.ToString() + "'";
            decimal obTax = Convert.ToDecimal(getScallerValue(strTaxOb));


            string strIncomeOb = " select isnull(SUM(SalaryBalance),0) as ObTax from TrnsOBSalary where EmpID = '" + emp.ID.ToString() + "'";
            decimal obIncome = Convert.ToDecimal(getScallerValue(strIncomeOb));

            string strAdjIncome = "select isnull(SUM(Amount),0) as ObTax from TrnsObSalaryAdj  where EmpID = '" + emp.ID.ToString() + "'";
            decimal obAdjustedIncome = Convert.ToDecimal(getScallerValue(strAdjIncome));


            alreadyProcessedIncome = obIncome + obAdjustedIncome;
            alreadyProcessedTax = obTax;


            foreach (DataRow dr in dtPreviousInfo.Rows)
            {
                alreadyProcessedIncome += Convert.ToDecimal(dtPreviousInfo.Rows[0]["TaxbleAmount"]);
                alreadyProcessedTax += Convert.ToDecimal(dtPreviousInfo.Rows[0]["PaidTax"]);

            }


            //TODO: For 1st month of calendar year for specific days
            if (alreadyProcessedIncome == 0 && PayRatioCondition != 1)
            {
                //projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) * ((remainingPeriodsCnt-1)*empGrossSalary)) + NonRecurringPyable + PeriodTaxbleIncomeOT + PeriodTaxbleLWOP;
                projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) + ((remainingPeriodsCnt - 1) * empGrossSalary)) + NonRecurringPyable + PeriodTaxbleLWOP + PeriodTaxbleIncomeOT;
                //projectedIncome += TaxQuarterlyComplete;
            }
            else
            {
                projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) * remainingPeriodsCnt) + NonRecurringPyable + PeriodTaxbleIncomeOT + PeriodTaxbleLWOP;
                //projectedIncome += TaxQuarterlyComplete;
            }

            if (TaxDiscountYearly == 0)
            {
                currentYearExpectedIncome = alreadyProcessedIncome + projectedIncome;
            }
            else
            {
                currentYearExpectedIncome = alreadyProcessedIncome + projectedIncome + TaxDiscountYearly;
            }

            //Quarter Tax Adjustment 
            if (TaxQuarterlyComplete > 0)
            {
                currentYearExpectedIncomeI = alreadyProcessedIncome + (projectedIncome + TaxQuarterlyComplete);
                currentYearExpectedIncome += TaxIncentivePrevious;

                int cnt1 = (from p in oDB.CfgTaxDetail where p.CfgTaxSetup.MstCalendar.Code == period.CalCode where p.MinAmount <= currentYearExpectedIncomeI && p.MaxAmount >= currentYearExpectedIncomeI select p).Count();
                if (cnt1 > 0)
                {
                    CfgTaxDetail taxLineI = (from p in oDB.CfgTaxDetail where p.CfgTaxSetup.MstCalendar.Code == period.CalCode where p.MinAmount <= currentYearExpectedIncomeI && p.MaxAmount >= currentYearExpectedIncomeI select p).Single();
                    TaxCodeI = taxLineI.TaxCode;
                    TaxMinI = Convert.ToString(taxLineI.MinAmount);
                    TaxMaxI = Convert.ToString(taxLineI.MaxAmount);
                    TaxFixI = Convert.ToString(taxLineI.FixTerm);
                    TaxPerI = Convert.ToString(taxLineI.TaxValue);
                    currentYearExpectedTaxI = (decimal)taxLineI.FixTerm + (decimal)(currentYearExpectedIncomeI - taxLineI.MinAmount) * (decimal)taxLineI.TaxValue / 100;

                }
            }

            int cnt = (from p in oDB.CfgTaxDetail where p.CfgTaxSetup.MstCalendar.Code == period.CalCode where p.MinAmount <= currentYearExpectedIncome && p.MaxAmount >= currentYearExpectedIncome select p).Count();
            if (cnt > 0)
            {
                CfgTaxDetail taxLine = (from p in oDB.CfgTaxDetail where p.CfgTaxSetup.MstCalendar.Code == period.CalCode where p.MinAmount <= currentYearExpectedIncome && p.MaxAmount >= currentYearExpectedIncome select p).Single();
                TaxCode = taxLine.TaxCode;
                TaxMin = Convert.ToString(taxLine.MinAmount);
                TaxMax = Convert.ToString(taxLine.MaxAmount);
                TaxFix = Convert.ToString(taxLine.FixTerm);
                TaxPer = Convert.ToString(taxLine.TaxValue);
                currentYearExpectedTax = (decimal)taxLine.FixTerm + (decimal)(currentYearExpectedIncome - taxLine.MinAmount) * (decimal)taxLine.TaxValue / 100;
                //if (TaxDiscountYearly != 0)
                //{
                //    currentYearExpectedTax -= TaxDiscountYearly;
                //}

                //outValue = (currentYearExpectedTax - (alreadyProcessedTax - TaxValueIncentive)) / remainingPeriodsCnt;
                salaryTaxValue = (currentYearExpectedTax - (alreadyProcessedTax - TaxValueIncentive)) / remainingPeriodsCnt;

                if (TaxDiscountMonthly != 0)
                {
                    outValue = salaryTaxValue + TaxDiscountMonthly;
                }
                else
                {
                    outValue = salaryTaxValue;
                }

                if (TaxQuarterlyComplete > 0)
                {
                    Int32 recCount = (from a in oDB.TrnsQuarterTaxAdjDetail where a.TrnsQuarterTaxAdj.EmpID == emp.ID && a.PayrollPeriodID == period.ID select a).Count();

                    if (recCount > 1)
                    {
                        TrnsQuarterTaxAdjDetail oDoc = (from a in oDB.TrnsQuarterTaxAdjDetail where a.TrnsQuarterTaxAdj.EmpID == emp.ID && a.PayrollPeriodID == period.ID select a).FirstOrDefault();
                        if (oDoc != null)
                        {
                            lastIncentiveValue = Convert.ToDecimal(oDoc.TaxableAmount);
                        }
                    }
                    incentiveValue = Math.Abs(currentYearExpectedTax - currentYearExpectedTaxI);

                    outValue += (incentiveValue - lastIncentiveValue);
                    if (incentiveValue > 0 && TaxDetailID != 0)
                    {
                        QuarterlyValue = (incentiveValue - lastIncentiveValue);
                        //string strQuery6 = "UPDATE dbo.TrnsQuarterTaxAdjDetail SET TaxableAmount = '" + incentiveValue.ToString() + "' WHERE dbo.TrnsQuarterTaxAdjDetail.ID = '"+ TaxDetailID.ToString() +"'";
                        //ExecuteQueries(strQuery6);
                    }
                    else
                    {
                        QuarterlyValue = 0.0M;
                    }
                }
            }

            oDB.AddTaxLog(emp.EmpID,
                Convert.ToString(emp.FirstName + emp.MiddleName + emp.LastName),
                Convert.ToString(emp.BasicSalary),
                Convert.ToString(empGrossSalary),
                Convert.ToString(PeriodTaxbleIncome),
                Convert.ToString(PeriodTaxbleLWOP),
                Convert.ToString(PeriodTaxbleIncomeOT),
                Convert.ToString(currentYearExpectedIncome),
                Convert.ToString(currentYearExpectedTax),
                Convert.ToString(currentYearExpectedIncomeI),
                Convert.ToString(currentYearExpectedTaxI),
                Convert.ToString(obIncome),
                Convert.ToString(obTax),
                Convert.ToString(projectedIncome), Convert.ToString(alreadyProcessedIncome),
                Convert.ToString(period.PeriodName),
                Convert.ToString(TaxQuarterlyComplete), Convert.ToString(incentiveValue - lastIncentiveValue),
                Convert.ToString(TaxDiscountYearly), Convert.ToString(TaxDiscountMonthly),
                Convert.ToString(TaxCode), Convert.ToString(TaxMin), Convert.ToString(TaxMax),
                Convert.ToString(TaxFix), Convert.ToString(TaxPer),
                Convert.ToString(TaxCodeI), Convert.ToString(TaxMinI), Convert.ToString(TaxMaxI),
                Convert.ToString(TaxFixI), Convert.ToString(TaxPerI),
                Convert.ToString(salaryTaxValue), Convert.ToString(outValue), "Incentive", "System"
                );

            return outValue;
        }

        public decimal getEmployeeTaxAmountIncentivePayment(CfgPeriodDates period, MstEmployee emp, decimal PeriodTaxbleIncomeOT, decimal PeriodTaxbleLWOP, decimal PeriodTaxbleIncome, decimal NonRecurringPyable, decimal empGrossSalary, decimal PayRatioCondition, out decimal QuarterlyValue)
        {
            int remainingPeriodsCnt = 0;
            decimal outValue = 0.00M;
            decimal salaryTaxValue = 0.0M;
            decimal alreadyProcessedIncome = 0.00M;
            decimal alreadyProcessedTax = 0.00M;
            decimal projectedIncome = 0.00M;
            decimal currentYearExpectedIncome = 0.00M;
            decimal currentYearExpectedTax = 0.00M;
            Decimal TaxDiscountYearly = 0.0M;
            Decimal TaxDiscountMonthly = 0.0M;
            Decimal FullTaxPaymentInThisMonth = 0.0M;
            Decimal TaxValueIncentive = 0.0M;
            decimal PreviosIncentives = 0.0M;
            decimal incentiveValue = 0.0M;
            string TaxCode = "", TaxMin = "", TaxMax = "", TaxPer = "", TaxFix = "";
            string TaxCodeI = "", TaxMinI = "", TaxMaxI = "", TaxPerI = "", TaxFixI = "";


            #region Tax Adjustment Section
            String strQuery = @"SELECT ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS Value
                                FROM dbo.TrnsTaxAdjustment AS A1 INNER JOIN dbo.TrnsTaxAdjustmentDetails AS A2 ON A1.ID = A2.TAID
	                                 INNER JOIN dbo.MstEmployee A3 ON A1.empID = A3.ID
                                WHERE 
	                                ISNULL(A1.flgActive,'0') = '1' AND
	                                ISNULL(A2.flgActive,'0') = '1' AND
                                    ISNULL(A2.flgMonthly,'0') = '0' AND  
	                                A3.EmpID = '" + emp.EmpID + "'";


            TaxDiscountYearly = Convert.ToDecimal(getScallerValue(strQuery));


            String strQuery2 = @"SELECT ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS Value
                                FROM dbo.TrnsTaxAdjustment AS A1 INNER JOIN dbo.TrnsTaxAdjustmentDetails AS A2 ON A1.ID = A2.TAID
	                                 INNER JOIN dbo.MstEmployee A3 ON A1.empID = A3.ID
                                WHERE 
	                                ISNULL(A1.flgActive,'0') = '1' AND
	                                ISNULL(A2.flgActive,'0') = '1' AND
                                    ISNULL(A2.flgMonthly,'0') = '1' AND  
	                                A3.EmpID = '" + emp.EmpID + "'";

            TaxDiscountMonthly = Convert.ToDecimal(getScallerValue(strQuery2));

            #endregion

            String strQuery3 = @"
                                SELECT 
	                                ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS Amount
                                FROM 
	                                dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID
                                WHERE
	                                A1.empID = '" + emp.ID + @"'
	                                AND A2.PayrollPeriodID = '" + period.ID + "'";

            FullTaxPaymentInThisMonth = Convert.ToDecimal(getScallerValue(strQuery3));

            //String strQuery5 = @"SELECT 	ISNULL(SUM(ISNULL(A2.TaxableAmount,0)),0) AS TaxableAmountI FROM dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID WHERE A1.empID = '"+emp.ID+"' AND A2.PayrollPeriodID < '" + period.ID + "'";
            string strQuery5 = @"SELECT 	ISNULL(SUM(ISNULL(A2.TaxableAmount,0)),0) AS TaxableAmountI FROM dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID INNER JOIN dbo.CfgPeriodDates A3 ON A2.PayrollPeriodID = A3.ID WHERE A1.empID = '" + emp.ID.ToString() + "' AND A3.ID < '" + period.ID.ToString() + "' AND A3.CalCode = '" + period.CalCode.ToString().Trim() + "'";
            TaxValueIncentive = Convert.ToInt32(getScallerValue(strQuery5));

            string strQuery6 = @"SELECT 	ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS TaxableAmountI FROM dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID INNER JOIN dbo.CfgPeriodDates A3 ON A2.PayrollPeriodID = A3.ID WHERE A1.empID = '" + emp.ID.ToString() + "' AND A3.ID < '" + period.ID.ToString() + "' AND A3.CalCode = '" + period.CalCode.ToString().Trim() + "'";
            PreviosIncentives = Convert.ToDecimal(getScallerValue(strQuery6));

            if (FullTaxPaymentInThisMonth > 0)
            {
                QuarterlyValue = FullTaxPaymentInThisMonth;
            }
            else
            {
                QuarterlyValue = 0.0M;
            }

            //End Section
            //string strPrevious = "SELECT SUM (ISNULL(dbo.TrnsSalaryProcessRegister.EmpTaxblTotal,0)) AS TaxbleAmount, SUM(ISNULL(dbo.TrnsSalaryProcessRegister.EmpTotalTax, 0)) AS PaidTax,  ";
            //strPrevious += "                     dbo.TrnsSalaryProcessRegister.EmpID";
            //strPrevious += " FROM dbo.TrnsSalaryProcessRegister INNER JOIN";
            //strPrevious += " dbo.CfgPeriodDates ON dbo.TrnsSalaryProcessRegister.PayrollPeriodID = dbo.CfgPeriodDates.ID";
            //strPrevious += " WHERE     (dbo.TrnsSalaryProcessRegister.EmpID = " + emp.ID.ToString() + ") AND (dbo.CfgPeriodDates.CalCode = '" + period.CalCode + "')";
            //strPrevious += " GROUP BY dbo.TrnsSalaryProcessRegister.EmpID";

            string strPrevious = "SELECT SUM(dbo.TrnsSalaryProcessRegister.EmpTaxblTotal) AS TaxbleAmount, SUM(ISNULL(dbo.TrnsSalaryProcessRegister.EmpTotalTax, 0)) AS PaidTax,  ";
            strPrevious += "                     dbo.TrnsSalaryProcessRegister.EmpID";
            strPrevious += " FROM dbo.TrnsSalaryProcessRegister INNER JOIN";
            strPrevious += " dbo.CfgPeriodDates ON dbo.TrnsSalaryProcessRegister.PayrollPeriodID = dbo.CfgPeriodDates.ID";
            strPrevious += " WHERE     (dbo.TrnsSalaryProcessRegister.EmpID = " + emp.ID.ToString() + ") AND (dbo.CfgPeriodDates.CalCode = '" + period.CalCode + "')";
            strPrevious += " GROUP BY dbo.TrnsSalaryProcessRegister.EmpID";

            DataTable dtPreviousInfo = getDataTable(strPrevious);

            remainingPeriodsCnt = (from p in oDB.CfgPeriodDates where p.ID >= period.ID && p.PayrollId == emp.PayrollID && p.CalCode == period.CalCode select p).Count();

            string strTaxOb = " select isnull(SUM(TaxBalance),0) as ObTax from TrnsOBTax where EmpID = '" + emp.ID.ToString() + "'";
            decimal obTax = Convert.ToDecimal(getScallerValue(strTaxOb));


            string strIncomeOb = " select isnull(SUM(SalaryBalance),0) as ObTax from TrnsOBSalary where EmpID = '" + emp.ID.ToString() + "'";
            decimal obIncome = Convert.ToDecimal(getScallerValue(strIncomeOb));


            string strIncomeObAdj = " select isnull(SUM(Amount),0) as ObTax from TrnsObSalaryAdj where EmpID = '" + emp.ID.ToString() + "' AND Isnull(FlgActive, 0) = 1 ";
            decimal obIncomeAdjusted = Convert.ToDecimal(getScallerValue(strIncomeObAdj));

            obIncome += obIncomeAdjusted;

            alreadyProcessedIncome = obIncome;
            alreadyProcessedTax = obTax;


            foreach (DataRow dr in dtPreviousInfo.Rows)
            {
                alreadyProcessedIncome += Convert.ToDecimal(dtPreviousInfo.Rows[0]["TaxbleAmount"]);
                alreadyProcessedTax += Convert.ToDecimal(dtPreviousInfo.Rows[0]["PaidTax"]);

            }


            //TODO: For 1st month of calendar year for specific days
            if (alreadyProcessedIncome == 0 && PayRatioCondition != 1)
            {
                //projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) * ((remainingPeriodsCnt-1)*empGrossSalary)) + NonRecurringPyable + PeriodTaxbleIncomeOT + PeriodTaxbleLWOP;
                //projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) + ((remainingPeriodsCnt - 1) * PeriodTaxbleIncome)) + NonRecurringPyable + PeriodTaxbleLWOP + PeriodTaxbleIncomeOT;
                projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) + ((remainingPeriodsCnt - 1) * empGrossSalary)) + NonRecurringPyable + PeriodTaxbleLWOP + PeriodTaxbleIncomeOT;
                //projectedIncome += TaxQuarterlyComplete;
            }
            else
            {
                projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) * remainingPeriodsCnt) + NonRecurringPyable + PeriodTaxbleIncomeOT + PeriodTaxbleLWOP;
                //projectedIncome += TaxQuarterlyComplete;
            }

            if (TaxDiscountYearly == 0)
            {
                currentYearExpectedIncome = alreadyProcessedIncome + projectedIncome;
            }
            else
            {
                currentYearExpectedIncome = alreadyProcessedIncome + projectedIncome + TaxDiscountYearly;
            }
            if (PreviosIncentives > 0)
                currentYearExpectedIncome += PreviosIncentives;

            int cnt = (from p in oDB.CfgTaxDetail
                       where p.CfgTaxSetup.MstCalendar.Code == period.CalCode
                       where p.MinAmount <= currentYearExpectedIncome
                       && p.MaxAmount >= currentYearExpectedIncome
                       select p).Count();
            if (cnt > 0)
            {
                CfgTaxDetail taxLine;
                //if (currentYearExpectedIncome > 1200000 && currentYearExpectedIncome <= 2500000)
                //{
                //    taxLine = (from p in oDB.CfgTaxDetail
                //               where p.CfgTaxSetup.MstCalendar.Code == period.CalCode
                //               where p.MinAmount > 1200000
                //               && p.MaxAmount <= 2500000
                //               select p).FirstOrDefault();
                //}
                //else
                //{
                taxLine = (from p in oDB.CfgTaxDetail
                           where p.CfgTaxSetup.MstCalendar.Code == period.CalCode
                           where p.MinAmount <= currentYearExpectedIncome
                           && p.MaxAmount >= currentYearExpectedIncome
                           select p).FirstOrDefault();
                //}
                TaxCode = taxLine.TaxCode;
                TaxMin = Convert.ToString(taxLine.MinAmount);
                TaxMax = Convert.ToString(taxLine.MaxAmount);
                TaxFix = Convert.ToString(taxLine.FixTerm);
                TaxPer = Convert.ToString(taxLine.TaxValue);
                decimal PercentageAmount = 0;
                //if (currentYearExpectedIncome > 1200000 && currentYearExpectedIncome <= 2500000)
                //{
                //    PercentageAmount = (decimal)(currentYearExpectedIncome - taxLine.MinAmount) * (decimal)taxLine.TaxValue / 100;
                //    if (PercentageAmount > Convert.ToDecimal(TaxFix))
                //    {
                //        currentYearExpectedTax = PercentageAmount;
                //    }
                //    else
                //    {
                //        currentYearExpectedTax = Convert.ToDecimal(TaxFix);
                //    }
                //}
                //else
                //{
                currentYearExpectedTax = (decimal)taxLine.FixTerm + (decimal)(currentYearExpectedIncome - taxLine.MinAmount) * (decimal)taxLine.TaxValue / 100;
                //}

                salaryTaxValue = (currentYearExpectedTax - (alreadyProcessedTax)) / remainingPeriodsCnt;
                if (TaxDiscountMonthly != 0)
                {
                    outValue = salaryTaxValue + TaxDiscountMonthly;
                }
                else
                {
                    outValue = salaryTaxValue;
                }

                if (FullTaxPaymentInThisMonth > 0)
                {
                    incentiveValue = Convert.ToDecimal((from a in oDB.TrnsQuarterTaxAdjDetail where a.TrnsQuarterTaxAdj.EmpID == emp.ID && a.PayrollPeriodID == period.ID select a.TaxableAmount).Sum());
                    outValue += incentiveValue;
                    if (incentiveValue > 0)
                    {
                        QuarterlyValue = incentiveValue;
                    }
                    else
                    {
                        QuarterlyValue = 0.0M;
                    }
                }
            }

            oDB.AddTaxLog(emp.EmpID,
                Convert.ToString(emp.FirstName + emp.MiddleName + emp.LastName),
                Convert.ToString(emp.BasicSalary),
                Convert.ToString(empGrossSalary),
                Convert.ToString(PeriodTaxbleIncome),
                Convert.ToString(PeriodTaxbleLWOP),
                Convert.ToString(PeriodTaxbleIncomeOT),
                Convert.ToString(currentYearExpectedIncome),
                Convert.ToString(currentYearExpectedTax),
                "0",
                "0",
                Convert.ToString(obIncome),
                Convert.ToString(obTax),
                Convert.ToString(projectedIncome), Convert.ToString(alreadyProcessedIncome),
                Convert.ToString(period.PeriodName),
                Convert.ToString(FullTaxPaymentInThisMonth), Convert.ToString(incentiveValue),
                Convert.ToString(TaxDiscountYearly), Convert.ToString(TaxDiscountMonthly),
                Convert.ToString(TaxCode), Convert.ToString(TaxMin), Convert.ToString(TaxMax),
                Convert.ToString(TaxFix), Convert.ToString(TaxPer),
                Convert.ToString(TaxCodeI), Convert.ToString(TaxMinI), Convert.ToString(TaxMaxI),
                Convert.ToString(TaxFixI), Convert.ToString(TaxPerI),
                Convert.ToString(salaryTaxValue), Convert.ToString(outValue), "Salary", "System"
                );


            return outValue;
        }

        public decimal getEmployeeTaxAmountIncentivePayment1819(CfgPeriodDates period, MstEmployee emp, decimal PeriodTaxbleIncomeOT, decimal PeriodTaxbleLWOP, decimal PeriodTaxbleIncome, decimal NonRecurringPyable, decimal empGrossSalary, decimal PayRatioCondition, out decimal QuarterlyValue)
        {
            int remainingPeriodsCnt = 0;
            decimal outValue = 0.00M;
            decimal salaryTaxValue = 0.0M;
            decimal alreadyProcessedIncome = 0.00M;
            decimal alreadyProcessedTax = 0.00M;
            decimal projectedIncome = 0.00M;
            decimal currentYearExpectedIncome = 0.00M;
            decimal currentYearExpectedTax = 0.00M;
            Decimal TaxDiscountYearly = 0.0M;
            Decimal TaxDiscountMonthly = 0.0M;
            Decimal FullTaxPaymentInThisMonth = 0.0M;
            Decimal TaxValueIncentive = 0.0M;
            decimal PreviosIncentives = 0.0M;
            decimal incentiveValue = 0.0M;
            string TaxCode = "", TaxMin = "", TaxMax = "", TaxPer = "", TaxFix = "";
            string TaxCodeI = "", TaxMinI = "", TaxMaxI = "", TaxPerI = "", TaxFixI = "";


            #region Tax Adjustment Section
            String strQuery = @"SELECT ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS Value
                                FROM dbo.TrnsTaxAdjustment AS A1 INNER JOIN dbo.TrnsTaxAdjustmentDetails AS A2 ON A1.ID = A2.TAID
	                                 INNER JOIN dbo.MstEmployee A3 ON A1.empID = A3.ID
                                WHERE 
	                                ISNULL(A1.flgActive,'0') = '1' AND
	                                ISNULL(A2.flgActive,'0') = '1' AND
                                    ISNULL(A2.flgMonthly,'0') = '0' AND  
	                                A3.EmpID = '" + emp.EmpID + "'";


            TaxDiscountYearly = Convert.ToDecimal(getScallerValue(strQuery));


            String strQuery2 = @"SELECT ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS Value
                                FROM dbo.TrnsTaxAdjustment AS A1 INNER JOIN dbo.TrnsTaxAdjustmentDetails AS A2 ON A1.ID = A2.TAID
	                                 INNER JOIN dbo.MstEmployee A3 ON A1.empID = A3.ID
                                WHERE 
	                                ISNULL(A1.flgActive,'0') = '1' AND
	                                ISNULL(A2.flgActive,'0') = '1' AND
                                    ISNULL(A2.flgMonthly,'0') = '1' AND  
	                                A3.EmpID = '" + emp.EmpID + "'";

            TaxDiscountMonthly = Convert.ToDecimal(getScallerValue(strQuery2));

            #endregion

            String strQuery3 = @"
                                SELECT 
	                                ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS Amount
                                FROM 
	                                dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID
                                WHERE
	                                A1.empID = '" + emp.ID + @"'
	                                AND A2.PayrollPeriodID = '" + period.ID + "'";

            FullTaxPaymentInThisMonth = Convert.ToDecimal(getScallerValue(strQuery3));

            //String strQuery5 = @"SELECT 	ISNULL(SUM(ISNULL(A2.TaxableAmount,0)),0) AS TaxableAmountI FROM dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID WHERE A1.empID = '"+emp.ID+"' AND A2.PayrollPeriodID < '" + period.ID + "'";
            string strQuery5 = @"SELECT 	ISNULL(SUM(ISNULL(A2.TaxableAmount,0)),0) AS TaxableAmountI FROM dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID INNER JOIN dbo.CfgPeriodDates A3 ON A2.PayrollPeriodID = A3.ID WHERE A1.empID = '" + emp.ID.ToString() + "' AND A3.ID < '" + period.ID.ToString() + "' AND A3.CalCode = '" + period.CalCode.ToString().Trim() + "'";
            TaxValueIncentive = Convert.ToInt32(getScallerValue(strQuery5));

            string strQuery6 = @"SELECT 	ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS TaxableAmountI FROM dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID INNER JOIN dbo.CfgPeriodDates A3 ON A2.PayrollPeriodID = A3.ID WHERE A1.empID = '" + emp.ID.ToString() + "' AND A3.ID < '" + period.ID.ToString() + "' AND A3.CalCode = '" + period.CalCode.ToString().Trim() + "'";
            PreviosIncentives = Convert.ToDecimal(getScallerValue(strQuery6));

            if (FullTaxPaymentInThisMonth > 0)
            {
                QuarterlyValue = FullTaxPaymentInThisMonth;
            }
            else
            {
                QuarterlyValue = 0.0M;
            }

            //End Section
            //string strPrevious = "SELECT SUM (ISNULL(dbo.TrnsSalaryProcessRegister.EmpTaxblTotal,0)) AS TaxbleAmount, SUM(ISNULL(dbo.TrnsSalaryProcessRegister.EmpTotalTax, 0)) AS PaidTax,  ";
            //strPrevious += "                     dbo.TrnsSalaryProcessRegister.EmpID";
            //strPrevious += " FROM dbo.TrnsSalaryProcessRegister INNER JOIN";
            //strPrevious += " dbo.CfgPeriodDates ON dbo.TrnsSalaryProcessRegister.PayrollPeriodID = dbo.CfgPeriodDates.ID";
            //strPrevious += " WHERE     (dbo.TrnsSalaryProcessRegister.EmpID = " + emp.ID.ToString() + ") AND (dbo.CfgPeriodDates.CalCode = '" + period.CalCode + "')";
            //strPrevious += " GROUP BY dbo.TrnsSalaryProcessRegister.EmpID";

            string strPrevious = "SELECT SUM(dbo.TrnsSalaryProcessRegister.EmpTaxblTotal) AS TaxbleAmount, SUM(ISNULL(dbo.TrnsSalaryProcessRegister.EmpTotalTax, 0)) AS PaidTax,  ";
            strPrevious += "                     dbo.TrnsSalaryProcessRegister.EmpID";
            strPrevious += " FROM dbo.TrnsSalaryProcessRegister INNER JOIN";
            strPrevious += " dbo.CfgPeriodDates ON dbo.TrnsSalaryProcessRegister.PayrollPeriodID = dbo.CfgPeriodDates.ID";
            strPrevious += " WHERE     (dbo.TrnsSalaryProcessRegister.EmpID = " + emp.ID.ToString() + ") AND (dbo.CfgPeriodDates.CalCode = '" + period.CalCode + "')";
            strPrevious += " GROUP BY dbo.TrnsSalaryProcessRegister.EmpID";

            DataTable dtPreviousInfo = getDataTable(strPrevious);

            remainingPeriodsCnt = (from p in oDB.CfgPeriodDates where p.ID >= period.ID && p.PayrollId == emp.PayrollID && p.CalCode == period.CalCode select p).Count();

            string strTaxOb = " select isnull(SUM(TaxBalance),0) as ObTax from TrnsOBTax where EmpID = '" + emp.ID.ToString() + "'";
            decimal obTax = Convert.ToDecimal(getScallerValue(strTaxOb));


            string strIncomeOb = " select isnull(SUM(SalaryBalance),0) as ObTax from TrnsOBSalary where EmpID = '" + emp.ID.ToString() + "'";
            decimal obIncome = Convert.ToDecimal(getScallerValue(strIncomeOb));


            string strIncomeObAdj = " select isnull(SUM(Amount),0) as ObTax from TrnsObSalaryAdj where EmpID = '" + emp.ID.ToString() + "' AND Isnull(FlgActive, 0) = 1 ";
            decimal obIncomeAdjusted = Convert.ToDecimal(getScallerValue(strIncomeObAdj));

            obIncome += obIncomeAdjusted;

            alreadyProcessedIncome = obIncome;
            alreadyProcessedTax = obTax;


            foreach (DataRow dr in dtPreviousInfo.Rows)
            {
                alreadyProcessedIncome += Convert.ToDecimal(dtPreviousInfo.Rows[0]["TaxbleAmount"]);
                alreadyProcessedTax += Convert.ToDecimal(dtPreviousInfo.Rows[0]["PaidTax"]);

            }


            //TODO: For 1st month of calendar year for specific days
            if (alreadyProcessedIncome == 0 && PayRatioCondition != 1)
            {
                //projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) * ((remainingPeriodsCnt-1)*empGrossSalary)) + NonRecurringPyable + PeriodTaxbleIncomeOT + PeriodTaxbleLWOP;
                //projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) + ((remainingPeriodsCnt - 1) * PeriodTaxbleIncome)) + NonRecurringPyable + PeriodTaxbleLWOP + PeriodTaxbleIncomeOT;
                projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) + ((remainingPeriodsCnt - 1) * empGrossSalary)) + NonRecurringPyable + PeriodTaxbleLWOP + PeriodTaxbleIncomeOT;
                //projectedIncome += TaxQuarterlyComplete;
            }
            else
            {
                projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) * remainingPeriodsCnt) + NonRecurringPyable + PeriodTaxbleIncomeOT + PeriodTaxbleLWOP;
                //projectedIncome += TaxQuarterlyComplete;
            }

            if (TaxDiscountYearly == 0)
            {
                currentYearExpectedIncome = alreadyProcessedIncome + projectedIncome;
            }
            else
            {
                currentYearExpectedIncome = alreadyProcessedIncome + projectedIncome + TaxDiscountYearly;
            }
            if (PreviosIncentives > 0)
                currentYearExpectedIncome += PreviosIncentives;

            int cnt = (from p in oDB.CfgTaxDetail
                       where p.CfgTaxSetup.MstCalendar.Code == period.CalCode
                       where p.MinAmount <= currentYearExpectedIncome
                       && p.MaxAmount >= currentYearExpectedIncome
                       select p).Count();
            if (cnt > 0)
            {
                CfgTaxDetail taxLine;
                if (currentYearExpectedIncome > 1200000 && currentYearExpectedIncome <= 2500000)
                {
                    taxLine = (from p in oDB.CfgTaxDetail
                               where p.CfgTaxSetup.MstCalendar.Code == period.CalCode
                               where p.MinAmount > 1200000
                               && p.MaxAmount <= 2500000
                               select p).FirstOrDefault();
                }
                else
                {
                    taxLine = (from p in oDB.CfgTaxDetail
                               where p.CfgTaxSetup.MstCalendar.Code == period.CalCode
                               where p.MinAmount <= currentYearExpectedIncome
                               && p.MaxAmount >= currentYearExpectedIncome
                               select p).Single();
                }
                TaxCode = taxLine.TaxCode;
                TaxMin = Convert.ToString(taxLine.MinAmount);
                TaxMax = Convert.ToString(taxLine.MaxAmount);
                TaxFix = Convert.ToString(taxLine.FixTerm);
                TaxPer = Convert.ToString(taxLine.TaxValue);
                decimal PercentageAmount = 0;
                if (currentYearExpectedIncome > 1200000 && currentYearExpectedIncome <= 2500000)
                {
                    PercentageAmount = (decimal)(currentYearExpectedIncome - taxLine.MinAmount) * (decimal)taxLine.TaxValue / 100;
                    if (PercentageAmount > Convert.ToDecimal(TaxFix))
                    {
                        currentYearExpectedTax = PercentageAmount;
                    }
                    else
                    {
                        currentYearExpectedTax = Convert.ToDecimal(TaxFix);
                    }
                }
                else
                {
                    currentYearExpectedTax = (decimal)taxLine.FixTerm + (decimal)(currentYearExpectedIncome - taxLine.MinAmount) * (decimal)taxLine.TaxValue / 100;
                }

                salaryTaxValue = (currentYearExpectedTax - (alreadyProcessedTax)) / remainingPeriodsCnt;
                if (TaxDiscountMonthly != 0)
                {
                    outValue = salaryTaxValue + TaxDiscountMonthly;
                }
                else
                {
                    outValue = salaryTaxValue;
                }

                if (FullTaxPaymentInThisMonth > 0)
                {
                    incentiveValue = Convert.ToDecimal((from a in oDB.TrnsQuarterTaxAdjDetail where a.TrnsQuarterTaxAdj.EmpID == emp.ID && a.PayrollPeriodID == period.ID select a.TaxableAmount).Sum());
                    outValue += incentiveValue;
                    if (incentiveValue > 0)
                    {
                        QuarterlyValue = incentiveValue;
                    }
                    else
                    {
                        QuarterlyValue = 0.0M;
                    }
                }
            }

            oDB.AddTaxLog(emp.EmpID,
                Convert.ToString(emp.FirstName + emp.MiddleName + emp.LastName),
                Convert.ToString(emp.BasicSalary),
                Convert.ToString(empGrossSalary),
                Convert.ToString(PeriodTaxbleIncome),
                Convert.ToString(PeriodTaxbleLWOP),
                Convert.ToString(PeriodTaxbleIncomeOT),
                Convert.ToString(currentYearExpectedIncome),
                Convert.ToString(currentYearExpectedTax),
                "0",
                "0",
                Convert.ToString(obIncome),
                Convert.ToString(obTax),
                Convert.ToString(projectedIncome), Convert.ToString(alreadyProcessedIncome),
                Convert.ToString(period.PeriodName),
                Convert.ToString(FullTaxPaymentInThisMonth), Convert.ToString(incentiveValue),
                Convert.ToString(TaxDiscountYearly), Convert.ToString(TaxDiscountMonthly),
                Convert.ToString(TaxCode), Convert.ToString(TaxMin), Convert.ToString(TaxMax),
                Convert.ToString(TaxFix), Convert.ToString(TaxPer),
                Convert.ToString(TaxCodeI), Convert.ToString(TaxMinI), Convert.ToString(TaxMaxI),
                Convert.ToString(TaxFixI), Convert.ToString(TaxPerI),
                Convert.ToString(salaryTaxValue), Convert.ToString(outValue), "Salary", "System"
                );


            return outValue;
        }

        public decimal getEmployeeTaxAmountEgytianLaw(CfgPeriodDates period, MstEmployee emp, decimal PeriodTaxbleIncomeOT, decimal PeriodTaxbleLWOP, decimal PeriodTaxbleIncome, decimal NonRecurringPyable, decimal empGrossSalary, decimal PayRatioCondition, out decimal QuarterlyValue)
        {
            int remainingPeriodsCnt = 0;
            decimal outValue = 0.00M;
            decimal salaryTaxValue = 0.0M;
            decimal alreadyProcessedIncome = 0.00M;
            decimal alreadyProcessedTax = 0.00M;
            decimal projectedIncome = 0.00M;
            decimal currentYearExpectedIncome = 0.00M;
            decimal currentYearExpectedTax = 0.00M;
            Decimal TaxDiscountYearly = 0.0M;
            Decimal TaxDiscountMonthly = 0.0M;
            Decimal FullTaxPaymentInThisMonth = 0.0M;
            Decimal TaxValueIncentive = 0.0M;
            decimal PreviosIncentives = 0.0M;
            decimal incentiveValue = 0.0M;
            string TaxCode = "";
            decimal TaxMin = 0, TaxMax = 0, TaxPer = 0, TaxFix = 0, TaxDisc = 0;
            string TaxCodeI = "", TaxMinI = "", TaxMaxI = "", TaxPerI = "", TaxFixI = "";


            #region Tax Adjustment Section
            String strQuery = @"SELECT ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS Value
                                FROM dbo.TrnsTaxAdjustment AS A1 INNER JOIN dbo.TrnsTaxAdjustmentDetails AS A2 ON A1.ID = A2.TAID
	                                 INNER JOIN dbo.MstEmployee A3 ON A1.empID = A3.ID
                                WHERE 
	                                ISNULL(A1.flgActive,'0') = '1' AND
	                                ISNULL(A2.flgActive,'0') = '1' AND
                                    ISNULL(A2.flgMonthly,'0') = '0' AND  
	                                A3.EmpID = '" + emp.EmpID + "'";


            TaxDiscountYearly = Convert.ToDecimal(getScallerValue(strQuery));


            String strQuery2 = @"SELECT ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS Value
                                FROM dbo.TrnsTaxAdjustment AS A1 INNER JOIN dbo.TrnsTaxAdjustmentDetails AS A2 ON A1.ID = A2.TAID
	                                 INNER JOIN dbo.MstEmployee A3 ON A1.empID = A3.ID
                                WHERE 
	                                ISNULL(A1.flgActive,'0') = '1' AND
	                                ISNULL(A2.flgActive,'0') = '1' AND
                                    ISNULL(A2.flgMonthly,'0') = '1' AND  
	                                A3.EmpID = '" + emp.EmpID + "'";

            TaxDiscountMonthly = Convert.ToDecimal(getScallerValue(strQuery2));

            #endregion

            String strQuery3 = @"
                                SELECT 
	                                ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS Amount
                                FROM 
	                                dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID
                                WHERE
	                                A1.empID = '" + emp.ID + @"'
	                                AND A2.PayrollPeriodID = '" + period.ID + "'";

            FullTaxPaymentInThisMonth = Convert.ToDecimal(getScallerValue(strQuery3));

            //String strQuery5 = @"SELECT 	ISNULL(SUM(ISNULL(A2.TaxableAmount,0)),0) AS TaxableAmountI FROM dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID WHERE A1.empID = '"+emp.ID+"' AND A2.PayrollPeriodID < '" + period.ID + "'";
            string strQuery5 = @"SELECT 	ISNULL(SUM(ISNULL(A2.TaxableAmount,0)),0) AS TaxableAmountI FROM dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID INNER JOIN dbo.CfgPeriodDates A3 ON A2.PayrollPeriodID = A3.ID WHERE A1.empID = '" + emp.ID.ToString() + "' AND A3.ID < '" + period.ID.ToString() + "' AND A3.CalCode = '" + period.CalCode.ToString().Trim() + "'";
            TaxValueIncentive = Convert.ToInt32(getScallerValue(strQuery5));

            string strQuery6 = @"SELECT 	ISNULL(SUM(ISNULL(A2.Amount,0)),0) AS TaxableAmountI FROM dbo.TrnsQuarterTaxAdj A1 INNER JOIN dbo.TrnsQuarterTaxAdjDetail A2 ON A1.ID = A2.QTAID INNER JOIN dbo.CfgPeriodDates A3 ON A2.PayrollPeriodID = A3.ID WHERE A1.empID = '" + emp.ID.ToString() + "' AND A3.ID < '" + period.ID.ToString() + "' AND A3.CalCode = '" + period.CalCode.ToString().Trim() + "'";
            PreviosIncentives = Convert.ToDecimal(getScallerValue(strQuery6));

            if (FullTaxPaymentInThisMonth > 0)
            {
                QuarterlyValue = FullTaxPaymentInThisMonth;
            }
            else
            {
                QuarterlyValue = 0.0M;
            }

            //End Section
            //string strPrevious = "SELECT SUM (ISNULL(dbo.TrnsSalaryProcessRegister.EmpTaxblTotal,0)) AS TaxbleAmount, SUM(ISNULL(dbo.TrnsSalaryProcessRegister.EmpTotalTax, 0)) AS PaidTax,  ";
            //strPrevious += "                     dbo.TrnsSalaryProcessRegister.EmpID";
            //strPrevious += " FROM dbo.TrnsSalaryProcessRegister INNER JOIN";
            //strPrevious += " dbo.CfgPeriodDates ON dbo.TrnsSalaryProcessRegister.PayrollPeriodID = dbo.CfgPeriodDates.ID";
            //strPrevious += " WHERE     (dbo.TrnsSalaryProcessRegister.EmpID = " + emp.ID.ToString() + ") AND (dbo.CfgPeriodDates.CalCode = '" + period.CalCode + "')";
            //strPrevious += " GROUP BY dbo.TrnsSalaryProcessRegister.EmpID";

            string strPrevious = "SELECT SUM(dbo.TrnsSalaryProcessRegister.EmpTaxblTotal) AS TaxbleAmount, SUM(ISNULL(dbo.TrnsSalaryProcessRegister.EmpTotalTax, 0)) AS PaidTax,  ";
            strPrevious += "                     dbo.TrnsSalaryProcessRegister.EmpID";
            strPrevious += " FROM dbo.TrnsSalaryProcessRegister INNER JOIN";
            strPrevious += " dbo.CfgPeriodDates ON dbo.TrnsSalaryProcessRegister.PayrollPeriodID = dbo.CfgPeriodDates.ID";
            strPrevious += " WHERE     (dbo.TrnsSalaryProcessRegister.EmpID = " + emp.ID.ToString() + ") AND (dbo.CfgPeriodDates.CalCode = '" + period.CalCode + "')";
            strPrevious += " GROUP BY dbo.TrnsSalaryProcessRegister.EmpID";

            DataTable dtPreviousInfo = getDataTable(strPrevious);

            remainingPeriodsCnt = (from p in oDB.CfgPeriodDates where p.ID >= period.ID && p.PayrollId == emp.PayrollID && p.CalCode == period.CalCode select p).Count();

            string strTaxOb = " select isnull(SUM(TaxBalance),0) as ObTax from TrnsOBTax where EmpID = '" + emp.ID.ToString() + "'";
            decimal obTax = Convert.ToDecimal(getScallerValue(strTaxOb));


            string strIncomeOb = " select isnull(SUM(SalaryBalance),0) as ObTax from TrnsOBSalary where EmpID = '" + emp.ID.ToString() + "'";
            decimal obIncome = Convert.ToDecimal(getScallerValue(strIncomeOb));


            string strIncomeObAdj = " select isnull(SUM(Amount),0) as ObTax from TrnsObSalaryAdj where EmpID = '" + emp.ID.ToString() + "' AND Isnull(FlgActive, 0) = 1 ";
            decimal obIncomeAdjusted = Convert.ToDecimal(getScallerValue(strIncomeObAdj));

            obIncome += obIncomeAdjusted;

            alreadyProcessedIncome = obIncome;
            alreadyProcessedTax = obTax;


            foreach (DataRow dr in dtPreviousInfo.Rows)
            {
                alreadyProcessedIncome += Convert.ToDecimal(dtPreviousInfo.Rows[0]["TaxbleAmount"]);
                alreadyProcessedTax += Convert.ToDecimal(dtPreviousInfo.Rows[0]["PaidTax"]);

            }


            //TODO: For 1st month of calendar year for specific days
            if (alreadyProcessedIncome == 0 && PayRatioCondition != 1)
            {
                //projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) * ((remainingPeriodsCnt-1)*empGrossSalary)) + NonRecurringPyable + PeriodTaxbleIncomeOT + PeriodTaxbleLWOP;
                //projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) + ((remainingPeriodsCnt - 1) * PeriodTaxbleIncome)) + NonRecurringPyable + PeriodTaxbleLWOP + PeriodTaxbleIncomeOT;
                //projectedIncome += TaxQuarterlyComplete;
                //projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) + ((remainingPeriodsCnt - 1) * empGrossSalary)) + NonRecurringPyable + PeriodTaxbleLWOP + PeriodTaxbleIncomeOT;
                //projectedIncome = ((PeriodTaxbleIncome) + ((remainingPeriodsCnt - 1) * empGrossSalary)) ;
                projectedIncome = emp.GrossSalary.GetValueOrDefault() * 12;
            }
            else
            {
                //projectedIncome = ((PeriodTaxbleIncome - NonRecurringPyable - PeriodTaxbleIncomeOT - PeriodTaxbleLWOP) * remainingPeriodsCnt) + NonRecurringPyable + PeriodTaxbleIncomeOT + PeriodTaxbleLWOP;
                projectedIncome = ((PeriodTaxbleIncome ) * 12);
                //projectedIncome += TaxQuarterlyComplete;
            }

            if (TaxDiscountYearly == 0)
            {
                //currentYearExpectedIncome = alreadyProcessedIncome + projectedIncome;
                currentYearExpectedIncome = projectedIncome;
            }
            else
            {
                //currentYearExpectedIncome = alreadyProcessedIncome + projectedIncome + TaxDiscountYearly;
                currentYearExpectedIncome = projectedIncome + TaxDiscountYearly;
            }
            if (PreviosIncentives > 0)
                currentYearExpectedIncome += PreviosIncentives;
            //New working for Transtec Only
            //currentYearExpectedIncome = emp.GrossSalary.GetValueOrDefault() * 12;
            // End of working
            int cnt = (from p in oDB.CfgTaxDetail
                       where p.CfgTaxSetup.MstCalendar.Code == period.CalCode
                       && p.MinAmount <= currentYearExpectedIncome
                       && p.MaxAmount >= currentYearExpectedIncome
                       select p).Count();
            if (cnt > 0)
            {
                IEnumerable<CfgTaxDetail> oSlabs;
                oSlabs = (from p in oDB.CfgTaxDetail
                          where p.CfgTaxSetup.MstCalendar.Code == period.CalCode
                          && p.MinAmount <= currentYearExpectedIncome
                          orderby p.MinAmount
                          select p).ToList();

                List<decimal> PerSlabTax = new List<decimal>();
                foreach (var One in oSlabs)
                {
                    TaxCode = One.TaxCode;
                    TaxMin = Convert.ToDecimal(One.MinAmount);
                    TaxMax = Convert.ToDecimal(One.MaxAmount);
                    TaxFix = Convert.ToDecimal(One.FixTerm);
                    TaxPer = Convert.ToDecimal(One.TaxValue);
                    TaxDisc = Convert.ToDecimal(One.AdditionalDisc);

                    if (TaxPer == 0) continue;

                    decimal ThisSlabValue = 0;

                    if (currentYearExpectedIncome >= TaxMin && currentYearExpectedIncome <= TaxMax)
                    {
                        ThisSlabValue = ((currentYearExpectedIncome - TaxMin) + 1) * (TaxPer / 100);
                    }
                    else
                    {
                        ThisSlabValue = ((TaxMax - TaxMin) + 1) * (TaxPer / 100);
                    }
                    //if (TaxDisc > 0)
                    //{
                    //    ThisSlabValue = ThisSlabValue - (ThisSlabValue * (TaxDisc / 100));
                    //}
                    PerSlabTax.Add(ThisSlabValue);
                }
                if (TaxDisc > 0)
                {
                    decimal TotalSlabValue = 0;
                    TotalSlabValue = PerSlabTax.Sum();
                    TotalSlabValue = TotalSlabValue - (TotalSlabValue * (TaxDisc / 100));
                    currentYearExpectedTax = TotalSlabValue;
                }
                else
                {
                    decimal TotalSlabValue = 0;
                    TotalSlabValue = PerSlabTax.Sum();
                    currentYearExpectedTax = TotalSlabValue;
                }

                //salaryTaxValue = (currentYearExpectedTax - (alreadyProcessedTax)) / remainingPeriodsCnt;
                salaryTaxValue = (currentYearExpectedTax) / 12;
                if (TaxDiscountMonthly != 0)
                {
                    outValue = salaryTaxValue + TaxDiscountMonthly;
                }
                else
                {
                    outValue = salaryTaxValue;
                }

                if (FullTaxPaymentInThisMonth > 0)
                {
                    incentiveValue = Convert.ToDecimal((from a in oDB.TrnsQuarterTaxAdjDetail where a.TrnsQuarterTaxAdj.EmpID == emp.ID && a.PayrollPeriodID == period.ID select a.TaxableAmount).Sum());
                    outValue += incentiveValue;
                    if (incentiveValue > 0)
                    {
                        QuarterlyValue = incentiveValue;
                    }
                    else
                    {
                        QuarterlyValue = 0.0M;
                    }
                }
            }

            oDB.AddTaxLog(emp.EmpID,
                Convert.ToString(emp.FirstName + emp.MiddleName + emp.LastName),
                Convert.ToString(emp.BasicSalary),
                Convert.ToString(empGrossSalary),
                Convert.ToString(PeriodTaxbleIncome),
                Convert.ToString(PeriodTaxbleLWOP),
                Convert.ToString(PeriodTaxbleIncomeOT),
                Convert.ToString(currentYearExpectedIncome),
                Convert.ToString(currentYearExpectedTax),
                "0",
                "0",
                Convert.ToString(obIncome),
                Convert.ToString(obTax),
                Convert.ToString(projectedIncome), Convert.ToString(alreadyProcessedIncome),
                Convert.ToString(period.PeriodName),
                Convert.ToString(FullTaxPaymentInThisMonth), Convert.ToString(incentiveValue),
                Convert.ToString(TaxDiscountYearly), Convert.ToString(TaxDiscountMonthly),
                Convert.ToString(TaxCode), Convert.ToString(TaxMin), Convert.ToString(TaxMax),
                Convert.ToString(TaxFix), Convert.ToString(TaxPer),
                Convert.ToString(TaxCodeI), Convert.ToString(TaxMinI), Convert.ToString(TaxMaxI),
                Convert.ToString(TaxFixI), Convert.ToString(TaxPerI),
                Convert.ToString(salaryTaxValue), Convert.ToString(outValue), "Salary", "System"
                );


            return outValue;
        }

        public DataTable EOSContribution(String pEmpInternalID)
        {
            DataTable dtOut = new DataTable();
            DataTable dtcon = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");

            try
            {
                String GetContributionQuery = @"
                                SELECT     
	                                dbo.MstElements.ElementName, 
                                    SUM(ABS(ISNULL(dbo.TrnsSalaryProcessRegisterDetail.LineValue,0))) AS LineValue,
                                    dbo.TrnsSalaryProcessRegisterDetail.LineBaseEntry,
                                    dbo.TrnsSalaryProcessRegisterDetail.BaseValueCalculatedOn,
                                    dbo.TrnsSalaryProcessRegisterDetail.BaseValue,
                                    dbo.TrnsSalaryProcessRegisterDetail.BaseValueType,
                                    dbo.TrnsSalaryProcessRegisterDetail.LineMemo,
                                    dbo.TrnsSalaryProcessRegisterDetail.CreditAccount,
                                    dbo.TrnsSalaryProcessRegisterDetail.CreditAccountName,
                                    dbo.TrnsSalaryProcessRegisterDetail.DebitAccount,
                                    dbo.TrnsSalaryProcessRegisterDetail.DebitAccountName
                                FROM
	                                dbo.TrnsSalaryProcessRegister INNER JOIN
	                                dbo.TrnsSalaryProcessRegisterDetail ON dbo.TrnsSalaryProcessRegister.Id = dbo.TrnsSalaryProcessRegisterDetail.SRID INNER JOIN
	                                dbo.TrnsEmployeeElementDetail ON dbo.TrnsSalaryProcessRegisterDetail.LineBaseEntry = dbo.TrnsEmployeeElementDetail.Id INNER JOIN
	                                dbo.MstElements ON dbo.TrnsEmployeeElementDetail.ElementId = dbo.MstElements.Id INNER JOIN
	                                dbo.MstElementContribution ON dbo.MstElements.Id = dbo.MstElementContribution.ElementId
                                WHERE    
	                                (dbo.TrnsSalaryProcessRegister.EmpID = '" + pEmpInternalID + @"') 
	                                AND (dbo.MstElementContribution.flgEOS = 1) 
	                                AND (dbo.TrnsEmployeeElementDetail.ElementType = 'Con')
                                GROUP BY	
	                                dbo.MstElements.ElementName,
                                    dbo.TrnsSalaryProcessRegisterDetail.LineBaseEntry,
                                    dbo.TrnsSalaryProcessRegisterDetail.BaseValueCalculatedOn,
                                    dbo.TrnsSalaryProcessRegisterDetail.BaseValue,
                                    dbo.TrnsSalaryProcessRegisterDetail.BaseValueType,
                                    dbo.TrnsSalaryProcessRegisterDetail.LineMemo,
                                    dbo.TrnsSalaryProcessRegisterDetail.CreditAccount,
                                    dbo.TrnsSalaryProcessRegisterDetail.CreditAccountName,
                                    dbo.TrnsSalaryProcessRegisterDetail.DebitAccount,
                                    dbo.TrnsSalaryProcessRegisterDetail.DebitAccountName
                                ";
                dtcon = getDataTable(GetContributionQuery);

                foreach (DataRow OneRow in dtcon.Rows)
                {
                    DataRow dr = dtOut.NewRow();
                    dr["LineType"] = "FSContribution";
                    dr["LineSubType"] = OneRow["ElementName"].ToString();
                    dr["LineValue"] = OneRow["LineValue"].ToString();
                    dr["LineMemo"] = OneRow["LineMemo"].ToString();
                    dr["DebitAccount"] = OneRow["DebitAccount"].ToString();
                    dr["CreditAccount"] = OneRow["CreditAccount"].ToString();
                    dr["DebitAccountName"] = OneRow["DebitAccountName"].ToString();
                    dr["CreditAccountName"] = OneRow["CreditAccountName"].ToString();


                    dr["LineBaseEntry"] = OneRow["LineBaseEntry"].ToString();
                    dr["BaseValueCalculatedOn"] = OneRow["BaseValueCalculatedOn"].ToString(); ;
                    dr["BaseValue"] = OneRow["BaseValue"].ToString();
                    dr["BaseValueType"] = OneRow["BaseValueType"].ToString();
                    dr["TaxbleAmnt"] = "0.0";
                    dtOut.Rows.Add(dr);
                }

                dtcon.Rows.Clear();

                GetContributionQuery = @"
                                        SELECT     
                                            dbo.MstElements.ElementName, 
                                            SUM(ABS(ISNULL(dbo.TrnsFinalSettelmentRegisterDetail.LineValue,0))) AS LineValue,
                                            dbo.TrnsFinalSettelmentRegisterDetail.LineBaseEntry,
                                            dbo.TrnsFinalSettelmentRegisterDetail.BaseValueCalculatedOn,
                                            dbo.TrnsFinalSettelmentRegisterDetail.BaseValue,
                                            dbo.TrnsFinalSettelmentRegisterDetail.BaseValueType,
                                            dbo.TrnsFinalSettelmentRegisterDetail.LineMemo,
                                            dbo.TrnsFinalSettelmentRegisterDetail.CreditAccount,
                                            dbo.TrnsFinalSettelmentRegisterDetail.CreditAccountName,
                                            dbo.TrnsFinalSettelmentRegisterDetail.DebitAccount,
                                            dbo.TrnsFinalSettelmentRegisterDetail.DebitAccountName
                                        FROM
                                            dbo.TrnsFinalSettelmentRegister INNER JOIN
                                            dbo.TrnsFinalSettelmentRegisterDetail ON dbo.TrnsFinalSettelmentRegister.Id = dbo.TrnsFinalSettelmentRegisterDetail.FSID INNER JOIN
                                            dbo.TrnsEmployeeElementDetail ON dbo.TrnsFinalSettelmentRegisterDetail.LineBaseEntry = dbo.TrnsEmployeeElementDetail.Id INNER JOIN
                                            dbo.MstElements ON dbo.TrnsEmployeeElementDetail.ElementId = dbo.MstElements.Id INNER JOIN
                                            dbo.MstElementContribution ON dbo.MstElements.Id = dbo.MstElementContribution.ElementId
                                        WHERE    
                                            (dbo.TrnsFinalSettelmentRegister.EmpID = '" + pEmpInternalID + @"') 
                                            AND (dbo.MstElementContribution.flgEOS = 1) 
                                            AND (dbo.TrnsEmployeeElementDetail.ElementType = 'Con')
                                        GROUP BY	
                                            dbo.MstElements.ElementName,
                                            dbo.TrnsFinalSettelmentRegisterDetail.LineBaseEntry,
                                            dbo.TrnsFinalSettelmentRegisterDetail.BaseValueCalculatedOn,
                                            dbo.TrnsFinalSettelmentRegisterDetail.BaseValue,
                                            dbo.TrnsFinalSettelmentRegisterDetail.BaseValueType,
                                            dbo.TrnsFinalSettelmentRegisterDetail.LineMemo,
                                            dbo.TrnsFinalSettelmentRegisterDetail.CreditAccount,
                                            dbo.TrnsFinalSettelmentRegisterDetail.CreditAccountName,
                                            dbo.TrnsFinalSettelmentRegisterDetail.DebitAccount,
                                            dbo.TrnsFinalSettelmentRegisterDetail.DebitAccountName
                                        ";
                dtcon = getDataTable(GetContributionQuery);

                foreach (DataRow OneRow in dtcon.Rows)
                {
                    DataRow dr = dtOut.NewRow();
                    dr["LineType"] = "FSContribution";
                    dr["LineSubType"] = OneRow["ElementName"].ToString();
                    dr["LineValue"] = OneRow["LineValue"].ToString();
                    dr["LineMemo"] = OneRow["LineMemo"].ToString();
                    dr["DebitAccount"] = OneRow["DebitAccount"].ToString();
                    dr["CreditAccount"] = OneRow["CreditAccount"].ToString();
                    dr["DebitAccountName"] = OneRow["DebitAccountName"].ToString();
                    dr["CreditAccountName"] = OneRow["CreditAccountName"].ToString();


                    dr["LineBaseEntry"] = OneRow["LineBaseEntry"].ToString();
                    dr["BaseValueCalculatedOn"] = OneRow["BaseValueCalculatedOn"].ToString(); ;
                    dr["BaseValue"] = OneRow["BaseValue"].ToString();
                    dr["BaseValueType"] = OneRow["BaseValueType"].ToString();
                    dr["TaxbleAmnt"] = "0.0";
                    dtOut.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return dtOut;
        }

        public DataTable EOSOBPFEmpValues(MstEmployee oEmp, MstGLDetermination glDetr)
        {
            DataTable dtOut = new DataTable();
            DataTable dtarrear = new DataTable();
            String LineValue = "";
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");

            try
            {

                if (oEmp == null)
                {
                    return dtOut;
                }

                String GetArrearQuery = @"
                                SELECT 
	                                ISNULL(EmployeeBalance,0)  AS EmpValue 
                                FROM 
	                                dbo.TrnsOBProvidentFund 
                                WHERE 
	                                EmpID = '" + oEmp.ID + @"'
                                ";

                //Get GL For Opening 

                dtarrear = getDataTable(GetArrearQuery);

                foreach (DataRow OneRow in dtarrear.Rows)
                {
                    DataRow dr = dtOut.NewRow();
                    dr["LineType"] = "PFEmployeeOB";
                    dr["LineSubType"] = "PFEmployeeOB";
                    if (String.IsNullOrEmpty(OneRow["EmpValue"].ToString()))
                    {
                        LineValue = "0.0";
                    }
                    else
                    {
                        LineValue = OneRow["EmpValue"].ToString();
                    }
                    dr["LineValue"] = LineValue;
                    dr["LineMemo"] = "PFOBValue";
                    dr["DebitAccount"] = glDetr.MstGLDContribution[0].CostAccount;
                    dr["CreditAccount"] = glDetr.MstGLDContribution[0].BalancingAccount;
                    dr["DebitAccountName"] = glDetr.MstGLDContribution[0].CostAcctDisplay;
                    dr["CreditAccountName"] = glDetr.MstGLDContribution[0].BalancingAcctDisplay;


                    dr["LineBaseEntry"] = "0";
                    dr["BaseValueCalculatedOn"] = "0.0";
                    dr["BaseValue"] = "0.0";
                    dr["BaseValueType"] = "";
                    dr["TaxbleAmnt"] = "0.0";
                    dtOut.Rows.Add(dr);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return dtOut;
        }

        public DataTable EOSOBPFEmplrValues(MstEmployee oEmp, MstGLDetermination glDetr)
        {
            DataTable dtOut = new DataTable();
            DataTable dtarrear = new DataTable();
            String LineValue = "";
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");

            try
            {

                if (oEmp == null)
                {
                    return dtOut;
                }

                String GetArrearQuery = @"
                                SELECT 
	                                ISNULL(EmployerBalance,0)  AS EmpValue 
                                FROM 
	                                dbo.TrnsOBProvidentFund 
                                WHERE 
	                                EmpID = '" + oEmp.ID + @"'
                                ";

                //Get GL For Opening 


                dtarrear = getDataTable(GetArrearQuery);

                foreach (DataRow OneRow in dtarrear.Rows)
                {
                    DataRow dr = dtOut.NewRow();
                    dr["LineType"] = "PFEmployerOB";
                    dr["LineSubType"] = "PFEmployerOB";
                    if (String.IsNullOrEmpty(OneRow["EmpValue"].ToString()))
                    {
                        LineValue = "0.0";
                    }
                    else
                    {
                        LineValue = OneRow["EmpValue"].ToString();
                    }
                    dr["LineValue"] = LineValue;
                    dr["LineMemo"] = "PFOBEmployerValue";
                    dr["DebitAccount"] = glDetr.MstGLDContribution[0].EmprCostAccount;
                    dr["CreditAccount"] = glDetr.MstGLDContribution[0].EmprBalancingAccount;
                    dr["DebitAccountName"] = glDetr.MstGLDContribution[0].EmprCostAccount;
                    dr["CreditAccountName"] = glDetr.MstGLDContribution[0].EmprCostAcctDisplay;


                    dr["LineBaseEntry"] = "0";
                    dr["BaseValueCalculatedOn"] = "0.0";
                    dr["BaseValue"] = "0.0";
                    dr["BaseValueType"] = "";
                    dr["TaxbleAmnt"] = "0.0";
                    dtOut.Rows.Add(dr);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return dtOut;
        }

        public DataTable EOSArrear(String pEmpInternalID)
        {
            DataTable dtOut = new DataTable();
            DataTable dtarrear = new DataTable();
            String LineValue = "";
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");

            try
            {
                String GetArrearQuery = @"
                                SELECT
                                    SUM(ISNULL(dbo.TrnsSalaryProcessRegisterDetail.LineValue,0)) AS LineValue
                                FROM         
	                                dbo.TrnsSalaryProcessRegister INNER JOIN
	                                dbo.TrnsSalaryProcessRegisterDetail ON dbo.TrnsSalaryProcessRegister.Id = dbo.TrnsSalaryProcessRegisterDetail.SRID INNER JOIN
	                                dbo.TrnsEmployeeElementDetail ON dbo.TrnsSalaryProcessRegisterDetail.LineBaseEntry = dbo.TrnsEmployeeElementDetail.Id
                                WHERE
                                    (dbo.TrnsSalaryProcessRegister.flgHoldPayment = 1) 
                                    AND (dbo.TrnsSalaryProcessRegister.EmpID = '" + pEmpInternalID + @"') 
                                ";
                dtarrear = getDataTable(GetArrearQuery);

                foreach (DataRow OneRow in dtarrear.Rows)
                {
                    DataRow dr = dtOut.NewRow();
                    dr["LineType"] = "Arrear";
                    dr["LineSubType"] = "Arrear";
                    if (String.IsNullOrEmpty(OneRow["LineValue"].ToString()))
                    {
                        LineValue = "0.0";
                    }
                    else
                    {
                        LineValue = OneRow["LineValue"].ToString();
                    }
                    dr["LineValue"] = LineValue;
                    dr["LineMemo"] = "";
                    dr["DebitAccount"] = "";
                    dr["CreditAccount"] = "";
                    dr["DebitAccountName"] = "";
                    dr["CreditAccountName"] = "";


                    dr["LineBaseEntry"] = "0";
                    dr["BaseValueCalculatedOn"] = "0.0";
                    dr["BaseValue"] = "0.0";
                    dr["BaseValueType"] = "";
                    dr["TaxbleAmnt"] = "0.0";
                    dtOut.Rows.Add(dr);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return dtOut;
        }

        public DataTable GratuityEOS(MstEmployee pEmp, String Debit, String Credit, String DebitName, String CreditName)
        {
            DateTime JoiningDate, TerminationDate;
            TimeSpan DifferencTime;
            Int32 DifferenceInDays = 0, GratuityType, PayrollDays;
            Int32 DifferenceInYears = 0, EligibleTimeInYear;
            Decimal PayPerDay = 0.0M, Factor = 0.0M, GratuityAmount = 0.0M;
            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");
            try
            {
                //check whether gruatity is enable or not
                MstGratuity oGratuity = (from a in oDB.MstGratuity where a.FlgActive == true select a).FirstOrDefault();

                if (oGratuity != null)
                {
                    //now check if employee is eligle or not
                    JoiningDate = Convert.ToDateTime(pEmp.JoiningDate);
                    TerminationDate = Convert.ToDateTime(pEmp.TerminationDate);
                    DifferencTime = TerminationDate - JoiningDate;
                    DifferenceInDays = DifferencTime.Days;
                    EligibleTimeInYear = Convert.ToInt32(oGratuity.YearFrom) * 365;
                    PayrollDays = Convert.ToInt32(pEmp.CfgPayrollDefination.WorkDays);
                    Factor = Convert.ToDecimal(oGratuity.Factor / 100);
                    if (PayrollDays == 0)
                    {
                        PayrollDays = 30;
                    }
                    if (DifferenceInDays >= EligibleTimeInYear)
                    {
                        GratuityType = Convert.ToInt32(oGratuity.GratuityType);
                        if (GratuityType == 0) //Full Calculation
                        {
                            DifferenceInYears = Convert.ToInt32(DifferenceInDays / 365);
                            if (oGratuity.SalaryType.Contains("0")) //ON Basic
                            {
                                PayPerDay = Convert.ToDecimal(((pEmp.BasicSalary * Factor) / 26) * 30);
                            }
                            else //On Gross
                            {
                                PayPerDay = Convert.ToDecimal(((getEmpGross(pEmp) * Factor) / 26) * 30);
                            }
                            GratuityAmount = PayPerDay * DifferenceInYears;
                        }
                        else if (GratuityType == 1) //Propotionate Calculation
                        {
                            if (oGratuity.SalaryType.Contains("0")) //ON Basic
                            {
                                PayPerDay = Convert.ToDecimal(((pEmp.BasicSalary * Factor) / 26) * 30);
                            }
                            else //On Gross
                            {
                                PayPerDay = Convert.ToDecimal(((getEmpGross(pEmp) * Factor) / 26) * 30);
                            }
                            GratuityAmount = PayPerDay * (DifferenceInDays / 365);
                        }
                    }
                }
                DataRow dr = dtOut.NewRow();
                dr["LineType"] = "FSGratuity";
                dr["LineSubType"] = "Gratuity";
                dr["LineValue"] = GratuityAmount.ToString();
                dr["LineMemo"] = "Gratuity";
                dr["DebitAccount"] = Debit;
                dr["CreditAccount"] = Credit;
                dr["DebitAccountName"] = DebitName;
                dr["CreditAccountName"] = CreditName;


                dr["LineBaseEntry"] = "0";
                dr["BaseValueCalculatedOn"] = "0.0";
                dr["BaseValue"] = "0.0";
                dr["BaseValueType"] = "";
                dr["TaxbleAmnt"] = "0.0";
                dtOut.Rows.Add(dr);
            }
            catch (Exception ex)
            {
            }
            return dtOut;
        }

        public DataTable GratuityEOSUAESlabs(MstEmployee pEmp, String Debit, String Credit, String DebitName, String CreditName)
        {
            DateTime StartDate, EndDate;
            TimeSpan DifferencTime;
            double DifferenceInDays = 0;
            decimal SlabRating = 0, PerValue = 0;
            decimal SlabDays = 0, CalculationDays = 0;
            decimal GratuityAmount = 0;
            decimal LeavesCount = 0;
            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");
            try
            {
                //check whether gruatity is enable or not
                if (pEmp.GratuitySlabs != null)
                {
                    //now check if employee is eligle or not
                    StartDate = Convert.ToDateTime(pEmp.JoiningDate);
                    var oRes = (from a in oDB.TrnsResignation
                                where a.MstEmployee.EmpID == pEmp.EmpID
                                && a.EmpTermCount == (pEmp.TermCount == null ? 1 : Convert.ToInt32(pEmp.TermCount))
                                select a).FirstOrDefault();
                    if (oRes == null)
                    {
                        return dtOut;
                    }
                    if (oRes.FlgOption1 == true)
                    {
                        EndDate = GetLastPeriodEndDate(pEmp, Convert.ToDateTime(oRes.ResignDate));
                        DifferencTime = EndDate - StartDate;
                        DifferenceInDays = DifferencTime.TotalDays;
                    }
                    else
                    {
                        EndDate = Convert.ToDateTime(pEmp.TerminationDate);
                        DifferencTime = EndDate - StartDate;
                        DifferenceInDays = DifferencTime.TotalDays;
                    }

                    if ((pEmp.TrnsGratuitySlabs.FlgWOPLeaves != null ? pEmp.TrnsGratuitySlabs.FlgWOPLeaves : false) == true)
                    {
                        LeavesCount = Convert.ToDecimal((from a in oDB.TrnsLeavesRequest where a.MstEmployee.EmpID == pEmp.EmpID && a.MstLeaveType.LeaveType.ToLower() == "ded" select a.TotalCount).Sum() ?? 0M);
                    }
                    if ((pEmp.TrnsGratuitySlabs.FlgAbsoluteYears != null ? pEmp.TrnsGratuitySlabs.FlgAbsoluteYears : false) == true)
                    {
                        SlabRating = Math.Round(Convert.ToDecimal((((decimal)DifferenceInDays - LeavesCount) / 365)));
                    }
                    else
                    {
                        SlabRating = Convert.ToDecimal((((decimal)DifferenceInDays - LeavesCount) / 365));
                    }

                    //var oSlab = (from a in oDB.TrnsGratuitySlabsDetail 
                    //             where a.TrnsGratuitySlabs.MstEmployee[0].EmpID == pEmp.EmpID &&
                    //             a.FromPoints <= SlabRating && (a.ToPoints != 0 ? a.ToPoints : 100M) >= SlabRating
                    //             select a).FirstOrDefault();
                    var oSlab = (from Detail in oDB.TrnsGratuitySlabsDetail
                                 join Head in oDB.TrnsGratuitySlabs on Detail.FKID equals Head.InternalID
                                 where Head.InternalID == pEmp.GratuitySlabs &&
                                 Detail.FromPoints <= SlabRating && (Detail.ToPoints != 0 ? Detail.ToPoints : 100M) >= SlabRating
                                 select new { Description = Detail.Description, Days = Detail.DaysCount, HeaderDays = Head.CalculatedDays }).FirstOrDefault();

                    if (oSlab != null)
                    {
                        SlabDays = Convert.ToDecimal(oSlab.Days);
                        CalculationDays = Convert.ToDecimal(oSlab.HeaderDays);
                        PerValue = Convert.ToDecimal(pEmp.TrnsGratuitySlabs.BasedOnValue);
                        if (pEmp.TrnsGratuitySlabs.BasedOn == "0") //Basic
                        {
                            decimal PerDayBasic = 0;
                            decimal PayrollDays = 0;
                            decimal EmpBasic = 0;
                            EmpBasic = Convert.ToDecimal(pEmp.BasicSalary);
                            if (CalculationDays == 0)
                            {
                                if (pEmp.PayrollID != null)
                                {
                                    PayrollDays = Convert.ToDecimal(pEmp.TrnsGratuitySlabs.CalculatedDays);
                                }
                                if (PayrollDays == 0)
                                {
                                    var Period = (from a in oDB.CfgPeriodDates
                                                  where a.StartDate <= EndDate && a.EndDate >= EndDate
                                                  && a.PayrollId == pEmp.CfgPayrollDefination.ID
                                                  select a).FirstOrDefault();
                                    TimeSpan oSpan = Convert.ToDateTime(Period.EndDate).Subtract(Convert.ToDateTime(Period.StartDate));
                                    PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                }
                            }
                            else
                            {
                                PayrollDays = CalculationDays;
                            }
                            if (pEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(pEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                            {
                                PerDayBasic = ((EmpBasic / 100) * PerValue) * 12 / PayrollDays;
                            }
                            else
                            {
                                PerDayBasic = ((EmpBasic / 100) * PerValue) / PayrollDays;
                            }

                            GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;
                        }
                        else //Gross
                        {
                            decimal PerDayGross = 0;
                            decimal PayrollDays = 0;
                            if (CalculationDays == 0)
                            {
                                if (pEmp.PayrollID != null)
                                {
                                    PayrollDays = Convert.ToDecimal(pEmp.TrnsGratuitySlabs.CalculatedDays);
                                }
                                if (PayrollDays == 0)
                                {
                                    var Period = (from a in oDB.CfgPeriodDates
                                                  where a.StartDate <= EndDate && a.EndDate >= EndDate
                                                  && a.PayrollId == pEmp.CfgPayrollDefination.ID
                                                  select a).FirstOrDefault();
                                    TimeSpan oSpan = Convert.ToDateTime(Period.EndDate).Subtract(Convert.ToDateTime(Period.StartDate));
                                    PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                }
                            }
                            else
                            {
                                PayrollDays = CalculationDays;
                            }
                            decimal empGrossValue = getEmpGross(pEmp);
                            if (pEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(pEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                            {
                                PerDayGross = ((empGrossValue / 100) * PerValue) * 12 / PayrollDays;
                            }
                            else
                            {
                                PerDayGross = ((empGrossValue / 100) * PerValue) / PayrollDays;
                            }

                            GratuityAmount = (SlabRating * SlabDays) * PerDayGross;
                        }
                    }
                }
                DataRow dr = dtOut.NewRow();
                dr["LineType"] = "FSGratuity";
                dr["LineSubType"] = "Gratuity";
                dr["LineValue"] = GratuityAmount.ToString();
                dr["LineMemo"] = "Gratuity";
                dr["DebitAccount"] = Debit;
                dr["CreditAccount"] = Credit;
                dr["DebitAccountName"] = DebitName;
                dr["CreditAccountName"] = CreditName;


                dr["LineBaseEntry"] = "0";
                dr["BaseValueCalculatedOn"] = "0.0";
                dr["BaseValue"] = "0.0";
                dr["BaseValueType"] = "";
                dr["TaxbleAmnt"] = "0.0";
                dtOut.Rows.Add(dr);
            }
            catch (Exception ex)
            {
                string value = string.Empty;
                value = ex.Message;
                value = string.Empty;
            }
            return dtOut;
        }

        public DataTable GratuityEOSUAESlabsWise(MstEmployee oEmp, String Debit, String Credit, String DebitName, String CreditName)
        {
            double DifferenceInDays = 0;
            decimal SlabRating = 0, PerValue = 0;
            decimal SlabDays = 0, CalculationDays = 0;
            decimal GratuityAmount = 0;
            decimal LeavesCount = 0;
            DateTime GratuityPeriodStartDate, GratuityPeriodEndDate;
            TimeSpan DifferencTime;
            decimal EmpGross = 0, EmpBasic = 0;
            decimal Slab1Calc = 0, Slab2Calc = 0, Slab3Calc = 0, Slab4Calc = 0, Slab5Calc = 0;
            DateTime FirstPeriodEndDate = DateTime.MinValue, SecondPeriodEndDate = DateTime.MinValue, ThirdPeriodEndDate = DateTime.MinValue, FourthPeriodEndDate = DateTime.MinValue, FifthPeriodEndDate = DateTime.MinValue;
            decimal PerDayGross = 0, PerDayBasic = 0, PayrollDays = 0, PerMonthProvision = 0m;
            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");
            try
            {
                //check whether gruatity is enable or not
                if (oEmp.GratuitySlabs != null)
                {
                    //now check if employee is eligle or not
                    GratuityPeriodStartDate = Convert.ToDateTime(oEmp.JoiningDate);
                    var oRes = (from a in oDB.TrnsResignation
                                where a.MstEmployee.EmpID == oEmp.EmpID
                                && a.EmpTermCount == (oEmp.TermCount == null ? 1 : Convert.ToInt32(oEmp.TermCount))
                                select a).FirstOrDefault();
                    if (oRes == null)
                    {
                        return dtOut;
                    }
                    if (oRes.FlgOption1 == true)
                    {
                        GratuityPeriodEndDate = GetLastPeriodEndDate(oEmp, Convert.ToDateTime(oRes.ResignDate));
                        DifferencTime = GratuityPeriodEndDate - GratuityPeriodStartDate;
                        DifferenceInDays = DifferencTime.TotalDays;
                    }
                    else
                    {
                        GratuityPeriodEndDate = Convert.ToDateTime(oEmp.TerminationDate);
                        DifferencTime = GratuityPeriodEndDate - GratuityPeriodStartDate;
                        DifferenceInDays = DifferencTime.TotalDays;
                    }
                    var oPeriod = (from a in oDB.CfgPeriodDates
                                   where a.PayrollId == oEmp.PayrollID
                                   && a.StartDate <= GratuityPeriodEndDate
                                   && a.EndDate >= GratuityPeriodEndDate
                                   select a).FirstOrDefault();
                    if (oPeriod == null) return dtOut;
                    #region Gratuity Calculations

                    if ((oEmp.TrnsGratuitySlabs.FlgWOPLeaves != null ? oEmp.TrnsGratuitySlabs.FlgWOPLeaves : false) == true)
                    {
                        LeavesCount = Convert.ToDecimal((from a in oDB.TrnsLeavesRequest where a.MstEmployee.EmpID == oEmp.EmpID && a.MstLeaveType.LeaveType.ToLower() == "ded" select a.TotalCount).Sum() ?? 0M);
                    }
                    if ((oEmp.TrnsGratuitySlabs.FlgAbsoluteYears != null ? oEmp.TrnsGratuitySlabs.FlgAbsoluteYears : false) == true)
                    {
                        SlabRating = Math.Round(Convert.ToDecimal((((decimal)DifferenceInDays - LeavesCount) / 365)));
                    }
                    else
                    {
                        SlabRating = Convert.ToDecimal((((decimal)DifferenceInDays - LeavesCount) / 365));
                    }
                    var oSlab = (from a in oDB.TrnsGratuitySlabsDetail
                                 where a.TrnsGratuitySlabs.InternalID == oEmp.GratuitySlabs
                                 && a.FromPoints < SlabRating
                                 select a).ToList();
                    int i = 1;
                    foreach (var One in oSlab)
                    {

                        if (i == 1) //slab 1
                        {
                            #region Slab 1
                            double DaysDifference1stSlab = 0, ToValueSlab = Convert.ToDouble(One.ToPoints) * 365;
                            DateTime Gratuity1stPeriodStart, Gratuity1stPeriodEnd;
                            TimeSpan Gratuity1stSlabSpan;
                            DateTime FirstPeriodToFind = Convert.ToDateTime(oEmp.JoiningDate).AddDays(ToValueSlab);
                            CfgPeriodDates oFirstSlabEndDatePeriod = (from a in oDB.CfgPeriodDates
                                                                      where a.PayrollId == oEmp.PayrollID
                                                                      && a.StartDate <= FirstPeriodToFind && a.EndDate >= FirstPeriodToFind
                                                                      select a).FirstOrDefault();
                            if (oFirstSlabEndDatePeriod != null)
                            {
                                if ((DateTime)oPeriod.StartDate <= (DateTime)oFirstSlabEndDatePeriod.StartDate)
                                {
                                    Gratuity1stPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    Gratuity1stPeriodEnd = Convert.ToDateTime(oFirstSlabEndDatePeriod.EndDate);
                                }
                            }
                            else
                            {
                                if ((DateTime)oPeriod.StartDate < FirstPeriodToFind)
                                {
                                    Gratuity1stPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    Gratuity1stPeriodEnd = Convert.ToDateTime(oEmp.JoiningDate).AddDays((double)(One.ToPoints * 365));
                                }
                            }
                            Gratuity1stPeriodStart = Convert.ToDateTime(oEmp.JoiningDate);
                            Gratuity1stSlabSpan = Gratuity1stPeriodEnd - Gratuity1stPeriodStart;
                            DaysDifference1stSlab = Gratuity1stSlabSpan.TotalDays;
                            if ((oEmp.TrnsGratuitySlabs.FlgAbsoluteYears != null ? oEmp.TrnsGratuitySlabs.FlgAbsoluteYears : false) == true)
                            {
                                SlabRating = Math.Round(Convert.ToDecimal((((decimal)DaysDifference1stSlab - LeavesCount) / 365)));
                            }
                            else
                            {
                                SlabRating = Convert.ToDecimal((((decimal)DaysDifference1stSlab - LeavesCount) / 365));
                            }

                            #region Value Calculation
                            SlabDays = Convert.ToDecimal(One.DaysCount);
                            CalculationDays = Convert.ToDecimal(One.TrnsGratuitySlabs.CalculatedDays);
                            PerValue = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.BasedOnValue);
                            if (oEmp.TrnsGratuitySlabs.BasedOn == "0") //Basic
                            {
                                EmpBasic = Convert.ToDecimal(oEmp.BasicSalary);
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                                else
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                            }
                            else //Gross
                            {
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                decimal empGrossValue = getEmpGross(oEmp);
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                                else
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                            }
                            #endregion

                            Slab1Calc = GratuityAmount;
                            FirstPeriodEndDate = Gratuity1stPeriodEnd;
                            #endregion
                        }
                        else if (i == 2) //slab 2
                        {
                            #region Slab 2
                            double DaysDifference2ndSlab = 0, ToValueSlab = Convert.ToDouble(One.ToPoints) * 365;
                            DateTime Gratuity2ndPeriodStart, Gratuity2ndPeriodEnd;
                            TimeSpan Gratuity2ndSlabSpan;
                            DateTime FirstPeriodToFind = Convert.ToDateTime(oEmp.JoiningDate).AddDays(ToValueSlab);
                            CfgPeriodDates oSecondSlabEndDatePeriod = (from a in oDB.CfgPeriodDates
                                                                       where a.PayrollId == oEmp.PayrollID
                                                                       && a.StartDate <= FirstPeriodToFind && a.EndDate >= FirstPeriodToFind
                                                                       select a).FirstOrDefault();
                            if (oSecondSlabEndDatePeriod != null)
                            {
                                if ((DateTime)oPeriod.StartDate <= (DateTime)oSecondSlabEndDatePeriod.StartDate)
                                {
                                    Gratuity2ndPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    Gratuity2ndPeriodEnd = Convert.ToDateTime(oSecondSlabEndDatePeriod.EndDate);
                                }
                            }
                            else
                            {
                                if ((DateTime)oPeriod.StartDate < FirstPeriodToFind)
                                {
                                    Gratuity2ndPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    Gratuity2ndPeriodEnd = Convert.ToDateTime(oEmp.JoiningDate).AddDays((double)(One.ToPoints * 365));
                                }
                            }
                            Gratuity2ndPeriodStart = Convert.ToDateTime(FirstPeriodEndDate.AddDays(1));
                            Gratuity2ndSlabSpan = Gratuity2ndPeriodEnd - Gratuity2ndPeriodStart;
                            DaysDifference2ndSlab = Gratuity2ndSlabSpan.TotalDays;
                            if ((oEmp.TrnsGratuitySlabs.FlgAbsoluteYears != null ? oEmp.TrnsGratuitySlabs.FlgAbsoluteYears : false) == true)
                            {
                                SlabRating = Math.Round(Convert.ToDecimal((((decimal)DaysDifference2ndSlab - LeavesCount) / 365)));
                            }
                            else
                            {
                                SlabRating = Convert.ToDecimal((((decimal)DaysDifference2ndSlab - LeavesCount) / 365));
                            }

                            #region Value Calculation
                            SlabDays = Convert.ToDecimal(One.DaysCount);
                            CalculationDays = Convert.ToDecimal(One.TrnsGratuitySlabs.CalculatedDays);
                            PerValue = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.BasedOnValue);
                            if (oEmp.TrnsGratuitySlabs.BasedOn == "0") //Basic
                            {
                                EmpBasic = Convert.ToDecimal(oEmp.BasicSalary);
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                                else
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                            }
                            else //Gross
                            {
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                decimal empGrossValue = getEmpGross(oEmp);
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                                else
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                            }
                            #endregion

                            Slab2Calc = GratuityAmount;
                            SecondPeriodEndDate = Gratuity2ndPeriodEnd;
                            #endregion
                        }
                        else if (i == 3) //slab 3
                        {
                            #region Slab 3
                            double DaysDifference3rdSlab = 0, ToValueSlab = Convert.ToDouble(One.ToPoints) * 365;
                            DateTime Gratuity3rdPeriodStart, Gratuity3rdPeriodEnd;
                            TimeSpan Gratuity3rdSlabSpan;
                            DateTime FirstPeriodToFind = Convert.ToDateTime(oEmp.JoiningDate).AddDays(ToValueSlab);
                            CfgPeriodDates oThirdSlabEndDatePeriod = (from a in oDB.CfgPeriodDates
                                                                      where a.PayrollId == oEmp.PayrollID
                                                                      && a.StartDate <= FirstPeriodToFind && a.EndDate >= FirstPeriodToFind
                                                                      select a).FirstOrDefault();
                            if (oThirdSlabEndDatePeriod != null)
                            {
                                if ((DateTime)oPeriod.StartDate <= (DateTime)oThirdSlabEndDatePeriod.StartDate)
                                {
                                    Gratuity3rdPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    Gratuity3rdPeriodEnd = Convert.ToDateTime(oThirdSlabEndDatePeriod.EndDate);
                                }
                            }
                            else
                            {
                                if ((DateTime)oPeriod.StartDate < FirstPeriodToFind)
                                {
                                    Gratuity3rdPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    Gratuity3rdPeriodEnd = Convert.ToDateTime(oEmp.JoiningDate).AddDays((double)(One.ToPoints * 365));
                                }
                            }
                            Gratuity3rdPeriodStart = Convert.ToDateTime(FirstPeriodEndDate.AddDays(1));
                            Gratuity3rdSlabSpan = Gratuity3rdPeriodEnd - Gratuity3rdPeriodStart;
                            DaysDifference3rdSlab = Gratuity3rdSlabSpan.TotalDays;
                            if ((oEmp.TrnsGratuitySlabs.FlgAbsoluteYears != null ? oEmp.TrnsGratuitySlabs.FlgAbsoluteYears : false) == true)
                            {
                                SlabRating = Math.Round(Convert.ToDecimal((((decimal)DaysDifference3rdSlab - LeavesCount) / 365)));
                            }
                            else
                            {
                                SlabRating = Convert.ToDecimal((((decimal)DaysDifference3rdSlab - LeavesCount) / 365));
                            }

                            #region Value Calculation
                            SlabDays = Convert.ToDecimal(One.DaysCount);
                            CalculationDays = Convert.ToDecimal(One.TrnsGratuitySlabs.CalculatedDays);
                            PerValue = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.BasedOnValue);
                            if (oEmp.TrnsGratuitySlabs.BasedOn == "0") //Basic
                            {
                                EmpBasic = Convert.ToDecimal(oEmp.BasicSalary);
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                                else
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                            }
                            else //Gross
                            {
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                decimal empGrossValue = getEmpGross(oEmp);
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                                else
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                            }
                            #endregion

                            Slab3Calc = GratuityAmount;
                            ThirdPeriodEndDate = Gratuity3rdPeriodEnd;
                            #endregion
                        }
                        else if (i == 4) //slab 4
                        {
                            #region Slab 4
                            double DaysDifference4thSlab = 0, ToValueSlab = Convert.ToDouble(One.ToPoints) * 365;
                            DateTime Gratuity4thPeriodStart, Gratuity4thPeriodEnd;
                            TimeSpan Gratuity4thSlabSpan;
                            DateTime FirstPeriodToFind = Convert.ToDateTime(oEmp.JoiningDate).AddDays(ToValueSlab);
                            CfgPeriodDates oFourthSlabEndDatePeriod = (from a in oDB.CfgPeriodDates
                                                                       where a.PayrollId == oEmp.PayrollID
                                                                       && a.StartDate <= FirstPeriodToFind && a.EndDate >= FirstPeriodToFind
                                                                       select a).FirstOrDefault();
                            if (oFourthSlabEndDatePeriod != null)
                            {
                                if ((DateTime)oPeriod.StartDate <= (DateTime)oFourthSlabEndDatePeriod.StartDate)
                                {
                                    Gratuity4thPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    if ((DateTime)oPeriod.StartDate < FirstPeriodToFind)
                                    {
                                        Gratuity4thPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                    }
                                    else
                                    {
                                        Gratuity4thPeriodEnd = Convert.ToDateTime(oEmp.JoiningDate).AddDays((double)(One.ToPoints * 365));
                                    }
                                }
                            }
                            else
                            {
                                if ((DateTime)oPeriod.StartDate < FirstPeriodToFind)
                                {
                                    Gratuity4thPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    Gratuity4thPeriodEnd = Convert.ToDateTime(oEmp.JoiningDate).AddDays((double)(One.ToPoints * 365));
                                }
                            }
                            Gratuity4thPeriodStart = Convert.ToDateTime(FirstPeriodEndDate.AddDays(1));
                            Gratuity4thSlabSpan = Gratuity4thPeriodEnd - Gratuity4thPeriodStart;
                            DaysDifference4thSlab = Gratuity4thSlabSpan.TotalDays;
                            if ((oEmp.TrnsGratuitySlabs.FlgAbsoluteYears != null ? oEmp.TrnsGratuitySlabs.FlgAbsoluteYears : false) == true)
                            {
                                SlabRating = Math.Round(Convert.ToDecimal((((decimal)DaysDifference4thSlab - LeavesCount) / 365)));
                            }
                            else
                            {
                                SlabRating = Convert.ToDecimal((((decimal)DaysDifference4thSlab - LeavesCount) / 365));
                            }

                            #region Value Calculation
                            SlabDays = Convert.ToDecimal(One.DaysCount);
                            CalculationDays = Convert.ToDecimal(One.TrnsGratuitySlabs.CalculatedDays);
                            PerValue = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.BasedOnValue);
                            if (oEmp.TrnsGratuitySlabs.BasedOn == "0") //Basic
                            {
                                EmpBasic = Convert.ToDecimal(oEmp.BasicSalary);
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                                else
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                            }
                            else //Gross
                            {
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                decimal empGrossValue = getEmpGross(oEmp);
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                                else
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                            }
                            #endregion

                            Slab4Calc = PerMonthProvision;
                            FourthPeriodEndDate = Gratuity4thPeriodEnd;
                            #endregion
                        }
                        else if (i == 5) //slab 5
                        {
                            #region Slab 5
                            double DaysDifference5thSlab = 0, ToValueSlab = Convert.ToDouble(One.ToPoints) * 365;
                            DateTime Gratuity5thPeriodStart, Gratuity5thPeriodEnd;
                            TimeSpan Gratuity5thSlabSpan;
                            DateTime FirstPeriodToFind = Convert.ToDateTime(oEmp.JoiningDate).AddDays(ToValueSlab);
                            CfgPeriodDates oFifthSlabEndDatePeriod = (from a in oDB.CfgPeriodDates
                                                                      where a.PayrollId == oEmp.PayrollID
                                                                      && a.StartDate <= FirstPeriodToFind && a.EndDate >= FirstPeriodToFind
                                                                      select a).FirstOrDefault();
                            if (oFifthSlabEndDatePeriod != null)
                            {
                                if ((DateTime)oPeriod.StartDate <= (DateTime)oFifthSlabEndDatePeriod.StartDate)
                                {
                                    Gratuity5thPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    Gratuity5thPeriodEnd = Convert.ToDateTime(oFifthSlabEndDatePeriod.EndDate);
                                }
                            }
                            else
                            {
                                if ((DateTime)oPeriod.StartDate < FirstPeriodToFind)
                                {
                                    Gratuity5thPeriodEnd = Convert.ToDateTime(oPeriod.EndDate);
                                }
                                else
                                {
                                    Gratuity5thPeriodEnd = Convert.ToDateTime(oEmp.JoiningDate).AddDays((double)(One.ToPoints * 365));
                                }
                            }
                            Gratuity5thPeriodStart = Convert.ToDateTime(FirstPeriodEndDate.AddDays(1));
                            Gratuity5thSlabSpan = Gratuity5thPeriodEnd - Gratuity5thPeriodStart;
                            DaysDifference5thSlab = Gratuity5thSlabSpan.TotalDays;
                            if ((oEmp.TrnsGratuitySlabs.FlgAbsoluteYears != null ? oEmp.TrnsGratuitySlabs.FlgAbsoluteYears : false) == true)
                            {
                                SlabRating = Math.Round(Convert.ToDecimal((((decimal)DaysDifference5thSlab - LeavesCount) / 365)));
                            }
                            else
                            {
                                SlabRating = Convert.ToDecimal((((decimal)DaysDifference5thSlab - LeavesCount) / 365));
                            }

                            #region Value Calculation
                            SlabDays = Convert.ToDecimal(One.DaysCount);
                            CalculationDays = Convert.ToDecimal(One.TrnsGratuitySlabs.CalculatedDays);
                            PerValue = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.BasedOnValue);
                            if (oEmp.TrnsGratuitySlabs.BasedOn == "0") //Basic
                            {
                                EmpBasic = Convert.ToDecimal(oEmp.BasicSalary);
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                                else
                                {
                                    PerDayBasic = ((EmpBasic / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayBasic;

                                }
                            }
                            else //Gross
                            {
                                if (CalculationDays == 0)
                                {
                                    if (oEmp.PayrollID != null)
                                    {
                                        PayrollDays = Convert.ToDecimal(oEmp.TrnsGratuitySlabs.CalculatedDays);
                                    }
                                    if (PayrollDays == 0)
                                    {
                                        TimeSpan oSpan = Convert.ToDateTime(oPeriod.EndDate).Subtract(Convert.ToDateTime(oPeriod.StartDate));
                                        PayrollDays = Convert.ToDecimal(oSpan.TotalDays + 1);
                                    }
                                }
                                else
                                {
                                    PayrollDays = CalculationDays;
                                }
                                decimal empGrossValue = getEmpGross(oEmp);
                                if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) * 12 / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                                else
                                {
                                    PerDayGross = ((empGrossValue / 100) * PerValue) / PayrollDays;
                                    GratuityAmount = (SlabRating * SlabDays) * PerDayGross;

                                }
                            }
                            #endregion

                            Slab5Calc = PerMonthProvision;
                            FifthPeriodEndDate = Gratuity5thPeriodEnd;
                            #endregion
                        }
                        i++;
                    }
                    if (oEmp.TrnsGratuitySlabs.FlgPerYear != null ? Convert.ToBoolean(oEmp.TrnsGratuitySlabs.FlgPerYear) : false)
                    {
                        //PerMonthProvision = ((Slab1Calc + Slab2Calc + Slab3Calc + Slab4Calc + Slab5Calc) / PayrollDays) * MonthDays;
                    }
                    else
                    {
                        //PerMonthProvision = ((Slab1Calc + Slab2Calc + Slab3Calc + Slab4Calc + Slab5Calc) / 365) * MonthDays;
                    }

                    GratuityAmount = Slab1Calc + Slab2Calc + Slab3Calc + Slab4Calc + Slab5Calc;

                    DataRow dr = dtOut.NewRow();
                    dr["LineType"] = "FSGratuity";
                    dr["LineSubType"] = "Gratuity";
                    dr["LineValue"] = GratuityAmount.ToString();
                    dr["LineMemo"] = "Gratuity";
                    dr["DebitAccount"] = Debit;
                    dr["CreditAccount"] = Credit;
                    dr["DebitAccountName"] = DebitName;
                    dr["CreditAccountName"] = CreditName;
                    dr["LineBaseEntry"] = "0";
                    dr["BaseValueCalculatedOn"] = oEmp.BasicSalary.ToString();
                    dr["BaseValue"] = oEmp.BasicSalary.ToString();
                    dr["BaseValueType"] = "FIX";
                    dr["TaxbleAmnt"] = "0.0";
                    //dr["NRTaxbleAmnt"] = 0.00M;
                    dtOut.Rows.Add(dr);
                    #endregion
                }
                //DataRow dr = dtOut.NewRow();
                //dr["LineType"] = "FSGratuity";
                //dr["LineSubType"] = "Gratuity";
                //dr["LineValue"] = GratuityAmount.ToString();
                //dr["LineMemo"] = "Gratuity";
                //dr["DebitAccount"] = Debit;
                //dr["CreditAccount"] = Credit;
                //dr["DebitAccountName"] = DebitName;
                //dr["CreditAccountName"] = CreditName;


                //dr["LineBaseEntry"] = "0";
                //dr["BaseValueCalculatedOn"] = "0.0";
                //dr["BaseValue"] = "0.0";
                //dr["BaseValueType"] = "";
                //dr["TaxbleAmnt"] = "0.0";
                //dtOut.Rows.Add(dr);
            }
            catch (Exception ex)
            {
                string value = string.Empty;
                value = ex.Message;
                value = string.Empty;
            }
            return dtOut;
        }

        private decimal setRowAmnt(MstEmployee emp, decimal Overtimehours, string Periodid, string strOverTimeType)
        {
            string SelectedEmp = "";
            short daysOT = 0;
            decimal HoursOT = 0;
            decimal fixValue = 0.0M;
            decimal daysinYear = 0.0M;
            decimal amount = 0.0M, formulaAmount = 0; ;
            decimal baseValue = 0.00M;
            decimal value = 0.00M;
            Boolean flgFormula = false;
            int otLineID = 0;
            short days = (short)emp.CfgPayrollDefination.WorkDays;
            decimal workhours = (decimal)emp.CfgPayrollDefination.WorkHours;
            decimal monthHours = Convert.ToDecimal(30.00 * 8.00);
            //decimal monthHours = Convert.ToDecimal(days * workhours);
            try
            {
                string code = strOverTimeType; //cb.Value; //Convert.ToString(dtOT.GetValue("Code", rowNum));
                if (!string.IsNullOrEmpty(code))
                {
                    var OTTYpe = oDB.MstOverTime.Where(o => o.ID.ToString() == code).FirstOrDefault();
                    if (OTTYpe != null)
                    {
                        value = Convert.ToDecimal(OTTYpe.Value.Value);
                        daysOT = string.IsNullOrEmpty(OTTYpe.Days) ? Convert.ToInt16(0) : Convert.ToInt16(OTTYpe.Days);
                        HoursOT = string.IsNullOrEmpty(OTTYpe.Hours) ? 0 : Convert.ToDecimal(OTTYpe.Hours);
                        fixValue = OTTYpe.FixValue == null ? 0 : Convert.ToDecimal(OTTYpe.FixValue);
                        daysinYear = OTTYpe.DaysinYear == null ? 0 : Convert.ToDecimal(OTTYpe.DaysinYear);
                        flgFormula = OTTYpe.FlgFormula == null ? false : Convert.ToBoolean(OTTYpe.FlgFormula);
                        if (OTTYpe.ValueType == "POB")
                        {
                            baseValue = (decimal)emp.BasicSalary;
                        }
                        if (OTTYpe.ValueType == "POG")
                        {
                            baseValue = getEmpGross(emp);
                        }
                        if (OTTYpe.ValueType == "FIX")
                        {
                            baseValue = OTTYpe.Value.Value;
                        }
                        otLineID = Convert.ToInt32(OTTYpe.ID);
                        SelectedEmp = emp.EmpID;
                    }

                    if (HoursOT > 0)
                    {
                        workhours = HoursOT;
                    }
                    if (daysOT > 0)
                    {
                        days = daysOT;
                    }
                    if (daysOT <= 0)
                    {
                        string PayrollPeriod = Periodid;// cbPeriod.Value.Trim();
                        if (!string.IsNullOrEmpty(PayrollPeriod))
                        {
                            CfgPeriodDates LeaveFromPeriod = oDB.CfgPeriodDates.Where(p => p.ID == Convert.ToInt32(PayrollPeriod)).FirstOrDefault();
                            if (LeaveFromPeriod != null)
                            {
                                if (days < 1)
                                {
                                    days = Convert.ToInt16(System.DateTime.DaysInMonth(LeaveFromPeriod.StartDate.Value.Date.Year, LeaveFromPeriod.StartDate.Value.Date.Month));
                                }
                                else if (days < 1)
                                {
                                    days = Convert.ToInt16(System.DateTime.DaysInMonth(DateTime.Now.Date.Year, DateTime.Now.Date.Month));
                                }
                            }
                        }
                    }
                    monthHours = Convert.ToDecimal(days * workhours);
                    decimal hours = Overtimehours; //Convert.ToDecimal(dtOT.GetValue("Hours", rowNum));
                    decimal baseAmoun = baseValue;  //Convert.ToDecimal(dtOT.GetValue("BaseVal", rowNum));
                    decimal Val = value; //Convert.ToDecimal(dtOT.GetValue("Value", rowNum));
                    if (fixValue > 0 && daysinYear > 0)
                    {
                        baseAmoun = baseAmoun + fixValue;
                        baseAmoun = baseAmoun * 12;
                        baseAmoun = baseAmoun / daysinYear;
                        baseAmoun = baseAmoun / workhours;
                        decimal baseAmountFormula = 0;
                        baseAmountFormula = baseAmountFormula * 12;
                        baseAmountFormula = baseAmountFormula / daysinYear;
                        baseAmountFormula = baseAmountFormula / workhours;
                        amount = ((baseAmoun * Val / 100) + baseAmountFormula) * hours;
                        //baseAmoun = baseAmoun * 2;  //2 Tiem of Noraml Working Hours
                        //amount = ((baseAmoun) * Val / 100) * hours;
                        //amount = ((baseAmoun / monthHours) * Val / 100) * hours;
                    }
                    else
                    {
                        //amount = ((baseAmoun / monthHours) * Val / 100) * hours;
                        if (OTTYpe.ValueType == "FIX")
                        {
                            amount = baseValue * hours;
                        }
                        else
                        {
                            amount = (((baseAmoun / monthHours) * Val / 100) + (formulaAmount / monthHours)) * hours;
                        }
                    }
                    //dtOT.SetValue("Amount", rowNum, amount.ToString());
                }
                return amount;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public DataTable salaryProcessingOvertimes(MstEmployee emp, CfgPeriodDates otperiod, decimal empGross, out Int32 OTMinutes)
        {
            decimal amnt = 0.0M;
            int DaysCnt = 0;
            decimal hoursfromrecord = 0;
            decimal minfromrecord = 0;
            OTMinutes = 0;
            decimal OTHours = 0;
            Hashtable elementGls = new Hashtable();

            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");
            int cnt = (from p in oDB.TrnsEmployeeOvertime where p.EmployeeId.ToString() == emp.ID.ToString() && p.Period == otperiod.ID select p).Count();
            if (cnt > 0)
            {


                IEnumerable<TrnsEmployeeOvertimeDetail> empPeriodOts = from p in oDB.TrnsEmployeeOvertimeDetail where p.TrnsEmployeeOvertime.EmployeeId.ToString() == emp.ID.ToString() && p.FlgActive == true && p.TrnsEmployeeOvertime.Period == otperiod.ID orderby p.OvertimeID select p;

                var query = oDB.TrnsEmployeeOvertimeDetail
                             .Where(bs => bs.TrnsEmployeeOvertime.EmployeeId == emp.ID && bs.DocAprStatus == "LV0006" && bs.FlgActive == true && bs.TrnsEmployeeOvertime.Period == otperiod.ID)
                             .GroupBy(bs => new { bs.OvertimeID })
                             .AsEnumerable().Select(g => new TrnsEmployeeOvertimeDetail
                             {
                                 Id = g.FirstOrDefault().Id,
                                 EmpOvertimeId = g.FirstOrDefault().EmpOvertimeId,
                                 OvertimeID = g.FirstOrDefault().OvertimeID,
                                 Amount = g.Sum(x => x.Amount),
                                 OTDate = g.FirstOrDefault().OTDate,
                                 FromTime = g.FirstOrDefault().FromTime,
                                 ToTime = g.FirstOrDefault().ToTime,
                                 BasicSalary = g.FirstOrDefault().BasicSalary,
                                 ValueType = g.FirstOrDefault().ValueType,
                                 OTValue = g.FirstOrDefault().OTValue,
                                 FlgActive = g.FirstOrDefault().FlgActive,
                                 OTHours = g.Sum(x => x.OTHours)
                                 //Amount = g.Sum(x => x.Amount) 
                             }).ToList();

                int count = query.Count();
                if (query != null && query.Count > 0)
                {
                    foreach (TrnsEmployeeOvertimeDetail otDet in query)
                    {

                        DataRow dr = dtOut.NewRow();
                        amnt = (decimal)otDet.Amount;
                        string othour = Convert.ToString(otDet.OTHours);
                        string[] tukray = othour.Split('.');
                        hoursfromrecord = 0;
                        minfromrecord = 0;
                        hoursfromrecord = Convert.ToInt32(tukray[0]);
                        string temp = "0." + tukray[1].Substring(0, 2);
                        minfromrecord = Convert.ToInt32(Convert.ToDecimal(temp) * 60);
                        OTMinutes += Convert.ToInt32((hoursfromrecord * 60) + minfromrecord);
                        OTHours = hoursfromrecord + (decimal)(minfromrecord / 60);
                        MstOverTime mstOT = oDB.MstOverTime.Where(OT => OT.ID == otDet.OvertimeID).FirstOrDefault();
                        if (mstOT.MaxHour.GetValueOrDefault() > 0)
                        {
                            //if (mstOT.MaxHour < setRowAmnt(emp, OTHours, otperiod.ID.ToString(), otDet.OvertimeID.ToString()))
                            if (mstOT.MaxHour < OTHours)
                            {
                                amnt = setRowAmnt(emp, (decimal)mstOT.MaxHour, otperiod.ID.ToString(), otDet.OvertimeID.ToString());
                            }
                        }

                        //elementGls = getOverTimeGL(emp, otDet.MstOverTime);
                        elementGls = getOverTimeGL(emp, mstOT);

                        if (amnt > 0)
                        {
                            TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                            dr["LineType"] = "Over Time";
                            dr["LineSubType"] = "Over Time";
                            dr["LineValue"] = amnt;
                            dr["TaxbleAmnt"] = amnt;
                            dr["LineMemo"] = mstOT.Description;
                            dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                            dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                            dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                            dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();

                            dr["LineBaseEntry"] = mstOT.ID;
                            string baseValType = mstOT.ValueType;
                            decimal baseValue = (decimal)mstOT.Value;
                            decimal baseCalculatedOn = 0.0M;

                            if (baseValType == "POB")
                            {
                                baseCalculatedOn = (decimal)emp.BasicSalary;

                            }
                            if (baseValType == "POG")
                            {
                                baseCalculatedOn = empGross;


                            }
                            if (baseValType.ToUpper() == "FIX")
                            {
                                baseCalculatedOn = (decimal)amnt;
                                baseValue = 100.00M;

                            }

                            dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                            dr["BaseValue"] = baseValue;
                            dr["BaseValueType"] = baseValType;
                            dtOut.Rows.Add(dr);

                        }
                    }
                }

                //var OTDetails=oDB.TrnsEmployeeOvertimeDetail.Where(o=>o.TrnsEmployeeOvertime.EmployeeId==emp.ID && o.FlgActive==true && o.TrnsEmployeeOvertime.Period==otperiod.ID).GroupBy(x => new { x.MstOverTime.ID, x. })

                //foreach (TrnsEmployeeOvertimeDetail otDet in empPeriodOts)
                //{

                //    DataRow dr = dtOut.NewRow();
                //    amnt = (decimal)otDet.Amount;
                //    elementGls = getOverTimeGL(emp, otDet.MstOverTime);

                //    if (amnt > 0)
                //    {
                //        TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                //        dr["LineType"] = "Over Time";
                //        dr["LineSubType"] = "Over Time";
                //        dr["LineValue"] = amnt;
                //        dr["TaxbleAmnt"] = amnt;
                //        dr["LineMemo"] = otDet.MstOverTime.Description;
                //        dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                //        dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                //        dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                //        dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();

                //        dr["LineBaseEntry"] = otDet.Id ;
                //        string baseValType = otDet.MstOverTime.ValueType;
                //        decimal baseValue = (decimal)otDet.MstOverTime.Value;
                //        decimal baseCalculatedOn = 0.0M;

                //        if (baseValType == "POB")
                //        {
                //            baseCalculatedOn = (decimal)emp.BasicSalary;

                //        }
                //        if (baseValType == "POG")
                //        {
                //            baseCalculatedOn = empGross;


                //        }
                //        if (baseValType.ToUpper() == "FIX")
                //        {
                //            baseCalculatedOn = (decimal)amnt;
                //            baseValue = 100.00M;

                //        }

                //        dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                //        dr["BaseValue"] = baseValue;
                //        dr["BaseValueType"] = baseValType;
                //        dtOut.Rows.Add(dr);
                //    }
                //}
            }
            return dtOut;
        }

        //public DataTable salaryProcessingOvertimes(MstEmployee emp, CfgPeriodDates otperiod, decimal empGross, out Int32 OTMinutes)
        //{
        //    decimal amnt = 0.0M;
        //    int DaysCnt = 0;
        //    decimal hoursfromrecord = 0;
        //    decimal minfromrecord = 0;
        //    OTMinutes = 0;
        //    Hashtable elementGls = new Hashtable();

        //    DataTable dtOut = new DataTable();
        //    dtOut.Columns.Add("LineType");
        //    dtOut.Columns.Add("LineSubType");
        //    dtOut.Columns.Add("LineValue");
        //    dtOut.Columns.Add("LineMemo");
        //    dtOut.Columns.Add("DebitAccount");
        //    dtOut.Columns.Add("CreditAccount");
        //    dtOut.Columns.Add("DebitAccountName");
        //    dtOut.Columns.Add("CreditAccountName");
        //    dtOut.Columns.Add("LineBaseEntry");
        //    dtOut.Columns.Add("BaseValueCalculatedOn");
        //    dtOut.Columns.Add("BaseValue");
        //    dtOut.Columns.Add("BaseValueType");
        //    dtOut.Columns.Add("TaxbleAmnt");
        //    int cnt = (from p in oDB.TrnsEmployeeOvertime where p.EmployeeId.ToString() == emp.ID.ToString() && p.Period == otperiod.ID select p).Count();
        //    if (cnt > 0)
        //    {


        //        IEnumerable<TrnsEmployeeOvertimeDetail> empPeriodOts = from p in oDB.TrnsEmployeeOvertimeDetail where p.TrnsEmployeeOvertime.EmployeeId.ToString() == emp.ID.ToString() && p.FlgActive == true && p.TrnsEmployeeOvertime.Period == otperiod.ID orderby p.OvertimeID select p;

        //        var query = oDB.TrnsEmployeeOvertimeDetail
        //                     .Where(bs => bs.TrnsEmployeeOvertime.EmployeeId == emp.ID && bs.DocAprStatus == "LV0006" && bs.FlgActive == true && bs.TrnsEmployeeOvertime.Period == otperiod.ID)
        //                     .GroupBy(bs => new { bs.OvertimeID })
        //                     .AsEnumerable().Select(g => new TrnsEmployeeOvertimeDetail
        //                     {
        //                         Id = g.FirstOrDefault().Id,
        //                         EmpOvertimeId = g.FirstOrDefault().EmpOvertimeId,
        //                         OvertimeID = g.FirstOrDefault().OvertimeID,
        //                         Amount = g.Sum(x => x.Amount),
        //                         OTDate = g.FirstOrDefault().OTDate,
        //                         FromTime = g.FirstOrDefault().FromTime,
        //                         ToTime = g.FirstOrDefault().ToTime,
        //                         BasicSalary = g.FirstOrDefault().BasicSalary,
        //                         ValueType = g.FirstOrDefault().ValueType,
        //                         OTValue = g.FirstOrDefault().OTValue,
        //                         FlgActive = g.FirstOrDefault().FlgActive,
        //                         OTHours = g.Sum(x => x.OTHours)
        //                         //Amount = g.Sum(x => x.Amount) 
        //                     }).ToList();

        //        int count = query.Count();
        //        if (query != null && query.Count > 0)
        //        {
        //            foreach (TrnsEmployeeOvertimeDetail otDet in query)
        //            {

        //                DataRow dr = dtOut.NewRow();
        //                amnt = (decimal)otDet.Amount;
        //                string othour = Convert.ToString(otDet.OTHours);
        //                string[] tukray = othour.Split('.');
        //                hoursfromrecord = 0;
        //                minfromrecord = 0;
        //                hoursfromrecord = Convert.ToInt32(tukray[0]);
        //                string temp = "0." + tukray[1].Substring(0, 2);
        //                minfromrecord = Convert.ToInt32(Convert.ToDecimal(temp) * 60);
        //                OTMinutes += Convert.ToInt32((hoursfromrecord * 60) + minfromrecord);
        //                MstOverTime mstOT = oDB.MstOverTime.Where(OT => OT.ID == otDet.OvertimeID).FirstOrDefault();

        //                //elementGls = getOverTimeGL(emp, otDet.MstOverTime);
        //                elementGls = getOverTimeGL(emp, mstOT);

        //                if (amnt > 0)
        //                {
        //                    TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
        //                    dr["LineType"] = "Over Time";
        //                    dr["LineSubType"] = "Over Time";
        //                    dr["LineValue"] = amnt;
        //                    dr["TaxbleAmnt"] = amnt;
        //                    dr["LineMemo"] = mstOT.Description;
        //                    dr["DebitAccount"] = elementGls["DrAcct"].ToString();
        //                    dr["CreditAccount"] = elementGls["CrAcct"].ToString();
        //                    dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
        //                    dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();

        //                    dr["LineBaseEntry"] = mstOT.ID;
        //                    string baseValType = mstOT.ValueType;
        //                    decimal baseValue = (decimal)mstOT.Value;
        //                    decimal baseCalculatedOn = 0.0M;

        //                    if (baseValType == "POB")
        //                    {
        //                        baseCalculatedOn = (decimal)emp.BasicSalary;

        //                    }
        //                    if (baseValType == "POG")
        //                    {
        //                        baseCalculatedOn = empGross;


        //                    }
        //                    if (baseValType.ToUpper() == "FIX")
        //                    {
        //                        baseCalculatedOn = (decimal)amnt;
        //                        baseValue = 100.00M;

        //                    }

        //                    dr["BaseValueCalculatedOn"] = baseCalculatedOn;
        //                    dr["BaseValue"] = baseValue;
        //                    dr["BaseValueType"] = baseValType;
        //                    dtOut.Rows.Add(dr);

        //                }
        //            }
        //        }

        //        //var OTDetails=oDB.TrnsEmployeeOvertimeDetail.Where(o=>o.TrnsEmployeeOvertime.EmployeeId==emp.ID && o.FlgActive==true && o.TrnsEmployeeOvertime.Period==otperiod.ID).GroupBy(x => new { x.MstOverTime.ID, x. })

        //        //foreach (TrnsEmployeeOvertimeDetail otDet in empPeriodOts)
        //        //{

        //        //    DataRow dr = dtOut.NewRow();
        //        //    amnt = (decimal)otDet.Amount;
        //        //    elementGls = getOverTimeGL(emp, otDet.MstOverTime);

        //        //    if (amnt > 0)
        //        //    {
        //        //        TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
        //        //        dr["LineType"] = "Over Time";
        //        //        dr["LineSubType"] = "Over Time";
        //        //        dr["LineValue"] = amnt;
        //        //        dr["TaxbleAmnt"] = amnt;
        //        //        dr["LineMemo"] = otDet.MstOverTime.Description;
        //        //        dr["DebitAccount"] = elementGls["DrAcct"].ToString();
        //        //        dr["CreditAccount"] = elementGls["CrAcct"].ToString();
        //        //        dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
        //        //        dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();

        //        //        dr["LineBaseEntry"] = otDet.Id ;
        //        //        string baseValType = otDet.MstOverTime.ValueType;
        //        //        decimal baseValue = (decimal)otDet.MstOverTime.Value;
        //        //        decimal baseCalculatedOn = 0.0M;

        //        //        if (baseValType == "POB")
        //        //        {
        //        //            baseCalculatedOn = (decimal)emp.BasicSalary;

        //        //        }
        //        //        if (baseValType == "POG")
        //        //        {
        //        //            baseCalculatedOn = empGross;


        //        //        }
        //        //        if (baseValType.ToUpper() == "FIX")
        //        //        {
        //        //            baseCalculatedOn = (decimal)amnt;
        //        //            baseValue = 100.00M;

        //        //        }

        //        //        dr["BaseValueCalculatedOn"] = baseCalculatedOn;
        //        //        dr["BaseValue"] = baseValue;
        //        //        dr["BaseValueType"] = baseValType;
        //        //        dtOut.Rows.Add(dr);
        //        //    }
        //        //}
        //    }
        //    return dtOut;
        //}

        public DataTable DynamicOTProcessing(MstEmployee emp, CfgPeriodDates oPeriod, decimal empGross, out Int32 OTMinutes, DateTime EndDate)
        {
            decimal otAmount = 0;
            OTMinutes = 0;
            Hashtable elementGls = new Hashtable();

            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");
            var oOvertimeCollection = oDB.SpGetPeriodOverTime(emp.EmpID, oPeriod.StartDate, EndDate).ToList();
            if (oOvertimeCollection.Count > 0)
            {
                foreach (var OneLine in oOvertimeCollection)
                {
                    var oOTMaster = (from a in oDB.MstOverTime
                                     where a.Code == OneLine.Code
                                     select a).FirstOrDefault();
                    string OTType = "";
                    decimal OTYearDays = 0, OTMonthDays = 0, baseValue = 0, fixValue = 0, otValue = 0, otWorkHour = 0, monthHour = 0, ValueCheck = 0;
                    Int32 otHours = 0, otMinutes = 0;
                    decimal otTime = 0;
                    OTMonthDays = oOTMaster.Days == null ? 0 : Convert.ToDecimal(oOTMaster.Days);
                    OTYearDays = oOTMaster.DaysinYear == null ? 0 : Convert.ToDecimal(oOTMaster.DaysinYear);
                    fixValue = oOTMaster.FixValue == null ? 0 : Convert.ToDecimal(oOTMaster.FixValue);
                    otValue = oOTMaster.Value == null ? 0 : Convert.ToDecimal(oOTMaster.Value);
                    OTType = string.IsNullOrEmpty(oOTMaster.ValueType) ? "POB" : Convert.ToString(oOTMaster.ValueType);
                    if (oOTMaster.Hours != null)
                    {
                        otWorkHour = Convert.ToDecimal(oOTMaster.Hours);
                    }
                    if (otWorkHour == 0)
                    {
                        if (emp.CfgPayrollDefination != null)
                        {
                            otWorkHour = Convert.ToDecimal(emp.CfgPayrollDefination.WorkHours);
                        }
                    }
                    if (OTYearDays == 0)
                    {
                        OTYearDays = 365;
                    }
                    if (OTMonthDays == 0)
                    {
                        OTMonthDays = (decimal)((Convert.ToDateTime(oPeriod.EndDate) - Convert.ToDateTime(oPeriod.StartDate)).TotalDays + 1);
                    }
                    if (OTType == "POB")
                    {
                        baseValue = Convert.ToDecimal(((Convert.ToDecimal(emp.BasicSalary) / 100) * otValue) + fixValue);
                    }
                    else if (OTType == "POG")
                    {
                        baseValue = ((Convert.ToDecimal(empGross) / 100) * otValue) + fixValue;
                    }
                    else if (OTType == "FIX")
                    {
                        baseValue = Convert.ToDecimal(oOTMaster.Value) + fixValue;
                    }
                    int dur = (Convert.ToInt32(OneLine.Hours) * 60) + (Convert.ToInt32(OneLine.Minutes));
                    otHours = dur / 60;
                    otMinutes = dur % 60;
                    string duration = otHours.ToString().Trim().PadLeft(2, '0') + ":" + otMinutes.ToString().Trim().PadLeft(2, '0');

                    //otTime = (decimal)TimeSpan.Parse(duration).TotalHours;
                    otTime = Convert.ToDecimal(otHours) + (Convert.ToDecimal(otMinutes) / 60M);
                    monthHour = OTMonthDays * otWorkHour;
                    otAmount = (baseValue / monthHour) * otTime;
                    if (otAmount > 0)
                    {
                        elementGls = getOverTimeGL(emp, oOTMaster);
                        DataRow dr = dtOut.NewRow();
                        dr["LineType"] = "Over Time";
                        dr["LineSubType"] = "Over Time";
                        dr["LineValue"] = otAmount;
                        dr["TaxbleAmnt"] = otAmount;
                        //dr["LineMemo"] = mstOT.Description;
                        dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                        dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                        dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                        dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();

                        //dr["LineBaseEntry"] = mstOT.ID;
                        //string baseValType = mstOT.ValueType;
                        //decimal baseValue = (decimal)mstOT.Value;
                        decimal baseCalculatedOn = 0.0M;

                        //dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                        //dr["BaseValue"] = baseValue;
                        //dr["BaseValueType"] = baseValType;
                        dtOut.Rows.Add(dr);

                    }
                }
            }
            return dtOut;
        }

        public DataTable salaryProcessingAbsents(MstEmployee emp, CfgPeriodDates leavePeriod, decimal grossSalary, out decimal LeaveCnt)
        {
            decimal amnt = 0.0M;
            int DaysCnt = 0;
            Hashtable elementGls = new Hashtable();
            LeaveCnt = 0.00M;
            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");
            decimal GrossSalary = grossSalary;
            decimal basicSalary = (decimal)emp.BasicSalary;
            int workDays = Convert.ToInt16(emp.CfgPayrollDefination.WorkDays);
            if (workDays == 0.00M)
            {
                workDays = (Convert.ToDateTime(leavePeriod.EndDate) - Convert.ToDateTime(leavePeriod.StartDate)).Days;
                workDays = workDays + 1;
            }
            decimal workHours = Convert.ToDecimal(emp.CfgPayrollDefination.WorkHours);
            decimal perdayGross = GrossSalary / workDays;
            decimal perdayBasic = basicSalary / workDays;



            IEnumerable<TrnsLeavesRequest> empLeaves = from p in oDB.TrnsLeavesRequest where p.EmpID.ToString() == emp.ID.ToString() && ((p.LeaveFrom >= leavePeriod.StartDate && p.LeaveFrom <= leavePeriod.EndDate) || (p.LeaveTo >= leavePeriod.StartDate && p.LeaveTo <= leavePeriod.EndDate)) && (p.DeductAmnt > 0 || p.PendingDedAmnt > 0) && p.DocStatus == "LV0002" select p;

            foreach (TrnsLeavesRequest lDet in empLeaves)
            {
                DataRow dr = dtOut.NewRow();
                decimal leaveCount = 0.00M;

                if (lDet.LeaveFrom >= leavePeriod.StartDate && lDet.LeaveTo <= leavePeriod.EndDate)
                {
                    leaveCount = (decimal)lDet.TotalCount;

                }
                if (lDet.LeaveFrom >= leavePeriod.StartDate && lDet.LeaveTo > leavePeriod.EndDate)
                {
                    leaveCount = (decimal)(Convert.ToDateTime(leavePeriod.EndDate) - Convert.ToDateTime(lDet.LeaveFrom)).Days + 1;

                }
                if (lDet.LeaveFrom < leavePeriod.StartDate && lDet.LeaveTo <= leavePeriod.EndDate)
                {
                    leaveCount = (decimal)(Convert.ToDateTime(lDet.LeaveTo) - Convert.ToDateTime(leavePeriod.StartDate)).Days + 1;

                }
                if (lDet.LeaveFrom < leavePeriod.StartDate && lDet.LeaveTo > leavePeriod.EndDate)
                {
                    leaveCount = workDays;

                }
                if (leaveCount <= 0) return dtOut;

                string deductId = "";
                if (lDet.DocAprStatus == "LV0006")
                {
                    deductId = lDet.DeductId;
                    amnt = leaveCount / (decimal)lDet.TotalCount * (decimal)lDet.DeductAmnt;
                }
                if (lDet.DocAprStatus == "LV0005" || lDet.DocAprStatus == "LV0007")
                {
                    deductId = lDet.PendingDedId;
                    amnt = leaveCount / (decimal)lDet.TotalCount * (decimal)lDet.PendingDedAmnt;
                }

                int leaveDeductedUnit = (int)lDet.DeductedUnit;

                if (deductId == null || deductId == "") return dtOut;
                MstLeaveDeduction leaveDed = (from p in oDB.MstLeaveDeduction where p.Code == deductId select p).Single();
                LeaveCnt += leaveCount;
                if (amnt > 0)
                {
                    elementGls = getLeaveDedGL(emp, leaveDed.Id);

                    TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                    dr["LineType"] = "Absent";
                    dr["LineSubType"] = "Deduction";
                    dr["LineValue"] = -amnt;
                    dr["TaxbleAmnt"] = -amnt;
                    dr["LineMemo"] = lDet.LeaveDescription;
                    dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                    dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                    dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                    dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();

                    dr["LineBaseEntry"] = lDet.ID.ToString();
                    string baseValType = leaveDed.TypeofDeduction;
                    decimal baseValue = (decimal)amnt;
                    decimal baseCalculatedOn = 0.0M;

                    if (baseValType == "POB")
                    {
                        baseCalculatedOn = (decimal)emp.BasicSalary;

                    }
                    if (baseValType == "POG")
                    {
                        baseCalculatedOn = grossSalary;


                    }
                    if (baseValType.ToUpper() == "FIX")
                    {
                        baseCalculatedOn = (decimal)amnt;
                        baseValue = 100.00M;

                    }

                    dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                    dr["BaseValue"] = baseValue;
                    dr["BaseValueType"] = baseValType;
                    dtOut.Rows.Add(dr);
                }

            }
            return dtOut;
        }

        public DataTable DynamicLeavesProcessing(MstEmployee emp, CfgPeriodDates oPeriod, decimal grossSalary, out decimal LeaveCnt, DateTime EndDate)
        {
            decimal amnt = 0.0M;
            int DaysCnt = 0;
            Hashtable elementGls = new Hashtable();
            LeaveCnt = 0.00M;
            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");
            try
            {
                decimal GrossSalary = grossSalary;
                decimal basicSalary = (decimal)emp.BasicSalary;
                int workDays = Convert.ToInt16(emp.CfgPayrollDefination.WorkDays);
                if (workDays == 0.00M)
                {
                    workDays = (Convert.ToDateTime(oPeriod.EndDate) - Convert.ToDateTime(oPeriod.StartDate)).Days;
                    workDays = workDays + 1;
                }
                decimal workHours = Convert.ToDecimal(emp.CfgPayrollDefination.WorkHours);
                var oLeaveCollection = oDB.SpGetPeriodLeaves(emp.EmpID, oPeriod.StartDate, EndDate).ToList();
                foreach (var Line in oLeaveCollection)
                {
                    decimal leavecount = 0m, leaveamount = 0m;
                    leavecount = Convert.ToDecimal(Line.LeaveCount);
                    var oDedRecord = (from a in oDB.MstLeaveDeduction
                                      where a.Code == Line.DedCode
                                      select a).FirstOrDefault();
                    if (oDedRecord != null)
                    {
                        if (oDedRecord.TypeofDeduction.Trim().ToUpper() == "POB")
                        {
                            leaveamount = ((((basicSalary / 100) * (Convert.ToDecimal(oDedRecord.DeductionValue))) / workDays) * leavecount);
                        }
                        else if (oDedRecord.TypeofDeduction.Trim().ToUpper() == "POG")
                        {
                            leaveamount = ((((grossSalary / 100) * (Convert.ToDecimal(oDedRecord.DeductionValue))) / workDays) * leavecount);
                        }
                        else
                        {
                            leaveamount = (Convert.ToDecimal(oDedRecord.DeductionValue) * leavecount);
                        }
                        if (leaveamount > 0)
                        {
                            elementGls = getLeaveDedGL(emp, oDedRecord.Id);
                            DataRow dr = dtOut.NewRow();
                            dr["LineType"] = "Absent";
                            dr["LineSubType"] = "Deduction";
                            dr["LineValue"] = -leaveamount;
                            dr["TaxbleAmnt"] = -leaveamount;
                            dr["LineMemo"] = Line.Description;
                            dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                            dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                            dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                            dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();

                            dr["LineBaseEntry"] = oDedRecord.Id.ToString();
                            string baseValType = oDedRecord.TypeofDeduction;
                            decimal baseValue = (decimal)leaveamount;
                            decimal baseCalculatedOn = 0.0M;
                            dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                            dr["BaseValue"] = baseValue;
                            dr["BaseValueType"] = baseValType;
                            dtOut.Rows.Add(dr);
                        }
                    }
                }
                return dtOut;
            }
            catch
            {
                return dtOut;
            }
        }

        public DataTable salaryProcessingAbsents(MstEmployee emp, CfgPeriodDates leavePeriod, decimal grossSalary, out decimal LeaveCnt, MstGLDetermination emGl)
        {
            decimal amnt = 0.0M;
            int DaysCnt = 0;
            Hashtable elementGls = new Hashtable();
            LeaveCnt = 0.00M;
            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            dtOut.Columns.Add("TaxbleAmnt");
            decimal GrossSalary = grossSalary;
            decimal basicSalary = (decimal)emp.BasicSalary;
            int workDays = Convert.ToInt16(emp.CfgPayrollDefination.WorkDays);
            if (workDays == 0.00M)
            {
                workDays = (Convert.ToDateTime(leavePeriod.EndDate) - Convert.ToDateTime(leavePeriod.StartDate)).Days;
                workDays = workDays + 1;
            }
            decimal workHours = Convert.ToDecimal(emp.CfgPayrollDefination.WorkHours);
            decimal perdayGross = GrossSalary / workDays;
            decimal perdayBasic = basicSalary / workDays;



            IEnumerable<TrnsLeavesRequest> empLeaves = from p in oDB.TrnsLeavesRequest
                                                       where p.EmpID.ToString() == emp.ID.ToString()
                                                       && ((p.LeaveFrom >= leavePeriod.StartDate
                                                       && p.LeaveFrom <= leavePeriod.EndDate)
                                                       || (p.LeaveTo >= leavePeriod.StartDate
                                                       && p.LeaveTo <= leavePeriod.EndDate))
                                                       && (p.DeductAmnt > 0 || p.PendingDedAmnt > 0)
                                                       && p.DocStatus == "LV0002" && p.FlgPaid == false
                                                       select p;

            foreach (TrnsLeavesRequest lDet in empLeaves)
            {
                DataRow dr = dtOut.NewRow();
                decimal leaveCount = 0.00M;

                if (lDet.LeaveFrom >= leavePeriod.StartDate && lDet.LeaveTo <= leavePeriod.EndDate)
                {
                    leaveCount = (decimal)lDet.TotalCount;

                }
                if (lDet.LeaveFrom >= leavePeriod.StartDate && lDet.LeaveTo > leavePeriod.EndDate)
                {
                    leaveCount = (decimal)(Convert.ToDateTime(leavePeriod.EndDate) - Convert.ToDateTime(lDet.LeaveFrom)).Days + 1;

                }
                if (lDet.LeaveFrom < leavePeriod.StartDate && lDet.LeaveTo <= leavePeriod.EndDate)
                {
                    leaveCount = (decimal)(Convert.ToDateTime(lDet.LeaveTo) - Convert.ToDateTime(leavePeriod.StartDate)).Days + 1;

                }
                if (lDet.LeaveFrom < leavePeriod.StartDate && lDet.LeaveTo > leavePeriod.EndDate)
                {
                    leaveCount = workDays;

                }
                if (leaveCount <= 0) return dtOut;

                string deductId = "";
                if (lDet.DocAprStatus == "LV0006")
                {
                    deductId = lDet.DeductId;
                    amnt = leaveCount / (decimal)lDet.TotalCount * (decimal)lDet.DeductAmnt;
                }
                //if (lDet.DocAprStatus == "LV0005" || lDet.DocAprStatus == "LV0007")
                //{
                //    deductId = lDet.PendingDedId;
                //    amnt = leaveCount / (decimal)lDet.TotalCount * (decimal)lDet.PendingDedAmnt;
                //}

                int leaveDeductedUnit = (int)lDet.DeductedUnit;

                if (deductId == null || deductId == "" || deductId == "-1") return dtOut;
                MstLeaveDeduction leaveDed = (from p in oDB.MstLeaveDeduction where p.Code == deductId select p).FirstOrDefault();
                LeaveCnt += leaveCount;
                if (amnt > 0)
                {
                    elementGls = getLeaveDedGL(emp, leaveDed.Id);

                    dr["LineType"] = "Absent";
                    dr["LineSubType"] = "Deduction";
                    dr["LineValue"] = -amnt;
                    dr["TaxbleAmnt"] = -amnt;
                    dr["LineMemo"] = lDet.LeaveDescription;
                    dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                    dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                    dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                    dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();

                    dr["LineBaseEntry"] = lDet.ID.ToString();
                    string baseValType = leaveDed.TypeofDeduction;
                    decimal baseValue = (decimal)amnt;
                    decimal baseCalculatedOn = 0.0M;

                    if (baseValType == "POB")
                    {
                        baseCalculatedOn = (decimal)emp.BasicSalary;

                    }
                    if (baseValType == "POG")
                    {
                        baseCalculatedOn = grossSalary;


                    }
                    if (baseValType.ToUpper() == "FIX")
                    {
                        baseCalculatedOn = (decimal)amnt;
                        baseValue = 100.00M;

                    }

                    dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                    dr["BaseValue"] = baseValue;
                    dr["BaseValueType"] = baseValType;
                    dtOut.Rows.Add(dr);
                }



            }
            decimal attAdjDays = 0.00M;
            string strSql = "SELECT ISNULL(dbo.attSummary.leaveId, 0) As LeaveId,ISNULL(dbo.attSummary.overtimeId, 0) As OvertimeId,     ISNULL(SUM(dbo.attSummaryDetail.adjDays), 0) AS daysAdj , Remarks " +
                 " FROM         dbo.attSummaryDetail INNER JOIN " +
                 "  dbo.attSummary ON dbo.attSummaryDetail.attSummaryId = dbo.attSummary.attSummaryID " +
                 "  WHERE     (dbo.attSummaryDetail.empId = '" + emp.ID.ToString() + "') AND (dbo.attSummary.PeriodId = '" + leavePeriod.ID.ToString() + "') AND (dbo.attSummaryDetail.flgActive = 1) " +
                 " group by Remarks,leaveId,OvertimeId  ";

            DataTable dtAttDays = getDataTable(strSql);
            foreach (DataRow drAdj in dtAttDays.Rows)
            {
                DataRow dr = dtOut.NewRow();

                attAdjDays = Convert.ToDecimal(drAdj["daysAdj"]);

                string dedReason = drAdj["Remarks"].ToString();
                string strLeaveId = Convert.ToString(drAdj["LeaveId"]);
                string strOvertimeId = Convert.ToString(drAdj["OvertimeId"]);
                if (!string.IsNullOrEmpty(strLeaveId))
                {
                    var mstLeaveType = oDB.MstLeaveType.Where(l => l.ID == Convert.ToInt32(strLeaveId)).FirstOrDefault();

                    //  if (dedReason == "") dedReason = "Absent Deduction";
                    string deductId = "LWS";
                    decimal dayAdjAmt = 0.00M;
                    if (leavePeriod.CfgPayrollDefination.WorkDays != 0)
                    {
                        dayAdjAmt = attAdjDays * (decimal)(basicSalary / leavePeriod.CfgPayrollDefination.WorkDays);
                    }
                    else
                    {
                        dayAdjAmt = attAdjDays * (decimal)(basicSalary / workDays);
                    }
                    int cntLws = 0;
                    if (mstLeaveType != null)
                    {
                        cntLws = (from p in oDB.MstLeaveDeduction where p.Code == mstLeaveType.DeductionCode select p).Count();
                    }
                    else
                    {
                        cntLws = (from p in oDB.MstLeaveDeduction where p.Code == deductId select p).Count();
                    }
                    if (cntLws > 0)
                    {
                        MstElements Ele;
                        MstLeaveDeduction leaveDed = null;
                        if (mstLeaveType != null)
                        {
                            leaveDed = (from p in oDB.MstLeaveDeduction where p.Code == mstLeaveType.DeductionCode select p).FirstOrDefault();
                        }
                        else
                        {
                            leaveDed = (from p in oDB.MstLeaveDeduction where p.Code == deductId select p).FirstOrDefault();
                        }
                        if (dayAdjAmt != 0)
                        {
                            if (dayAdjAmt > 0)
                            {
                                Ele = (from p in oDB.MstElements where p.Id == emp.CfgPayrollDefination.AddDaysEle select p).FirstOrDefault();
                                elementGls = getElementGL(emp, Ele, emGl);
                            }
                            else
                            {
                                Ele = (from p in oDB.MstElements where p.Id == emp.CfgPayrollDefination.DedDaysEle select p).FirstOrDefault();
                                elementGls = getElementGL(emp, Ele, emGl);
                            }
                            dr["LineType"] = "DaysAdj";
                            dr["LineSubType"] = Ele.ElementName;
                            dr["LineValue"] = (decimal)-dayAdjAmt;// dayAdjAmt;
                            dr["TaxbleAmnt"] = (decimal)-dayAdjAmt;// dayAdjAmt;
                            dr["LineMemo"] = "Absent_DaysAdjustment";//Ele.Description;
                            dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                            dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                            dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                            dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();
                            dr["LineBaseEntry"] = leaveDed.Id.ToString();
                            string baseValType = "POB";
                            decimal baseValue = (decimal)-dayAdjAmt;
                            decimal baseCalculatedOn = attAdjDays;
                            baseCalculatedOn = basicSalary;
                            dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                            dr["BaseValue"] = baseValue;
                            dr["BaseValueType"] = baseValType;
                            dtOut.Rows.Add(dr);
                        }
                    }
                }
            }
            decimal attHrsDays = 0.00M;
            string strSqlHrs = "SELECT     ISNULL(SUM(dbo.attSummaryDetail.adjHrs), 0) AS HrsAdj , Remarks " +
                 " FROM         dbo.attSummaryDetail INNER JOIN " +
                 "  dbo.attSummary ON dbo.attSummaryDetail.attSummaryId = dbo.attSummary.attSummaryID " +
                 "  WHERE     (dbo.attSummaryDetail.empId = '" + emp.ID.ToString() + "') AND (dbo.attSummary.PeriodId = '" + leavePeriod.ID.ToString() + "') AND (dbo.attSummaryDetail.flgActive = 1) " +
                 " group by Remarks ";

            DataTable dtAttHrs = getDataTable(strSqlHrs);

            foreach (DataRow drAdj in dtAttHrs.Rows)
            {
                DataRow dr = dtOut.NewRow();


                attHrsDays = Convert.ToDecimal(drAdj["HrsAdj"]);

                string dedReason = drAdj["Remarks"].ToString();
                //  if (dedReason == "") dedReason = "Absent Deduction";
                string deductId = "LWS";
                int cntLws = (from p in oDB.MstLeaveDeduction where p.Code == deductId select p).Count();
                decimal HrsAdjAmt = 0.00M;
                if (leavePeriod.CfgPayrollDefination.WorkDays != 0)
                {
                    HrsAdjAmt = attHrsDays * (decimal)(basicSalary / leavePeriod.CfgPayrollDefination.WorkDays);
                }
                else
                {
                    HrsAdjAmt = attHrsDays * (decimal)(basicSalary / workDays);
                }
                //if (HrsAdjAmt != 0)
                //{
                //    if (leavePeriod.CfgPayrollDefination.WorkDays != 0)
                //    {
                //        HrsAdjAmt = (decimal)(HrsAdjAmt / leavePeriod.CfgPayrollDefination.WorkHours);
                //    }
                //    else
                //    {
                //        HrsAdjAmt = (decimal)(HrsAdjAmt / workDays);
                //    }
                //}                
                if (cntLws > 0)
                {
                    MstElements Ele;
                    //MstLeaveDeduction leaveDed = (from p in oDB.MstLeaveDeduction where p.Code == deductId select p).Single();
                    if (HrsAdjAmt != 0)
                    {
                        if (HrsAdjAmt > 0)
                        {
                            Ele = (from p in oDB.MstElements where p.Id == emp.CfgPayrollDefination.AddDaysEle select p).Single();
                            elementGls = getElementGL(emp, Ele, emGl);
                        }
                        else
                        {
                            Ele = (from p in oDB.MstElements where p.Id == emp.CfgPayrollDefination.DedDaysEle select p).Single();
                            elementGls = getElementGL(emp, Ele, emGl);
                        }
                        dr["LineType"] = "DaysAdj";
                        dr["LineSubType"] = Ele.ElementName;
                        dr["LineValue"] = HrsAdjAmt;
                        dr["TaxbleAmnt"] = HrsAdjAmt;
                        dr["LineMemo"] = "OverTime_DaysAdjustment";//Ele.Description;
                        dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                        dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                        dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                        dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();
                        dr["LineBaseEntry"] = Ele.Id.ToString();
                        string baseValType = "POB";
                        decimal baseValue = (decimal)-HrsAdjAmt;
                        decimal baseCalculatedOn = attAdjDays;
                        baseCalculatedOn = basicSalary;
                        dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                        dr["BaseValue"] = baseValue;
                        dr["BaseValueType"] = baseValType;
                        dtOut.Rows.Add(dr);
                    }
                }
            }
            return dtOut;
        }

        public void EmployeeReferralsPayments(MstEmployee oEmp, CfgPeriodDates oPeriod)
        {
            try
            {
                decimal runningamount = 0;
                int elementid = 0;
                Hashtable elementGls = new Hashtable();
                var oEmpRef = (from a in oDB.MstEmployeeReferrals where a.MstEmployee.EmpID == oEmp.EmpID select a).FirstOrDefault();
                if (oEmpRef == null) return;
                if (oEmpRef.MstEmployeeReferralsDetails.Count > 0)
                {
                    var oCollection = (from a in oDB.MstReferralSchemes where a.FlgActive == true select a).ToList();
                    foreach (var List in oCollection)
                    {
                        elementid = Convert.ToInt32(List.ElementID);
                        foreach (var OneRef in oEmpRef.MstEmployeeReferralsDetails)
                        {
                            if (OneRef.FlgActive == false) continue;
                            DateTime JoiningDate = Convert.ToDateTime(OneRef.MstEmployee.JoiningDate);
                            DateTime ToDate = Convert.ToDateTime(oPeriod.StartDate).AddDays(-1);
                            int Months = Convert.ToInt32(List.Months);
                            int CalculatedMonths = 0;
                            TimeSpan oSpan = ToDate - JoiningDate;
                            CalculatedMonths = Convert.ToInt32(Math.Floor(oSpan.TotalDays / 30));
                            if (Months == CalculatedMonths)
                            {
                                runningamount += Convert.ToDecimal(List.PValue);
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }

                    //Enter Full Amount with Detials
                    if (runningamount == 0) return;
                    TrnsEmployeeElement oEmpEle = (from a in oDB.TrnsEmployeeElement where a.MstEmployee.EmpID == oEmp.EmpID select a).FirstOrDefault();
                    if (oEmpEle == null) return;
                    MstElements oElement = (from a in oDB.MstElements where a.Id == elementid select a).FirstOrDefault();
                    if (oElement == null) return;
                    int chkDetail = 0;
                    chkDetail = (from a in oDB.TrnsEmployeeElementDetail where a.TrnsEmployeeElement.Id == oEmpEle.Id && a.PeriodId == oPeriod.ID && a.MstElements.Id == oElement.Id select a).Count();
                    TrnsEmployeeElementDetail oDoc = null;
                    if (chkDetail == 0)
                    {
                        oDoc = new TrnsEmployeeElementDetail();
                        oEmpEle.TrnsEmployeeElementDetail.Add(oDoc);
                    }
                    else
                    {
                        oDoc = (from a in oDB.TrnsEmployeeElementDetail where a.TrnsEmployeeElement.Id == oEmpEle.Id && a.PeriodId == oPeriod.ID && a.MstElements.Id == oElement.Id select a).FirstOrDefault();
                    }
                    oDoc.MstElements = oElement;
                    oDoc.StartDate = oElement.StartDate;
                    oDoc.EndDate = oElement.EndDate;
                    oDoc.FlgRetro = false;
                    oDoc.RetroAmount = 0M;
                    oDoc.FlgActive = true;
                    oDoc.FlgOneTimeConsumed = false;
                    oDoc.PeriodId = oPeriod.ID;
                    oDoc.ElementType = oElement.ElmtType;
                    oDoc.ValueType = oElement.MstElementEarning[0].ValueType;
                    oDoc.Value = runningamount;
                    oDoc.Amount = runningamount;
                    oDoc.EmpContr = 0M;
                    oDoc.EmplrContr = 0M;

                    oDB.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void EmployeeAttendanceAllowanceFromMaster(MstEmployee oEmp, CfgPeriodDates oPeriod)
        {
            try
            {
                Hashtable elementGls = new Hashtable();

                var CheckEnteredLeaves = (from a in oDB.TrnsLeavesRequest
                                          where a.MstEmployee.EmpID == oEmp.EmpID
                                          && oPeriod.StartDate <= a.LeaveFrom
                                          && oPeriod.EndDate >= a.LeaveFrom
                                           && a.LeaveType == a.MstLeaveType.ID
                                       && a.MstLeaveType.LeaveType == "Ded"
                                          select a).FirstOrDefault();
                if (CheckEnteredLeaves != null) return;

                decimal TotalLeaveUsed = 0;
                #region Calculate Allowance Amount
                TotalLeaveUsed = (from a in oDB.TrnsLeavesRequest
                                  where a.MstEmployee.EmpID == oEmp.EmpID
                                  && oPeriod.StartDate <= a.LeaveFrom
                                       && oPeriod.EndDate >= a.LeaveFrom
                                       && a.LeaveType == a.MstLeaveType.ID
                                       && a.MstLeaveType.LeaveType != "Ded"
                                  select a.TotalCount).Sum() ?? 0;
                var oMasterAllowanceCollection = (from a in oDB.MstAttendanceAllowance
                                                  where a.FlgActive == true
                                                  && a.DocNo == oEmp.AttendanceAllowance
                                                  select a).OrderBy(o => o.LeaveCount).ToList();
                int count = 1;
                string ElementCode = "";
                decimal ElementAmount = 0M;

                var maxLimit = oMasterAllowanceCollection.FirstOrDefault(o => o.LeaveCount == oMasterAllowanceCollection.Max(e => e.LeaveCount));

                if (maxLimit != null)
                {
                    foreach (var OneAllowance in oMasterAllowanceCollection)
                    {
                        string AllowanceCode = OneAllowance.Code;
                        if (TotalLeaveUsed >= maxLimit.LeaveCount)
                        {
                            ElementAmount = Convert.ToDecimal(maxLimit.Value);
                            ElementCode = maxLimit.ElementType;
                            break;
                        }
                        else if (TotalLeaveUsed <= OneAllowance.LeaveCount)
                        {
                            ElementAmount = Convert.ToDecimal(OneAllowance.Value);
                            ElementCode = OneAllowance.ElementType;
                            break;
                        }
                    }
                }
                #endregion

                if (ElementCode != "" && ElementAmount != 0M)
                {
                    //Enter Full Amount with Detials
                    TrnsEmployeeElement oEmpEle = (from a in oDB.TrnsEmployeeElement where a.MstEmployee.EmpID == oEmp.EmpID select a).FirstOrDefault();
                    if (oEmpEle == null) return;
                    MstElements oElement = (from a in oDB.MstElements where a.ElementName == ElementCode select a).FirstOrDefault();
                    if (oElement == null) return;
                    int chkDetail = 0;
                    //chkDetail = (from a in oDB.TrnsEmployeeElementDetail where a.TrnsEmployeeElement.Id == oEmpEle.Id && a.PeriodId == oPeriod.ID && a.MstElements.Id == oElement.Id select a).Count();
                    chkDetail = (from a in oDB.TrnsEmployeeElementDetail where a.TrnsEmployeeElement.Id == oEmpEle.Id && a.MstElements.Id == oElement.Id select a).Count();
                    TrnsEmployeeElementDetail oDoc = null;
                    if (chkDetail == 0)
                    {
                        oDoc = new TrnsEmployeeElementDetail();
                        oEmpEle.TrnsEmployeeElementDetail.Add(oDoc);
                    }
                    else
                    {
                        //oDoc = (from a in oDB.TrnsEmployeeElementDetail where a.TrnsEmployeeElement.Id == oEmpEle.Id && a.PeriodId == oPeriod.ID && a.MstElements.Id == oElement.Id select a).FirstOrDefault();
                        oDoc = (from a in oDB.TrnsEmployeeElementDetail where a.TrnsEmployeeElement.Id == oEmpEle.Id && a.MstElements.Id == oElement.Id select a).FirstOrDefault();
                    }
                    oDoc.MstElements = oElement;
                    oDoc.StartDate = oElement.StartDate;
                    oDoc.EndDate = oElement.EndDate;
                    oDoc.FlgRetro = false;
                    oDoc.RetroAmount = 0M;
                    oDoc.FlgActive = true;
                    oDoc.FlgOneTimeConsumed = false;
                    oDoc.PeriodId = oPeriod.ID;
                    oDoc.ElementType = oElement.ElmtType;
                    oDoc.ValueType = oElement.MstElementEarning[0].ValueType;
                    //oDoc.Value = oElement.MstElementEarning[0].Value;
                    oDoc.Value = ElementAmount;
                    oDoc.Amount = ElementAmount;
                    oDoc.EmpContr = 0M;
                    oDoc.EmplrContr = 0M;

                    oDB.SubmitChanges();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void EmployeeAttendanceAllowance(MstEmployee oEmp, CfgPeriodDates oPeriod)
        {
            try
            {
                decimal runningamount = 0;
                int elementid = 0;
                int elementidPays = 0;
                decimal TotalLeave = 0.5M;
                Hashtable elementGls = new Hashtable();
                var oEmpValidity = (from a in oDB.TrnsEmployeeAttendanceAllowanceDetail where a.MstEmployee.EmpID == oEmp.EmpID && a.TrnsEmployeeAttendanceAllowance.DocStatus == true select a).FirstOrDefault();
                if (oEmpValidity == null) return;
                var CheckEnteredLeaves = (from a in oDB.TrnsLeavesRequest
                                          where a.MstEmployee.EmpID == oEmp.EmpID && oPeriod.StartDate <= a.LeaveFrom
                                          && oPeriod.EndDate >= a.LeaveFrom && a.TotalCount >= TotalLeave
                                          select a).FirstOrDefault();
                if (CheckEnteredLeaves != null) return;
                elementid = Convert.ToInt32(oEmpValidity.TrnsEmployeeAttendanceAllowance.CalculatedOn);
                elementidPays = Convert.ToInt32(oEmpValidity.TrnsEmployeeAttendanceAllowance.PaysThrough);

                //Enter Full Amount with Detials
                TrnsEmployeeElement oEmpEle = (from a in oDB.TrnsEmployeeElement where a.MstEmployee.EmpID == oEmp.EmpID select a).FirstOrDefault();
                if (oEmpEle == null) return;
                MstElements oElement = (from a in oDB.MstElements where a.Id == elementid select a).FirstOrDefault();
                if (oElement == null) return;
                MstElements oElementPaysThrough = (from a in oDB.MstElements where a.Id == elementidPays select a).FirstOrDefault();
                if (oElementPaysThrough == null) return;

                int chkDetail = 0;
                chkDetail = (from a in oDB.TrnsEmployeeElementDetail where a.TrnsEmployeeElement.Id == oEmpEle.Id && a.PeriodId == oPeriod.ID && a.MstElements.Id == oElementPaysThrough.Id select a).Count();
                TrnsEmployeeElementDetail oDoc = new TrnsEmployeeElementDetail();
                #region Element Calculation
                var oElementEar = (from a in oDB.MstElementEarning where a.ElementID == oElement.Id select a).FirstOrDefault();
                if (oElementEar == null) return;
                string Elementname = oElement.ElementName;
                //
                //CalculateElementValues(oElement.ElmtType, oEmp, oElementEar.ValueType, oDoc);
                decimal outValue = 0M;
                if (oElementEar.ValueType == "POB")
                {
                    oDoc.Value = Convert.ToDecimal(oElementEar.Value);
                    outValue = Convert.ToDecimal(oElementEar.Value) / 100 * (decimal)oEmp.BasicSalary;
                    oDoc.Amount = outValue;
                }
                if (oElementEar.ValueType == "POG")
                {
                    oDoc.Value = Convert.ToDecimal(oElementEar.Value);
                    outValue = Convert.ToDecimal(oElementEar.Value) / 100 * (decimal)getEmpGross(oEmp);
                    oDoc.Amount = outValue;
                }
                if (oElementEar.ValueType == "FIX")
                {
                    oDoc.Value = Convert.ToDecimal(oElementEar.Value);
                    oDoc.Amount = Convert.ToDecimal(oElementEar.Value);
                }
                //
                #endregion
                if (chkDetail == 0)
                {
                    oDoc = new TrnsEmployeeElementDetail();
                    oEmpEle.TrnsEmployeeElementDetail.Add(oDoc);
                }
                else
                {
                    oDoc = (from a in oDB.TrnsEmployeeElementDetail where a.TrnsEmployeeElement.Id == oEmpEle.Id && a.PeriodId == oPeriod.ID && a.MstElements.Id == oElementPaysThrough.Id select a).FirstOrDefault();
                }
                oDoc.MstElements = oElementPaysThrough;
                oDoc.StartDate = oElementPaysThrough.StartDate;
                oDoc.EndDate = oElementPaysThrough.EndDate;
                oDoc.FlgRetro = false;
                oDoc.RetroAmount = 0M;
                oDoc.FlgActive = true;
                oDoc.FlgOneTimeConsumed = false;
                oDoc.PeriodId = oPeriod.ID;
                oDoc.ElementType = oElementPaysThrough.ElmtType;
                oDoc.ValueType = oElementPaysThrough.MstElementEarning[0].ValueType;
                oElementPaysThrough.MstElementEarning[0].Value = outValue;
                oDoc.Value = oElementPaysThrough.MstElementEarning[0].Value;
                //oDoc.Value = outValue;
                oDoc.Amount = 0M;
                oDoc.EmpContr = 0M;
                oDoc.EmplrContr = 0M;

                oDB.SubmitChanges();
            }
            catch (Exception ex)
            {
                string mess = ex.Message;
                Console.WriteLine(mess);
            }
        }

        public void EmployeeNoLateAllowance(MstEmployee oEmp, CfgPeriodDates oPeriod)
        {
            try
            {
                decimal runningamount = 0;
                int elementid = 0;
                int elementidPays = 0;
                int LateCount = 0;

                Hashtable elementGls = new Hashtable();
                //LateCount = (from a in oDB.TrnsAttendanceRegister where a.EmpID == oEmp.ID && a.PeriodID == oPeriod.ID && (a.LateInMin != "00:00" ) || (a.EarlyOutMin != "00:00") select a).Count();
                //LateCount = (from a in oDB.TrnsAttendanceRegister where a.EmpID == oEmp.ID && a.PeriodID == oPeriod.ID && (a.LateInMin == "00:00" || a.LateInMin == "") && (a.EarlyOutMin == "00:00" || a.EarlyOutMin == "") select a).Count();
                LateCount = (from a in oDB.TrnsAttendanceRegister
                             where a.EmpID == oEmp.ID &&
                             (a.Date >= oPeriod.StartDate && a.Date <= oPeriod.EndDate) &&
                             ((a.LateInMin != "" && a.LateInMin != "00:00" && a.LateInMin != null) ||
                             (a.EarlyOutMin != "" && a.EarlyOutMin != "00:00" && a.EarlyOutMin != null))
                             select a).Count();
                if (LateCount == 0)
                {
                    var oEmpValidity = (from a in oDB.TrnsEmployeeNoLateAllowanceDetail where a.MstEmployee.EmpID == oEmp.EmpID && a.TrnsEmployeeNoLateAllowance.DocStatus == true select a).FirstOrDefault();
                    if (oEmpValidity == null) return;


                    elementid = Convert.ToInt32(oEmpValidity.TrnsEmployeeNoLateAllowance.CalculatedOn);
                    elementidPays = Convert.ToInt32(oEmpValidity.TrnsEmployeeNoLateAllowance.PaysThrough);

                    //Enter Full Amount with Detials
                    TrnsEmployeeElement oEmpEle = (from a in oDB.TrnsEmployeeElement where a.MstEmployee.EmpID == oEmp.EmpID select a).FirstOrDefault();
                    if (oEmpEle == null) return;
                    MstElements oElement = (from a in oDB.MstElements where a.Id == elementid select a).FirstOrDefault();
                    if (oElement == null) return;
                    MstElements oElementPaysThrough = (from a in oDB.MstElements where a.Id == elementidPays select a).FirstOrDefault();
                    if (oElementPaysThrough == null) return;

                    int chkDetail = 0;
                    chkDetail = (from a in oDB.TrnsEmployeeElementDetail where a.TrnsEmployeeElement.Id == oEmpEle.Id && a.PeriodId == oPeriod.ID && a.MstElements.Id == oElementPaysThrough.Id select a).Count();
                    TrnsEmployeeElementDetail oDoc = new TrnsEmployeeElementDetail();
                    #region Element Calculation
                    var oElementEar = (from a in oDB.MstElementEarning where a.ElementID == oElement.Id select a).FirstOrDefault();
                    if (oElementEar == null) return;
                    string Elementname = oElement.ElementName;
                    //
                    //CalculateElementValues(oElement.ElmtType, oEmp, oElementEar.ValueType, oDoc);
                    decimal outValue = 0M;
                    if (oElementEar.ValueType == "POB")
                    {
                        oDoc.Value = Convert.ToDecimal(oElementEar.Value);
                        outValue = Convert.ToDecimal(oElementEar.Value) / 100 * (decimal)oEmp.BasicSalary;
                        oDoc.Amount = outValue;
                    }
                    if (oElementEar.ValueType == "POG")
                    {

                        oDoc.Value = Convert.ToDecimal(oElementEar.Value);
                        outValue = Convert.ToDecimal(oElementEar.Value) / 100 * (decimal)getEmpGross(oEmp);
                        oDoc.Amount = outValue;
                    }
                    if (oElementEar.ValueType == "FIX")
                    {
                        oDoc.Value = Convert.ToDecimal(oElementEar.Value);
                        oDoc.Amount = Convert.ToDecimal(oElementEar.Value);
                    }
                    //
                    #endregion
                    if (chkDetail == 0)
                    {
                        oDoc = new TrnsEmployeeElementDetail();
                        oEmpEle.TrnsEmployeeElementDetail.Add(oDoc);
                    }
                    else
                    {
                        oDoc = (from a in oDB.TrnsEmployeeElementDetail where a.TrnsEmployeeElement.Id == oEmpEle.Id && a.PeriodId == oPeriod.ID && a.MstElements.Id == oElementPaysThrough.Id select a).FirstOrDefault();
                    }
                    oDoc.MstElements = oElementPaysThrough;
                    oDoc.StartDate = oElementPaysThrough.StartDate;
                    oDoc.EndDate = oElementPaysThrough.EndDate;
                    oDoc.FlgRetro = false;
                    oDoc.RetroAmount = 0M;
                    oDoc.FlgActive = true;
                    oDoc.FlgOneTimeConsumed = false;
                    oDoc.PeriodId = oPeriod.ID;
                    oDoc.ElementType = oElementPaysThrough.ElmtType;
                    oDoc.ValueType = oElementPaysThrough.MstElementEarning[0].ValueType;
                    oElementPaysThrough.MstElementEarning[0].Value = outValue;
                    oDoc.Value = oElementPaysThrough.MstElementEarning[0].Value;
                    //oDoc.Value = outValue;
                    oDoc.Amount = 0M;
                    oDoc.EmpContr = 0M;
                    oDoc.EmplrContr = 0M;

                    oDB.SubmitChanges();
                }

            }
            catch (Exception ex)
            {
                string mess = ex.Message;
                Console.WriteLine(mess);
            }
        }

        public decimal getLoanBalance(TrnsLoanDetail loanDet)
        {
            decimal loanBalance = 0.0M;

            decimal loanRecovered = 0.0M;
            string strSql = "SELECT     ISNULL(SUM(RecoveredAmount), 0) AS recoverdAmt  FROM TrnsLoanRegister where loanId = '" + loanDet.TrnsLoan.ID.ToString() + "'";
            DataTable dtLoanBal = getDataTable(strSql);
            try
            {
                loanRecovered = Convert.ToDecimal(dtLoanBal.Rows[0]["recoverdAmt"]);
            }
            catch { }

            loanBalance = Convert.ToDecimal(loanDet.ApprovedAmount) - loanRecovered;
            return loanBalance;
        }

        public void postLoanInstallmentIntoRegister(MstEmployee emp)
        {
            //  IEnumerable<TrnsLoanDetail > empLoans = from p in oDB.TrnsLoanDetail where p.TrnsLoan.EmpID == emp.ID && p.  select p;

        }

        public void addPeriodDates(DateTime fromdate, DateTime toDate, string periodName)
        {
            var payrolls = (from p in oDB.CfgPayrollDefination where (p.FlgIsDefault == null ? false : p.FlgIsDefault) == true select p).ToList();
            foreach (CfgPayrollDefination prl in payrolls)
            {
                CfgPeriodDates oLastPeriod = (from a in oDB.CfgPeriodDates where a.PayrollId == prl.ID orderby a.ID descending select a).FirstOrDefault();
                DateTime dtEndDate;
                if (oLastPeriod == null)
                {
                    dtEndDate = Convert.ToDateTime(prl.FirstPeriodEndDt);
                }
                else
                {
                    Int32 YearValue = 0;
                    YearValue = (fromdate.Year - Convert.ToDateTime(prl.FirstPeriodEndDt).Year);
                    dtEndDate = Convert.ToDateTime(prl.FirstPeriodEndDt).AddYears(YearValue);
                }

                string prltype = prl.PayrollType;
                int i = 0;
                Boolean flgHalfMonthlyTrigger = true;
                while (dtEndDate <= toDate && flgHalfMonthlyTrigger)
                {
                    i++;
                    CfgPeriodDates prdDate = new CfgPeriodDates();
                    prdDate.CreateDate = DateTime.Now;
                    prdDate.EndDate = dtEndDate;
                    prdDate.PeriodName = periodName + "-" + CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(dtEndDate.Month);
                    prdDate.PayrollId = prl.ID;
                    prdDate.UpdateDate = DateTime.Now;
                    prdDate.UserId = userId;
                    prdDate.UserId = userId;
                    prdDate.FlgLocked = false;
                    prdDate.CalCode = periodName;
                    switch (prltype.Trim())
                    {
                        case "MNTH":
                            if (dtEndDate.AddDays(1).Month != dtEndDate.Month)
                            {
                                prdDate.StartDate = new DateTime(dtEndDate.Year, dtEndDate.Month, 1);
                                dtEndDate = Convert.ToDateTime(prdDate.StartDate).AddMonths(2).AddDays(-1);
                            }
                            else
                            {
                                prdDate.StartDate = Convert.ToDateTime(prdDate.EndDate).AddDays(1).AddMonths(-1);
                                dtEndDate = dtEndDate.AddMonths(1);
                            }

                            break;
                        case "HMNT":
                        case "BWKS":
                            //if (dtEndDate.AddDays(1).Month != dtEndDate.Month)
                            if (dtEndDate.AddDays(-14).AddDays(29) >= toDate)
                            {
                                prdDate.StartDate = Convert.ToDateTime(dtEndDate).AddDays(-14);
                                prdDate.PeriodName += "-" + String.Format("{0:000}", dtEndDate.DayOfYear);
                                dtEndDate = toDate;
                                prdDate.EndDate = toDate;
                                flgHalfMonthlyTrigger = false;
                            }
                            else
                            {
                                prdDate.StartDate = Convert.ToDateTime(dtEndDate).AddDays(-14);
                                prdDate.PeriodName += "-" + String.Format("{0:000}", dtEndDate.DayOfYear);
                                dtEndDate = dtEndDate.AddDays(15);
                            }
                            break;

                        case "QRTR":
                            if (dtEndDate.AddDays(1).Month != dtEndDate.Month)
                            {
                                prdDate.StartDate = new DateTime(dtEndDate.Year, dtEndDate.AddMonths(-2).Month, 1);
                                dtEndDate = Convert.ToDateTime(prdDate.StartDate).AddMonths(6).AddDays(-1);
                            }
                            else
                            {
                                prdDate.StartDate = Convert.ToDateTime(prdDate.EndDate).AddDays(1).AddMonths(-3);
                                dtEndDate = dtEndDate.AddMonths(3);
                            }
                            break;
                        case "YEAR":
                            if (dtEndDate.AddDays(1).Month != dtEndDate.Month)
                            {
                                prdDate.StartDate = new DateTime(dtEndDate.Year, dtEndDate.AddMonths(-2).Month, 1);
                                dtEndDate = Convert.ToDateTime(prdDate.StartDate).AddMonths(6).AddDays(-1);
                            }
                            else
                            {
                                prdDate.StartDate = Convert.ToDateTime(prdDate.EndDate).AddDays(1).AddMonths(-3);
                                dtEndDate = dtEndDate.AddMonths(3);
                            }
                            break;
                    }

                    int cnt = (from p in oDB.CfgPeriodDates where p.PayrollId.ToString() == prl.ID.ToString() && p.StartDate == prdDate.StartDate && p.EndDate == prdDate.EndDate select p).Count();
                    if (cnt == 0)
                    {
                        oDB.CfgPeriodDates.InsertOnSubmit(prdDate);
                    }
                }
            }
            oDB.SubmitChanges();


        }

        public void AddPeriodDates(DateTime pFromDate, DateTime pToDate, string pCalendarCode)
        {
            try
            {
                var oPayrollCollection = (from a in oDB.CfgPayrollDefination
                                          where (a.FlgIsDefault == null ? false : a.FlgIsDefault) == true
                                          select a).ToList();
                foreach (var oPayroll in oPayrollCollection)
                {
                    var CheckPeriods = (from a in oDB.CfgPeriodDates
                                        where a.PayrollId == oPayroll.ID
                                        && a.CalCode == pCalendarCode
                                        select a).Count();
                    if (CheckPeriods == 0)
                    {
                        DateTime PeriodEndDate;
                        DateTime FirstPeriodEndDate = Convert.ToDateTime(oPayroll.FirstPeriodEndDt);
                        if (pFromDate < FirstPeriodEndDate)
                        {
                            PeriodEndDate = FirstPeriodEndDate;
                        }
                        else
                        {
                            if (FirstPeriodEndDate.Month == pFromDate.Month)
                            {
                                PeriodEndDate = new DateTime(pFromDate.Year, FirstPeriodEndDate.Month, FirstPeriodEndDate.Day);
                            }
                            else
                            {
                                PeriodEndDate = new DateTime(pFromDate.Year, pFromDate.Month, FirstPeriodEndDate.Day);
                            }
                        }
                        string PayrollType = oPayroll.PayrollType;
                        int i = 0;
                        int count = 0;
                        Boolean flgHalfMonthlyTrigger = true;
                        Boolean flgWeeklyTrigger = true;
                        while (PeriodEndDate <= pToDate && flgHalfMonthlyTrigger)
                        {
                            i++;
                            CfgPeriodDates oPeriod = new CfgPeriodDates();
                            oPeriod.CreateDate = DateTime.Now;
                            oPeriod.EndDate = PeriodEndDate;
                            oPeriod.PeriodName = pCalendarCode + "-" + CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(PeriodEndDate.Month);
                            oPeriod.PayrollId = oPayroll.ID;
                            oPeriod.UpdateDate = DateTime.Now;
                            oPeriod.UserId = userId;
                            oPeriod.UpdateBy = userId;
                            oPeriod.FlgLocked = false;
                            oPeriod.FlgPosted = false;
                            oPeriod.FlgVisible = true;
                            oPeriod.CalCode = pCalendarCode;
                            switch (PayrollType.Trim())
                            {
                                case "MNTH":
                                    if (PeriodEndDate.AddDays(1).Month != PeriodEndDate.Month)
                                    {
                                        oPeriod.StartDate = new DateTime(PeriodEndDate.Year, PeriodEndDate.Month, 1);
                                        PeriodEndDate = Convert.ToDateTime(oPeriod.StartDate).AddMonths(2).AddDays(-1);
                                    }
                                    else
                                    {
                                        oPeriod.StartDate = Convert.ToDateTime(oPeriod.EndDate).AddDays(1).AddMonths(-1);
                                        PeriodEndDate = PeriodEndDate.AddMonths(1);
                                    }

                                    break;
                                case "HMNT":
                                    int thatMonthDays = DateTime.DaysInMonth(PeriodEndDate.Year, PeriodEndDate.Month);
                                    int monthHalf = thatMonthDays / 2;
                                    int mFirstStart = 0, mFirstEnd = monthHalf - 1;
                                    int mMidStart = monthHalf, mMidEnd = thatMonthDays - 1;
                                    {
                                        count++;
                                        if (count == 1)
                                        {
                                            oPeriod.StartDate = Convert.ToDateTime(PeriodEndDate.AddDays(mFirstStart));
                                            oPeriod.PeriodName += "-" + String.Format("{0:000}", PeriodEndDate.DayOfYear) + "-" + count;
                                            oPeriod.EndDate = Convert.ToDateTime(PeriodEndDate.AddDays(mFirstEnd));
                                        }
                                        else
                                        {
                                            oPeriod.StartDate = Convert.ToDateTime(PeriodEndDate.AddDays(mMidStart));
                                            oPeriod.PeriodName += "-" + String.Format("{0:000}", PeriodEndDate.DayOfYear) + "-" + count;
                                            oPeriod.EndDate = Convert.ToDateTime(PeriodEndDate.AddDays(mMidEnd));
                                        }
                                        if (count == 2)
                                        {
                                            PeriodEndDate = PeriodEndDate.AddMonths(1);
                                            count = 0;
                                        }
                                    }
                                    break;
                                case "BWKS":
                                    //if (dtEndDate.AddDays(1).Month != dtEndDate.Month)
                                    if (PeriodEndDate.AddDays(-14).AddDays(29) >= pToDate)
                                    {
                                        oPeriod.StartDate = Convert.ToDateTime(PeriodEndDate).AddDays(-14);
                                        oPeriod.PeriodName += "-" + String.Format("{0:000}", PeriodEndDate.DayOfYear);
                                        PeriodEndDate = pToDate;
                                        oPeriod.EndDate = pToDate;
                                        flgHalfMonthlyTrigger = false;
                                    }
                                    else
                                    {
                                        oPeriod.StartDate = Convert.ToDateTime(PeriodEndDate).AddDays(-14);
                                        oPeriod.PeriodName += "-" + String.Format("{0:000}", PeriodEndDate.DayOfYear);
                                        PeriodEndDate = PeriodEndDate.AddDays(15);
                                    }
                                    break;
                                case "WKS":
                                    DateTime GetDate;
                                    if (PeriodEndDate.AddDays(1).Month != PeriodEndDate.Month && flgWeeklyTrigger == true)
                                    {
                                        GetDate = new DateTime(PeriodEndDate.Year, PeriodEndDate.Month, 1);

                                        oPeriod.StartDate = Convert.ToDateTime(GetDate).AddDays(0);
                                        //int weekOfMonth = (PeriodEndDate.Day + ((int)PeriodEndDate.DayOfWeek)) / 7;
                                        int weekOfMonth = (GetDate.Day + ((int)GetDate.DayOfWeek)) / 7;
                                        oPeriod.PeriodName += "-WK-" + Convert.ToString(weekOfMonth);
                                        PeriodEndDate = GetDate.AddDays(6);
                                        oPeriod.EndDate = PeriodEndDate;
                                        flgWeeklyTrigger = false;

                                    }
                                    else if (flgWeeklyTrigger == false)
                                    {

                                        oPeriod.StartDate = Convert.ToDateTime(PeriodEndDate).AddDays(1);

                                        int weekOfMonth = (PeriodEndDate.Day + ((int)PeriodEndDate.DayOfWeek)) / 7;
                                        oPeriod.PeriodName += "-WK-" + Convert.ToString(weekOfMonth);//String.Format("{0:000}", PeriodEndDate.DayOfYear);
                                        PeriodEndDate = PeriodEndDate.AddDays(7);
                                        oPeriod.EndDate = PeriodEndDate;

                                    }
                                    break;
                                case "QRTR":
                                    if (PeriodEndDate.AddDays(1).Month != PeriodEndDate.Month)
                                    {
                                        oPeriod.StartDate = new DateTime(PeriodEndDate.Year, PeriodEndDate.AddMonths(-2).Month, 1);
                                        PeriodEndDate = Convert.ToDateTime(oPeriod.StartDate).AddMonths(6).AddDays(-1);
                                    }
                                    else
                                    {
                                        oPeriod.StartDate = Convert.ToDateTime(oPeriod.EndDate).AddDays(1).AddMonths(-3);
                                        PeriodEndDate = PeriodEndDate.AddMonths(3);
                                    }
                                    break;
                                case "YEAR":
                                    if (PeriodEndDate.AddDays(1).Month != PeriodEndDate.Month)
                                    {
                                        oPeriod.StartDate = new DateTime(PeriodEndDate.Year, PeriodEndDate.AddMonths(-2).Month, 1);
                                        PeriodEndDate = Convert.ToDateTime(oPeriod.StartDate).AddMonths(6).AddDays(-1);
                                    }
                                    else
                                    {
                                        oPeriod.StartDate = Convert.ToDateTime(oPeriod.EndDate).AddDays(1).AddMonths(-3);
                                        PeriodEndDate = PeriodEndDate.AddMonths(3);
                                    }
                                    break;
                            }

                            int cnt = (from p in oDB.CfgPeriodDates where p.PayrollId.ToString() == oPayroll.ID.ToString() && p.StartDate == oPeriod.StartDate && p.EndDate == oPeriod.EndDate select p).Count();
                            if (cnt == 0)
                            {
                                oDB.CfgPeriodDates.InsertOnSubmit(oPeriod);
                            }
                        }
                        oDB.SubmitChanges();
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            catch (Exception ex)
            { }
        }

        public DataTable getHrmsEmp()
        {
            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("SboId");
            IEnumerable<MstEmployee> emps = from p in oDB.MstEmployee where p.SBOEmpCode != null && p.SBOEmpCode != "" select p;
            foreach (MstEmployee emp in emps)
            {
                dtOut.Rows.Add(emp.SBOEmpCode);
            }
            return dtOut;
        }

        public DataTable salaryProcessingLoans(MstEmployee emp, decimal RemainingSalaryBalance)
        {
            decimal amnt = 0.0M;
            int DaysCnt = 0;
            Hashtable elementGls = new Hashtable();

            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            if (RemainingSalaryBalance <= 0) return dtOut;

            IEnumerable<TrnsLoanDetail> emploans = from p in oDB.TrnsLoanDetail where p.ApprovedAmount > 0 && p.TrnsLoan.EmpID == emp.ID && (bool)p.FlgStopRecovery != true select p;
            foreach (TrnsLoanDetail recLoan in emploans)
            {
                if (RemainingSalaryBalance > 0)
                {
                    decimal InstallmentAmnt = (decimal)recLoan.Installments;
                    decimal loanBalance = (decimal)(recLoan.ApprovedAmount - recLoan.RecoveredAmount);  // getLoanBalance(recLoan);
                    if (loanBalance > InstallmentAmnt && RemainingSalaryBalance > InstallmentAmnt) amnt = InstallmentAmnt;
                    if (loanBalance > InstallmentAmnt && RemainingSalaryBalance <= InstallmentAmnt) amnt = RemainingSalaryBalance;
                    if (loanBalance <= InstallmentAmnt && RemainingSalaryBalance > loanBalance) amnt = loanBalance;
                    if (loanBalance <= InstallmentAmnt && RemainingSalaryBalance <= loanBalance) amnt = RemainingSalaryBalance;

                    RemainingSalaryBalance = RemainingSalaryBalance - amnt;

                    DataRow dr = dtOut.NewRow();
                    if (loanBalance > recLoan.Installments)
                    {
                        amnt = (decimal)recLoan.Installments;
                    }
                    else
                    {
                        amnt = loanBalance;
                    }
                    elementGls = getLoanGL(emp, recLoan.MstLoans);
                    if (amnt > 0)
                    {
                        TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                        dr["LineType"] = "Loan Recovery";
                        dr["LineSubType"] = "Loan Recovery";
                        dr["LineValue"] = -amnt;
                        dr["LineMemo"] = recLoan.MstLoans.Description;
                        dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                        dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                        dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                        dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();

                        dr["LineBaseEntry"] = recLoan.ID;
                        decimal baseValue = 0.0M;
                        decimal baseCalculatedOn = 0.0M;
                        baseCalculatedOn = (decimal)amnt;
                        baseValue = 100.00M;
                        dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                        dr["BaseValue"] = baseValue;
                        dr["BaseValueType"] = "FIX";
                        dtOut.Rows.Add(dr);


                    }
                }
            }
            return dtOut;
        }

        public DataTable salaryProcessingLoans(MstEmployee emp, decimal RemainingSalaryBalance, CfgPeriodDates payrollperiod)
        {
            decimal amnt = 0.0M;
            int DaysCnt = 0;
            string strApprovalStatus = "LV0006";
            Hashtable elementGls = new Hashtable();

            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("Indicator");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            if (RemainingSalaryBalance <= 0) return dtOut;

            IEnumerable<TrnsLoanDetail> emploans = from p in oDB.TrnsLoanDetail where p.ApprovedAmount > 0 && p.RequiredDate <= payrollperiod.EndDate && p.TrnsLoan.DocAprStatus == strApprovalStatus && p.TrnsLoan.EmpID == emp.ID && (bool)p.FlgStopRecovery != true select p;
            foreach (TrnsLoanDetail recLoan in emploans)
            {
                if (RemainingSalaryBalance > 0)
                {
                    decimal InstallmentAmnt = (decimal)recLoan.ApprovedInstallment;
                    //decimal loanBalance = (decimal)(recLoan.ApprovedAmount - recLoan.RecoveredAmount);  // getLoanBalance(recLoan);
                    decimal loanBalance = (decimal)(recLoan.ApprovedAmount - recLoan.RecoveredAmount.GetValueOrDefault());
                    if (loanBalance > InstallmentAmnt && RemainingSalaryBalance > InstallmentAmnt) amnt = InstallmentAmnt;
                    if (loanBalance > InstallmentAmnt && RemainingSalaryBalance <= InstallmentAmnt) amnt = RemainingSalaryBalance;
                    if (loanBalance <= InstallmentAmnt && RemainingSalaryBalance > loanBalance) amnt = loanBalance;
                    if (loanBalance <= InstallmentAmnt && RemainingSalaryBalance <= loanBalance) amnt = RemainingSalaryBalance;

                    RemainingSalaryBalance = RemainingSalaryBalance - amnt;

                    DataRow dr = dtOut.NewRow();
                    if (loanBalance > recLoan.ApprovedInstallment)
                    {
                        amnt = (decimal)recLoan.ApprovedInstallment;
                    }
                    else
                    {
                        amnt = loanBalance;
                    }
                    elementGls = getLoanGL(emp, recLoan.MstLoans);
                    if (amnt > 0)
                    {
                        TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                        dr["LineType"] = "Loan Recovery";
                        dr["LineSubType"] = "Loan Recovery";
                        dr["LineValue"] = -amnt;
                        dr["LineMemo"] = recLoan.MstLoans.Description;
                        dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                        dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                        dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                        dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();
                        dr["Indicator"] = elementGls["Indicator"].ToString();
                        dr["LineBaseEntry"] = recLoan.ID;
                        decimal baseValue = 0.0M;
                        decimal baseCalculatedOn = 0.0M;
                        baseCalculatedOn = (decimal)amnt;
                        baseValue = 100.00M;
                        dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                        dr["BaseValue"] = baseValue;
                        dr["BaseValueType"] = "FIX";
                        dtOut.Rows.Add(dr);


                    }
                }
            }
            return dtOut;
        }

        public DataTable salaryProcessingAdvance(MstEmployee emp, decimal RemainingSalaryBalance, CfgPeriodDates payrollperiod)
        {
            decimal amnt = 0.0M;
            int DaysCnt = 0;
            string strApprovalStatus = "LV0006";
            Hashtable elementGls = new Hashtable();

            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("Indicator");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            if (RemainingSalaryBalance <= 0) return dtOut;

            //IEnumerable<TrnsAdvance> emploans = from p in oDB.TrnsAdvance where p.ApprovedAmount > 0 && p.EmpID == emp.ID select p;
            IEnumerable<TrnsAdvance> emploans = from p in oDB.TrnsAdvance where p.ApprovedAmount > 0 && p.RequiredDate <= payrollperiod.EndDate && p.DocAprStatus == strApprovalStatus && (bool)p.FlgStop != true && p.EmpID == emp.ID select p;
            foreach (TrnsAdvance recAdvance in emploans)
            {
                if (RemainingSalaryBalance > 0)
                {
                    decimal InstallmentAmnt = (decimal)recAdvance.RemainingAmount;
                    decimal loanBalance = (decimal)recAdvance.RemainingAmount;
                    if (loanBalance > InstallmentAmnt && RemainingSalaryBalance > InstallmentAmnt) amnt = InstallmentAmnt;
                    if (loanBalance > InstallmentAmnt && RemainingSalaryBalance <= InstallmentAmnt) amnt = RemainingSalaryBalance;
                    if (loanBalance <= InstallmentAmnt && RemainingSalaryBalance > loanBalance) amnt = loanBalance;
                    if (loanBalance <= InstallmentAmnt && RemainingSalaryBalance <= loanBalance) amnt = RemainingSalaryBalance;
                    if (!string.IsNullOrEmpty(emp.EmployeeContractType) && emp.EmployeeContractType != "DWGS")
                    {
                        RemainingSalaryBalance = RemainingSalaryBalance - amnt;
                    }
                    else
                    {
                        amnt = (decimal)recAdvance.RemainingAmount;
                    }
                    MstAdvance adv = (from p in oDB.MstAdvance where p.Id.ToString() == recAdvance.AdvanceType.ToString() select p).Single();
                    DataRow dr = dtOut.NewRow();
                    elementGls = getAdvGL(emp, adv);
                    if (amnt > 0)
                    {
                        TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                        dr["LineType"] = "Advance Recovery";
                        dr["LineSubType"] = "Advance Recovery";
                        dr["LineValue"] = -amnt;
                        dr["LineMemo"] = adv.Description;
                        dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                        dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                        dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                        dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();
                        dr["Indicator"] = elementGls["Indicator"].ToString();
                        dr["LineBaseEntry"] = recAdvance.ID;
                        decimal baseValue = 0.0M;
                        decimal baseCalculatedOn = 0.0M;
                        baseCalculatedOn = (decimal)amnt;
                        baseValue = 100.00M;
                        dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                        dr["BaseValue"] = baseValue;
                        dr["BaseValueType"] = "FIX";
                        dtOut.Rows.Add(dr);


                    }
                }


            }

            return dtOut;
        }

        public DataTable salaryProcessingAdvance(MstEmployee emp, decimal RemainingSalaryBalance)
        {
            decimal amnt = 0.0M;
            int DaysCnt = 0;
            Hashtable elementGls = new Hashtable();

            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");
            if (RemainingSalaryBalance <= 0) return dtOut;


            IEnumerable<TrnsAdvance> emploans = from p in oDB.TrnsAdvance where p.ApprovedAmount > 0 && p.EmpID == emp.ID select p;
            foreach (TrnsAdvance recAdvance in emploans)
            {
                if (RemainingSalaryBalance > 0)
                {
                    decimal InstallmentAmnt = (decimal)recAdvance.RemainingAmount;
                    decimal loanBalance = (decimal)recAdvance.RemainingAmount;
                    if (loanBalance > InstallmentAmnt && RemainingSalaryBalance > InstallmentAmnt) amnt = InstallmentAmnt;
                    if (loanBalance > InstallmentAmnt && RemainingSalaryBalance <= InstallmentAmnt) amnt = RemainingSalaryBalance;
                    if (loanBalance <= InstallmentAmnt && RemainingSalaryBalance > loanBalance) amnt = loanBalance;
                    if (loanBalance <= InstallmentAmnt && RemainingSalaryBalance <= loanBalance) amnt = RemainingSalaryBalance;

                    RemainingSalaryBalance = RemainingSalaryBalance - amnt;
                    MstAdvance adv = (from p in oDB.MstAdvance where p.Id.ToString() == recAdvance.AdvanceType.ToString() select p).Single();
                    DataRow dr = dtOut.NewRow();
                    elementGls = getAdvGL(emp, adv);
                    if (amnt > 0)
                    {
                        TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                        dr["LineType"] = "Advance Recovery";
                        dr["LineSubType"] = "Advance Recovery";
                        dr["LineValue"] = -amnt;
                        dr["LineMemo"] = adv.Description;
                        dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                        dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                        dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                        dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();
                        dr["LineBaseEntry"] = recAdvance.ID;
                        decimal baseValue = 0.0M;
                        decimal baseCalculatedOn = 0.0M;
                        baseCalculatedOn = (decimal)amnt;
                        baseValue = 100.00M;
                        dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                        dr["BaseValue"] = baseValue;
                        dr["BaseValueType"] = "FIX";
                        dtOut.Rows.Add(dr);


                    }
                }


            }

            return dtOut;
        }

        public DataTable EOSAdvanceRecovery(MstEmployee emp)
        {
            decimal amnt = 0.0M;
            Hashtable elementGls = new Hashtable();

            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");

            IEnumerable<TrnsAdvance> oAdvanceCollection = from p in oDB.TrnsAdvance where p.ApprovedAmount > 0 && p.EmpID == emp.ID select p;
            foreach (TrnsAdvance recAdvance in oAdvanceCollection)
            {
                //decimal InstallmentAmnt = (decimal)recAdvance.RemainingAmount;
                decimal AmmountTobeRecovered = (decimal)recAdvance.RemainingAmount;
                //if (AmmountTobeRecovered > InstallmentAmnt && RemainingSalaryBalance > InstallmentAmnt) amnt = InstallmentAmnt;
                //if (AmmountTobeRecovered > InstallmentAmnt && RemainingSalaryBalance <= InstallmentAmnt) amnt = RemainingSalaryBalance;
                //if (AmmountTobeRecovered <= InstallmentAmnt && RemainingSalaryBalance > AmmountTobeRecovered) amnt = AmmountTobeRecovered;
                //if (AmmountTobeRecovered <= InstallmentAmnt && RemainingSalaryBalance <= AmmountTobeRecovered) amnt = RemainingSalaryBalance;
                amnt = AmmountTobeRecovered;
                //RemainingSalaryBalance = RemainingSalaryBalance - amnt;
                MstAdvance adv = (from p in oDB.MstAdvance where p.Id.ToString() == recAdvance.AdvanceType.ToString() select p).Single();
                DataRow dr = dtOut.NewRow();
                elementGls = getAdvGL(emp, adv);
                if (amnt > 0)
                {
                    //TrnsSalaryProcessRegisterDetail spdetail = new TrnsSalaryProcessRegisterDetail();
                    dr["LineType"] = "FSAdvanceRecovery";
                    dr["LineSubType"] = "AdvanceRecovery";
                    dr["LineValue"] = -amnt;
                    dr["LineMemo"] = adv.Description + " # " + recAdvance.DocNum.ToString();
                    dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                    dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                    dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                    dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();
                    dr["LineBaseEntry"] = recAdvance.ID;
                    decimal baseValue = 0.0M;
                    decimal baseCalculatedOn = 0.0M;
                    baseCalculatedOn = (decimal)amnt;
                    baseValue = 100.00M;
                    dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                    dr["BaseValue"] = baseValue;
                    dr["BaseValueType"] = "FIX";
                    dtOut.Rows.Add(dr);
                }
            }

            return dtOut;
        }

        public DataTable EOSLoanRecovery(MstEmployee emp)
        {

            decimal amnt = 0.0M;
            Hashtable elementGls = new Hashtable();

            DataTable dtOut = new DataTable();
            dtOut.Columns.Add("LineType");
            dtOut.Columns.Add("LineSubType");
            dtOut.Columns.Add("LineValue");
            dtOut.Columns.Add("LineMemo");
            dtOut.Columns.Add("DebitAccount");
            dtOut.Columns.Add("CreditAccount");
            dtOut.Columns.Add("DebitAccountName");
            dtOut.Columns.Add("CreditAccountName");
            dtOut.Columns.Add("LineBaseEntry");
            dtOut.Columns.Add("BaseValueCalculatedOn");
            dtOut.Columns.Add("BaseValue");
            dtOut.Columns.Add("BaseValueType");

            IEnumerable<TrnsLoanDetail> emploans = from p in oDB.TrnsLoanDetail
                                                   where p.ApprovedAmount > 0
                                                   && p.TrnsLoan.EmpID == emp.ID
                                                   && (bool)p.FlgStopRecovery != true
                                                   && (bool)p.FlgActive == true
                                                   && (bool)p.FlgVoid != true
                                                   select p;
            foreach (TrnsLoanDetail recLoan in emploans)
            {

                decimal InstallmentAmnt = (decimal)recLoan.Installments;
                decimal loanBalance = (decimal)(recLoan.ApprovedAmount - recLoan.RecoveredAmount);  // getLoanBalance(recLoan);
                //if (loanBalance > InstallmentAmnt && RemainingSalaryBalance > InstallmentAmnt) amnt = InstallmentAmnt;
                //if (loanBalance > InstallmentAmnt && RemainingSalaryBalance <= InstallmentAmnt) amnt = RemainingSalaryBalance;
                //if (loanBalance <= InstallmentAmnt && RemainingSalaryBalance > loanBalance) amnt = loanBalance;
                //if (loanBalance <= InstallmentAmnt && RemainingSalaryBalance <= loanBalance) amnt = RemainingSalaryBalance;

                //RemainingSalaryBalance = RemainingSalaryBalance - amnt;

                DataRow dr = dtOut.NewRow();
                //amnt = (decimal)recLoan.Installments;
                amnt = loanBalance;
                elementGls = getLoanGL(emp, recLoan.MstLoans);
                if (amnt > 0)
                {
                    dr["LineType"] = "FSLoanRecovery";
                    dr["LineSubType"] = "Loan Recovery";
                    dr["LineValue"] = -amnt;
                    dr["LineMemo"] = recLoan.MstLoans.Description + " # " + recLoan.TrnsLoan.DocNum.ToString();
                    dr["DebitAccount"] = elementGls["DrAcct"].ToString();
                    dr["CreditAccount"] = elementGls["CrAcct"].ToString();
                    dr["DebitAccountName"] = elementGls["DrAcctName"].ToString();
                    dr["CreditAccountName"] = elementGls["CrAcctName"].ToString();

                    dr["LineBaseEntry"] = recLoan.ID;
                    decimal baseValue = 0.0M;
                    decimal baseCalculatedOn = 0.0M;
                    baseCalculatedOn = (decimal)amnt;
                    baseValue = 100.00M;
                    dr["BaseValueCalculatedOn"] = baseCalculatedOn;
                    dr["BaseValue"] = baseValue;
                    dr["BaseValueType"] = "FIX";
                    dtOut.Rows.Add(dr);
                }
            }
            return dtOut;
        }

        public void updateStandardElements(MstEmployee instance, bool updateExisitng)
        {
            //throw new NotImplementedException();
            if (instance.PayrollID != null)
            {
                int i = 0;
                TrnsEmployeeElement empEle;
                int cnt = (from p in oDB.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == instance.EmpID select p).Count();
                if (cnt == 0)
                {
                    empEle = new TrnsEmployeeElement();
                    empEle.CreateDate = DateTime.Now;
                    empEle.UserId = instance.CreatedBy;
                    empEle.MstEmployee = instance;
                    empEle.FlgActive = true; //instance.FlgActive;
                    oDB.TrnsEmployeeElement.InsertOnSubmit(empEle);
                }
                else
                {
                    empEle = (from p in oDB.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == instance.EmpID select p).FirstOrDefault();
                }
                foreach (MstElementLink linkedElement in instance.CfgPayrollDefination.MstElementLink)
                {
                    if ((bool)linkedElement.MstElements.FlgStandardElement)
                    {
                        int linkedCnt = (from p in oDB.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == instance.ID && p.ElementId == linkedElement.ElementID select p).Count();
                        if (linkedCnt == 0)
                        {
                            TrnsEmployeeElementDetail trntEle = new TrnsEmployeeElementDetail();


                            trntEle.MstElements = linkedElement.MstElements;
                            trntEle.RetroAmount = Convert.ToDecimal(0.00);
                            trntEle.FlgRetro = false;
                            trntEle.StartDate = linkedElement.MstElements.StartDate;
                            trntEle.EndDate = linkedElement.MstElements.EndDate;
                            trntEle.ElementType = linkedElement.MstElements.ElmtType;
                            Custom.clsElement elemDetail = new Custom.clsElement(oDB, linkedElement.MstElements, instance);
                            trntEle.ValueType = elemDetail.ValueType;
                            trntEle.Value = elemDetail.Value;
                            trntEle.Amount = elemDetail.Amount;
                            trntEle.FlgActive = linkedElement.FlgActive;
                            empEle.TrnsEmployeeElementDetail.Add(trntEle);
                        }
                        else
                        {
                            if (updateExisitng)
                            {
                                TrnsEmployeeElementDetail trntEle = (from p in oDB.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == instance.ID && p.ElementId == linkedElement.ElementID select p).FirstOrDefault();
                                trntEle.MstElements = linkedElement.MstElements;
                                trntEle.RetroAmount = Convert.ToDecimal(0.00);
                                trntEle.FlgRetro = false;
                                trntEle.StartDate = linkedElement.MstElements.StartDate;
                                trntEle.EndDate = linkedElement.MstElements.EndDate;
                                trntEle.ElementType = linkedElement.MstElements.ElmtType;
                                //trntEle.FlgActive = instance.FlgActive; //linkedElement.FlgActive;
                                trntEle.FlgActive = linkedElement.FlgActive;
                                Custom.clsElement elemDetail = new Custom.clsElement(oDB, linkedElement.MstElements, instance);
                                if (elemDetail.Value > 0)
                                {
                                    trntEle.ValueType = elemDetail.ValueType;
                                    trntEle.Value = elemDetail.Value;
                                    trntEle.Amount = elemDetail.Amount;
                                    trntEle.EmplrContr = elemDetail.emprAmount;
                                }
                                trntEle.FlgActive = linkedElement.FlgActive;

                            }
                            else
                            {
                                TrnsEmployeeElementDetail trntEle = (from p in oDB.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == instance.ID && p.ElementId == linkedElement.ElementID select p).FirstOrDefault();
                                trntEle.MstElements = linkedElement.MstElements;
                                trntEle.FlgActive = linkedElement.FlgActive;
                            }
                        }


                    }
                }
                oDB.SubmitChanges();
            }

        }//end of function

        public void updateSelectedElements(MstEmployee instance, bool updateExisitng)
        {
            //throw new NotImplementedException();
            if (instance.PayrollID != null)
            {
                int i = 0;
                TrnsEmployeeElement empEle;
                int cnt = (from p in oDB.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == instance.EmpID select p).Count();
                if (cnt == 0)
                {
                    empEle = new TrnsEmployeeElement();
                    empEle.CreateDate = DateTime.Now;
                    empEle.UserId = instance.CreatedBy;
                    empEle.MstEmployee = instance;
                    empEle.FlgActive = true; //instance.FlgActive;
                    oDB.TrnsEmployeeElement.InsertOnSubmit(empEle);
                }
                else
                {
                    empEle = (from p in oDB.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == instance.EmpID select p).FirstOrDefault();
                }
                foreach (MstElementLink linkedElement in instance.CfgPayrollDefination.MstElementLink)
                {

                    int linkedCnt = (from p in oDB.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == instance.ID && p.ElementId == linkedElement.ElementID select p).Count();
                    if (linkedCnt == 0)
                    {
                        TrnsEmployeeElementDetail trntEle = new TrnsEmployeeElementDetail();


                        trntEle.MstElements = linkedElement.MstElements;
                        trntEle.RetroAmount = Convert.ToDecimal(0.00);
                        trntEle.FlgRetro = false;
                        trntEle.StartDate = linkedElement.MstElements.StartDate;
                        trntEle.EndDate = linkedElement.MstElements.EndDate;
                        trntEle.ElementType = linkedElement.MstElements.ElmtType;
                        Custom.clsElement elemDetail = new Custom.clsElement(oDB, linkedElement.MstElements);
                        trntEle.ValueType = elemDetail.ValueType;
                        trntEle.Value = elemDetail.Value;
                        trntEle.Amount = elemDetail.Amount;
                        trntEle.FlgActive = linkedElement.FlgActive;
                        empEle.TrnsEmployeeElementDetail.Add(trntEle);
                    }
                    else
                    {
                        if (updateExisitng)
                        {
                            TrnsEmployeeElementDetail trntEle = (from p in oDB.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == instance.ID && p.ElementId == linkedElement.ElementID select p).FirstOrDefault();
                            trntEle.MstElements = linkedElement.MstElements;
                            trntEle.RetroAmount = Convert.ToDecimal(0.00);
                            trntEle.FlgRetro = false;
                            trntEle.StartDate = linkedElement.MstElements.StartDate;
                            trntEle.EndDate = linkedElement.MstElements.EndDate;
                            trntEle.ElementType = linkedElement.MstElements.ElmtType;
                            //trntEle.FlgActive = instance.FlgActive; //linkedElement.FlgActive;
                            trntEle.FlgActive = linkedElement.FlgActive;
                            Custom.clsElement elemDetail = new Custom.clsElement(oDB, linkedElement.MstElements, instance);
                            if (elemDetail.Value > 0)
                            {
                                trntEle.ValueType = elemDetail.ValueType;
                                trntEle.Value = elemDetail.Value;
                                trntEle.Amount = elemDetail.Amount;

                            }
                            trntEle.FlgActive = linkedElement.FlgActive;

                        }
                        else
                        {
                            TrnsEmployeeElementDetail trntEle = (from p in oDB.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == instance.ID && p.ElementId == linkedElement.ElementID select p).FirstOrDefault();
                            trntEle.MstElements = linkedElement.MstElements;
                            trntEle.FlgActive = linkedElement.FlgActive;
                        }
                    }

                }
                oDB.SubmitChanges();
            }

        }//end of function

        public void updateStandardElements(string empid, bool updateExisitng, string conStr)
        {
            //throw new NotImplementedException();
            try
            {
                using (dbHRMS oDB1 = new dbHRMS(conStr))
                {
                    MstEmployee instance = (from a in oDB1.MstEmployee where a.EmpID == empid select a).FirstOrDefault();

                    if (instance.PayrollID != null)
                    {
                        int i = 0;
                        TrnsEmployeeElement empEle;
                        int cnt = (from p in oDB1.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == instance.EmpID select p).Count();
                        if (cnt == 0)
                        {
                            empEle = new TrnsEmployeeElement();
                            empEle.CreateDate = DateTime.Now;
                            empEle.UserId = instance.CreatedBy;
                            empEle.MstEmployee = instance;
                            empEle.FlgActive = true; //instance.FlgActive;
                            oDB1.TrnsEmployeeElement.InsertOnSubmit(empEle);
                        }
                        else
                        {
                            empEle = (from p in oDB1.TrnsEmployeeElement
                                      where p.MstEmployee.EmpID.ToString() == instance.EmpID
                                      select p).FirstOrDefault();
                        }
                        foreach (MstElementLink linkedElement in instance.CfgPayrollDefination.MstElementLink)
                        {
                            if ((bool)linkedElement.MstElements.FlgStandardElement)
                            {

                                int linkedCnt = (from p in oDB1.TrnsEmployeeElementDetail
                                                 where p.TrnsEmployeeElement.EmployeeId == instance.ID
                                                 && p.ElementId == linkedElement.ElementID
                                                 select p).Count();

                                if (linkedCnt == 0)
                                {
                                    TrnsEmployeeElementDetail trntEle = new TrnsEmployeeElementDetail();


                                    trntEle.MstElements = linkedElement.MstElements;
                                    trntEle.RetroAmount = Convert.ToDecimal(0.00);
                                    trntEle.FlgRetro = false;
                                    trntEle.StartDate = linkedElement.MstElements.StartDate;
                                    trntEle.EndDate = linkedElement.MstElements.EndDate;
                                    trntEle.ElementType = linkedElement.MstElements.ElmtType;
                                    Custom.clsElement elemDetail = new Custom.clsElement(oDB1, linkedElement.MstElements);
                                    trntEle.ValueType = elemDetail.ValueType;
                                    trntEle.Value = elemDetail.Value;
                                    trntEle.Amount = elemDetail.Amount;
                                    trntEle.FlgActive = linkedElement.FlgActive;
                                    empEle.TrnsEmployeeElementDetail.Add(trntEle);
                                }
                                else
                                {
                                    if (updateExisitng)
                                    {
                                        TrnsEmployeeElementDetail trntEle = (from p in oDB1.TrnsEmployeeElementDetail
                                                                             where p.TrnsEmployeeElement.EmployeeId == instance.ID
                                                                             && p.ElementId == linkedElement.ElementID
                                                                              && p.FlgActive == true
                                                                             select p).FirstOrDefault();

                                        trntEle.MstElements = linkedElement.MstElements;
                                        trntEle.RetroAmount = Convert.ToDecimal(0.00);
                                        trntEle.FlgRetro = false;
                                        trntEle.StartDate = linkedElement.MstElements.StartDate;
                                        trntEle.EndDate = linkedElement.MstElements.EndDate;
                                        trntEle.ElementType = linkedElement.MstElements.ElmtType;
                                        trntEle.FlgActive = instance.FlgActive; //linkedElement.FlgActive;

                                        Custom.clsElement elemDetail = new Custom.clsElement(oDB1, linkedElement.MstElements, instance);
                                        if (elemDetail.Value > 0)
                                        {
                                            trntEle.ValueType = elemDetail.ValueType;
                                            trntEle.Value = elemDetail.Value;
                                            trntEle.Amount = elemDetail.Amount;
                                            //trntEle.EmplrContr = elemDetail.emprAmount;
                                        }
                                        trntEle.FlgActive = linkedElement.FlgActive;
                                    }
                                    else
                                    {
                                        TrnsEmployeeElementDetail trntEle = (from p in oDB1.TrnsEmployeeElementDetail
                                                                             where p.TrnsEmployeeElement.EmployeeId == instance.ID
                                                                             && p.ElementId == linkedElement.ElementID
                                                                             && p.FlgActive == true
                                                                             select p).FirstOrDefault();

                                        trntEle.MstElements = linkedElement.MstElements;
                                        trntEle.FlgActive = linkedElement.FlgActive;
                                    }
                                }
                            }
                        }
                        oDB1.SubmitChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                string mess = ex.Message;
                Console.WriteLine(mess);
            }

        }//end of function

        public void updateStandardElements(MstEmployee instance, bool updateExisitng, bool flgactive)
        {
            //throw new NotImplementedException();
            if (instance.PayrollID != null)
            {
                int i = 0;
                TrnsEmployeeElement empEle;
                int cnt = (from p in oDB.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == instance.EmpID select p).Count();
                if (cnt == 0)
                {
                    empEle = new TrnsEmployeeElement();
                    empEle.CreateDate = DateTime.Now;
                    empEle.UserId = instance.CreatedBy;
                    empEle.MstEmployee = instance;
                    empEle.FlgActive = true;
                    oDB.TrnsEmployeeElement.InsertOnSubmit(empEle);
                }
                else
                {
                    empEle = (from p in oDB.TrnsEmployeeElement where p.MstEmployee.EmpID.ToString() == instance.EmpID select p).Single();
                }
                foreach (MstElementLink linkedElement in instance.CfgPayrollDefination.MstElementLink)
                {
                    if ((bool)linkedElement.MstElements.FlgStandardElement)
                    {
                        int linkedCnt = (from p in oDB.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == instance.ID && p.ElementId == linkedElement.ElementID select p).Count();
                        if (linkedCnt == 0)
                        {
                            TrnsEmployeeElementDetail trntEle = new TrnsEmployeeElementDetail();


                            trntEle.MstElements = linkedElement.MstElements;
                            trntEle.RetroAmount = Convert.ToDecimal(0.00);
                            trntEle.FlgRetro = false;
                            trntEle.StartDate = linkedElement.MstElements.StartDate;
                            trntEle.EndDate = linkedElement.MstElements.EndDate;
                            trntEle.ElementType = linkedElement.MstElements.ElmtType;
                            Custom.clsElement elemDetail = new Custom.clsElement(oDB, linkedElement.MstElements);
                            trntEle.ValueType = elemDetail.ValueType;
                            trntEle.Value = elemDetail.Value;
                            trntEle.Amount = elemDetail.Amount;
                            trntEle.FlgActive = linkedElement.FlgActive;
                            empEle.TrnsEmployeeElementDetail.Add(trntEle);
                        }
                        else
                        {
                            if (updateExisitng)
                            {
                                TrnsEmployeeElementDetail trntEle = (from p in oDB.TrnsEmployeeElementDetail where p.TrnsEmployeeElement.EmployeeId == instance.ID && p.ElementId == linkedElement.ElementID select p).Single();
                                trntEle.MstElements = linkedElement.MstElements;
                                trntEle.RetroAmount = Convert.ToDecimal(0.00);
                                trntEle.FlgRetro = false;
                                trntEle.StartDate = linkedElement.MstElements.StartDate;
                                trntEle.EndDate = linkedElement.MstElements.EndDate;
                                trntEle.ElementType = linkedElement.MstElements.ElmtType;
                                trntEle.FlgActive = linkedElement.FlgActive;

                                Custom.clsElement elemDetail = new Custom.clsElement(oDB, linkedElement.MstElements);
                                if (elemDetail.Value > 0)
                                {
                                    trntEle.ValueType = elemDetail.ValueType;
                                    trntEle.Value = elemDetail.Value;
                                    trntEle.Amount = elemDetail.Amount;
                                }
                                // trntEle.FlgActive = linkedElement.FlgActive;

                            }
                        }


                    }
                }
                oDB.SubmitChanges();
            }

        }

        public bool isPeriodLocked(int periodId)
        {
            bool outResult = false;
            int cnt = (from p in oDB.CfgPeriodDates where p.ID == periodId && p.FlgLocked == true select p).Count();

            if (cnt > 0) outResult = true;

            return outResult;


        }

        public System.Data.DataTable getValidEmpElement(string empid, string periodId)
        {
            String strSql = "";
            //string strSql = "SELECT    lnk.ID, ele.ElementName, ele.Description, ele.Type , elear.flgMultipleEntryAllowed AS 'Allowed Multiple' ";
            //strSql += "  FROM         " + hrmsDbname + ".dbo.MstEmployee emp INNER JOIN ";
            //strSql += "                   " + hrmsDbname + ".dbo.CfgPayrollDefination prl ON emp.PayrollID = prl.ID INNER JOIN ";
            //strSql += "                     " + hrmsDbname + ".dbo.MstElementLink lnk ON prl.ID = lnk.PayrollID INNER JOIN";
            //strSql += "                    " + hrmsDbname + ".dbo.MstElements ele ON lnk.ElementID = ele.Id LEFT OUTER JOIN ";
            //strSql += "                    " + hrmsDbname + ".dbo.MstElementEarning elear ON ele.Id = elear.ElementID  ";
            //strSql += " WHERE     (lnk.flgActive = 1) AND (emp.EmpID = '" + empid + "')";
            strSql = @"
                    SELECT     
	                    dbo.MstElementLink.ID, 
                        dbo.MstElements.ElementName, 
                        dbo.MstElements.Description,  
                        dbo.MstElements.Type, 
	                    CASE WHEN ElmtType = 'Ded' THEN dbo.MstElementDeduction.flgMultipleEntryAllowed 
	                    WHEN ElmtType = 'Ear' THEN	dbo.MstElementEarning.flgMultipleEntryAllowed ELSE 0 END AS 'Allowed Multiple'
                    FROM      
	                    dbo.CfgPayrollDefination INNER JOIN
	                    dbo.MstEmployee ON dbo.CfgPayrollDefination.ID = dbo.MstEmployee.PayrollID INNER JOIN
	                    dbo.MstElementLink ON dbo.CfgPayrollDefination.ID = dbo.MstElementLink.PayrollID INNER JOIN
	                    dbo.MstElements ON dbo.MstElementLink.ElementID = dbo.MstElements.Id LEFT OUTER JOIN
	                    dbo.MstElementEarning ON dbo.MstElements.Id = dbo.MstElementEarning.ElementID LEFT OUTER JOIN
	                    dbo.MstElementDeduction ON dbo.MstElements.Id = dbo.MstElementDeduction.ElementID
                    WHERE 
	                    dbo.MstElementLink.flgActive = 1
	                    AND dbo.MstEmployee.EmpID = '" + empid + @"'
                     ";
            System.Data.DataTable dtValidElement = getDataTable(strSql);
            return dtValidElement;
        }

        public System.Data.DataTable getValidPrlElement(string payrollId, string periodId)
        {
            //string strSql = "SELECT    lnk.ID, ele.ElementName, ele.Description, ele.Type , elear.flgMultipleEntryAllowed AS 'Allowed Multiple' ";
            //strSql += "  FROM         " + hrmsDbname + ".dbo.CfgPayrollDefination prl  INNER JOIN ";
            //strSql += "                     " + hrmsDbname + ".dbo.MstElementLink lnk ON prl.ID = lnk.PayrollID INNER JOIN";
            //strSql += "                    " + hrmsDbname + ".dbo.MstElements ele ON lnk.ElementID = ele.Id LEFT OUTER JOIN ";
            //strSql += "                    " + hrmsDbname + ".dbo.MstElementEarning elear ON ele.Id = elear.ElementID  ";
            //strSql += " WHERE     (lnk.flgActive = 1) AND (prl.ID = '" + payrollId + "')";

            string strSql = @"
                        SELECT A3.Id AS ID, A3.ElementName, A3.[Description], A3.[Type] , A3.ElmtType
                        FROM dbo.CfgPayrollDefination A1 INNER JOIN dbo.MstElementLink A2 ON A1.ID = A2.PayrollID
	                        INNER JOIN dbo.MstElements A3 ON A2.ElementID = A3.Id
                        WHERE A1.ID = " + payrollId + " AND ISNULL(A2.flgActive,0) = 1";
            System.Data.DataTable dtValidElement = getDataTable(strSql);
            return dtValidElement;
        }

        public DateTime GetLastPeriodEndDate(MstEmployee oEmp, DateTime pDate)
        {
            DateTime retValue = DateTime.Now;
            try
            {
                var oCalendar = (from a in oDB.MstCalendar where a.FlgActive == true select a).FirstOrDefault();

                var oCurrentPeriod = (from a in oDB.CfgPeriodDates
                                      where a.PayrollId == oEmp.PayrollID
                                      && a.CalCode == oCalendar.Code
                                      && a.StartDate <= pDate && a.EndDate >= pDate
                                      select a).FirstOrDefault();
                var oPeriodCollection = (from a in oDB.CfgPeriodDates
                                         where a.PayrollId == oEmp.PayrollID
                                         && a.CalCode == oCalendar.Code
                                         orderby a.StartDate ascending
                                         select a).ToList();
                int i = 0;
                foreach (var Line in oPeriodCollection)
                {
                    if (Line.ID == oCurrentPeriod.ID)
                    {
                        if (i != 0)
                        {
                            retValue = Convert.ToDateTime(oPeriodCollection[i - 1].EndDate);
                        }
                        else
                        {
                            var oCalendarCollection = (from a in oDB.MstCalendar select a).ToList();
                            int j = 0;
                            MstCalendar oPreviosCal = null;
                            foreach (var CalLine in oCalendarCollection)
                            {
                                if (CalLine.Id == oCalendar.Id)
                                {
                                    if (j != 0)
                                    {
                                        oPreviosCal = oCalendarCollection[j - 1];
                                    }
                                    else
                                    {
                                        oPreviosCal = oCalendarCollection[j];
                                    }
                                }
                                j++;
                            }
                            var oPeriodsCollectionPrevios = (from a in oDB.CfgPeriodDates
                                                             where a.PayrollId == oEmp.PayrollID
                                                             && a.CalCode == oPreviosCal.Code
                                                             orderby a.EndDate descending
                                                             select a).FirstOrDefault();
                            if (oPeriodsCollectionPrevios != null)
                                retValue = Convert.ToDateTime(oPeriodsCollectionPrevios.EndDate);
                        }
                    }
                    i++;
                }

            }
            catch (Exception ex)
            {

            }
            return retValue;
        }

        public decimal GetAdvanceRecoveredAmount(int AdvanceID, int EmpID, int PeriodID)
        {
            decimal retValue = 0;
            try
            {
                string QueryAdvance = @"
                                        SELECT SUM(ISNULL(A2.LineValue,0))
                                        FROM 
                                            dbo.TrnsSalaryProcessRegister A1 INNER JOIN dbo.TrnsSalaryProcessRegisterDetail A2 ON A2.SRID = A1.Id
                                            LEFT OUTER JOIN dbo.trnsJE A3 ON A1.JENum = A3.ID
                                        WHERE 
                                            A2.LineType = 'Advance Recovery' 
                                            AND A1.EmpID = " + EmpID + @" 
                                            AND A2.LineBaseEntry = " + AdvanceID + @" 
                                            AND A1.PayrollPeriodID <= " + PeriodID + @"
                                            AND ISNULL(A3.SBOJeNum,0) > 0
                                        ";
                retValue = Convert.ToDecimal(ExecuteQueries(QueryAdvance));
            }
            catch (Exception ex)
            {
                retValue = 0;
            }
            return retValue;
        }

        public decimal GetLoanRecoveredAmount(int LoanID, int EmpID, int PeriodID)
        {
            decimal retValue = 0;
            try
            {
                string QueryLoan = @"
                                    SELECT SUM(ISNULL(ABS(A2.LineValue),0))
                                    FROM 
                                        dbo.TrnsSalaryProcessRegister A1 INNER JOIN dbo.TrnsSalaryProcessRegisterDetail A2 ON A2.SRID = A1.Id
                                        LEFT OUTER JOIN dbo.trnsJE A3 ON A1.JENum = A3.ID
                                    WHERE 
                                        A2.LineType = 'Loan Recovery' 
                                        AND A1.EmpID = " + EmpID + @" 
                                        AND A2.LineBaseEntry = " + LoanID + @" 
                                        AND A1.PayrollPeriodID <= " + PeriodID + @"
                                        AND ISNULL(A3.SBOJeNum,0) > 0
                                    ";
                retValue = Convert.ToDecimal(ExecuteQueries(QueryLoan));
            }
            catch (Exception ex)
            {
                retValue = 0;
            }
            return retValue;
        }

        public MstGLDetermination getLocationGL(int LocationID)
        {
            MstGLDetermination detr = null;
            string GlType = "LOC";
            try
            {

                if (GlType == "LOC")
                {
                    detr = (from p in oDB.MstGLDetermination where p.GLType == "LOC" && p.GLValue == LocationID select p).FirstOrDefault();
                }
                else
                {
                    detr = (from p in oDB.MstGLDetermination where p.GLType == "COMP" select p).FirstOrDefault();
                }

            }
            catch (Exception ex)
            {

            }
            return detr;
        }

        public Hashtable getLoanGLClassified(MstEmployee emp, MstLoans loan, int LocationID)
        {
            MstGLDetermination glDetr = getLocationGL(LocationID);
            Hashtable gls = new Hashtable();
            int GlId = glDetr.Id;
            int cntGl = 0;

            cntGl = (from p in oDB.MstGLDLoansDetails where p.GLDId.ToString() == GlId.ToString() && p.LoanId.ToString() == loan.Id.ToString() select p).Count();
            if (cntGl > 0)
            {
                MstGLDLoansDetails glloan = (from p in oDB.MstGLDLoansDetails where p.GLDId.ToString() == GlId.ToString() && p.LoanId.ToString() == loan.Id.ToString() select p).FirstOrDefault();
                gls.Add("DrAcct", glloan.CostAccount);
                gls.Add("CrAcct", glloan.BalancingAccount);
                gls.Add("DrAcctName", glloan.CostAcctDisplay);
                gls.Add("CrAcctName", glloan.BalancingAcctDisplay);
                gls.Add("Indicator", string.IsNullOrEmpty(glloan.A1Indicator) == true ? "Not Found" : glloan.A1Indicator);
            }
            else
            {
                gls.Add("DrAcct", "Not Found");
                gls.Add("CrAcct", "Not Found");
                gls.Add("DrAcctName", "Not Found");
                gls.Add("CrAcctName", "Not Found");
                gls.Add("Indicator", "Not Found");
            }

            return gls;

        }

        public Hashtable getOverTimeGLClassified(MstEmployee emp, MstOverTime overtim, int LocationID)
        {
            MstGLDetermination glDetr = getLocationGL(LocationID);
            Hashtable gls = new Hashtable();
            int GlId = glDetr.Id;
            int cntGl = 0;

            cntGl = (from p in oDB.MstGLDOverTimeDetail where p.GLDId.ToString() == GlId.ToString() && p.OvertimeId.ToString() == overtim.ID.ToString() select p).Count();

            if (cntGl > 0)
            {
                MstGLDOverTimeDetail glOt = (from p in oDB.MstGLDOverTimeDetail where p.GLDId.ToString() == GlId.ToString() && p.OvertimeId.ToString() == overtim.ID.ToString() select p).Single();
                gls.Add("DrAcct", glOt.CostAccount);
                gls.Add("CrAcct", glOt.BalancingAccount);
                gls.Add("DrAcctName", glOt.CostAcctDisplay);
                gls.Add("CrAcctName", glOt.BalancingAcctDisplay);
            }
            else
            {
                gls.Add("DrAcct", "Not Found");
                gls.Add("CrAcct", "Not Found");
                gls.Add("DrAcctName", "Not Found");
                gls.Add("CrAcctName", "Not Found");
            }

            return gls;

        }

        public Hashtable getAdvGLClassified(MstEmployee emp, MstAdvance adv, int LocationID)
        {
            MstGLDetermination glDetr = getLocationGL(LocationID);
            Hashtable gls = new Hashtable();
            int GlId = glDetr.Id;
            int cntGl = 0;

            cntGl = (from p in oDB.MstGLDAdvanceDetail where p.GLDId.ToString() == GlId.ToString() && p.AdvancesId.ToString() == adv.Id.ToString() select p).Count();
            if (cntGl > 0)
            {
                MstGLDAdvanceDetail glAdv = (from p in oDB.MstGLDAdvanceDetail where p.GLDId.ToString() == GlId.ToString() && p.AdvancesId.ToString() == adv.Id.ToString() select p).FirstOrDefault();
                gls.Add("DrAcct", glAdv.CostAccount);
                gls.Add("CrAcct", glAdv.BalancingAccount);
                gls.Add("DrAcctName", glAdv.CostAcctDisplay);
                gls.Add("CrAcctName", glAdv.BalancingAcctDisplay);
                gls.Add("Indicator", string.IsNullOrEmpty(glAdv.A1Indicator) == true ? "Not Found" : glAdv.A1Indicator);
            }
            else
            {
                gls.Add("DrAcct", "Not Found");
                gls.Add("CrAcct", "Not Found");
                gls.Add("DrAcctName", "Not Found");
                gls.Add("CrAcctName", "Not Found");
                gls.Add("Indicator", "Not Found");
            }

            return gls;

        }

    }

}
