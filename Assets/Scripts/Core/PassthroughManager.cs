using System;
using UnityEngine;
using MusicOM.Infrastructure.Logging;

namespace MusicOM.Core
{
    public class PassthroughManager : MonoBehaviour
    {
        [Header("Passthrough Settings")]
        [SerializeField] private bool _startInPassthrough = false;
        [SerializeField] [Range(0f, 1f)] private float _defaultOpacity = 1f;

        [Header("Color Correction")]
        [SerializeField] [Range(-1f, 1f)] private float _brightness = 0f;
        [SerializeField] [Range(-1f, 1f)] private float _contrast = 0f;
        [SerializeField] [Range(-1f, 1f)] private float _saturation = 0f;

        [Header("Edge Rendering")]
        [SerializeField] private bool _edgeRenderingEnabled = false;
        [SerializeField] private Color _edgeColor = Color.white;

        private OVRPassthroughLayer _passthroughLayer;
        private IAppLogger _logger;
        private AppConfig _appConfig;
        private float _currentOpacity;
        private bool _isInitialized;

        public event Action<bool> OnPassthroughToggled;
        public event Action<float> OnOpacityChanged;

        public bool IsPassthroughActive => _currentOpacity > 0f;
        public bool IsMixedRealityMode => _currentOpacity > 0f && _currentOpacity < 1f;
        public float Opacity => _currentOpacity;
        public bool IsInitialized => _isInitialized;

        private void Start()
        {
            _logger = ServiceLocator.Get<IAppLogger>();
            _appConfig = ServiceLocator.Get<AppConfig>();

            Initialize();
        }

        private void Initialize()
        {
            if (!_appConfig.passthroughEnabled)
            {
                _logger?.Log("[PassthroughManager] Passthrough disabled in config");
                return;
            }

            // Check if passthrough is initialized at system level
            if (!OVRManager.IsInsightPassthroughInitialized())
            {
                _logger?.LogWarning("[PassthroughManager] Insight Passthrough not initialized. Enable in OculusProjectConfig.");
                return;
            }

            // Find existing passthrough layer or create one
            _passthroughLayer = FindFirstObjectByType<OVRPassthroughLayer>();

            if (_passthroughLayer == null)
            {
                var layerGO = new GameObject("OVRPassthroughLayer");
                layerGO.transform.SetParent(transform);
                _passthroughLayer = layerGO.AddComponent<OVRPassthroughLayer>();
                _logger?.Log("[PassthroughManager] Created OVRPassthroughLayer");
            }

            // Apply initial settings
            _currentOpacity = _startInPassthrough ? _defaultOpacity : 0f;
            ApplyOpacity();
            ApplyColorCorrection();
            ApplyEdgeRendering();

            _isInitialized = true;
            ServiceLocator.Register(this);

            _logger?.Log($"[PassthroughManager] Initialized (active: {IsPassthroughActive})");
        }

        public void EnablePassthrough()
        {
            SetOpacity(_defaultOpacity);
        }

        public void DisablePassthrough()
        {
            SetOpacity(0f);
        }

        public void TogglePassthrough()
        {
            if (IsPassthroughActive)
            {
                DisablePassthrough();
            }
            else
            {
                EnablePassthrough();
            }
        }

        public void SetOpacity(float opacity)
        {
            if (!_isInitialized || _passthroughLayer == null) return;

            var previouslyActive = IsPassthroughActive;
            _currentOpacity = Mathf.Clamp01(opacity);

            ApplyOpacity();

            OnOpacityChanged?.Invoke(_currentOpacity);

            if (previouslyActive != IsPassthroughActive)
            {
                OnPassthroughToggled?.Invoke(IsPassthroughActive);
                _logger?.Log($"[PassthroughManager] Passthrough {(IsPassthroughActive ? "enabled" : "disabled")}");
            }
        }

        private void ApplyOpacity()
        {
            if (_passthroughLayer == null) return;

            _passthroughLayer.textureOpacity = _currentOpacity;
            _passthroughLayer.hidden = _currentOpacity <= 0f;
        }

        public void SetColorCorrection(float brightness, float contrast, float saturation)
        {
            _brightness = Mathf.Clamp(brightness, -1f, 1f);
            _contrast = Mathf.Clamp(contrast, -1f, 1f);
            _saturation = Mathf.Clamp(saturation, -1f, 1f);

            ApplyColorCorrection();
        }

        private void ApplyColorCorrection()
        {
            if (_passthroughLayer == null) return;

            _passthroughLayer.SetBrightnessContrastSaturation(_brightness, _contrast, _saturation);
        }

        public void SetEdgeRendering(bool enabled, Color? color = null)
        {
            _edgeRenderingEnabled = enabled;
            if (color.HasValue)
            {
                _edgeColor = color.Value;
            }

            ApplyEdgeRendering();
        }

        private void ApplyEdgeRendering()
        {
            if (_passthroughLayer == null) return;

            _passthroughLayer.edgeRenderingEnabled = _edgeRenderingEnabled;
            if (_edgeRenderingEnabled)
            {
                _passthroughLayer.edgeColor = _edgeColor;
            }
        }

        public void ResetColorCorrection()
        {
            SetColorCorrection(0f, 0f, 0f);
        }

        // Fade passthrough in/out over time
        public void FadePassthrough(float targetOpacity, float duration)
        {
            StartCoroutine(FadeCoroutine(targetOpacity, duration));
        }

        private System.Collections.IEnumerator FadeCoroutine(float targetOpacity, float duration)
        {
            var startOpacity = _currentOpacity;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                SetOpacity(Mathf.Lerp(startOpacity, targetOpacity, t));
                yield return null;
            }

            SetOpacity(targetOpacity);
        }

        private void OnDestroy()
        {
            if (ServiceLocator.IsRegistered<PassthroughManager>())
            {
                ServiceLocator.Unregister<PassthroughManager>();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Apply changes in editor when values change
            if (_passthroughLayer != null && Application.isPlaying)
            {
                ApplyColorCorrection();
                ApplyEdgeRendering();
            }
        }
#endif
    }
}
