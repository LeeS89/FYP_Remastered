using System;
using UnityEngine;

public class BufferedInbox
{
    private readonly NotificationInbox _a = new();
    private readonly NotificationInbox _b = new();
    private NotificationInbox _write;
    private NotificationInbox _read;

    public BufferedInbox()
    {
        _write = _a;
        _read = _b;
    }

    public void Enqueue(in NpcNotification notification) => _write.Enqueue(notification);

    public void Flush(Action<NpcNotification> handler)
    {
        // Swap read and write buffers
        var temp = _read;
        _read = _write;
        _write = temp;

        _write.Clear();

        // Flush the read buffer
        _read.Flush(handler);
        _read.Clear();
    }


}
