# Day 27 - 洞穴場景、小怪區、Boss 房封鎖與核心護甲機制

Day27 接續 Day26。Day26 已經完成主角升級、死亡重生、藥草補給，以及武器匠確認雷痕符文後把任務推進到洞穴。

Day27 的重點不是重新介紹森林，也不是重寫拳套符文系統，而是正式把玩家帶進最後區域：洞穴。

這一天完成到 Boss 戰第一階段為止：

1. 建立通往洞穴與洞穴內部場景。
2. 調整洞穴鏡頭與主角速度，讓洞穴看起來比其他場景更壓迫、更巨大。
3. 準備 3 種洞穴小怪，並把 30 隻小怪放進洞穴。
4. 設定洞穴小怪血量、經驗、金幣、藥草與傷害。
5. 進入 Boss 房後用藤蔓封住回頭路。
6. Boss 不建立獨立物件，Boss 本體存在於背景圖上，只用碰撞與核心機制模擬戰鬥。
7. 玩家踩到 Boss 對話觸發器後，自動跑到 `dialogmark`，面向上方 Boss，並播放開戰前對話。
8. Boss 對話結束後，右上角出現棕色 Boss 血條。
9. 三個核心護甲隨機亮起一個，玩家必須依序打碎護甲、攻擊核心，才能扣 Boss 血。

Day27 先停在「Boss 血條與核心護甲機制」完成。Boss 的主動攻擊、藤蔓技能、核心破壞後的完整 Boss 階段，留到下一天。

## Day27 文章口徑

Day27 可以寫成「從一般探索區進入最終 Boss 房」。前半段是洞穴探索，後半段是 Boss 房規則建立。

文章要特別說清楚：

- 洞穴不是森林換皮，而是更大的場景。
- 主角在洞穴中縮小，移動速度降低，讓場景比例顯得更巨大。
- 洞穴小怪比森林小怪強很多，玩家不能再用森林前期打法硬衝。
- Boss 圖像已經畫在背景裡，不額外做一個可移動 Boss 物件。
- Boss 戰不是直接打 Boss 本體，而是打三個核心護甲。
- 雷痕符文對 Boss 增傷 300%，這個詞條在 Day26 已經鋪好，Day27 正式進入戰鬥規則。

## 素材與工具口徑

洞穴入口與洞穴內部背景圖使用 ChatGPT 生成後放入專案，再匯入 Unity 作為完整背景圖。這裡和武器匠鋪一樣，不再用 Tilemap 一格一格拼，而是先用完整 2D 圖建立氛圍，再補上碰撞箱、怪物、觸發器與戰鬥邏輯。

洞穴小怪素材使用 Codex 影像生成工具補出 3 種像素風小怪：

- `cave_crystal_slime.png`
- `cave_stone_beetle.png`
- `cave_root_imp.png`

輸出位置：

`Assets/Game/Generated/Day27/Monsters`

Boss 房封路藤蔓與核心石頭護甲使用 ChatGPT 生成圖片，再由 Codex 整理成透明 PNG：

- `Assets/Game/Generated/Day27/Boss/vines_transparent.png`
- `Assets/Game/Generated/Day27/Boss/stone_armor_transparent.png`

文章口徑可以寫成：洞穴背景與 Boss 房視覺先用生成圖建立整體風格；可互動部分則用 Unity 物件、Collider 與腳本補上。

## Day27 截圖總表

### 27-1 建立洞穴入口場景

截圖位置：

- 場景：`ToCaveScene`
- Scene 視窗要截到：洞穴入口背景、主角、入口道路、周圍岩壁或地形。
- Hierarchy 建議截到：`Player`、`Main Camera`、入口背景物件、轉場點。
- Inspector 建議選取洞穴入口背景物件，截到 Transform 與 SpriteRenderer。

文案重點：

Day26 結束後，任務目標已經推進到洞穴。因此 Day27 先建立 `ToCaveScene`，讓玩家不是直接被丟進 Boss 地圖，而是先經過一個通往洞穴的過渡場景。這個場景負責營造「即將進入最終區域」的氣氛，也讓村莊、森林與洞穴之間的場景流程更完整。

這裡的重點是先放好完整背景，再確認主角站位和鏡頭能拍到洞口。若文章要寫得自然，可以說：先建立洞穴入口作為最終區域的前哨，讓玩家從外部世界逐步進入洞穴深處。

