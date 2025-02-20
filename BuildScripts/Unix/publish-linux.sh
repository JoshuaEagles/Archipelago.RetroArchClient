#!/bin/sh
# Creates a self contained build in /bin/Release/net8.0/linux-x64/publish/
BASEDIR=$(dirname $0)
echo "Script location: ${BASEDIR}"
echo "Current working directory: ${PWD}"
dotnet publish ../Archipelago.RetroArchClient.csproj --configuration Release --self-contained -r linux-x64
