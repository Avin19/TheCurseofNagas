// #define PERLIN_NOISE_1

#define DEBUG_POISSON_DISC
// #define DEBUG_TERRAIN_GEN_1
// #define DEBUG_TERRAIN_GEN_2

#define POISSON_EMERGENCY_BREAK_1

using UnityEngine;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;


#if PERLIN_NOISE_1
using CurseOfNaga.Utils;
#endif

using Random = UnityEngine.Random;
using static CurseOfNaga.Global.UniversalConstant;
using CurseOfNaga.Utils;

namespace CurseOfNaga.Gameplay.Environment
{
    // This can be a decoupled script which can take objects from the pool and instantiate them
    public class EnviromentGenerator : MonoBehaviour
    {
        // Randomly spawn the tree asset , brush ,and rock using noise 
        // some point to trigger cutsense  -> waypoints to ruined village where the main chacter will see the cut sence


        [System.Serializable]
        internal class EnvironmentObj
        {
            public EnvironmentType Type;
            public GameObject prefab;
        }

        [SerializeField] private EnvironmentObj[] _environmentPrefabs;

        private PoissonDiscSampler _poissonDiscSampler;
        private CancellationTokenSource _cts;

        private void OnDestroy()
        {
            if (_cts != null) _cts.Cancel();
        }

        private async void Start()
        {
            _cts = new CancellationTokenSource();

            // GenerateEnvironment();

            InitializePoissonData();
            // try
            {
                _poissonDiscSampler = new PoissonDiscSampler(ref _grid);
                // await GeneratePoissonDiscSamples_2();
                GeneratePoissonDiscSamples_2();              //TEST
            }
            // catch (Exception ex)
            {
                // Debug.LogError($"Caught Exception: {ex.Message}");
            }
            if (_cts.IsCancellationRequested) return;
            GenerateGrid();

            MainGameplayManager.Instance.SetGameStatus(GameStatus.LOADED_ENVIRONMENT);

#if PERLIN_NOISE_1
            InitializePerlinValues();
#endif
        }

        //Spawn trees, bushes and rocks
        private void GenerateEnvironment()
        {

        }

        private void Update()
        {
#if DEBUG_POISSON_DISC
            if (UpdatePoissonMap)
            {
                _activeGrid.Clear();

                _poissonTex = new Texture2D(_rows, _cols);              //Memory problems
                _poissonTex.Apply();

                UpdatePoissonMap = false;
                _ = GeneratePoissonDiscSamples_2();
            }
#endif

#if PERLIN_NOISE_1
            if (GenerateNewPerlinMap)
            {
                GenerateNewPerlinMap = false;
                GeneratePerlinMap();
            }
#endif
        }

        [SerializeField] private SpriteRenderer poissonMapPreview;
        private Texture2D _poissonTex;
        // [HideInInspector] private Color[] _poissonPixels;
        /*private void CreatePoissonSpriteRenderer()
        {
            GameObject spriteRenderer = new GameObject("DebugPerlinNoiseRenderer");
            spriteRenderer.transform.parent = transform;
            spriteRenderer.transform.localPosition = Vector3.zero;

            poissonMapPreview = spriteRenderer.AddComponent<SpriteRenderer>();
            poissonMapPreview.sortingLayerID = SortingLayer.NameToID("Environment");
            Debug.Log($"{poissonMapPreview.sortingLayerName}");

            // _poissonPixels = new Color[_rows * _cols];
        }*/

#if DEBUG_POISSON_DISC
        public bool UpdatePoissonMap = false;
#endif

        internal enum LayerType
        {
            EMPTY = 255, TREE = 0, BUSH = 1, GRASS = 2, FLOWER = 3, ROCK = 4,
            TREE_SUB_SPAWNED = 40, BUSH_SUB_SPAWNED = 41, GRASS_SUB_SPAWNED = 42, FLOWER_SUB_SPAWNED = 43, ROCK_SUB_SPAWNED = 44
        }

        [System.Serializable]
        internal struct LayerData
        {
            public bool SkipLayer;
            [Range(1, 20)] public int CellRadius;
            public Color CellColor;                                 // For Texture Preview
            public LayerType CellType;

            public bool SL_SpawnRandom;
            [Range(1, 5)] public int SL_CellRadius;
            [Range(1, 10)] public int SL_KAttempts;
            [Range(1, 5)] public int SL_Rows, SL_Cols;
        }

