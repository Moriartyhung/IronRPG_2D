# Day 29 - 場景傳送、路線封鎖與音樂音效系統

Day29 接續 Day28。Day28 已經完成 Boss 攻擊、洞穴入口與出口，以及 Boss 戰後離開洞穴的流程。

Day29 的重點是回頭修正全流程測試時發現的問題，並補上正式展示需要的聲音表現。

教學遺跡其實是專案最早做出來的場景之一。前期它只是教學區，所以當時沒有完整限制玩家往森林、洞穴道路前進。可是後期森林、武器匠、雷痕符文、洞穴 Boss 都做出來後，這個問題就變得很明顯：如果玩家一開始就能從遺跡提前跑去森林或洞穴道路，主線順序會被打亂，甚至可能在還沒取得雷痕符文前就接觸到後期內容。

因此 Day29 不是「第一次設計遺跡路線」，而是後期全流程測試後，回頭替早期場景補上進度限制。

Day29 完成的內容包含：

1. 全流程測試時，回頭檢查遺跡到村莊、洞穴道路的路線是否能正確銜接。
2. 補上遺跡場景缺少的傳送點與落點，例如 `tocaveroad`、`villagetoruins`、`caveroadtoruins`。
3. 根據後期測試結果，回到教學遺跡補上通往森林、通往洞穴道路的路線封鎖。
4. 根據主線任務階段，讓兩個封鎖碰撞箱在正確時間消失。
5. 先確立每個場景需要的音樂情緒。
6. 上網尋找授權清楚的背景音樂與音效。
7. 將音樂音效素材匯入 Unity。
8. 把匯入的素材接進場景音樂與戰鬥音效系統。

Day29 可以寫成「全流程測試後的路線修正與聲音補強」。這樣比較符合實際開發邏輯：早期先把場景做出來，後期流程完整後才回頭補防跳關機制。

## Day29 文章口徑

前面幾天已經把各個場景和戰鬥系統逐步做出來，但真正從頭開始玩時，問題才會浮出來。

最明顯的是教學遺跡。這是早期建立的場景，當時還沒有森林精英怪、雷痕符文、武器匠確認符文、洞穴 Boss 這些後期流程，所以遺跡通往森林和洞穴道路的位置沒有被嚴格限制。等後期內容做完，再從頭跑流程時就會發現：玩家如果提前走到森林或洞穴道路，會破壞主線順序。

所以 Day29 的第一個重點，是回頭修正這個早期場景的流程漏洞。這不是新增一個漂亮裝飾，而是為了讓主線順序成立：先完成前置對話，才能去森林；取得雷痕符文並讓武器匠確認後，才能往洞穴道路前進。

第二個重點是補齊遺跡場景缺少的傳送點。傳送點這套機制前面很多天已經一直在用，Day29 不需要重新介紹它是什麼。這裡真正要解決的是：全流程測試時發現遺跡作為中繼場景，缺少幾個關鍵入口和回程落點，導致玩家從村莊、洞穴道路回到遺跡時，路線不夠完整。因此這一天回到遺跡場景，把缺少的傳送點和出生點補齊。

第三個重點是補上音樂和音效。聲音素材的流程不能直接寫成「我把音樂放進去」，而是要先確立素材目標，再去網路上找素材，確認授權後匯入 Unity，最後把素材接進場景與程式。這樣文章讀起來才像正常開發流程，而不是突然冒出幾個音檔。

## Day29 截圖總表

### 29-1 全流程測試後補齊遺跡缺少的傳送點

截圖位置：

- 場景：`TutorialScene`
- Scene 視窗拍遺跡整體路線，最好能看到村莊方向、森林方向、洞穴道路方向。
- 選取遺跡中補上的傳送點，例如：
  - `tocaveroad`
  - `Exit_ToVillage`
  - `Exit_ToForest`
- Scene 視窗要截到傳送點位於對應道路上。
- Inspector 要截到：
  - Transform
  - BoxCollider2D
  - Is Trigger 勾選
  - SceneTransitionPoint

文案重點：

傳送點系統前面已經用了很多次，例如進出主角家、村長家、酒館、武器店、森林和洞穴。Day29 不是重新做一套傳送系統，而是全流程測試時發現遺跡場景作為中繼場景，還缺少幾個應該存在的傳送點。

