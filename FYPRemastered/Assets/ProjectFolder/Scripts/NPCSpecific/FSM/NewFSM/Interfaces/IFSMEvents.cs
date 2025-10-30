using System;
using UnityEngine;

public interface IFSMEvents : ITickable
{
    void BeginPatrol();
    void BeginChase();
    void BeginFlank();
    void TakeCover();
    void FollowGroup();

    //void BeginSearch();
    void ClearState();

    void OnPathRequestComplete(in PathResult result);


    Transform Transform { get; }

    StateNotificationProvider Notification { get; set; }
}
