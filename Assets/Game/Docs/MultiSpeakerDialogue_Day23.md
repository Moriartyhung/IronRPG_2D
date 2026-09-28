# Day 23 Multi Speaker Dialogue

Day 23 主題是升級簡易對話系統，讓同一段對話可以逐句指定說話者。

Day 22 的酒館對話已經可以觸發，但 `NpcDialogueInteraction` 原本只有一個 `speakerName`，所以整段對話都會顯示成同一個 NPC 在說話。這會讓劇本中的主角追問、NPC 回答無法正確呈現。

## 目標

- 保留 Day 19 / Day 21 已經使用的舊版單一 speaker 對話。
- 新增可選的多 speaker 對話資料。
- 讓每一句對話都能指定 `speakerName` 與 `text`。
- 將酒館三個背景人物的對話改成 NPC 與主角交錯。
- 加入最小任務階段，避免玩家跳過酒館直接觸發第二次村長回報。
- 不重做 UI，不做正式對話框美術，不做選項分支。

## 修改檔案

- `Assets/Game/Dialogue/Scripts/NpcDialogueInteraction.cs`
  - 新增 `DialogueLineEntry`
  - 新增 `useSpeakerLines`
  - 新增 `speakerLines`
  - 新增 `DialogueSequence`
  - 新增 `useDialogueSequences`
  - 新增 `dialogueSequences`
  - 新增最小 `MainQuestStage` 與 `MainQuestState`
  - 顯示對話時依照目前行數取得 speaker 與台詞
  - 可依照目前 `MainQuestStage` 選擇要播放哪一段對話
  - 舊欄位 `dialogueLines` 仍保留，舊 NPC 不需要重做
- `Assets/Game/Editor/Day23MultiSpeakerDialogueOrganizer.cs`
  - 新增 Unity menu：
    `IronRPG > Day 23 > 1 Apply Tavern Multi Speaker Dialogues`
  - 將酒館三個背景人物互動區改成逐句 speaker 對話
  - 讓洛克對話完成後推進到 `ReturnToChief`
  - 讓村長互動區依任務階段播放第一次村長對話或第二次回報對話

## Day 23 工具

### `IronRPG > Day 23 > 1 Apply Tavern Multi Speaker Dialogues`

正常 Unity 流程：

1. 開啟 `TavernInteriorScene`。
2. 選取酒館背景人物互動區，例如 `BackgroundCharacter_01_Bar`。
3. 在 `NpcDialogueInteraction` 中啟用任務階段對話。
4. 在對話序列中逐句填入說話者與台詞。
5. 對 `BackgroundCharacter_02_Pillar` 與 `BackgroundCharacter_03_LowerHall` 重複相同設定。
6. 將洛克對話設定為完成後更新目標並推進任務階段。

工具只是幕後加速，不截工具選單。

### `IronRPG > Day 23 > 2 Apply Chief Multi Speaker Dialogue`

正常 Unity 流程：

1. 開啟 `ChiefHouseInteriorScene`。
2. 選取 `ChiefInteractionArea`。
3. 在 `NpcDialogueInteraction` 中啟用任務階段對話。
4. 第一段對話條件設為 `ReportToChief`，填入劇本 `4.2 第一次找村長` 的村長 / 主角交錯台詞。
5. 保留第一次對話完成後更新目前目標：
   `前往酒館，詢問受傷的年輕人`
6. 第一次對話完成後推進任務階段到 `TavernInvestigation`。

工具只是幕後加速，不截工具選單。

### `IronRPG > Day 23 > 3 Apply Chief Report Dialogue`

正常 Unity 流程：

1. 酒館追問完成後，回到 `ChiefHouseInteriorScene`。
2. 選取 `ChiefInteractionArea`。
3. 保持任務階段對話啟用。
4. 第二段對話條件設為 `ReturnToChief`，填入劇本 `4.8 第二次找村長` 的村長 / 主角回報對話。
5. 第二段對話完成後更新目前目標：
   `前往武器鋪，詢問特殊武器情報`
