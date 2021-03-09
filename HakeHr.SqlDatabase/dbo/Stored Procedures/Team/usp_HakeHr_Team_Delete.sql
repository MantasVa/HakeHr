CREATE PROCEDURE [dbo].[usp_HakeHr_Team_Delete]
	@id INT 
AS
	BEGIN TRANSACTION [Transaction];
	BEGIN TRY
		DELETE FROM [TeamEmployee] WHERE [TeamId] = @id;

		DELETE FROM [TeamOrganization] WHERE [TeamId] = @id;

		DELETE FROM [Team] WHERE Id = @id;

		COMMIT TRANSACTION [Transaction]
	END TRY
	
	BEGIN CATCH
		ROLLBACK TRANSACTION [Transaction];
		THROW 50000,'Deletion was unsuccessful',1;
	END CATCH
GO;
