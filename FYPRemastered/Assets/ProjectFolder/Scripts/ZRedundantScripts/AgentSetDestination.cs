using ProjectDawn.Navigation.Hybrid;
using UnityEngine;

public class AgentSetDestination : MonoBehaviour
{
    public Transform target;
    public AgentAuthoring _auth;
    public AgentNavMeshAuthoring _pathAuth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _auth.SetDestination(target.position);
        //_auth.EntityBody.RequestValidPath
        //GetComponent<AgentNavMeshAuthoring>
        
    }

    
}
