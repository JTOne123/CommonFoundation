﻿IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AppProvisioning]') AND type in (N'U'))
    DROP TABLE [dbo].[AppProvisioning]
GO

CREATE TABLE [dbo].[AppProvisioning](
    [RowId] INT NOT NULL IDENTITY(1,1),
    [Key] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(), 
    [SnapshotKey] UNIQUEIDENTIFIER NOT NULL,
    [PlatformKey] UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(256) NOT NULL DEFAULT '',
    [CreatedStamp] DATETIME NOT NULL DEFAULT GETUTCDATE(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
CONSTRAINT [PK_AppProvisioning_Key] PRIMARY KEY NONCLUSTERED 
(
    [Key] ASC
),
CONSTRAINT [CIX_AppProvisioning] UNIQUE CLUSTERED 
(
    [RowId] ASC
)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
);

GO
