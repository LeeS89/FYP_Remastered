using UnityEngine;

namespace mitaywalle.SparkBurstImpactVFX._Demo.Scripts
{
    [RequireComponent(typeof(Camera))]
    public class ReplaceShaderWireframe : MonoBehaviour
    {
        private Camera _cam;
        [SerializeField] private Shader _shader;
        [SerializeField] private bool _wireframe;
        [SerializeField] private bool _replaceShader;

        private void Start()
        {
            if (!_shader)
            {
                _shader = Shader.Find("Unlit/Color");
            }

            if (!_cam) _cam = GetComponent<Camera>();
        }

        public void SetReplaceShader(bool state)
        {
            _replaceShader = state;
        }
        
        private void OnPreRender()
        {
            if (_replaceShader)
            {
                _cam.SetReplacementShader(_shader, null);
            }
            else
            {
                _cam.SetReplacementShader(null, null);
            }
            if (_wireframe)
            {
                GL.wireframe = true;
            }
        }

        private void OnPostRender()
        {
            GL.wireframe = false;
        }
    }
}