# Day 24 - 武器匠鋪與拳套符文主線

Day24 接續 Day23 的任務階段。玩家完成「村長 -> 酒館 -> 村長」之後，任務目標會變成：

`前往武器鋪，詢問拳套與符文情報`

這一天的文章重點不是完整戰鬥系統，而是把武器匠鋪做成可進入、可對話、可交易的主線場景，並正式把裝備方向改成「拳套 + 符文」。玩家不是去找一把劍，而是要透過拳套承受符文力量，最後尋找能對洞穴 Boss 生效的雷痕符文。

## 素材與工具口徑

武器匠鋪室內背景使用 ChatGPT 影像生成做出一張完整 2D 鐵匠鋪圖片，再放進 Unity 當作場景背景。這張圖包含鍛造爐、展示架、櫃台、工具與材料堆，所以 Day24 不再用 Tilemap 一格一格拼室內。

拳套與符文圖示則整理在：

`Assets/Game/Generated/Day26/GauntletsRunes`

文章可以寫成：為了讓商店與背包不要只有文字，後續使用 Codex 輔助產生並整理拳套、符文圖示，輸出成 Unity 可用的 PNG/Sprite。Day24 先建立商店資料與交易入口，Day26 再把背包與戰鬥數值補完整。

## 劇情重點

武器匠這段對話要說清楚幾件事：

- 主角主要靠拳套戰鬥，不把刀劍當作主武器。
- 低階符文和普通拳套只能用來清小怪。
- 精鐵拳套可以承受更高階符文。
- 赤鋒符文是昂貴的史詩符文，能對森林精英怪造成可觀傷害。
- 雷痕符文不是商品，而是在森林深處寶箱裡。
- 雷痕符文才是洞穴 Boss 主線的關鍵。

文章口徑：

村長只知道洞穴 Boss 不能靠普通攻擊傷到，但武器匠知道真正原因。主角來到武器匠鋪後，得知洞穴 Boss 的弱點不是普通武器，而是能把雷電導入拳套的古老符文。雷痕符文失蹤多年，最近酒館年輕人提到的森林五色寶光，很可能就是它重新現身的線索。

## Day24 截圖總表

### 24-1 建立武器匠鋪室內背景

截圖位置：

- 場景：`WeaponShopInteriorScene`
- 選取物件：`weapon_scene_0`
- 要截到：Scene 視窗完整鐵匠鋪背景，以及 Inspector 的 Transform。
- Inspector 觀察重點：Position 約為 `(0, 0, 0)`，Scale 約為 `(0.6, 0.6, 1)`。

文案重點：

建立武器匠鋪室內場景。這裡不再手工拼 Tilemap，而是使用生成的 2D 鐵匠鋪圖片作為背景，先把空間氛圍、展示架、鍛造爐與櫃台一次完成。

### 24-2 設定室內場景碰撞箱

截圖位置：

- 場景：`WeaponShopInteriorScene`
- 選取物件：`WeaponShopCollision`
- 要截到：Scene 視窗中的碰撞箱外框，以及 Hierarchy 中的 `WeaponShopCollision`。
- Inspector 觀察重點：碰撞箱用來阻擋牆面、展示架、櫃台、鍛造爐與雜物區。

文案重點：

背景圖只是視覺，玩家仍然可能穿過牆面或家具。所以這一步要補上碰撞箱，讓鐵匠鋪的櫃台、牆、鍛造爐和展示架都變成不能穿越的實體物件。

### 24-3 設定室內入口與出口

截圖位置：

- 場景：`WeaponShopInteriorScene`
- 選取物件 1：`Entry_FromVillage`
- 選取物件 2：`Exit_ToVillage`
- 要截到：兩個點位在場景中的位置。
- Inspector 觀察重點：
  - `Exit_ToVillage` 有 `BoxCollider2D`，並勾選 `Is Trigger`。
  - `Exit_ToVillage` 有 `SceneTransitionPoint`。
  - Target Scene Name 指向 `VillageScene`。
  - Target Spawn Point Name 指向 `Entry_FromWeaponShop`。

