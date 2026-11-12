# Windows Arrangement Manager

Windows Arrangement Manager (formerly TAN Window Manager) is a powerful, all-in-one utility for managing your multi-monitor setup and window layouts. 

Whether you are dealing with windows jumping to the wrong screen after waking from sleep, or you need to quickly switch between complex monitor arrangements (like a gaming setup vs. a work setup), this tool handles it all. It runs quietly in your system tray and ties both your **window positions** and your **physical display configurations** into unified, easy-to-load profiles.

## 🌟 Features

### 💻 Window Management
* **Save & Restore Window Positions:** Save the exact size, position, and state of your open applications.
* **Auto-Restore on Launch:** Set a profile as your "Auto Load" profile. When you open an application, it will instantly snap to its saved position (until you manually move it).
* **Multi-Profile Support:** Create different profiles for different tasks (e.g., "Work", "Gaming", "Media").
* **System Tray Integration:** Quick access to load profiles directly from the system tray menu.

### 🖥️ Display Management
* **Visual Topology Map:** Drag and drop monitors in the visual map to rearrange your physical displays. 
* **Smart Snapping & Overlap Prevention:** Monitors snap together magnetically like in Windows Settings. The app strictly prevents monitors from overlapping in the virtual space (hold `Shift` to bypass snapping for precise placement).
* **Undo/Reset Layout:** Accidentally clicked Apply? Click the "Reset Layout" button to instantly revert your monitors to their previous physical arrangement.
* **Zoom & Pan:** Hold `Ctrl` and scroll to zoom in/out of the monitor map. Click and drag the empty background to pan around.
* **Identify Monitors:** Click the "Identify" button to flash a large number, resolution, and coordinates on your physical screens to easily see which is which.
* **Change Resolution, Refresh Rate & Orientation:** Click on any monitor's stats in the list to instantly change its resolution, Hz, or orientation via dropdown menus.
* **Safety Confirmation:** When changing display settings, a 15-second countdown popup appears allowing you to "Keep changes" or "Revert" (exactly like Windows Display Settings).

### 🎨 Modern UI & System Integration
* **Full Dark Mode Support:** The app automatically detects your Windows Light/Dark theme. It applies dark mode to the main window, the title bar, the scrollbars, the system tray menu, and all custom popups.
* **Custom Popups:** All text inputs and confirmation messages are handled by sleek, dark-themed custom dialogs.
* **Dynamic Layout:** The UI scales beautifully. Columns stretch to fit text perfectly, and the topology map resizes with the window.
* **Start Up Toggle:** A simple checkbox in the app automatically registers it to Windows startup, so your layout is ready the moment you log in.

## 🚀 Getting Started

### Run the Pre-Built Binary (Recommended)

1. Go to the **Releases** section (or build it yourself) and download `WindowsArrangementManager.exe` to a folder on your computer. 
2. *Note: A `Settings.ini` file will be automatically created in the same directory. If you move the `.exe`, move the `Settings.ini` with it.*
3. Open the app, arrange your windows and monitors, and click **Save Selected**. Make sure to check the **Start Up** box so it runs when you turn on your PC!
4. Use the tray icon to quickly load your profiles at any time!

### 🛠️ Build it Yourself

1. Download **Visual Studio Community** (2019 or 2022).
2. During installation, select the **.NET desktop development** workload.
3. Open the `WindowsArrangementManager.sln`.
4. Build | Build Solution or `Ctrl+Shift+B`.
5. The `.exe` file will be created in `bin\Debug\` or `bin\Release\`.

## 📂 How Profiles Work
Everything is saved inside a single `Settings.ini` file. 
* **Windows** are saved under their process name (e.g., `notepad,100,100,500,500,1,Default`).
* **Displays** are saved under the `Display` header (e.g., `Display,\\.\DISPLAY1,1920,1080,0,0,0,144,Default`).
* **Auto Load** is saved at the very top of the file.
You can open `Settings.ini` in Notepad at any time to manually adjust values or back them up.

## ⌨️ Command Line Arguments

`-saveall`   Save all open windows to the configuration file under the `Default` profile.  
`-restore`  Restore layout for the `Default` profile from the configuration file.  
`-restore=<Name>`  Restore layout for the `<Name>` profile from the configuration file. Use quotes if the profile name has spaces.  
`-autoexit`  Don't keep the app active in the tray. Useful with the above switches.

### Examples
`WindowsArrangementManager.exe -saveall -autoexit`  
`WindowsArrangementManager.exe -restore -autoexit`  
`WindowsArrangementManager.exe -restore="My Home Layout" -autoexit`

## ⚠️ Known Limitations
1. **UWP/Metro Apps:** Windows 11 apps (like Calculator, Settings, or modern Notepad) use complex internal window wrappers. The app can still manage them, but occasionally Windows overrides the window placement.
2. **Multiple Instances:** If you have multiple instances of the exact same program open (e.g., 3 separate Notepad windows), the app will only apply settings to the main window.
3. **Display Overlaps:** Windows OS strictly forbids monitors from physically overlapping in the virtual desktop space. If you attempt to overlap them, the app (and Windows) will automatically snap them to the closest valid edge.

## 👨‍💻 Credits
Originally created by **Todd Nelson** (https://toddnelson.net). 
Heavily modified and expanded into the Windows Arrangement Manager.
