using UnityEngine;
using MusicOM.Infrastructure.Logging;
using MusicOM.Input;
using MusicOM.Audio;

namespace MusicOM.Core
{
    public class XRSetup : MonoBehaviour
    {
        [Header("XR References")]
        [SerializeField] private Transform _cameraOffset;
        [SerializeField] private Camera _mainCamera;

        [Header("Hand Tracking")]
        [SerializeField] private GameObject _leftHandPrefab;
        [SerializeField] private GameObject _rightHandPrefab;
        [SerializeField] private Transform _leftHandAnchor;
        [SerializeField] private Transform _rightHandAnchor;

        [Header("Controllers")]
        [SerializeField] private GameObject _leftControllerPrefab;
        [SerializeField] private GameObject _rightControllerPrefab;

        [Header("Audio")]
        [SerializeField] private AudioManager _audioManager;

        [Header("Haptics")]
        [SerializeField] private HapticManager _hapticManager;

        [Header("Passthrough")]
        [SerializeField] private PassthroughManager _passthroughManager;

        private IAppLogger _logger;
        private AppConfig _appConfig;
        private InputManager _inputManager;
        private HandTrackingController _leftHand;
        private HandTrackingController _rightHand;
        private OVRCameraRig _cameraRig;

        private void Start()
        {
            _logger = ServiceLocator.Get<IAppLogger>();
            _appConfig = ServiceLocator.Get<AppConfig>();

            _logger?.Log("[XRSetup] Initializing...");

            SetupCamera();
            SetupHandTracking();
            SetupAudio();
            SetupHaptics();
            SetupPassthrough();
            SetupInputManager();

            _logger?.Log("[XRSetup] Initialization complete");
        }

        private void SetupCamera()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            if (_mainCamera != null)
            {
                // Ensure AudioListener is on main camera
                if (_mainCamera.GetComponent<AudioListener>() == null)
                {
                    _mainCamera.gameObject.AddComponent<AudioListener>();
                }
            }

            // Find or setup OVRCameraRig
            _cameraRig = FindFirstObjectByType<OVRCameraRig>();
            if (_cameraRig != null)
            {
                _leftHandAnchor = _cameraRig.leftHandAnchor;
                _rightHandAnchor = _cameraRig.rightHandAnchor;
                _logger?.Log("[XRSetup] OVRCameraRig found");
            }
        }

        private void SetupHandTracking()
        {
            if (_appConfig != null && !_appConfig.handTrackingEnabled)
            {
                _logger?.Log("[XRSetup] Hand tracking disabled in config");
                return;
            }

            // Setup left hand
            if (_leftHandAnchor != null)
            {
                _leftHand = SetupHand(_leftHandAnchor, HandSide.Left);
            }

            // Setup right hand
            if (_rightHandAnchor != null)
            {
                _rightHand = SetupHand(_rightHandAnchor, HandSide.Right);
            }

            _logger?.Log("[XRSetup] Hand tracking configured");
        }

        private HandTrackingController SetupHand(Transform anchor, HandSide side)
        {
            // Check if OVRHand already exists on anchor
            var existingHand = anchor.GetComponentInChildren<OVRHand>();
            var existingSkeleton = anchor.GetComponentInChildren<OVRSkeleton>();

            GameObject handObject;

            if (existingHand != null)
            {
                handObject = existingHand.gameObject;
            }
            else
            {
                // Create hand tracking object
                handObject = new GameObject($"{side}Hand_Tracking");
                handObject.transform.SetParent(anchor);
                handObject.transform.localPosition = Vector3.zero;
                handObject.transform.localRotation = Quaternion.identity;

                existingHand = handObject.AddComponent<OVRHand>();
                existingSkeleton = handObject.AddComponent<OVRSkeleton>();

                // Configure OVRHand
                var handType = side == HandSide.Left ? OVRHand.Hand.HandLeft : OVRHand.Hand.HandRight;
                SetOVRHandType(existingHand, handType);

                // Configure OVRSkeleton
                var skeletonType = side == HandSide.Left ?
                    OVRSkeleton.SkeletonType.HandLeft : OVRSkeleton.SkeletonType.HandRight;
                SetOVRSkeletonType(existingSkeleton, skeletonType);
            }

            // Add our wrapper
            var controller = handObject.GetComponent<HandTrackingController>();
            if (controller == null)
            {
                controller = handObject.AddComponent<HandTrackingController>();
            }

            controller.SetReferences(existingHand, existingSkeleton);

            return controller;
        }

