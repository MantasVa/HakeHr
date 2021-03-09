CREATE PROCEDURE [dbo].[usp_HakeHr_Organization_List]
	@recordsPerPage INT,
	@pageNumber INT
AS
	SET @recordsPerPage = ISNULL(@recordsPerPage, 2147483647);
	SET @pageNumber = ISNULL(@pageNumber, 1);

	SELECT [Id],
		   [OrganizationName],
		   [PhotoPath]
	FROM [Organization]
	ORDER BY [OrganizationName]
	OFFSET @recordsPerPage * (@pageNumber - 1)
	ROWS FETCH NEXT @recordsPerPage ROWS ONLY;
RETURN 0
