CREATE PROCEDURE [dbo].[usp_HakeHr_Team_GetByOrganizationId]
	@organizationId INT
AS
	SELECT [Id], [TeamName], [PhotoPath]
    FROM   [Team] 
    INNER JOIN [TeamOrganization] o  ON o.[TeamId] = [Id]
	WHERE [OrganizationId] = @organizationId
GO;
