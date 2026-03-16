using Npc.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace Services.Internal
{

    public class FsmFactory : ServiceBundle<FsmFeatureGroup>, IFsmFactory, ITickable
    {

        private List<ITickable> _tickables = new(5);
        private IAddressableService _wpService; // Split interfaces in WaypointResources class so only correct interface is used by relevant classes i.e. => This class need only store it as an IAddressableService
        private IAddressableService _flankService;
        private IAddressableService _chaseService;
        private IDistanceService _distService;

        private FsmRegistry _registry;
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
                _wpService = await TryLoadStateServiceAndInitialize</*WaypointBlockData, */WaypointResources>(waypointFeature/*, ()=> new WaypointResources()*/);
                if (_wpService == null) DebugLogs.Nre(_wpService, "WaypointService");
                else DebugLogs.Log("Found Waypoint service", this);

            }

            var flankFeature = _metaData.FlankPoints;
            if (flankFeature.enabled)
            {
                _flankService = await TryLoadStateServiceAndInitialize</*SamplePointDataSO,*/ PlayerFlankingResources>(flankFeature/*, () => new PlayerFlankingResources()*/);
                if (_flankService == null) DebugLogs.Nre(_flankService, "Flank service", this);
                else DebugLogs.Log("Flank Service Constructed successfully", this);
                //  var flnk = await TryLoadStateServiceAndInitialize<SamplePointDataSO, PlayerFlankingResources>(flankFeature, ()=> new PlayerFlankingResources());
            }

            var chaseFeature = _metaData.ChaseData;
            if (chaseFeature.enabled)
            {
                _chaseService = await TryLoadStateServiceAndInitialize<ChaseResources>(chaseFeature);
                if (_chaseService == null) DebugLogs.Nre(_chaseService, "Chase Service", this);
                else DebugLogs.Log("Chase service constructed successfully", this);
            }

        }

        private async Task<TConcrete> TryLoadStateServiceAndInitialize<TAsset, TConcrete>(FeatureMeta data, Func<TConcrete> factory) where TConcrete : class, IAddressableService
        {
            if (string.IsNullOrWhiteSpace(data.addressKey)) { DebugLogs.RequireNotNull(data.addressKey, "addressKey", this); return null; }

            var svc = factory();//new TConcrete();
            bool serviceInitSuccess = await svc.TryInitialiseAsync(data);

            if (!serviceInitSuccess)
            {
                DebugLogs.LoadFail(svc, $"(The Service of {typeof(TConcrete).Name})", this);
                svc.Dispose();
                return null;

            }

            return svc;
        }
        private async Task<TConcrete> TryLoadStateServiceAndInitialize<TConcrete>(FeatureMeta data) where TConcrete : class, IAddressableService, new()
        {
            if (string.IsNullOrWhiteSpace(data.addressKey)) { DebugLogs.RequireNotNull(data.addressKey, "addressKey", this); return null; }

            var svc = new TConcrete();
            bool serviceInitSuccess = await svc.TryInitialiseAsync(data);

            if (!serviceInitSuccess)
            {
                DebugLogs.LoadFail(svc, $"(The Service of {typeof(TConcrete).Name})", this);
                svc.Dispose();
                return null;

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

        public bool TryCreateFsm(out IFsmController fsm, INpcBody body, TryGetTarget targetRetrieverFunc, IPathNotifications pathNotifySender, IAnimationRequestNotifications animNotifySender = null)
        {
            if (body == null) { fsm = null; return false; }

            if (_registry == null) _registry = new FsmRegistry();
            int id = body.Owner.Transform.GetInstanceID();

            if (!_registry.TryRegister(id, body, targetRetrieverFunc)) { fsm = null; return false; }

            fsm = new FsmManager(null, null,null, null, null); // Placeholder
            return true;
        }
    }


    public interface IFsmFactory
    {
        bool TryCreateFsm(out IFsmController fsm, INpcBody body, TryGetTarget targetRetrieverFunc,
            IPathNotifications pathNotifySender, IAnimationRequestNotifications animNotifySender = null);

        bool TryCreateAndAddState(StateId id, Dictionary<StateId, IFsmState> _stateDict, NavMeshPath path, Transform ownerTransform, TryGetTarget targetRetrieverFunc);
    }









    internal interface IFsmManagerView
    {
        bool TryGetAgent(IFsmController controller, out NavMeshAgent a);
        bool TryGetObstacle(IFsmController controller, out NavMeshObstacle o);
    }

    internal interface IStateView
    {

    }





   



    internal sealed class FsmRegistry : IDisposable
    {
        private readonly Dictionary<int, FsmEntry> _entries = new(25);

        public bool TryRegister(int npcId, INpcBody body, TryGetTarget targetGetter)
            => _entries.TryAdd(npcId, new FsmEntry(body, targetGetter));

        #region Owning Npc Data
        public bool TryGetOwnerTransform(int id, out Transform t)
        {
            if (!_entries.TryGetValue(id, out var entry)) { t = null; return false; }

            return entry.TryGetOwnerTransform(out t);
        }

        public bool TryGetOwnerPosition(int id, out Vector3 pos)
        {
            if (!_entries.TryGetValue(id, out var entry)) { pos = default; return false; }
            return entry.TryGetOwnerPosition(out pos);
        }

        public bool TryGetAgent(int id, out NavMeshAgent agent)
        {
            if (!_entries.TryGetValue(id, out var entry)) { agent = null; return false; }
            return entry.TryGetAgent(out agent);
        }

        public bool TryGetObstacle(int id, out NavMeshObstacle obstacle)
        {
            if(!_entries.TryGetValue(id, out var entry)) { obstacle = null; return false; }
            return entry.TryGetObstacle(out obstacle);
        }

        public bool TryGetPath(int id, out NavMeshPath path)
        {
            if(!_entries.TryGetValue(id, out var entry)) { path = null; return false; }
            return entry.TryGetPath(out path);
        }
        #endregion

        #region Owning Npc Target Data
        public bool TryGetTargetPosition(int id, out Vector3 pos)
        {
            if (!_entries.TryGetValue(id, out var entry)) { pos = default; return false; }
            return entry.TryGetTargetPosition(out pos);
        }
        #endregion

        public void Dispose()
        {
            foreach (var entry in _entries.Values) { entry.Dispose(); }
            _entries.Clear();
        }

        private sealed class FsmEntry : IDisposable
        {
            private INpcBody _body;
            private TryGetTarget _targetGetter;

            
            public FsmEntry(INpcBody body, TryGetTarget targetGetterFunc)
            {
                _body = body;
                _targetGetter = targetGetterFunc;
            }

            #region Owning Npc Data region
            public bool TryGetOwnerTransform(out Transform t)
            {
                if (_body == null || _body.Owner == null) { t = null; return false; }
             
                t = _body.Owner.Transform;
                return t != null;
            }

            public bool TryGetOwnerPosition(out Vector3 pos)
            {
                if (_body == null || _body.Owner == null) { pos = Vector3.zero; return false; }

                pos = _body.Owner.Position();
                return true;
            }

            public bool TryGetAgent(out NavMeshAgent a)
            {
                if (_body == null) { a = null; return false; }
                
                a = _body.Agent;
                return a != null;
            }

            public bool TryGetObstacle(out NavMeshObstacle o)
            {
                if (_body == null) { o = null; return false; }
                o = _body.Obstacle;
                return o != null;
            }

            public bool TryGetPath(out NavMeshPath p)
            {
                if(_body == null) { p = null; return false; }
                p = _body.Path;
                return p != null;
            }
            #endregion

            #region Owning Npc Target data

            public bool TryGetTargetPosition(out Vector3 pos)
            {
                pos = default;

                if (_targetGetter == null) return false;

                if (!_targetGetter.Invoke(out var target)) return false;

                if (target == null || target.Transform == null) return false;
                pos = target.Transform.position;

                return true;
            }


            #endregion

            public void Dispose()
            {
                _body = null;
                _targetGetter = null;
            }

        }
    }
    
}

public interface INpcBody
{
    ITargetable Owner { get; }
    NavMeshAgent Agent { get; }
    NavMeshObstacle Obstacle { get; }
    NavMeshPath Path { get; }
}