遺跡是玩家最早進入的教學場景，但後期它變成村莊、森林與洞穴道路之間的中繼點。測試完整流程時才會發現，如果這裡缺少傳送點，玩家從村莊出發、完成森林與洞穴流程後，回程路線會不完整。

因此這一步回到 `TutorialScene`，補上缺少的轉場觸發區。例如 `tocaveroad` 是從遺跡前往洞穴道路的觸發點，玩家踩到後會進入 `ToCaveScene`。這些物件的設定沿用前面已經做好的 `SceneTransitionPoint`，所以文章不需要重新解釋傳送點系統，只要說明這次是補齊遺跡場景缺漏。

### 29-2 補上村莊與洞穴道路回到遺跡的落點

截圖位置：

- 場景：`TutorialScene`
- 選取：
  - `villagetoruins`
  - `caveroadtoruins`
- Scene 視窗要截到它們位於遺跡中合理的入口位置。
- Inspector 截到 Transform。

文案重點：

傳送不是只有入口，也需要回來時的落點。全流程測試時，如果玩家從村莊進入遺跡、或從洞穴道路回到遺跡，主角必須出現在合理的位置，而不是被放到場景原點或錯誤角落。

因此在遺跡場景補上兩個落點：

- `villagetoruins`：從村莊進入遺跡時的出生點。
- `caveroadtoruins`：從洞穴道路回到遺跡時的出生點。

這兩個點本身不需要負責轉場，只需要提供位置。其他場景的 `SceneTransitionPoint` 會在切換場景時指定這些名稱，進入 `TutorialScene` 後，系統就會把玩家放到對應落點。

### 29-3 測試從遺跡前往洞穴道路

截圖位置：

- Play Mode。
- 玩家站在遺跡中通往洞穴道路的觸發點附近。
- 截圖可以分兩張：
  - 進入前：玩家在遺跡場景。
  - 轉場後：玩家出現在 `ToCaveScene`。

文案重點：

這一步確認 Day29 補上的遺跡傳送點是否生效。玩家從遺跡走到 `tocaveroad`，應該會進入 `ToCaveScene`，也就是洞穴外部道路。

測試時要確認兩件事：第一，踩到觸發點後場景確實切換；第二，切換後主角出現在正確落點，而不是回到場景原點。

### 29-4 測試從洞穴道路進入洞穴

截圖位置：

- 場景：`ToCaveScene`
- 選取或靠近 `tocave`
- Play Mode 截圖：
  - 玩家站在洞穴入口前。
  - 轉場後進入 `CaveScene`。

文案重點：

`ToCaveScene` 是洞穴外部道路，`CaveScene` 才是真正的洞穴內部。這裡使用 `tocave` 作為進洞入口。玩家走到洞口觸發區後，場景會切換到 `CaveScene`。

這個入口不需要檢查 Boss 是否死亡，因為它是進入洞穴的普通入口。只要玩家任務已經推進到洞穴階段，遺跡的路線封鎖解除後，就能一路走到這裡。

### 29-5 測試 Boss 戰後從洞穴出口離開

截圖位置：

- 場景：`CaveScene`
- Boss 已被擊敗後。
- 玩家走到 `Exit`。
- 轉場後出現在 `ToCaveScene` 的 `Exitformcave`。

文案重點：

Boss 戰後出口和普通入口不一樣。`Exit` 需要確認 Boss 已經被擊敗，玩家才可以使用。這樣玩家不能在 Boss 戰開始前直接從出口離開。

玩家從 `Exit` 離開後，會回到 `ToCaveScene` 的 `Exitformcave`。這個落點表示「從洞穴裡出來的位置」，方便玩家沿著路線回到遺跡，再回村莊向村長回報。

### 29-6 設定遺跡中的方向提示

截圖位置：

- 場景：遺跡場景，也就是 `TutorialScene`。
- Scene 視窗截到方向提示物件，例如 `Direction_ToVillage`。
- Play Mode 截到畫面上出現 `↓ 村莊` 提示。

文案重點：

遺跡是多條路線的交會點。如果只靠玩家記憶，很容易不知道哪條路通往村莊、森林或洞穴道路。這裡先加入方向提示，讓玩家在遺跡場景中能更快理解出口方向。

目前方向提示會在村莊方向顯示 `↓ 村莊`。它不是一張固定貼圖，而是根據場景中指定的 Marker 位置，把文字畫在畫面上。這樣提示會跟著攝影機視角出現在正確位置附近。

