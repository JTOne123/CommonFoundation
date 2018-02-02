﻿IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppVersion]') AND type in (N'U'))
    DROP TABLE [dbo].[AppVersion]
GO

CREATE TABLE [dbo].[AppVersion](
    [RowId] INT NOT NULL IDENTITY(1,1),
    [Key] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(), 
    [PlatformKey] UNIQUEIDENTIFIER NOT NULL,
    [LatestBuild] INT NOT NULL,
    [LatestVersion] NVARCHAR(64) NULL,
    [MinRequiredBuild] INT NOT NULL,
    [Note] [NVARCHAR](MAX) NULL,
    [AppServiceStatus] INT NULL,
    [CreatedStamp] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [LastUpdatedStamp] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [LastUpdatedBy] UNIQUEIDENTIFIER NULL,
    [State] [int] NOT NULL DEFAULT 0,
CONSTRAINT [PK_AppVersion_Key] PRIMARY KEY NONCLUSTERED 
(
    [Key] ASC
),
CONSTRAINT [CIX_AppVersion] UNIQUE CLUSTERED 
(
    [RowId] ASC
)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
);

GO