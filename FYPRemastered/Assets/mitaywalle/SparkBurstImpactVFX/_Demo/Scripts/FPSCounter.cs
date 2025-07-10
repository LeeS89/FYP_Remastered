using UnityEngine;
using UnityEngine.UI;

namespace mitaywalle.SparksVFXMobile._Demo.Scripts
{
    public class FPSCounter : MonoBehaviour
    {
        private const string DISPLAY_FORMAT = "{0} FPS";
        private float _timer = 0;
        private float _refresh = 0;
        private float _avgFramerate = 0;
        private Text _text = default;

        private void Start()
        {
            _text = GetComponent<Text>();
        }

        private void Update()
        {
            if (Time.timeScale == 0) return;
            float timelapse = Time.smoothDeltaTime;
            _timer = _timer <= 0 ? _refresh : _timer -= timelapse;

            if (_timer <= 0) _avgFramerate = (int) (1f / timelapse);
            _text.text = string.Format(DISPLAY_FORMAT, _avgFramerate);
        }
    }
}