### 27-2 建立洞穴內部背景

截圖位置：

- 場景：`CaveScene`
- 選取洞穴背景物件。
- Scene 視窗要截到：完整洞穴地圖，包含一般探索區、通往 Boss 房的通道、Boss 房。
- Inspector 要截到：背景物件的 Transform、SpriteRenderer、Sorting Order。
- 如果畫面太大，可以截一張縮遠的 Scene 視窗，再補一張 Game 視窗確認實際遊戲視角。

文案重點：

洞穴內部不再用 Tilemap 一格一格拼，而是使用完整 2D 背景圖。這張背景圖直接包含三個區域：前段探索區、中段通往 Boss 房的道路、最上方的 Boss 房。這樣做的好處是畫面風格更統一，也能讓 Boss 房的巨大背景和洞穴牆面一次成形。

文章可以補充：背景圖只負責視覺，真正的可通行範圍仍然要靠 Unity 裡的 Collider 來限制。也就是說，先把洞穴畫面搭起來，再用碰撞箱把玩家能走的路線框出來。

### 27-3 設定洞穴碰撞箱

截圖位置：

- 場景：`CaveScene`
- 選取洞穴碰撞根物件或任一代表性碰撞箱。
- Scene 視窗要截到：多個綠色 Collider 沿著洞穴牆壁、岩石、道路邊界鋪設。
- Inspector 建議截到：`BoxCollider2D` 或 `PolygonCollider2D`，並確認它不是 Trigger。
- 如果手動鋪了很多碰撞箱，可以截總覽，再補一張局部細節，例如 Boss 房入口或狹窄通道。

文案重點：

完整背景圖不能直接阻擋玩家。如果不加碰撞箱，主角會直接穿過岩壁或走出地圖。因此這一步要沿著洞穴牆壁、石塊、道路兩側與 Boss 房邊界補上碰撞箱。

洞穴碰撞箱的重點不是塞滿整張圖，而是讓玩家只能走在設計好的道路上。尤其是通往 Boss 房的唯一道路，需要用碰撞箱把玩家導向正確位置，後續藤蔓封路和 Boss 觸發器才會有意義。

### 27-4 調整洞穴主角比例與鏡頭

截圖位置：

- Play Mode
- 場景：`CaveScene`
- 要截到：主角比例較小，畫面能看出洞穴空間很大。
- 可以補截 Inspector 中 `Player` 的 Transform Scale。
- 可以補截 `Main Camera` 的 Orthographic Size 或 Camera Follow 設定。

文案重點：

洞穴要和村莊、武器匠鋪、主角家形成差異。這裡把主角比例調小，讓洞穴背景看起來更巨大；同時調整鏡頭高度和跟隨效果，避免玩家只看到自己腳邊一小塊地圖。

另外，洞穴中主角移動速度也比一般場景慢一些。這不是單純為了限制玩家，而是為了讓玩家感覺自己正在穿過一個龐大、沉重、危險的空間。

### 27-5 生成 3 種洞穴小怪素材

截圖位置：

- Project 視窗打開 `Assets/Game/Generated/Day27/Monsters`
- 要截到：
  - `cave_crystal_slime.png`
  - `cave_stone_beetle.png`
  - `cave_root_imp.png`

文案重點：

洞穴不能只沿用森林怪物，所以新增 3 種洞穴小怪。它們的色調和外形更接近岩石、晶體與根系，讓玩家感覺自己進入新的危險區域。

### 27-6 建立洞穴小怪 Prefab

截圖位置：

- 執行 `IronRPG > Day 27 > Create Cave Small Monster Prefabs Only`
- Project 視窗打開 `Assets/Game/Generated/Day27/Monsters/Prefabs`
- 要截到 3 個 prefab：
  - `CaveCrystalSlime`
  - `CaveStoneBeetle`
  - `CaveRootImp`

文案重點：

先把 3 張小怪圖做成 Prefab。每個 Prefab 會沿用森林小怪已經建立好的基本戰鬥架構，例如血量、追擊、接觸傷害、視覺動畫與掉落系統。這裡不重新解釋小怪腳本，只需要說明：洞穴怪物使用同一套敵人基礎系統，但數值調整成後期版本。

