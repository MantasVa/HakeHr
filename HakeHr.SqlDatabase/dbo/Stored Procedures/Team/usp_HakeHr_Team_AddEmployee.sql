CREATE PROCEDURE [dbo].[usp_HakeHr_Team_AddEmployee]
	@teamId INT,
	@employeeId INT
AS
	IF NOT EXISTS(SELECT 1 FROM [Team] WHERE Id = @teamId)
		THROW 50000, 'Team with specified Id does not exist',1

	IF NOT EXISTS(SELECT 1 FROM [Employee] WHERE Id = @employeeId)
		THROW 50000, 'Employee with specified Id does not exist',1

	IF EXISTS (SELECT 1 FROM [TeamEmployee] WHERE [EmployeeId] = @employeeId AND [TeamId] = @teamId)
		THROW 50000, 'This team member already belongs to this team',1

	INSERT INTO [TeamEmployee] ([TeamId],[EmployeeId])
	VALUES(@teamId, @employeeId)
GO;
