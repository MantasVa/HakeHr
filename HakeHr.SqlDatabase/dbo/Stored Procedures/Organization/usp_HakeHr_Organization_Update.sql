CREATE PROCEDURE [dbo].[usp_HakeHr_Organization_Update]
	@id INT,
	@organizationName NVARCHAR(50) = NULL,
	@photoPath NVARCHAR(75) = NULL
AS
	UPDATE [dbo].[Organization]
	SET
	[OrganizationName] = ISNULL(@organizationName, [OrganizationName]),
	[PhotoPath] = ISNULL(@photoPath, [PhotoPath]),
	[UpdatedAt] = GETDATE() 
	WHERE Id = @id;
GO;
