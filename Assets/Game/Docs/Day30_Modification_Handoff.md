# Day30 修改檔案交接清單

這份文件只記錄 Day30「設定系統」與「首頁」直接相關的既有檔案修改。新增檔案不寫在這份 md 內。

整理原則：

- 每個檔案列出修改位置的行號範圍。
- 說明修改前後差異。
- 只描述既有檔案的修改內容。

## `Assets/Game/Audio/Scripts/GameAudioManager.cs`

### 第 14-17 行

修改前：

- 音樂與音效沒有獨立的玩家設定儲存鍵。
- 沒有預設音量常數。

修改後：

- 新增 `MusicVolumeKey` 與 `SfxVolumeKey`，用來把音樂音量和音效音量存進 `PlayerPrefs`。
- 新增 `DefaultMusicVolume` 與 `DefaultSfxVolume`，作為第一次啟動遊戲時的預設音量。

### 第 32-33 行

修改前：

- `GameAudioManager` 沒有保存目前音樂音量與音效音量的欄位。

修改後：

- 新增 `musicVolume` 與 `sfxVolume`。
- 兩個欄位會先使用預設值，之後會從 `PlayerPrefs` 讀取玩家設定。

### 第 66-82 行

修改前：

- 外部系統無法直接讀取或調整音樂、音效音量。
- 設定選單沒有可呼叫的音樂暫停入口。

修改後：

- 新增 `MusicVolume` 與 `SfxVolume` 讀取屬性。
- 新增 `SetMusicVolume(float volume)`。
- 新增 `SetSfxVolume(float volume)`。
- 新增 `SetMusicPausedBySettings(bool paused)`。
- 這些方法提供給設定選單使用。

### 第 143-146 行

修改前：

- AudioSource 初始化後不會讀取玩家保存的音量設定。

修改後：

- 初始化音源時會從 `PlayerPrefs` 讀取音樂音量與音效音量。
- 讀取後同步套用到 `musicSource.volume` 與 `sfxSource.volume`。

### 第 191-208 行

修改前：

- 場景音樂切換規則沒有首頁場景。

修改後：

- 在 `ChooseMusicForCurrentScene()` 中加入 `MainMenuScene`。
- 首頁使用村莊背景音樂。
- 其他原本的場景音樂規則保留。

### 第 238-254 行

修改前：

- 沒有實際寫入音量設定的方法。

修改後：

- 新增 `SetMusicVolumeInternal(float volume)`。
- 新增 `SetSfxVolumeInternal(float volume)`。
- 調整音量時會同步更新 AudioSource，並寫入 `PlayerPrefs`。

### 第 256-277 行

修改前：

- 打開設定選單時，背景音樂不會暫停。

修改後：

- 新增 `SetMusicPausedBySettingsInternal(bool paused)`。
- 設定選單打開時呼叫 `musicSource.Pause()`。
- 設定選單關閉時呼叫 `musicSource.UnPause()`，必要時重新播放音樂。

## `ProjectSettings/EditorBuildSettings.asset`

### 第 6-38 行

修改前：

- 打包場景列表不是以首頁作為第一個場景。
- 場景列表中曾保留已刪除或不存在的場景項目。

修改後：

- 把 `Assets/Scenes/MainMenuScene.unity` 放到 Build Settings 第 0 個。
- 移除已刪除或不存在的場景項目。
- 目前打包場景順序為：
  1. `Assets/Scenes/MainMenuScene.unity`
  2. `Assets/Scenes/TutorialScene.unity`
  3. `Assets/Scenes/VillageScene.unity`
  4. `Assets/Scenes/PlayerHouseInteriorScene.unity`
  5. `Assets/Scenes/ChiefHouseInteriorScene.unity`
  6. `Assets/Scenes/ForestScene.unity`
  7. `Assets/Scenes/TavernInteriorScene.unity`
  8. `Assets/Scenes/WeaponShopInteriorScene.unity`
  9. `Assets/Scenes/ToCaveScene.unity`
  10. `Assets/Scenes/CaveScene.unity`
