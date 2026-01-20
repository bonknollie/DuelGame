Prison Prototype

How to use
1. In Unity, create an empty GameObject in a new empty Scene (or an existing scene).
2. Attach the `PrisonPrototypeBuilder` component (Assets/Assets/Scripts/Prototype/PrisonPrototypeBuilder.cs).
3. (Optional) Assign a `guardPrefab` if you have one; otherwise a cube will be used.
4. Press Play if you enabled `autoBuild` on the `PrisonPrototypeBuilder` component. The builder can also be invoked manually from the component's context menu (right-click the component header -> "Build Prototype"):
   - Floor, cells, side/back walls
   - SpawnPoints inside each cell (objects with `SpawnPoint` script)
   - A Gate controlled by the levers placed in the cells
   - A Guard GameObject that patrols between two waypoints

Notes
- This builder creates runtime primitives only; it doesn't modify scenes in Edit mode (except removing a previous runtime-built `PrisonPrototype` child).
- Interact with levers by selecting the Lever GameObjects in the Hierarchy and calling `InteractableLever.Interact()` from the Inspector or via a simple key-binding (you can wire player interaction later).
- The Gate opens when all levers created by the builder are toggled ON.

Next steps
- Hook up your `PlayerController` to spawn at a `SpawnPoint` position on Start.
- Add a simple player interaction key (e.g., E) that raycasts and calls `Interact()` on `InteractableLever`.
- Replace primitives with your art/assets and add NavMesh/AI for guard if desired.
