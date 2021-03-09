CREATE PROCEDURE [dbo].[usp_HakeHr_Contract_BulkDelete]
    @startIndex INT = NULL,
    @endIndex INT = NULL,
    @Ids [IdList] READONLY
AS
	SET NOCOUNT ON;

	BEGIN TRY
        BEGIN TRANSACTION

        DELETE 
        FROM [Contract] 
        WHERE [Contract].Id IN (SELECT Id FROM @Ids) OR
        [Contract].Id >= @startIndex AND [Contract].Id <= @endIndex

        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
            PRINT ERROR_MESSAGE();

            ROLLBACK TRANSACTION
            THROW; -- Rethrow exception
    END CATCH
GO;