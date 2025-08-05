using System.IO;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Experimental.Rendering;
using System.Threading;
using System.Threading.Tasks;

namespace CurseOfNaga.Gameplay.Managers.Test
{
    public class TestMinimapManager : MonoBehaviour
    {
        [System.Serializable]
        class TrackableEntity
        {
            public Color EnColor;
            public Transform Entity;
            public Vector2Int EnPreviousPos;                // 1 Vec2Int = 8 B | 1MiB = 10,00,000 B = 1,25,0000 Vec2Int
        }

        [SerializeField] private Camera _mainCamTerrain;
        [SerializeField] private RenderTexture _minimapRT;
        [SerializeField] private RawImage _minimapImg;
        [SerializeField] private TrackableEntity _player;
        [SerializeField] private TrackableEntity[] _trackables;
        private const float yPos = 30f;

        [Header("Minimap Settings")]
        [SerializeField, Range(0.1f, 5f)] private float _minimapRefreshRate = 1;
        // [SerializeField, Range(0.1f, 5f)] private float _minimapAddOffsetMult = 1;
        // [SerializeField] private float _scaleFactor = 5;
        [SerializeField] private int _mapDimension = 10, _minimapDimension = 512, _markerDimension = 5;
        private float _currentTime = 0f;

        [Header("Map-Overhead Camera")]
        [SerializeField] private LayerMask _bgMask;
        [SerializeField, Range(0.1f, 5f)] private float _bgRefreshRate = 5;
        // [SerializeField] private 
        private float _bgProjectionSize = 50f;
        private bool _takeOverhead;


        [Header("Player Location Camera")]
        [SerializeField] private LayerMask _playerLocationCullingGroup;
        private float _playerProjectionSize = 15f;
        private Vector2Int _playerPrevPos;         //Record previous position to replace with bgTex

        private Texture2D _bgMapTex, _ogBgMapTex;
        private RenderTexture _currentRT;
        public bool updateRender = false;

        private CancellationTokenSource _cts;

        private void OnDestroy()
        {
            _cts.Cancel();
        }

        private void Start()
        {
            _cts = new CancellationTokenSource();

            _bgMapTex = new Texture2D(_minimapRT.width, _minimapRT.height, _minimapRT.graphicsFormat, TextureCreationFlags.None);
            _ogBgMapTex = new Texture2D(_minimapRT.width, _minimapRT.height, _minimapRT.graphicsFormat, TextureCreationFlags.None);
            //_bgMapTex = new Texture2D(_minimapRT.width, _minimapRT.height, gfxFormat, false, true);

            _takeOverhead = true;
            UpdateMinimap();
            UpdateBGTexture();
        }

        private void Update()
        {
            if (updateRender)
            {
                updateRender = false;
                RenderCameraHelper();
            }

            _currentTime += Time.deltaTime;
            if (_currentTime >= _minimapRefreshRate)
            {
                RenderCameraHelper();
                _currentTime = 0f;
            }
        }

