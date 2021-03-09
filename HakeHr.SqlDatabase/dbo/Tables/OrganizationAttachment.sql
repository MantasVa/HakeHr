CREATE TABLE [dbo].[OrganizationAttachment]
(
	PRIMARY KEY([OrganizationId], [AttachmentId]),
	[OrganizationId] INT FOREIGN KEY REFERENCES [Organization](Id),
	[AttachmentId] INT FOREIGN KEY REFERENCES [Attachment](Id),
	[CreatedAt] SMALLDATETIME NOT NULL DEFAULT(GETDATE()),
	[UpdatedAt] SMALLDATETIME NULL
)
