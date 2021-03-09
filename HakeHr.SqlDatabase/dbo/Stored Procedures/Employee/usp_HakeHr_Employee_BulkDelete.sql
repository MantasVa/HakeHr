CREATE PROCEDURE [dbo].[usp_HakeHr_Employee_BulkDelete]
    @startIndex INT = NULL,
    @endIndex INT = NULL,
	@Ids [IdList] READONLY
AS
	SET NOCOUNT ON;

	BEGIN TRY
        BEGIN TRANSACTION

        DELETE 
        FROM [Employee] 
        WHERE [Employee].Id IN (SELECT Id FROM @Ids) OR
        [Employee].Id >= @startIndex AND [Employee].Id <= @endIndex

        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
            PRINT ERROR_MESSAGE();

            ROLLBACK TRANSACTION
            THROW; -- Rethrow exception
    END CATCH
GO;
