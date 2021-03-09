CREATE PROCEDURE [dbo].[usp_HakeHr_Organization_Delete]
	@id INT
AS
		BEGIN TRANSACTION [Transaction];
	BEGIN TRY
		DELETE FROM [TeamOrganization] WHERE [OrganizationId] = @id;

		DELETE FROM [Organization] WHERE Id = @id;

		COMMIT TRANSACTION [Transaction]
	END TRY
	
	BEGIN CATCH
		ROLLBACK TRANSACTION [Transaction];
		THROW 50000,'Deletion was unsuccessful',1;
	END CATCH
GO
