using System;
using UnityEngine;
using MusicOM.Core;
using MusicOM.Infrastructure.Logging;

namespace MusicOM.Input
{
    public enum InputType
    {
        None,
        HandTracking,
        Controller
    }

    public enum HandSide
    {
        Left,
        Right
    }

    public class InputManager : MonoBehaviour
    {
        [SerializeField] private HandTrackingController _leftHand;
        [SerializeField] private HandTrackingController _rightHand;

        private IAppLogger _logger;
        private InputType _activeInputType = InputType.None;
        private bool _controllersConnected;

        public event Action<HandSide> OnPinchStart;
        public event Action<HandSide> OnPinchEnd;
        public event Action<HandSide> OnGripStart;
        public event Action<HandSide> OnGripEnd;
        public event Action<HandSide, bool> OnPrimaryButton;
        public event Action<HandSide, bool> OnSecondaryButton;
        public event Action<HandSide, float> OnTrigger;
        public event Action<InputType> OnInputTypeChanged;

        public InputType ActiveInputType => _activeInputType;
        public bool IsHandTrackingActive => _activeInputType == InputType.HandTracking;
        public HandTrackingController LeftHand => _leftHand;
        public HandTrackingController RightHand => _rightHand;

        private void Start()
        {
            _logger = ServiceLocator.Get<IAppLogger>();
            ServiceLocator.Register(this);

            _logger?.Log("[InputManager] Initialized");
        }

        private void Update()
        {
            UpdateInputType();
            ProcessHandInput();
            ProcessControllerInput();
        }

        private void UpdateInputType()
        {
            var handsTracked = (_leftHand != null && _leftHand.IsTracked) ||
                               (_rightHand != null && _rightHand.IsTracked);

            var newInputType = handsTracked ? InputType.HandTracking :
                               _controllersConnected ? InputType.Controller :
                               InputType.None;

            if (newInputType != _activeInputType)
            {
                _activeInputType = newInputType;
                _logger?.Log($"[InputManager] Input type changed: {_activeInputType}");
                OnInputTypeChanged?.Invoke(_activeInputType);
            }
        }

        private void ProcessHandInput()
        {
            if (_activeInputType != InputType.HandTracking) return;

            ProcessHandPinch(_leftHand, HandSide.Left);
            ProcessHandPinch(_rightHand, HandSide.Right);
        }

        private void ProcessHandPinch(HandTrackingController hand, HandSide side)
        {
            if (hand == null) return;

            if (hand.IsPinchStarted)
            {
                OnPinchStart?.Invoke(side);
            }
            else if (hand.IsPinchEnded)
            {
                OnPinchEnd?.Invoke(side);
            }
        }

        private void ProcessControllerInput()
        {
            if (_activeInputType != InputType.Controller) return;

            // Controller input processing via OVRInput
            ProcessControllerButtons(OVRInput.Controller.LTouch, HandSide.Left);
            ProcessControllerButtons(OVRInput.Controller.RTouch, HandSide.Right);
        }

        private void ProcessControllerButtons(OVRInput.Controller controller, HandSide side)
        {
            // Primary button (A on right, X on left)
            if (OVRInput.GetDown(OVRInput.Button.One, controller))
                OnPrimaryButton?.Invoke(side, true);
            if (OVRInput.GetUp(OVRInput.Button.One, controller))
                OnPrimaryButton?.Invoke(side, false);

            // Secondary button (B on right, Y on left)
            if (OVRInput.GetDown(OVRInput.Button.Two, controller))
                OnSecondaryButton?.Invoke(side, true);
            if (OVRInput.GetUp(OVRInput.Button.Two, controller))
                OnSecondaryButton?.Invoke(side, false);

            // Trigger
            var triggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller);
            if (triggerValue > 0.1f)
                OnTrigger?.Invoke(side, triggerValue);

            // Grip as pinch equivalent
            if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, controller))
                OnPinchStart?.Invoke(side);
            if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger, controller))
                OnPinchEnd?.Invoke(side);
        }

        public Vector3 GetHandPosition(HandSide side)
        {
            var hand = side == HandSide.Left ? _leftHand : _rightHand;
            if (hand != null && hand.IsTracked)
            {
                return hand.GetPalmPosition();
            }

            // Fallback to controller position
            var controller = side == HandSide.Left ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
            return OVRInput.GetLocalControllerPosition(controller);
        }

        public Quaternion GetHandRotation(HandSide side)
        {
            var hand = side == HandSide.Left ? _leftHand : _rightHand;
            if (hand != null && hand.IsTracked)
            {
                return hand.GetPalmRotation();
            }

            var controller = side == HandSide.Left ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
            return OVRInput.GetLocalControllerRotation(controller);
        }

        public Ray GetPointerRay(HandSide side)
        {
            var position = GetHandPosition(side);
            var rotation = GetHandRotation(side);
            return new Ray(position, rotation * Vector3.forward);
        }

        public void Vibrate(HandSide side, float amplitude = 0.5f, float duration = 0.1f)
        {
            var controller = side == HandSide.Left ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
            OVRInput.SetControllerVibration(amplitude, amplitude, controller);

            // Auto-stop vibration after duration
            StartCoroutine(StopVibrationAfter(controller, duration));
        }

        private System.Collections.IEnumerator StopVibrationAfter(OVRInput.Controller controller, float duration)
        {
            yield return new WaitForSeconds(duration);
            OVRInput.SetControllerVibration(0, 0, controller);
        }

        public void SetControllersConnected(bool connected)
        {
            _controllersConnected = connected;
        }

        private void OnDestroy()
        {
            if (ServiceLocator.IsRegistered<InputManager>())
            {
                ServiceLocator.Unregister<InputManager>();
            }
        }
    }
}