### 29-7 設定通往森林與洞穴道路的封鎖碰撞箱

截圖位置：

- 場景：`TutorialScene`
- Scene 視窗先拍一張遺跡整體路線，讓讀者看出它同時連到村莊、森林與洞穴道路。
- 選取 `special box1`
- Scene 視窗要截到它擋在通往森林的路上。
- Inspector 要截到 BoxCollider2D。
- 再選取 `special box2`
- Scene 視窗要截到它擋在通往洞穴道路的路上。
- Inspector 要截到 BoxCollider2D。

文案重點：

教學遺跡是專案最早建立的場景之一。早期做它時，森林、洞穴道路和 Boss 還沒有完整流程，所以當時只先把場景路線做出來，沒有嚴格限制玩家能不能提前往後期區域走。

但是後期做完整體流程測試時，這個問題就出現了：玩家如果一開始就能從遺跡提前去森林，會跳過村長、酒館和武器匠的前置劇情；如果提前去洞穴道路，甚至會在還沒取得雷痕符文前接近最終區域。

所以 Day29 回到這個早期場景，補上兩個路線封鎖碰撞箱：

- `special box1`：擋住通往森林的路。
- `special box2`：擋住通往洞穴道路的路。

這兩個物件不負責劇情演出，只負責實際阻擋玩家。它們的存在是流程修正：不是一開始設計遺跡時就想好的機制，而是後期測試發現主角不應該提前去森林和洞穴後，回頭補上的限制。

文章裡可以把它們稱為「森林路線封鎖」和「洞穴路線封鎖」。物件名稱保留 `special box1`、`special box2` 是為了程式能穩定找到它們。

### 29-8 測試接到森林任務後解除 `special box1`

截圖位置：

- Play Mode。
- 任務還沒推進前，玩家走向森林路線會被擋住。
- 和武器匠完成對話，任務推進到調查森林後。
- 回到遺跡，確認 `special box1` 不再阻擋通往森林的路。

文案重點：

`special box1` 的消失條件是主線階段進入 `ForestInvestigation`。也就是玩家完成前置劇情、接到前往森林調查的任務後，森林路線才會開放。

這裡的邏輯是補救早期場景的流程漏洞：前期遺跡已經存在，但後期才發現需要任務門檻。因此它不是玩家按按鈕關掉碰撞箱，而是任務狀態改變後，遺跡場景每次刷新都會檢查目前主線階段。如果階段已經到達森林調查，就把森林路線封鎖關閉。

### 29-9 測試接到洞穴任務後解除 `special box2`

截圖位置：

- Play Mode。
- 任務還沒推進到洞穴前，玩家走向洞穴道路會被擋住。
- 玩家拿到雷痕符文並讓武器匠確認後，任務推進到準備前往洞穴。
- 回到遺跡，確認 `special box2` 不再阻擋通往洞穴道路的路。

文案重點：

`special box2` 的消失條件是主線階段進入 `PrepareForCave`。這代表玩家已經完成森林精英怪、取得雷痕符文，並回到武器匠那裡確認符文能對 Boss 生效。

在這之前，洞穴道路不會開放。這樣可以避免玩家還沒拿到雷痕符文就跑去洞穴，導致 Boss 戰劇情和戰鬥條件錯亂。

### 29-10 確立音樂素材目標

截圖位置：

- 可以截一張整理好的文字紀錄或 Unity 專案中的 `AudioCredits.md`。
- 重點是讓讀者看到這次不是隨便找音樂，而是先把音樂需求分成幾種場景情緒。

文案重點：

Day29 補上背景音樂前，先確立素材目標。因為不同場景需要不同情緒，如果只找一首泛用 RPG 音樂，村莊、遺跡、洞穴道路和戰鬥會聽起來都一樣。

使用的 BGM 分成四類：

- 村莊與室內安全區：輕快、放鬆。
- 遺跡：偏神秘，但不能太壓迫。
- 前往洞穴道路：緊張、有進入危險區域的感覺。
- 戰鬥：節奏緊湊，用於森林與 Boss 戰。

確立目標後，才開始到網路上搜尋素材。這樣找到音樂時，不是覺得「好像能用」就放進去，而是對照每個場景需要的情緒來挑選。

