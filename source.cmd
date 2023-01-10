@echo off
cls

dotnet tool restore
dotnet paket restore
dotnet paket install
dotnet restore Src
dotnet restore Tests
dotnet restore Examples/aspnetcore/common
dotnet restore Examples/aspnetcore/backend
dotnet restore Examples/aspnetcore/frontend
dotnet pack Src  --configuration Release

MD source
COPY Src/bin/Release/*.nupkg source
