# Day 22 Tavern Story Setup

Day 22 主題是建立酒館與受傷年輕人 NPC，讓 Day 21 的目前目標「前往酒館，詢問受傷的年輕人」有實際落點。

這一天先不做完整分支任務系統，也不做多角色逐句切換的正式對話框。現階段仍使用 `NpcDialogueInteraction`，每個 NPC 各自擁有一組濃縮後的劇情台詞。

## 場景

- 新增場景：`TavernInteriorScene`
- 酒館室內背景由使用者手動從 Tavern 素材中切出 `Travern_01_3` 與 `Travern_01_4` 兩塊，再拼成正式酒館背景。
- 不要再使用 `Tavern_01_2` 作為酒館背景，因為 `Tavern_01_2` 已經用於 Day 18 村長家。
- 酒館外觀尚未正式建立。後續村莊中的酒館外觀應使用 `Anokolisa_TopdownRPG/Buildings` 內的 `Roofs.png`、`Walls.png`、`Props.png` 或 `Village_BuildingPalette_16x16.prefab` 拼出較大的建築，不建議使用 `Buildings/MockUps/Tavern_01.png`，因為那是酒館內裝示意圖。
- 村莊中已有 `TavernBuildSite`，可作為酒館外觀的預留位置。
- 場景中建立：
  - `TavernInterior_Base`
  - `TavernMarkers`
    - `Entry_FromVillage`
    - `Exit_ToVillage`
  - `TavernCollision`
  - `TavernNPCs`

## 村莊轉場

`VillageScene` 中新增或更新：

- `Exit_ToTavern`
  - Target Scene Name：`TavernInteriorScene`
  - Target Spawn Point Name：`Entry_FromVillage`
- `Entry_FromTavern`
  - 玩家從酒館回村莊後的出生點

## 背景人物互動

酒館背景中已經畫了三個可對話的人物，因此 Day 22 不再另外放置 NPC Sprite，避免人物重疊。

- `BackgroundCharacter_01_Bar`
  - 位置：吧檯的人，使用者標記為 1 號
  - Speaker：`酒館老闆`
  - 作用：引導玩家去問受傷年輕人
- `BackgroundCharacter_02_Pillar`
  - 位置：柱子旁邊的人，使用者標記為 2 號
  - Speaker：`阿森`
  - 作用：透露森林深處有五彩寶光、怪吼、小怪群
- `BackgroundCharacter_03_LowerHall`
  - 位置：下方走廊的人，使用者標記為 3 號
  - Speaker：`洛克`
  - 作用：補充洞穴門口線索，並在對話完成後更新目前目標

目前只有 `BackgroundCharacter_03_LowerHall` 的對話完成後會把目前目標改成：

`回村長家，回報洞穴線索`

這是最小可用版本，代表玩家已經問出洞穴線索。之後如果要做完整任務流程，可以再改成記錄阿森、米娜、洛克三個線索是否全部詢問完成。

## Day 22 工具

Unity menu：

- `IronRPG > Day 22 > 1 Create Tavern Interior Scene`
  - 目前只用於建立空場景殼、Scene List、Player/Main Camera 與 marker，不再放置背景圖。
- `IronRPG > Day 22 > 2 Setup Tavern Colliders Only`
  - 讀取場景中的 `Travern_01_3` / `Travern_01_4`，只建立 `TavernCollision`，不修改背景。
- `IronRPG > Day 22 > 3 Setup Background Character Dialogues`
  - 不產生新的 NPC Sprite，只在背景人物附近建立透明互動 Trigger 並掛上 `NpcDialogueInteraction`。
- `IronRPG > Day 22 > 4 Setup Village To Tavern Transition`
- `IronRPG > Day 22 > 5 Setup Tavern Interior Entry Exit Points`
  - 只建立或刷新酒館內部 `TavernMarkers`、`Entry_FromVillage`、`Exit_ToVillage`，不修改背景、碰撞箱或村莊外觀。
- `IronRPG > Day 22 > Open Tavern Interior Scene`

工具只是幕後加速。文章不要截工具選單，而要寫正常 Unity 流程。

## 建議截圖

### 22-1 拼出酒館背景

截圖內容：

- `TavernInteriorScene`
- 場景中可看到 `Travern_01_3` 與 `Travern_01_4` 拼成的酒館背景

文案方向：

村長家已經使用過 Tavern 素材中的大型室內圖，因此酒館不能再直接使用同一塊背景。這裡改成從 Tavern 素材中重新切出兩塊區域，拼成真正的酒館室內，讓酒館和村長家在視覺上有所區分。

### 22-2 建立酒館碰撞箱

截圖內容：

- `TavernCollision`
- 牆壁、櫃台、桌子、廚房家具、下方走廊邊界的 `BoxCollider2D`

文案方向：

酒館背景只是圖片，玩家仍然可以穿過牆壁和家具。因此我建立 `TavernCollision` 分組，針對牆面、櫃台、桌椅與走廊邊界補上 `BoxCollider2D`，讓酒館變成可實際探索的室內場景。

### 22-3 設定背景人物互動區

截圖內容：

- `TavernBackgroundCharacterInteractions`
- `BackgroundCharacter_01_Bar`
- `BackgroundCharacter_02_Pillar`
- `BackgroundCharacter_03_LowerHall`
- Inspector 中的 `BoxCollider2D (Is Trigger)` 與 `NpcDialogueInteraction`

文案方向：

酒館背景裡已經有三位人物，不能再額外疊新的 NPC 圖像，因此我改用透明互動區的做法。每個背景人物附近放一個 `BoxCollider2D` Trigger，並掛上 `NpcDialogueInteraction`，讓玩家靠近背景人物時也能觸發對話。

### 22-4 設定酒館入口與出口

截圖內容：

- `TavernMarkers`
- `Entry_FromVillage`
- `Exit_ToVillage`
- `Exit_ToVillage` 上的 `SceneTransitionPoint`

文案方向：

酒館場景需要和村莊互相連接，因此我建立進入點與離開點。離開酒館時會回到 `VillageScene` 的 `Entry_FromTavern`。

### 22-5 設定村莊通往酒館的轉場

截圖內容：

- `VillageScene`
- `Exit_ToTavern`
- Inspector 中的 `SceneTransitionPoint`

文案方向：

在村莊的酒館入口位置放置 Trigger，並把目標場景設定為 `TavernInteriorScene`。玩家走到酒館門口時，就能進入酒館內部。

### 22-6 設定酒館進入點

截圖內容：

- `TavernInteriorScene`
- `TavernMarkers`
- `Entry_FromVillage`

文案方向：

從村莊進入酒館時，玩家不應該出現在場景原點，而是要出現在酒館門口附近。因此我在 `TavernInteriorScene` 中建立 `Entry_FromVillage`，作為玩家從 `VillageScene` 進入酒館後的出生點。

### 22-7 設定酒館離開點

截圖內容：

- `TavernInteriorScene`
- `TavernMarkers`
- `Exit_ToVillage`
- `Exit_ToVillage` 上的 `SceneTransitionPoint`

文案方向：

接著建立 `Exit_ToVillage`，讓玩家從酒館門口離開時可以回到 `VillageScene`。這個物件需要 `BoxCollider2D` 並勾選 `Is Trigger`，同時把 `SceneTransitionPoint` 的目標場景設定為 `VillageScene`，目標出生點設定為 `Entry_FromTavern`。

## 測試文字紀錄

Day 22 的測試不另外截圖。文章可用文字補充：進入 Play Mode 後，玩家可以從村莊進入酒館，從酒館門口離開後回到村莊酒館外側；靠近背景人物也能觸發對話。