這一步只建立 Prefab，不把小怪放進場景。這樣素材整理和場景擺放可以分開處理，後面如果要調整洞穴小怪位置，也不會每次都重新做素材。

### 27-7 放置 30 隻洞穴小怪

截圖位置：

- 執行 `IronRPG > Day 27 > Place 30 Cave Small Monsters`
- 場景：`CaveScene`
- 選取 `CaveSmallMonsters`
- 要截到：Hierarchy 中 30 隻洞穴小怪，以及 Scene 視窗中怪物分布。

文案重點：

洞穴是最終 Boss 前的消耗路線，所以小怪數量比森林更密集。這裡把 30 隻小怪分布在洞穴探索區和通往 Boss 房的路上，形成一段真正需要玩家推進的戰鬥路線。

截圖時可以特別讓畫面看到小怪不是集中堆在一個點，而是分散在通道、轉角和空地附近。文章可以寫成：玩家不是走幾步就進 Boss 房，而是必須先通過洞穴小怪區，消耗藥草、獲得經驗與金幣，才抵達最終戰場。

### 27-8 更新洞穴小怪數值

截圖位置：

- 執行 `IronRPG > Day 27 > Update Cave Small Monster Stats Only`
- 選取任一洞穴小怪。
- Inspector 截到：
  - `EnemyHealth`
  - `EnemyContactDamage`
  - `EnemyLootDrop`

數值重點：

- 血量：1200
- 經驗：40
- 金幣：15-22
- 藥草：必定掉落 1 株
- 傷害：高於森林小怪

文案重點：

洞穴小怪是後期敵人，血量和獎勵都比森林小怪高。玩家打死一隻洞穴小怪會獲得 40 點經驗、15 到 22 枚金幣，並必定掉落 1 株藥草。這讓洞穴既危險，也能成為 Boss 前最後補給來源。

小怪程式本身沿用 Day25 已經介紹過的敵人架構，Day27 只需要強調參數差異：血量提高、經驗提高、金幣提高、傷害提高，並加入必掉藥草。這樣文章不需要重複講追擊、血條、掉落腳本的完整實作。

### 27-9 進入 Boss 房後藤蔓封路

截圖位置：

- 場景：`CaveScene`
- 選取 `BossRoomReturnBlockVines`
- Scene 視窗要截到：藤蔓位於 Boss 房入口回頭路，剛好堵住玩家退回洞穴小怪區的通道。
- Inspector 要截到：藤蔓物件的 SpriteRenderer 與 BoxCollider2D。
- 再選取 `BossRoomEntrySealTrigger`
- Scene 視窗要截到：Trigger 位置在玩家進入 Boss 房後會踩到的通道上。
- Inspector 要截到：Trigger 的 BoxCollider2D 勾選 `Is Trigger`。

文案重點：

玩家進入 Boss 房後，回頭路會被藤蔓封住。這不是單純裝飾，而是戰鬥場地封鎖。玩家一旦走進 Boss 房，就不能再退回小怪區慢慢拖時間，也不能用離開 Boss 房的方式重置戰鬥。

這裡的擺放重點是：藤蔓要放在唯一回頭路上，碰撞箱要確實覆蓋通道；觸發器則要放在玩家進入 Boss 房後一定會踩到的位置，但不能太早觸發，否則玩家還沒看清 Boss 房就被封路。

### 27-10 放置三個核心護甲

截圖位置：

- 場景：`CaveScene`
- 選取 `BossRoomSealAndCoreArmor`
- 要截到三個：
  - `CoreStoneArmor_01`
  - `CoreStoneArmor_02`
  - `CoreStoneArmor_03`
- Scene 視窗要截到：三個護甲分布在 Boss 房上半部，彼此有明顯間距。
- Inspector 建議選取其中一個 `CoreStoneArmor`，截到 SpriteRenderer、BoxCollider2D、BossCoreNode。

文案重點：

Boss 本體已經畫在背景上，所以不用再建立一個會移動的 Boss Sprite。戰鬥規則改成攻擊 Boss 房中的三個核心護甲。這三個護甲的位置要分開，不能疊在一起，玩家才需要在 Boss 房裡移動並判斷當前目標。

護甲的定位是「Boss 弱點的外殼」。護甲存在時，核心碰撞箱不啟用；護甲被打碎後，核心才短暫暴露。這樣玩家不是站在原地一直打 Boss，而是要跟著高亮提示去攻擊正確護甲。

