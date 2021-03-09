CREATE PROCEDURE [dbo].[usp_HakeHr_Employee_List]
	@recordsPerPage INT,
	@pageNumber INT
AS
	SET @recordsPerPage = ISNULL(@recordsPerPage, 2147483647);
	SET @pageNumber = ISNULL(@pageNumber, 1);

	SELECT [Id],
		   [Firstname],
		   [Lastname],
		   [Email],
		   [Birthdate],
		   [PhoneNumber],
		   [Address],
		   [Certifications],
		   [ManagerId],
		   [PhotoPath]
	FROM [Employee]
	ORDER BY [Firstname], [Lastname]
	OFFSET @recordsPerPage * (@pageNumber - 1)
	ROWS FETCH NEXT @recordsPerPage ROWS ONLY;
GO;
