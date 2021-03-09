CREATE PROCEDURE [dbo].[usp_HakeHr_Organization_BulkDelete]
    @startIndex INT = NULL,
    @endIndex INT = NULL,
	@Ids [IdList] READONLY
AS
	SET NOCOUNT ON;

	BEGIN TRY
        BEGIN TRANSACTION

        DELETE 
        FROM [Organization] 
        WHERE [Organization].Id IN (SELECT Id FROM @Ids) OR
        [Organization].Id >= @startIndex AND [Organization].Id <= @endIndex

        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
            PRINT ERROR_MESSAGE();

            ROLLBACK TRANSACTION
            THROW; -- Rethrow exception
    END CATCH
GO;
