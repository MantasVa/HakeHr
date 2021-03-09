CREATE PROCEDURE [dbo].[usp_HakeHr_Team_GetById]
	@id INT
AS
	SELECT [Id], 
           [TeamName],
           [PhotoPath]
    FROM   [Team]  
    WHERE  (Id = @id) 
GO;
