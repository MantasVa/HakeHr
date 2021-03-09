CREATE PROCEDURE [dbo].[usp_HakeHr_Team_List]
	@recordsPerPage INT,
	@pageNumber INT
AS
	SET @recordsPerPage = ISNULL(@recordsPerPage, 2147483647);
	SET @pageNumber = ISNULL(@pageNumber, 1);

	SELECT [Id],
		   [TeamName],
		   [PhotoPath]
	FROM [Team]
	ORDER BY [TeamName]
	OFFSET @recordsPerPage * (@pageNumber - 1)
	ROWS FETCH NEXT @recordsPerPage ROWS ONLY;
GO;
