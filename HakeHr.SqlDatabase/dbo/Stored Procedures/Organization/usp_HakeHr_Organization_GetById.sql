CREATE PROCEDURE [dbo].[usp_HakeHr_Organization_GetById]
	@id INT
AS
    SELECT [Id], 
           [OrganizationName],
           [PhotoPath]
    FROM   [Organization]  
    WHERE  (Id = @id) 
GO;
