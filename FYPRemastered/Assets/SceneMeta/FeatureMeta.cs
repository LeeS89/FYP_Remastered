using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FeatureMeta
{
    public bool enabled;
    public string addressKey;
    public List<string> subDataKeys;
}
