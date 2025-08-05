#define TESTING

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace CurseOfNaga.Gameplay.Managers
{
    [System.Serializable]
    public class TrackableEntity
    {
        public Color EnColor;
        public Transform Entity;
        //Record previous position to replace with bgTex
        public Vector2Int EnPreviousPos;                // 1 Vec2Int = 8 B | 1MiB = 10,00,000 B = 1,25,0000 Vec2Int
    }

    [System.Serializable]
    public class MiniMapManager
    {

        public List<TrackableEntity> Trackables;

        private Camera _orthoCam;
        private RenderTexture _mainRenderTex;
        private UnityEngine.UI.RawImage _minimapImg;
        // private const float _MINIMAP_CAM_YPOS = 30f;

        [Header("Minimap Settings")]
        private float _mapRefreshRate = 1;
        private int _markerDimension = 5;        //TODO: Make this constant
        private const int _MAP_DIMENSION = 18, _MINI_MAP_DIMENSION = 512;
        private float _currentTime = 0f;

        [Header("Map-Overhead Camera")]
        private float _bgRefreshRate = 5;             //TODO: Make this constant
        private bool _takeOverhead;
        // [SerializeField] private LayerMask _bgMask;                 //TODO: Make this constant
        // private const float _bgProjectionSize = 50f;

        // [Header("Player Location Camera")]
        // [SerializeField] private Transform _player;
        // [SerializeField] private LayerMask _normalMask;             //TODO: Make this constant
        // private const float _playerProjectionSize = 15f;

        private Texture2D _bgMapTex, _ogBgMapTex;
        private RenderTexture _currentRT;

#if TESTING
        public bool updateRender = false;
#endif

        private CancellationTokenSource _cts;

        ~MiniMapManager()
        {
            _cts.Cancel();
        }

        public void Initialize(Camera minimapOrthoCam, RenderTexture minimapRenderTex,
            UnityEngine.UI.RawImage minimapImg, ref float minimapRefreshRate,
            ref float bgRefreshRate, ref int markerDimension)
        {
            _cts = new CancellationTokenSource();

            _orthoCam = minimapOrthoCam;
            _mainRenderTex = minimapRenderTex;
            _minimapImg = minimapImg;
            _mapRefreshRate = minimapRefreshRate;
            _bgRefreshRate = bgRefreshRate;
            _markerDimension = markerDimension;

            _bgMapTex = new Texture2D(_mainRenderTex.width, _mainRenderTex.height, _mainRenderTex.graphicsFormat, TextureCreationFlags.None);
            _ogBgMapTex = new Texture2D(_mainRenderTex.width, _mainRenderTex.height, _mainRenderTex.graphicsFormat, TextureCreationFlags.None);
            //_bgMapTex = new Texture2D(_minimapRT.width, _minimapRT.height, gfxFormat, false, true);

            _takeOverhead = true;
            // UpdateMinimap();
            UpdateBGTexture();
        }

        public void Update()
        {
#if TESTING
            if (updateRender)
            {
                updateRender = false;
                RenderCameraHelper();
            }
#endif

            _currentTime += Time.deltaTime;
            if (_currentTime >= _mapRefreshRate)
            {
                RenderCameraHelper();
                _currentTime = 0f;
            }
        }

        private void RenderCameraHelper()
        {
            //Get the BG Texture from Camera
            //Save and copy the Texture from Camera
            //Set pixels in the Texture using entity positions

            _currentRT = RenderTexture.active;
            RenderTexture.active = _mainRenderTex;

            //This is expensive
            if (_takeOverhead)
            {
                _takeOverhead = false;

                // _minimapOrthoCam.cullingMask = _bgMask;
                // _minimapOrthoCam.orthographicSize = _bgProjectionSize;

                _orthoCam.Render();

                _ogBgMapTex.ReadPixels(new Rect(0, 0, _mainRenderTex.width, _mainRenderTex.height), 0, 0);
                _bgMapTex.ReadPixels(new Rect(0, 0, _mainRenderTex.width, _mainRenderTex.height), 0, 0);

                //Reset back to player settings
                // _minimapOrthoCam.cullingMask = _normalMask;
                // _minimapOrthoCam.orthographicSize = _playerProjectionSize;
            }

            // Vector3 finalPos = _player.position;
            // finalPos.y = _MINIMAP_CAM_YPOS;
            // _minimapOrthoCam.transform.position = finalPos;

            int minimapffset = (_MAP_DIMENSION * _MAP_DIMENSION) / 2;
            float scaleFactor = (float)_MINI_MAP_DIMENSION / (_MAP_DIMENSION * _MAP_DIMENSION);

            for (int trackableID = 0; trackableID < Trackables.Count; trackableID++)
            {
                // Clear previous set pixels
                for (int i = -_markerDimension; i <= _markerDimension; i++)
                {
                    for (int j = -_markerDimension; j <= _markerDimension; j++)
                    {
                        _bgMapTex.SetPixel(Trackables[trackableID].EnPreviousPos.x + i, Trackables[trackableID].EnPreviousPos.y + j,
                            _ogBgMapTex.GetPixel(Trackables[trackableID].EnPreviousPos.x + i, Trackables[trackableID].EnPreviousPos.y + j));
                    }
                }

                Trackables[trackableID].EnPreviousPos.x = Mathf.RoundToInt((Trackables[trackableID].Entity.position.x + minimapffset) * scaleFactor);
                Trackables[trackableID].EnPreviousPos.y = Mathf.RoundToInt((Trackables[trackableID].Entity.position.z + minimapffset) * scaleFactor);

                // Debug.Log($"Initial | OgstartX: {(_player.Entity.position.x + minimapffset) * scaleFactor} "
                //     + $"| OgstartX: {(_player.Entity.position.z + minimapffset) * scaleFactor} | scaleFactor: {scaleFactor}"
                //     + $"| startX: {_playerPrevPos.x} | startZ: {_playerPrevPos.y}");

                for (int i = -_markerDimension; i <= _markerDimension; i++)
                {
                    for (int j = -_markerDimension; j <= _markerDimension; j++)
                    {
                        _bgMapTex.SetPixel(Trackables[trackableID].EnPreviousPos.x + i,
                            Trackables[trackableID].EnPreviousPos.y + j, Trackables[trackableID].EnColor);
                    }
                }
            }
            _bgMapTex.Apply();

            _minimapImg.texture = _bgMapTex;
            RenderTexture.active = _currentRT;
        }

#if TESTING
        private async void UpdateMinimap()
        {
            while (true)
            {
                await Task.Delay((int)(_mapRefreshRate * 1000));
                if (_cts.IsCancellationRequested) return;

                RenderCameraHelper();
            }
        }
#endif

        private async void UpdateBGTexture()
        {
            while (true)
            {
                await Task.Delay((int)(_bgRefreshRate * 1000));
                if (_cts.IsCancellationRequested) return;

                _takeOverhead = true;
            }
        }
    }
}