# FlappyBirdUnityTutorial
Following the Game Makers Toolkit tutorial (https://www.youtube.com/watch?v=XtQMytORBmM) to learn Unity and game development.

## Assets
### Sound Assets
- [RPG Essentals SFX Free](https://leohpaz.itch.io/rpg-essentials-sfx-free)
  - On flap: `30_Jump_03.wav`
  - On death: `52_Dive_02.wav`
  - On gain point: `001_Hover_01.wav`
  - On break record: `013_Confirm_03.wav`
  
### Background music
- [Beach - by Sakura Girl](https://soundcloud.com/sakuragirl_official/beach)

## Building and Running locally
### Requirements
- [Unity](https://unity.com/releases/editor/archive) v2022.3.21f1
- [Visual Studio](https://visualstudio.microsoft.com/vs/community/) v17.9.2
	- With "Game development with Unity" extension
- [Python](https://www.python.org/downloads/) v3.12.2

### Windows (manual)
1. From repository root, open `GameMakersToolkitFlappyBirdTutorial` folder.
2. File > Build Settings
3. Ensure Platform `Windows, Mac, Linux` is active.
	- If not, click the `Switch Platform` button in the lower right corner and wait for the switch to finish.
4. Click the 'Build and Run' button.
5. When asked, determine the location to build
	- Recommendation: `<repo root>\Builds\win64` for an `Windows` target platform and `Intel 64-bit` architecture
6. Wait for the build to complete. The game will start automatically after.

### HTML 5 (manual)
1. From repository root, open `GameMakersToolkitFlappyBirdTutorial` folder.
2. File > Build Settings
3. Ensure Platform `WebGL` is active.
	- If not, click the `Switch Platform` button in the lower right corner and wait for the switch to finish.
4. Click the 'Build' button.
5. When asked, determine the location to build
	- Recommendation: `<repo root>\Builds\html5`
6. Wait for the build to complete.
7. Navigate into the build folder.
8. Open a command line in that folder.
9. Run the command: `python -m http.server`
10. Open a browser. Naviate to `localhost:8000` (assuming 8000 portnumber; Specific port number is mentioned in the command line window). Game is now started.