6. 第二段對話完成後推進任務階段到 `AskBlacksmith`。

這一步不是手動替換第一次村長對話，而是讓同一個 `ChiefInteractionArea` 依照目前 `MainQuestStage` 自動選擇對話。只有酒館洛克對話完成、任務階段進入 `ReturnToChief` 後，第二次村長回報才會出現。

工具只是幕後加速，不截工具選單。

### `IronRPG > Day 23 > 4 Apply Quest Stage Dialogue Flow`

正常 Unity 流程：

1. 在村長互動區建立兩段對話序列：
   - `ReportToChief`：第一次找村長
   - `ReturnToChief`：酒館後回報村長
2. 在酒館人物互動區建立酒館調查階段的對話序列。
3. 第一次村長對話完成後，推進到 `TavernInvestigation`。
4. 洛克對話完成後，推進到 `ReturnToChief`。
5. 第二次村長回報完成後，推進到 `AskBlacksmith`。

工具只是幕後加速，不截工具選單。

## 酒館對話對應

- `BackgroundCharacter_01_Bar`
  - 酒館老闆與主角的開場對話
  - 來源：劇本 `4.3 酒館老闆開場`
- `BackgroundCharacter_02_Pillar`
  - 阿森與主角的森林異常對話
  - 來源：劇本 `4.4 與阿森對話`
- `BackgroundCharacter_03_LowerHall`
  - 洛克與主角的洞穴門口真相對話
  - 來源：劇本 `4.6 與洛克對話`
  - 對話完成後更新目前目標：
    `回村長家，回報洞穴線索`

## 建議截圖

後續每個截圖節點都要同時滿足兩件事：

1. 使用者看得懂要截 Unity 或程式碼的哪個位置。
2. 匯出給 ChatGPT 後，ChatGPT 看得懂這張圖要寫什麼文章段落。

因此每張圖都保留「截圖內容」與「文案方向」兩段。

### 23-1 說明原本對話系統的限制

截圖內容：

- `NpcDialogueInteraction.cs`
- 舊欄位 `speakerName`
- 舊欄位 `dialogueLines`

程式碼內容說明：

- 第 33 行到第 44 行：保留舊版 `speakerName`、`interactPrompt`、`dialogueLines` 欄位，讓 Day 19 / Day 21 已經做好的單人 NPC 對話不用全部重做。
- 第 75 行到第 106 行：玩家靠近互動區後，按 `E` 或 `Space` 會開啟或推進對話；這段仍沿用舊版互動流程。
- 第 344 行到第 374 行：取得目前台詞內容時，如果沒有啟用新版逐句 speaker 資料，就回到舊版 `dialogueLines`。

文案方向：

原本的對話系統只支援一個 `speakerName`，所以整段對話都會被視為同一個 NPC 在說話。這在村長單人說明任務時可以接受，但酒館追問劇情需要主角和 NPC 交錯對話，因此必須升級資料結構。

### 23-2 新增逐句 speaker 對話資料

截圖內容：

- `NpcDialogueInteraction.cs`
- `DialogueLineEntry`
- `useSpeakerLines`
- `speakerLines`

程式碼內容說明：

- 第 9 行到第 14 行：新增 `DialogueLineEntry`，每一句台詞都保存自己的 `speakerName` 與 `text`。
- 第 39 行到第 40 行：在 NPC 本體上新增 `useSpeakerLines` 與 `speakerLines`，用來控制是否啟用新版逐句說話者資料。
- 第 302 行到第 320 行：計算對話總行數時，會優先檢查 `speakerLines`；如果沒有啟用，才回到舊版 `dialogueLines`。

文案方向：

