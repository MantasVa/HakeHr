CREATE PROCEDURE [dbo].[usp_HakeHr_Employee_GetById]
	@id int
AS
	SELECT [Id],
		   [Firstname],
		   [Lastname],
		   [Email],
		   [Birthdate],
		   [PhoneNumber],
		   [Address],
		   [Certifications],
		   [ManagerId],
		   [PhotoPath]
	FROM [Employee]
	WHERE [Id] = @id;
GO
