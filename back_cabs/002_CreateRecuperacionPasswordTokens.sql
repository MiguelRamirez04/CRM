-- Migration: 002 - Create RecuperacionPasswordTokens table
-- Description: Creates the table to store password recovery tokens
-- Date: 2025-11-13

USE cabs_pruebas;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RecuperacionPasswordTokens' AND xtype='U')
BEGIN
    CREATE TABLE RecuperacionPasswordTokens (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Email NVARCHAR(255) NOT NULL,
        Token NVARCHAR(10) NOT NULL, -- Para tokens de 6 dígitos
        Expiracion DATETIME2 NOT NULL,
        Usado BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        INDEX IX_RecuperacionPasswordTokens_Email_Token (Email, Token),
        INDEX IX_RecuperacionPasswordTokens_Expiracion (Expiracion)
    );

    PRINT 'Table RecuperacionPasswordTokens created successfully.';
END
ELSE
BEGIN
    PRINT 'Table RecuperacionPasswordTokens already exists.';
END
GO