# Day 28 - Boss 攻擊、洞穴進出口與戰後離場流程

Day28 接續 Day27。Day27 已經完成洞穴場景、Boss 房、Boss 開場對話、Boss 血條，以及三個核心護甲的破甲與核心受傷機制。

Day28 的重點是讓 Boss 真正開始攻擊玩家，並補上洞穴進出流程：

1. 使用 Codex 生成 3 張 Boss 攻擊素材。
2. 將素材匯入 Unity，整理成透明 PNG Sprite。
3. 在 `BossCoreBattleController` 所在物件上掛上 `BossVineAttackController`。
4. 做出 Boss 的三種攻擊：藤蔓直刺、連續藤蔓重砸、左右半場橫掃。
5. 在 `ToCaveScene` 裡放置擋住退路的藤蔓，讓 Boss 戰前後的路線狀態不同。
6. 設定 `CaveScene` 的 `Exit`，Boss 被擊敗後才能離開洞穴。
7. 設定 `ToCaveScene` 的 `Exitformcave`，作為打完 Boss 從洞穴回來的位置。
8. 設定 `ToCaveScene` 的 `tocave`，作為進入 `CaveScene` 的洞穴入口。
9. Boss 死亡後播放戰後對話，並把任務推進到「回村莊向村長回報」。

文章口徑可以明確說明素材是用 Codex 生成的，但 Unity 內的實作流程要用人的操作視角描述：匯入圖片、新增物件、加入 Collider、掛腳本、指定 Sprite、設定欄位，讓流程看起來像正常的 Unity 製作紀錄。

## Day28 文章口徑

Day27 只做到 Boss 核心機制，玩家可以打護甲、打核心，但 Boss 還不會主動攻擊。這樣比較像靶場，不像最終戰。

Day28 的目標是讓 Boss 戰變得完整：

- Boss 會持續用藤蔓直刺追擊玩家。
- Boss 會不定期使用範圍技能，迫使玩家移動。
- Boss 房入口和出口開始有完整流程，不再只是單張洞穴地圖。
- 玩家擊敗 Boss 後，能從洞穴出口回到 `ToCaveScene`。
- 回到 `ToCaveScene` 後，原本擋住退路的藤蔓會消失，表示 Boss 戰已經結束，可以往遺跡方向回村。

這一天可以寫成「把 Boss 從機制靶子變成真正會攻擊的敵人」。

## 素材口徑

Boss 本體仍然不建立成獨立角色。Boss 已經畫在 Cave 背景的上半部，所以 Day28 只補 Boss 的攻擊效果。

這次需要 3 張透明 PNG 素材。因為專案裡沒有現成的 Boss 藤蔓攻擊圖，所以這三張圖改用 Codex 生成，再匯入 Unity 當成攻擊特效使用：

- `boss_vine_stab.png`：藤蔓直刺。
- `boss_vine_slam.png`：藤蔓重砸。
- `boss_vine_sweep.png`：藤蔓橫掃。

素材位置：

`Assets/Game/Generated/Day28/BossAttacks`

文章可以寫成：因為 Boss 是一棵被洞穴根系包覆的古樹，所以攻擊不做成普通怪物揮手，而是使用藤蔓與樹根延伸到場地中。這樣可以維持 Boss 固定在背景上的設計，同時讓玩家感覺整個 Boss 房都在攻擊自己。

## Day28 截圖總表

### 28-1 使用 Codex 生成並匯入 Boss 攻擊素材

截圖位置：

- Project 視窗打開 `Assets/Game/Generated/Day28/BossAttacks`
- 要截到：
  - `boss_vine_stab.png`
  - `boss_vine_slam.png`
  - `boss_vine_sweep.png`
- 如果 Project 視窗空間足夠，建議使用縮圖模式，讓三張素材的形狀可以直接看出差異。

文案重點：

Day28 先補 Boss 攻擊素材。Boss 是洞穴背景中的巨大古樹，不適合做成會走動的小怪，因此攻擊素材以藤蔓和樹根為主。

