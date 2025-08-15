using UnityEngine;


public sealed class RotationHandler
{
    public readonly struct Config
    {
        public readonly Transform Eyes;
        public readonly Transform Body;
        public readonly float RotationSpeed;
        public readonly float RotationThreshold;
        public readonly float ExitRotationThreshold;

        public Config(Transform eyes, Transform body, float rotationSpeed, float rotationThreshold, float exitRotationThreshold)
        {
            Eyes = eyes;
            Body = body;
            RotationSpeed = rotationSpeed;
            RotationThreshold = rotationThreshold;
            ExitRotationThreshold = exitRotationThreshold;
        }
    }
   
    private PlayerEventManager _eventManager;

    public float RotationSpeed { get; set; }
    public float RotationThreshold { get; set; }
    public float StopRotationThreshold { get; set; }
    public bool IsRotating { get; private set; } = false;

   
    private Transform _eyes;
    private Transform _body;
   
    public RotationHandler(PlayerEventManager eventManager, in Config cfg)
    {
        _eventManager = eventManager;
        _eyes = cfg.Eyes;
        _body = cfg.Body;
        RotationSpeed = cfg.RotationSpeed;
        RotationThreshold = cfg.RotationThreshold;
        StopRotationThreshold = cfg.ExitRotationThreshold;
    }


    public void LateTick()
    {
        CheckRotation();
        CheckHeight();
    }

    private void CheckRotation()
    {
        Vector3 rotationDirection = _eyes.forward;
        //_moveDirection = _playerCamera.forward;
        rotationDirection.y = 0f;  // Ensure we don't rotate around the X and Z axes
        rotationDirection.Normalize();

        // Target rotation
        Quaternion startRotation = _body.rotation;
        //_startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(rotationDirection, Vector3.up);

        // Calculate angle difference
        float angleDifference = Quaternion.Angle(startRotation, targetRotation);

        // Start rotating if the angle difference exceeds the threshold
        if (!IsRotating && angleDifference > RotationThreshold)
        {
            IsRotating = true;
        }
        // Stop rotating if the angle difference is small enough
        else if (IsRotating && angleDifference < StopRotationThreshold)
        {
            IsRotating = false;
        }

        // Apply rotation if rotating
        if (IsRotating)
        {
            // Smoothly interpolate the rotation
            targetRotation = Quaternion.Slerp(startRotation, targetRotation, RotationSpeed * Time.deltaTime);
            _eventManager.PlayerRotate(targetRotation);
        }
    }

    private void CheckHeight()
    {
        _eventManager.PlayerHeightUpdated(_eyes.localPosition);
    }

    public void OnInstanceDestroyed()
    {
        _eyes = null;
        _body = null;
        _eventManager = null;
    }

}
