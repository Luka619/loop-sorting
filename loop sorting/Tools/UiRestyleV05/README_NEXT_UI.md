# 下一阶段：全游戏 UI 重做清单（v0.5 / Creamy Plastic）

目标：在已完成 HUD 的基础上，继续把 **设置/商店/购买弹窗/结算/关卡** 等 UI 全部替换到同一风格，并保持“同名同尺寸 + 只替换 `.png` 不动 `.meta`”的闭环流程。

## 1) 资源范围（当前项目）
- UIKit 包内（可直接替换）：
  - `Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/UI_Sprites/*.png`（共 116）
  - `Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/World_Sprites/*.png`（共 29，可选）
- UIKit 包外（也属于 UI，需要一起重做）：
  - `Assets/Resources/setting_page.png`（设置页整张底图，902x1233）
  - `Assets/Resources/setting_page_assets/*.png`（设置页按钮/Toggle 叠加图）
  - `Assets/Resources/BoosterPurchase/*.png`（Boosters 购买弹窗整套图）

## 2) Prompt Sheet（你接下来要用的 2 份）
- HUD：`Tools/UiRestyleV05/_prompt_sheet_hud_v05.md`（已完成）
- 其它界面（设置/商店/面板/Toggle/长按钮等）：`Tools/UiRestyleV05/_prompt_sheet_meta_v05.md`
- 全量（HUD + 其它界面）：`Tools/UiRestyleV05/_prompt_sheet_all_v05.md`

生成方式：
```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/GeneratePromptSheet.ps1 -Scope Meta
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/GeneratePromptSheet.ps1 -Scope All
```

## 3) 推荐的重做顺序（按“用户可见 + 风格一致性”排序）
1) **面板与按钮体系**：`panel_*`, `btn_small_*`, `btn_price_green_*`, `btn_close_red_*`, `*_long_*`, `pill_*`
2) **商店**：`shop_*`（卡片/行/分组条/滚动 fade/装饰 tile）
3) **锁/节点**：`lock_*`（芯片底板/节点底/标签底/锁图标）
4) **设置页**：
   - 整图 `Assets/Resources/setting_page.png`（建议最后做，避免切图对齐返工）
   - `Assets/Resources/setting_page_assets/*.png`（按钮/Toggle 叠加资源）
5) **购买弹窗 BoosterPurchase**：`Assets/Resources/BoosterPurchase/*.png`
6) （可选）**World_Sprites**：完成/锁等世界 UI 视觉统一

## 4) 出图后的一键处理与替换（抠图/对齐/覆盖）
### 4.1 先把你的新图放到 `_openai_output`
目录结构建议保持一致：
- UIKit：`Tools/UiRestyleV05/_openai_output/UI_Sprites/*.png`
- 可选世界：`Tools/UiRestyleV05/_openai_output/World_Sprites/*.png`
- 其它 Resources：
  - `Tools/UiRestyleV05/_openai_output/BoosterPurchase/*.png`
  - `Tools/UiRestyleV05/_openai_output/setting_page_assets/*.png`
  - `Tools/UiRestyleV05/_openai_output/ResourcesRoot/setting_page.png`（如果重做整张 setting_page）

### 4.2 自动抠图 + 尺寸/居中对齐（覆盖写回）
```powershell
python Tools/UiRestyleV05/NormalizeWebImages.py --in-dir Tools/UiRestyleV05/_openai_output --out-dir Tools/UiRestyleV05/_openai_output --prompt-sheet Tools/UiRestyleV05/_prompt_sheet_all_v05.md --overwrite --allow-partial
```

### 4.3 覆盖到 Unity 工程（保留 `.meta` + 自动备份）
UIKit（必做）：
```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir Tools/UiRestyleV05/_openai_output -Backup -AllowPartial
```

UIKit + World_Sprites（可选）：
```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir Tools/UiRestyleV05/_openai_output -IncludeWorldSprites -Backup -AllowPartial
```

UIKit + 其它 Resources（设置页/BoosterPurchase 等）：
```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir Tools/UiRestyleV05/_openai_output -IncludeExtraResources -Backup -AllowPartial
```

## 5) 验收清单（避免“看起来对了但用起来不对”）
- SafeArea：微信胶囊/刘海下顶栏不挤压；按键不被遮挡
- 9-slice：面板/卡片拉伸不变形（边角不被拉伸）
- 点击区：小按钮（close/plus）可点击面积足够
- 文案层级：数字/价格/倍率在深色底上可读、对比足够

