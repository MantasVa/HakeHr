CREATE TABLE [dbo].[ContractAttachment]
(
	PRIMARY KEY([ContractId], [AttachmentId]),
	[ContractId] INT FOREIGN KEY REFERENCES [Contract](Id),
	[AttachmentId] INT FOREIGN KEY REFERENCES [Attachment](Id),
	[CreatedAt] SMALLDATETIME NOT NULL DEFAULT(GETDATE()),
	[UpdatedAt] SMALLDATETIME NULL
)
