using System.Collections;
using UnityEditor;
using UnityEngine;

public class TestFov : MonoBehaviour, IFieldOfViewOwner
{
    public FOVParameters _params;
    private NPCFieldOfViewHandler _fovHandler;
    private ITargetable _player;
    public FOVResult _result = FOVResult.TargetNotSeen;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _fovHandler = new NPCFieldOfViewHandler(this, _params);
       
        StartCoroutine(GetPlayerDelay());
    }

    IEnumerator GetPlayerDelay()
    {
        yield return new WaitForSeconds(3f);

        if(GameManager.Instance.TryGetPlayer(out var player))
        {
            _player = player;
            Debug.LogError("Player Found!!!");
            _params.FOVTarget = _player;
        }
        else
        {
            Debug.LogError("Failed to find player interface");
        }
       
    }

    // Update is called once per frame
    void Update()
    {
        _fovHandler?.Tick();
    }

    public void FieldOfViewSweepResult(FOVResult result, bool withinAttackAngles)
    {
        Debug.LogError("In ShootingAngle: "+withinAttackAngles);
        //Debug.LogError("Recieved FOV result of: "+result.ToString());
        
       
    }

    public float _horizontalAngleMultiplier;
    public float _verticalAngleMultiplier;

   

    void OnDrawGizmos()
    {
        if (_params.fovOrigin == null) return;
      

        Vector3 origin = _params.fovOrigin.position;
        float viewRadius = _params.fovRadius;//_proximityRadius;
        float hAng = _params.fovHalfAngle * _horizontalAngleMultiplier;//_fovViewangle * _horizontalAngleMultiplier;
        float vAng = _params.fovHalfAngle * _verticalAngleMultiplier;

#if UNITY_EDITOR
        Handles.color = Color.white;
#endif
        // Draw detection sphere
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(origin, viewRadius);

        // Fetch basis vectors
        Vector3 forward = _params.fovOrigin.forward;  // full 3D forward
        Vector3 up = _params.fovOrigin.up;
        Vector3 right = _params.fovOrigin.right;

        // Horizontal bounds: rotate forward around head.up
        Vector3 rightBound = Quaternion.AngleAxis(hAng, up) * forward;
        Vector3 leftBound = Quaternion.AngleAxis(-hAng, up) * forward;

        if (_result == FOVResult.TargetSeen)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.red;
        }
            Gizmos.DrawRay(origin, rightBound * viewRadius);
        Gizmos.DrawRay(origin, leftBound * viewRadius);

        // Vertical bounds: rotate forward around head.right
        Vector3 upperBound = Quaternion.AngleAxis(vAng, right) * forward;
        Vector3 lowerBound = Quaternion.AngleAxis(-vAng, right) * forward;

        Gizmos.DrawRay(origin, upperBound * viewRadius);
        Gizmos.DrawRay(origin, lowerBound * viewRadius);



    }
}
