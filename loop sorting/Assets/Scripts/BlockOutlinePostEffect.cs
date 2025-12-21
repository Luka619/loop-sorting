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

        private Camera _camera;
        private Camera _maskCamera;
        private RenderTexture _maskTexture;
        private Material _outlineMaterial;

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
                _maskTexture = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32)
                {
                    name = "BlockMaskRT",
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
            }
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (_outlineMaterial == null || outlineShader == null || maskShader == null)
            {
                Graphics.Blit(src, dest);
                return;
            }

            EnsureMaskCamera();
            EnsureMaskTexture(src.width, src.height);

            blockLayer = 1 << ResolveBlockLayer();
            _maskCamera.cullingMask = blockLayer;
            _maskCamera.targetTexture = _maskTexture;
            _maskCamera.Render();

            _outlineMaterial.SetTexture(MaskTexId, _maskTexture);
            _outlineMaterial.SetColor(OutlineColorId, outlineColor);
            _outlineMaterial.SetFloat(OutlineThicknessId, outlineThickness);

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
    }
}