### 29-11 上網尋找授權清楚的音樂素材

截圖位置：

- 瀏覽器打開 OpenGameArt 的素材頁或搜尋結果頁。
- 建議截其中一個 BGM 素材頁，要看到：
  - 素材名稱。
  - 下載檔案。
  - 授權資訊，最好能看到 CC0。
- 如果畫面空間足夠，可以截 `Port Town Loop` 或 `Dark Shrine Loop` 的頁面。

文案重點：

確立音樂目標後，開始上網找素材。這裡選擇 OpenGameArt，原因是它有大量遊戲用音樂，而且每個素材頁都會標示授權。

這一步最重要的不是「找到好聽的音樂」而已，還要確認授權能不能放進專案。最後挑選的音樂都使用 CC0 授權，適合放進遊戲展示中使用。

最後選用的音樂方向是：

- `Port Town Loop`：村莊與室內安全區。
- `Town in ruins loop`：遺跡。
- `Dark Shrine Loop`：前往洞穴道路。
- `As fast as you can`：戰鬥與 Boss 戰。

文章這裡要截一張網頁圖，讓讀者知道素材不是憑空出現，而是先查找來源、確認授權後才下載。

### 29-12 匯入背景音樂到 Unity

截圖位置：

- Project 視窗打開：
  - `Assets/ThirdParty/OpenGameArt/Audio`
- 要截到原始下載音檔，例如：
  - `port_town_in_game.ogg`
  - `town_in_ruins_loop.ogg`
  - `dark_shrine_loop.ogg`
  - `as_fast_as_you_can_2_31_low.ogg`
- 再補一張 Project 視窗：
  - `Assets/Game/Audio/Resources/Audio`
- 要截到遊戲實際讀取的檔案：
  - `bgm_village.ogg`
  - `bgm_ruins.ogg`
  - `bgm_cave_road.ogg`
  - `bgm_battle.ogg`

文案重點：

下載後先把原始音樂保留在 `Assets/ThirdParty/OpenGameArt/Audio`。這個資料夾的作用是保存第三方素材來源，未來如果要查素材從哪裡來，可以直接回到這裡確認。

接著把實際要讓遊戲讀取的音檔複製到 `Assets/Game/Audio/Resources/Audio`，並改成固定檔名。

檔名統一很重要。場景切換時，程式不需要知道原始素材叫什麼，只需要載入固定名稱：

- `bgm_village`
- `bgm_ruins`
- `bgm_cave_road`
- `bgm_battle`

這樣之後如果想換音樂，只要覆蓋同名檔案，不需要重新改每一個場景。

### 29-13 把音樂素材接進場景

截圖位置：

- Play Mode 的 Hierarchy 截到 `GameAudioManager`。
- Inspector 截到 `GameAudioManager` 物件上的 AudioSource。
- 或打開 `GameAudioManager.cs`，截到依照場景選擇 BGM 的程式碼。
- 建議再截一張 Play Mode 畫面，例如：
  - 村莊正在播放村莊音樂。
  - `ToCaveScene` 正在播放洞穴道路音樂。
  - Boss 戰開始後切成戰鬥音樂。

文案重點：

匯入音樂後，還要把音樂真正接進遊戲。這裡不是在每個場景手動放一個 AudioSource，而是建立一個全域的 `GameAudioManager`。它會在場景載入後判斷目前在哪個場景，再播放對應的 BGM。

這樣做比較適合目前的專案，因為玩家會頻繁在村莊、遺跡、森林、洞穴道路和洞穴之間切換。如果每個場景都各自管理音樂，很容易出現重複播放、音樂斷掉或忘記切換的問題。

目前規則是：

- `VillageScene`、主角家、村長家、酒館、武器店：村莊音樂。
- `TutorialScene` 沒有敵人時：遺跡音樂。
- `TutorialScene` 還有教學史萊姆時：戰鬥音樂。
- `ForestScene`：戰鬥音樂。
- `ToCaveScene`：洞穴道路音樂。
- `CaveScene` 小怪區：洞穴道路音樂。
- `CaveScene` Boss 開場對話結束後：戰鬥音樂。

這一步就是把剛才匯入的素材從「Project 裡的音檔」變成「遊戲實際會播放的 BGM」。

### 29-14 記錄音樂來源與授權

截圖位置：

