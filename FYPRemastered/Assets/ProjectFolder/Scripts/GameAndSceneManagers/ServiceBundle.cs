using System.Threading.Tasks;
using UnityEngine;

public abstract class ServiceBundle
{
    protected readonly SceneMetaData _metaData;

    public ServiceBundle(SceneMetaData data) => _metaData = data;

    public abstract Task InitialiseAsync();
}
