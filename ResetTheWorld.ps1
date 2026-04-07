Write-Host "Setup docker containers..."
Write-Host "Pulling..."
docker pull postgres
docker pull mcr.microsoft.com/mssql/server:2025-latest

Write-Host "Spinning up the containers..."
docker compose up -d 
# Note we aren't waiting for things to be up, but we build first, should be enough time.

Write-Host "Building dotnet..."
dotnet build

Write-Host "Building node..."
Push-Location
Set-Location .\App1\App1.Frontend
npm ci
npm run build
Pop-Location

Push-Location
Set-Location .\App2\App2.Frontend
npm ci
npm run build
Pop-Location

# NOTE A better method would be to ask whether you wanted to drop the DB, but this is reset mate!
Push-Location
Write-Host "Drop and rebuild databases..."
Set-Location .\App1\App1DbUp
dotnet run --dropdb
Pop-Location

Push-Location
Set-Location .\App2\App2DbUp
dotnet run --dropdb
Pop-Location

Write-Host "Seeding databases..."
Push-Location
Set-Location .\App1\App1Seed
dotnet run
Pop-Location

Push-Location
Set-Location .\App2\App2Seed
dotnet run
Pop-Location

Write-Host "Finished!"