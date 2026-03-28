using System.Threading.Tasks;
using UnityEngine;

public interface IAddressableService : IDisposable
{
    Task<bool> TryInitialiseAsync(FeatureMeta data);

}
