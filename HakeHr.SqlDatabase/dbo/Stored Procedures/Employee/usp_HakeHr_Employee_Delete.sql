CREATE PROCEDURE [dbo].[usp_HakeHr_Employee_Delete]
	@id int
AS
	BEGIN TRANSACTION [Transaction];
	BEGIN TRY
		DELETE FROM [EmployeeContract] WHERE [EmployeeId] = @id;

		DELETE FROM [TeamEmployee] WHERE [EmployeeId] = @id;

		UPDATE Employee
		SET [ManagerId] = NULL
		WHERE [ManagerId] = @id;

		DELETE FROM [Employee] WHERE Id = @id;

		COMMIT TRANSACTION [Transaction]
	END TRY
	
	BEGIN CATCH
		ROLLBACK TRANSACTION [Transaction];
		THROW 50000,'Deletion was unsuccessful',1;
	END CATCH
GO