文案重點：

室內場景需要兩種點位：玩家從村莊進來時出生的 `Entry_FromVillage`，以及離開店鋪回村莊的 `Exit_ToVillage`。這樣武器匠鋪才不是單獨測試場景，而是村莊地圖的一部分。

### 24-4 設定村莊進入武器匠鋪

截圖位置：

- 場景：`VillageScene`
- 選取物件：`Exit_ToWeaponShop`
- 要截到：村莊裡武器匠鋪門口的 Trigger。
- Inspector 觀察重點：
  - `BoxCollider2D` 勾選 `Is Trigger`。
  - `SceneTransitionPoint` 的 Target Scene Name 是 `WeaponShopInteriorScene`。
  - Target Spawn Point Name 是 `Entry_FromVillage`。

文案重點：

玩家從村莊武器匠鋪門口進入室內。這一步把村莊外觀和室內場景接起來，讓主線能自然從村長家推進到武器匠鋪。

### 24-5 放置武器匠 NPC

截圖位置：

- 場景：`WeaponShopInteriorScene`
- 選取物件：`BlacksmithNPC`
- 要截到：武器匠站在店內櫃台或工作區附近。
- Inspector 觀察重點：
  - `SpriteRenderer` 顯示 NPC 圖像。
  - `BoxCollider2D` 沒有勾選 `Is Trigger`，因為 NPC 本體要阻擋玩家。
  - `Sorting Order` 要讓 NPC 顯示在地板和背景物件之上。

文案重點：

武器匠不是背景裝飾，而是主線 NPC。本體碰撞箱負責讓玩家不能直接穿過 NPC，互動用的 Trigger 另外放在他前方。

### 24-6 設定武器匠對話區

截圖位置：

- 場景：`WeaponShopInteriorScene`
- 選取物件：`BlacksmithInteractionArea`
- 要截到：Scene 視窗中 NPC 前方的互動範圍。
- Inspector 觀察重點：
  - `BoxCollider2D` 勾選 `Is Trigger`。
  - `NpcDialogueInteraction` 掛在 `BlacksmithInteractionArea` 上。
  - Interact Prompt 是 `按 E 與武器匠交談`。

文案重點：

NPC 本體和對話觸發區要分開。玩家靠近 `BlacksmithInteractionArea` 後按 `E`，才會開始主線對話。這樣玩家能被 NPC 擋住，但不需要完全貼到 NPC 身上才觸發對話。

### 24-7 設定武器匠任務階段對話

截圖位置：

- 場景：`WeaponShopInteriorScene`
- 選取物件：`BlacksmithInteractionArea`
- Inspector 展開：`NpcDialogueInteraction`
- 要截到：
  - `Use Dialogue Sequences`
  - `Required Quest Stage = AskBlacksmith`
  - `Use Speaker Lines`
  - `Speaker Lines`
  - `Objective Text On Complete = 前往森林，調查五色寶光`
  - `Quest Stage On Complete = ForestInvestigation`

文案重點：

這段對話必須被任務階段鎖住。只有玩家完成村長與酒館流程後，任務階段變成 `AskBlacksmith`，武器匠才會講拳套與符文的情報。對話結束後，任務目標更新為前往森林調查五色寶光。

### 24-8 測試武器匠主線對話

截圖位置：

- Play Mode
- 場景：`WeaponShopInteriorScene`
- 操作：靠近武器匠，按 `E`。
- 要截到：對話框中出現武器匠與主角的 Speaker Lines。
- 建議截圖內容：
  - 主角問拳套和符文。
  - 武器匠提到 `赤鋒符文`。
  - 武器匠提到 `雷痕符文`。

文案重點：

這裡要展示不是單純 NPC 獨白，而是主角和武器匠互相對話。武器匠負責把玩家從村莊調查引導到森林探索，並埋下精英怪與雷痕符文的目標。

