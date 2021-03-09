CREATE PROCEDURE [dbo].[usp_HakeHr_Attachment_DeleteFromOwner]
	@attachmentId INT,
	@ownerId INT,
	@attachmentFor NVARCHAR(50)
AS
	IF(@attachmentFor = 'Contract')
		DELETE FROM ContractAttachment WHERE ContractId = @ownerId AND AttachmentId = @attachmentId

	IF(@attachmentFor = 'Employee')
		DELETE FROM EmployeeAttachment WHERE EmployeeId = @ownerId AND AttachmentId = @attachmentId

	IF(@attachmentFor = 'Organization')
		DELETE FROM OrganizationAttachment WHERE OrganizationId = @ownerId AND AttachmentId = @attachmentId

	IF(@attachmentFor = 'Team')
		DELETE FROM TeamAttachment WHERE TeamId = @ownerId AND AttachmentId = @attachmentId

	IF NOT EXISTS(SELECT 1 FROM TeamAttachment WHERE AttachmentId = @attachmentId 
				 UNION 
				  SELECT 1 FROM OrganizationAttachment WHERE AttachmentId = @attachmentId 
				 UNION
				  SELECT 1 FROM EmployeeAttachment WHERE AttachmentId = @attachmentId 
				 UNION
				  SELECT 1 FROM ContractAttachment WHERE AttachmentId = @attachmentId
				  )
		DELETE FROM Attachment 
		OUTPUT DELETED.Filename
		WHERE Id = @attachmentId 
GO;