### 27-11 建立 Boss 對話觸發器與 dialogmark

截圖位置：

- 場景：`CaveScene`
- 選取 `dialogmark`
- 要截到它位於 Boss 房下半部、面向 Boss 的位置。
- 選取 `BossIntroDialogueTrigger`
- 要截到 Trigger 範圍。

文案重點：

Boss 開場不是玩家靠近後直接彈對話。玩家踩到觸發器後，主角會自動跑到 `dialogmark`，站在 Boss 房下半部，面向上方 Boss，然後才開始對話。這樣畫面構圖會更像正式 Boss 戰開場。

這裡要特別截出 `dialogmark` 的位置。它不是隨便放的 Marker，而是用來決定主角進入 Boss 戰前的站位。主角站在畫面下方，Boss 位於畫面上方，玩家才能同時看到自己的角色、Boss 位置和後續出招方向。

這個流程的實作邏輯是：玩家先踩到 `BossIntroDialogueTrigger`，系統立刻鎖住玩家手動操作，暫停敵人，然後讀取 `dialogmark` 的 Transform 位置。接著主角不是瞬間傳送過去，而是用腳本控制移動方向，自動跑到 `dialogmark`。到達指定位置後，角色停止移動並轉向上方，Boss 開場對話才正式開始。

### 27-12 測試主角自動跑到 dialogmark

截圖位置：

- Play Mode
- 主角踩到 `BossIntroDialogueTrigger`
- 要截到主角正在往 `dialogmark` 移動，或已經站到 `dialogmark` 面向 Boss。

文案重點：

這一步讓 Boss 對話有演出感。玩家不是隨便站在觸發器邊緣對話，而是被移動到固定位置，鏡頭也會往上偏移，讓 Boss 房上半部能出現在畫面中。

截圖時可以把重點放在「主角正在自動移動」或「主角已經站在 `dialogmark` 並面向 Boss」這兩種畫面。文章可以說明：這裡不是一般 NPC 對話，而是先把主角帶到 Boss 戰的演出位置，避免玩家站在角落觸發對話，導致畫面看不到 Boss。

### 27-13 測試 Boss 開場對話

截圖位置：

- Play Mode
- 主角站在 `dialogmark`
- 畫面出現 Boss 對話框。
- 建議截圖內容：
  - Boss 說 `那枚符文……竟又回到人類手中。`
  - 主角說 `你認得雷痕符文。`
  - 或 Boss 提到先祖、雷痕符文、精英怪拔走壓制符文。

文案重點：

Boss 對話承接 Day25 與 Day26 的雷痕符文主線。Boss 認得雷痕符文，也知道它曾經被用來壓制自己。這讓最終 Boss 不是突然出現的敵人，而是前面森林與武器匠線索的收束。

### 27-14 Boss 對話結束後顯示血條

截圖位置：

- Play Mode
- 點完 Boss 對話。
- 要截到右上角棕色血條：
  - `洞穴古樹`
  - `15000 / 15000`

文案重點：

Boss 血條不會在進洞穴時提前出現，而是在 Boss 對話結束後才顯示。這代表 Boss 戰正式開始。血條放在右上角，避免和左上角的玩家 HP、任務與操作提示擠在一起。

### 27-15 測試隨機亮起護甲

截圖位置：

- Play Mode
- Boss 對話結束後。
- 要截到三個護甲中有一個呈現高亮或閃爍。

文案重點：

Boss 不會讓三個核心同時暴露。每次只會隨機亮起一個護甲，提示玩家目前該攻擊哪個位置。打錯護甲會出現沒有反應的提示。

### 27-16 測試打碎護甲

截圖位置：

- Play Mode
- 攻擊亮起的護甲。
- 要截到訊息：
  - `護甲出現裂痕，還需要 x 下。`
  - 或第 5 下後出現 `護甲碎裂，核心暴露。`

文案重點：

亮起的護甲需要被攻擊 5 下才會碎裂。護甲碎裂前，核心碰撞箱不啟用，所以玩家不能直接跳過護甲打核心。

### 27-17 測試核心暴露與 Boss 扣血

截圖位置：

- Play Mode
- 護甲碎裂後攻擊核心。
- 要截到：
  - 核心可被攻擊。
  - 右上角 Boss 血量下降。
  - 訊息顯示核心受創與造成傷害。