### 24-9 測試對話完成後更新目標

截圖位置：

- Play Mode
- 操作：把武器匠對話點完。
- 要截到：左上角或任務 UI 顯示 `前往森林，調查五色寶光`。

文案重點：

對話結束後不能只停在原地。任務狀態必須推進，讓玩家知道下一步要去森林。這也是後面 Day25 森林戰鬥的入口。

### 24-10 設定拳套與符文交易區

截圖位置：

- 場景：`WeaponShopInteriorScene`
- 選取物件：`WeaponShopTradingArea`
- 要截到：Scene 視窗中的交易 Trigger 和 Inspector。
- Inspector 觀察重點：
  - `BoxCollider2D` 勾選 `Is Trigger`。
  - `WeaponShopInteraction`
  - `Interact Prompt = 按 T 打開拳套與符文交易`
  - `Weapons For Sale` 清單中有拳套與符文。

文案重點：

武器匠同時負責劇情與交易，所以按鍵要拆開。`E` 是交談，`T` 是交易。交易區不一定要和 NPC 對話區完全重疊，可以放在櫃台附近，讓玩家站到櫃台前才打開買賣介面。

### 24-11 測試交易畫面

截圖位置：

- Play Mode
- 操作：走到 `WeaponShopTradingArea`，按 `T`。
- 要截到：
  - 交易介面標題 `拳套與符文`
  - 玩家金幣
  - 目前拳套
  - 目前符文
  - 拳擊總傷害
  - 商店販售清單
  - 玩家背包清單

文案重點：

交易畫面同時顯示商店和背包。玩家可以買拳套、買符文、出售掉落物，也可以在背包中切換目前裝備。赤鋒符文價格很高，玩家第一次通常買不起，必須先去森林刷小怪賺金幣。

### 24-12 Day24 收尾截圖

截圖位置：

- 可以使用 Play Mode 中站在武器匠鋪內、畫面同時看到主角、武器匠和店鋪背景。
- 或使用交易畫面搭配任務目標。

文案重點：

Day24 完成後，武器匠鋪已經具備三個功能：場景可進出、NPC 可對話、商店可交易。接下來 Day25 就能把玩家帶進森林，開始用拳套和符文系統打小怪、刷金幣，並準備挑戰森林精英怪。

## 程式碼截圖節點

### A. 場景與工具常數

截圖檔案：

`Assets/Game/Editor/Day24WeaponShopOrganizer.cs`

截圖行數：

`11-28`

程式碼內容說明：

- `11-13`：定義村莊場景與武器匠鋪室內場景名稱，之後建立傳送點時會用這些字串指定目標場景。
- `14-16`：定義武器匠鋪場景檔案路徑、背景圖路徑與生成圖示資料夾，讓工具知道要打開哪個場景、載入哪張背景、從哪裡找拳套符文圖示。
- `17-28`：集中定義武器匠、入口、出口、交易區與森林目標文字等常數。這些常數讓後面建立 NPC、互動區、交易區和任務目標時都使用同一套名稱，避免手動輸入錯字。

要截到：

- `VillageSceneName`
- `WeaponShopSceneName`
- `WeaponShopScenePath`
- `WeaponShopBackgroundPath`
- `GeneratedWeaponFolder`
- `ForestObjective`

文案重點：

這裡集中定義 Day24 會操作哪些場景、讀取哪張背景圖、拳套符文圖示放在哪個資料夾，以及對話完成後要顯示的森林任務目標。

### B. 拳套與符文商品資料

截圖檔案：

`Assets/Game/Editor/Day24WeaponShopOrganizer.cs`

截圖行數：

`67-78`

程式碼內容說明：

- `67-69`：建立普通拳套資料，例如練習拳套、皮革拳套，作為玩家前期清小怪的基礎裝備。
- `70-72`：建立精英拳套與普通符文資料，讓玩家能逐步從低階裝備過渡到更高傷害配置。
- `73-75`：建立精英符文與史詩符文資料，其中赤鋒符文價格高、傷害高，是挑戰森林精英怪的重要商品。
- `76-78`：保留傳說符文雷痕符文資料，但它不是商店商品，而是後續森林寶箱或主線獎勵要使用的關鍵道具。

