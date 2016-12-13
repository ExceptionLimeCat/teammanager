ALTER TABLE Game
ADD SendMail bit

ALTER TABLE Player
ADD IsActive bit

ALTER TABLE Game
DROP COLUMN SendMail

ALTER TABLE Game
ADD SendEmail bit not null

select * from Game
select * from Player