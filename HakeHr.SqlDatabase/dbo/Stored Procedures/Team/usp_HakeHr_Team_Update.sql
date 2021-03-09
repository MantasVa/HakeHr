CREATE PROCEDURE [dbo].[usp_HakeHr_Team_Update]
	@id INT,
	@teamName NVARCHAR(50) = NULL,
	@photoPath NVARCHAR(75) = NULL
AS
	UPDATE [dbo].[Team]
	SET
	[TeamName] = ISNULL(@teamName, [TeamName]),
	[PhotoPath] = ISNULL(@photoPath, [PhotoPath]),
	[UpdatedAt] = GETDATE() 
	WHERE Id = @id;
GO;
