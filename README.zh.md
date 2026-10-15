# Host Tournament 举办竞技大赛

English | [中文说明](README.zh.md)

《骑马与砍杀 2：霸主》（v1.5.5）模组，让玩家可以在任意当前未举办竞技大赛的城镇，通过缴纳奖金来主动发起一场竞技大赛。

## 功能特性

- **按需举办竞技大赛**：在城镇菜单添加"举办竞技大赛（缴纳奖金）"选项。缴纳 5000 第纳尔即可在当前城镇发起一场竞技大赛。
- **无冷却**：想办就办——唯一的限制是当前城镇不能正在举办竞技大赛。
- **竞技场菜单增强**：进入竞技场时，菜单会列出当前地图上所有正在举办竞技大赛的城镇。
- **菜单图标**：选项带有竞技场战斗图标。
- **无 Harmony 依赖**：纯 `CampaignBehavior` + 游戏菜单实现。

## 安装

### 方式 A：下载 Release（推荐）

1. 打开 [Releases 页面](https://github.com/humphreylibai/HostTournament/releases/latest)。
2. 下载最新版的 `HostTournament.zip`。
3. 将 zip 解压到你的 `Mount & Blade II Bannerlord/Modules/` 目录。zip 内顶层就是 `HostTournament/` 文件夹，解压后应得到 `Modules/HostTournament/SubModule.xml` 和 `Modules/HostTournament/bin/Win64_Shipping_Client/HostTournament.dll`。
4. 启动游戏，在启动器中勾选 **HostTournament**。

### 方式 B：从源码编译

1. 克隆本仓库。
2. 在 `src/HostTournament/` 目录下执行 `dotnet build -c Release`（见下方 [编译](#编译) 章节）。
3. 将生成的 `HostTournament` 文件夹（包含 `SubModule.xml` 和 `bin/Win64_Shipping_Client/HostTournament.dll`）复制到 `Mount & Blade II Bannerlord/Modules/`。
4. 启动游戏，在启动器中勾选 **HostTournament**。

### 方式 C：复制已有安装

如果你已经有模组文件：

1. 将 `HostTournament` 文件夹复制到 `Mount & Blade II Bannerlord/Modules/`。
2. 启动游戏，在启动器中勾选本模组。

## 使用方法

1. 进入一个当前未举办竞技大赛的城镇。
2. 在城镇菜单选择"举办竞技大赛（缴纳奖金）"。
3. 缴纳 5000 第纳尔——竞技大赛随即创建（奖品由游戏自动生成）。
4. 选择立即参赛或稍后再来。

## 配置

编辑 `src/HostTournament/HostTournamentConfig.cs`：

| 配置项 | 默认值 | 说明 |
|--------|--------|------|
| `HostGoldCost` | 5000 | 举办竞技大赛所需金币 |
| `AskToJoinAfterHost` | true | 举办后是否立即询问参赛 |
| `VerboseLog` | false | 详细文件日志 |

修改后重新编译即可生效。

## 编译

```
cd src/HostTournament
"C:\Program Files\dotnet\dotnet.exe" build -c Release
```

DLL 会自动复制到 `bin/Win64_Shipping_Client/`。

## 环境要求

- 《骑马与砍杀 2：霸主》v1.5.5

## 许可证

MIT
