using UnityEngine;
using UnityEngine.UI;

namespace LoopSorting
{
    [DisallowMultipleComponent]
    public sealed class LoopSortingScreenAdapter : MonoBehaviour
    {
        [Header("Scale")]
        public bool applyCanvasScalerMatch = true;
        public Vector2 referenceResolution = new Vector2(1080f, 1920f);
        [Range(0f, 0.5f)]
        public float matchLerpRange = 0.10f;

        [Header("Safe Area")]
        public bool applySafeArea = true;
        public RectTransform safeAreaRect;
        public bool preferWeChatSafeAreaOnWebGL = true;
        [Tooltip("Some platforms (e.g. WeChat mini game) may report a reduced safeArea width in portrait (to avoid the top-right capsule). " +
                 "Keeping horizontal insets off in portrait prevents the whole UI from shifting left; handle top-right buttons separately if needed.")]
        public bool ignoreHorizontalInsetsInPortrait = true;

        [Header("Refs (optional)")]
        public CanvasScaler canvasScaler;

        private int _lastScreenW;
        private int _lastScreenH;
        private Rect _lastSafeArea;
        private float _lastMatch = -1f;
        private Vector4 _rawSafeAreaInsetsPx;
        private Rect _menuButtonRectPx;
        private float _menuButtonRightInsetPx;
        private float _statusBarHeightPx;

        // x=left, y=right, z=top, w=bottom in pixels (screen space).
        public Vector4 RawSafeAreaInsetsPx => _rawSafeAreaInsetsPx;
        // Distance from screen right edge to the left edge of WeChat capsule/menu button (pixels).
        public float MenuButtonRightInsetPx => _menuButtonRightInsetPx;
        // Reported status bar height in Unity screen pixels (best-effort, WeChat WebGL only).
        public float StatusBarHeightPx => _statusBarHeightPx;
        public Rect MenuButtonRectPx => _menuButtonRectPx;

        private void Awake()
        {
            if (canvasScaler == null) canvasScaler = GetComponent<CanvasScaler>();
            Apply(force: true);
        }

        private void OnEnable()
        {
            Apply(force: true);
        }

        private void Update()
        {
            Apply(force: false);
        }

        public void Refresh()
        {
            Apply(force: true);
        }

        private void Apply(bool force)
        {
            int sw = Screen.width;
            int sh = Screen.height;
            if (sw <= 0 || sh <= 0) return;

            Rect safeArea = applySafeArea ? GetBestSafeArea(sw, sh) : new Rect(0, 0, sw, sh);
            _rawSafeAreaInsetsPx = applySafeArea
                ? new Vector4(
                    safeArea.xMin,
                    sw - safeArea.xMax,
                    sh - safeArea.yMax,
                    safeArea.yMin)
                : Vector4.zero;

            if (ignoreHorizontalInsetsInPortrait && sh >= sw)
            {
                safeArea.x = 0f;
                safeArea.width = sw;
            }
            bool screenChanged = sw != _lastScreenW || sh != _lastScreenH;
            bool safeAreaChanged = safeArea != _lastSafeArea;

            if (!force && !screenChanged && !safeAreaChanged)
            {
                return;
            }

            _lastScreenW = sw;
            _lastScreenH = sh;
            _lastSafeArea = safeArea;

            if (Debug.isDebugBuild)
            {
                Debug.Log(
                    $"[LoopSortingScreenAdapter] screen={sw}x{sh}, safeArea={safeArea}, rawInsets(px)={_rawSafeAreaInsetsPx}, " +
                    $"menuRightInset(px)={_menuButtonRightInsetPx:0.#}, statusBar(px)={_statusBarHeightPx:0.#}");
            }

            if (applyCanvasScalerMatch && canvasScaler != null)
            {
                if (canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                }

                if (canvasScaler.referenceResolution != referenceResolution)
                {
                    canvasScaler.referenceResolution = referenceResolution;
                }

                float refAspect = referenceResolution.x / Mathf.Max(0.01f, referenceResolution.y);
                float aspect = sw / (float)sh;
                float minAspect = refAspect * (1f - matchLerpRange);
                float maxAspect = refAspect * (1f + matchLerpRange);
                float match = Mathf.Clamp01(Mathf.InverseLerp(minAspect, maxAspect, aspect));

                if (!Mathf.Approximately(match, _lastMatch) || force)
                {
                    canvasScaler.matchWidthOrHeight = match;
                    _lastMatch = match;
                }
            }

            if (applySafeArea && safeAreaRect != null)
            {
                ApplySafeAreaToRect(safeAreaRect, sw, sh, safeArea);
            }
        }

