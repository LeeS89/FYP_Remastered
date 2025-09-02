using Meta.XR.Util;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using System;
using UnityEngine;
using static OVRPlugin;
using static OVRSkeleton;

namespace Oculus.Interaction.Input
{


    public class FromOVRHandDataSource_NoRadius : FromOVRHandDataSource
    {
        private float[] _cachedRadii;
        private float[] _radiusByBoneIndex;
        private bool _radiiReady;
        private Handedness _cachedHandedness;

        /*protected override void UpdateDataPoses(SkeletonPoseData poseData)
        {
            EnsureRadiiCached();

            _handDataAsset.HandScale = poseData.RootScale;
            _handDataAsset.IsTracked = _ovrHand.IsTracked;
            _handDataAsset.IsHighConfidence = poseData.IsDataHighConfidence;
            _handDataAsset.IsDominantHand = _ovrHand.IsDominantHand;
            _handDataAsset.RootPoseOrigin = _handDataAsset.IsTracked ? PoseOrigin.RawTrackedPose : PoseOrigin.None;

            for (var fingerIdx = 0; fingerIdx < Constants.NUM_FINGERS; fingerIdx++)
            {
                var ovrFingerIdx = (OVRHand.HandFinger)fingerIdx;
                _handDataAsset.IsFingerPinching[fingerIdx] =
                    _ovrHand.GetFingerIsPinching(ovrFingerIdx);

                _handDataAsset.IsFingerHighConfidence[fingerIdx] =
                    _ovrHand.GetFingerConfidence(ovrFingerIdx) == OVRHand.TrackingConfidence.High;

                _handDataAsset.FingerPinchStrength[fingerIdx] =
                    _ovrHand.GetFingerPinchStrength(ovrFingerIdx);
            }

            _handDataAsset.Root = new Pose(
                poseData.RootPose.Position.FromFlippedZVector3f(),
                poseData.RootPose.Orientation.FromFlippedZQuatf()
            );

            if (_ovrHand.IsPointerPoseValid)
            {
                _handDataAsset.PointerPoseOrigin = PoseOrigin.RawTrackedPose;
                var pp = _ovrHand.PointerPose;
                _handDataAsset.PointerPose = new Pose(pp.localPosition, pp.localRotation);
            }
            else
            {
                _handDataAsset.PointerPoseOrigin = PoseOrigin.None;
            }

#if ISDK_OPENXR_HAND
            float scaleFixup = _handDataAsset.HandScale > 0 ? 1f / _handDataAsset.HandScale : 0f;

            for (int i = 0; i < Constants.NUM_HAND_JOINTS; i++)
            {
                Pose jointPose = new Pose(
                    poseData.BoneTranslations[i].FromFlippedZVector3f(),
                    poseData.BoneRotations[i].FromFlippedZQuatf());

                Pose fromRoot = PoseUtils.Delta(_handDataAsset.Root, jointPose);
                fromRoot.position *= scaleFixup;
                _handDataAsset.JointPoses[i] = fromRoot;

                // ✅ use cached, no per-frame Array.FindIndex alloc
                _handDataAsset.JointRadii[i] = _cachedRadii[i];
            }

#pragma warning disable 0618
            HandJointUtils.WristJointPosesToLocalRotations(_handDataAsset.JointPoses,
                ref _handDataAsset.Joints);
#pragma warning restore 0618
#else
        var bones = poseData.BoneRotations;
        for (int i = 0; i < bones.Length; i++)
        {
            _handDataAsset.Joints[i] = float.IsNaN(bones[i].w)
                ? Config.HandSkeleton.joints[i].pose.rotation
                : bones[i].FromFlippedXQuatf();
        }
        _handDataAsset.Joints[0] = WristFixupRotation;
#endif
        }

        private void EnsureRadiiCached()
        {
            if (_radiiReady && _cachedHandedness == _handedness) return;

            var skel = (_handedness == Handedness.Left)
                ? OVRSkeletonData.LeftSkeleton
                : OVRSkeletonData.RightSkeleton;

            int maxIdx = 0;
            var caps = skel.BoneCapsules;
            for (int i = 0; i < caps.Length; i++)
                if (caps[i].BoneIndex > maxIdx) maxIdx = caps[i].BoneIndex;

            if (_radiusByBoneIndex == null || _radiusByBoneIndex.Length < maxIdx + 1)
                _radiusByBoneIndex = new float[maxIdx + 1];
            else
                Array.Clear(_radiusByBoneIndex, 0, _radiusByBoneIndex.Length);

            for (int i = 0; i < caps.Length; i++)
                _radiusByBoneIndex[caps[i].BoneIndex] = caps[i].Radius;

            if (_cachedRadii == null || _cachedRadii.Length != Constants.NUM_HAND_JOINTS)
                _cachedRadii = new float[Constants.NUM_HAND_JOINTS];

            for (int joint = 0; joint < Constants.NUM_HAND_JOINTS; joint++)
            {
                int mapped = RemapBoneIndexForSDK(joint);
                _cachedRadii[joint] = (mapped >= 0 && mapped < _radiusByBoneIndex.Length)
                    ? _radiusByBoneIndex[mapped]
                    : 0f;
            }

            _radiiReady = true;
            _cachedHandedness = _handedness;
        }

        private static int RemapBoneIndexForSDK(int boneIndex)
        {
#if ISDK_OPENXR_HAND
            if (boneIndex == (int)HandJointId.HandIndex0) return (int)HandJointId.HandIndex1;
            if (boneIndex == (int)HandJointId.HandMiddle0) return (int)HandJointId.HandMiddle1;
            if (boneIndex == (int)HandJointId.HandRing0) return (int)HandJointId.HandRing1;
            if (boneIndex == (int)HandJointId.HandPinky0) return (int)HandJointId.HandPinky1;
#else
        if (boneIndex == (int)HandJointId.HandThumb0)  return (int)HandJointId.HandThumb1;
        if (boneIndex == (int)HandJointId.HandPinky0)  return (int)HandJointId.HandPinky1;
#endif
            return boneIndex;
        }*/
    }
    // Replace this line:
    // _cachedRadii[i] = HandSkeletonOVR.GetBoneRadius(_ovrSkeletonCached, i);

    // With the following code block, which manually extracts the radius from the skeleton's BoneCapsules array:
    
}