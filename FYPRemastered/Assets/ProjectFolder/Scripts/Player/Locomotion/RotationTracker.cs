using UnityEngine;

public class RotationTracker : MonoBehaviour, IComponentEvents, IPlayerEvents
{
    [SerializeField]
    PlayerEventManager _playerEventManager;

    public float _rotationSpeed = 5.0f;
    public float _rotateThreshold = 20.0f;
    public float _stopRotatingThreshold = 18.0f;

    private Vector3 _rotationDirection;
    private Quaternion _startRotation;
    private bool _isRotating = false;

    private bool _inputEnabled = false;

    public bool InputEnabled
    {
        get => _inputEnabled;
        private set => _inputEnabled = value;
    }
    /*// Event to broadcast rotation (to be subscribed by the locomotion script)
    public delegate void RotationHandler(Quaternion targetRotation);
    public event RotationHandler OnRotationChanged;*/

  
    public void RegisterEvents(EventManager eventManager)
    {
        _playerEventManager = (PlayerEventManager)eventManager;
        GameManager.OnPlayerRespawn += OnPlayerRespawned;
        GameManager.OnPlayerDied += OnPlayerDied;
        
    }

    public void UnRegisterEvents(EventManager eventManager)
    {
        GameManager.OnPlayerRespawn -= OnPlayerRespawned;
        GameManager.OnPlayerDied -= OnPlayerDied;
        _playerEventManager = null;
    }

    

    private void LateUpdate()
    {
        if (!InputEnabled || _playerEventManager == null) { return; }

        CheckRotation();
        CheckHeight();

    }

    private void CheckRotation()
    {
        _rotationDirection = transform.forward;
        //_moveDirection = _playerCamera.forward;
        _rotationDirection.y = 0f;  // Ensure we don't rotate around the X and Z axes
        _rotationDirection.Normalize();

        // Target rotation
        _startRotation = transform.root.rotation;
        //_startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(_rotationDirection, Vector3.up);

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

    private void CheckHeight()
    {
        _playerEventManager.PlayerHeightUpdated(transform.localPosition);
    }

    public void OnSceneStarted()
    {
        throw new System.NotImplementedException();
    }

    public void OnSceneComplete()
    {
        throw new System.NotImplementedException();
    }

    public void OnPlayerDied()
    {
        InputEnabled = false;
    }

    public void OnPlayerRespawned()
    {
        InputEnabled = true;
    }
}