        [Serializable]
        internal struct POIData
        {
            public Transform PointOfInterest;
            public int PoiRadius;
        }

        [SerializeField] private POIData[] _poiDatas;
        // [IMPORTANT] Array needs to be filled according to LayerType enum.
        [SerializeField] private LayerData[] _layerDatas;
        // [SerializeField] private int _radius = 4;
        [Range(1, 30)][SerializeField] private int _kAttempts = 30;
        [SerializeField] private float _wCellSize;
        [Range(5, 200)][SerializeField] private int _rows = 10, _cols = 10;                   // Original Rows = 180 | Original Cols = 180
        [Range(5, 30)][SerializeField] private int _startSubRows = 10, _startSubCols = 10;
        [Range(0.001f, 1f)][SerializeField] private float _waitIntervalInSec = 0.01f;
        // [Range(1f, 100f)][SerializeField] private float _poiRadius = 2f;


        // private readonly Vector2Int _GridDimensions = new Vector2Int(10, 10);
        private Vector2Int _GridDimensions;

        [SerializeField] private bool _randomSeed = false;
        [SerializeField] private string RandomSeed_2 = "135653245";             //135653245

        private List<int> _activeGrid;
        [HideInInspector] private byte[] _grid;
        // [HideInInspector] private byte[] _grid;

        private void InitializePoissonData()
        {
            // _wCellSize = _radius / Mathf.Sqrt(2);
            _wCellSize = 1;

            _grid = new byte[_rows * _cols];
            _activeGrid = new List<int>();
            _GridDimensions = new Vector2Int(_rows, _cols);

            // Set up the texture
            _poissonTex = new Texture2D(_rows, _cols);
        }

