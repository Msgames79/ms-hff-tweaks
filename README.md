# M's HFF Tweaks - 機能追加mod

## 機能一覧 (GUI推奨)
### マルチプレイのホワイトリスト、ブラックリスト作成(Apply whitelist/blacklist)
有効にすると専用のウィンドウが出現します(両方有効にすることはできません)
入力枠にSteamID(17桁の数字)を入れるとホワイトリストまたはブラックリストとして機能します
### 無敵のボブ(Invincible humans)
有効にするとボブが死ななくなり、あらゆるリスポーンを無効化します
※ただし死亡系のカウントは増加させるようにしています
※仕様上ゴールもできなくなります
### チェックポイント固定(Disable new checkpoints)
有効にするとチェックポイントの設定が展開され、固定するチェックポイントが選べるようになります
### ゴール判定無効化(Disable passtriggers)
有効にするとゴールが無効化されます(マルチプレイのステージロックを動的に行えるようにしている感じ)

### コマンド追加
基本コマンド : 
```
hfftweaks <commands> <option>
```
または
```
ht <commands> [<option>]
```
### `commands`一覧(1行目はフルネームによる記述、2行目はエイリアスを使用した最短記法)
ホワイトリストの操作
```
hfftweaks whitelist [<subcommands>]
```
```
ht wl [<subcommands>]
```
`<subcommands>`について
- なし または `apply [<booloptions>]`: 適用設定(`bool`値なので[RevertBehaviorのbool値設定](https://github.com/Msgames79/hff_revertbehavior#:~:text=rb%20rfr%20%5B%3Coption%3E%5D-,%3C,-option%3E%E3%81%AF)と同じ、以下`booloption`であれば同じ)
- `add ^[0-9]{17}$`: ホワイトリストにユーザーを追加
- `remove <index>`または`delete <index>`: 指定したインデックスの値を削除
- `clear`: ホワイトリストをクリア
ブラックリストの操作
```
hfftweaks blacklist [<subcommands>]
```
```
ht bl [<subcommands>]
```
`<subcommands>`について
- なし または `apply [<booloption>]`: 適用設定
- `add ^[0-9]{17}$`: ブラックリストにユーザーを追加
- `remove <index>`または`delete <index>`: 指定したインデックスの値を削除
- `clear`: ブラックリストをクリア
無敵のボブ
```
hfftweaks invinciblehumans [<booloption>]
```
```
ht ih [<booloption>]
```
チェックポイント固定
```
hfftweaks disablecheckpoints [<booloption>]
```
```
ht dcp [<booloption>]
```
ゴール判定無効化
```
hfftweaks disablepasstrigger [<booloption>]
```
```
ht dpt [<booloption>]
```


## インストール
1. Steam版Human Fall Flatに`BepInEx`をインストール
2. [Releases](https://github.com/Msgames79/ms-hff-tweaks/Releases/latest)からdllファイル(`Ms-HFF-Tweaks-(version).dll`)をダウンロード
3. `Ms-HFF-Tweaks-(version).dll`を`BepInEx\plugins`に置く

またはPowerShellで
```ps1
Invoke-RestMethod -Uri "https://raw.githubusercontent.com/Msgames79/ms-hff-tweaks/refs/heads/main/install.ps1" | Invoke-Expression
```

## 過去の機能

## バージョン履歴
### 0.1.0
プレリリース