        private void RenderCameraHelper()
        {
            //Get the Texture from _mainCamOther    -> Background transperant for this one
            //Get the Texture from _mainCamTerrain
            //Overlap _mainCamOther to Terrain

            Vector3 finalPos = Vector3.zero;
            _currentRT = RenderTexture.active;
            RenderTexture.active = _minimapRT;

            //This is expensive
            if (_takeOverhead)
            {
                _takeOverhead = false;

                _mainCamTerrain.cullingMask = _bgMask;
                _mainCamTerrain.orthographicSize = _bgProjectionSize;

                finalPos.y = yPos;
                _mainCamTerrain.transform.position = finalPos;

                _mainCamTerrain.Render();

                // var gfxFormat = TextureFormat.ARGB32;
                _ogBgMapTex.ReadPixels(new Rect(0, 0, _minimapRT.width, _minimapRT.height), 0, 0);
                _bgMapTex.ReadPixels(new Rect(0, 0, _minimapRT.width, _minimapRT.height), 0, 0);
                // _bgMapTex.Apply();
                // SaveImage(image);

                //Reset back to player settings
                _mainCamTerrain.cullingMask = _playerLocationCullingGroup;
                _mainCamTerrain.orthographicSize = _playerProjectionSize;
            }

            finalPos = _player.Entity.position;
            finalPos.y = yPos;
            _mainCamTerrain.transform.position = finalPos;

            //Copy an area from the saved bg texture
            // _mainCamTerrain.Render();
            // _bgMapTex.ReadPixels(new Rect(0, 0, _minimapRT.width, _minimapRT.height), 0, 0);

            int minimapffset = (_mapDimension * _mapDimension) / 2;
            float scaleFactor = (float)_minimapDimension / (_mapDimension * _mapDimension);

            for (int trackableID = 0; trackableID < _trackables.Length; trackableID++)
            {
                // Clear previous set pixels
                for (int i = -_markerDimension; i <= _markerDimension; i++)
                {
                    for (int j = -_markerDimension; j <= _markerDimension; j++)
                    {
                        // _bgMapTex.SetPixel(_playerPrevPos.x + i, _playerPrevPos.y + j,
                        //     _ogBgMapTex.GetPixel(_playerPrevPos.x + i, _playerPrevPos.y + j));

                        _bgMapTex.SetPixel(_trackables[trackableID].EnPreviousPos.x + i, _trackables[trackableID].EnPreviousPos.y + j,
                            _ogBgMapTex.GetPixel(_trackables[trackableID].EnPreviousPos.x + i, _trackables[trackableID].EnPreviousPos.y + j));
                    }
                }

                // _playerPrevPos.x = Mathf.RoundToInt((_player.Entity.position.x + minimapffset) * scaleFactor);
                // _playerPrevPos.y = Mathf.RoundToInt((_player.Entity.position.z + minimapffset) * scaleFactor);

                _trackables[trackableID].EnPreviousPos.x = Mathf.RoundToInt((_trackables[trackableID].Entity.position.x + minimapffset) * scaleFactor);
                _trackables[trackableID].EnPreviousPos.y = Mathf.RoundToInt((_trackables[trackableID].Entity.position.z + minimapffset) * scaleFactor);

                // Debug.Log($"Initial | OgstartX: {(_player.Entity.position.x + minimapffset) * scaleFactor} "
                //     + $"| OgstartX: {(_player.Entity.position.z + minimapffset) * scaleFactor} | scaleFactor: {scaleFactor}"
                //     + $"| startX: {_playerPrevPos.x} | startZ: {_playerPrevPos.y}");

                // float addXOffset = ((_mapDimension * _mapDimension) - (_player.Entity.position.x + minimapffset)) / (_mapDimension * _mapDimension);
                // float addZOffset = ((_mapDimension * _mapDimension) - (_player.Entity.position.z + minimapffset)) / (_mapDimension * _mapDimension);

                // _playerPrevPos.x = (int)(_playerPrevPos.x * addXOffset);
                // _playerPrevPos.y = (int)(_playerPrevPos.y * addZOffset);
                // Debug.Log($"Final | startX: {startX} | addXOffset: {addXOffset} | startZ: {startZ} " 
                //      + $"| addZOffset: {addZOffset}");

                // _playerPrevPos.x = (int)(_playerPrevPos.x * _minimapAddOffsetMult);
                // _playerPrevPos.y = (int)(_playerPrevPos.y * _minimapAddOffsetMult);

                for (int i = -_markerDimension; i <= _markerDimension; i++)
                {
                    for (int j = -_markerDimension; j <= _markerDimension; j++)
                    {
                        // _bgMapTex.SetPixel(_playerPrevPos.x + i, _playerPrevPos.y + j, _player.EnColor);
                        _bgMapTex.SetPixel(_trackables[trackableID].EnPreviousPos.x + i,
                            _trackables[trackableID].EnPreviousPos.y + j, _trackables[trackableID].EnColor);
                    }
                }
            }
            _bgMapTex.Apply();

            _minimapImg.texture = _bgMapTex;
            RenderTexture.active = _currentRT;
        }

        private async void UpdateMinimap()
        {
            while (true)
            {
                await Task.Delay((int)(_minimapRefreshRate * 1000));
                if (_cts.IsCancellationRequested) return;

                RenderCameraHelper();
            }
        }

        private async void UpdateBGTexture()
        {
            while (true)
            {
                await Task.Delay((int)(_bgRefreshRate * 1000));
                if (_cts.IsCancellationRequested) return;

                _takeOverhead = true;
            }
        }

        private void SaveImage(Texture2D textureToSave)
        {
            byte[] imgData = textureToSave.EncodeToPNG();
            string timeNow = $"{System.DateTime.Now.Hour}_{System.DateTime.Now.Minute}_{System.DateTime.Now.Second}";
            string filePath = Path.Combine(Application.persistentDataPath, $"Img_{timeNow}.png");

            FileStream saveStream = File.Create(filePath);
            saveStream.Write(imgData);
            saveStream.Close();
            Debug.Log($"Saved File to: {filePath}");
        }
    }
}