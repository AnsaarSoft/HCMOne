using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DIHRMS.Custom
{
    public class clsElement
    {
        dbHRMS oDB = null;
        public string ValueType;
        public decimal Value;
        public decimal emprValue;
        public string ElementType;
        public decimal Amount;
        public decimal emprAmount;
        public string debitAcct;
        public string creditAcct;
        public string debitAcctName;
        public string creditAcctName;
        public bool flgActive = true;
        public DataServices ds;

        public clsElement(dbHRMS pDB)
        {
            ValueType = "";
            Value = 0.00M;
            emprValue = 0.00M;
            ElementType = "";
            Amount = 0.00M;
            debitAcct = "";
            creditAcct = "";
            debitAcctName = "";
            creditAcctName = "";

        }
        
        public clsElement(dbHRMS pDB,MstElements empelement)
        {
            ElementType = empelement.ElmtType.Trim();
            debitAcct = "";
            creditAcct = "";
            debitAcctName = "";
            creditAcctName = "";
            oDB = pDB;
            Amount = 0M;

            if (empelement.ElmtType.Trim() == "Ear")
            {
                ValueType = empelement.MstElementEarning[0].ValueType.ToString();
                Value = Convert.ToDecimal(empelement.MstElementEarning[0].Value.ToString());                
            }
            if (empelement.ElmtType.Trim() == "Ded")
            {
                ValueType = empelement.MstElementDeduction[0].ValueType.ToString();
                Value = Convert.ToDecimal(empelement.MstElementDeduction[0].Value.ToString());
                
            }
            if (empelement.ElmtType.Trim() == "Con")
            {
                ValueType = empelement.MstElementContribution[0].ContributionID.ToString();
                Value = Convert.ToDecimal(empelement.MstElementContribution[0].Employee.ToString());
                emprValue = Convert.ToDecimal(empelement.MstElementContribution[0].Employer.ToString());
            }
            if (empelement.ElmtType.Trim() == "Inf")
            {
                ValueType = "FIX"; //empelement.MstElementInformation[0].ValueType.ToString();
                Value = Convert.ToDecimal(empelement.MstElementInformation[0].Value.ToString());
                //Amount = Convert.ToDecimal(empelement.MstElementInformation[0].Value.ToString());
            }
        }

        public clsElement(dbHRMS pDB, MstElements empelement, MstEmployee oEmp)
        {
            ElementType = empelement.ElmtType.Trim();
            debitAcct = "";
            creditAcct = "";
            debitAcctName = "";
            
            oDB = pDB;

            if (empelement.ElmtType.Trim() == "Ear")
            {
                ValueType = empelement.MstElementEarning[0].ValueType.ToString();
                Value = Convert.ToDecimal(empelement.MstElementEarning[0].Value.ToString());
                //emprAmount = Convert.ToDecimal(emprValue) / 100 * (decimal)emp.BasicSalary;
                switch (ValueType)
                {
                    case "POB":
                        Amount = ((Value / 100) * Convert.ToDecimal(oEmp.BasicSalary));
                        break;
                    case "POG":
                        //Amount = ((Value / 100) * ds.getEmpGross(oEmp, ""));
                        Amount = (Value / 100) * Convert.ToDecimal(oEmp.GrossSalary == null ? 0M : oEmp.GrossSalary);
                        break;
                    case "FIX":
                        Amount = Value;
                        break;
                }
            }
            if (empelement.ElmtType.Trim() == "Ded")
            {
                ValueType = empelement.MstElementDeduction[0].ValueType.ToString();
                Value = Convert.ToDecimal(empelement.MstElementDeduction[0].Value.ToString());
                switch (ValueType)
                {
                    case "POB":
                        Amount = ((Value / 100) * Convert.ToDecimal(oEmp.BasicSalary));
                        break;
                    case "POG":
                        //Amount = ((Value / 100) * ds.getEmpGross(oEmp, ""));
                        Amount = (Value / 100) * Convert.ToDecimal(oEmp.GrossSalary == null ? 0M : oEmp.GrossSalary);
                        break;
                    case "FIX":
                        Amount = Value;
                        break;
                }
            }
            if (empelement.ElmtType.Trim() == "Con")
            {
                ValueType = empelement.MstElementContribution[0].ContributionID.ToString().Trim();
                Value = Convert.ToDecimal(empelement.MstElementContribution[0].Employee.ToString());
                emprValue = Convert.ToDecimal(empelement.MstElementContribution[0].Employer.ToString());
                switch (ValueType)
                {
                    case "POB":
                        Amount = ((Value / 100) * Convert.ToDecimal(oEmp.BasicSalary));
                        emprAmount = ((emprValue / 100) * Convert.ToDecimal(oEmp.BasicSalary));
                        break;
                    case "POG":
                        //Amount = ((Value / 100) * ds.getEmpGross(oEmp, ""));
                        Amount = (Value / 100) * Convert.ToDecimal(oEmp.GrossSalary == null ? 0M : oEmp.GrossSalary);
                        emprAmount = (emprValue / 100) * Convert.ToDecimal(oEmp.GrossSalary == null ? 0M : oEmp.GrossSalary);
                        break;
                    case "FIX":
                        Amount = Value;
                        emprAmount = emprValue;
                        break;
                }
            }
            if (empelement.ElmtType.Trim() == "Inf")
            {
                ValueType = "FIX"; //empelement.MstElementInformation[0].ValueType.ToString();
                Value = Convert.ToDecimal(empelement.MstElementInformation[0].Value.ToString());
                //Amount = Convert.ToDecimal(empelement.MstElementInformation[0].Value.ToString());
            }
        }

        public MstGLDetermination getEmpGl(MstEmployee emp)
        {
            MstGLDetermination detr = new MstGLDetermination();
            string GlType = emp.CfgPayrollDefination.GLType.ToString().Trim();

            try
            {

                if (GlType == "LOC")
                {
                    detr = (from p in oDB.MstGLDetermination where p.GLType == "LOC" && p.GLValue == emp.Location select p).Single();
                }
                else if (GlType == "DEPT")
                {
                    detr = (from p in oDB.MstGLDetermination where p.GLType == "DEPT" && p.GLValue == emp.DepartmentID select p).Single();
                }
                else if (GlType == "COMP")
                {
                    detr = (from p in oDB.MstGLDetermination where p.GLType == "COMP" select p).Single();
                }
                else
                {
                    detr = (from p in oDB.MstGLDetermination where p.GLType == "COMP" select p).Single();
                }

            }
            catch (Exception ex)
            {

            }
            return detr;
        }

        public Hashtable getElementGL(MstEmployee emp, MstElements ele)
        {
            MstGLDetermination glDetr = getEmpGl(emp);
            Hashtable gls = new Hashtable();
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
                    }
                    else
                    {
                        gls.Add("DrAcct", "Not Found");
                        gls.Add("CrAcct", "Not Found");
                        gls.Add("DrAcctName", "Not Found");
                        gls.Add("CrAcctName", "Not Found");
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
                    }
                    else
                    {
                        gls.Add("DrAcct", "Not Found");
                        gls.Add("CrAcct", "Not Found");
                        gls.Add("DrAcctName", "Not Found");
                        gls.Add("CrAcctName", "Not Found");
                    }
                    break;
                //case "Inf":
                //    cntGl = (from p in oDB.mstGli where p.GLDId.ToString() == GlId.ToString() && p.ContributionId.ToString() == ele.Id.ToString() select p).Count();
                //    if (cntGl > 0)
                //    {
                //        MstGLDContribution glCont = (from p in oDB.MstGLDContribution where p.GLDId.ToString() == GlId.ToString() && p.ContributionId.ToString() == ele.Id.ToString() select p).Single();
                //        gls.Add("DrAcct", glCont.CostAccount);
                //        gls.Add("CrAcct", glCont.BalancingAccount);
                //        gls.Add("DrAcctName", glCont.CostAcctDisplay);
                //        gls.Add("CrAcctName", glCont.BalancingAcctDisplay);
                //    }
                //    else
                //    {
                //        gls.Add("DrAcct", "Not Found");
                //        gls.Add("CrAcct", "Not Found");
                //        gls.Add("DrAcctName", "Not Found");
                //        gls.Add("CrAcctName", "Not Found");
                //    }
                //    break;
            }

            return gls;

        }

        public clsElement(dbHRMS pDB,TrnsEmployeeElementDetail empelement, MstEmployee emp, int pType = 1) : this(pDB, empelement.MstElements)
        {
            
            decimal outValue = Convert.ToDecimal(0.00);
            ValueType = empelement.ValueType;
            bool flgGosi = empelement.MstElements.FlgGosi == null ? false : empelement.MstElements.FlgGosi.Value;
           
            string valType = ValueType;


            Value = Convert.ToDecimal(empelement.Value);

           
            switch (valType.Trim())
            {

                case "POB":
                    outValue = Convert.ToDecimal(Value) / 100 * (decimal)emp.BasicSalary;
                    emprAmount = Convert.ToDecimal(emprValue) / 100 * (decimal)emp.BasicSalary;
                    if (flgGosi)
                    {
                        outValue = Convert.ToDecimal(Value) / 100 * (decimal)emp.GosiSalary;
                        emprAmount = Convert.ToDecimal(emprValue) / 100 * (decimal)emp.GosiSalary;
                    }
                    break;             
                case "POG":
                    //Changing From Basic Into Gross Due high demand by RAJ
                    //outValue = Convert.ToDecimal(Value) / 100 * (decimal) emp.BasicSalary;
                    //emprAmount = Convert.ToDecimal(emprValue) / 100 * (decimal)emp.BasicSalary;
                    outValue = Convert.ToDecimal(Value) / 100 * Convert.ToDecimal(ds.getEmpGross(emp)) ;
                    emprAmount = Convert.ToDecimal(emprValue) / 100 * Convert.ToDecimal(ds.getEmpGross(emp));
                    if (flgGosi)
                    {
                        outValue = Convert.ToDecimal(Value) / 100 * (decimal)emp.GosiSalary;
                        emprAmount = Convert.ToDecimal(emprValue) / 100 * (decimal)emp.GosiSalary;
                    }
                    break;
                case "FIX":
                    outValue = Convert.ToDecimal(Value);
                    emprAmount = Convert.ToDecimal(emprValue);
                    break;
            }
            Hashtable elementGls = new Hashtable();
           elementGls = getElementGL(emp, empelement.MstElements);
           debitAcct = elementGls["DrAcct"].ToString();
           debitAcctName = elementGls["DrAcctName"].ToString();
           creditAcct = elementGls["CrAcct"].ToString();
           creditAcctName = elementGls["CrAcctName"].ToString();
           if (pType == 1)
           {
               Amount = Math.Round(outValue, 0, MidpointRounding.AwayFromZero);
           }
           else
           {
               Amount = outValue;
           }
           //Amount = Math.Round(outValue, 4);
        }

        public clsElement(dbHRMS pDB, TrnsEmployeeElementDetail empelement, MstEmployee emp, decimal grossSal, int pType)
            : this(pDB, empelement.MstElements)
        {
            string ElementName = "";
            decimal outValue = Convert.ToDecimal(0.00);
            ValueType = empelement.ValueType;
            ElementName = empelement.MstElements.Description;
            bool flgGosi = empelement.MstElements.FlgGosi == null ? false : empelement.MstElements.FlgGosi.Value;

            string valType = ValueType;

            if (!Convert.ToBoolean(empelement.MstElements.MstElementEarning[0].FlgLeaveEncashment))
            {
                Value = Convert.ToDecimal(empelement.Value);
                if (empelement.ElementType == "Con")
                {
                    if (empelement.EmplrContr != null)
                    {
                        emprValue = Convert.ToDecimal(empelement.EmplrContr);
                    }
                    else
                    {
                        emprValue = Convert.ToDecimal(empelement.MstElements.MstElementContribution[0].Employer);
                    }
                }
                else
                {
                    emprValue = 0;
                }
            }
            else
            {
                Value = Convert.ToDecimal(empelement.MstElements.MstElementEarning[0].Value);
                if (empelement.ElementType == "Con")
                {
                    emprValue = Convert.ToDecimal(empelement.MstElements.MstElementContribution[0].Employer);
                }
                else
                {
                    emprValue = 0;
                }
            }

            switch (valType.Trim())
            {

                case "POB":
                    outValue = Convert.ToDecimal(Value) / 100 * (decimal)emp.BasicSalary;
                    emprAmount = Convert.ToDecimal(emprValue) / 100 * (decimal)emp.BasicSalary;
                    if (flgGosi)
                    {
                        outValue = Convert.ToDecimal(Value) / 100 * (decimal)emp.GosiSalary;
                        emprAmount = Convert.ToDecimal(emprValue) / 100 * (decimal)emp.GosiSalary;
                    }
                    break;
                case "POG":
                    //Changing From Basic Into Gross Due high demand by RAJ
                    //outValue = Convert.ToDecimal(Value) / 100 * (decimal) emp.BasicSalary;
                    //emprAmount = Convert.ToDecimal(emprValue) / 100 * (decimal)emp.BasicSalary;
                    outValue = Convert.ToDecimal(Value) / 100 * Convert.ToDecimal(grossSal);
                    emprAmount = Convert.ToDecimal(emprValue) / 100 * Convert.ToDecimal(grossSal);
                    if (flgGosi)
                    {
                        outValue = Convert.ToDecimal(Value) / 100 * (decimal)emp.GosiSalary;
                        emprAmount = Convert.ToDecimal(emprValue) / 100 * (decimal)emp.GosiSalary;
                    }
                    break;
                case "FIX":
                    if (empelement.Amount == null || empelement.Amount == 0)
                    {
                        if (empelement.Value != null || empelement.Value != 0)
                        {
                            outValue = Convert.ToDecimal(empelement.Value);
                        }
                    }
                    else
                    {
                        outValue = Convert.ToDecimal(empelement.Amount);
                    }
                    if (empelement.EmplrContr != null)
                    {
                        emprAmount = Convert.ToDecimal(empelement.EmplrContr);
                    }
                    else
                    {
                        emprAmount = Convert.ToDecimal(empelement.MstElements.MstElementContribution[0].Employer);
                    }
                    break;
                case "FGosi%":
                    outValue = Convert.ToDecimal(Value) / 100 * (decimal)emp.GosiSalary;
                    emprAmount = Convert.ToDecimal(emprValue) / 100 * (decimal)emp.GosiSalary;
                    break;
                case "VGosi%":
                    outValue = Convert.ToDecimal(Value) / 100 * (decimal)emp.GosiSalaryV;
                    emprAmount = Convert.ToDecimal(emprValue) / 100 * (decimal)emp.GosiSalaryV;
                    break;
            }
            Hashtable elementGls = new Hashtable();
            elementGls = getElementGL(emp, empelement.MstElements);
            debitAcct = elementGls["DrAcct"].ToString();
            debitAcctName = elementGls["DrAcctName"].ToString();
            creditAcct = elementGls["CrAcct"].ToString();
            creditAcctName = elementGls["CrAcctName"].ToString();
            if (pType == 1)
            {
                Amount = Math.Round(outValue, 0, MidpointRounding.AwayFromZero);
            }
            else if (pType == 0)
            {
                Amount = outValue;
            }
            //Amount = Math.Round(outValue, 4);
        }

    }
}