        private int GeneratePoissonDiscSamples_2()              //TEST
        // private async Task<int> GeneratePoissonDiscSamples_2()
        {
            //Intialize all value to default 
            for (int i = 0; i < _rows * _cols; i++)
                _grid[i] = 255;

            if (_randomSeed)
            {
                RandomSeed_2 = $"{System.DateTime.Now.Hour}{System.DateTime.Now.Minute}" +
                                $"{System.DateTime.Now.Second}{System.DateTime.Now.Millisecond}";
            }
            _poissonDiscSampler.UpdateValues(ref _grid, RandomSeed_2);

            int runResult;
            const int START_MID_INDEX = 0;
            // const int START_SUB_ROWS = 10, START_SUB_COLS = 10;
            const bool START_SPAWN_RANDOM_CLUSTER = false;          //If this is not there, then the whole grid will fill up
            int layerDataIndex = -1;
            Debug.Log($"Seed: {RandomSeed_2}");

            #region LAYER

            for (int poiID = 0; poiID < _poiDatas.Length; poiID++)
            // for (int poiID = 0; poiID < 1; poiID++)
            {
                layerDataIndex = (int)_poiDatas[poiID].PointOfInterest.position.x + (_rows / 2)
                    + ((int)_poiDatas[poiID].PointOfInterest.position.z + (_cols / 2)) * _cols;
                // layerDataIndex = (_rows / 2) + (_cols / 2) * _cols;         //TEST

                // Debug.Log($"Position: {_poiDatas[poiID].PointOfInterest.position} | layerDataIndex: {layerDataIndex}");

                for (int layerId = 0; layerId < _layerDatas.Length; layerId++)
                // for (int layerId = 0; layerId < 2; layerId++)
                {
                    if (_layerDatas[layerId].SkipLayer)
                        continue;

                    // /*
                    // runResult = await _poissonDiscSampler.GeneratePoissonDiscSamples(_rows, _cols,
                    runResult = _poissonDiscSampler.GeneratePoissonDiscSamples(_rows, _cols,
                        (byte)_layerDatas[layerId].CellType,     // _layerDatas[layerId].CellColor,
                        _startSubRows, _startSubCols,
                        START_MID_INDEX, layerDataIndex, _layerDatas[layerId].CellRadius,
                        START_SPAWN_RANDOM_CLUSTER, _kAttempts,
                        _poiDatas[poiID].PoiRadius, _waitIntervalInSec);

                    if (runResult == 0)
                    {
                        Debug.LogError($"Error occured while generating");
                    }
                    // */
                }
            }

            #endregion LAYER

            #region SPAWN_SUB_LAYERS

            int subLayersSet = 0;
            for (int layerId = 0; layerId < _layerDatas.Length; layerId++)
            {
                if (_layerDatas[layerId].SL_SpawnRandom)
                    subLayersSet |= (1 << layerId);
            }

            const byte SPAWNED_OFFSET = 40;
            int tempRunCount = 0;
            for (int poiID = 0; poiID < _poiDatas.Length; poiID++)
            // for (int poiID = 0; poiID < 1; poiID++)
            {

                layerDataIndex = (int)_poiDatas[poiID].PointOfInterest.position.x + (_rows / 2)
                    + ((int)_poiDatas[poiID].PointOfInterest.position.z + (_cols / 2)) * _cols;
                // layerDataIndex = (_rows / 2) + (_cols / 2) * _cols;         //TEST

                // Debug.Log($"Position: {_poiDatas[poiID].PointOfInterest.position} | layerDataIndex: {layerDataIndex}");

                for (int gridIndex = 0; gridIndex < _grid.Length;// && tempRunCount < 1;
                    gridIndex++)
                {
                    //Check if the layer has been set to create a sub-layer
                    if (_grid[gridIndex] == 255 || (subLayersSet & (1 << _grid[gridIndex])) == 0)
                        continue;

                    tempRunCount++;

                    // Debug.Log($"Bush Sub-Layer | grid: {_grid[gridIndex]} | gridIndex: {gridIndex} | "
                    // + $"CellRadius: {_layerDatas[(int)LayerType.BUSH].SL_CellRadius}");

                    // runResult = await _poissonDiscSampler.GeneratePoissonDiscSamples(_rows, _cols,
                    runResult = _poissonDiscSampler.GeneratePoissonDiscSamples(_rows, _cols,
                            (byte)(_grid[gridIndex] + SPAWNED_OFFSET),    // _layerDatas[layerId].CellColor,
                            _layerDatas[_grid[gridIndex]].SL_Rows, _layerDatas[_grid[gridIndex]].SL_Cols,
                            layerDataIndex, gridIndex, _layerDatas[_grid[gridIndex]].SL_CellRadius,
                            _layerDatas[_grid[gridIndex]].SL_SpawnRandom, _layerDatas[_grid[gridIndex]].SL_KAttempts,
                            _poiDatas[poiID].PoiRadius, _waitIntervalInSec);
                    _grid[gridIndex] += SPAWNED_OFFSET;

                    if (runResult == 0)
                    {
                        Debug.LogError($"Error occured while generating");
                    }

                    // if (_grid[layerId] == (byte)_layerDatas[1].CellType) { }
                }
            }

            #endregion SPAWN_SUB_LAYERS

            #region SPAWN_REST_AREA

            const float POI_RADIUS = 0f;
            const int FULL_MID_INDEX = 0, FULL_START_OFFSET = -1;
            const int FULL_SUB_ROW = 0, FULL_SUB_COL = 0;
            // const int SUBLAYER_RAND_OFFSET = 0;
            // const int CELL_RADIUS = 1;
            // const bool SPAWN_RANDOM_CLUSTER = true;
            // int subLayerRows = 4, subLayerCols = 4;

            // /*
            for (int layerId = 0; layerId < _layerDatas.Length; layerId++)
            // for (int layerId = 0; layerId < 2; layerId++)
            {
                if (_layerDatas[layerId].SkipLayer)
                    continue;

                runResult = _poissonDiscSampler.GeneratePoissonDiscSamples(_rows, _cols,
                    (byte)_layerDatas[layerId].CellType,     // _layerDatas[layerId].CellColor,
                    FULL_SUB_ROW, FULL_SUB_COL,
                    FULL_MID_INDEX, FULL_START_OFFSET, _layerDatas[layerId].CellRadius,
                    START_SPAWN_RANDOM_CLUSTER, _kAttempts,
                    POI_RADIUS, _waitIntervalInSec);

                if (runResult == 0)
                {
                    Debug.LogError($"Error occured while generating");
                }
            }
            // */
            #endregion SPAWN_REST_AREA

            #region SPAWN_SUB_LAYER_FOR_REST_AREA

            for (int gridIndex = 0; gridIndex < _grid.Length;// && tempRunCount < 1;
                gridIndex++)
            {
                //Check if the layer has been set to create a sub-layer
                if (_grid[gridIndex] == 255 || (subLayersSet & (1 << _grid[gridIndex])) == 0)
                    continue;
                // tempRunCount++;

                // Debug.Log($"Bush Sub-Layer | grid: {_grid[gridIndex]} | gridIndex: {gridIndex} | "
                // + $"CellRadius: {_layerDatas[(int)LayerType.BUSH].SL_CellRadius}");

                // runResult = await _poissonDiscSampler.GeneratePoissonDiscSamples(_rows, _cols,
                runResult = _poissonDiscSampler.GeneratePoissonDiscSamples(_rows, _cols,
                        _grid[gridIndex],    // _layerDatas[layerId].CellColor,
                        _layerDatas[_grid[gridIndex]].SL_Rows, _layerDatas[_grid[gridIndex]].SL_Cols,
                        FULL_MID_INDEX, gridIndex, _layerDatas[_grid[gridIndex]].SL_CellRadius,
                        _layerDatas[_grid[gridIndex]].SL_SpawnRandom, _layerDatas[_grid[gridIndex]].SL_KAttempts,
                        POI_RADIUS, _waitIntervalInSec);

                if (runResult == 0)
                {
                    Debug.LogError($"Error occured while generating");
                }

                // if (_grid[layerId] == (byte)_layerDatas[1].CellType) { }
            }

            #endregion SPAWN_SUB_LAYER_FOR_REST_AREA

            int xIndex = 0, yIndex = 0;
            for (int i = 0; i < _grid.Length; i++)
            {
                xIndex = i % _cols;
                yIndex = i / _cols;

                //[IMPORTANT] This needs to be according to the layer data
                switch (_grid[i])
                {
                    case (byte)LayerType.EMPTY:         //Do Nothing
                        break;

                    case (byte)LayerType.TREE:
                        _poissonTex.SetPixel(xIndex, yIndex, _layerDatas[0].CellColor);
                        break;

                    case (byte)LayerType.BUSH:
                        _poissonTex.SetPixel(xIndex, yIndex, _layerDatas[1].CellColor);
                        break;

                    case (byte)LayerType.GRASS:
                        _poissonTex.SetPixel(xIndex, yIndex, _layerDatas[2].CellColor);
                        break;

                    case (byte)LayerType.FLOWER:
                        _poissonTex.SetPixel(xIndex, yIndex, _layerDatas[3].CellColor);
                        break;

                    case (byte)LayerType.ROCK:
                        _poissonTex.SetPixel(xIndex, yIndex, _layerDatas[2].CellColor);
                        break;
                }
                // if (_grid[i] == (byte)_layerDatas[0].CellType)
                // _poissonTex.SetPixel(xIndex, yIndex, _layerDatas[0].CellColor);
            }

            _poissonTex.Apply();

            poissonMapPreview.sprite = Sprite.Create(_poissonTex, new Rect(0f, 0f, _rows, _cols)
                    , new Vector2(0.5f, 0.5f));

            return 1;
        }