這三張素材是使用 Codex 生成的透明 PNG。生成時的方向不是單純做一張漂亮插圖，而是以 Unity 實作為目標：背景必須透明、風格要接近目前洞穴場景的暗色像素風、藤蔓輪廓要清楚，並且要能分別對應三種攻擊。

三張素材的用途分別是：

- `boss_vine_stab.png`：直刺藤蔓，負責從 Boss 方向往玩家所在位置延伸。
- `boss_vine_slam.png`：重砸藤蔓，負責顯示大範圍落下攻擊。
- `boss_vine_sweep.png`：橫掃藤蔓，負責左右半場掃擊。

匯入 Unity 後，三張圖都設定為 Sprite，並保留透明通道。這樣在 Boss 戰中生成攻擊物件時，畫面只會出現藤蔓本體，不會出現多餘的方形背景。

### 28-2 設定素材 Sprite 匯入參數

截圖位置：

- 選取其中一張 Boss 攻擊 PNG。
- Inspector 截到 Texture Import Settings。
- 建議截到：
  - Texture Type：Sprite
  - Sprite Mode：Single
  - Filter Mode：Point
  - Compression：None 或 Uncompressed
  - Alpha Is Transparency

文案重點：

Boss 攻擊素材需要透明背景，否則藤蔓出現時會帶一整塊方形底色。這裡把圖片設定為 Sprite，保留透明通道，並使用 Point Filter，避免像素風素材在遊戲中被模糊化。

文章可以補充：Boss 攻擊不是場景裝飾，而是戰鬥中會被程式動態生成的攻擊物件，所以匯入設定會直接影響實際畫面。如果 Filter Mode 使用預設的平滑顯示，藤蔓邊緣會變糊，和目前遊戲的像素風角色、洞穴背景不一致。

### 28-3 在 Boss 控制物件上掛攻擊腳本

截圖位置：

- 場景：`CaveScene`
- Hierarchy 選取 `BossCoreBattleController`
- Inspector 要截到：
  - `BossCoreBattleController`
  - `BossVineAttackController`
  - `stabSprite`
  - `slamSprite`
  - `sweepSprite`
  - `bossOrigin`
  - `sweepCenter`

文案重點：

Boss 本身不是一個會走動的 Sprite，而是存在於背景圖中。因此 Boss 攻擊腳本掛在 `BossCoreBattleController` 所在物件上。這個物件本來就負責 Boss 血量與核心機制，Day28 讓它再負責啟動藤蔓攻擊。

`BossVineAttackController` 需要指定三張攻擊 Sprite，並設定 Boss 攻擊的起點 `bossOrigin`。這個起點大致對應 Boss 主體所在的位置，藤蔓攻擊會從這裡向場地延伸。

場景內的設定可以這樣寫：Boss 房的背景圖已經包含巨大古樹，所以我沒有再額外建立一個 Boss Sprite。實際上，Boss 的戰鬥邏輯集中在 `BossCoreBattleController` 這個物件上，它像是整個 Boss 房的控制中心。Day28 新增的 `BossVineAttackController` 也掛在同一個物件上，讓 Boss 血量、核心護甲、攻擊節奏都由同一個控制點管理。

Inspector 裡要確認三個 Sprite 欄位都有指定素材。如果其中一個欄位是空的，對應技能就算程式有執行，畫面也不會出現藤蔓。`bossOrigin` 則用來決定藤蔓從哪裡出現，位置要接近背景中 Boss 的身體或根部，否則藤蔓會像是從場景空白處冒出來。

### 28-4 測試藤蔓直刺攻擊

截圖位置：

- Play Mode
- Boss 對話結束後。
- 截到紅色預警線或藤蔓從 Boss 方向伸向玩家。
- 建議截到主角正在躲直刺的畫面。

文案重點：

藤蔓直刺是 Boss 的普通攻擊。它會鎖定玩家當下的位置，先顯示紅色預警，再從 Boss 方向伸出藤蔓。

直刺攻擊不是瞬間生成完整碰撞箱，而是隨著藤蔓延長逐步拉長碰撞箱。玩家如果碰到藤蔓，就會受到 70 點傷害，而且直刺停留期間每 0.5 秒可以再次扣血。

