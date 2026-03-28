using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class GraphicsStateRecorder : MonoBehaviour
{
    GraphicsStateCollection _gsc;

    async void Start()
    {
        _gsc = new GraphicsStateCollection();

        if (!_gsc.BeginTrace())
        {
            Debug.LogError("[GSC] BeginTrace failed.");
            return;
        }

        Debug.LogError("[GSC] Trace started. Exercise your content now…");

        await Awaitable.WaitForSecondsAsync(5f); // let all materials/shaders be seen once

        _gsc.EndTrace();

#if UNITY_EDITOR
        // ✅ call on the instance, not the class
        bool ok = _gsc.SendToEditor("Assets/Boot.graphicsState");
        Debug.LogError(ok
            ? "[GSC] Trace data sent to Editor and saved as Assets/Boot.graphicsState"
            : "[GSC] Failed to send trace data to Editor");
#else
        // For player builds, use Serialize / SaveToFile approach instead
#endif
    }
}
