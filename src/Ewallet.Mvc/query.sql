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


CREATE TABLE dbo.Wallets(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL UNIQUE,
    WalletNumber NVARCHAR(20) NOT NULL UNIQUE,
    PINHash NVARCHAR(255) NOT NULL,
    Balance DECIMAL(18,2) NOT NULL DEFAULT 0 CHECK (Balance>=0),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    Updated DATETIME2 NULL DEFAULT GETDATE()

    CONSTRAINT FK_Wallet_Users FOREIGN KEY (Userid) REFERENCES dbo.Users(Id)
)
GO

INSERT INTO dbo.Users (Username, PasswordHash, Email, Fullname, Phone, Photo) VALUES
('danymz', 'password_dany', 'dany@gmail.com', 'Dany Zakariya', '892736478329', null);

INSERT INTO dbo.Wallets(UserId,WalletNumber,PINHash,Balance) VALUES
(5,'11269012','123456',0)
GO 

SELECT u.Fullname, u.Email, w.WalletNumber, w.Balance, w.CreatedAt
FROM Wallets w
JOIN Users u ON w.UserId=u.Id
WHERE u.Id = 12
GO

select * From dbo.Users;
select * From dbo.Wallets;
GO

-- *TEST: insert with output into in one query
-- ref: https://stackoverflow.com/questions/49675059/insert-with-id-from-previous-insert

DECLARE @InsertedUsers TABLE (Id INT);
INSERT INTO dbo.Users (Username, PasswordHash, Email, Fullname, Phone, Photo)
OUTPUT INSERTED.Id INTO @InsertedUsers
VALUES ('ayudiarhm', 'password_ayu', 'dewi@gmail.com', 'Ayudia Rahma', '89783472389', 'https://cdn.linkpotodewi.jpg');

INSERT INTO dbo.Wallets (UserId, WalletNumber, PINHash, Balance)
SELECT 
    u.Id, 
    '28913819', 
    '823812', 
    150000
FROM @InsertedUsers u;
-- SUccess