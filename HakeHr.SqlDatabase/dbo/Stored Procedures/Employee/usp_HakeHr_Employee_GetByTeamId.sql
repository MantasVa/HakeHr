CREATE PROCEDURE [dbo].[usp_HakeHr_Employee_GetByTeamId]
	@teamId INT
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
    FROM   [Employee] 
    INNER JOIN [TeamEmployee] o  ON o.EmployeeId = [Id]
	WHERE [TeamId] = @teamId
GO;
