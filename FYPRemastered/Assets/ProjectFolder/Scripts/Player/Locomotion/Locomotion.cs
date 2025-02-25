using UnityEngine;

public class Locomotion : MonoBehaviour, IBindableToPlayerEvents
{
    // Character Controller variables - start
    [SerializeField]
    CharacterController _controller;

    [SerializeField]
    private float _bodyHeightMin = 0.5f;
    [SerializeField]
    private float _bodyHeightMax = 2f;
    private Vector3 _newControllerCenter = Vector3.zero;
    [SerializeField]
    private float _moveSpeed = 4.0f;
    [SerializeField]
    private bool _shouldMoveForward = false;
    //End



    private void Awake()
    {
        if(TryGetComponent<CharacterController>(out CharacterController characterController))
        {
            _controller = characterController;
        }
        else
        {
            Debug.LogWarning("Character controller not found, please ensure component exists before use");
        }
    }

    public void OnBindToPlayerEvents(PlayerEventManager eventManager)
    {
        if(eventManager == null)
        {
            Debug.LogError("Player event manager is null");
            return;
        }
        eventManager.OnPlayerRotate += HandleRotation;
        eventManager.OnPlayerHeightUpdated += AdjustPlayerHeight;
        eventManager.OnPlayerMove += SetShouldMoveforward;
    }

    private void Update()
    {
        if (!_shouldMoveForward) { return; }

        MoveForward();
    }

    private void HandleRotation(Quaternion targetRotation)
    {
        _controller.transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
    }

    private void AdjustPlayerHeight(Vector3 _cameraLocalPos)
    {
        if(_controller == null) { return; }

        _controller.height = Mathf.Clamp(_cameraLocalPos.y, _bodyHeightMin, _bodyHeightMax);
        _newControllerCenter.x = _cameraLocalPos.x;
        _newControllerCenter.y = _controller.height / 2;
        _newControllerCenter.z = _cameraLocalPos.z;

        _controller.center = _newControllerCenter;


/*
        playerCollider.height = Mathf.Clamp(playerHead.localPosition.y, bodyHeightMin, bodyHeightMax);
        playerCollider.center = new Vector3(playerHead.localPosition.x, playerCollider.height / 2, playerHead.localPosition.z);*/
    }

    private void MoveForward()
    {
        if (_controller == null) { return; };

        Vector3 moveDirection = _controller.transform.forward;

        _controller.Move(moveDirection * _moveSpeed * Time.deltaTime);
    }

    private void SetShouldMoveforward(bool move)
    {
        _shouldMoveForward = move;
    }
}
