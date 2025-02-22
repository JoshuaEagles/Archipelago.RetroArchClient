# Archipelago.RetroArchClient

This project currently serves as a client that allows people to play Ocarina of Time on Archipelago (https://archipelago.gg) using RetroArch rather than the officially supported emulator, Bizhawk.

At the moment there is only a command line UI, so it must be run from a terminal. It also requires my version of the mupen64plus-nx core in order to have support for reading and writing the memory of N64 games on RetroArch. See the PR for my version of the core here: https://github.com/libretro/mupen64plus-libretro-nx/pull/545.

For setup help if needed, see the `#Retroarch OOT Client` thread on the Archipelago discord.

## Build Instructions (Dev)

Requires the .NET 8 SDK.

Simply run `dotnet run` in the directory with the solution while having the .NET 8 SDK installed. Can also use an IDE like Rider to build and run the project.

## Build Instructions (Release)

### Windows  
TODO

### Linux  
These commands should be run from the directory with the .sln file. The output of the commands contains the directory that the built

Requires the .NET 8 SDK.

#### Build Linux version
``cd BuildScripts/Unix && ./publish-linux.sh``

#### Build Windows version
``cd BuildScripts/Unix && ./publish-win.sh``