文案重點：

核心被攻擊時才會真正扣 Boss 血。傷害會讀取玩家目前拳套與符文總傷害，並套用雷痕符文對 Boss 增傷 300%。因此雷痕符文不是普通高傷害道具，而是 Boss 戰的關鍵。

### 27-18 測試核心被打 2 次後護甲重生

截圖位置：

- Play Mode
- 同一個核心被攻擊 2 次後。
- 要截到：
  - 護甲重新顯示。
  - 另一個護甲重新高亮。
  - 訊息顯示 `護甲重新生長。`

文案重點：

核心不是打開後一直暴露。核心被攻擊 2 次後，護甲會重新生長，系統再隨機亮起下一個護甲。玩家必須反覆打碎護甲、攻擊核心，直到 Boss 血量歸零。

## 程式碼截圖節點

### 27-19 `Day27CaveMonsterPrefabBuilder.cs`：Day27 小怪工具重點

截圖檔案：

`Assets/Game/Editor/Day27CaveMonsterPrefabBuilder.cs`

截圖行數：

- `9-18`：洞穴場景路徑、小怪資料夾、Prefab 資料夾、小怪血量與掉落數值。
- `73-78`：三種洞穴小怪資料。
- `80-112`：30 隻洞穴小怪的預設放置位置。

重點功能：

- 這裡集中管理 Day27 洞穴小怪工具會用到的場景、素材資料夾與重要數值。
- 小怪數值只需要在文章中簡單說明：血量 1200、經驗 40、金幣 15-22、必掉 1 株藥草。
- 30 個位置用來快速鋪出洞穴小怪群；後續如果你手動調整位置，就改用更新數值工具，不要重新放怪。

文案方向：

Day27 的 Unity 工具只保留洞穴小怪流程。工具負責建立三種小怪 Prefab、放置 30 隻小怪，以及更新小怪數值。Boss 房、藤蔓、對話與核心護甲不再寫成 Unity 選單工具，而是直接使用場景物件與戰鬥腳本說明。

### 27-20 `Day27CaveMonsterPrefabBuilder.cs`：洞穴小怪參數調整

截圖檔案：

`Assets/Game/Editor/Day27CaveMonsterPrefabBuilder.cs`

截圖行數：

- `114-199`：三個保留在 Unity 選單中的 Day27 小怪工具入口。
- `265-340`：洞穴小怪血量、經驗、追擊、傷害、金幣與藥草掉落設定。

重點功能：

- `CreateCaveSmallMonsterPrefabsOnly()` 只做 Prefab，不放怪，方便先整理素材。
- `PlaceThirtyCaveSmallMonsters()` 把 30 隻小怪放到 `CaveSmallMonsters` 群組。
- `Update Cave Small Monster Stats Only` 只更新數值，不移動你已經手調好的怪物位置。
- 洞穴小怪相對森林小怪的差異是：血量更高、經驗更多、金幣更多、傷害更高，而且必掉藥草。

文案方向：

小怪程式碼不用重複細講。文章可以一句帶過：洞穴小怪沿用 Day25 已完成的小怪戰鬥系統，本日只把數值調整成後期版本，讓洞穴成為 Boss 前的高壓戰鬥區。

### 27-21 `BossRoomVineSealTrigger.cs`：進 Boss 房後封住回頭路

截圖檔案：

`Assets/Game/Combat/Scripts/BossRoomVineSealTrigger.cs`

截圖行數：

- `5-17`：指定藤蔓物件，並在一開始隱藏。
- `20-32`：玩家進入 Trigger 後啟用藤蔓並關閉觸發器。

重點功能：

- 開場時藤蔓隱藏。
- 玩家踩到 Boss 房入口 Trigger 後，藤蔓顯示並用碰撞箱封住回頭路。
- 觸發一次後腳本關閉，避免重複啟用。

文案方向：

Boss 房入口不是單純傳送點，而是戰鬥場地封鎖。玩家走進 Boss 房後，藤蔓長出來堵住退路，製造「已經進入最終戰」的壓力。

### 27-22 `BossIntroDialogueTrigger.cs`：Boss 開場對話與站位

截圖檔案：

`Assets/Game/Combat/Scripts/BossIntroDialogueTrigger.cs`

截圖行數：

