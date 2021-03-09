CREATE PROCEDURE [dbo].[usp_HakeHr_Team_Insert]
	@teamName NVARCHAR(50)
AS
    SET NOCOUNT OFF

	INSERT INTO [dbo].[Team]
		([TeamName])
	VALUES
		(@teamName);
GO;