要截到：

- 練習拳套
- 皮革拳套
- 精鐵拳套
- 裂紋符文
- 微光符文
- 焰紋符文
- 守心符文
- 赤鋒符文
- 雷痕符文

文案重點：

這裡是拳套與符文的資料表。普通與精英裝備可以讓玩家刷森林小怪，赤鋒符文價格最高，用來挑戰森林精英怪。雷痕符文雖然寫在資料裡，但它不是商店商品，而是森林深處寶箱獎勵。

### C. 武器匠劇情對話內容

截圖檔案：

`Assets/Game/Editor/Day24WeaponShopOrganizer.cs`

截圖行數：

`80-119`

程式碼內容說明：

- `80-85`：建立武器匠對話陣列，讓工具能一次把多句主角與武器匠台詞寫入 `NpcDialogueInteraction`。
- `86-98`：用主角提問引出 Boss、拳套與符文的設定，說明主角不是尋找刀劍，而是需要能承受符文力量的拳套。
- `99-110`：讓武器匠解釋赤鋒符文、雷痕符文與森林五色寶光的關聯，將商店商品和後續森林任務接起來。
- `111-119`：收束對話，把玩家下一步導向森林調查，讓 Day24 的武器匠劇情自然銜接 Day25 森林探索。

要截到：

- `BlacksmithLines`
- 主角台詞
- 武器匠台詞
- 台詞中出現拳套、赤鋒符文、雷痕符文、森林寶光

文案重點：

這段不是系統說明，而是劇情對話。主角不是被動聽 NPC 講設定，他會追問 Boss、符文、森林寶光與自己買不起裝備的問題，讓任務推進比較自然。

### D. 建立 BlacksmithNPC 與互動區

截圖檔案：

`Assets/Game/Editor/Day24WeaponShopOrganizer.cs`

截圖行數：

`175-223`

程式碼內容說明：

- `175-184`：建立或找到 `WeaponShopNPCs` 根物件，讓武器匠 NPC 與互動區集中放在同一個 Hierarchy 分組底下。
- `185-197`：建立 `BlacksmithNPC`，指定武器匠 Sprite、排序層級與位置，讓 NPC 顯示在武器匠鋪櫃台附近。
- `198-207`：在 NPC 本體上加入 `BoxCollider2D`，這個碰撞箱不是 Trigger，而是用來阻擋玩家直接穿過武器匠。
- `208-218`：另外建立 `BlacksmithInteractionArea`，加上 Trigger 碰撞箱，作為玩家按 `E` 觸發劇情對話的範圍。
- `219-223`：把 `NpcDialogueInteraction` 套到互動區，並呼叫對話設定函式，將武器匠劇情內容寫進 Inspector。

要截到：

- `SetupBlacksmithNpcAndDialogue`
- `BlacksmithNPC`
- `BlacksmithInteractionArea`
- NPC 本體 `BoxCollider2D`
- Trigger 區域設定
- `ApplyBlacksmithDialogueSequence(dialogue)`

文案重點：

這段工具做的是正常 Unity 流程的自動化：建立 NPC、指定 Sprite、加上本體碰撞箱，再另外建立互動 Trigger，最後把對話資料掛上去。

### E. 測試用任務階段工具

截圖檔案：

`Assets/Game/Editor/Day24WeaponShopOrganizer.cs`

截圖行數：

`226-235`

程式碼內容說明：

- `226-229`：建立一個測試用選單工具，讓開發時可以直接把主線階段切到 `AskBlacksmith`。
- `230-235`：呼叫 `EnsureMainQuestState(MainQuestStage.AskBlacksmith)`，確保場上有主線狀態物件，並把目前階段設成可以觸發武器匠對話的狀態。

要截到：

