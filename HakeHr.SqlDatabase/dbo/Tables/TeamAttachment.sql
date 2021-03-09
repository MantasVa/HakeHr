CREATE TABLE [dbo].[TeamAttachment]
(
	PRIMARY KEY([TeamId], [AttachmentId]),
	[TeamId] INT FOREIGN KEY REFERENCES [Team](Id),
	[AttachmentId] INT FOREIGN KEY REFERENCES [Attachment](Id),
	[CreatedAt] SMALLDATETIME NOT NULL DEFAULT(GETDATE()),
	[UpdatedAt] SMALLDATETIME NULL
)
