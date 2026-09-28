# Day29 腳本修改說明

這份檔案只整理 Day29 相關腳本中「新增或修改過的程式碼」。

`GameAudioManager.cs` 和 `RuinRouteGuideController.cs` 這兩個檔案不在本文解釋範圍內，後續交給 ChatGPT 另外撰寫。

## 不在本文解釋的檔案

- `Assets/Game/Audio/Scripts/GameAudioManager.cs`
- `Assets/Game/SceneFlow/Scripts/RuinRouteGuideController.cs`

## `SceneTransitionPoint.cs`

檔案位置：

`Assets/Game/SceneFlow/Scripts/SceneTransitionPoint.cs`

修改行號：

- `9-10`
- `20-25`
- `28-38`
- `48-55`
- `64-127`

### 修改前

原本的 `SceneTransitionPoint` 主要只負責兩件事：

1. 玩家踩到傳送觸發區。
2. 讀取目標場景，並把玩家放到指定落點。

也就是說，它比較像一般場景入口，沒有額外判斷「這個出口是否能使用」，也不會在玩家抵達新場景後順便關閉某個擋路物件。

### 修改後

這次新增兩個欄位：

```csharp
[SerializeField] private bool requireBossDefeatedBeforeUse;
[SerializeField] private string sceneObjectToDisableAfterArrival;
```

第一個欄位 `requireBossDefeatedBeforeUse` 用來限制 Boss 戰後出口。當它被勾選時，玩家踩到出口後，程式會先確認 Boss 是否已經死亡，否則不會切換場景。

第二個欄位 `sceneObjectToDisableAfterArrival` 用來指定玩家抵達新場景後，要關閉哪個物件。這次用在洞穴 Boss 戰後回到 `ToCaveScene` 時，移除擋住退路的藤蔓。

`Configure()` 也同步增加參數：

```csharp
public void Configure(
    string sceneName,
    string spawnPointName = "",
    bool requireBossDefeated = false,
    string objectToDisableAfterArrival = "")
```

這樣之後如果其他腳本或編輯器工具需要建立傳送點，就能同時設定：

- 目標場景
- 目標落點
- 是否需要 Boss 死亡後才能使用
- 抵達後要關閉的場景物件

`OnTriggerEnter2D()` 也多了條件檢查：

```csharp
if (isLoading || !IsPlayer(other) || string.IsNullOrWhiteSpace(targetSceneName) || !CanUseTransition())
{
    return;
}
```

修改前只要玩家碰到觸發區就會轉場；修改後會先呼叫 `CanUseTransition()`。如果這個傳送點要求 Boss 死亡，但 Boss 還沒死亡，就不會傳送。

切換場景前，也會記住抵達後要關閉的物件：

```csharp
pendingSpawnPointName = targetSpawnPointName;
pendingObjectToDisableAfterArrival = sceneObjectToDisableAfterArrival;
SceneManager.LoadScene(targetSceneName);
```

場景載入後，`MovePlayerToPendingSpawnPoint()` 會做兩件事：

1. 把玩家移到指定落點。
2. 如果有指定要關閉的物件，就把它關閉。

核心邏輯是：

```csharp
if (hasPendingDisable)
{
    GameObject blocker = GameObject.Find(objectToDisable);
    if (blocker != null)
    {
        blocker.SetActive(false);
    }
}
```

另外，`GetObjectToDisableAfterArrival()` 裡面保留一個預設判斷：

```csharp
if (scene.name == "ToCaveScene" && pendingSpawnPointName == "Exitformcave")
{
    return "special box";
}
```

意思是：如果玩家是從洞穴 Boss 戰後出口回到 `ToCaveScene` 的 `Exitformcave`，即使 Inspector 沒有手動填物件名稱，也會預設關閉 `special box`。

## `PlayerAttack.cs`

檔案位置：

`Assets/Game/Combat/Scripts/PlayerAttack.cs`

修改行號：

- `189-200`

### 修改前

原本主角攻擊後，只會根據是否命中顯示文字：

- 命中敵人：顯示造成多少傷害。
- 沒有命中：顯示攻擊揮空。

但是沒有音效，所以玩家只能靠文字和血條判斷攻擊結果。

### 修改後

現在攻擊結果會同時呼叫音效：

```csharp
if (hitBossPart)
{
    GameAudioManager.PlayPlayerHitEnemy();
    CombatMessageDisplay.Show(!string.IsNullOrWhiteSpace(bossMessage) ? bossMessage : $"拳擊命中，造成 {shownDamage} 點傷害");
    return;
}
```

如果命中 Boss 部位，會播放命中音效。

一般敵人也是同樣邏輯：

```csharp
if (hitEnemy)
{
    GameAudioManager.PlayPlayerHitEnemy();
}
else
{
    GameAudioManager.PlayPlayerAttackMiss();
}
```

修改後的效果是：

- 打中敵人或 Boss 部位：播放命中音效。
- 攻擊揮空：播放揮空音效。
- 原本的文字提示仍然保留。

## `PlayerHealth.cs`

檔案位置：

`Assets/Game/Combat/Scripts/PlayerHealth.cs`

修改行號：

- `38`

### 修改前

主角受到傷害時，程式會扣血、更新受傷無敵時間，然後顯示受傷文字。

### 修改後

在主角扣血後，新增受傷音效：

```csharp
GameAudioManager.PlayPlayerHurt();
```

修改後的效果是：主角每次真的受到傷害時，除了顯示「主角受到多少點傷害」，也會播放受傷音效。

## `EnemyHealth.cs`

檔案位置：

`Assets/Game/Combat/Scripts/EnemyHealth.cs`

修改行號：

- `93`

### 修改前

敵人死亡時，程式會處理掉落、給經驗值、顯示死亡文字，然後觸發死亡事件並刪除敵人物件。

### 修改後

在敵人死亡流程中加入死亡音效：

```csharp
GameAudioManager.PlayEnemyDefeated();
```

修改後的效果是：一般小怪被擊敗時，會播放「敵人被消滅」的音效，讓擊殺回饋更明確。

## `BossCoreBattleController.cs`

檔案位置：

`Assets/Game/Combat/Scripts/BossCoreBattleController.cs`

修改行號：

- `197`

### 修改前

Boss 被擊敗後，`EndBattle()` 會做這些事：

- 結束戰鬥狀態。
- 清掉目前高亮的核心。
- 標記 Boss 已被擊敗。
- 重置核心節點。
- 更新任務目標。
- 推進主線階段。
- 觸發 Boss 擊敗事件。
- 顯示 Boss 擊敗後的對話。

但是沒有擊敗音效。

### 修改後

在 Boss 被擊敗流程中加入：

```csharp
GameAudioManager.PlayEnemyDefeated();
```

修改後的效果是：Boss 死亡時也會播放敵人被消滅的音效。這樣 Boss 戰結束時，玩家除了看到任務更新和對話，也能聽到明確的擊敗回饋。
