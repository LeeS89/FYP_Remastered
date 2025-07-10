using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR

#endif

namespace mitaywalle.SparkBurstImpactVFX._Demo.Scripts
{
    /// <summary>
    /// Main class for controlling Demo Scene
    /// </summary>
    public sealed class SparkVfxDemoController : MonoBehaviour
    {
        #region Vars

        private const string PARTICLES_COUNT_FORMAT = "particle Count: {0}";
        private const string TEXTURE_RESOLUTION_FORMAT = "resolution: {0}";

        // references
        [SerializeField] private ParticleSystem[] _prefabs = default;
        [SerializeField] private Transform _parent = default;
        [SerializeField] private GameObject _all = default;
        [SerializeField] private GameObject _closeSurface = default;
        [SerializeField] private Camera cam = default;

        // UI
        [SerializeField] private Text _text = default;
        [SerializeField] private Text _particleCountLabel = default;

        [SerializeField] private Button _textureResolution = default;
        [SerializeField] private Button _left = default;
        [SerializeField] private Button _right = default;

        [SerializeField] private Toggle _surface = default;
        [SerializeField] private Toggle _pause = default;
        [SerializeField] private Toggle _showAll = default;
        [SerializeField] private Toggle _soft = default;
        [SerializeField] private Toggle _wire = default;
        [SerializeField] private Toggle _overdraw = default;

        [SerializeField] private Slider _particleTime = default;
        [SerializeField] private Slider _timeScaleSlide = default;

        // temp
        private ParticleSystem _currentParentFx;
        private ParticleSystem[] _currentFxs;

        private int index;
        private int textureQualityLvl;

        #endregion

        void Start()
        {
            var fxs = GetComponentsInChildren<ParticleSystem>(true);
            foreach (var fx in fxs)
            {
#if UNITY_5_3_OR_NEWER
                var main = fx.main;
                main.loop = true;
#else
                fx.loop = true;
#endif
            }

            if (_wire)
            {
                // GL.wireframe is not supported in OpenGL ES
                if (Application.isMobilePlatform)
                {
                    _wire.gameObject.SetActive(false);
                    cam.transform.GetChild(0).gameObject.SetActive(false);
                }
                else
                {
                    _wire.onValueChanged.AddListener(ToggleWire);
                    _wire.isOn = false;
                }
            }

            if (_overdraw)
            {
                _overdraw.onValueChanged.AddListener(ToggleOverdraw);
                _overdraw.isOn = false;
            }


            _left.onClick.AddListener(Previous);
            _right.onClick.AddListener(Next);
            _textureResolution.onClick.AddListener(ChangeResolution);

            _showAll.onValueChanged.AddListener(ToggleAll);
            _pause.onValueChanged.AddListener(PauseTime);
            _surface.onValueChanged.AddListener(ToggleSurface);
            _soft.onValueChanged.AddListener(ToggleSoft);

            _timeScaleSlide.onValueChanged.AddListener(SetTimeScale);
            _soft.isOn = true;
            ToggleSoft(true);
            index = -1;
            Next();
            _showAll.isOn = true;
            ToggleAll(true);

            Application.targetFrameRate = 10000;
            QualitySettings.vSyncCount = 0;
            QualitySettings.antiAliasing = 0;
        }

        #region UI Events

        private void ToggleOverdraw(bool state)
        {
            cam.GetComponent<ReplaceShaderWireframe>().SetReplaceShader(state);
        }

        private void ToggleWire(bool state)
        {
            cam.transform.GetChild(0).gameObject.SetActive(state);
        }

        private void ToggleSoft(bool state)
        {
            QualitySettings.softParticles = state;
            cam.depthTextureMode = state ? DepthTextureMode.Depth : DepthTextureMode.None;
        }

