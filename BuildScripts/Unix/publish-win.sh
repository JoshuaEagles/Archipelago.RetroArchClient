#!/bin/sh
# Creates a self contained build in /bin/Release/net8.0/win-x64/publish/
dotnet publish --configuration Release --self-contained -r win-x64
