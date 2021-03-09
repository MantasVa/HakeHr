﻿CREATE TABLE [dbo].[Status]
(
	[Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[Type] NVARCHAR(20) UNIQUE NOT NULL ,
	[CreatedAt] SMALLDATETIME NOT NULL DEFAULT(GETDATE()),
	[UpdatedAt] SMALLDATETIME NULL
)