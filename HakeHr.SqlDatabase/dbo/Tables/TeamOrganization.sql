CREATE TABLE [dbo].[TeamOrganization]
(
	PRIMARY KEY([TeamId], [OrganizationId]),
	[TeamId] INT FOREIGN KEY REFERENCES [Team](Id),
	[OrganizationId] INT FOREIGN KEY REFERENCES [Organization](Id),
	[CreatedAt] SMALLDATETIME NOT NULL DEFAULT(GETDATE()),
	[UpdatedAt] SMALLDATETIME NULL
)
