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

## Required local setup

Unity Hub currently has Unity 6 installed, but the editor log reports a Package Manager service failure (`UnityPackageManager.exe` could not start). Repair/reinstall the Unity 6 editor installation in Unity Hub, then open this folder and allow the declared packages to resolve.

The GLB files are imported through Unity glTFast. If the package cannot resolve, the models remain in the project but the runtime will report a clear missing-asset error instead of creating placeholder people.

Open `Assets/Scenes/OfficePrototype.unity`, press Play, and use `Company CEO Simulator/Build Windows x64` after the package manager is healthy.

The V2 acceptance test is intentionally still pending: Unity has not completed a clean package import on this machine, so no FPS, NavMesh, animation or ten-minute autonomous-run result is reported as successful yet.

Asset licenses are preserved in `Assets/Resources/Characters/LICENSE.md` and `Assets-OFFICE-LICENSE.md`.
