# FlappyBird — Sky Hop

Initial Unity 6.5 (6000.5.10f1) Flappy Bird-inspired game. Open `ISD/UnityGame` in Unity Hub, then open `Assets/Scenes/FlappyBird.unity` and enter Play mode.

Press Space to start, flap, or restart after game over. Escape returns to the menu. The scene includes original simple geometric bird/scenery, Rigidbody2D physics, colliders, a pipe prefab, randomized gaps, scoring, best-score storage, and Canvas menus.

## Validation and limits

C# compilation and scene generation succeeded. Manual Unity checks observed the menu, flight, pipe spawning, and ground-collision game-over screen. Scoring through pipe gates, mouse-button behavior, and the latest restart correction still require complete playtesting. This is an initial implementation, not a fully validated release. WebGL integration, React/Express and PostgreSQL work remain outstanding.

The editor menu `Flappy Bird > Create playable scene` regenerates the scene and prefab; run it only outside Play mode. It replaces the generated scene.

ISD ownership: Anubhav. Implementation was prepared with AI assistance. No contributions from Aditya or Yash are claimed in this checkpoint.