我新增 `DialogueLineEntry`，讓每一句對話都能保存 `speakerName` 與 `text`。同時用 `useSpeakerLines` 決定是否啟用新版資料，這樣舊 NPC 仍能使用原本的 `dialogueLines`，不需要全部重做。

### 23-3 依目前行數取得說話者與台詞

截圖內容：

- `GetCurrentSpeakerName()`
- `GetCurrentLineText()`
- `OnGUI()` 中顯示 speaker 與 line 的位置

程式碼內容說明：

- 第 274 行到第 300 行：`OnGUI()` 繪製對話框、說話者名稱、目前台詞，以及底部的下一句提示。
- 第 322 行到第 342 行：`GetCurrentSpeakerName()` 依照目前 `lineIndex` 取得這一句的說話者；如果該句沒有填 speaker，就使用這段對話的預設 speaker。
- 第 344 行到第 374 行：`GetCurrentLineText()` 依照目前 `lineIndex` 取得台詞文字，並保留新版 `speakerLines` 與舊版 `dialogueLines` 的 fallback。

文案方向：

顯示對話時，系統會依照目前的 `lineIndex` 取得這一句的說話者與台詞。如果沒有啟用多 speaker 模式，就回到舊版 `speakerName + dialogueLines` 的顯示方式。

### 23-4 套用酒館多 speaker 對話

截圖內容：

- `BackgroundCharacter_01_Bar` 或任一酒館背景人物互動區
- Inspector 中的 `Use Speaker Lines`
- `Speaker Lines` 中能看到 `酒館老闆 / 主角` 或 `阿森 / 主角`

程式碼內容說明：

- `Day23MultiSpeakerDialogueOrganizer.cs` 第 61 行到第 117 行：保存酒館老闆、阿森、洛克與主角交錯的對話文字。
- 第 218 行到第 255 行：Unity 選單 `Apply Tavern Multi Speaker Dialogues` 會打開酒館場景，並把三個背景人物互動區改成多 speaker 對話。
- 第 376 行到第 408 行：工具實際寫入 `dialogueSequences`，逐句填入 `speakerName` 與 `text`。

文案方向：

酒館背景人物本身是畫在背景圖上的，所以仍然使用透明 Trigger 作為互動區。這次只更新 `NpcDialogueInteraction` 的對話資料，讓同一個互動區可以呈現 NPC 與主角的來回問答。

### 23-5 Play Mode 測試多 speaker 對話

截圖內容：

- Play Mode
- 對話框 speaker 名稱從 NPC 切換到 `主角`

文案方向：

測試時，玩家靠近酒館人物按下互動鍵，對話框會依照每一句資料切換說話者。這代表酒館追問劇情可以用接近劇本的方式呈現，而不是所有台詞都顯示成同一個 NPC 在自言自語。

### 23-6 測試第一次村長對話完成後更新目前目標

截圖內容：

- Play Mode
- 與村長完成第一次對話後
- 左上角目前目標顯示：
  `前往酒館，詢問受傷的年輕人`

文案方向：

第一次找村長的劇情，本來就不是村長單方面交代任務，而是主角回報遺跡異常、村長補充森林狀況，再由兩人討論下一步行動。因此我也把 `ChiefInteractionArea` 改成使用 `Speaker Lines`，讓村長與主角可以逐句切換說話者。對話完成後，仍然會把目前目標更新為「前往酒館，詢問受傷的年輕人」。

### 23-7 加入主線階段限制

截圖內容：

- `NpcDialogueInteraction.cs`
- `DialogueSequence`
- `requiredQuestStage`
- `advanceQuestStageOnComplete`
- `questStageOnComplete`
- 建議截程式碼位置：`DialogueSequence` 類別定義附近，以及對話完成時推進任務階段的邏輯。

程式碼內容說明：

