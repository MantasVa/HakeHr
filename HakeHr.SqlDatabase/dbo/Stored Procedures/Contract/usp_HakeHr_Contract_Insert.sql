CREATE PROCEDURE [dbo].[usp_HakeHr_Contract_Insert]
	@Salary DECIMAL(19,4),
	@StartDate date,
	@ExpireDate date,
	@StatusId int
AS
	SET NOCOUNT OFF

	IF @StatusId IS NOT NULL AND NOT EXISTS(SELECT 0 FROM [Status] WHERE Id = @StatusId)
		THROW 50000, 'Status with specified ID does not exist',1

	INSERT INTO [dbo].[Contract]
		([Salary]
		,[StartDate]
		,[ExpireDate]
		,[StatusId])
	VALUES
		(@Salary
		,@StartDate
		,@ExpireDate
		,@StatusId);
GO