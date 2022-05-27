use SteamRoller
go

-- Execute SQL without throwing an error
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