### 28-5 測試連續藤蔓重砸

截圖位置：

- Play Mode
- Boss 使用重砸技能時。
- 要截到大範圍紅色預警區。
- 最好截到藤蔓落下或場地被大範圍覆蓋的畫面。

文案重點：

重砸技能是 Boss 的範圍壓迫攻擊。它會連續砸擊數次，每次以玩家附近的位置為中心生成預警區，再用大型藤蔓向地面砸下。

這個技能的 X 軸範圍比較大，Y 軸覆蓋整個 Boss 房主要活動區，目的是讓玩家不能只站在固定安全點輸出核心。

重砸命中會造成 150 點傷害。

### 28-6 測試左右半場橫掃

截圖位置：

- Play Mode
- Boss 使用橫掃技能時。
- 建議截兩張：
  - 左半場橫掃。
  - 右半場橫掃。

文案重點：

橫掃藤蔓不是一整條水平放在畫面上直接打全場，而是像雨刷一樣，以 Boss 方向為原點做左右掃擊。

這個技能分成左半場和右半場。左掃只威脅左半邊，右掃只威脅右半邊，玩家需要判斷目前預警在哪一側，再往另一側或安全區移動。

橫掃命中會造成 300 點傷害，是 Day28 三種 Boss 攻擊中最痛的技能。

### 28-7 確認 Boss 普通攻擊不會因技能停止

截圖位置：

- Play Mode
- Boss 使用重砸或橫掃時，畫面中仍能看到直刺預警或直刺藤蔓。

文案重點：

直刺攻擊是 Boss 的普通攻擊，會持續進行。重砸和橫掃是額外技能，不會讓普通攻擊完全停止。

這樣 Boss 戰會更有壓力：玩家不只要處理核心護甲，也要持續躲避普通藤蔓追擊。

### 28-8 設定擋住退路的藤蔓

截圖位置：

- 場景：`ToCaveScene`
- 選取擋住退路的藤蔓物件。
- Scene 視窗截到藤蔓長在 Boss 房出口外的小路上，擋住玩家回頭。
- Inspector 截到：
  - Transform
  - BoxCollider2D

文案重點：

玩家進入 Boss 房後，退路不能只是空著，否則戰鬥張力會變弱。這裡在 `ToCaveScene` 的回頭小路上放一段藤蔓，讓它像是洞穴深處的根鬚突然長出來，把原本的退路封住。

這段藤蔓本身不需要複雜腳本，只需要 Collider 能擋住玩家。它的作用是讓玩家知道：進入 Boss 房後，這場戰鬥必須打完，不能直接原路退回去。

這個藤蔓不是永久牆壁，而是劇情狀態的一部分。Boss 戰前，它會擋住退路；Boss 被擊敗後，玩家從 `CaveScene` 的 `Exit` 回到 `ToCaveScene` 的 `Exitformcave`，這段藤蔓就會被移除，表示洞穴封鎖解除。

截圖時建議同時拍到小路、藤蔓本體和 Collider 範圍。文章可以寫成：我用一段藤蔓取代普通牆壁，讓「不能回頭」這件事看起來像是 Boss 的根鬚封住道路，而不是單純放了一個看不見的擋路箱。

### 28-9 設定 `CaveScene` 的 `Exit`

截圖位置：

- 場景：`CaveScene`
- 選取 `Exit`
- Scene 視窗要截到它放在 Boss 戰後離開的位置。
- Inspector 截到：
  - BoxCollider2D 勾選 Is Trigger
  - SceneTransitionPoint
  - Target Scene：`ToCaveScene`
  - Target Spawn Point：`Exitformcave`
  - Require Boss Defeated Before Use
  - Scene Object To Disable After Arrival：`special box`

文案重點：

`Exit` 是 Boss 戰後離開洞穴的點。這個出口不能讓玩家一開始就用，否則玩家可以不打 Boss 直接離開。

因此 `Exit` 上的轉場點會檢查 Boss 是否已經被擊敗。Boss 血量還沒歸零時，玩家踩到 `Exit` 不會轉場。Boss 被擊敗後，玩家才可以從 `Exit` 回到 `ToCaveScene`。