- 打開：
  - `Assets/Game/Audio/AudioCredits.md`
- 截到來源表格。

文案重點：

只要是從網路取得的素材，就要記錄來源和授權。這裡建立 `AudioCredits.md`，把每一首 BGM 的用途、來源標題、作者、網址和授權都列出來。

目前 BGM 來源如下：

- `Port Town Loop`：村莊與室內安全區。
- `Town in ruins loop`：遺跡。
- `Dark Shrine Loop`：前往洞穴道路。
- `As fast as you can`：戰鬥與 Boss 戰。

這些音樂都使用 CC0 授權，可以放進專案使用。即使 CC0 不強制署名，專案中仍然保留來源記錄，方便之後整理文章或發佈版本時查核。

### 29-15 匯入音效素材

截圖位置：

- Project 視窗打開：
  - `Assets/Game/Audio/Resources/Audio`
- 要截到：
  - `sfx_player_swing_miss.ogg`
  - `sfx_player_hit_enemy.ogg`
  - `sfx_player_hurt.ogg`
  - `sfx_enemy_defeated.ogg`

文案重點：

除了背景音樂，Day29 也補上四種基本音效：

- 主角揮空。
- 主角打中敵人。
- 主角受傷。
- 敵人被消滅。

音效來源使用 Kenney 音效包，授權同樣是 CC0。這些音效不需要像 BGM 一樣長時間循環，而是在事件發生時播放一次。例如主角按攻擊但沒打中敵人，就播放揮空音效；命中敵人或 Boss 部位時，播放命中音效。

### 29-16 測試不同場景的背景音樂

截圖位置：

- Play Mode。
- 可截不同場景畫面：
  - 村莊。
  - 遺跡。
  - `ToCaveScene`。
  - 森林。
  - Cave 小怪區。
  - Boss 戰開始後。

文案重點：

BGM 不是每個場景手動放一個 AudioSource，而是由全域的 `GameAudioManager` 管理。場景載入後，它會判斷目前場景名稱，再決定播放哪一首音樂。

目前規則是：

- `VillageScene`、主角家、村長家、酒館、武器店：村莊音樂。
- `TutorialScene` 沒有敵人時：遺跡音樂。
- `TutorialScene` 還有教學史萊姆時：戰鬥音樂。
- `ForestScene`：戰鬥音樂。
- `ToCaveScene`：洞穴道路音樂。
- `CaveScene` 小怪區：洞穴道路音樂。
- `CaveScene` Boss 開場對話結束後：戰鬥音樂。

這樣 Cave 不會一進去就直接播戰鬥音樂。玩家穿過小怪區時仍然聽到洞穴道路的緊張音樂，等 Boss 戰真正開始後才切換成戰鬥音樂。

### 29-17 測試音效

截圖位置：

- Play Mode。
- 建議錄影或截不同事件畫面：
  - 主角揮空。
  - 主角攻擊命中小怪。
  - 主角被小怪打中。
  - 小怪死亡。

文案重點：

音效主要是補玩家回饋。沒有音效時，玩家只能從血條和文字判斷攻擊是否有效；有音效後，攻擊、受傷、擊殺會更明確。

程式中把音效分成四個公開方法：

- `PlayPlayerAttackMiss()`
- `PlayPlayerHitEnemy()`
- `PlayPlayerHurt()`
- `PlayEnemyDefeated()`

不同系統只要在事件發生時呼叫對應方法即可。主角攻擊腳本負責播放揮空與命中，主角血量腳本負責播放受傷，敵人血量腳本負責播放死亡音效。

## Day29 實作流程整理

### 補齊遺跡缺少的傳送點

這裡不是重新介紹傳送點系統。前面進出主角家、村長家、酒館、武器店、森林、洞穴時，已經反覆使用同一套轉場做法。

Day29 做的是全流程測試後的補修：確認遺跡作為中繼場景時，哪些入口和落點還不完整，然後把缺少的點補回去。

實作時沿用既有做法：

1. 在場景中建立空物件。
2. 命名成這次補上的用途名稱，例如 `tocaveroad`、`villagetoruins`、`caveroadtoruins`。
3. 把物件放到玩家會踩到的位置。
4. 如果它是入口，就加上 `BoxCollider2D` 並勾選 `Is Trigger`。
5. 入口物件掛上 `SceneTransitionPoint`。
6. 填入目標場景與目標落點名稱。
7. 如果它只是回程落點，就只需要放在正確位置，不需要觸發器。
8. 測試玩家從遺跡、村莊、洞穴道路之間切換時，是否會出現在正確位置。