        private void ChangeResolution()
        {
            textureQualityLvl++;

            if (textureQualityLvl > 3)
            {
                textureQualityLvl = 0;
            }

            QualitySettings.globalTextureMipmapLimit = textureQualityLvl;

            _textureResolution.GetComponentInChildren<Text>().text =
                string.Format(TEXTURE_RESOLUTION_FORMAT, 2048f / Mathf.Pow(2f, textureQualityLvl));
        }

        private void ToggleSurface(bool state)
        {
            _closeSurface.SetActive(state);
        }

        private void PauseTime(bool state)
        {
            Time.timeScale = !state ? _timeScaleSlide.value : 0;
        }

        private void ToggleAll(bool state)
        {
            _all.SetActive(state);
            _parent.gameObject.SetActive(!state);
            Select();
            _text.text = state ? "all prefabs" : _currentParentFx.name;
        }

        private void Previous()
        {
            index--;
            if (index < 0) index = _prefabs.Length - 1;
            _showAll.isOn = false;
            Select();
        }

        private void Next()
        {
            index++;
            if (index >= _prefabs.Length) index = 0;
            _showAll.isOn = false;
            Select();
        }

        private void Select()
        {
            if (!_showAll.isOn)
            {
                _currentFxs = _parent.GetChild(index).GetComponentsInChildren<ParticleSystem>(true);
                _currentParentFx = _currentFxs[0];
                _text.text = _currentParentFx.name;

                for (int i = 0; i < _prefabs.Length; i++) _prefabs[i].gameObject.SetActive(index == i);
            }
            else
            {
                _currentFxs = _all.GetComponentsInChildren<ParticleSystem>(true);
                _currentParentFx = _currentFxs[0];
                _text.text = "all prefabs";
            }

            _pause.isOn = false;
        }

        private void SetTimeScale(float timeScale)
        {
            if (!_pause.isOn)
            {
                Time.timeScale = timeScale;
            }
        }

        #endregion

        #region Update

        private void Update()
        {
            FillParticleTime();
            FillParticlesCount();
        }

        private void FillParticleTime()
        {
            _particleTime.value = _currentParentFx.time % _currentParentFx.main.duration;
        }

        private void FillParticlesCount()
        {
            var count = 0;

            ExecuteForEach((fx) => { count += fx.particleCount; });

            _particleCountLabel.text = string.Format(PARTICLES_COUNT_FORMAT, count);
        }

        #endregion

        private void ExecuteForEach(Action<ParticleSystem> action)
        {
            foreach (var fx in _currentFxs) action.Invoke(fx);
        }

        #region Editor

#if UNITY_EDITOR
        [ContextMenu("Reset")]
        private void Reset()
        {
            Undo.RecordObject(this, "reset");

            // Don't do GameObject.Find() at runtime! time-save

            // references
            _parent = GameObject.Find("single").transform;
            _all = GameObject.Find("all");
            _closeSurface = GameObject.Find("closeSurface");

            _prefabs = _parent.GetComponentsInChildren<ParticleSystem>().ToList()
                .Where(fx => fx.transform.parent == _parent).ToArray();

            // UI
            _text = GameObject.Find("label_Prefab_name").GetComponent<Text>();
            _particleCountLabel = GameObject.Find("label_particles_count").GetComponent<Text>();

            _left = GameObject.Find("<_btn").GetComponent<Button>();
            _right = GameObject.Find(">_btn").GetComponent<Button>();
            _textureResolution = GameObject.Find("resolution_btn").GetComponent<Button>();

            _showAll = GameObject.Find("toggle_all_single").GetComponent<Toggle>();
            _surface = GameObject.Find("toggle_surface").GetComponent<Toggle>();
            _pause = GameObject.Find("toggle_pause").GetComponent<Toggle>();

            _timeScaleSlide = GameObject.Find("slider_time").GetComponent<Slider>();
            _particleTime = GameObject.Find("slider_particle_time").GetComponent<Slider>();

            EditorUtility.SetDirty(this);
        }
#endif

        #endregion
    }
}