# MOPMAN

An asymmetric two-player cooperative VR game built with Unity and Ubiq for the Immersive Technologies course (A.Y. 2025–2026).

## Group Members

| Member | Role | Main Contributions |
| --- | --- | --- |
| Bergström Ella | Developer | Project Setup, Coins |
| Colosimo Davide | Developer | Second Floor, Communication, Player Motion |
| Facchinelli Davide | Project Manager | Project Setup, General Polishment, Sounds & Music |
| Filippo Marcon | Developer | Keys, Graphics & Models |
| Nejati Zadeh Daryaei Sam | Developer | Mop, End Game |
| Stevan Pietro | Developer | Monsters, GUI |

## Game Overview

Traditional cooperative VR games often rely on static player roles and flawless communication. MOPMAN subverts these assumptions by introducing deliberate physical and sensory limitations.

The core experience is built on an asymmetric dependency loop: one player acts entirely from a ground-level perspective under constant threat, while the other holds a "god-view" that is constantly deteriorating. Communication is actively degraded by proximity limits and *Monster* proximity, forcing players to adapt under pressure.

### Game Structure & Organization
The game is divided in 3 spatial areas:
   1. **The Lobby**: The starting area where players firstly spawn when entering the game.\
   Here networking is established, instruction to play the game are given through posters and role-selection pads appear. Once the connection via Ubiq is established, it is possible to use the pads to teleport in the main playing area.

   2. **The Maze (Ground Floor)**: A dark, claustrophobic labyrinth where the core objective takes place.

   3. **The Observation Deck (Second Foor)**: A glass-floored platform suspended directly above the maze, granting a top-down view of the labyrinth below.


### Player Roles & Dynamics
- **The Executive**
   - Objective: Explore the dangerous maze to collect the three hidden *Keys* that are needed to open the *Exit Door* and safely reach the escape area.\
   As an additional job the *Executive* also has to collect the *Coins* that are going to be used from the *Coordinator*. 

   - Threats: Must constantly evade an autonomous *Monster* roaming the halls.
- **The Coordinator**
   - Objective: Act as the "eyes in the sky" to guide the *Executive* through the maze, pinpointing key locations and warning them of the *Monster*'s path.

   - The Twist: The glass floor steadily accumulates grime and dirt, progressively blinding the *Coordinator*. The *Coordinator* must physically use a mop to clean the glass floor to maintain visibility. They can also buy mop upgrades using *Coins* collected by the Executive to improve cleaning efficiency.

   The *Executive* and the *Coordinator* have a shared pool of 3 lives.

### Communication Degradation Mechanics
- Spatialized Voice: The game uses a 2D proximity-based audio system that ignores vertical distance. If the *Executive* moves too far away horizontally, voice chat fades.

- Signal Jamming: When the *Monster* gets close to the *Executive*, the communication channel becomes heavily distorted, eventually leading to total signal loss right when the *Executive* is in the most danger.

### Edgame 
   - **Win** → both players return to the lobby with all *Keys* collected.
   - **Lose** → the executive loses all three lives to the *Monster*; both players are sent back to the lobby.
   - A "Play Again" sign then lets either player restart the match for both.

## Gameplay Demo
<div align="center">
  <a href="https://youtu.be/uUIWcZobgbw">
    <img src="https://img.youtube.com/vi/uUIWcZobgbw/maxresdefault.jpg" alt="Watch the video" style="width: 80%; height: auto;">
  </a>
</div>

## Requirements

- **Unity**: 6000.3.11f1;
- **Git LFS**: to manage large assets.
   ```bash
   # Install the git extension
   git lfs install
   # Let the extension know which large files to track
   git lfs track <path-to-file>  
   ```
   If you already worked on this repository and you see broken files, then launch the following:
   ```bash
   git lfs install
   git lfs pull
   ```
- **Ubiq**: `com.ucl.ubiq` (Installed via Unity Package Manager);
- **SideQuest**: Required to sideload the compiled .apk onto standalone VR headsets;
- **Hardware**: An OpenXR-compatible VR headset (e.g., Meta Quest series);

## Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/unitn-IT-group/MopMan.git
   ```
2. Open the project in Unity **6000.3.11f1** via Unity Hub. Required packages are restored automatically from `Packages/manifest.json`.
3. Open the main scene: `Assets/MopMan/Scenes/Maze.unity`.

## Build the Project

To generate a standalone build for the Meta Quest or other Android-based VR headsets:
   1. Open **File ➔ Build Settings**;
   2. Switch the platform to Android.
   3. Ensure the texture compression is set appropriately (ASTC is standard for Quest).
   4. Click Build to generate your .apk file.\
   Alternatively, the following [.apk](apks/ANDROID/MopMan.apk) can be directly used.
   5. Install it using SideQuest. 

To generate a executable for WINDOWS (for testing purposes) you can follow the same instruction making sure that in point *2.* you set Windows as platform instead of Android.


