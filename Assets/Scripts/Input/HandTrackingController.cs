using UnityEngine;

namespace MusicOM.Input
{
    public class HandTrackingController : MonoBehaviour
    {
        [SerializeField] private OVRHand _ovrHand;
        [SerializeField] private OVRSkeleton _ovrSkeleton;
        [SerializeField] private HandSide _handSide;

        [Header("Pinch Settings")]
        [SerializeField] private float _pinchThreshold = 0.7f;
        [SerializeField] private float _pinchReleaseThreshold = 0.3f;

        private bool _isPinching;
        private bool _wasPinching;
        private float _pinchStrength;

        public bool IsTracked => _ovrHand != null && _ovrHand.IsTracked;
        public bool IsHighConfidence => _ovrHand != null && _ovrHand.IsTracked &&
                                        _ovrHand.HandConfidence == OVRHand.TrackingConfidence.High;
        public float PinchStrength => _pinchStrength;
        public bool IsPinching => _isPinching;
        public bool IsPinchStarted => _isPinching && !_wasPinching;
        public bool IsPinchEnded => !_isPinching && _wasPinching;
        public HandSide Side => _handSide;
        public OVRHand OVRHand => _ovrHand;
        public OVRSkeleton OVRSkeleton => _ovrSkeleton;

        private void Update()
        {
            if (!IsTracked) return;

            _wasPinching = _isPinching;
            UpdatePinchState();
        }

        private void UpdatePinchState()
        {
            _pinchStrength = _ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);

            if (!_isPinching && _pinchStrength >= _pinchThreshold)
            {
                _isPinching = true;
            }
            else if (_isPinching && _pinchStrength <= _pinchReleaseThreshold)
            {
                _isPinching = false;
            }
        }

        public Vector3 GetPalmPosition()
        {
            if (_ovrSkeleton == null || !IsTracked) return transform.position;

            var wristBone = GetBone(OVRSkeleton.BoneId.Hand_WristRoot);
            return wristBone != null ? wristBone.position : transform.position;
        }

        public Quaternion GetPalmRotation()
        {
            if (_ovrSkeleton == null || !IsTracked) return transform.rotation;

            var wristBone = GetBone(OVRSkeleton.BoneId.Hand_WristRoot);
            return wristBone != null ? wristBone.rotation : transform.rotation;
        }

        public Vector3 GetFingerTipPosition(OVRHand.HandFinger finger)
        {
            if (_ovrSkeleton == null || !IsTracked) return transform.position;

            var boneId = GetFingerTipBoneId(finger);
            var bone = GetBone(boneId);
            return bone != null ? bone.position : transform.position;
        }

        public Vector3 GetIndexFingerTip()
        {
            return GetFingerTipPosition(OVRHand.HandFinger.Index);
        }

        public Vector3 GetThumbTip()
        {
            return GetFingerTipPosition(OVRHand.HandFinger.Thumb);
        }

        public Vector3 GetPinchPosition()
        {
            var indexTip = GetIndexFingerTip();
            var thumbTip = GetThumbTip();
            return Vector3.Lerp(indexTip, thumbTip, 0.5f);
        }

        public Ray GetPointerRay()
        {
            var position = GetIndexFingerTip();
            var direction = (GetIndexFingerTip() - GetPalmPosition()).normalized;
            return new Ray(position, direction);
        }

        private Transform GetBone(OVRSkeleton.BoneId boneId)
        {
            if (_ovrSkeleton == null || _ovrSkeleton.Bones == null) return null;

            foreach (var bone in _ovrSkeleton.Bones)
            {
                if (bone.Id == boneId)
                {
                    return bone.Transform;
                }
            }
            return null;
        }

        private OVRSkeleton.BoneId GetFingerTipBoneId(OVRHand.HandFinger finger)
        {
            return finger switch
            {
                OVRHand.HandFinger.Thumb => OVRSkeleton.BoneId.Hand_ThumbTip,
                OVRHand.HandFinger.Index => OVRSkeleton.BoneId.Hand_IndexTip,
                OVRHand.HandFinger.Middle => OVRSkeleton.BoneId.Hand_MiddleTip,
                OVRHand.HandFinger.Ring => OVRSkeleton.BoneId.Hand_RingTip,
                OVRHand.HandFinger.Pinky => OVRSkeleton.BoneId.Hand_PinkyTip,
                _ => OVRSkeleton.BoneId.Hand_IndexTip
            };
        }

        public void SetReferences(OVRHand hand, OVRSkeleton skeleton)
        {
            _ovrHand = hand;
            _ovrSkeleton = skeleton;
        }
    }
}
