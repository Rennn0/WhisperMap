IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF SCHEMA_ID(N'realtime') IS NULL EXEC(N'CREATE SCHEMA [realtime];');
GO

IF SCHEMA_ID(N'xaticraft') IS NULL EXEC(N'CREATE SCHEMA [xaticraft];');
GO

IF SCHEMA_ID(N'cache') IS NULL EXEC(N'CREATE SCHEMA [cache];');
GO

CREATE TABLE [dbo].[Log] (
    [Id] int NOT NULL IDENTITY,
    [EventId] int NOT NULL,
    [Level] int NOT NULL,
    [Message] nvarchar(max) NULL,
    [Name] nvarchar(255) NULL,
    [TimeStamp] datetimeoffset NOT NULL,
    CONSTRAINT [PK_Log] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [realtime].[Log] (
    [Id] int NOT NULL IDENTITY,
    [EventId] int NOT NULL,
    [Level] int NOT NULL,
    [Message] nvarchar(max) NULL,
    [Name] nvarchar(255) NULL,
    [TimeStamp] datetimeoffset NOT NULL,
    CONSTRAINT [PK_Log] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [xaticraft].[Log] (
    [Id] int NOT NULL IDENTITY,
    [EventId] int NOT NULL,
    [Level] int NOT NULL,
    [Message] nvarchar(max) NULL,
    [Name] nvarchar(255) NULL,
    [TimeStamp] datetimeoffset NOT NULL,
    CONSTRAINT [PK_Log] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [cache].[RealtimeCache] (
    [Id] nvarchar(449) COLLATE SQL_Latin1_General_CP1_CS_AS NOT NULL,
    [Value] varbinary(max) NOT NULL,
    [ExpiresAtTime] datetimeoffset NOT NULL,
    [SlidingExpirationInSeconds] bigint NULL,
    [AbsoluteExpiration] datetimeoffset NULL,
    CONSTRAINT [PK__Realtime__3214EC077108BA1A] PRIMARY KEY ([Id])
);
GO

CREATE INDEX [Index_ExpiresAtTime] ON [cache].[RealtimeCache] ([ExpiresAtTime]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260412175233_Initial', N'8.0.22');
GO

COMMIT;
GO

