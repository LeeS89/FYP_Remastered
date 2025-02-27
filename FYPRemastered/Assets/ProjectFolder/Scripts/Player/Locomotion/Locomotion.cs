using Oculus.Interaction;
using UnityEngine;

public class Locomotion : MonoBehaviour, IBindableToPlayerEvents
{
    // Character Controller variables - start
    [SerializeField] CharacterController _controller;

    [SerializeField] private float _bodyHeightMin = 0.5f;
    [SerializeField] private float _bodyHeightMax = 2f;
    private Vector3 _newControllerCenter = Vector3.zero;
    [SerializeField] private float _moveSpeed = 4.0f;
    [SerializeField] private bool _shouldMoveForward = false;
    private const float GRAVITY = -9.8f;
    //private float velocityY = 0f;
    private Vector3 velocity = Vector3.zero;
    private Vector3 _moveDirection = Vector3.zero;
    private float _effectiveMoveSpeed = 0f;

    float newVelocityY = 0f;
    //End



    private void Awake()
    {
        if (TryGetComponent<CharacterController>(out CharacterController characterController))
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
        if (eventManager == null)
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
        ApplyPlayerMovement();
    }

    private void HandleRotation(Quaternion targetRotation)
    {
        _controller.transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
    }

    private void AdjustPlayerHeight(Vector3 _cameraLocalPos)
    {
        if (_controller == null) { return; }

        _controller.height = Mathf.Clamp(_cameraLocalPos.y, _bodyHeightMin, _bodyHeightMax);
        _newControllerCenter.x = _cameraLocalPos.x;
        _newControllerCenter.y = _controller.height / 2;
        _newControllerCenter.z = _cameraLocalPos.z;



        _controller.center = _newControllerCenter;

    }

   
    private void SetShouldMoveforward(bool move)
    {
        _shouldMoveForward = move;
    }



    private void ApplyPlayerMovement()
    {
        if (_controller == null) { return; }

        _effectiveMoveSpeed = _shouldMoveForward ? _moveSpeed : 0f;

        _moveDirection = transform.forward;
        velocity = _moveDirection * _effectiveMoveSpeed;
        velocity.y = ApplyGravity();

        _controller.Move(velocity * Time.deltaTime);

    }

    private float ApplyGravity()
    {
        if(_controller.isGrounded)
        {
            newVelocityY = GRAVITY / 2f;
        }
        else
        {
            newVelocityY += GRAVITY * Time.deltaTime;
        }
       
        return newVelocityY;
    }

    
}