        private static void ApplySafeAreaToRect(RectTransform target, int screenW, int screenH, Rect safeAreaPx)
        {
            Vector2 anchorMin = new Vector2(
                safeAreaPx.xMin / screenW,
                safeAreaPx.yMin / screenH);
            Vector2 anchorMax = new Vector2(
                safeAreaPx.xMax / screenW,
                safeAreaPx.yMax / screenH);

            if (target.anchorMin != anchorMin) target.anchorMin = anchorMin;
            if (target.anchorMax != anchorMax) target.anchorMax = anchorMax;

            if (target.offsetMin != Vector2.zero) target.offsetMin = Vector2.zero;
            if (target.offsetMax != Vector2.zero) target.offsetMax = Vector2.zero;
        }

        private Rect GetBestSafeArea(int screenW, int screenH)
        {
            Rect safeArea = Screen.safeArea;
            _menuButtonRectPx = default;
            _menuButtonRightInsetPx = 0f;
            _statusBarHeightPx = 0f;

#if (UNITY_WEBGL || WEIXINMINIGAME) && !UNITY_EDITOR
            if (preferWeChatSafeAreaOnWebGL)
            {
                try
                {
                    var windowInfo = WeChatWASM.WX.GetWindowInfo();

                    float pixelRatio = 1f;
                    try
                    {
                        pixelRatio = Mathf.Max(0.01f, (float)windowInfo.pixelRatio);
                    }
                    catch
                    {
                        pixelRatio = 1f;
                    }

                    // Build a few candidate "base coordinate systems" reported by WeChat, then pick the one that yields
                    // the most plausible safeArea/menu button in Unity screen pixels.
                    var baseW = new float[8];
                    var baseH = new float[8];
                    int baseCount = 0;

                    void AddBase(float w, float h)
                    {
                        if (w <= 1f || h <= 1f) return;
                        for (int i = 0; i < baseCount; i++)
                        {
                            if (Mathf.Abs(baseW[i] - w) < 0.01f && Mathf.Abs(baseH[i] - h) < 0.01f) return;
                        }
                        if (baseCount >= baseW.Length) return;
                        baseW[baseCount] = w;
                        baseH[baseCount] = h;
                        baseCount++;
                    }

                    AddBase((float)windowInfo.screenWidth, (float)windowInfo.screenHeight);
                    AddBase((float)windowInfo.windowWidth, (float)windowInfo.windowHeight);
                    AddBase((float)windowInfo.screenWidth * pixelRatio, (float)windowInfo.screenHeight * pixelRatio);
                    AddBase((float)windowInfo.windowWidth * pixelRatio, (float)windowInfo.windowHeight * pixelRatio);
                    AddBase((float)windowInfo.screenWidth / pixelRatio, (float)windowInfo.screenHeight / pixelRatio);
                    AddBase((float)windowInfo.windowWidth / pixelRatio, (float)windowInfo.windowHeight / pixelRatio);

                    bool haveBestSafe = false;
                    Rect bestSafe = default;
                    float bestSafeArea = -1f;
                    float bestSafeScaleY = 1f;

                    bool haveBestMenu = false;
                    Rect bestMenu = default;
                    float bestMenuRightInset = 0f;
                    float bestMenuEdgeGap = float.PositiveInfinity;
                    float bestMenuScaleY = 1f;

                    for (int i = 0; i < baseCount; i++)
                    {
                        float bw = baseW[i];
                        float bh = baseH[i];
                        float scaleX = screenW / bw;
                        float scaleY = screenH / bh;

                        // WeChat safeArea -> Unity px (bottom-left origin).
                        try
                        {
                            var sa = windowInfo.safeArea;
                            if (sa.width > 0 && sa.height > 0)
                            {
                                if (TryGetWeChatSafeArea(screenW, screenH, windowInfo, bw, bh, scaleX, scaleY, out var wxRect) &&
                                    IsPlausibleSafeArea(wxRect, screenW, screenH))
                                {
                                    float area = wxRect.width * wxRect.height;
                                    if (!haveBestSafe || area > bestSafeArea)
                                    {
                                        haveBestSafe = true;
                                        bestSafe = wxRect;
                                        bestSafeArea = area;
                                        bestSafeScaleY = scaleY;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // Best-effort only.
                        }

                        // WeChat capsule/menu button rect (top-right "..." area) -> Unity px.
                        if (TryGetWeChatMenuButtonRect(screenW, screenH, scaleX, scaleY, out var menuRect))
                        {
                            float rightInset = Mathf.Max(0f, screenW - menuRect.xMin);
                            float edgeGap = Mathf.Abs(screenW - menuRect.xMax);

                            bool plausible =
                                rightInset > 0f &&
                                rightInset <= screenW * 0.9f &&
                                menuRect.width > 1f &&
                                menuRect.height > 1f &&
                                menuRect.width <= screenW * 0.35f &&
                                menuRect.height <= screenH * 0.25f &&
                                menuRect.xMax >= screenW * 0.7f;

                            if (plausible && (!haveBestMenu || edgeGap < bestMenuEdgeGap))
                            {
                                haveBestMenu = true;
                                bestMenu = menuRect;
                                bestMenuRightInset = rightInset;
                                bestMenuEdgeGap = edgeGap;
                                bestMenuScaleY = scaleY;
                            }
                        }
                    }

                    if (haveBestSafe)
                    {
                        safeArea = bestSafe;
                    }

                    if (haveBestMenu)
                    {
                        _menuButtonRectPx = bestMenu;
                        // Sanity clamp: if this is huge, we likely picked the wrong scale base; ignore to avoid shifting UI to the left.
                        if (bestMenuRightInset <= screenW * 0.9f)
                        {
                            _menuButtonRightInsetPx = bestMenuRightInset;
                        }
                    }

                    // Best-effort: status bar height (top-left origin units).
                    float statusScaleY = haveBestSafe ? bestSafeScaleY : (haveBestMenu ? bestMenuScaleY : 1f);
                    try
                    {
                        _statusBarHeightPx = Mathf.Max(0f, (float)windowInfo.statusBarHeight) * statusScaleY;
                        _statusBarHeightPx = Mathf.Clamp(_statusBarHeightPx, 0f, screenH * 0.25f);
                    }
                    catch
                    {
                        _statusBarHeightPx = 0f;
                    }
                }
                catch
                {
                    // Best-effort only.
                }
            }
#endif

            if (safeArea.width <= 0 || safeArea.height <= 0)
            {
                safeArea = new Rect(0, 0, screenW, screenH);
            }

            safeArea.x = Mathf.Clamp(safeArea.x, 0, screenW);
            safeArea.y = Mathf.Clamp(safeArea.y, 0, screenH);
            safeArea.width = Mathf.Clamp(safeArea.width, 0, screenW - safeArea.x);
            safeArea.height = Mathf.Clamp(safeArea.height, 0, screenH - safeArea.y);
            return safeArea;
        }

#if (UNITY_WEBGL || WEIXINMINIGAME) && !UNITY_EDITOR
        private static bool TryGetWeChatScale(
            int screenW,
            int screenH,
            WeChatWASM.WindowInfo windowInfo,
            out float scaleX,
            out float scaleY,
            out float baseW,
            out float baseH)
        {
            scaleX = 1f;
            scaleY = 1f;
            baseW = 0f;
            baseH = 0f;

            float pixelRatio = 1f;
            try
            {
                // Some SDKs expose it as a number; keep it best-effort.
                pixelRatio = Mathf.Max(0.01f, (float)windowInfo.pixelRatio);
            }
            catch
            {
                pixelRatio = 1f;
            }

            // Pick the base dimensions that best match Unity's Screen size.
            // Depending on platform, WeChat may report logical pixels (windowWidth) or physical pixels (screenWidth),
            // and Unity WebGL may use devicePixelRatio-scaled pixels.
            float bestW = 0f, bestH = 0f;
            float bestErr = float.PositiveInfinity;
            float targetAspect = screenW / Mathf.Max(1f, screenH);

            void Consider(float w, float h)
            {
                if (w <= 1f || h <= 1f) return;
                float aspect = w / h;
                float aspectErr = Mathf.Abs(aspect - targetAspect) * 10000f;
                float sizeErr = Mathf.Abs(w - screenW) + Mathf.Abs(h - screenH);
                float err = sizeErr + aspectErr;
                if (err < bestErr)
                {
                    bestErr = err;
                    bestW = w;
                    bestH = h;
                }
            }

            Consider((float)windowInfo.screenWidth, (float)windowInfo.screenHeight);
            Consider((float)windowInfo.windowWidth, (float)windowInfo.windowHeight);
            Consider((float)windowInfo.screenWidth * pixelRatio, (float)windowInfo.screenHeight * pixelRatio);
            Consider((float)windowInfo.windowWidth * pixelRatio, (float)windowInfo.windowHeight * pixelRatio);

            if (bestW <= 1f || bestH <= 1f) return false;

            baseW = bestW;
            baseH = bestH;
            scaleX = screenW / bestW;
            scaleY = screenH / bestH;
            return true;
        }

        private static bool TryGetWeChatSafeArea(
            int screenW,
            int screenH,
            WeChatWASM.WindowInfo windowInfo,
            float baseW,
            float baseH,
            float scaleX,
            float scaleY,
            out Rect safeAreaPx)
        {
            safeAreaPx = default;

            var sa = windowInfo.safeArea;
            if (sa.width <= 0 || sa.height <= 0) return false;

            float left = (float)sa.left;
            float top = (float)sa.top;
            float width = (float)sa.width;
            float height = (float)sa.height;

            float rightRaw = (float)sa.right;
            float bottomRaw = (float)sa.bottom;

            // Interpret right/bottom as absolute coordinates if that matches (right-left) / (bottom-top) ~ width/height,
            // otherwise interpret them as insets (distance from the edge).
            float rightCoord = rightRaw > 0 ? rightRaw : left + width;
            float rightInsetCoord = baseW - Mathf.Max(0f, rightRaw);
            float coordWidth = rightCoord - left;
            float insetWidth = rightInsetCoord - left;
            float xMax = Mathf.Abs(coordWidth - width) <= Mathf.Abs(insetWidth - width) ? rightCoord : rightInsetCoord;

            float bottomCoord = bottomRaw > 0 ? bottomRaw : top + height;
            float bottomInsetCoord = baseH - Mathf.Max(0f, bottomRaw);
            float coordHeight = bottomCoord - top;
            float insetHeight = bottomInsetCoord - top;
            float yBottomFromTop = Mathf.Abs(coordHeight - height) <= Mathf.Abs(insetHeight - height) ? bottomCoord : bottomInsetCoord;

            float xMinPx = left * scaleX;
            float xMaxPx = xMax * scaleX;
            float yMinPx = (screenH - yBottomFromTop * scaleY);
            float yMaxPx = (screenH - top * scaleY);

            float wPx = xMaxPx - xMinPx;
            float hPx = yMaxPx - yMinPx;
            if (wPx <= 1f || hPx <= 1f) return false;

            safeAreaPx = new Rect(xMinPx, yMinPx, wPx, hPx);
            return true;
        }

        private static bool TryGetWeChatMenuButtonRect(int screenW, int screenH, float scaleX, float scaleY, out Rect menuRectPx)
        {
            menuRectPx = default;
            try
            {
                var mb = WeChatWASM.WX.GetMenuButtonBoundingClientRect();
                if (mb.width <= 0 || mb.height <= 0) return false;

                float left = (float)mb.left * scaleX;
                float top = (float)mb.top * scaleY;
                float width = (float)mb.width * scaleX;
                float height = (float)mb.height * scaleY;
                if (width <= 1f || height <= 1f) return false;

                // mb is in top-left origin units; convert to Unity bottom-left pixels.
                float y = screenH - (top + height);
                menuRectPx = new Rect(left, y, width, height);
                return menuRectPx.width > 1f && menuRectPx.height > 1f;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsPlausibleSafeArea(Rect r, int screenW, int screenH)
        {
            if (r.width <= 0 || r.height <= 0) return false;
            if (r.width < screenW * 0.6f) return false;
            if (r.height < screenH * 0.6f) return false;

            float top = screenH - r.yMax;
            float bottom = r.yMin;
            if (top > screenH * 0.35f) return false;
            if (bottom > screenH * 0.35f) return false;

            return true;
        }
#endif
    }
}
