CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS "Users" (
    "Id" uuid PRIMARY KEY,
    "Name" varchar(120) NOT NULL,
    "Email" varchar(320) NOT NULL UNIQUE,
    "PasswordHash" text NOT NULL,
    "EmailConfirmed" boolean NOT NULL DEFAULT false,
    "CreatedAtUtc" timestamptz NOT NULL,
    "LastLoginAtUtc" timestamptz NULL
);

CREATE TABLE IF NOT EXISTS "Accounts" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL,
    "Institution" varchar(100) NOT NULL,
    "Name" varchar(100) NOT NULL,
    "Type" integer NOT NULL,
    "Balance" numeric(18,2) NOT NULL,
    "YieldType" integer NOT NULL,
    "YieldPercentage" numeric(8,4) NULL
);
CREATE INDEX IF NOT EXISTS "IX_Accounts_UserId" ON "Accounts" ("UserId");

CREATE TABLE IF NOT EXISTS "RefreshTokens" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL,
    "TokenHash" varchar(128) NOT NULL UNIQUE,
    "ExpiresAtUtc" timestamptz NOT NULL,
    "CreatedAtUtc" timestamptz NOT NULL,
    "RevokedAtUtc" timestamptz NULL
);
CREATE INDEX IF NOT EXISTS "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");

CREATE TABLE IF NOT EXISTS "AuthTokens" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL,
    "Type" integer NOT NULL,
    "TokenHash" varchar(128) NOT NULL UNIQUE,
    "ExpiresAtUtc" timestamptz NOT NULL,
    "CreatedAtUtc" timestamptz NOT NULL,
    "UsedAtUtc" timestamptz NULL
);
CREATE INDEX IF NOT EXISTS "IX_AuthTokens_UserId_Type" ON "AuthTokens" ("UserId", "Type");
