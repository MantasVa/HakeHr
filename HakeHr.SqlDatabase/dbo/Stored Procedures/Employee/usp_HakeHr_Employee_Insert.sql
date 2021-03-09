CREATE PROCEDURE [dbo].[usp_HakeHr_Employee_Insert]
	@firstname NVARCHAR(50),
	@lastname NVARCHAR(50),
	@email NVARCHAR(100) = NULL,
	@birthdate DATE,
	@address NVARCHAR(200) = NULL, 
	@phoneNumber NVARCHAR(15),
	@certifications NVARCHAR(1000) = NULL,
	@managerId INT = NULL
AS
	SET NOCOUNT ON

	IF @managerId IS NOT NULL AND NOT EXISTS(SELECT 0 FROM [Employee] WHERE [Id] = @managerId)
		THROW 50000, 'Manager with specified ManagerId does not exist',1

	INSERT INTO [dbo].[Employee]
		([Firstname]
		,[Lastname]
		,[Email]
		,[Birthdate]
		,[Address]
		,[PhoneNumber]
		,[Certifications])
	VALUES
		(@firstname
		,@lastname
		,@email
		,@birthdate
		,@address
		,@phoneNumber
		,@certifications);
GO