- `9-21`：Boss 對話完成事件、移動速度、鏡頭偏移與任務目標。
- `31-70`：Boss 與主角對話內容。
- `149-202`：踩到 Trigger 後暫停敵人、鎖住輸入、自動跑到 `dialogmark`。
- `227-249`：對話完成後恢復玩家控制，並發出 Boss 戰啟動事件。

重點功能：

- 玩家踩到 Boss 對話 Trigger 後，系統會暫停敵人、鎖住玩家操作，並讓主角自動跑到 `dialogmark`。
- 自動移動不是瞬間傳送，而是每幀計算主角目前位置和 `dialogmark` 的距離，再用 `MoveTowards` 往目標點靠近。
- 移動過程會呼叫 `SetScriptedMoveInput(direction, true)`，所以角色會按照移動方向播放跑步狀態，看起來像主角自己跑到定位點。
- 主角到達後會面向上方 Boss，開始播放對話。
- 對話完成後發出 `BossIntroCompleted`，Boss 血條與核心戰鬥會等這個事件才啟動。

文案方向：

這段的重點是「演出」。主角不是站在任意位置對話，也不是被硬傳送到 Boss 面前，而是由腳本接管移動，自動跑到 Boss 房下半部固定位置，面向上方 Boss。這讓 Boss 開場更像正式戰鬥，而不是普通 NPC 對話。

### 27-23 `BossCoreBattleController.cs`：Boss 血條與戰鬥啟動

截圖檔案：

`Assets/Game/Combat/Scripts/BossCoreBattleController.cs`

截圖行數：

- `6-12`：Boss 血量、護甲打碎次數、核心重生次數與血條顏色。
- `32-58`：等待 Boss 對話完成後啟動戰鬥。
- `189-239`：右上角棕色 Boss 血條 UI。

重點功能：

- Boss 血量設定為 15000。
- `OnEnable()` 監聽 `BossIntroCompleted`，所以 Boss 對話沒結束前不會顯示血條。
- `ActivateBattle()` 啟動戰鬥後，右上角才出現棕色 Boss 血條。
- 血條顯示 Boss 名稱和即時血量，例如 `洞穴古樹 15000 / 15000`。

文案方向：

Boss 血條不應該一進洞穴就出現，而是要等開場對話結束才顯示。這個事件讓「劇情演出」和「戰鬥開始」有清楚分界。

### 27-24 `BossCoreBattleController.cs`：三個護甲中隨機高亮一個

截圖檔案：

`Assets/Game/Combat/Scripts/BossCoreBattleController.cs`

截圖行數：

- `122-139`：`HighlightRandomArmor()` 隨機選擇亮起的護甲。
- `141-163`：`GetAvailableArmorNodes()` 取得目前還有護甲存在的節點。

重點功能：

- `GetAvailableArmorNodes()` 會先把三個核心節點中「護甲還沒碎」的節點整理成候選清單。
- `Random.Range(0, candidates.Length)` 從候選清單中抽出一個。
- 抽到的節點被記錄成 `highlightedNode`。
- 迴圈會呼叫每個節點的 `SetHighlighted()`，只有被抽中的護甲會亮起，其他護甲恢復普通狀態。

文案方向：

這是 Boss 核心戰的主要規則：三個護甲不會同時可打，而是每次隨機亮起一個。玩家要觀察哪個護甲亮起，再移動過去攻擊。這讓 Boss 戰多了一點判斷與走位，而不是站在原地一直按攻擊。

### 27-25 `BossCoreNode.cs`：護甲高亮如何實現

截圖檔案：

`Assets/Game/Combat/Scripts/BossCoreNode.cs`

截圖行數：

- `6-11`：護甲 SpriteRenderer、Collider、核心物件、核心 Collider、高亮顏色。
- `48-57`：高亮時用 `Mathf.Sin(Time.time * 8f)` 做顏色脈衝。
- `81-90`：`SetHighlighted()` 切換護甲是否高亮。

重點功能：

- 每個護甲都有自己的 `BossCoreNode`。
- 高亮不是換 Sprite，而是改變 `SpriteRenderer.color`。
- `SetHighlighted(true)` 會先把護甲改成金黃色。
- `Update()` 裡用 Sin 波讓顏色在原色和高亮色之間脈衝，看起來像閃爍。
- 這樣不需要另外做高亮動畫素材，也能讓玩家知道目前要攻擊哪個護甲。

