# AquaGuardian: A Village of Choices

A Unity WebGL story game about an Indian village caring for its shared groundwater. Meet villagers through five chapters, listen to their problems and choose a response. Decisions affect the well, community trust, coins and score; together they lead to one of three endings. The story covers drought, drip irrigation, millet, rainwater recharge, contamination and collective planning. Values are illustrative game units, not scientific predictions.

Open `unity-game` in Unity 6000.5.10f1, open `Assets/Scenes/Village.unity`, and press Play. Click **Begin Story**, then **Talk** and choose a response. Click **Continue Story** after reading each outcome. An unfinished game is stored locally in the browser or Unity player; **Continue** resumes it. **Play Again** starts a fresh story.

## Web build

Install Unity WebGL Build Support. Use **AquaGuardian > Build WebGL** and choose an empty output directory, or run:

```sh
Unity -batchmode -quit -projectPath /path/to/unity-game -buildTarget WebGL \
  -executeMethod AquaBuild.BuildWebGL -aquaguardianOutput /path/to/build -logFile /path/to/build.log
```

Serve the generated directory through a web server with the correct compression headers; opening `index.html` directly is not supported. Do not commit generated builds.

## Result handoff

At the end, WebGL emits an `aquaguardian:result` event on `window` with `scenario`, `score`, `groundwaterBefore`, `groundwaterAfter`, `waterSaved` and `sustainabilityRating`. `waterSaved` is an illustrative nonnegative game value derived from the final well change. A host application must listen before the build loads. This event does not register players or save results online; the future application must authenticate, map the scenario, validate and persist results. The story itself does not require a server connection beyond serving the game.

## Scope

This is a short choice-driven illustrated story with local resume, not a 3D cinematic adventure. Online accounts, leaderboard, certificates, and backend integration are not implemented in Unity. Story content and game values are educational simplifications. Generated village artwork was created for this project.