- `PrepareAskBlacksmithTestStage`
- `EnsureMainQuestState(MainQuestStage.AskBlacksmith)`

文案重點：

這個工具只用來測試，不是正式流程。正式遊戲中，`AskBlacksmith` 會由第二次村長對話推進；測試時才直接把任務階段設成 `AskBlacksmith`，方便在武器匠鋪直接按 Play 檢查對話。

### F. 玩家初始拳套與符文

截圖檔案：

`Assets/Game/Editor/Day24WeaponShopOrganizer.cs`

截圖行數：

`530-544`

程式碼內容說明：

- `530-533`：找到或建立玩家背包資料，並設定起始金幣，讓玩家進入拳套符文系統時不是空資料。
- `534-539`：設定 `startingWeapons` 陣列，把練習拳套與裂紋符文放入起始背包。
- `540-544`：把修改寫回 Unity 物件並標記為已變更，確保場景儲存後起始背包資料不會消失。

要截到：

- `startingGold`
- `startingWeapons`
- 初始裝備包含練習拳套與裂紋符文

文案重點：

玩家一開始不能空手進入拳套符文系統，所以背包預設給一個練習拳套和一枚低階符文。這樣玩家即使買不起赤鋒符文，也能先去森林清小怪。

### G. 建立交易區與販售清單

截圖檔案：

`Assets/Game/Editor/Day24WeaponShopOrganizer.cs`

截圖行數：

`550-578`

程式碼內容說明：

- `550-557`：建立或找到 `WeaponShopTradingArea`，這是玩家站進去後可以按 `T` 開商店的交易 Trigger。
- `558-563`：設定交易區位置、大小與 `BoxCollider2D`，並勾選 `Is Trigger`，讓它只負責偵測玩家是否進入範圍。
- `564-566`：掛上 `WeaponShopInteraction`，並設定交易提示文字為 `按 T 打開拳套與符文交易`。
- `567-575`：把販售清單大小設成 8，依序寫入普通拳套、精英拳套、低階符文、精英符文與赤鋒符文。
- `576-578`：套用序列化資料並標記物件已修改，讓交易區和販售清單真正存進場景。

要截到：

- `CreateWeaponShopTradingArea`
- `WeaponShopTradingArea`
- `interactPrompt = "按 T 打開拳套與符文交易"`
- `weaponsForSale.arraySize = 8`
- `AssignWeaponItem`

文案重點：

交易區是獨立 Trigger。玩家站進交易區後按 `T`，才會打開商店。商店販售 8 個商品，包含普通拳套、精英拳套、低階符文、精英符文與赤鋒符文。

### H. 對話序列與任務推進

截圖檔案：

`Assets/Game/Editor/Day24WeaponShopOrganizer.cs`

截圖行數：

`708-739`

程式碼內容說明：

- `708-713`：找到 `NpcDialogueInteraction` 的序列化欄位，設定武器匠預設名稱與互動提示。
- `714-719`：啟用任務階段對話序列，讓這段對話只在玩家主線推進到 `AskBlacksmith` 時出現。
- `720-727`：設定序列名稱、要求階段、說話者名稱、提示文字，以及完成後要更新的目前目標。
- `728-733`：設定對話完成後推進主線階段到 `ForestInvestigation`，讓玩家下一步被導向森林。
- `734-739`：把 `BlacksmithLines` 中的多說話者台詞逐句寫入 `speakerLines`，讓對話框能顯示主角與武器匠輪流說話。

要截到：

- `ApplyBlacksmithDialogueSequence`
- `interactPrompt = "按 E 與武器匠交談"`
- `useDialogueSequences = true`
- `requiredQuestStage = AskBlacksmith`
- `objectiveTextOnComplete = ForestObjective`
- `questStageOnComplete = ForestInvestigation`
- `speakerLines`

文案重點：

這段是避免劇情跳關的關鍵。武器匠對話只會在 `AskBlacksmith` 階段出現，對話完成後才會把任務推到 `ForestInvestigation`，讓玩家前往森林。

