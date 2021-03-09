CREATE PROCEDURE [dbo].[usp_HakeHr_Contract_GetByEmployeeId]
    @employeeId int
AS 
BEGIN 
    SELECT [Id], [Salary], [StartDate], [ExpireDate], [StatusId]
    FROM   [Contract] 
    INNER JOIN [EmployeeContract] ec  ON ec.[ContractId] = Id
	WHERE [EmployeeId] = @employeeId
END
GO
