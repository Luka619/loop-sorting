using UnityEngine;

namespace LoopSorting
{
    [RequireComponent(typeof(Camera))]
    public sealed class BlockOutlinePostEffect : MonoBehaviour
    {
        [SerializeField] private LayerMask blockLayer = 0;
        [SerializeField] private Color outlineColor = Color.black;
        [SerializeField, Range(0.5f, 6f)] private float outlineThickness = 2.0f;
        [SerializeField] private Shader outlineShader;
        [SerializeField] private Shader maskShader;
        [SerializeField, Range(0.25f, 1f)] private float maskResolutionScale = 1f;
        [SerializeField] private bool autoScaleForMobile = true;
        [SerializeField, Range(0.25f, 1f)] private float mobileMaskScale = 0.5f;

        private Camera _camera;
        private Camera _maskCamera;
        private RenderTexture _maskTexture;
        private Material _outlineMaterial;
        private bool _maskReady;
        private float _effectiveMaskScale = 1f;

        private static readonly int MaskTexId = Shader.PropertyToID("_MaskTex");
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineThicknessId = Shader.PropertyToID("_OutlineThickness");
        private const string BlockLayerName = "Block";
        private const int BlockLayerFallback = 10;

        private void OnEnable()
        {
            _camera = GetComponent<Camera>();
            blockLayer = 1 << ResolveBlockLayer();
            if (outlineShader == null) outlineShader = Shader.Find("Hidden/BlockOutline");
            if (maskShader == null) maskShader = Shader.Find("Hidden/BlockMask");
            if (outlineShader != null)
            {
                _outlineMaterial = new Material(outlineShader);
            }
            EnsureMaskCamera();
            _maskReady = false;
        }

        private void OnDisable()
        {
            if (_maskCamera != null)
            {
                _maskCamera.targetTexture = null;
                _maskCamera.ResetReplacementShader();
                SafeDestroy(_maskCamera.gameObject);
                _maskCamera = null;
            }

            if (_maskTexture != null)
            {
                _maskTexture.Release();
                SafeDestroy(_maskTexture);
                _maskTexture = null;
            }

            if (_outlineMaterial != null)
            {
                SafeDestroy(_outlineMaterial);
                _outlineMaterial = null;
            }
            _maskReady = false;
        }

        private void EnsureMaskCamera()
        {
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
            }

            if (_maskCamera == null)
            {
                var go = new GameObject("BlockMaskCamera");
                go.hideFlags = HideFlags.HideAndDontSave;
                go.transform.SetParent(transform, false);
                _maskCamera = go.AddComponent<Camera>();
            }

            _maskCamera.CopyFrom(_camera);
            _maskCamera.enabled = false;
            _maskCamera.clearFlags = CameraClearFlags.SolidColor;
            _maskCamera.backgroundColor = Color.black;
            _maskCamera.cullingMask = blockLayer;
            _maskCamera.allowHDR = false;
            _maskCamera.allowMSAA = false;
            _maskCamera.depthTextureMode = DepthTextureMode.None;

            if (maskShader != null)
            {
                _maskCamera.SetReplacementShader(maskShader, "RenderType");
            }
        }

        private void EnsureMaskTexture(int width, int height)
        {
            if (_maskTexture != null && (_maskTexture.width != width || _maskTexture.height != height))
            {
                _maskTexture.Release();
                SafeDestroy(_maskTexture);
                _maskTexture = null;
            }

            if (_maskTexture == null)
            {
                _maskTexture = CreateMaskTexture(width, height, 16);
            }
            else if (!_maskTexture.IsCreated())
            {
                _maskTexture.Create();
            }
        }

        private void LateUpdate()
        {
            _maskReady = false;
            if (_outlineMaterial == null || outlineShader == null || maskShader == null)
            {
                return;
            }

            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
            }

            if (_camera == null || !_camera.enabled || !SystemInfo.supportsRenderTextures)
            {
                return;
            }

            _effectiveMaskScale = GetEffectiveMaskScale();
            if (outlineColor.a <= 0f || outlineThickness <= 0f)
            {
                return;
            }

            EnsureMaskCamera();
            int width = Mathf.Max(1, Mathf.CeilToInt(_camera.pixelWidth * _effectiveMaskScale));
            int height = Mathf.Max(1, Mathf.CeilToInt(_camera.pixelHeight * _effectiveMaskScale));
            EnsureMaskTexture(width, height);
            if (_maskTexture == null)
            {
                return;
            }

            blockLayer = 1 << ResolveBlockLayer();
            _maskCamera.cullingMask = blockLayer;
            _maskCamera.targetTexture = _maskTexture;
            _maskCamera.Render();
            _maskReady = true;
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (_outlineMaterial == null || outlineShader == null || maskShader == null || !_maskReady || _maskTexture == null)
            {
                Graphics.Blit(src, dest);
                return;
            }

            _outlineMaterial.SetTexture(MaskTexId, _maskTexture);
            _outlineMaterial.SetColor(OutlineColorId, outlineColor);
            _outlineMaterial.SetFloat(OutlineThicknessId, outlineThickness * _effectiveMaskScale);

            Graphics.Blit(src, dest, _outlineMaterial);
        }

        private static int ResolveBlockLayer()
        {
            int layer = LayerMask.NameToLayer(BlockLayerName);
            return layer >= 0 ? layer : BlockLayerFallback;
        }

        private static void SafeDestroy(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying)
            {
                Destroy(obj);
            }
            else
            {
                DestroyImmediate(obj);
            }
        }

        private static RenderTexture CreateMaskTexture(int width, int height, int depth)
        {
            var format = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32)
                ? RenderTextureFormat.ARGB32
                : RenderTextureFormat.Default;
            var texture = new RenderTexture(width, height, depth, format)
            {
                name = "BlockMaskRT",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            if (!texture.Create() && depth > 0)
            {
                texture.Release();
                SafeDestroy(texture);
                texture = new RenderTexture(width, height, 0, format)
                {
                    name = "BlockMaskRT",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
                texture.Create();
            }

            return texture;
        }

        private float GetEffectiveMaskScale()
        {
            float scale = Mathf.Clamp(maskResolutionScale, 0.25f, 1f);
            if (autoScaleForMobile && (Application.isMobilePlatform || Application.platform == RuntimePlatform.WebGLPlayer))
            {
                scale = Mathf.Min(scale, Mathf.Clamp(mobileMaskScale, 0.25f, 1f));
            }
            return scale;
        }
    }
}