### I. 品質分類 enum

截圖檔案：

`Assets/Game/Inventory/Scripts/WeaponQuality.cs`

截圖行數：

`1-7`

程式碼內容說明：

- `1-7`：定義裝備品質列舉，包含 `Common`、`Elite`、`Epic`、`Legendary` 四種品質。商店、背包、掉落與劇情門檻都用這個 enum 判斷裝備階級。

要截到：

- `Common`
- `Elite`
- `Epic`
- `Legendary`

文案重點：

品質分類仍然保留四階。普通與精英用於刷小怪與過渡，史詩用於森林精英怪，傳說用於洞穴 Boss 主線。

### J. 拳套與符文資料結構

截圖檔案：

`Assets/Game/Inventory/Scripts/WeaponItem.cs`

截圖行數：

`4-22`

程式碼內容說明：

- `4-8`：定義 `EquipmentKind`，用 `Gauntlet` 和 `Rune` 區分拳套與符文，避免所有裝備都被當成同一種武器。
- `10-18`：定義 `WeaponItem` 的基本欄位，包含 id、顯示名稱、種類、品質、圖示、傷害、買價與賣價。
- `20-22`：提供建構或序列化時會用到的資料入口，讓 Editor 工具和商店清單能直接建立拳套符文商品。

要截到：

- `EquipmentKind`
- `Gauntlet`
- `Rune`
- `weaponId`
- `displayName`
- `itemType`
- `quality`
- `icon`
- `damage`
- `buyPrice`
- `sellPrice`

文案重點：

雖然檔名仍叫 `WeaponItem`，但正式設計已經改成拳套與符文。`EquipmentKind` 用來區分拳套和符文，`damage` 則代表拳套基礎傷害或符文加成。

### K. 道具自動轉成新版拳套符文

截圖檔案：

`Assets/Game/Inventory/Scripts/WeaponItem.cs`

截圖行數：

`47-85`

程式碼內容說明：

- `47-55`：進入 `NormalizeForCurrentStory()`，先檢查資料是否需要轉換，避免已經是新版拳套符文的物件被重複改寫。
- `56-70`：依照舊版武器 id 或名稱，把舊資料轉成新版拳套或符文，例如把舊武器轉成練習拳套、皮革拳套或裂紋符文。
- `71-80`：把重要劇情道具轉成赤鋒符文或雷痕符文，確保商店、掉落與寶箱都使用新版劇情口徑。
- `81-85`：補齊種類、品質、傷害與價格等欄位，讓舊資料即使來自先前場景，也能在新版背包與交易 UI 中正常顯示。

要截到：

- `NormalizeForCurrentStory`
- 舊資料 id 轉換成練習拳套、皮革拳套、裂紋符文、赤鋒符文、雷痕符文

文案重點：

這段是版本整理用。因為前面曾經做過舊裝備資料，所以這裡會把舊 id 轉換成新版拳套與符文，避免背包、掉落或商店還殘留不符合劇情的名稱。

### L. 背包中的買、賣、裝備與傷害計算

截圖檔案：

`Assets/Game/Inventory/Scripts/PlayerInventory.cs`

截圖行數：

- `20-31`：目前拳套、目前符文與總傷害欄位。
- `79-106`：購買商品。
- `112-151`：出售背包道具。
- `158-181`：裝備拳套或鑲嵌符文。
- `186-224`：取得目前拳套、目前符文與拳擊總傷害。

程式碼內容說明：

