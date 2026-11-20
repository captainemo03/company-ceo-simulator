# Company CEO Simulator · Unity 3D Prototype

This is a separate Unity 6 prototype. The production web game remains in the sibling `company-ceo-simulator-site` worktree and is not modified by this project.

## Current prototype

- Unity `6000.4.0f1` project scaffold.
- AI Navigation and glTFast package declarations compatible with Unity `6000.4.0f1`.
- Real CC0 GLB assets are kept in `Assets/Resources/Characters` and `Assets/Resources/Office`, and copied to `Assets/StreamingAssets` for standalone runtime loading.
- Two autonomous agents: CEO and CFO.
- Independent spawn/target routes, `NavMeshAgent`, avoidance priorities and stuck detection.
- Runtime office and character asset loading through glTFast with no geometric human fallback. The bootstrap waits for the GLB scenes before baking the office NavMesh.
- Runtime camera follow rig and Windows x64 editor build menu.
- V2 modules for utility-based needs, employee memory/relationships, smart objects, company time speeds, meetings, automatic scene direction, NPC inspection and staggered LOD budgeting.
- V3 first world slice: a bounded 500 x 500 Liberty Bay business district, rival company presence, traffic route network, day/night lighting and deterministic weather state.
- V3 asset boundary: city buildings, factories, container terminal and vehicles are loaded only from `Assets/Resources/City`; missing licensed art is reported and labelled scaffolding is used for the district layout. No human or vehicle model is silently presented as final art.
- Core gameplay slice: an interactive consumer-electronics Product Design Studio for a smartphone (component tiers, compatibility, BOM cost, R&D, tests, production, launch, sales and PlayerPrefs save/load).
- The phone preview is a runtime modular prototype made of editable product components. A final release still needs a separately licensed modular smartphone asset pack; the prototype does not present the procedural preview as licensed final art.
- Universal Product Design Studio: blank-category products, primitive modeling, freehand 2D sketch extrusion, assembly tree, material/cost effects, custom metrics, OBJ/GLB/GLTF import paths with size limits, version snapshots, world publishing and save/load.
- The universal editor is a game simulation, not professional CAD or engineering validation. Imported GLB/GLTF loading depends on the declared glTFast package and must be rechecked in a completed Windows build.

## Required local setup

Unity 6 is installed, the Unity Personal entitlement is active with unlimited expiration, and the Package Manager IPC service starts correctly. Do not share credentials in chat.

The GLB files are loaded through Unity glTFast from `StreamingAssets`. If a file or package is unavailable, the runtime reports a clear error instead of creating placeholder people.

Open `Assets/Scenes/OfficePrototype.unity`, press Play, and use `Company CEO Simulator/Build Windows x64` after the package manager is healthy.

The Unity import and Windows x64 build have completed successfully with Mono scripting backend. A standalone smoke run reaches the GTX 1650/D3D12 player without managed exceptions; the remaining acceptance work is a longer autonomous-run and visual inspection of the loaded GLB scenes.

Asset licenses are preserved in `Assets/Resources/Characters/LICENSE.md`, `Assets/StreamingAssets/Characters/LICENSE.md` and `Assets-OFFICE-LICENSE.md`.
