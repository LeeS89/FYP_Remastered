using UnityEngine;

public class RotationTracker : MonoBehaviour, IBindableToPlayerEvents
{
    [SerializeField]
    private Transform _playerCamera;

    [SerializeField]
    PlayerEventManager _playerEventManager;

    public float _rotationSpeed = 5.0f;
    public float _rotateThreshold = 20.0f;
    public float _stopRotatingThreshold = 18.0f;

    private Vector3 _moveDirection;
    private Quaternion _startRotation;
    private bool _isRotating = false;




    /*// Event to broadcast rotation (to be subscribed by the locomotion script)
    public delegate void RotationHandler(Quaternion targetRotation);
    public event RotationHandler OnRotationChanged;*/

    public void OnBindToPlayerEvents(PlayerEventManager eventManager)
    {
        _playerEventManager = eventManager;
    }


    private void Update()
    {
        if (_playerEventManager == null || _playerCamera == null) { return; }

        CheckRotation();

    }

    private void CheckRotation()
    {
        _moveDirection = _playerCamera.forward;
        _moveDirection.y = 0f;  // Ensure we don't rotate around the X and Z axes
        _moveDirection.Normalize(); 

        // Target rotation
        _startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(_moveDirection, Vector3.up);

        // Calculate angle difference
        float angleDifference = Quaternion.Angle(_startRotation, targetRotation);

        // Start rotating if the angle difference exceeds the threshold
        if (!_isRotating && angleDifference > _rotateThreshold)
        {
            _isRotating = true;
        }
        // Stop rotating if the angle difference is small enough
        else if (_isRotating && angleDifference < _stopRotatingThreshold)
        {
            _isRotating = false;
        }

        // Apply rotation if rotating
        if (_isRotating)
        {
            // Smoothly interpolate the rotation
            targetRotation = Quaternion.Slerp(_startRotation, targetRotation, _rotationSpeed * Time.deltaTime);
            _playerEventManager.PlayerRotate(targetRotation);
        }
    }

   
}
