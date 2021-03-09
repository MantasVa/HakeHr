CREATE PROCEDURE [dbo].[usp_HakeHr_Contract_GetById]
    @id int
AS 
BEGIN 
 
    SELECT [Id], [Salary], [StartDate], [ExpireDate], [StatusId]
    FROM   [Contract]  
    WHERE  ([Id] = @id) 
END
GO
