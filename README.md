# Aspire Onboarding and Documentation Talk

# Aspire readme
## Requirements  
- Docker
- dotnet cli
- nodejs 22

## How to run  
- `dotnet run AspireProj.AppHost/AspireProj.AppHost.csproj`
or F5 in Visual Studio/Rider
- Use buttons to create/seed db and run apps


-----------------------------------------------------------------------------


# Non-Aspire readme
## Requirements  
- Docker (note if you have podman, and you have it aliased, you might need to adjust the ResetTheWorld.ps1 to use docker instead of podman)
- dotnet cli
- nodejs

### If not using docker  
- Sql server
- Postgres 
- Seq
- SSMS (or dbgate)
- pgadmin

## Setup
- DbUp projects create database (add --dropdb to drop an existing db)  
- Seed projects create data in databases  

## How to run  
- F5 App1 and/or App2
- npm run dev App1.Frontend and/or App2.Frontend