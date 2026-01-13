using UnityEngine;
using Oculus.Haptics;
using MusicOM.Core;
using MusicOM.Infrastructure.Logging;

namespace MusicOM.Input
{
    public class HapticManager : MonoBehaviour
    {
        [Header("Custom Haptic Clips")]
        [SerializeField] private HapticClip _errorHaptic;
        [SerializeField] private HapticClip _minorActionHaptic;
        [SerializeField] private HapticClip _majorActionHaptic;

        private IAppLogger _logger;
        private HapticClipPlayer _errorPlayer;
        private HapticClipPlayer _minorActionPlayer;
        private HapticClipPlayer _majorActionPlayer;

        public static HapticManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            _logger = ServiceLocator.Get<IAppLogger>();
            InitializePlayers();
            ServiceLocator.Register(this);

            _logger?.Log("[HapticManager] Initialized");
        }

        private void InitializePlayers()
        {
            if (_errorHaptic != null)
                _errorPlayer = new HapticClipPlayer(_errorHaptic);

            if (_minorActionHaptic != null)
                _minorActionPlayer = new HapticClipPlayer(_minorActionHaptic);

            if (_majorActionHaptic != null)
                _majorActionPlayer = new HapticClipPlayer(_majorActionHaptic);
        }

        public void PlayError(Controller controller = Controller.Both)
        {
            _errorPlayer?.Play(controller);
            _logger?.Log($"[HapticManager] Error haptic on {controller}");
        }

        public void PlayMinorAction(Controller controller = Controller.Both)
        {
            _minorActionPlayer?.Play(controller);
        }

        public void PlayMajorAction(Controller controller = Controller.Both)
        {
            _majorActionPlayer?.Play(controller);
        }

        public void PlayMinorAction(HandSide side)
        {
            var controller = side == HandSide.Left ? Controller.Left : Controller.Right;
            PlayMinorAction(controller);
        }

        public void PlayMajorAction(HandSide side)
        {
            var controller = side == HandSide.Left ? Controller.Left : Controller.Right;
            PlayMajorAction(controller);
        }

        public void PlayError(HandSide side)
        {
            var controller = side == HandSide.Left ? Controller.Left : Controller.Right;
            PlayError(controller);
        }

        public void StopAll()
        {
            _errorPlayer?.Stop();
            _minorActionPlayer?.Stop();
            _majorActionPlayer?.Stop();
        }

        private void OnDestroy()
        {
            _errorPlayer?.Dispose();
            _minorActionPlayer?.Dispose();
            _majorActionPlayer?.Dispose();

            if (ServiceLocator.IsRegistered<HapticManager>())
            {
                ServiceLocator.Unregister<HapticManager>();
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
