# IronRPG 2D

2026 iThome 鐵人賽製作的 2D 像素風角色扮演遊戲。

玩家會從教學遺跡出發，探索村莊、森林、洞穴道路與洞穴深處，透過拳套與符文戰鬥、累積經驗升級，並挑戰洞穴古樹 Boss。

## 下載與直接遊玩（Windows 64 位元）

**玩家不需要安裝 Unity 或 Unity Hub。**

1. 前往 [最新遊戲版本](https://github.com/Moriartyhung/IronRPG_2D/releases/latest)。
2. 在 **Assets** 下載 `IronRPG_2D-v0.1.0-Windows-x64.zip`。
3. 對 ZIP 按右鍵，選擇「解壓縮全部」，解壓縮到可寫入的資料夾。
4. 開啟解壓縮後的 `IronRPG_2D` 資料夾，雙擊 `IronRPG_2D.exe`。
5. 在首頁選擇「新遊戲」開始遊玩；已有進度時可選擇「繼續遊戲」。

請保留整個遊戲資料夾：`IronRPG_2D_Data`、`MonoBleedingEdge`、`UnityPlayer.dll` 等檔案都需要與 EXE 放在一起。不要只下載或搬移 EXE，也不要直接在 ZIP 內執行。

Release 中的 `Source code (zip)` / `Source code (tar.gz)` 是開發用原始碼，玩家請下載上述 Windows 遊戲 ZIP。目前提供 Windows x64 版本；macOS 與 Linux 尚未提供。

Release 同時提供 SHA-256 校驗檔，可用 PowerShell 的 `Get-FileHash` 核對下載檔案。

## 主要內容

- 四方向移動、奔跑與角色攻擊動畫
- 拳套、符文、背包、商店與藥草系統
- 經驗值、升級、血量、傷害與死亡重生
- 一般怪物、森林精英怪與洞穴 Boss 戰鬥
- 任務對話、場景傳送、背景音樂與戰鬥音效
- 首頁與遊戲內設定選單

## 操作方式

- `WASD`：移動
- `Shift`：奔跑
- `J`：攻擊
- `B`：開啟背包
- `E`：調查或對話
- `T`：交易
- `U`：使用藥草
- `Esc`：設定／返回

## 開發者：開啟 Unity 專案

1. 使用 Unity Hub 加入此專案資料夾。
2. 使用 Unity `6000.5.0f1` 開啟專案。
3. 開啟 `Assets/Scenes/MainMenuScene.unity`。
4. 進入 Play Mode，或透過 Build Profiles 建置 Windows 版本。

## 專案結構

- `Assets/`：場景、腳本、圖片、音樂、音效與第三方素材
- `Packages/`：Unity 套件設定
- `ProjectSettings/`：Unity 專案設定

第三方素材的來源與授權資訊保留在專案內對應的素材與文件目錄中。