        [SerializeField] private float xOrigin, zOrigin;
        private void GenerateGrid()
        {
            Transform[] objHolders = new Transform[5];
            for (int i = 0; i < 5; i++)
            {
                objHolders[i] = new GameObject($"{(LayerType)i}").transform;
                objHolders[i].parent = transform;
            }

            GameObject objToCreate = null;
            Vector3 finalPos = Vector3.zero;
            int randomObjIndex = 0;
            for (int i = 0; i < _grid.Length; i++)
            {

                switch (_grid[i])
                {
                    //Do Nothing
                    case (byte)LayerType.EMPTY:
                    case (byte)LayerType.TREE_SUB_SPAWNED:
                    case (byte)LayerType.BUSH_SUB_SPAWNED:
                    case (byte)LayerType.GRASS_SUB_SPAWNED:
                    case (byte)LayerType.FLOWER_SUB_SPAWNED:
                    case (byte)LayerType.ROCK_SUB_SPAWNED:
                        continue;

                    case (byte)LayerType.TREE:
                        randomObjIndex = Random.Range(0, 9);
                        objToCreate = Instantiate(_environmentPrefabs[randomObjIndex].prefab, transform);
                        finalPos.y = 1.55f;

                        break;

                    case (byte)LayerType.BUSH:
                        // randomObjIndex = Random.Range(0, 9);
                        objToCreate = Instantiate(_environmentPrefabs[9].prefab, transform);
                        finalPos.y = 0.8f;

                        break;

                    case (byte)LayerType.GRASS:
                        randomObjIndex = Random.Range(10, 14);
                        objToCreate = Instantiate(_environmentPrefabs[randomObjIndex].prefab, transform);
                        finalPos.y = 0.8f;

                        break;

                    case (byte)LayerType.FLOWER:
                        randomObjIndex = Random.Range(14, 18);
                        objToCreate = Instantiate(_environmentPrefabs[randomObjIndex].prefab, transform);
                        finalPos.y = 0.8f;

                        break;

                    case (byte)LayerType.ROCK:
                        // randomObjIndex = Random.Range(14, 18);
                        objToCreate = Instantiate(_environmentPrefabs[(byte)LayerType.ROCK].prefab, transform);

                        break;

                    default:
                        Debug.LogError($"Unknown object: {(LayerType)_grid[i]} | i: {i}");
                        continue;
                }
                objToCreate.name = $"{(LayerType)_grid[i]}_{i}";
                objToCreate.transform.parent = objHolders[_grid[i]];

                // Shift the grid towards lower left, so that it centers on the point-of-interest
                // Offset original Pos and also by grid mid row and col     | (Ver)Row is y, (Hor)Col is x
                finalPos.x = xOrigin - (_GridDimensions.y / 2) + (i % _GridDimensions.y);
                finalPos.z = zOrigin - (_GridDimensions.x / 2) + (i / _GridDimensions.y);
                objToCreate.name += $"_X{i % _GridDimensions.y}_Z{i / _GridDimensions.y}";

                // finalPos.x = i % _GridDimensions.y;       //TEST
                // finalPos.z = i / _GridDimensions.y;       //TEST

                // Debug.Log($"zVal: {i / _GridDimensions.y} | zOrigin: {zOrigin} | _grid: {_grid[i]} | i: {i}");

                objToCreate.transform.localPosition = finalPos;
                objToCreate.transform.localRotation = Quaternion.identity;
            }
        }

#if DEBUG_TERRAIN_GEN_2
        private void InstantitateDebugCube(int xIndex, int yIndex, bool inValid = false)
        {
            Vector3 tempSpawnPos = Vector3.zero;

            GameObject tempObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tempObject.transform.localScale = Vector3.one * 1f;

            tempSpawnPos.x = xIndex;
            tempSpawnPos.y = 0.5f;
            tempSpawnPos.z = yIndex;

            tempObject.transform.localPosition = tempSpawnPos;
            if (inValid)
                tempObject.name = $"Inv_PoissonObj_{xIndex + (yIndex * _GridDimensions.y)}_[{xIndex},{yIndex}]";
            else
                tempObject.name = $"PoissonObj_{xIndex + (yIndex * _GridDimensions.y)}_[{xIndex},{yIndex}]";
        }

