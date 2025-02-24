using UnityEngine;

public class Locomotion : MonoBehaviour, IBindableToPlayerEvents
{

    [SerializeField]
    CharacterController _controller;

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
    }

    private void HandleRotation(Quaternion targetRotation)
    {
        _controller.transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
    }
}