- 第 17 行到第 30 行：新增 `DialogueSequence`，一段對話序列可以指定自己的任務階段、預設說話者、逐句台詞、完成後目標文字與下一個任務階段。
- 第 245 行到第 264 行：對話完成時，如果目前序列勾選 `setObjectiveOnComplete`，就更新左上角目前目標；如果勾選 `advanceQuestStageOnComplete`，就推進 `MainQuestState`。
- 第 387 行到第 415 行：`GetActiveSequence()` 會讀取目前 `MainQuestStage`，只挑選符合 `requiredQuestStage` 的對話序列。
- 第 468 行到第 536 行：`MainQuestStage` 定義主線節點，`MainQuestState` 用 `DontDestroyOnLoad` 保存目前階段，讓跨場景後仍知道玩家劇情進度。

文案方向：

多段對話不能只靠同一個互動區反覆替換內容，否則玩家可能在第一次見村長時就觸發酒館後的回報劇情。因此我加入 `DialogueSequence`，讓每段對話都有自己的 `requiredQuestStage`。只有目前任務階段符合時，對話才會出現；對話完成後，也可以用 `questStageOnComplete` 推進到下一個階段。

### 23-8 設定村長與酒館的任務階段對話

截圖內容：

- `ChiefInteractionArea`
- `Dialogue Sequences`
- 第一段 `ReportToChief`
- 第二段 `ReturnToChief`
- 或酒館洛克互動區中完成後推進到 `ReturnToChief`
- 建議截 Unity Inspector：選取 `ChiefInteractionArea`，展開 `Npc Dialogue Interaction` 的 `Dialogue Sequences`。

程式碼內容說明：

- `Day23MultiSpeakerDialogueOrganizer.cs` 第 120 行到第 215 行：保存第一次找村長、第二次回報村長，以及洛克線索對話的逐句內容。
- 第 349 行到第 357 行：`ApplyChiefDialogueFlow()` 會替同一個 `ChiefInteractionArea` 建立兩段對話序列，分別對應 `ReportToChief` 與 `ReturnToChief`。
- 第 387 行到第 395 行：每段序列寫入自己的 `requiredQuestStage`、預設 speaker、互動提示、完成後是否更新目標與推進階段。
- 第 397 行到第 405 行：每一句台詞都寫入 `speakerName` 與 `text`，因此村長和主角能在同一段對話中交錯出現。

文案方向：

村長互動區現在同時保存兩段對話：第一次找村長時播放委託玩家去酒館的劇情；酒館問完洛克、任務階段進入 `ReturnToChief` 後，才播放回報洞穴線索的劇情。這樣主線順序就會固定為「村長 -> 酒館 -> 村長」，不會提前跳到後面的內容。

### 23-9 測試回報完成後更新武器鋪目標

截圖內容：

- Play Mode
- 完成第二次村長對話後
- 左上角目前目標顯示：
  `前往武器鋪，詢問特殊武器情報`
- 建議截 Unity Game 視窗：對話關閉後，左上角目前目標清楚可見。

文案方向：

第二次村長對話會揭露洞穴與 Boss 的舊事，也讓主角意識到普通武器可能無法解決問題。對話完成後，目前目標更新為前往武器鋪，讓下一段劇情可以接到武器匠與特殊武器情報。

## Day23 後半：更換主角與補上基礎動作系統

完成多 speaker 對話後，遊戲的劇情流程已經能從村長推進到酒館，再回到村長家。但實際測試時，原本的主角素材仍然比較像臨時佔位：角色移動時缺少明確方向表現，也沒有適合正式展示的攻擊動作。

因此 Day23 後半補上主角操作表現：

- 將主角換成 `NinjaAdventure` 的 Samurai 角色素材。
- 主角可以按 `Shift` 跑步。
- 主角依照上、下、左右方向切換待機、走路與跑步圖像。
- 主角按 `J` 進行拳擊攻擊。
- 攻擊時播放角色本體的攻擊 frame。
- 攻擊方向依照方向鍵或目前面向決定。
- 攻擊傷害讀取目前拳套與符文的總傷害。