回到 `ToCaveScene` 後，玩家會出現在 `Exitformcave`，並且自動關閉擋住退路的藤蔓物件，讓回村方向的路打開。

場景設定的重點有兩個：第一，`Exit` 的 BoxCollider2D 必須勾選 Is Trigger，否則玩家只是被擋住，不會觸發轉場；第二，`Require Boss Defeated Before Use` 必須開啟，這樣出口才會變成 Boss 戰後的離場點，而不是普通傳送門。

`Scene Object To Disable After Arrival` 填入藤蔓物件在 Unity 裡的名稱。現在場景內這個物件名稱是 `special box`，所以欄位值仍然填 `special box`。文章正文可以把它稱為「擋住退路的藤蔓」，不要把內部物件名稱當成玩家看到的內容。

這一步讓場景看起來真的有被 Boss 戰結果改變，而不是單純切換地圖。

### 28-10 設定 `Exitformcave`

截圖位置：

- 場景：`ToCaveScene`
- 選取 `Exitformcave`
- Scene 視窗截到它的位置。

文案重點：

`Exitformcave` 是玩家打完 Boss 從洞穴回來時的出生點。它不是入口，而是回程位置。

命名上保留 `Exitformcave`，因為它的意思是「從 Cave 出來後的落點」。`CaveScene` 的 `Exit` 會把玩家送到這個位置。

截圖時建議拍 `Exitformcave` 在 `ToCaveScene` 裡的位置，最好能看見附近路線。這樣文章可以說清楚：Boss 戰後不是把玩家送回村莊，而是先回到洞穴外的過渡場景，再沿著路線往遺跡或村莊方向離開。

### 28-11 設定 `tocave` 洞穴入口

截圖位置：

- 場景：`ToCaveScene`
- 選取 `tocave`
- Scene 視窗截到它位於洞穴入口處。
- Inspector 截到：
  - Transform
  - BoxCollider2D 勾選 Is Trigger
  - SceneTransitionPoint，目標場景為 `CaveScene`

文案重點：

`tocave` 是從 `ToCaveScene` 進入 `CaveScene` 的入口。玩家走到洞口的觸發範圍後，就會載入洞穴內部場景。

這裡的流程和 Boss 戰後離開洞穴不同：`tocave` 是進洞入口，不需要檢查 Boss 是否死亡，也不需要關閉 `special box`。它只負責把玩家從洞穴外部場景送進洞穴內部。

場景內要確認 `tocave` 放在玩家看得懂的位置，也就是洞穴入口或道路盡頭。它的 Collider 只需要作為觸發區，不需要阻擋玩家。玩家踩上去後會切到 `CaveScene`，正式進入洞穴內部。

### 28-12 測試從 `ToCaveScene` 進入 `CaveScene`

截圖位置：

- Play Mode
- 玩家站在 `tocave` 附近。
- 截圖可分成兩張：
  - 進洞前站在 `ToCaveScene` 的洞穴入口。
  - 轉場後出現在 `CaveScene`。

文案重點：

這一步確認入口流程。玩家不是直接從村莊跳到 Boss 房，而是先到 `ToCaveScene`，再從 `tocave` 進入真正的 `CaveScene`。這讓最後區域的場景銜接更完整。

### 28-13 測試 Boss 被擊敗後戰後對話

截圖位置：

- Play Mode
- Boss 血量歸零後。
- 畫面出現戰後對話框。
- 建議截到：
  - Boss 說 `雷……還在……`
  - 主角說 `雷痕符文撕開了你的魔甲。結束了。`
  - 或主角說 `我要回村，把這裡發生的事告訴村長。`

文案重點：

Boss 被擊敗後不應該直接安靜結束。這裡補上一段簡短戰後對話，讓雷痕符文主線收束：Boss 確認雷的力量仍然存在，主角則決定回村向村長回報。

對話期間會暫停敵人與玩家攻擊，避免戰後畫面被其他小怪血條或攻擊訊息干擾。

### 28-14 測試 Boss 戰後從 `Exit` 離開

截圖位置：

