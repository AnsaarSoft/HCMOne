ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegisterDetail_TrnsFinalSettelmentRegister]
ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegisterDetail_MstAdvance]
ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegisterDetail_MstArrears]
ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegisterDetail_MstBonus]
ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegisterDetail_MstElements]
ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegisterDetail_MstExpense]
ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegisterDetail_MstLoans]


ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegister_CfgPayrollDefination]
ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegister_CfgPeriodDates]
ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] DROP CONSTRAINT[FK_TrnsFinalSettelmentRegister_MstEmployee]

ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] DROP CONSTRAINT [PK_TrnsFinalSettelmentRegister]

ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] DROP CONSTRAINT [PK_TrnsFinalSettelmentRegisterDetail]

ALTER TABLE [dbo].[MstEmployee] ALTER COLUMN [OfficeEmail] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
ALTER TABLE [dbo].[MstEmployee] ALTER COLUMN [BankBranch] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL

ALTER TABLE [dbo].[MstElements] ALTER COLUMN [ElementName] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL

ALTER TABLE [dbo].[tblRpts] ADD
[Critaria] [nvarchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL

DROP TABLE dbo.TrnsFinalSettelmentRegister

DROP TABLE dbo.TrnsFinalSettelmentRegisterDetail


CREATE TABLE [dbo].[TrnsFinalSettelmentRegister]
(
[Id] [int] NOT NULL IDENTITY(1, 1),
[PayrollID] [int] NULL,
[PayrollPeriodID] [int] NULL,
[PayrollName] [nvarchar] (30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[PeriodName] [nvarchar] (30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[EmpID] [int] NULL,
[EmpName] [nvarchar] (50) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[EmpGross] [numeric] (18, 6) NULL,
[EmpBasic] [numeric] (18, 6) NULL,
[EmpOT] [numeric] (18, 6) NULL,
[EmpElementTotal] [numeric] (18, 6) NULL,
[EmpAdvance] [numeric] (18, 6) NULL,
[EmpBonus] [numeric] (18, 6) NULL,
[EmpLoan] [numeric] (18, 6) NULL,
[EmpExpense] [numeric] (18, 6) NULL,
[EmpArrears] [numeric] (18, 6) NULL,
[EmpRetroElement] [numeric] (18, 6) NULL,
[JENum] [int] NULL,
[FinalSettlementStatus] [int] NOT NULL,
[EmpTaxblTotal] [numeric] (18, 6) NULL,
[EmpTotalTax] [numeric] (18, 6) NULL,
[DaysPaid] [smallint] NULL,
[FSStatus] [tinyint] NULL,
[CreateDate] [datetime] NULL,
[UpdateDate] [datetime] NULL,
[UserId] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[UpdateBy] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)


CREATE TABLE [dbo].[TrnsFinalSettelmentRegisterDetail]
(
[Id] [int] NOT NULL IDENTITY(1, 1),
[FSID] [int] NOT NULL,
[LineType] [nvarchar] (30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LineSubType] [nvarchar] (30) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LineValue] [numeric] (18, 6) NULL,
[LineMemo] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[LineBaseEntry] [int] NULL,
[BaseValue] [numeric] (18, 6) NULL,
[BaseValueType] [nvarchar] (20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[BaseValueCalculatedOn] [numeric] (18, 6) NULL,
[NoOfDay] [smallint] NULL,
[OTHours] [numeric] (18, 6) NULL,
[CreditAccount] [nvarchar] (16) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[CreditAccountName] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[DebitAccount] [nvarchar] (16) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[DebitAccountName] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[CreditValue] [numeric] (18, 6) NULL,
[DebitValue] [numeric] (18, 6) NULL,
[flgGross] [bit] NULL,
[flgStandard] [bit] NULL,
[SortOrder] [smallint] NULL,
[TaxableAmount] [numeric] (18, 6) NULL,
[CreateDate] [datetime] NULL,
[UpdateDate] [datetime] NULL,
[UserId] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[UpdatedBy] [nvarchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)


ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] ADD CONSTRAINT [FK_TrnsFinalSettelmentRegister_CfgPayrollDefination] FOREIGN KEY ([PayrollID]) REFERENCES [dbo].[CfgPayrollDefination] ([ID])
ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] ADD CONSTRAINT [FK_TrnsFinalSettelmentRegister_CfgPeriodDates] FOREIGN KEY ([PayrollPeriodID]) REFERENCES [dbo].[CfgPeriodDates] ([ID])
ALTER TABLE [dbo].[TrnsFinalSettelmentRegister] ADD CONSTRAINT [FK_TrnsFinalSettelmentRegister_MstEmployee] FOREIGN KEY ([EmpID]) REFERENCES [dbo].[MstEmployee] ([ID])

ALTER TABLE [dbo].[TrnsFinalSettelmentRegisterDetail] ADD CONSTRAINT [PK_TrnsFinalSettelmentRegisterDetail] PRIMARY KEY CLUSTERED  ([Id])

ALTER TABLE [dbo].[MstEmployee] ADD
[TerminationDate] [datetime] NULL

ALTER TABLE [dbo].[MstEmployee] ALTER COLUMN [OfficeEmail] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
ALTER TABLE [dbo].[MstEmployee] ALTER COLUMN [FatherName] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
ALTER TABLE [dbo].[MstEmployee] ALTER COLUMN [MotherName] [nvarchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
ALTER TABLE [dbo].[MstEmployee] ALTER COLUMN [PercentagePaid] [numeric] (18, 6) NULL
ALTER TABLE [dbo].[MstEmployee] ALTER COLUMN [BankBranch] [nvarchar] (150) COLLATE SQL_Latin1_General_CP1_CI_AS NULL

ALTER TABLE dbo.TrnsSalaryProcessRegister ADD
[flgHoldPayment] BIT NULL

ALTER TABLE [dbo].[MstLeaveType] ADD
[flgVL] [bit] NULL