### 確認 Boss 戰後出口銜接

這一段也是路線補修的一部分。Day28 已經做完 Boss 戰與洞穴出口，Day29 要確認它能接回洞穴道路，並且不會讓玩家在 Boss 戰前亂用出口。

檢查重點：

1. 在 `CaveScene` 建立 `Exit`。
2. 加上 `BoxCollider2D` 並勾選 `Is Trigger`。
3. 加上 `SceneTransitionPoint`。
4. `Target Scene Name` 設為 `ToCaveScene`。
5. `Target Spawn Point Name` 設為 `Exitformcave`。
6. 勾選 `Require Boss Defeated Before Use`。
7. 在 `ToCaveScene` 放置 `Exitformcave` 落點。
8. 如果回到 `ToCaveScene` 後要解除擋路藤蔓，就在 `Scene Object To Disable After Arrival` 填入該物件名稱。

### 補上遺跡路線封鎖

這是後期測試才補上的限制。因為遺跡是早期場景，當時還沒有完整主線流程，所以玩家理論上可能提前走到森林或洞穴道路。Day29 回頭補上兩個實體碰撞箱，避免玩家跳過前置劇情。

實作方式：

1. 在 `TutorialScene` 通往森林的路上放置 `special box1`。
2. 加上普通 `BoxCollider2D`，不要勾選 `Is Trigger`，讓它能擋住玩家。
3. 在通往洞穴道路的路上放置 `special box2`。
4. 同樣加上普通 `BoxCollider2D`。
5. 在場景中加入 `RuinRouteGuideController`。
6. 腳本會根據 `MainQuestState.CurrentStage` 判斷是否關閉這兩個封鎖物件。

### 音樂素材匯入

正常流程：

1. 確立素材目標。
   - 村莊要輕快。
   - 遺跡要神秘但不壓迫。
   - 洞穴道路要緊張。
   - 戰鬥音樂要緊湊。
   - 截圖可以截 `AudioCredits.md` 或本日文案中的素材目標整理。

2. 上網找素材。
   - 到 OpenGameArt 搜尋可用於 RPG 的背景音樂。
   - 打開素材頁確認授權，優先選擇 CC0。
   - 這一步要截一張網頁圖，最好能看到素材名稱、下載檔案與授權資訊。

3. 匯入素材。
   - 下載 `.ogg` 檔案。
   - 原始檔保留在 `Assets/ThirdParty/OpenGameArt/Audio`。
   - 複製一份到 `Assets/Game/Audio/Resources/Audio`。
   - 改成固定檔名：
     - `bgm_village.ogg`
     - `bgm_ruins.ogg`
     - `bgm_cave_road.ogg`
     - `bgm_battle.ogg`
   - 這一步要截一張 Project 視窗，拍到匯入後的音檔。

4. 把素材加進場景。
   - 透過 `GameAudioManager` 載入 `Resources/Audio` 裡的音檔。
   - 按照目前場景選擇要播放的 BGM。
   - 進入 Cave 時先播放洞穴道路音樂，Boss 戰開始後再切戰鬥音樂。
   - 這一步可以截 Play Mode 中的 `GameAudioManager`，或截 `GameAudioManager.cs` 的場景音樂判斷段落。

最後把來源和授權寫進 `AudioCredits.md`，避免之後整理文章或發佈版本時忘記素材來源。

### 音效素材匯入

正常流程：

1. 選擇 Kenney 的 CC0 音效素材。
2. 原始檔保留在 `Assets/ThirdParty/Kenney/Audio`。
3. 複製實際要用的音效到 `Assets/Game/Audio/Resources/Audio`。
4. 改成固定檔名：
   - `sfx_player_swing_miss.ogg`
   - `sfx_player_hit_enemy.ogg`
   - `sfx_player_hurt.ogg`
   - `sfx_enemy_defeated.ogg`
5. 在攻擊、受傷、死亡事件中呼叫對應播放方法。

## 程式碼截圖節點

### 29-18 `SceneTransitionPoint.cs`：這次補點用到的轉場欄位

截圖檔案：

