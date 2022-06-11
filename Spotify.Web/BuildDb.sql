use SteamRoller
go

-- Execute SQL without throwing an error.
create or alter procedure dbo.DoTry @SqlCode varchar(max)
as
begin
	begin try
		exec(@SqlCode)
	end try
	begin catch
		print(error_message())
	end catch
end
go

-- Drop existing tables. This will also reset the identity counter.
exec DoTry 'drop table Spotify.Groups'
exec DoTry 'drop schema Spotify'
go


-- Recreate tables.
create schema Spotify
authorization dbo
go

create table Spotify.Groups
(
	GroupId int identity(0, 500) not null,
	
	Username nvarchar(100) not null,
	GroupName nvarchar(100) not null,
	
	GroupDescription nvarchar(1000) null,
	
	constraint PK_UsernameGroupName primary key (Username, GroupName)
)

-- This should only be run against a development database. 
-- The production database should be populated as it is used through the application
declare @Username nvarchar(500) = 'jacobapoirier9'
insert into Spotify.Groups (Username, GroupName, GroupDescription)
values
	(@Username, 'Road Trip', 'Test'),
	(@Username, 'Funny', null),
	(@Username, 'Childhood', null),
	(@Username, 'Chill', null)