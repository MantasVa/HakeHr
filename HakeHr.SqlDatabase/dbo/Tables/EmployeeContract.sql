CREATE TABLE [dbo].[EmployeeContract]
(
	PRIMARY KEY([ContractId], [EmployeeId]),
	[ContractId] INT FOREIGN KEY REFERENCES [Contract](Id),
	[EmployeeId] INT FOREIGN KEY REFERENCES [Employee](Id),
	[IsCurrent] BIT NOT NULL, 
	[CreatedAt] SMALLDATETIME NOT NULL DEFAULT(GETDATE()),
	[UpdatedAt] SMALLDATETIME NULL
)
