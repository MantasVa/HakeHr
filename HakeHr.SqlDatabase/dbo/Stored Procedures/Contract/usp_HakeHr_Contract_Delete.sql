CREATE PROCEDURE [dbo].[usp_HakeHr_Contract_Delete]
	@id int
AS
	BEGIN TRANSACTION [Transaction];
	BEGIN TRY

		DELETE FROM [EmployeeContract] WHERE [ContractId] = @id;
		DELETE FROM Contract WHERE [Id] = @id;

		COMMIT TRANSACTION [Transaction]
	END TRY
	
	BEGIN CATCH
		ROLLBACK TRANSACTION [Transaction];
		THROW 50000,'Deletion was unsuccessful',1;
	END CATCH
GO