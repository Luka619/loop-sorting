using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LoopSorting
{
    public static class LoopSortingUIKit
    {
        [Serializable]
        private sealed class ConfigFile
        {
            public string resourcesRoot;
            public LayoutFile layout;
            public NineSliceRuleFile[] nineSliceRules;
            public SpriteEntry[] sprites;
            public TextureEntry[] textures;
        }

        [Serializable]
        private sealed class LayoutFile
        {
            public float referenceWidth = 1080f;
            public float referenceHeight = 1920f;
            public float reservedTop = 0.10f;
            public float reservedBottom = 0.20f;
            public LayoutModulesFile modules;
            public BoosterLayoutFile boosters;
        }

        [Serializable]
        private sealed class LayoutModulesFile
        {
            public RectFile counter;
            public RectFile speed;
            public RectFile settings;
            public RectFile level;
            public RectFile shop;
            public RectFile coins;
            public RectFile lives;
        }

        [Serializable]
        private sealed class BoosterLayoutFile
        {
            public float anchorX = 0.5f;
            public float anchorY = 0.10f;
            public float offsetX = 185f;
            public float offsetY = 0f;
            public float width = 340f;
            public float height = 340f;
        }

        [Serializable]
        private sealed class RectFile
        {
            // Rect in TOP-LEFT origin pixels in reference resolution.
            public float x;
            public float y;
            public float w;
            public float h;
        }

        [Serializable]
        private sealed class NineSliceRuleFile
        {
            // Pattern supports either exact file name (e.g. "panel_modal.png") or prefix wildcard (e.g. "mint_square_*").
            public string pattern;
            // Matches manifest.json order: [left, right, top, bottom].
            public int[] border;
        }

        [Serializable]
        private sealed class SpriteEntry
        {
            public string key;
            public string path;
            public float pixelsPerUnit = 100f;
            public bool applyNineSlice = true;
        }

        [Serializable]
        private sealed class TextureEntry
        {
            public string key;
            public string path;
        }

        private sealed class NineSliceRule
        {
            public readonly string Prefix;
            public readonly string Exact;
            public readonly Vector4 BorderUnity; // left, bottom, right, top

            public NineSliceRule(string prefix, string exact, Vector4 borderUnity)
            {
                Prefix = prefix;
                Exact = exact;
                BorderUnity = borderUnity;
            }

            public bool Matches(string fileName)
            {
                if (!string.IsNullOrEmpty(Exact))
                {
                    return string.Equals(fileName, Exact, StringComparison.OrdinalIgnoreCase);
                }

                if (!string.IsNullOrEmpty(Prefix))
                {
                    return fileName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase);
                }

                return false;
            }
        }

        private const string DefaultResourcesRoot = "loop_sorting_ui_components_v04_4_meta_pack_firework_confetti";
        private const string ConfigResourcePath = "LoopSortingUIKitConfig";

        private static readonly Dictionary<string, Texture2D> TextureCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private static ConfigFile _config;
        private static string _configRootCached;
        private static Dictionary<string, SpriteEntry> _spriteByKey;
        private static Dictionary<string, TextureEntry> _textureByKey;
        private static NineSliceRule[] _nineSliceRules;
        private static RuntimeLayout _runtimeLayout;
        private static bool _runtimeLayoutFromConfig;

        public struct RuntimeLayout
        {
            public float referenceWidth;
            public float referenceHeight;
            public float reservedTop;
            public float reservedBottom;
            public Rect counter;
            public Rect speed;
            public Rect settings;
            public Rect level;
            public Rect shop;
            public Rect coins;
            public Rect lives;
            public Vector2 boosterAnchor;
            public Vector2 boosterOffset;
            public Vector2 boosterSize;
        }

        public static bool IsAvailable()
        {
            EnsureConfig();
            string root = GetResourcesRoot();
            if (string.IsNullOrEmpty(root)) return false;

            // Check one stable key if present; otherwise fall back to a known file name.
            var sprite = LoadSpriteByKey("ui.button.mint_square.normal");
            if (sprite != null) return true;

            return Resources.Load<Texture2D>($"{root}/UI_Sprites/mint_square_normal") != null;
        }

        public static string GetResourcesRoot()
        {
            EnsureConfig();
            if (_config != null && !string.IsNullOrWhiteSpace(_config.resourcesRoot))
            {
                return _config.resourcesRoot.Trim();
            }
            return DefaultResourcesRoot;
        }

        public static bool TryGetRuntimeLayout(out RuntimeLayout layout)
        {
            EnsureConfig();
            layout = _runtimeLayout;
            return _runtimeLayoutFromConfig;
        }

        public static RuntimeLayout GetRuntimeLayout()
        {
            EnsureConfig();
            return _runtimeLayout;
        }

        public static Sprite LoadSpriteByKey(string key)
        {
            EnsureConfig();
            if (string.IsNullOrWhiteSpace(key)) return null;

            if (_spriteByKey != null && _spriteByKey.TryGetValue(key.Trim(), out var entry) && entry != null)
            {
                return LoadSprite(entry.path, pixelsPerUnit: entry.pixelsPerUnit, applyNineSlice: entry.applyNineSlice);
            }

            return null;
        }

        public static Texture2D LoadTextureByKey(string key)
        {
            EnsureConfig();
            if (string.IsNullOrWhiteSpace(key)) return null;

            if (_textureByKey != null && _textureByKey.TryGetValue(key.Trim(), out var entry) && entry != null)
            {
                return LoadTexture(entry.path);
            }

            return null;
        }

        public static string NormalizeRelativePath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return string.Empty;
            }

            var path = relativePath.Replace('\\', '/').TrimStart('/');
            var root = GetResourcesRoot();
            if (!string.IsNullOrEmpty(root) && path.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(root.Length + 1);
            }

            // Resources.Load expects no extension.
            var ext = Path.GetExtension(path);
            if (!string.IsNullOrEmpty(ext))
            {
                path = path.Substring(0, path.Length - ext.Length);
            }

            return path;
        }

        public static Texture2D LoadTexture(string relativePath)
        {
            EnsureConfig();
            var normalized = NormalizeRelativePath(relativePath);
            if (string.IsNullOrEmpty(normalized))
            {
                return null;
            }

            string key = $"{GetResourcesRoot()}/{normalized}";
            if (TextureCache.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var tex = Resources.Load<Texture2D>($"{GetResourcesRoot()}/{normalized}");
            TextureCache[key] = tex;
            return tex;
        }

        public static Sprite LoadSprite(string relativePath, float pixelsPerUnit = 100f, bool applyNineSlice = true)
        {
            EnsureConfig();
            var normalized = NormalizeRelativePath(relativePath);
            if (string.IsNullOrEmpty(normalized))
            {
                return null;
            }

            string cacheKey = $"{GetResourcesRoot()}/{normalized}|ppu={pixelsPerUnit:0.###}|9={applyNineSlice}";
            if (SpriteCache.TryGetValue(cacheKey, out var cached) && cached != null)
            {
                return cached;
            }

            // Try direct sprite load first (in case the project later imports these as Sprites).
            var direct = Resources.Load<Sprite>($"{GetResourcesRoot()}/{normalized}");
            if (direct != null)
            {
                if (!applyNineSlice)
                {
                    SpriteCache[cacheKey] = direct;
                    return direct;
                }

                // If the sprite is imported without 9-slice borders, recreate it at runtime with config borders.
                // This keeps UI consistent even when assets are imported as Sprites in Unity with default (zero) borders.
                Vector4 sliceBorder = Vector4.zero; // Unity order: left, bottom, right, top
                string fileName = Path.GetFileName(relativePath.Replace('\\', '/'));
                var rules = GetNineSliceRules();
                for (int i = 0; i < rules.Length; i++)
                {
                    if (rules[i].Matches(fileName))
                    {
                        sliceBorder = rules[i].BorderUnity;
                        break;
                    }
                }

                if (sliceBorder.sqrMagnitude <= 0.0001f || direct.border.sqrMagnitude > 0.0001f)
                {
                    SpriteCache[cacheKey] = direct;
                    return direct;
                }

                var directTex = direct.texture;
                if (directTex == null)
                {
                    SpriteCache[cacheKey] = direct;
                    return direct;
                }

                var recreated = Sprite.Create(
                    directTex,
                    direct.rect,
                    new Vector2(0.5f, 0.5f),
                    Mathf.Max(1f, pixelsPerUnit),
                    0,
                    SpriteMeshType.FullRect,
                    sliceBorder);

                recreated.name = $"{GetResourcesRoot()}/{normalized}";
                SpriteCache[cacheKey] = recreated;
                return recreated;
            }

            var tex = LoadTexture(normalized);
            if (tex == null)
            {
                SpriteCache[cacheKey] = null;
                return null;
            }

            Vector4 border = Vector4.zero; // Unity order: left, bottom, right, top
            if (applyNineSlice)
            {
                string fileName = Path.GetFileName(relativePath.Replace('\\', '/'));
                var rules = GetNineSliceRules();
                for (int i = 0; i < rules.Length; i++)
                {
                    if (rules[i].Matches(fileName))
                    {
                        border = rules[i].BorderUnity;
                        break;
                    }
                }
            }

            var sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f),
                Mathf.Max(1f, pixelsPerUnit),
                0,
                SpriteMeshType.FullRect,
                border);

            sprite.name = $"{GetResourcesRoot()}/{normalized}";
            SpriteCache[cacheKey] = sprite;
            return sprite;
        }

        public static Material CreateUnlitTextureMaterial(Texture2D texture, Color color, int renderQueue)
        {
            var shader =
                Shader.Find("Unlit/Transparent") ??
                Shader.Find("Unlit/Texture") ??
                Shader.Find("Sprites/Default") ??
                Shader.Find("UI/Default") ??
                Shader.Find("Standard");

            if (shader == null)
            {
                return null;
            }

            var mat = new Material(shader);
            mat.mainTexture = texture;
            mat.color = color;
            mat.renderQueue = renderQueue;

            // Best-effort: keep it from writing depth so overlays behave nicely.
            if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 0);
            return mat;
        }

        private static void EnsureConfig()
        {
            if (_config != null && _spriteByKey != null && _textureByKey != null && _nineSliceRules != null)
            {
                return;
            }

            _config = LoadConfigInternal();
            _configRootCached = _config != null && !string.IsNullOrWhiteSpace(_config.resourcesRoot)
                ? _config.resourcesRoot.Trim()
                : DefaultResourcesRoot;

            TextureCache.Clear();
            SpriteCache.Clear();
            _spriteByKey = BuildSpriteKeyMap(_config);
            _textureByKey = BuildTextureKeyMap(_config);
            _nineSliceRules = CompileNineSliceRules(_config);
            _runtimeLayout = BuildRuntimeLayout(_config, out _runtimeLayoutFromConfig);
        }

        private static ConfigFile LoadConfigInternal()
        {
            try
            {
                var ta = Resources.Load<TextAsset>(ConfigResourcePath);
                if (ta == null || string.IsNullOrWhiteSpace(ta.text))
                {
                    return DefaultConfig();
                }

                var parsed = JsonUtility.FromJson<ConfigFile>(ta.text);
                if (parsed == null)
                {
                    return DefaultConfig();
                }
                return parsed;
            }
            catch
            {
                return DefaultConfig();
            }
        }

        private static ConfigFile DefaultConfig()
        {
            // Mirrors the current kit so the game still works even if config is missing.
            return new ConfigFile
            {
                resourcesRoot = DefaultResourcesRoot,
                layout = new LayoutFile
                {
                    referenceWidth = 1080f,
                    referenceHeight = 1920f,
                    reservedTop = 0.09f,
                    reservedBottom = 0.11f,
                    modules = new LayoutModulesFile
                    {
                        shop = new RectFile { x = 24f, y = 20f, w = 84f, h = 84f },
                        counter = new RectFile { x = 120f, y = 20f, w = 220f, h = 84f },
                        level = new RectFile { x = 380f, y = 20f, w = 320f, h = 84f },
                        speed = new RectFile { x = 876f, y = 20f, w = 84f, h = 84f },
                        settings = new RectFile { x = 972f, y = 20f, w = 84f, h = 84f },
                        lives = new RectFile { x = 600f, y = 120f, w = 220f, h = 78f },
                        coins = new RectFile { x = 836f, y = 120f, w = 220f, h = 78f },
                    },
                    boosters = new BoosterLayoutFile
                    {
                        anchorX = 0.5f,
                        anchorY = 0.07f,
                        offsetX = 120f,
                        offsetY = 0f,
                        width = 180f,
                        height = 180f,
                    },
                },
                nineSliceRules = new[]
                {
                    new NineSliceRuleFile { pattern = "mint_square_*", border = new[] {170,170,170,170} },
                    new NineSliceRuleFile { pattern = "purple_square_*", border = new[] {170,170,170,170} },
                    new NineSliceRuleFile { pattern = "orange_square_*", border = new[] {170,170,170,170} },
                    new NineSliceRuleFile { pattern = "mint_long_*", border = new[] {140,140,90,90} },
                    new NineSliceRuleFile { pattern = "purple_long_*", border = new[] {140,140,90,90} },
                    new NineSliceRuleFile { pattern = "orange_long_*", border = new[] {140,140,90,90} },
                    new NineSliceRuleFile { pattern = "pill_bg*", border = new[] {90,90,60,60} },
                    new NineSliceRuleFile { pattern = "panel_modal.png", border = new[] {120,120,120,120} },
                    new NineSliceRuleFile { pattern = "panel_result.png", border = new[] {120,120,120,120} },
                    new NineSliceRuleFile { pattern = "tag_fast_*", border = new[] {60,60,40,40} },
                    new NineSliceRuleFile { pattern = "tag_small_*", border = new[] {50,50,30,30} },
                    new NineSliceRuleFile { pattern = "lock_overlay.png", border = new[] {60,60,60,60} },
                },
                sprites = new[]
                {
                    new SpriteEntry { key = "ui.bg_main", path = "UI_Sprites/bg_main.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.counter.bg", path = "UI_Sprites/pill_bg.png", pixelsPerUnit = 100f, applyNineSlice = true },
                    new SpriteEntry { key = "ui.counter.icon", path = "UI_Sprites/icon_loop.png", pixelsPerUnit = 100f, applyNineSlice = false },

                    new SpriteEntry { key = "ui.button.mint_square.normal", path = "UI_Sprites/mint_square_normal.png", pixelsPerUnit = 100f, applyNineSlice = true },
                    new SpriteEntry { key = "ui.button.mint_square.pressed", path = "UI_Sprites/mint_square_pressed.png", pixelsPerUnit = 100f, applyNineSlice = true },
                    new SpriteEntry { key = "ui.button.mint_square.disabled", path = "UI_Sprites/mint_square_disabled.png", pixelsPerUnit = 100f, applyNineSlice = true },

                    new SpriteEntry { key = "ui.button.purple_square.normal", path = "UI_Sprites/purple_square_normal.png", pixelsPerUnit = 100f, applyNineSlice = true },
                    new SpriteEntry { key = "ui.button.purple_square.pressed", path = "UI_Sprites/purple_square_pressed.png", pixelsPerUnit = 100f, applyNineSlice = true },
                    new SpriteEntry { key = "ui.button.purple_square.disabled", path = "UI_Sprites/purple_square_disabled.png", pixelsPerUnit = 100f, applyNineSlice = true },

                    new SpriteEntry { key = "ui.button.mint_long.normal", path = "UI_Sprites/mint_long_normal.png", pixelsPerUnit = 100f, applyNineSlice = true },
                    new SpriteEntry { key = "ui.button.mint_long.pressed", path = "UI_Sprites/mint_long_pressed.png", pixelsPerUnit = 100f, applyNineSlice = true },
                    new SpriteEntry { key = "ui.button.mint_long.disabled", path = "UI_Sprites/mint_long_disabled.png", pixelsPerUnit = 100f, applyNineSlice = true },

                    new SpriteEntry { key = "ui.button.orange_long.normal", path = "UI_Sprites/orange_long_normal.png", pixelsPerUnit = 100f, applyNineSlice = true },
                    new SpriteEntry { key = "ui.button.orange_long.pressed", path = "UI_Sprites/orange_long_pressed.png", pixelsPerUnit = 100f, applyNineSlice = true },
                    new SpriteEntry { key = "ui.button.orange_long.disabled", path = "UI_Sprites/orange_long_disabled.png", pixelsPerUnit = 100f, applyNineSlice = true },

                    new SpriteEntry { key = "ui.icon.gear", path = "UI_Sprites/icon_gear.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.icon.close", path = "UI_Sprites/icon_close.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.icon.fill", path = "UI_Sprites/icon_fill.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.icon.shuffle", path = "UI_Sprites/icon_shuffle.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.icon.next", path = "UI_Sprites/icon_next.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.icon.retry", path = "UI_Sprites/icon_retry.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.icon.lock", path = "UI_Sprites/icon_lock.png", pixelsPerUnit = 100f, applyNineSlice = false },

                    new SpriteEntry { key = "ui.badge.bg", path = "UI_Sprites/badge_red_bg.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.digit.0", path = "UI_Sprites/digit_0.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.digit.1", path = "UI_Sprites/digit_1.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.digit.2", path = "UI_Sprites/digit_2.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.digit.3", path = "UI_Sprites/digit_3.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.digit.4", path = "UI_Sprites/digit_4.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.digit.5", path = "UI_Sprites/digit_5.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.digit.6", path = "UI_Sprites/digit_6.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.digit.7", path = "UI_Sprites/digit_7.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.digit.8", path = "UI_Sprites/digit_8.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.digit.9", path = "UI_Sprites/digit_9.png", pixelsPerUnit = 100f, applyNineSlice = false },

                    new SpriteEntry { key = "ui.tag_fast.info", path = "UI_Sprites/tag_fast_info_bg.png", pixelsPerUnit = 100f, applyNineSlice = true },
                    new SpriteEntry { key = "ui.tag_fast.danger", path = "UI_Sprites/tag_fast_danger_bg.png", pixelsPerUnit = 100f, applyNineSlice = true },
                    new SpriteEntry { key = "ui.tag_small.info", path = "UI_Sprites/tag_small_info_bg.png", pixelsPerUnit = 100f, applyNineSlice = true },

                    new SpriteEntry { key = "ui.overlay_dim", path = "UI_Sprites/overlay_dim.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.panel_modal", path = "UI_Sprites/panel_modal.png", pixelsPerUnit = 100f, applyNineSlice = true },
                    new SpriteEntry { key = "ui.panel_result", path = "UI_Sprites/panel_result.png", pixelsPerUnit = 100f, applyNineSlice = true },

                    new SpriteEntry { key = "ui.toggle.track_on", path = "UI_Sprites/toggle_track_on.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.toggle.track_off", path = "UI_Sprites/toggle_track_off.png", pixelsPerUnit = 100f, applyNineSlice = false },
                    new SpriteEntry { key = "ui.toggle.knob", path = "UI_Sprites/toggle_knob.png", pixelsPerUnit = 100f, applyNineSlice = false },
                },
                textures = new[]
                {
                    new TextureEntry { key = "ui.bg_main", path = "UI_Sprites/bg_main.png" },
                    new TextureEntry { key = "world.conveyor_belt", path = "World_Sprites/conveyor_belt.png" },
                    new TextureEntry { key = "world.conveyor_slot", path = "World_Sprites/conveyor_slot.png" },
                    new TextureEntry { key = "world.lock_overlay", path = "World_Sprites/lock_overlay.png" },
                    new TextureEntry { key = "world.lock_marker_plate", path = "World_Sprites/lock_marker_plate.png" },
                    new TextureEntry { key = "world.lock_marker_color_disc", path = "World_Sprites/lock_marker_color_disc.png" },
                    new TextureEntry { key = "world.lock_marker_lock_icon", path = "World_Sprites/lock_marker_lock_icon.png" },
                    new TextureEntry { key = "world.completed_overlay", path = "World_Sprites/completed_overlay.png" },
                },
            };
        }

        private static RuntimeLayout BuildRuntimeLayout(ConfigFile cfg, out bool fromConfig)
        {
            fromConfig = cfg != null && cfg.layout != null;
            var layoutFile = cfg != null && cfg.layout != null ? cfg.layout : DefaultConfig().layout;

            float refW = Mathf.Max(1f, layoutFile != null ? layoutFile.referenceWidth : 1080f);
            float refH = Mathf.Max(1f, layoutFile != null ? layoutFile.referenceHeight : 1920f);
            float reservedTop = Mathf.Clamp01(layoutFile != null ? layoutFile.reservedTop : 0.10f);
            float reservedBottom = Mathf.Clamp01(layoutFile != null ? layoutFile.reservedBottom : 0.16f);

            Rect counter = new Rect(50f, 50f, 320f, 110f);
            Rect speed = new Rect(372f, 24f, 150f, 150f);
            Rect settings = new Rect(542f, 24f, 150f, 150f);
            Rect level = new Rect(280f, 24f, 520f, 90f);
            Rect shop = new Rect(24f, 140f, 120f, 120f);
            Rect coins = new Rect(716f, 24f, 340f, 96f);
            Rect lives = new Rect(716f, 132f, 340f, 96f);
            if (layoutFile != null && layoutFile.modules != null)
            {
                counter = ToRect(layoutFile.modules.counter, counter);
                speed = ToRect(layoutFile.modules.speed, speed);
                settings = ToRect(layoutFile.modules.settings, settings);
                level = ToRect(layoutFile.modules.level, level);
                shop = ToRect(layoutFile.modules.shop, shop);
                coins = ToRect(layoutFile.modules.coins, coins);
                lives = ToRect(layoutFile.modules.lives, lives);
            }

            var booster = layoutFile != null && layoutFile.boosters != null ? layoutFile.boosters : null;
            var boosterAnchor = new Vector2(0.5f, 0.08f);
            var boosterOffset = new Vector2(165f, 0f);
            var boosterSize = new Vector2(280f, 280f);
            if (booster != null)
            {
                boosterAnchor = new Vector2(Mathf.Clamp01(booster.anchorX), Mathf.Clamp01(booster.anchorY));
                boosterOffset = new Vector2(booster.offsetX, booster.offsetY);
                boosterSize = new Vector2(Mathf.Max(1f, booster.width), Mathf.Max(1f, booster.height));
            }

            return new RuntimeLayout
            {
                referenceWidth = refW,
                referenceHeight = refH,
                reservedTop = reservedTop,
                reservedBottom = reservedBottom,
                counter = counter,
                speed = speed,
                settings = settings,
                level = level,
                shop = shop,
                coins = coins,
                lives = lives,
                boosterAnchor = boosterAnchor,
                boosterOffset = boosterOffset,
                boosterSize = boosterSize,
            };
        }

        private static Rect ToRect(RectFile f, Rect fallback)
        {
            if (f == null) return fallback;
            return new Rect(f.x, f.y, Mathf.Max(1f, f.w), Mathf.Max(1f, f.h));
        }

        private static Dictionary<string, SpriteEntry> BuildSpriteKeyMap(ConfigFile cfg)
        {
            var map = new Dictionary<string, SpriteEntry>(StringComparer.OrdinalIgnoreCase);
            if (cfg?.sprites == null) return map;
            for (int i = 0; i < cfg.sprites.Length; i++)
            {
                var e = cfg.sprites[i];
                if (e == null) continue;
                if (string.IsNullOrWhiteSpace(e.key) || string.IsNullOrWhiteSpace(e.path)) continue;
                map[e.key.Trim()] = e;
            }
            return map;
        }

        private static Dictionary<string, TextureEntry> BuildTextureKeyMap(ConfigFile cfg)
        {
            var map = new Dictionary<string, TextureEntry>(StringComparer.OrdinalIgnoreCase);
            if (cfg?.textures == null) return map;
            for (int i = 0; i < cfg.textures.Length; i++)
            {
                var e = cfg.textures[i];
                if (e == null) continue;
                if (string.IsNullOrWhiteSpace(e.key) || string.IsNullOrWhiteSpace(e.path)) continue;
                map[e.key.Trim()] = e;
            }
            return map;
        }

        private static NineSliceRule[] CompileNineSliceRules(ConfigFile cfg)
        {
            if (cfg?.nineSliceRules == null || cfg.nineSliceRules.Length == 0)
            {
                return Array.Empty<NineSliceRule>();
            }

            var list = new List<NineSliceRule>(cfg.nineSliceRules.Length);
            for (int i = 0; i < cfg.nineSliceRules.Length; i++)
            {
                var r = cfg.nineSliceRules[i];
                if (r == null) continue;
                if (string.IsNullOrWhiteSpace(r.pattern) || r.border == null || r.border.Length < 4) continue;

                string pattern = r.pattern.Trim();
                string exact = null;
                string prefix = null;
                if (pattern.EndsWith("*", StringComparison.Ordinal))
                {
                    prefix = pattern.Substring(0, pattern.Length - 1);
                }
                else
                {
                    exact = pattern;
                }

                // Config order is [left, right, top, bottom] (manifest style).
                int left = r.border[0];
                int right = r.border[1];
                int top = r.border[2];
                int bottom = r.border[3];
                var unity = new Vector4(left, bottom, right, top);

                list.Add(new NineSliceRule(prefix, exact, unity));
            }

            return list.ToArray();
        }

        private static NineSliceRule[] GetNineSliceRules()
        {
            EnsureConfig();
            return _nineSliceRules ?? Array.Empty<NineSliceRule>();
        }
    }
}
