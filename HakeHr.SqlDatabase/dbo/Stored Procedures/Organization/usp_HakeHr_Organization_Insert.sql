CREATE PROCEDURE [dbo].[usp_HakeHr_Organization_Insert]
	@organizationName NVARCHAR(50)
AS
	SET NOCOUNT OFF

	INSERT INTO [dbo].[Organization]
		([OrganizationName])
	VALUES
		(@organizationName);
GO;
