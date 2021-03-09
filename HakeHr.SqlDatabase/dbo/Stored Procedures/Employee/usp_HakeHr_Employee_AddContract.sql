CREATE PROCEDURE [dbo].[usp_HakeHr_Employee_AddContract]
	@employeeId INT,
	@contractId INT,
	@isCurrent BIT
AS
	IF NOT EXISTS(SELECT 1 FROM [Employee] WHERE Id = @employeeId)
	THROW 50000, 'Employee with specified Id does not exist',1

	IF NOT EXISTS (SELECT 1 FROM [Contract] WHERE Id = @contractId)
		THROW 50000, 'Contract with specified Id does not exist',1

	IF EXISTS (SELECT 1 FROM [EmployeeContract] WHERE [EmployeeId] = @employeeId AND [ContractId] = @contractId)
		THROW 50000, 'This contract is already assigned to this employee',1

	INSERT INTO EmployeeContract ([EmployeeId],[ContractId],[IsCurrent])
	VALUES(@employeeId, @contractId, @isCurrent)
RETURN 0
