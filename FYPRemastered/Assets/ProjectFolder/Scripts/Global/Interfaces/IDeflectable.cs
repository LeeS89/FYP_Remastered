using UnityEngine;

public interface IDeflectable
{
    void Deflect();

    GameObject ParentOwner { get; set; }
    GameObject RootComponent { get; set; }

}