        private void DebugPoissonSampleThroughCubes()
        {
            GameObject cube;
            Vector3 finalPos = Vector3.zero;

            for (int i = 0; i < _rows * _cols; i++)
            {
                if (_grid[i] == 255) continue;

                cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = Vector3.one * 0.4f;

                finalPos.x = (i % _rows);
                finalPos.y = 0.5f;
                finalPos.z = (i / _rows) * _rows;
                cube.transform.position = finalPos;
            }
        }
#endif


#if PERLIN_NOISE_1
        [Space(10)]
        [Header("Perlin Values")]
        public bool GenerateNewPerlinMap = false;
        private PerlinNoiseGenerator _perlinNoiseGenerator;

        [SerializeField]
        private Color _water = new Color(0, 0.7933049f, 1f, 1f), _sand = new Color(0, 0.9490887f, 0.3726415f, 1f),
                _grass = new Color(0.4009434f, 1f, 0.4009434f, 1f), _forest = new Color(0f, 0.6698113f, 0f, 1f),
                _mountains = new Color(0.5f, 0.5f, 0.5f, 1f);

        [SerializeField] private SpriteRenderer _perlinPreview;
        [SerializeField] private int _pixWidth = 256, _pixHeight = 256, _scale = 8;

        private void InitializePerlinValues()
        {
            _perlinNoiseGenerator = new PerlinNoiseGenerator(_pixWidth, _pixHeight, _scale, "",
                    _water, _sand, _grass, _forest, _mountains);
            GeneratePerlinMap();
        }

        private void GeneratePerlinMap()
        {
            _perlinNoiseGenerator.SetValues(_pixWidth, _pixHeight, _scale);
            Texture2D noiseTex = _perlinNoiseGenerator.GenerateMap();

            _perlinPreview.sprite = Sprite.Create(noiseTex,
                    new Rect(0f, 0f, _pixWidth, _pixHeight), new Vector2(0.5f, 0.5f));
        }
#endif
    }
}