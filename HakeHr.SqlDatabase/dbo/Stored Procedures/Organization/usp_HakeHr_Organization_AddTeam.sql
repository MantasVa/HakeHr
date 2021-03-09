CREATE PROCEDURE [dbo].[usp_HakeHr_Organization_AddTeam]
	@organizationId INT,
	@teamId INT
AS
	IF NOT EXISTS(SELECT 1 FROM [Organization] WHERE Id = @organizationId)
		THROW 50000, 'Organization with specified Id does not exist',1

	IF NOT EXISTS(SELECT 1 FROM [Team] WHERE Id = @teamId)
		THROW 50000, 'Team with specified Id does not exist',1

	IF EXISTS (SELECT 1 FROM [TeamOrganization] WHERE [OrganizationId] = @organizationId AND [TeamId] = @teamId)
		THROW 50000, 'This team already belongs to this organization',1

	INSERT INTO [TeamOrganization] ([OrganizationId], [TeamId])
	VALUES(@organizationId, @teamId)
GO;
