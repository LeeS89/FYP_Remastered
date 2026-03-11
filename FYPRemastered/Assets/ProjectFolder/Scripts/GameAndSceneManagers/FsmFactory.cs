using Npc.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Services.Internal
{

    public class FsmFactory : ServiceBundle<FsmFeatureGroup>, IFsmService, ITickable
    {

        private List<ITickable> _tickables = new(5);
        private IAddressableService _wpService; // Split interfaces in WaypointResources class so only correct interface is used by relevant classes i.e. => This class need only store it as an IAddressableService
        private IFlankService _flankService;
        private IDistanceService _distService;

        // SO's
  
        private AgentChaseData _agentChaseData;
        // End SO's

        private readonly List<AsyncOperationHandle> _handles = new(5);

       
        public FsmFactory(FsmFeatureGroup data) : base(data) { }

    

        public IWaypointService GetWService() => _wpService as IWaypointService;


        public override async Task InitialiseAsync()
        {

            if (_metaData == null)
            {
                DebugLogs.RequireNotNull(_metaData, "SceneMetaData", this);
                return;
            } //=> Possibly just switch all to manual systems

            var waypointFeature = _metaData.Waypoints;
            if (waypointFeature.enabled)
            {
                _wpService = await TryLoadStateServiceAndInitialize<WaypointBlockData, WaypointResources>(waypointFeature/*.addressKey*/);
                if (_wpService == null) DebugLogs.Nre(_wpService, "WaypointService");
                else DebugLogs.Log("Found Waypoint service", this);



            }


        }

        private async Task<TConcrete> TryLoadStateServiceAndInitialize<TAsset, TConcrete>(FeatureMeta data/*string addressKey*/) where TConcrete : class, IAddressableService, new()
        {
            if (string.IsNullOrWhiteSpace(data.addressKey)) { DebugLogs.RequireNotNull(data.addressKey, "addressKey", this); return null; }

            var svc = new TConcrete();
            bool serviceInitSuccess = await svc.TryInitialiseAsync(/*addressKey*/data);

            if (!serviceInitSuccess)
            {
                DebugLogs.LoadFail(svc, $"(The Service of {typeof(TConcrete).Name})", this);
                svc.Dispose();
                return null;
                // Call Dispose on scv and return null;
            }

            return svc;
        }
        // END NEW META SETUP


        public void Dispose()
        {
            throw new System.NotImplementedException();
        }






        public void LateTick(float dt) { }


        public void Tick(float dt)
        {
            if (_tickables == null || _tickables.Count == 0) return;

            foreach (var t in _tickables)
                t.Tick(dt);
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
}

public interface IFsmService
{
    bool TryCreateAndAddState(StateId id, Dictionary<StateId, IFsmState> _stateDict, NavMeshPath path, Transform ownerTransform, TryGetTarget targetRetrieverFunc);
}
