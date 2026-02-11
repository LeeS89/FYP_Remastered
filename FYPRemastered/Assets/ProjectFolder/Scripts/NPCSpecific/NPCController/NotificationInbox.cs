using System;
using System.Collections.Generic;
using UnityEngine;

public class NotificationInbox
{
    private readonly List<NpcNotification> _critical = new(8);
    private readonly List<NpcNotification> _high = new(8);
    private readonly List<NpcNotification> _normal = new(16);
    private readonly List<NpcNotification> _low = new(16);

    public void Clear()
    {
        _critical.Clear();
        _high.Clear();
        _normal.Clear();
        _low.Clear();
    }

    public void Enqueue(NpcNotification notification)
    {
        switch (notification.Priority)
        {
            case NotifyPriority.Critical:
                _critical.Add(notification);
                break;
            case NotifyPriority.High:
                _high.Add(notification);
                break;
            case NotifyPriority.Normal:
                _normal.Add(notification);
                break;
            case NotifyPriority.Low:
                _low.Add(notification);
                break;
        }
    }

    public void Flush(Action<NpcNotification> handler)
    {
        for(int i = 0; i < _critical.Count; i++) handler(_critical[i]);
        for(int i = 0; i < _high.Count; i++) handler(_high[i]);
        for(int i = 0; i < _normal.Count; i++) handler(_normal[i]);
        for(int i = 0; i < _low.Count; i++) handler(_low[i]);
    }
}