這裡的重點不是做武器揮舞，而是讓主角本身看起來會移動、會轉向、會出拳。因為新版劇情已經改成拳套與符文系統，所以主角不需要把刀劍拿在手上，攻擊表現以角色本體攻擊動畫為主。

### 23-10 更換主角為 NinjaAdventure Samurai

截圖內容：

- Project 視窗或 Inspector 顯示主角使用 NinjaAdventure Samurai 素材。
- 建議截圖位置：
  - `Assets/ImportedAssets/NinjaAdventure/Characters/samurai_blue_sprite.png`
  - 或 `Assets/Game/Generated/Day26/NinjaAdventurePlayerFrames`
  - Player 的 `SpriteRenderer` / `PlayerSpriteAnimator`

文案方向：

原本主角圖像比較像測試用佔位，沒有適合展示的攻擊動作。這次改用專案中已匯入的 NinjaAdventure Samurai 角色素材，讓主角有更完整的待機、移動、跑步與攻擊圖像。這樣後面進入森林戰鬥時，玩家看起來就不再是佔位角色，而是真正有動作表現的主角。

補充說明：

- 素材來源：`NinjaAdventure`
- 授權：Creative Commons Zero v1.0 Universal
- 用途：主角待機、走路、跑步、攻擊動畫

### 23-11 解說 PlayerMovement.cs：走路、跑步與面向

截圖內容：

- 程式碼檔案：`Assets/Game/Player/Scripts/PlayerMovement.cs`
- 建議截圖行號：
  - 第 7 行到第 19 行：走路速度、跑步速度、目前移動輸入、目前面向、是否跑步。
  - 第 25 行到第 41 行：初始化 `Rigidbody2D`、碰撞篩選與玩家背包資料，讓移動、碰撞與裝備資料能在遊戲開始時就準備好。
  - 第 43 行到第 59 行：`Update()` 每幀讀取輸入，`FixedUpdate()` 依照走路或跑步速度計算位移，再交給剛體移動。
  - 第 61 行到第 69 行：`FaceDirection()` 更新角色面向，讓角色停止移動後仍記得最後方向。
  - 第 71 行到第 111 行：讀取 WASD / 方向鍵 / Shift，並在有方向輸入時更新 `moveInput`、`isRunning` 與 `facingDirection`。

程式碼內容說明：

- `walkSpeed` 和 `runSpeed` 分開，讓一般移動與按住 `Shift` 跑步有不同速度。
- `MoveInput`、`FacingDirection`、`IsMoving`、`IsRunning` 是給動畫和攻擊系統讀取的狀態。
- 真正移動放在 `FixedUpdate()`，比較符合 Unity 物理更新流程；玩家輸入則放在 `Update()`，避免按鍵反應延遲。
- `facingDirection` 是 Day23 後半的核心資料，後面的攻擊方向與角色動畫都會用到它。

文案方向：

角色移動不再只有單一速度。玩家平常用 WASD 或方向鍵走路，按住 `Shift` 時會切換成跑步速度。`PlayerMovement` 也會記錄角色最後面向，讓角色停下來後仍然知道自己是面向上、下、左或右。這個面向資料後面會交給動畫和攻擊系統使用。

文章可用文字：

為了讓主角操作更像 RPG 角色，我把移動拆成走路和跑步。`walkSpeed` 是一般移動速度，`runSpeed` 是按住 `Shift` 後的速度。每次玩家輸入方向時，系統也會更新 `facingDirection`，這樣角色攻擊時可以沿用目前面向，不會出現往左走、攻擊卻仍然面朝下的問題。

### 23-12 解說 PlayerSpriteAnimator.cs：四方向待機、走路、跑步與攻擊

截圖內容：

