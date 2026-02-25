-- Helpdesk Database
IF DB_ID('HelpdeskDb') IS NULL
BEGIN
    CREATE DATABASE HelpdeskDb;
END
GO

USE HelpdeskDb;
GO

SET NOCOUNT ON;

-- Users table
CREATE TABLE dbo.Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    PasswordHash NVARCHAR(256) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT(1),
    CreatedDate DATETIME NOT NULL CONSTRAINT DF_Users_CreatedDate DEFAULT(GETDATE())
);

-- unique email
ALTER TABLE dbo.Users
ADD CONSTRAINT UQ_Users_Email UNIQUE(Email);

-- Categories table
CREATE TABLE dbo.Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Categories_IsActive DEFAULT(1),
    CreatedDate DATETIME NOT NULL CONSTRAINT DF_Categories_CreatedDate DEFAULT(GETDATE())
);

ALTER TABLE dbo.Categories
ADD CONSTRAINT UQ_Categories_Name UNIQUE(Name);

-- Tickets table
CREATE TABLE dbo.Tickets (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    CategoryId INT NOT NULL,
    CreatedBy INT NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CreatedDate DATETIME NOT NULL CONSTRAINT DF_Tickets_CreatedDate DEFAULT(GETDATE()),
    IsDeleted BIT NOT NULL CONSTRAINT DF_Tickets_IsDeleted DEFAULT(0),
    CONSTRAINT CK_Tickets_Status CHECK (Status IN ('Open','InProgress','Closed'))
);

-- foreign keys for Tickets
ALTER TABLE dbo.Tickets
ADD CONSTRAINT FK_Tickets_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id);

ALTER TABLE dbo.Tickets
ADD CONSTRAINT FK_Tickets_Users_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(Id);

-- TicketComments table
CREATE TABLE dbo.TicketComments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TicketId INT NOT NULL,
    CommentText NVARCHAR(MAX) NOT NULL,
    CreatedByU INT NOT NULL,
    CreatedDate DATETIME NOT NULL CONSTRAINT DF_TicketComments_CreatedDate DEFAULT(GETDATE())
);

ALTER TABLE dbo.TicketComments
ADD CONSTRAINT FK_TicketComments_Tickets FOREIGN KEY (TicketId) REFERENCES dbo.Tickets(Id);

ALTER TABLE dbo.TicketComments
ADD CONSTRAINT FK_TicketComments_Users FOREIGN KEY (CreatedByU) REFERENCES dbo.Users(Id);

-- Seed data

INSERT INTO dbo.Users (FullName, Email, PasswordHash, IsActive)
VALUES
('Admin User', 'admin@helpdesk.com', 'Admin123!', 1),
('Inactive User', 'inactive@helpdesk.com', 'User123!', 0);

-- Categories
INSERT INTO dbo.Categories (Name, IsActive)
VALUES
('IT Support', 1),
('Software Bug', 1),
('HR Request', 1),
('Facilities', 1),
('Network', 1);

-- Tickets
INSERT INTO dbo.Tickets (Title, Description, CategoryId, CreatedBy, Status)
VALUES
('Cannot access VPN', 'Unable to connect to company VPN from home', 1, 1, 'Open'),
('App crashes on save', 'When saving changes the app crashes with error code 500', 2, 1, 'InProgress'),
('Request for new keyboard', 'Ergonomic keyboard requested', 4, 2, 'Open'),
('Email not syncing', 'Mobile email not syncing since update', 5, 1, 'Closed');

-- Comments
INSERT INTO dbo.TicketComments (TicketId, CommentText, CreatedByU)
VALUES
(1, 'Please confirm your public IP and try again', 1),
(2, 'We are investigating the stack trace', 1),
(4, 'Issue resolved after mailbox reset', 1);

GO

PRINT 'Database schema and seed data created successfully';
