-- ================================================
-- RateLimitApi Database Schema (with SubscriptionTier)
-- ================================================

IF DB_ID('RateLimitDemo') IS NULL
    CREATE DATABASE RateLimitDemo;
GO

USE RateLimitDemo;
GO

-- ================================
-- API Keys
-- ================================
IF OBJECT_ID('dbo.ApiKeys', 'U') IS NOT NULL DROP TABLE dbo.ApiKeys;
GO

CREATE TABLE dbo.ApiKeys (
    ApiKey           NVARCHAR(64)  NOT NULL PRIMARY KEY,
    UserId           UNIQUEIDENTIFIER NOT NULL,
    SubscriptionTier NVARCHAR(16)  NOT NULL CHECK (SubscriptionTier IN ('Free','Basic','Pro')),
    IsActive         BIT NOT NULL DEFAULT 1,
    CreatedAtUtc     DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

-- ================================
-- Daily Usage
-- ================================
IF OBJECT_ID('dbo.DailyUsage', 'U') IS NOT NULL DROP TABLE dbo.DailyUsage;
GO

CREATE TABLE dbo.DailyUsage (
    UsageDate      DATE NOT NULL,
    ApiKey         NVARCHAR(64) NOT NULL,
    IP             VARCHAR(45) NOT NULL,
    Count          INT NOT NULL DEFAULT 0,
    PRIMARY KEY (UsageDate, ApiKey, IP)
);

-- ================================
-- Blocks
-- ================================
IF OBJECT_ID('dbo.Blocks', 'U') IS NOT NULL DROP TABLE dbo.Blocks;
GO

CREATE TABLE dbo.Blocks (
    Id              BIGINT IDENTITY PRIMARY KEY,
    ApiKey          NVARCHAR(64) NULL,
    IP              VARCHAR(45) NULL,
    Reason          NVARCHAR(256) NOT NULL,
    BlockedUntilUtc DATETIME2 NOT NULL,
    CreatedAtUtc    DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ClearedAtUtc    DATETIME2 NULL
);

CREATE INDEX IX_Blocks_Active 
    ON dbo.Blocks (ApiKey, IP, BlockedUntilUtc) 
    INCLUDE (Reason, CreatedAtUtc) 
    WHERE ClearedAtUtc IS NULL;

-- ================================
-- Request Log
-- ================================
IF OBJECT_ID('dbo.RequestLog', 'U') IS NOT NULL DROP TABLE dbo.RequestLog;
GO

CREATE TABLE dbo.RequestLog (
    Id             BIGINT IDENTITY PRIMARY KEY,
    AtUtc          DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ApiKey         NVARCHAR(64) NOT NULL,
    IP             VARCHAR(45) NOT NULL,
    Path           NVARCHAR(256) NOT NULL,
    StatusCode     INT NOT NULL,
    Note           NVARCHAR(256) NULL
);

CREATE INDEX IX_RequestLog_KeyIpTime ON dbo.RequestLog(ApiKey, IP, AtUtc);
CREATE INDEX IX_RequestLog_IpTime ON dbo.RequestLog(IP, AtUtc);

-- ================================
-- View: Active Blocks
-- ================================
IF OBJECT_ID('dbo.vActiveBlocks', 'V') IS NOT NULL DROP VIEW dbo.vActiveBlocks;
GO

CREATE VIEW dbo.vActiveBlocks AS
SELECT Id, ApiKey, IP, Reason, BlockedUntilUtc, CreatedAtUtc
FROM dbo.Blocks
WHERE ClearedAtUtc IS NULL
  AND BlockedUntilUtc > SYSUTCDATETIME();
GO

-- ================================
-- Seed Sample Keys
-- ================================
INSERT INTO dbo.ApiKeys (ApiKey, UserId, SubscriptionTier) VALUES
('FREE-KEY-1', NEWID(), 'Free'),
('BASIC-KEY-1', NEWID(), 'Basic'),
('PRO-KEY-1', NEWID(), 'Pro');
GO