        private void SetOVRHandType(OVRHand hand, OVRHand.Hand handType)
        {
            var field = typeof(OVRHand).GetField("HandType",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(hand, handType);
            }
        }

        private void SetOVRSkeletonType(OVRSkeleton skeleton, OVRSkeleton.SkeletonType skeletonType)
        {
            var field = typeof(OVRSkeleton).GetField("_skeletonType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(skeleton, skeletonType);
            }
        }

        private void SetupAudio()
        {
            if (_audioManager == null)
            {
                _audioManager = FindFirstObjectByType<AudioManager>();
            }

            if (_audioManager == null)
            {
                var audioGO = new GameObject("AudioManager");
                audioGO.transform.SetParent(transform);
                _audioManager = audioGO.AddComponent<AudioManager>();
            }

            _logger?.Log("[XRSetup] Audio configured");
        }

        private void SetupHaptics()
        {
            if (_hapticManager == null)
            {
                _hapticManager = FindFirstObjectByType<HapticManager>();
            }

            if (_hapticManager == null)
            {
                var hapticGO = new GameObject("HapticManager");
                hapticGO.transform.SetParent(transform);
                _hapticManager = hapticGO.AddComponent<HapticManager>();
            }

            _logger?.Log("[XRSetup] Haptics configured");
        }

        private void SetupPassthrough()
        {
            if (_appConfig != null && !_appConfig.passthroughEnabled)
            {
                _logger?.Log("[XRSetup] Passthrough disabled in config");
                return;
            }

            if (_passthroughManager == null)
            {
                _passthroughManager = FindFirstObjectByType<PassthroughManager>();
            }

            if (_passthroughManager == null)
            {
                var passthroughGO = new GameObject("PassthroughManager");
                passthroughGO.transform.SetParent(transform);
                _passthroughManager = passthroughGO.AddComponent<PassthroughManager>();
            }

            _logger?.Log("[XRSetup] Passthrough configured");
        }

        private void SetupInputManager()
        {
            _inputManager = FindFirstObjectByType<InputManager>();

            if (_inputManager == null)
            {
                var inputGO = new GameObject("InputManager");
                inputGO.transform.SetParent(transform);
                _inputManager = inputGO.AddComponent<InputManager>();
            }

            // Link hand controllers via reflection or serialized field
            SetInputManagerHands(_inputManager, _leftHand, _rightHand);

            // Check for connected controllers
            _inputManager.SetControllersConnected(OVRInput.IsControllerConnected(OVRInput.Controller.Touch));

            _logger?.Log("[XRSetup] Input manager configured");
        }

        private void SetInputManagerHands(InputManager manager, HandTrackingController left, HandTrackingController right)
        {
            var leftField = typeof(InputManager).GetField("_leftHand",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var rightField = typeof(InputManager).GetField("_rightHand",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            leftField?.SetValue(manager, left);
            rightField?.SetValue(manager, right);
        }

        private void Update()
        {
            // Update controller connection status
            if (_inputManager != null)
            {
                var controllersConnected = OVRInput.IsControllerConnected(OVRInput.Controller.Touch);
                _inputManager.SetControllersConnected(controllersConnected);
            }
        }

        public Camera MainCamera => _mainCamera;
        public HandTrackingController LeftHand => _leftHand;
        public HandTrackingController RightHand => _rightHand;
        public InputManager InputManager => _inputManager;
        public AudioManager AudioManager => _audioManager;
        public HapticManager HapticManager => _hapticManager;
        public PassthroughManager PassthroughManager => _passthroughManager;
    }
}
