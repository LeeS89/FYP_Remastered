using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageDatabase", menuName = "Scriptable Objects/DamageDatabase")]
public class DamageDatabase : ScriptableObject
{
    public List<DamageData> bulletDataList;

    public DamageData GetBulletData(DamageType bulletType)
    {
        return bulletDataList.Find(data => data.damageType == bulletType);
    }
}