`Assets/Game/SceneFlow/Scripts/SceneTransitionPoint.cs`

截圖行數：

- `6-12`：目標場景、落點名稱、Boss 擊敗限制、抵達後要關閉的物件。
- `18-25`：用程式設定傳送資料的 `Configure()`。

重點功能：

這段不是 Day29 新寫的系統，而是本日補齊遺跡路線時會用到的既有轉場欄位。這次新增或檢查的 `tocaveroad`、`toruins`、`Exit` 都會透過這些欄位指定目標場景與落點。Boss 戰後出口還會用到「Boss 死亡後才能使用」以及「抵達後關閉指定物件」。

### 29-19 `SceneTransitionPoint.cs`：玩家踩到 Trigger 後切換場景

截圖檔案：

`Assets/Game/SceneFlow/Scripts/SceneTransitionPoint.cs`

截圖行數：

- `27-39`：玩家進入 Trigger 後檢查條件並載入目標場景。
- `41-45`：判斷進入 Trigger 的是不是玩家。
- `47-55`：如果需要 Boss 死亡，檢查 Boss 血量是否歸零。

重點功能：

玩家走進 Trigger 後，程式會先確認這個碰撞物件是不是玩家，再確認是否有目標場景。如果是 Boss 戰後出口，還會多檢查 Boss 是否已經被擊敗。條件都成立後，才會載入目標場景。

### 29-20 `SceneTransitionPoint.cs`：進入新場景後移動到指定落點

截圖檔案：

`Assets/Game/SceneFlow/Scripts/SceneTransitionPoint.cs`

截圖行數：

- `57-63`：註冊場景載入事件。
- `65-104`：進入新場景後尋找落點，把玩家移到指定位置，並關閉指定物件。
- `114-125`：如果從洞穴回到 `Exitformcave`，自動指定要關閉的擋路物件。

重點功能：

切換場景後，玩家不能永遠出現在場景預設位置。因此傳送前會先記住落點名稱。新場景載入完成後，程式會尋找同名物件，把玩家移到它的位置。

如果傳送點指定了抵達後要關閉的物件，程式也會在這時找到該物件並關閉。Boss 戰後回到洞穴道路時，就是用這個方式讓擋住退路的藤蔓消失。

### 29-21 `RuinRouteGuideController.cs`：遺跡封鎖如何根據任務階段消失

截圖檔案：

`Assets/Game/SceneFlow/Scripts/RuinRouteGuideController.cs`

截圖行數：

- `4-9`：指定遺跡場景、方向提示物件、兩個封鎖物件名稱。
- `53-61`：根據主線階段更新封鎖狀態。
- `63-72`：找到封鎖物件並切換啟用狀態。
- `74-86`：包含未啟用物件也能被找到。

重點功能：

`special box1` 和 `special box2` 不需要玩家手動刪除。它們一直存在於場景中，只是會根據任務階段被啟用或關閉。

規則是：

- 任務階段還沒到 `ForestInvestigation`：`special box1` 保持啟用，擋住森林路。
- 任務階段到達 `ForestInvestigation` 後：`special box1` 關閉，森林路開放。
- 任務階段還沒到 `PrepareForCave`：`special box2` 保持啟用，擋住洞穴道路。
- 任務階段到達 `PrepareForCave` 後：`special box2` 關閉，洞穴道路開放。

這樣路線開放就和劇情進度綁在一起，不會出現玩家還沒拿到雷痕符文就提前跑去洞穴的情況。

### 29-22 `RuinRouteGuideController.cs`：方向提示

截圖檔案：

`Assets/Game/SceneFlow/Scripts/RuinRouteGuideController.cs`

截圖行數：

- `44-50`：尋找村莊方向 Marker。
- `88-104`：把 Marker 世界座標轉成畫面座標，顯示 `↓ 村莊`。

重點功能：

方向提示不是直接固定在螢幕左上角，而是根據場景中的 Marker 位置顯示。腳本先找到 `Direction_ToVillage`，再用攝影機把它的世界座標轉成螢幕座標，最後在對應位置畫出文字。

### 29-23 `GameAudioManager.cs`：音樂與音效載入

截圖檔案：

`Assets/Game/Audio/Scripts/GameAudioManager.cs`

截圖行數：

- `5-13`：BGM 與 SFX 的 Resources 路徑。
- `100-125`：建立 Music AudioSource、SFX AudioSource，並載入音檔。

