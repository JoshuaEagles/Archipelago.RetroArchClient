#!/bin/sh
# Creates a self contained build in /bin/Release/net8.0/linux-x64/publish/
dotnet publish ../../Archipelago.RetroArchClient/Archipelago.RetroArchClient.csproj --configuration Release --self-contained -r linux-x64