- `20-31`：記錄目前裝備中的拳套格子、符文格子、起始金幣與起始道具。背包用這些欄位知道玩家目前穿戴什麼，以及攻擊力該怎麼計算。
- `79-106`：`TryBuyWeapon()` 先檢查玩家金幣是否足夠，足夠才扣錢並把商品複製進背包；不足時回傳提示訊息。
- `112-151`：`TrySellWeaponAt()` 根據背包格子出售道具，賣出後增加金幣，並處理如果賣掉的是目前裝備，裝備索引要跟著清掉或修正。
- `158-181`：`TryEquipWeaponAt()` 判斷道具種類，如果是拳套就設為目前拳套，如果是符文就設為目前符文，讓兩種裝備可以同時存在。
- `186-224`：提供 `GetEquippedGauntlet()`、`GetEquippedRune()` 和 `GetAttackDamage()` 等查詢函式。戰鬥系統和商店 UI 都透過這裡取得目前裝備和總拳擊傷害。

文案重點：

背包不是只存道具。它還要知道玩家目前穿戴哪個拳套、鑲嵌哪枚符文，以及最後攻擊力是多少。商店交易、森林掉落和戰鬥傷害都會讀取這份資料。

### M. 交易介面按鍵與 UI

截圖檔案：

`Assets/Game/Inventory/Scripts/WeaponShopInteraction.cs`

截圖行數：

- `4-8`：交易腳本掛在 Trigger 物件上，並保留 `interactPrompt` 與 `weaponsForSale` 販售清單。
- `32-38`：啟動時把 `BoxCollider2D` 設為 Trigger，整理販售商品資料，並確保場上有 `PlayerInventory`。
- `40-57`：每幀檢查玩家是否在交易範圍內；玩家在範圍內按 `T` 開關交易畫面，按 `Esc` 關閉。
- `60-75`：玩家進出 Trigger 時更新 `playerInRange`，離開交易範圍時關閉商店。
- `77-141`：用 `OnGUI()` 畫出交易面板，包含標題、金幣、目前拳套、目前符文、拳擊總傷害，以及左右兩欄清單。
- `143-153`：左側 `武器匠販售` 清單，每列顯示圖示、名稱、種類、品質、威力與價格，按 `購買` 呼叫背包購買邏輯。
- `156-179`：右側 `玩家背包` 清單，每列顯示背包道具，並提供 `裝備 / 鑲嵌` 與 `出售` 按鈕。
- `181-192`：從 `WeaponItem` 載入圖示並繪製在交易列上。
- `202-235`：用碰撞範圍與玩家位置判斷是否仍在交易區，避免玩家離開櫃台後商店還開著。
- `283-308`：把品質、拳套 / 符文種類、威力文字與按鈕文字轉成玩家看得懂的中文。

文案重點：

`WeaponShopInteraction` 是武器匠鋪交易區的核心腳本。它不是掛在武器匠 NPC 本體上，而是掛在櫃台附近的 `WeaponShopTradingArea` Trigger 上。這樣玩家必須站到交易區附近，按 `T` 才會開啟買賣畫面；`E` 則保留給 `BlacksmithInteractionArea` 的劇情對話。

腳本啟動時會先確保交易區 Collider 是 Trigger，接著整理販售清單中的拳套與符文資料，避免舊版武器 id 或圖示殘留。`Update()` 裡只做兩件事：第一，檢查玩家是否仍在交易範圍內；第二，玩家按 `T` 時切換商店開關，按 `Esc` 時關閉商店。

交易畫面用 `OnGUI()` 繪製。面板上方顯示標題 `拳套與符文`、玩家金幣、目前拳套、目前符文與拳擊總傷害；下方分成左右兩欄，左側是武器匠販售清單，右側是玩家背包。玩家可以在同一個畫面買入商品、出售背包道具，也可以直接切換目前拳套或鑲嵌符文。

左側販售列由 `DrawSaleRow()` 負責，按下 `購買` 後會呼叫 `PlayerInventory.TryBuyWeapon()`，由背包系統判斷金幣是否足夠並加入道具。右側背包列由 `DrawInventoryRow()` 負責，按下 `裝備` 或 `鑲嵌` 會呼叫 `TryEquipWeaponAt()`，按下 `出售` 則呼叫 `TrySellWeaponAt()`。因此交易腳本本身只負責 UI 與按鈕流程，真正的金幣、背包內容與裝備狀態都交給 `PlayerInventory` 管理。

