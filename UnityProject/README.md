# Company CEO Simulator · Unity 3D Prototype

This is a separate Unity 6 prototype. The production web game remains in the sibling `company-ceo-simulator-site` worktree and is not modified by this project.

## Current prototype

- Unity `6000.4.0f1` project scaffold.
- URP, AI Navigation, Cinemachine, Animation Rigging, Timeline and glTFast package declarations.
- Real CC0 GLB assets copied into `Assets/Resources/Characters` and `Assets/Resources/Office`.
- Two autonomous agents: CEO and CFO.
- Independent spawn/target routes, `NavMeshAgent`, avoidance priorities and stuck detection.
- Runtime office asset loading with no geometric human fallback.
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

The GLB files are imported through Unity glTFast. If the package cannot resolve, the models remain in the project but the runtime will report a clear missing-asset error instead of creating placeholder people.

Open `Assets/Scenes/OfficePrototype.unity`, press Play, and use `Company CEO Simulator/Build Windows x64` after the package manager is healthy.

The V2/V3 acceptance test is still pending until a full Unity import and Windows build completes on this machine. No FPS, NavMesh, animation or ten-minute autonomous-run result is reported as successful yet.

Asset licenses are preserved in `Assets/Resources/Characters/LICENSE.md` and `Assets-OFFICE-LICENSE.md`.
