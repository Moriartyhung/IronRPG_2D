# Scene Flow - Day 13

## Purpose

Day 13 is used to clarify the early scene flow before building the actual scene transition system.

The project currently treats `TutorialScene` as the main branching area. The village does not directly lead to the forest or cave.

## Existing Scenes

| Scene | Current Role | Notes |
| --- | --- | --- |
| `TutorialScene` | Tutorial ruins and early branching point | Already contains tutorial markers and three exit directions. |
| `VillageScene` | Main village | Already contains the village layout, player, buildings, props, and collision setup. |

## Planned Scenes

| Scene | Planned Role | Status |
| --- | --- | --- |
| `ForestScene` | Forest exploration area | Not created yet. |
| `CaveScene` | Cave or dungeon exploration area | Not created yet. |

## Scene Connections

| From Scene | Marker | Target Scene | Purpose |
| --- | --- | --- | --- |
| `TutorialScene` | `Exit_ToVillage` | `VillageScene` | Enter the main village after leaving the tutorial ruins. |
| `TutorialScene` | `Exit_ToForest` | `ForestScene` | Future forest exploration route. |
| `TutorialScene` | `Exit_ToCave` | `CaveScene` | Future cave or dungeon route. |
| `VillageScene` | `Exit_ToTutorialRuins` | `TutorialScene` | Return from the village to the tutorial ruins. |
| `VillageScene` | `Entry_FromTutorialRuins` | None | Player spawn position when entering the village from `TutorialScene`. |

## Day 13 Scope

- Rename and clarify scene exit markers.
- Add village entry and exit markers.
- Record the planned scene flow.
- Do not implement actual scene loading yet.
- Do not create `ForestScene` or `CaveScene` yet.
- Do not modify player movement, camera follow, collision, or dialogue scripts today.

## Notes

The intended early-game flow is:

1. The player starts from the tutorial ruins.
2. The player reaches the village and reports the abnormal situation.
3. The village sends the player back toward the ruins route.
4. The ruins area later branches into the forest and cave routes.

This keeps the village as a story hub while keeping exploration routes controlled from the tutorial ruins.