- 程式碼檔案：`Assets/Game/Player/Scripts/PlayerSpriteAnimator.cs`
- 建議截圖行號：
  - 第 7 行到第 25 行：SpriteRenderer、移動狀態、待機/走路/跑步/攻擊 frame 陣列。
  - 第 27 行到第 40 行：用 `FacingGroup` 分成下、上、側面三組，並保存目前播放中的 frame、方向與攻擊狀態。
  - 第 42 行到第 63 行：初始化 `SpriteRenderer`、`PlayerMovement`，並停用舊 Animator，改由這支腳本直接換圖。
  - 第 65 行到第 95 行：Update 中依照攻擊狀態或移動狀態切換動畫。
  - 第 97 行到第 122 行：`PlayAttack()` 依照攻擊方向播放攻擊 frame。
  - 第 124 行到第 152 行：依照目前是跑步、走路或待機選擇 frame。
  - 第 154 行到第 162 行：依照上、下、側面選擇攻擊 frame。
  - 第 174 行到第 190 行：逐格播放 Sprite。

程式碼內容說明：

- 第 65 行到第 75 行：如果正在攻擊，就優先播放攻擊 frame，暫時不切回走路或待機動畫。
- 第 77 行到第 95 行：沒有攻擊時，根據 `PlayerMovement.FacingDirection` 和 `IsMoving` 選擇待機、走路或跑步 frame。
- 第 97 行到第 122 行：攻擊開始時會記錄攻擊方向、攻擊 frame 與持續時間，並決定側面攻擊時是否要 `flipX`。
- 第 124 行到第 152 行：同一個方向會依照 `IsRunning`、`IsMoving` 切換跑步、走路或待機圖。
- 第 174 行到第 190 行：用時間累積來推進 frame，平常動畫會循環，攻擊動畫播完後會回到一般狀態。

文案方向：

這裡沒有使用複雜 Animator Controller，而是用腳本直接控制 SpriteRenderer。角色會根據 `PlayerMovement` 的狀態選擇待機、走路或跑步 frame；方向則分成上、下、側面三組。左右共用側面圖，往左時用 `flipX` 翻轉。攻擊時會暫時覆蓋一般移動動畫，播放對應方向的攻擊 frame。

文章可用文字：

主角動畫分成兩層：平常移動時播放待機、走路或跑步；按下攻擊時，`PlayAttack()` 會短暫切換到攻擊 frame。這樣主角在森林中移動時能看出方向，攻擊時也不是靜止圖片扣血，而是角色本體真的做出攻擊動作。

### 23-13 解說 PlayerAttack.cs：主角如何攻擊

截圖內容：

- 程式碼檔案：`Assets/Game/Combat/Scripts/PlayerAttack.cs`
- 建議截圖行號：
  - 第 6 行到第 14 行：攻擊範圍、冷卻、攻擊方向、移動、攻擊視覺與動畫引用。
  - 第 19 行到第 42 行：初始化移動、武器視覺、攻擊特效與角色動畫引用。
  - 第 44 行到第 63 行：Update 中讀取鍵盤，按 `J` 呼叫 `Attack()`。
  - 第 65 行到第 82 行：攻擊冷卻、更新面向。
  - 第 84 行到第 98 行：播放手持武器視覺、角色攻擊動畫與攻擊視覺效果。
  - 第 100 行到第 128 行：從背包讀取拳套與符文總傷害，偵測攻擊範圍內的敵人並扣血。
  - 第 130 行到第 163 行：攻擊方向由方向鍵決定，沒有方向鍵時使用目前面向。
  - 第 165 行到第 179 行：畫面提示與 Scene 視窗攻擊範圍 Gizmo。

程式碼內容說明：

