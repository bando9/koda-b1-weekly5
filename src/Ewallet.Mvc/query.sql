CREATE DATABASE ewalletdb;
GO

USE ewalletdb;
GO

Select * FROM sys.tables;
Go

CREATE TABLE dbo.Users(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Fullname NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NOT NULL,
    Photo NVARCHAR(255) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated DATETIME2 NULL DEFAULT GETDATE()
);
GO

INSERT INTO dbo.Users (Username, PasswordHash, Email, Fullname, Phone, Photo) VALUES
('bandomega', 'password_bando', 'bando@gmail.com', 'Bando Mega', '89677291389', 'link_photo-profile');
GO

select Id, Username, Fullname, Email From dbo.Users;
GO