# UiRestyleV05（Creamy Plastic）

> 总索引：`../../README.md`

目的：用大模型批量生成 UI PNG，并**只替换 Unity 工程里的 `.png`（不动 `.meta`）**，方便快速横向对比多套 UI 资产。

**重要（不做裁切/对齐）**  
本工具链不再做任何“对齐尺寸/抠图/裁切/bbox 归一化”。透明 UI 资产非常容易因为这些后处理而被截断（阴影/外轮廓最常见）。正确做法是：**从源头生成时就留足安全边距（padding）**。

## 1) 你需要准备什么
- 生成的文件名必须与工程内目标文件一致（同名覆盖）。  
- 生成的目录结构建议保持一致（例如 `UI_Sprites/`、`World_Sprites/`、`setting_page_assets/`、`BoosterPurchase/`、`ResourcesRoot/`）。
- API Key：放到 `Tools/UiRestyleV05/_secrets/openai_api_key.txt`（一行，不要引号，不提交；这是**代理**的 Key）。
- **禁止使用官方 API（`https://api.openai.com/v1`）**，必须通过代理调用。

## 2) Prompt（主存储：Prompt DB）
- Prompt DB：`Tools/UiRestyleV05/_prompt_db_all_v05.json`
- （可选）导出浏览版 Markdown：`python Tools/UiRestyleV05/PromptDbCli.py export-md --out Tools/UiRestyleV05/_prompt_sheet_all_v05.md`

## 3) 用代理批量出图（必须）
统一使用代理（示例：`https://api.apiyi.com/v1`），不要直连官方 API。

```powershell
python Tools/UiRestyleV05/GenerateOpenAiImages.py `
  --api-base https://api.apiyi.com/v1 `
  --api-key-file Tools/UiRestyleV05/_secrets/openai_api_key.txt `
  --model gpt-image-1.5 `
  --quality low `
  --gen-size auto `
  --background transparent `
  --parallel 5 `
  --prompt-sheet Tools/UiRestyleV05/_prompt_db_all_v05.json `
  --sizes-json Tools/UiRestyleV05/_sizes_ui_sprites.json `
  --out-dir Tools/UiRestyleV05/_openai_output `
  --overwrite
```

背景图注意：`UI_Sprites/bg_main.png` 这类主背景不要用透明背景生成，否则可能得到“全透明背景”导致关卡内背景缺失。建议单独用 `--background opaque` 重出：

```powershell
python Tools/UiRestyleV05/GenerateOpenAiImages.py `
  --api-base https://api.apiyi.com/v1 `
  --api-key-file Tools/UiRestyleV05/_secrets/openai_api_key.txt `
  --model gpt-image-1.5 `
  --quality low `
  --gen-size auto `
  --background opaque `
  --only UI_Sprites/bg_main.png `
  --prompt-sheet Tools/UiRestyleV05/_prompt_db_all_v05.json `
  --out-dir Tools/UiRestyleV05/_openai_output `
  --overwrite
```

## 4) 覆盖到 Unity 工程（保留 `.meta` + 自动备份）
```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir Tools/UiRestyleV05/_openai_output -Backup -AllowPartial
```

（可选）也替换 World_Sprites：
```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir Tools/UiRestyleV05/_openai_output -IncludeWorldSprites -Backup -AllowPartial
```

（可选）也替换额外 Resources（设置页/BoosterPurchase 等）：
```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir Tools/UiRestyleV05/_openai_output -IncludeExtraResources -Backup -AllowPartial
```

## 5) 网页版/其它来源图片的整理（可选）
如果你的图片来自网页版下载或其它来源，只想把它们按目录结构“拷贝成可替换的形态”，可以用：

```powershell
python Tools/UiRestyleV05/NormalizeWebImages.py --in-dir D:\\ui_web_out --out-dir Tools/UiRestyleV05/_web_output --allow-partial --overwrite
```

说明：这个脚本**不会**做任何裁切/对齐/尺寸归一化，只负责结构化拷贝并统一保存为 PNG。

## 6) 多套 UI 资产快速切换（横向对比）
推荐把每一套 UIKit 放到 `Assets/Resources/<PackRoot>/...`，然后只改一个根目录就能整套切换：

- 覆盖到指定包：`Tools/UiRestyleV05/ReplacePngs.ps1 -KitRoot Assets/Resources/<PackRoot>`
- 运行时切换（PlayerPrefs）：`LoopSortingUIKit.ResourcesRoot = "<PackRoot>"`

示例（Unity Editor / 开发版）：
```csharp
PlayerPrefs.SetString("LoopSortingUIKit.ResourcesRoot", "loop_sorting_ui_components_v05_pack_b");
PlayerPrefs.Save();
```
