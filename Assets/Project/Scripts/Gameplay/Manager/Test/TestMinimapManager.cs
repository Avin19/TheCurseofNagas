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
        [SerializeField] private Camera _mainCamTerrain;
        [SerializeField] private RenderTexture _minimapRT;
        [SerializeField] private RawImage _minimapImg;
        [SerializeField] private Transform _player;
        private const float yPos = 30f;

        [Header("Minimap Settings")]
        [SerializeField, Range(0.1f, 5f)] private float _minimapRefreshRate = 1;
        [SerializeField, Range(0.1f, 5f)] private float _minimapAddOffsetMult = 1;
        // [SerializeField] private float _scaleFactor = 5;
        [SerializeField] private int _mapDimension = 10, _minimapDimension = 512, _markerDimension = 5;
        [SerializeField] private Color _playerColor;
        [SerializeField] private Transform _obj1;

        [Header("Map-Overhead Camera")]
        [SerializeField] private LayerMask _bgMask;
        [SerializeField] private int _bgRefreshRate = 5;
        // [SerializeField] private 
        private float _bgProjectionSize = 50f;
        private bool _takeOverhead;


        [Header("Player Location Camera")]
        [SerializeField] private LayerMask _playerLocationCullingGroup;
        private float _playerProjectionSize = 15f;

        private Texture2D _bgMapTex;
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
        }

        private void RenderCameraHelper()
        {
            //Get the Texture from _mainCamOther    -> Background transperant for this one
            //Get the Texture from _mainCamTerrain
            //Overlap _mainCamOther to Terrain

            Vector3 finalPos = Vector3.zero;
            _currentRT = RenderTexture.active;
            RenderTexture.active = _minimapRT;

            if (_takeOverhead)
            {
                _takeOverhead = false;

                _mainCamTerrain.cullingMask = _bgMask;
                _mainCamTerrain.orthographicSize = _bgProjectionSize;

                finalPos.y = yPos;
                _mainCamTerrain.transform.position = finalPos;

                _mainCamTerrain.Render();

                var gfxFormat = TextureFormat.ARGB32;
                _bgMapTex.ReadPixels(new Rect(0, 0, _minimapRT.width, _minimapRT.height), 0, 0);
                // _bgMapTex.Apply();
                // SaveImage(image);

                //Reset back to player settings
                _mainCamTerrain.cullingMask = _playerLocationCullingGroup;
                _mainCamTerrain.orthographicSize = _playerProjectionSize;
            }

            finalPos = _player.position;
            finalPos.y = yPos;
            _mainCamTerrain.transform.position = finalPos;

            //Copy an area from the saved bg texture
            // _mainCamTerrain.Render();
            // _bgMapTex.ReadPixels(new Rect(0, 0, _minimapRT.width, _minimapRT.height), 0, 0);

            int minimapffset = (_mapDimension * _mapDimension) / 2;
            float scaleFactor = _minimapDimension / (_mapDimension * _mapDimension);
            // int startX = (int)(_obj1.position.x * _scaleFactor) + minimapffset
            //     , startZ = (int)(_obj1.position.z * _scaleFactor) + minimapffset;

            // int startX = Mathf.CeilToInt(_player.position.x + minimapffset)
            //     , startZ = Mathf.CeilToInt(_player.position.z + minimapffset);
            // _bgMapTex.SetPixel(startX, startZ, _playerColor);

            int startX = Mathf.CeilToInt((_player.position.x + minimapffset) * scaleFactor)
                , startZ = Mathf.CeilToInt((_player.position.z + minimapffset) * scaleFactor);
            // int startX = Mathf.RoundToInt((_player.position.x + minimapffset) * scaleFactor)
            //     , startZ = Mathf.RoundToInt((_player.position.z + minimapffset) * scaleFactor);

            // Debug.Log($"Initial | OgstartX: {(_player.position.x + minimapffset) * scaleFactor} "
            //     + $"| OgstartX: {(_player.position.z + minimapffset) * scaleFactor} | scaleFactor: {scaleFactor}"
            //     + $"| startX: {startX} | startZ: {startZ}");

            // float addXOffset = ((_mapDimension * _mapDimension) - (_player.position.x + minimapffset)) / (_mapDimension * _mapDimension);
            // float addZOffset = ((_mapDimension * _mapDimension) - (_player.position.z + minimapffset)) / (_mapDimension * _mapDimension);

            // startX = (int)(startX * addXOffset);
            // startZ = (int)(startX * addZOffset);
            // Debug.Log($"Final | startX: {startX} | addXOffset: {addXOffset} | startZ: {startZ} | addZOffset: {addZOffset}");

            // startX = (int)(startX * _minimapAddOffsetMult);
            // startZ = (int)(startX * _minimapAddOffsetMult);

            // startZ += _markerDimension / 2;
            // startX += _markerDimension / 2;
            // if (_markerDimension % 2 == 1)
            // {
            //     startX += 1;
            // }

            // for (int i = 0; i < _markerDimension; i++)
            // {
            //     for (int j = 0; j < _markerDimension; j++)
            //     {
            //         _bgMapTex.SetPixel(startX + i, startZ + j, _playerColor);
            //     }
            // }

            for (int i = -_markerDimension; i <= _markerDimension; i++)
            {
                for (int j = -_markerDimension; j <= _markerDimension; j++)
                {
                    _bgMapTex.SetPixel(startX + i, startZ + j, _playerColor);
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
                await Task.Delay(_bgRefreshRate * 1000);
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