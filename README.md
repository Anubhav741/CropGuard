CropGuard - Plant Disease Rescue

Open this folder as a project in Unity Hub using Unity 6000.5.10f1. Open Assets/Scenes/MainMenu.unity and press Play. Click Play in the menu. Move with A/D or the arrow keys, jump with Space, inspect with E, select and confirm an answer to a practical farm question, then press F to apply the solution. Five crops must be rescued. Wrong answers cost lives.

The project includes generated farm, farmer, and crop artwork. The game world and UI are assembled at runtime by Assets/Scripts/CropGuardGame.cs. Scene setup is in Assets/Editor/CropGuardSetup.cs. MainMenu and GameLevel1 are included in Build Settings. After saving all five crops in Level 1, select Next Level to enter Moonlight Rescue: save six crops before the 60-second storm timer expires. Each saved crop gives 150 points. The timer pauses while answering and while paused.

Known limitations: character is a single static sprite (no walk/run/spray animation); diseased crops use color tint rather than separate detailed disease sprites; the UI uses rectangular panels and has no illustrated disease close-up; spray has a healing particle effect but no separate directional spray effect; no audio or configurable settings are included. The complete crop-to-completion path and second-level transition need manual play-testing. Exit works in a standalone build, not while running inside the editor.

Question bank: 24 distinct crop-disease questions are shuffled without repetition across the 18 crops in one Level 1–3 run; each question also shuffles its three options. Starting a new game reshuffles. Level 3 unlocks after Level 2 and has seven crops and a farm built from Unity objects rather than a background image. Farmer and crop sprites are still image assets.

Farm education references: https://extension.umn.edu/agriculture/specialty-crops/vegetable-farming/preventing-plant-diseases-on-farms and https://ipm.ucanr.edu/what-is-ipm/ . The quiz is simplified educational material; actual field diagnosis and treatments depend on the crop, location, and local guidance.

Disease question references: https://extension.umn.edu/garden-and-home/yard-and-garden/gardening-in-minnesota/yard-and-garden-problems/tomato-leaf-spot-diseases and https://extension.umn.edu/agriculture/specialty-crops/vegetable-farming/disease-management/early-blight-in-tomato-and-potato .