- Play Mode
- Boss 已擊敗。
- 玩家走到 `CaveScene` 的 `Exit`。
- 轉場後出現在 `ToCaveScene` 的 `Exitformcave`。
- 畫面中擋住退路的藤蔓已經不再阻擋路線。

文案重點：

Boss 戰後離場流程要同時完成三件事：

1. 確認 Boss 已被擊敗。
2. 從 `CaveScene` 轉到 `ToCaveScene`。
3. 抵達 `Exitformcave` 後移除擋住退路的藤蔓。

這代表玩家完成 Boss 戰後，路線會被打開，可以往遺跡方向離開，最後回村莊向村長回報。

## 正常 Unity 製作流程

### Boss 攻擊素材

正常流程：

1. 使用 Codex 生成 3 張透明 PNG：直刺、重砸、橫掃。
2. 匯入 `Assets/Game/Generated/Day28/BossAttacks`。
3. 在 Inspector 中把 Texture Type 設為 Sprite。
4. 設定 Filter Mode 為 Point。
5. 確認透明背景正常。

文章可以寫成：這次素材不是從既有素材包中挑選，而是根據洞穴 Boss 的戰鬥需求生成。因為 Boss 是古樹與根系構成的巨大怪物，所以攻擊效果也統一使用藤蔓、樹根和暗色洞穴質感，避免和原本場景風格脫節。

### Boss 攻擊腳本

正常流程：

1. 打開 `CaveScene`。
2. 選取 `BossCoreBattleController`。
3. Add Component：`BossVineAttackController`。
4. 指定三張攻擊 Sprite：
   - `stabSprite`
   - `slamSprite`
   - `sweepSprite`
5. 調整 Boss 攻擊起點 `bossOrigin`。
6. 調整橫掃中心 `sweepCenter`。
7. 設定三種攻擊的傷害與時間：
   - 直刺：70 傷害，0.5 秒一跳。
   - 重砸：150 傷害。
   - 橫掃：300 傷害。

### ToCave 與 Cave 進出口

正常流程：

1. 在 `ToCaveScene` 放置 `tocave`，作為進洞入口。
2. `tocave` 加上 BoxCollider2D，勾選 Is Trigger。
3. `tocave` 掛上 SceneTransitionPoint，目標場景設為 `CaveScene`。
4. 在 `ToCaveScene` 放置 `Exitformcave`，作為從洞穴出來的位置。
5. 在 `ToCaveScene` 放置擋住退路的藤蔓，加上 Collider 擋住打 Boss 前不該走的路。
6. 在 `CaveScene` 放置 `Exit`，作為 Boss 戰後離開洞穴的位置。
7. `Exit` 加上 BoxCollider2D，勾選 Is Trigger。
8. `Exit` 掛上 SceneTransitionPoint：
   - Target Scene：`ToCaveScene`
   - Target Spawn Point：`Exitformcave`
   - Require Boss Defeated Before Use：true
   - Scene Object To Disable After Arrival：藤蔓物件名稱，目前場景中填 `special box`

## 程式碼截圖節點

### 28-15 `BossVineAttackController.cs`：Boss 攻擊參數

截圖檔案：

`Assets/Game/Combat/Scripts/BossVineAttackController.cs`

截圖行數：

- `7-15`：Boss 控制器、三張攻擊 Sprite、攻擊起點、技能冷卻。
- `18-27`：藤蔓直刺傷害、預警時間、延伸時間、寬度與冷卻。
- `30-35`：藤蔓重砸傷害、次數、預警與範圍。
- `38-46`：橫掃傷害、預警、範圍與預警顏色。

重點功能：

這一段集中管理 Boss 攻擊的可調參數。文章中不用逐行解釋，只要說明：Boss 有三種攻擊，每種攻擊的傷害、預警時間、持續時間與範圍都可以在 Inspector 中調整。

### 28-16 `BossVineAttackController.cs`：普通攻擊與技能同時運作

截圖檔案：

`Assets/Game/Combat/Scripts/BossVineAttackController.cs`

截圖行數：

- `87-99`：Boss 對話完成後啟動普通攻擊與技能攻擊兩條 Coroutine。
- `116-132`：`StabLoop()` 持續執行藤蔓直刺。
- `135-158`：`SkillLoop()` 隨機選擇重砸或橫掃。

