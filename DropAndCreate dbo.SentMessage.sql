USE [Soccer]
GO

/****** Object: Table [dbo].[SentMessage] Script Date: 12/3/2016 5:09:16 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

DROP TABLE [dbo].[SentMessage];


GO
CREATE TABLE [dbo].[SentMessage] (
    [SentMessageId] INT           IDENTITY (1, 1) PRIMARY KEY NOT NULL,
    [PlayerId]      INT          NOT NULL,
    [GameId]        INT          NOT NULL,
    [MessageBody]   VARCHAR (MAX) NULL,
    [Sender]        VARCHAR (100) NULL,
    [SentTimeStamp] DATETIME      NULL,
    [Response]      VARCHAR (255) NULL,
    [Updated]       DATETIME      NULL,
	CONSTRAINT fk_Player FOREIGN KEY (PlayerId) REFERENCES Player(PlayerId),
	CONSTRAINT fk_Game FOREIGN KEY (GameId) REFERENCES Game(GameId)
);


