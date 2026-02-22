# Setup Guide

**Note: Make sure to save often.**

## 1. Retroarch
1. [Download retroarch](https://www.retroarch.com/?page=platforms) - Grab the latest release from the official site. The Steam build is currently unsupported. The Linux flatpak or AppImage versions work fine on most distributions.
1. **Install / load the core**

   1. Open **Install/Load Core** -> **Download Core**.  
   1. Navigate to **Nintendo - Nintendo 64 (Mupen64Plus‑Next)** and download it.

1. Return to the RetroArch main menu
1. **Enable network commands & QoL settings**

   1. Choose **Settings** -> **User Interface**  
      - `Show Advanced Settings`: **ON**  
      - `Pause Content When Menu Is Active`: **OFF**  
      - `Pause Content When Not Active`: **OFF**
   1. Back to **Settings** -> **Network**  
      - `Network Commands`: **ON** *(near the bottom of the list)*
1. Save your Configuration
   - **Main Menu** -> **Configuration File** -> "Save Current Configuration"

---

## 2. ROM Generation

1. Generate a seed and download the resulting `.apz5` file.
1. Extract the .apz5 with Ark or other extraction tool on Linux, 7zip on Windows
1. **Patch your ROM** - you need the vanilla *Ocarina of Time* v1.0 ROM.
1. Randomizer generator: [Ocarina of Time Randomizer (v7.1.0)](https://ootrandomizer.com/generator?version=7.1.0)
    - `Generate From Patch File` option (near the bottom of the page)
    - `Override Original Cosmetics` optionally un check this unless you specify new cosmetics on this page

1. **Launch the patched ROM** in RetroArch. The title screen should display "Randomizer".

---

## 3. Client

1. Download the latest release of the client:  
   <https://github.com/JoshuaEagles/Archipelago.RetroArchClient/releases/latest>
1. Extract the archive.
1. Open a terminal (Linux & Mac) in the extracted directory and run:

   ```bash 
   ./Archipelago.RetroArchClient
   ```


## 4. Notes
If the client disconnects you may need to restart the client to get it to reconnect.
Improvements to the client are planned and in development.