重點功能：

Boss 攻擊分成兩條流程：

- `StabLoop()`：負責普通藤蔓直刺，持續追擊玩家。
- `SkillLoop()`：負責技能，隨機使用重砸或橫掃。

這樣普通攻擊不會因為 Boss 放技能就完全停止。玩家需要同時注意直刺預警和範圍技能。

### 28-17 `BossVineAttackController.cs`：藤蔓直刺

截圖檔案：

`Assets/Game/Combat/Scripts/BossVineAttackController.cs`

截圖行數：

- `162-193`：`PerformVineStab()`。
- `289-312`：`AnimateLinearVine()`。

重點功能：

藤蔓直刺會先鎖定玩家當下位置，計算 Boss 起點到玩家的方向與距離。接著先建立一條紅色預警，再生成真正的藤蔓攻擊物件。

攻擊開始後，藤蔓不是瞬間變成最長，而是透過 `AnimateLinearVine()` 逐步延伸。延伸時會同步更新 Sprite 大小和 BoxCollider2D，所以碰撞箱會跟著藤蔓長度一起變長。

文案可以寫成：直刺攻擊的碰撞不是固定一塊，而是跟著藤蔓延伸，玩家看到藤蔓伸過來時需要立刻移動。

### 28-18 `BossVineAttackController.cs`：連續藤蔓重砸

截圖檔案：

`Assets/Game/Combat/Scripts/BossVineAttackController.cs`

截圖行數：

- `195-239`：`PerformVineSlam()`。
- `382-390`：重砸範圍高度與中心 Y 軸計算。

重點功能：

重砸技能會連續執行多次。每次先取得玩家位置，再加上一點隨機偏移，讓技能不會永遠只砸在同一個點。

重砸範圍的 Y 軸會依照 Boss 房高度拉長，讓它覆蓋 Boss 房主要活動區。這樣玩家不能只靠上下移動完全躲掉技能，而是需要觀察預警區的 X 軸位置。

### 28-19 `BossVineAttackController.cs`：左右半場橫掃

截圖檔案：

`Assets/Game/Combat/Scripts/BossVineAttackController.cs`

截圖行數：

- `242-247`：橫掃分成左右兩次。
- `249-286`：`PerformOneSweep()` 計算左半場或右半場的掃擊。

重點功能：

橫掃不是打整個 X 軸，而是分成左半場和右半場。

`PerformOneSweep(true)` 會處理其中一側，`PerformOneSweep(false)` 處理另一側。每次掃擊都會先顯示對應半場的預警區，然後用旋轉角度讓藤蔓像雨刷一樣掃過去。

文章可以寫成：橫掃技能不是全畫面傷害，而是半場壓迫，玩家需要根據預警判斷要往哪一側躲。

### 28-20 `BossAttackDamageArea.cs`：Boss 攻擊如何扣主角血

截圖檔案：

`Assets/Game/Combat/Scripts/BossAttackDamageArea.cs`

截圖行數：

- `7-18`：傷害、傷害間隔、是否重複扣血。
- `27-33`：進入或停留在攻擊範圍內都會嘗試扣血。
- `35-70`：判斷玩家、重複傷害間隔、呼叫 PlayerHealth 扣血。

重點功能：

Boss 攻擊物件會用 Trigger Collider 判斷玩家是否碰到攻擊範圍。直刺攻擊可以重複扣血，所以會記錄每個玩家下一次能被扣血的時間；重砸和橫掃只需要命中一次，因此不會反覆扣血。

這裡也會檢查背包是否開啟、敵人是否暫停。當背包或對話狀態存在時，Boss 攻擊不會偷偷扣玩家血。

### 28-21 `SceneTransitionPoint.cs`：洞穴出口與移除擋住退路的藤蔓

截圖檔案：

`Assets/Game/SceneFlow/Scripts/SceneTransitionPoint.cs`

截圖行數：

