CREATE PROCEDURE [dbo].[usp_HakeHr_Employee_Update]
	@id INT,
	@firstname NVARCHAR(50) = NULL,
	@lastname NVARCHAR(50) = NULL,
	@email NVARCHAR(100) = NULL,
	@birthdate DATE = NULL,
	@address NVARCHAR(200) = NULL, 
	@phoneNumber NVARCHAR(15) = NULL,
	@certifications NVARCHAR(1000) = NULL,
	@photoPath NVARCHAR(125) = NULL,
	@managerId INT = NULL
AS
	IF @managerId IS NOT NULL AND NOT EXISTS(SELECT 0 FROM Employee WHERE Id = @managerId)
		THROW 50000, 'Manager with specified ManagerId does not exist',1

	UPDATE [dbo].[Employee]
	SET
	[Firstname] = ISNULL(@firstname, [Firstname]),
	[Lastname] = ISNULL(@lastname, [Lastname]),
	[Email] = ISNULL(@email, [Email]),
	[Birthdate] = ISNULL(@birthdate, [Birthdate]),
	[Address] = ISNULL(@address, [Address]),
	[PhoneNumber] = ISNULL(@phoneNumber, [PhoneNumber]),
	[Certifications] = ISNULL(@certifications, [Certifications]),
	[ManagerId] = ISNULL(@managerId, [ManagerId]),
	[PhotoPath] = ISNULL(@photoPath, [PhotoPath]),
	[UpdatedAt] = GETDATE() 
	WHERE Id = @id;
GO