重點功能：

音樂和音效都放在 `Resources/Audio` 中，因此程式可以用 `Resources.Load<AudioClip>()` 讀取。背景音樂使用一個循環 AudioSource，音效使用另一個 AudioSource 播放一次性聲音。

這樣 BGM 和 SFX 不會互相打斷。背景音樂可以持續播放，攻擊、受傷、死亡音效則用 `PlayOneShot()` 疊在上面。

### 29-24 `GameAudioManager.cs`：依場景切換背景音樂

截圖檔案：

`Assets/Game/Audio/Scripts/GameAudioManager.cs`

截圖行數：

- `31-36`：場景載入後刷新音樂。
- `83-96`：定期檢查音樂是否需要切換。
- `135-158`：依照場景名稱選擇 BGM。
- `161-165`：判斷 Cave 是否進入 Boss 戰。

重點功能：

場景載入後，`GameAudioManager` 會重新判斷該播放哪一首音樂。平常只要場景不變、音樂也沒變，就不會重新播放，避免 BGM 一直從頭開始。

`CaveScene` 比較特殊。洞穴小怪階段使用洞穴道路音樂，等 Boss 開場對話結束、Boss 戰正式開始後，才切換成戰鬥音樂。這樣探索和 Boss 戰有明確的音樂區分。

### 29-25 `GameAudioManager.cs`：音效播放入口

截圖檔案：

`Assets/Game/Audio/Scripts/GameAudioManager.cs`

截圖行數：

- `38-57`：四個公開音效播放方法。
- `182-189`：實際播放一次性音效。

重點功能：

其他腳本不需要知道音效檔案在哪裡，只要呼叫對應方法即可：

- 揮空：`PlayPlayerAttackMiss()`
- 命中：`PlayPlayerHitEnemy()`
- 受傷：`PlayPlayerHurt()`
- 敵人死亡：`PlayEnemyDefeated()`

這樣未來如果要換音效，只要替換音檔或調整 `GameAudioManager`，不需要到每個戰鬥腳本裡改路徑。

### 29-26 戰鬥腳本如何呼叫音效

截圖檔案：

`Assets/Game/Combat/Scripts/PlayerAttack.cs`

截圖行數：

- `187-201`：命中 Boss、命中一般敵人、揮空時播放不同音效。

截圖檔案：

`Assets/Game/Combat/Scripts/PlayerHealth.cs`

截圖行數：

- `29-39`：主角受傷時扣血並播放受傷音效。

截圖檔案：

`Assets/Game/Combat/Scripts/EnemyHealth.cs`

截圖行數：

- `80-94`：敵人死亡時播放敵人消滅音效。

重點功能：

音效不是獨立測試用，而是直接接在現有戰鬥流程中。玩家攻擊時，程式會根據是否命中來決定播放揮空或命中音效；玩家被敵人扣血時播放受傷音效；敵人死亡時播放擊殺音效。

## Day29 結束時應該達成的狀態

- 遺跡、村莊、洞穴道路、洞穴之間的傳送點流程可以正常使用。
- 傳送點使用 Unity 物件、Trigger Collider 與 `SceneTransitionPoint` 實作。
- 進入新場景後，主角會被放到指定落點。
- Boss 戰後出口只有 Boss 被擊敗後才能使用。
- Boss 戰後從洞穴回到 `ToCaveScene` 時，擋住退路的藤蔓會消失。
- 遺跡中的 `special box1` 會在接到森林調查任務後關閉。
- 遺跡中的 `special box2` 會在接到前往洞穴任務後關閉。
- 遺跡中有村莊方向提示，讓玩家知道出口方向。
- 背景音樂已從網路尋找、確認授權、下載並匯入 Unity。
- 音樂來源與授權記錄在 `AudioCredits.md`。
- 村莊、遺跡、洞穴道路、戰鬥狀態會播放不同 BGM。
- Cave 小怪階段播放洞穴道路音樂，Boss 戰開始後才切換戰鬥音樂。
- 主角揮空、命中、受傷、敵人死亡都有對應音效。

Day29 到這裡結束。接下來 Day30 可以收尾：測試從主角家出發，到村莊、遺跡、森林、武器匠、洞穴、Boss、回村回報的完整流程，並整理最後一天的完成版展示。
