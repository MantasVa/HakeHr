CREATE PROCEDURE [dbo].[usp_HakeHr_Attachment_BulkInsert]
	@nameList NameList READONLY,
	@attachmentFor NVARCHAR(50),
	@ownerId INT
AS

	BEGIN TRANSACTION [Transaction];
	BEGIN TRY
		DECLARE @pkTable TABLE (Id INT)

		INSERT INTO [Attachment]
		(
			[Filename]
		)
		OUTPUT Inserted.Id
		INTO @pkTable(Id)
		SELECT [Name] FROM @nameList

		IF(@attachmentFor = 'Contract')
			INSERT INTO [ContractAttachment](ContractId, AttachmentId) (SELECT @ownerId, Id FROM @pkTable);

		IF(@attachmentFor = 'Employee')
			INSERT INTO [EmployeeAttachment](EmployeeId, AttachmentId) (SELECT @ownerId, Id FROM @pkTable)

		IF(@attachmentFor = 'Organization')
			INSERT INTO [OrganizationAttachment](OrganizationId, AttachmentId) (SELECT @ownerId, Id FROM @pkTable)

		IF(@attachmentFor = 'Team')
			INSERT INTO [TeamAttachment](TeamId, AttachmentId) (SELECT @ownerId, Id FROM @pkTable)

		COMMIT TRANSACTION [Transaction]
	END TRY
	
	BEGIN CATCH
		ROLLBACK TRANSACTION [Transaction];
		THROW 50000,'Attachment insert was unsuccessful',1;
	END CATCH

GO