文案方向：

護甲高亮的實作很簡單：不是新增特效，而是讓護甲的 SpriteRenderer 顏色變亮，並用時間函式做脈衝。文章可以寫成「亮起的護甲會持續閃爍，提醒玩家目前要攻擊的核心位置」。

### 27-26 `BossCoreNode.cs`：護甲碎裂與核心顯示

截圖檔案：

`Assets/Game/Combat/Scripts/BossCoreNode.cs`

截圖行數：

- `106-155`：記錄護甲被打幾下，滿 5 下後隱藏護甲、啟用核心。
- `157-190`：核心被打 2 次後護甲重生，核心碰撞箱關閉。

重點功能：

- 只有被高亮的護甲會計算命中次數。
- 護甲被打 5 下後，護甲圖片和護甲 Collider 關閉。
- 護甲關閉後，核心物件和核心 Collider 啟用。
- 核心被攻擊 2 次後，護甲重新顯示，核心再次隱藏。

文案方向：

核心不是一開始就能打。玩家必須先照著高亮提示打碎護甲，核心才會短暫暴露。這讓 Boss 戰形成「找亮點、破護甲、打核心、護甲重生」的循環。

### 27-27 `BossCoreBattleController.cs`：核心扣 Boss 血與雷痕符文增傷

截圖檔案：

`Assets/Game/Combat/Scripts/BossCoreBattleController.cs`

截圖行數：

- `85-120`：核心被攻擊後扣 Boss 血，並計算雷痕符文增傷。

重點功能：

- 核心被攻擊時才會扣 Boss 血。
- 傷害先讀取玩家目前拳套和符文總傷害。
- 如果玩家裝備雷痕符文，`GetBossDamageBonusPercent()` 會回傳 300。
- 實際 Boss 傷害是 `baseDamage * 4`，也就是原傷害加上對 Boss 增傷 300%。
- Boss 血量歸零後，任務目標更新為 Boss 已被擊敗。

文案方向：

這裡把 Day26 的雷痕符文詞條真正接到 Boss 戰。雷痕符文不是只在背包裡顯示漂亮文字，而是在 Boss 核心受傷時提供 300% 增傷。

### 27-28 `PlayerAttack.cs`：玩家攻擊如何接到 Boss 部位

截圖檔案：

`Assets/Game/Combat/Scripts/PlayerAttack.cs`

截圖行數：

- `104-113`：播放攻擊視覺效果，讀取目前拳套與符文傷害，並依照攻擊方向建立圓形攻擊判定。
- `115-119`：準備記錄本次攻擊是否打中一般敵人、Boss 部位，以及最後要顯示的戰鬥訊息。
- `122-148`：如果命中的物件不是一般敵人，就先檢查「命中的 Collider 本身」是不是 Boss 護甲或核心。
- `151-168`：如果 Collider 本身不是 Boss 部位，再往父物件尋找 Boss 部位腳本。
- `173-182`：一般小怪與精英怪仍然走原本的 `EnemyHealth` 扣血流程。
- `185-191`：最後依照命中結果顯示 Boss 訊息、一般傷害訊息或攻擊揮空。

重點功能：

- Boss 戰沒有新增攻擊鍵，玩家仍然使用 `J` 進行拳擊攻擊。
- 攻擊判定不是只看角色腳下，而是依照目前攻擊方向，把判定圓往前推一段距離。這樣玩家面向護甲或核心攻擊時，才會打到前方目標。
- 程式先用 `GetComponentInParent<EnemyHealth>()` 判斷命中的東西是不是一般敵人。如果是小怪或精英怪，就照原本敵人血量系統扣血。
- 精英怪仍然保留自己的減傷邏輯，所以這段攻擊程式不會破壞前面森林精英怪的規則。
- 如果命中的 Collider 沒有 `EnemyHealth`，才會進入 Boss 部位判斷。Boss 護甲和核心不是一般小怪，所以它們不會掉金幣、經驗或藥草。
- Boss 部位統一透過 `IBossDamageReceiver.ReceivePlayerAttack()` 接收玩家攻擊。護甲、核心可以各自決定被打時要顯示什麼訊息、要不要扣 Boss 血。
- 程式會先檢查「命中的 Collider 本身」，如果本身已經是核心或護甲，就直接讓它處理攻擊，並跳過父物件檢查。
- 如果 Collider 本身沒有 Boss 部位腳本，才往父物件找。這是為了支援一些 Collider 掛在子物件、腳本掛在父物件的情況。

