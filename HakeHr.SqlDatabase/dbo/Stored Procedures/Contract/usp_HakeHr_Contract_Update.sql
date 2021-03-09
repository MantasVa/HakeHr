CREATE PROCEDURE [dbo].[usp_HakeHr_Contract_Update]
	@id INT,
	@Salary DECIMAL(19,4) = NULL,
	@StartDate DATE = NULL,
	@ExpireDate DATE = NULL,
	@StatusId INT = NULL
AS
	IF @StatusId IS NOT NULL AND NOT EXISTS(SELECT 0 FROM [Status] WHERE Id = @StatusId)
		THROW 50000, 'Status with specified Status does not exist',1

	UPDATE [dbo].[Contract]
	SET
	[Salary] = ISNULL(@Salary, [Salary]),
	[StartDate] = ISNULL(@StartDate, [StartDate]),
	[ExpireDate] = ISNULL(@ExpireDate, [ExpireDate]),
	[StatusId] = ISNULL(@StatusId, [StatusId]),
	[UpdatedAt] = GETDATE() 
	WHERE [Id] = @id;
GO