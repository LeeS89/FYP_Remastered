using Npc.API;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FsmStateFactory
{
    private IWaypointService _wpService;
    private IFlankService _flkService;
    private IDistanceService _distService;


    private FsmStateFactory() { }

    public FsmStateFactory(IWaypointService wpService = null, IFlankService fService = null, IDistanceService dService = null)
    {

    }

    /// <summary>
    /// Attempts to create a new state with the specified identifier and add it to the provided state dictionary.
    /// </summary>
    /// <param name="id">The unique identifier for the state to create and add.</param>
    /// <param name="_stateDict">A dictionary that maps state identifiers to their corresponding state instances. The new state will be added to
    /// this dictionary if creation succeeds.</param>
    /// <param name="path">The navigation path to associate with the new state. Cannot be null.</param>
    /// <param name="ownerTransform">The transform representing the owner of the state. Used to initialize the new state.</param>
    /// <param name="targetRetrieverFunc">A delegate used to retrieve the target for the state. The function is invoked whenever a state needs to know its targets position</param>
    /// <returns>true if the state was successfully created and added to the dictionary; otherwise, false.</returns>
    public bool TryCreateAndAddState(StateId id, Dictionary<StateId, IFsmState> _stateDict, NavMeshPath path, Transform ownerTransform, TryGetTarget targetRetrieverFunc)
    {
        if (id == StateId.None || _stateDict == null || path == null ||
            ownerTransform == null || targetRetrieverFunc == null) return false;

        if (_stateDict.ContainsKey(id)) return false;


        return id switch
        {
            StateId.Patrol => TryCreatePatrol(_stateDict, path, ownerTransform, targetRetrieverFunc),
            _ => false

        };

       /* switch (id)
        {
            case StateId.Patrol:
              //  FSMPatrolState ps = new FSMPatrolState();
               // _stateDict[id] = ps;
                return true;
            default:
                return false;
        }*/

    }

    private bool TryCreatePatrol(Dictionary<StateId, IFsmState> _dict, NavMeshPath path, Transform t, TryGetTarget tgt)
        => _dict.TryAdd(StateId.Patrol, new FSMPatrolState(null, null, null));
}
