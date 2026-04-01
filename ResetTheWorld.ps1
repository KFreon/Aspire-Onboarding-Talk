Write-Host "Building dotnet..."
dotnet build

Write-Host "Building node..."
Set-Location .\App1.Frontend
npm ci
npm run build
Set-Location ..

Set-Location .\App2.Frontend
npm ci
npm run build
Set-Location ..

# NOTE A better method would be to ask whether you wanted to drop the DB, but this is reset mate!
Write-Host "Drop and rebuild databases..."
Set-Location .\App1DbUp
dotnet run --dropdb
Set-Location ..

Set-Location .\App2DbUp
dotnet run --dropdb
Set-Location ..

Write-Host "Seeding databases..."
Set-Location .\App1Seed
dotnet run
Set-Location ..

Set-Location .\App2Seed
dotnet run

Write-Host "Finished!"