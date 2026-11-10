# AquaGuardian Unity game

Open `unity-game` with Unity 6000.5.10f1, open `Assets/Scenes/Village.unity`, and press Play. Explore the village and complete four hands-on tasks: collect rain, build a recharge pit, use drip irrigation and choose millet, and remove litter near the water. Use WASD or arrow keys to move and E or Space to interact; on-screen controls are also provided. The environment is generated when the scene runs. Water, savings and points are illustrative game values, not scientific measurements.

## WebGL

WebGL Build Support must be installed. The Village scene is included in the build profile. To build from a terminal (replace the paths for your machine):

```sh
Unity -batchmode -quit -projectPath /path/to/unity-game -buildTarget WebGL \
  -executeMethod AquaBuild.BuildWebGL -aquaguardianOutput /path/to/build -logFile /path/to/build.log
```

Serve the build through a web server; browsers cannot reliably run Unity WebGL by opening its HTML file directly. The output is generated and should not be committed to Git.

## Web app handoff

The build emits `aquaguardian:result` as a browser `CustomEvent` on `window` when a game ends. Its `detail` contains, for example:

```json
{"scenario":"VillageAdventure","score":1570,"groundwaterBefore":40,"groundwaterAfter":74,"waterSaved":70,"sustainabilityRating":"Excellent"}
```

The host page should install a listener before loading the Unity build:

```js
window.addEventListener('aquaguardian:result', ({ detail }) => {
  // Map detail.scenario to the backend scenario ID, then submit using
  // the authenticated user's session and the actual backend API contract.
  console.log(detail);
});
```

This event does not itself save a result. The frontend/backend must provide authentication, scenario-ID mapping, session creation, validation and persistence. If the build is embedded in an iframe, listen inside that frame and deliberately forward the event to the parent using a validated `postMessage` origin.

## Scope

This is an early playable adventure, with sequential missions rather than a dynamic groundwater simulation. The game sends a result event but does not implement online registration, persistent scores or certificates. Mobile deployment and the full browser-to-backend flow still need verification. Earlier prototype scripts remain in the project but are not the active game.
