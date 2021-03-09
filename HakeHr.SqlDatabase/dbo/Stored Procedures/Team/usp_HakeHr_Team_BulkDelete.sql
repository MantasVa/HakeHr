CREATE PROCEDURE [dbo].[usp_HakeHr_Team_BulkDelete]
    @startIndex INT = NULL,
    @endIndex INT = NULL,
	@Ids [IdList] READONLY
AS
	SET NOCOUNT ON;

	BEGIN TRY
        BEGIN TRANSACTION

        DELETE 
        FROM [Team] 
        WHERE [Team].Id IN (SELECT Id FROM @Ids) OR
        [Team].Id >= @startIndex AND [Team].Id <= @endIndex

        COMMIT TRANSACTION
    END TRY
    BEGIN CATCH
            PRINT ERROR_MESSAGE();

            ROLLBACK TRANSACTION
            THROW; -- Rethrow exception
    END CATCH
GO;