- `7-11`：目標場景、出生點、Boss 擊敗限制、抵達後要關閉的物件名稱。
- `20-26`：用程式設定轉場資料。
- `28-38`：玩家踩到 Trigger 後載入目標場景。
- `46-54`：如果需要 Boss 擊敗，會檢查 Boss 血量。
- `64-102`：場景載入後把玩家移到指定出生點，並關閉指定物件。
- `104-117`：從 Cave 回到 Exitformcave 時，預設關閉擋住退路的藤蔓物件。

重點功能：

`SceneTransitionPoint` 不只負責載入場景，還可以指定玩家抵達後的位置。這次新增的重點是 `sceneObjectToDisableAfterArrival`。當玩家從 `CaveScene` 的 `Exit` 回到 `ToCaveScene` 的 `Exitformcave` 時，系統會找到擋住退路的藤蔓物件並關閉它。

目前 Unity 物件名稱仍然是 `special box`，所以程式欄位裡要填這個名稱；但文章正文要寫成「擋住退路的藤蔓」。這樣既能對上程式設定，也不會讓讀者覺得場景裡只是放了一個普通方塊。

文章可以寫成：這樣 Boss 戰後的場景狀態會產生變化，玩家回到洞穴外後，原本堵住回頭路的藤蔓會消失，表示 Boss 房封鎖已解除。

### 28-22 `BossCoreBattleController.cs`：Boss 死亡後觸發戰後對話

截圖檔案：

`Assets/Game/Combat/Scripts/BossCoreBattleController.cs`

截圖行數：

- `7-8`：Boss 死亡事件與死亡狀態。
- `191-201`：Boss 血量歸零後停止戰鬥、更新任務、觸發戰後對話。

重點功能：

Boss 血量歸零時，戰鬥狀態會關閉，任務目標更新為「離開洞穴，回村莊向村長回報」，主線狀態推進到 `BossDefeated`，並呼叫 `BossDefeatDialogueDisplay.ShowDialogue()` 播放戰後對話。

這段讓 Boss 死亡不是單純血條歸零，而是正式接到後續回村回報流程。

### 28-23 `BossDefeatDialogueDisplay.cs`：戰後對話

截圖檔案：

`Assets/Game/Combat/Scripts/BossDefeatDialogueDisplay.cs`

截圖行數：

- `9-33`：戰後對話 UI 參數與對話內容。
- `45-76`：建立或取得對話顯示物件。
- `91-123`：開啟戰後對話時鎖住玩家、暫停敵人。
- `137-155`：對話結束後恢復玩家控制，更新任務目標。
- `171-190`：繪製對話框。

重點功能：

戰後對話不是 NPC 互動，而是 Boss 死亡後自動播放。對話開啟時會鎖住玩家、停用攻擊、暫停敵人，避免玩家在對話期間被洞穴小怪或 Boss 攻擊效果干擾。

對話內容維持雷痕符文主線：Boss 感受到雷仍然存在，主角確認雷痕符文已經撕開魔甲，接著決定回村向村長回報。

## Day28 結束時應該達成的狀態

- Boss 攻擊素材已匯入 Unity。
- `BossVineAttackController` 已掛到 Boss 控制物件上。
- Boss 對話結束後，普通直刺會持續攻擊玩家。
- Boss 會隨機施放重砸或橫掃。
- 直刺藤蔓的碰撞箱會隨著藤蔓延長。
- 重砸技能會覆蓋 Boss 房主要 Y 軸範圍。
- 橫掃技能分成左半場與右半場，不再全場橫掃。
- Boss 攻擊會讀取背包、暫停狀態，避免對話或背包期間扣血。
- `ToCaveScene` 有 `tocave` 作為進入洞穴的入口。
- `CaveScene` 有 `Exit` 作為 Boss 戰後離開洞穴的出口。
- `Exit` 只有 Boss 被擊敗後才能使用。
- 從 `Exit` 回到 `Exitformcave` 後，擋住退路的藤蔓會消失。
- Boss 死亡後會播放戰後對話。
- 戰後任務目標變成「離開洞穴，回村莊向村長回報」。

Day28 到這裡結束。下一步應該是做 Day29：回到村莊找村長回報 Boss 已被擊敗，收束主線劇情。
