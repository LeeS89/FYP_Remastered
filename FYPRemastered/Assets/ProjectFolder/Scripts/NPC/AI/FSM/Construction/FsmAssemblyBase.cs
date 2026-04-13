using UnityEngine;

public abstract class FsmAssemblyBase<T> : ServiceBundle<T> where T : class
{
    public FsmAssemblyBase(T data) : base(data) { }
    
}
