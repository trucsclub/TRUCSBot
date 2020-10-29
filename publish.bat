@echo off
dotnet publish --self-contained true -p:PublishTrimmed=true --runtime linux-x64 --output bin/publish -c Release