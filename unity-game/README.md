# AquaGuardian Unity game

Open `unity-game` with Unity 6000.5.10f1 and open `Assets/Scenes/Village.unity`. Press Play. The village starts with a menu for normal rainfall, drought and heavy rainfall. WASD/arrow keys move the player; use the on-screen buttons for decisions. Each scenario lasts up to six turns, or ends early if water reaches zero. The Q-learning menu trains a small tabular agent in the same simulation. Game units and rewards are illustrative, not scientific measurements.

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
{"scenario":"Drought","score":545,"groundwaterBefore":45,"groundwaterAfter":39,"waterSaved":56,"sustainabilityRating":"At risk"}
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

The game currently includes a tabular Q-learning demonstration. PPO, imitation learning, VR and backend integration are not implemented. The prototype dialogue and interaction scripts remain in the project, but the current gameplay uses on-screen decision buttons.
