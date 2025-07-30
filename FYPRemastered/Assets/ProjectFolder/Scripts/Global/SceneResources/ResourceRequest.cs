
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceRequest
{  
    public PoolResourceType ResourceType { get; set; }

    public Action<PoolManager> poolRequestCallback;

    public virtual void OnInstanceDestroyed()
    {
        poolRequestCallback = null;
    }
}
