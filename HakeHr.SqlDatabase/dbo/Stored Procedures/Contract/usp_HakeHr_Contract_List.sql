CREATE PROCEDURE [dbo].[usp_HakeHr_Contract_List]
	@recordsPerPage INT,
	@pageNumber INT
AS
	SET @recordsPerPage = ISNULL(@recordsPerPage, 2147483647);
	SET @pageNumber = ISNULL(@pageNumber, 1);

	SELECT [Id],
		   [Salary],
		   [StartDate],
		   [ExpireDate],
		   [StatusId]
	FROM [Contract]
	ORDER BY [StartDate]
	OFFSET @recordsPerPage * (@pageNumber - 1)
	ROWS FETCH NEXT @recordsPerPage ROWS ONLY;
GO;