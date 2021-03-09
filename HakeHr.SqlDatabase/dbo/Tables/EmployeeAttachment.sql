CREATE TABLE [dbo].[EmployeeAttachment]
(
	PRIMARY KEY([EmployeeId], [AttachmentId]),
	[EmployeeId] INT FOREIGN KEY REFERENCES [Employee](Id),
	[AttachmentId] INT FOREIGN KEY REFERENCES [Attachment](Id),
	[CreatedAt] SMALLDATETIME NOT NULL DEFAULT(GETDATE()),
	[UpdatedAt] SMALLDATETIME NULL
)
