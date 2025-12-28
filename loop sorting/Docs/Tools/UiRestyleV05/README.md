# UiRestyleV05锛圕reamy Plastic锛?
## API易 / gpt-image-1.5 (proxy only)
- Base URL: `https://api.apiyi.com/v1` (proxy only; never use `https://api.openai.com/v1`)
- Key file: `Tools/UiRestyleV05/_secrets/openai_api_key.txt` (proxy key, one line, no quotes, do not commit)
- This same key file can be reused by `Tools/ImageGen` for single-asset generation.

> 鎬荤储寮曪細`../../README.md`

鐩殑锛氱敤澶фā鍨嬫壒閲忕敓鎴?UI PNG锛屽苟**鍙浛鎹?Unity 宸ョ▼閲岀殑 `.png`锛堜笉鍔?`.meta`锛?*锛屾柟渚垮揩閫熸í鍚戝姣斿濂?UI 璧勪骇銆?
**閲嶈锛堜笉鍋氳鍒?瀵归綈锛?*  
鏈伐鍏烽摼涓嶅啀鍋氫换浣曗€滃榻愬昂瀵?鎶犲浘/瑁佸垏/bbox 褰掍竴鍖栤€濄€傞€忔槑 UI 璧勪骇闈炲父瀹规槗鍥犱负杩欎簺鍚庡鐞嗚€岃鎴柇锛堥槾褰?澶栬疆寤撴渶甯歌锛夈€傛纭仛娉曟槸锛?*浠庢簮澶寸敓鎴愭椂灏辩暀瓒冲畨鍏ㄨ竟璺濓紙padding锛?*銆?
## 1) 浣犻渶瑕佸噯澶囦粈涔?- 鐢熸垚鐨勬枃浠跺悕蹇呴』涓庡伐绋嬪唴鐩爣鏂囦欢涓€鑷达紙鍚屽悕瑕嗙洊锛夈€? 
- 鐢熸垚鐨勭洰褰曠粨鏋勫缓璁繚鎸佷竴鑷达紙渚嬪 `UI_Sprites/`銆乣World_Sprites/`銆乣setting_page_assets/`銆乣BoosterPurchase/`銆乣ResourcesRoot/`锛夈€?- API Key锛氭斁鍒?`Tools/UiRestyleV05/_secrets/openai_api_key.txt`锛堜竴琛岋紝涓嶈寮曞彿锛屼笉鎻愪氦锛涜繖鏄?*浠ｇ悊**鐨?Key锛夈€?- **绂佹浣跨敤瀹樻柟 API锛坄https://api.openai.com/v1`锛?*锛屽繀椤婚€氳繃浠ｇ悊璋冪敤銆?
## 2) Prompt锛堜富瀛樺偍锛歅rompt DB锛?- Prompt DB锛歚Tools/UiRestyleV05/_prompt_db_all_v05.json`
- 锛堝彲閫夛級瀵煎嚭娴忚鐗?Markdown锛歚python Tools/UiRestyleV05/PromptDbCli.py export-md --out Tools/UiRestyleV05/_prompt_sheet_all_v05.md`

## 3) 鐢ㄤ唬鐞嗘壒閲忓嚭鍥撅紙蹇呴』锛?缁熶竴浣跨敤浠ｇ悊锛堢ず渚嬶細`https://api.apiyi.com/v1`锛夛紝涓嶈鐩磋繛瀹樻柟 API銆?
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

鑳屾櫙鍥炬敞鎰忥細`UI_Sprites/bg_main.png` 杩欑被涓昏儗鏅笉瑕佺敤閫忔槑鑳屾櫙鐢熸垚锛屽惁鍒欏彲鑳藉緱鍒扳€滃叏閫忔槑鑳屾櫙鈥濆鑷村叧鍗″唴鑳屾櫙缂哄け銆傚缓璁崟鐙敤 `--background opaque` 閲嶅嚭锛?
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

## 4) 瑕嗙洊鍒?Unity 宸ョ▼锛堜繚鐣?`.meta` + 鑷姩澶囦唤锛?```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir Tools/UiRestyleV05/_openai_output -Backup -AllowPartial
```

锛堝彲閫夛級涔熸浛鎹?World_Sprites锛?```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir Tools/UiRestyleV05/_openai_output -IncludeWorldSprites -Backup -AllowPartial
```

锛堝彲閫夛級涔熸浛鎹㈤澶?Resources锛堣缃〉/BoosterPurchase 绛夛級锛?```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir Tools/UiRestyleV05/_openai_output -IncludeExtraResources -Backup -AllowPartial
```

## 5) 缃戦〉鐗?鍏跺畠鏉ユ簮鍥剧墖鐨勬暣鐞嗭紙鍙€夛級
濡傛灉浣犵殑鍥剧墖鏉ヨ嚜缃戦〉鐗堜笅杞芥垨鍏跺畠鏉ユ簮锛屽彧鎯虫妸瀹冧滑鎸夌洰褰曠粨鏋勨€滄嫹璐濇垚鍙浛鎹㈢殑褰㈡€佲€濓紝鍙互鐢細

```powershell
python Tools/UiRestyleV05/NormalizeWebImages.py --in-dir D:\\ui_web_out --out-dir Tools/UiRestyleV05/_web_output --allow-partial --overwrite
```

璇存槑锛氳繖涓剼鏈?*涓嶄細**鍋氫换浣曡鍒?瀵归綈/灏哄褰掍竴鍖栵紝鍙礋璐ｇ粨鏋勫寲鎷疯礉骞剁粺涓€淇濆瓨涓?PNG銆?
## 6) 澶氬 UI 璧勪骇蹇€熷垏鎹紙妯悜瀵规瘮锛?鎺ㄨ崘鎶婃瘡涓€濂?UIKit 鏀惧埌 `Assets/Resources/<PackRoot>/...`锛岀劧鍚庡彧鏀逛竴涓牴鐩綍灏辫兘鏁村鍒囨崲锛?
- 瑕嗙洊鍒版寚瀹氬寘锛歚Tools/UiRestyleV05/ReplacePngs.ps1 -KitRoot Assets/Resources/<PackRoot>`
- 杩愯鏃跺垏鎹紙PlayerPrefs锛夛細`LoopSortingUIKit.ResourcesRoot = "<PackRoot>"`

绀轰緥锛圲nity Editor / 寮€鍙戠増锛夛細
```csharp
PlayerPrefs.SetString("LoopSortingUIKit.ResourcesRoot", "loop_sorting_ui_components_v05_pack_b");
PlayerPrefs.Save();
```