- 第 44 行到第 63 行：每幀先更新攻擊方向；玩家按 `J` 時才真正進入攻擊流程。
- 第 65 行到第 82 行：攻擊前先檢查冷卻時間，避免玩家連按造成每幀都判定傷害。
- 第 84 行到第 98 行：攻擊不是只有扣血，也會同步播放角色本體攻擊 frame 和前方攻擊效果。
- 第 100 行到第 128 行：`PlayerInventory.GetAttackDamage()` 讓拳套與符文的裝備數值參與傷害；`Physics2D.OverlapCircleAll()` 負責找出前方攻擊範圍內的敵人。
- 第 130 行到第 163 行：方向鍵優先決定攻擊方向；沒有方向鍵時，攻擊沿用 `PlayerMovement.FacingDirection`。
- 第 165 行到第 170 行：左上角只畫一行操作提示，包含 `Shift`、`J`、`B`、`E`、`T`。

文案方向：

主角攻擊鍵是 `J`。玩家按下 `J` 後，系統會先確認攻擊冷卻，接著根據目前方向更新角色面向，播放 `PlayerSpriteAnimator.PlayAttack()` 的攻擊動畫，再播放 `PlayerAttackVisual` 的攻擊視覺效果。最後用 `Physics2D.OverlapCircleAll()` 在角色前方做一次範圍偵測，命中的敵人會被扣血。

文章可用文字：

主角不是用碰到怪物就自動扣血，而是有明確攻擊鍵。攻擊方向優先使用玩家當下按住的方向鍵；如果玩家沒有按方向鍵，就使用角色最後面向。傷害則從 `PlayerInventory.GetAttackDamage()` 讀取，也就是目前拳套與符文提供的總傷害。這樣主角的操作、動畫、面向和裝備數值都能連在一起。

### 23-14 Unity 檢查 Player 元件

截圖內容：

- 任一場景中選取 `Player`
- Inspector 截到：
  - `PlayerMovement`
  - `PlayerSpriteAnimator`
  - `PlayerAttack`
  - `PlayerAttackVisual`
  - `PlayerVisual` 子物件

文案方向：

完成主角替換後，Player 不再只是單一 SpriteRenderer。`PlayerMovement` 負責移動和面向，`PlayerSpriteAnimator` 負責播放待機、走路、跑步與攻擊圖，`PlayerAttack` 負責攻擊輸入與傷害判定，`PlayerAttackVisual` 負責攻擊時的前衝和衝擊效果。

文章可用文字：

我把主角拆成「控制」和「視覺」兩部分。根物件保留碰撞與移動，`PlayerVisual` 負責顯示角色圖像。這樣攻擊時可以只讓角色畫面短暫前衝，不會把碰撞箱一起拉走，操作上比較穩定。

### 23-15 測試跑步與四方向顯示

截圖內容：

- Play Mode
- 按住 `Shift` 移動，截主角跑步畫面。
- 再截主角往上、往左或往右移動的畫面。
- 畫面要能看出主角不是固定面朝下。

文案方向：

測試時，玩家按住 `Shift` 後速度會變快，主角會播放跑步 frame。往不同方向移動時，角色圖像也會跟著改變。這一步確認主角已經具備基本 RPG 操作表現，不再是單方向移動的佔位角色。

### 23-16 測試主角拳擊攻擊

截圖內容：

- Play Mode
- 按 `J` 攻擊。
- 截到主角播放攻擊姿勢。
- 若在森林場景，可截主角攻擊小怪、小怪血條下降或訊息顯示「拳擊命中」。

文案方向：

最後測試攻擊。按下 `J` 後，主角會依照目前方向播放攻擊 frame，並在角色前方做一次攻擊範圍判定。若範圍內有小怪，就會根據拳套與符文的總傷害扣血，畫面也會顯示命中訊息。這代表主角的動畫和戰鬥判定已經接上。

## 後續保留

Day 23 仍不是正式 UI 系統。後續若時間足夠，可以再把 OnGUI 對話框換成 Canvas UI，並加入角色頭像、選項分支、任務階段限制。

主角動作系統目前先完成移動、跑步、四方向顯示與基礎拳擊攻擊。後續 Day25 會把這套攻擊接到森林小怪、怪物血量、掉落與精英怪門檻上。
