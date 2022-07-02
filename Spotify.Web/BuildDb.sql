use SteamRollerDev
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

exec DoTry 'drop table Spotify.GroupRelationship'
exec DoTry 'drop table Spotify.Groups'
--exec DoTry 'drop schema Spotify'
go

--create schema Spotify
--authorization dbo
--go


create table Spotify.Groups
(
	GroupId int identity(1, 1) not null,
	
	Username nvarchar(100) not null,
	GroupName nvarchar(100) not null,
	
	GroupDescription nvarchar(1000) null,
	
	constraint PK_UsernameGroupName primary key (Username, GroupName)
)

create table Spotify.GroupRelationship
(
	RelationshipId int identity(1, 1) not null,
	
	Username nvarchar(100) not null,
	GroupId int not null,
	
	ItemType nvarchar(20) not null,
	ItemId nvarchar(100) not null,
	
	constraint PK_UsernameGroupIdItemTypeItemId primary key (Username, GroupId, ItemType, ItemId)
)
go

create or alter view Spotify.FindGroups
as
	select
		result.Username,
		result.GroupId,
		result.GroupName,
		isnull(result.track, 0) as TrackCount,
		isnull(result.album, 0) as AlbumCount,
		isnull(result.artist, 0) as ArtistCount
	from
	(
		select 
			g.Username,
			g.GroupId,
			g.GroupName,
			gr.ItemType,
			count(distinct gr.ItemId) as ItemCount
		from Spotify.Groups g
		left join Spotify.GroupRelationship gr
			on g.GroupId = gr.GroupId
		group by g.Username, g.GroupId, g.GroupName, gr.ItemType
	) records pivot
	(
		sum(records.ItemCount) for records.ItemType in (track, album, artist)
	) result;
go

create or alter view Spotify.FindRelationships
as
	select
		gr.Username,
		gr.RelationshipId,
		g.GroupId,
		g.GroupName,
		gr.ItemType,
		gr.ItemId
	from Spotify.GroupRelationship gr
	join Spotify.Groups g
		on gr.GroupId = g.GroupId
go

declare @Username nvarchar(500) = 'jacobapoirier9'

insert into Spotify.Groups (Username, GroupName, GroupDescription)
values
	(@Username, 'Chill', null),
	(@Username, 'Rock', null),
	(@Username, 'Road Trip', null),
	(@Username, 'Rap', null)
	
insert into Spotify.GroupRelationship (Username, GroupId, ItemType, ItemId)
values
	(@Username, 1, 'track', '6ET22Ri3FYXi5C8BBqHWcO'), -- Missed Calls by Mac Miller
	(@Username, 4, 'track', '5sNESr6pQfIhL3krM8CtZn'), -- Numb / Encore
	(@Username, 4, 'album', '4lhyg7YGQagE8FT8cZBqyw'), -- Numb / Encore:  MTV Ultimate Mash-Ups Presents Collision Course
	(@Username, 4, 'artist', '3nFkdlSjzX9mRTtwJOzDYB'), -- Jay Z
	(@Username, 4, 'artist', '6XyY86QOPPrYVGvF9ch6wz') -- Linken Park
go

print('Database has been setup!')