文案方向：

這一段可以寫成：玩家的操作沒有變，仍然是按 `J` 攻擊；真正改變的是攻擊命中後的判斷流程。一般怪物走原本的血量系統，Boss 房的護甲與核心則走 Boss 部位系統。

這裡最重要的細節是核心判定。核心暴露後，玩家實際打到的是核心自己的 Collider。如果程式先往父物件找，就可能先找到外層護甲節點，導致核心已經露出來了，畫面卻顯示「這塊護甲沒有反應」。所以這裡先處理直接命中的 Collider，再處理父物件，確保打到核心時會正確顯示核心受傷與 Boss 扣血訊息。

文章中可以補一句：同一套拳擊系統被延伸到 Boss 戰，但 Boss 部位不是一般敵人，它們只負責控制護甲、核心與 Boss 血量，不會觸發小怪的死亡掉落流程。

## 正常 Unity 流程對應工具

### `IronRPG > Day 27 > Create Cave Small Monster Prefabs Only`

正常流程：

1. 匯入 3 張洞穴小怪 PNG。
2. 設定 Texture Type 為 Sprite。
3. 建立 3 個 Prefab。
4. 每個 Prefab 加上 `SpriteRenderer`、`Rigidbody2D`、`BoxCollider2D`。
5. 掛上 `EnemyHealth`、`SimpleEnemyChase`、`EnemyVisualAnimator`、`EnemyContactDamage`、`EnemyLootDrop`。
6. 只建立 Prefab，不放進場景。

### `IronRPG > Day 27 > Place 30 Cave Small Monsters`

正常流程：

1. 打開 `CaveScene`。
2. 建立 `CaveSmallMonsters` 群組。
3. 從 3 種 Prefab 輪流生成 30 隻小怪。
4. 依照洞穴比例調整小怪縮放。
5. 不修改背景圖與洞穴碰撞箱。

### `IronRPG > Day 27 > Update Cave Small Monster Stats Only`

正常流程：

1. 找到既有 `CaveSmallMonsters`。
2. 不移動小怪位置。
3. 不修改小怪比例。
4. 只更新血量、經驗、金幣、藥草、追擊與傷害數值。

這個工具適合在你手動調整完小怪位置後使用，避免重新放怪把位置覆蓋。

Day27 的 Unity 選單工具只保留以上三個。Boss 房藤蔓、Boss 對話與核心護甲不再列為工具流程，文章中改用場景截圖與腳本截圖說明它們的正常製作方式。

## Day27 結束時應該達成的狀態

- `ToCaveScene` 與 `CaveScene` 已建立。
- 洞穴背景圖已放入場景。
- 洞穴牆壁與道路邊界已經有碰撞箱。
- 洞穴中主角比例較小，移動速度較慢，讓洞穴比例更大。
- 3 種洞穴小怪已生成並做成 Prefab。
- 洞穴中放置 30 隻小怪。
- 洞穴小怪血量 1200。
- 洞穴小怪死亡給 40 經驗。
- 洞穴小怪掉落 15 到 22 金幣。
- 洞穴小怪必定掉落 1 株藥草。
- 玩家進入 Boss 房後，藤蔓封住回頭路。
- Boss 本體不建立獨立物件，Boss 直接存在於背景圖中。
- Boss 房有三個核心護甲。
- 玩家踩到 Boss 對話觸發器後，會自動跑到 `dialogmark` 並面向 Boss。
- Boss 對話完成後，右上角顯示棕色 Boss 血條。
- Boss 血量是 15000。
- 雷痕符文對 Boss 套用增傷 300%。
- 三個護甲會隨機亮起一個。
- 亮起護甲被打 5 下後隱藏，核心碰撞箱啟用。
- 核心被攻擊 2 次後，護甲重新長出並重新抽選下一個亮起護甲。

Day27 到這裡先結束。下一步應該開始做 Boss 主動攻擊：藤蔓直線刺擊、範圍拍擊或左右掃擊。核心護甲與血條已經準備好，後續只要把 Boss 攻擊節奏接進這個 Boss 房流程即可。
