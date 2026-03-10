using System.Threading.Tasks;
using UnityEngine;

public abstract class ServiceBundle<T> where T : class
{
    protected readonly T _metaData;

    public ServiceBundle(T data) => _metaData = data;

    public abstract Task InitialiseAsync();
}