這樣拆分之後，武器匠鋪同時具備兩種互動：靠近武器匠按 `E` 推進主線對話，站到交易區按 `T` 開啟拳套與符文買賣。兩套互動不共用同一顆按鍵，也不會互相打斷。

## 正常 Unity 流程對應工具

### `IronRPG > Day 24 > 1 Create Weapon Shop Interior Scene`

正常流程：

1. 建立 `WeaponShopInteriorScene`。
2. 加入 Build Settings。
3. 建立背景物件 `weapon_scene_0`。
4. 指定生成的鐵匠鋪背景圖。
5. 設定 Transform 為 Position `(0, 0, 0)`、Scale `(0.6, 0.6, 1)`。
6. 保留 `Player`、`Main Camera`、`MainQuestState`。
7. 建立後續會用到的根物件，例如碰撞、NPC、Marker。

工具只是把以上流程自動化，文章不要寫成「按工具就完成」，要寫成「建立場景、放入背景、調整 Transform」。

### `IronRPG > Day 24 > 2 Setup Village To Weapon Shop Transition`

正常流程：

1. 回到 `VillageScene`。
2. 在武器匠鋪外觀門口建立 `Exit_ToWeaponShop`。
3. 加上 Trigger 碰撞箱。
4. 掛上 `SceneTransitionPoint`。
5. Target Scene Name 設成 `WeaponShopInteriorScene`。
6. Target Spawn Point Name 設成 `Entry_FromVillage`。
7. 建立 `Entry_FromWeaponShop` 作為離開室內後回村莊的位置。

### `IronRPG > Day 24 > 3 Setup Blacksmith NPC And Dialogue`

正常流程：

1. 在武器匠鋪內建立 `WeaponShopNPCs`。
2. 建立 `BlacksmithNPC`。
3. 指定武器匠圖像。
4. 設定本體碰撞箱。
5. 建立 `BlacksmithInteractionArea`。
6. 互動區使用 Trigger。
7. 掛上 `NpcDialogueInteraction`。
8. 設定 `AskBlacksmith` 階段對話。
9. 對話完成後推進到 `ForestInvestigation`。

### `IronRPG > Day 24 > 4 Prepare AskBlacksmith Test Stage`

這是測試輔助，不建議當正式文章截圖。

用途：

把 `MainQuestState` 暫時設成 `AskBlacksmith`，讓你可以直接在武器匠鋪 Play Mode 測試對話。正式文章可以一句帶過：「測試時暫時把任務階段切到 AskBlacksmith，正式流程會由村長對話推進。」

### `IronRPG > Day 24 > 6 Setup Weapon Shop Buy Sell System`

正常流程：

1. 確認玩家身上有 `PlayerInventory`。
2. 建立 `WeaponShopTradingArea`。
3. 加上 Trigger 碰撞箱。
4. 掛上 `WeaponShopInteraction`。
5. 交易提示設定為 `按 T 打開拳套與符文交易`。
6. 販售清單放入拳套與符文。
7. `E` 保留給劇情對話，`T` 用來打開交易。

## Day24 結束時應該達成的狀態

- 玩家可以從村莊進入武器匠鋪。
- 玩家可以從武器匠鋪回到村莊。
- 武器匠 NPC 有本體碰撞箱。
- 武器匠對話區可以按 `E` 觸發。
- 武器匠對話只在 `AskBlacksmith` 階段出現。
- 對話完成後任務推進到 `ForestInvestigation`。
- 店內有 `WeaponShopTradingArea`。
- 玩家可以按 `T` 打開拳套與符文交易畫面。
- 商店販售拳套、低階符文、精英符文與赤鋒符文。
- 雷痕符文保留給森林深處寶箱，不在商店販售。

Day24 完成後，Day25 就能進入森林戰鬥：玩家先用普通拳套和低階符文刷小怪，累積金幣後回武器匠鋪購買赤鋒符文，再準備挑戰森林精英怪。
