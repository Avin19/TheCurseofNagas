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
            try
            {
                _poissonDiscSampler = new PoissonDiscSampler(ref _grid);
                await GeneratePoissonDiscSamples_2();
                // GeneratePoissonDiscSamples_2();              //TEST
                // await GeneratePoissonDiscSamples();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Caught Exception: {ex.Message}");
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
                // _ = GeneratePoissonDiscSamples();
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

        internal enum LayerType { EMPTY = 255, TREE = 0, BUSH = 1, GRASS = 2, FLOWER = 3, ROCK = 4 }

        [System.Serializable]
        internal struct LayerData
        {
            [Range(1f, 20f)] public int CellRadius;
            public Color CellColor;                                 // For Texture Preview
            public LayerType CellType;
            public bool SpawnRandomCluster;
        }

        [SerializeField] private LayerData[] _layerDatas;
        // [SerializeField] private int _radius = 4;
        [SerializeField] private int _kAttempts = 30;
        [SerializeField] private float _wCellSize;
        [Range(5f, 150f)][SerializeField] private int _rows = 10, _cols = 10;
        [Range(0.001f, 1f)][SerializeField] private float _waitIntervalInSec = 0.01f;
        [Range(1f, 100f)][SerializeField] private float _poiRadius = 2f;
        private int _activeCount;


        // private readonly Vector2Int _GridDimensions = new Vector2Int(10, 10);
        private Vector2Int _GridDimensions;

        [SerializeField] private bool _randomSeed = false;
        [SerializeField] private string RandomSeed_2 = "135653245";             //135653245

        private List<int> _activeGrid;
        private byte[] _grid;
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

        private async Task<int> GeneratePoissonDiscSamples()
        {
            //Intialize all value to default
            for (int i = 0; i < _rows * _cols; i++)
                _grid[i] = 255;

            if (_randomSeed)
            {
                RandomSeed_2 = $"{System.DateTime.Now.Hour}{System.DateTime.Now.Minute}" +
                                $"{System.DateTime.Now.Second}{System.DateTime.Now.Millisecond}";
            }
            Random.InitState(RandomSeed_2.GetHashCode());
            Debug.Log($"Seed: {RandomSeed_2}");

            //Select a random point from the grid
            // int randIndex = Random.Range(0, _rows * _cols);
            // _grid[randIndex] = 1;
            // _activeGrid.Add(randIndex);

            //Selecting a center point from rows/cols
            // If row * col is even, then need to subtract a row, else not equal            //Wrong
            // If row * col is odd, then can get half                                       //Wrong
            // int randIndex = (_rows * _cols / 2) - ((_rows % 2) * _cols);                 //Wrong

            // This wont work as the middle is to be exlcuded, it needs to be clear
            // int randIndex = (_rows / 2) + ((_cols / 2) * _GridDimensions.x);

            // Starting from 2nd last row
            Vector2 midPointVec;
            int randIndex;

            // _grid[randIndex] = 1;
            // _activeGrid.Add(randIndex);
            // Debug.Log($"Selected randIndex: {randIndex}");

            // Display on Sprite Renderer
            int xIndex, yIndex;

            // xIndex = randIndex % _rows;
            // yIndex = randIndex / _rows;
            // _poissonTex.SetPixel(xIndex, yIndex, Color.blue);
            // InstantitateDebugCube(xIndex, yIndex);

            // Loop active list | Check valid neighbour | Add List | Remove if not valid
            Vector2 randomOffsetVec, currentVec;
            bool withinDistance, foundCell;
            int randomAngle, additionalOffset;

            System.Text.StringBuilder debugStr = new System.Text.StringBuilder();

            //Layer 0: For trees | Layer 1: For Bushes
            for (int layerId = 0; layerId < _layerDatas.Length; layerId++)
            {
                // Starting from 2nd last row
                midPointVec = new Vector3(_rows / 2, _cols / 2);
                randIndex = 3 + layerId;        // + _GridDimensions.y;
                _grid[randIndex] = (byte)_layerDatas[layerId].CellType;
                _activeGrid.Add(randIndex);

                // Display on Sprite Renderer
                xIndex = yIndex = 0;

                xIndex = randIndex % _rows;
                yIndex = randIndex / _rows;
                // _poissonTex.SetPixel(xIndex, yIndex, Color.blue);
                _poissonTex.SetPixel(xIndex, yIndex, _layerDatas[layerId].CellColor);

                randomOffsetVec = currentVec = Vector2.zero;

                withinDistance = foundCell = false;
                randomAngle = additionalOffset = -1;

#if POISSON_EMERGENCY_BREAK_1
                int emergencyBreak = 0;
#endif

                // Debug.Log($"Starting Check | Time: {System.DateTime.Now.Minute}m {System.DateTime.Now.Second}s "
                //     + $"{System.DateTime.Now.Millisecond}ms {System.DateTime.Now.Ticks}");
                while (_activeGrid.Count > 0)
                {
#if POISSON_EMERGENCY_BREAK_1
                    emergencyBreak++;
                    if (emergencyBreak > 2000)
                    {
                        Debug.LogError($"Emergency Break: {emergencyBreak} | List count: {_activeGrid.Count}");
                        break;
                    }
#endif

                    //Choose a random point from active list
                    randIndex = Random.Range(0, _activeGrid.Count);
                    _activeCount = _activeGrid.Count;
                    // Debug.Log($"randIndex: {_activeGrid[randIndex]} | Count: {_activeGrid.Count}");

                    //Generate upto k points between r and 2r
                    for (int i = 0; i < _kAttempts; i++)
                    {
                        withinDistance = false;

                        // Get a random point
                        randomAngle = Random.Range(0, 360);
                        randomOffsetVec.x = Mathf.Cos(randomAngle);
                        randomOffsetVec.y = Mathf.Sin(randomAngle);

#if DEBUG_TERRAIN_GEN_1
                        debugStr.Clear();
                        debugStr.Append($"Initial Vec: {randomOffsetVec} | ");
#endif

                        // Offset more to be between r and 2r
                        additionalOffset = Random.Range(_layerDatas[layerId].CellRadius, (2 * _layerDatas[layerId].CellRadius) + 1);
                        randomOffsetVec *= additionalOffset;

                        // https://www.desmos.com/calculator/fshadmxjaa - Checking
                        // xIndex = _activeGrid[randIndex] % _rows + Mathf.FloorToInt(randomOffsetVec.x / _wCellSize);
                        // yIndex = _activeGrid[randIndex] / _rows + Mathf.FloorToInt(randomOffsetVec.y / _wCellSize);

                        //_wCellSize will only be needed to calculate the position afterwards, not now
                        xIndex = (_activeGrid[randIndex] % _rows) + Mathf.FloorToInt(randomOffsetVec.x);
                        yIndex = (_activeGrid[randIndex] / _rows) + Mathf.FloorToInt(randomOffsetVec.y);
#if DEBUG_TERRAIN_GEN_1
                        debugStr.Append($"Offset Vec: [{randomOffsetVec.x}, {randomOffsetVec.y}] | additionalOffset: {additionalOffset} | ");
                        debugStr.Append($"Floor X: {Mathf.FloorToInt(randomOffsetVec.x)} | Floor Y: {Mathf.FloorToInt(randomOffsetVec.y)} | ");
                        debugStr.Append($"xIndex: {xIndex} | yIndex: {yIndex} | ");
                        // Debug.Log($"Random Vec: {randomOffsetVec} | additionalOffset: {additionalOffset}"
                        //         + $" | xIndex: {xIndex} | yIndex: {yIndex}");
#endif

                        // Updating offset co-ords to current active cell
                        randomOffsetVec.x = xIndex;
                        randomOffsetVec.y = yIndex;

                        //Bounds Check
                        if (xIndex < 0 || yIndex < 0 || xIndex >= _GridDimensions.x || yIndex >= _GridDimensions.y
                            || (Vector3.SqrMagnitude(randomOffsetVec - midPointVec) - (_poiRadius * _poiRadius)) <= 0           // Also check if it is not inside the point of interest
                            || _grid[xIndex + (yIndex * _GridDimensions.y)] != 255)             //Cell occupied by something
                        {
#if DEBUG_TERRAIN_GEN_1
                            debugStr.Append($"Outside Bounds");
                            // Debug.Log($"Outside Bounds: {debugStr}");
#endif
                            continue;
                        }

#if DEBUG_TERRAIN_GEN_1
                        debugStr.Append($"SqrDist: {Vector3.SqrMagnitude(randomOffsetVec - midPointVec)} | ");
#endif

                        // Check if the space(r) is enough between points and no neigbours are within it
                        // Also checking if the spot itself is valid or not
                        for (int hor = _layerDatas[layerId].CellRadius * -1;
                            hor <= _layerDatas[layerId].CellRadius && !withinDistance; hor++)              // TODONE: Fix range as this should check to r
                        {
                            for (int ver = _layerDatas[layerId].CellRadius * -1;
                                ver <= _layerDatas[layerId].CellRadius && !withinDistance; ver++)              // TODONE: Fix range as this should check to r
                            {
#if DEBUG_TERRAIN_GEN_1
                                debugStr.Append($"\nhor: {hor} | ver : {ver} | ");
                                // Debug.Log($"{debugStr}");               //Test
#endif

                                //Bounds Check
                                if ((xIndex + hor) < 0 || (yIndex + ver) < 0
                                    || (xIndex + hor) >= _GridDimensions.x || (yIndex + ver) >= _GridDimensions.y)
                                    continue;

                                int neighbourIndex = (xIndex + hor) + ((yIndex + ver) * _GridDimensions.y);

#if DEBUG_TERRAIN_GEN_1
                                debugStr.Append($"neighbourIndex: {neighbourIndex} | _GridDimensions: {_GridDimensions} | _grid Val: {_grid[neighbourIndex]} | ");
                                // Debug.Log($"neighbourIndex: {neighbourIndex} | xIndex: {xIndex} | yIndex: {yIndex}"
                                //         + $" | hor: {hor} | ver : {ver} | _GridDimensions: {_GridDimensions}");
#endif

                                if (_grid[neighbourIndex] != 255
                                    && _grid[neighbourIndex] == (byte)_layerDatas[layerId].CellType)
                                {
                                    withinDistance = true;
                                }

                                /*
                                if (_grid[neighbourIndex] == 255) continue;

                                //Checking the distance
                                currentVec.x = xIndex + hor;
                                currentVec.y = yIndex + ver;
                                // float distance = _grid[neighbourIndex] - _grid[xIndex + (yIndex * _GridDimensions.x)];
                                float distance = Vector2.SqrMagnitude(randomOffsetVec - currentVec);

                                if (distance < _radius * _radius)
                                {
#if DEBUG_TERRAIN_GEN_1
                                    // Debug.Log($"CurrentVec: [{currentVec.x}, {currentVec.y}] | randomOffsetVec: [{randomOffsetVec.x}, {randomOffsetVec.y}]");
                                    debugStr.Append($" Dist: {distance} | ");
                                    // InstantitateDebugCube((xIndex + hor), (yIndex + ver), true);
#endif

                                    withinDistance = true;

                                }
                                */
                            }
                        }

                        if (!withinDistance)
                        {
                            // Debug.Log($"Adding index: {xIndex + (yIndex * _GridDimensions.x)}| xIndex {xIndex} | yIndex: {yIndex}");
                            // debugStr.Append($"Added");
                            foundCell = true;
                            _grid[xIndex + (yIndex * _GridDimensions.y)] = (byte)_layerDatas[layerId].CellType;
                            _activeGrid.Add(xIndex + (yIndex * _GridDimensions.y));

                            // _poissonTex.SetPixel(xIndex, yIndex, Color.blue);
                            _poissonTex.SetPixel(xIndex, yIndex, _layerDatas[layerId].CellColor);
                            // InstantitateDebugCube(xIndex, yIndex);

                            // Random chance to spawn cluster of objects
                            if (_layerDatas[layerId].SpawnRandomCluster)
                            {
                                int randomCluster = Random.Range(0, 10);

                                if (randomCluster > 2 && randomCluster < 5)
                                {
                                    //Cover the 1-cell radius fully or randomly some cells only?
                                    for (int hor = -1; hor <= 1; hor++)
                                    {
                                        for (int ver = -1; ver <= 1; ver++)
                                        {
                                            //Bounds Check
                                            if ((xIndex + hor) < 0 || (yIndex + ver) < 0
                                                || (xIndex + hor) >= _GridDimensions.x || (yIndex + ver) >= _GridDimensions.y
                                                || _grid[(xIndex + hor) + ((yIndex + ver) * _GridDimensions.y)] != 255)             //Check if cell is occupied or not
                                                continue;

                                            _grid[(xIndex + hor) + ((yIndex + ver) * _GridDimensions.y)] = (byte)_layerDatas[layerId].CellType;
                                            _poissonTex.SetPixel((xIndex + hor), (yIndex + ver), _layerDatas[layerId].CellColor);
                                        }
                                    }
                                }
                            }

                            // break;
                        }
#if DEBUG_TERRAIN_GEN_1
                        Debug.Log($"{debugStr}");
#endif
                    }

                    if (!foundCell)
                    {
#if DEBUG_TERRAIN_GEN_1
                        // Debug.Log($"Removing from active | index: {randIndex} | Val: {_activeGrid[randIndex]}");
#endif
                        _activeGrid.RemoveAt(randIndex);
                    }
                    foundCell = false;

                    await Task.Delay((int)(_waitIntervalInSec * 1000));

                    if (_cts.IsCancellationRequested) return 0;
                }
                // Debug.Log($"Finish Check | Time: {System.DateTime.Now.Minute}m {System.DateTime.Now.Second}s "
                //     + $"{System.DateTime.Now.Millisecond}ms {System.DateTime.Now.Ticks}");

#if POISSON_EMERGENCY_BREAK_1
                Debug.Log($"Emergency Break Count: {emergencyBreak}");               //FOR DEBUG
#endif
            }

            _poissonTex.Apply();

            poissonMapPreview.sprite = Sprite.Create(_poissonTex, new Rect(0f, 0f, _rows, _cols)
                    , new Vector2(0.5f, 0.5f));

            return 1;
        }

        // private int GeneratePoissonDiscSamples_2()              //TEST
        private async Task<int> GeneratePoissonDiscSamples_2()
        {
            //Intialize all value to default 
            for (int i = 0; i < _rows * _cols; i++)
                _grid[i] = 255;

            if (_randomSeed)
            {
                RandomSeed_2 = $"{System.DateTime.Now.Hour}{System.DateTime.Now.Minute}" +
                                $"{System.DateTime.Now.Second}{System.DateTime.Now.Millisecond}";
            }

            Debug.Log($"Seed: {RandomSeed_2}");

            for (int layerId = 0; layerId < _layerDatas.Length; layerId++)
            {
                // _poissonTex = _poissonDiscSampler.GeneratePoissonDiscSamples(_rows, _cols, (byte)_layerDatas[0].CellType,
                _poissonTex = await _poissonDiscSampler.GeneratePoissonDiscSamples(_rows, _cols, (byte)_layerDatas[layerId].CellType,
                        _layerDatas[layerId].CellColor, layerId, _layerDatas[layerId].CellRadius,
                        _layerDatas[layerId].SpawnRandomCluster, _kAttempts, _poiRadius, _waitIntervalInSec, RandomSeed_2);

                if (_poissonTex == null) return 0;

                /*
                int xIndex = 0, yIndex = 0;
                for (int i = 0; i < _grid.Length; i++)
                {
                    xIndex = i % _cols;
                    yIndex = i / _cols;
                    if (_grid[i] == (byte)_layerDatas[0].CellType)
                        _poissonTex.SetPixel(xIndex, yIndex, _layerDatas[0].CellColor);
                }
                */

                _poissonTex.Apply();
            }

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
                        continue;

                    case (byte)LayerType.TREE:
                        randomObjIndex = Random.Range(0, 9);
                        objToCreate = Instantiate(_environmentPrefabs[randomObjIndex].prefab, transform);
                        finalPos.y = 1.55f;
                        objToCreate.name = $"{LayerType.TREE}_{i}";
                        objToCreate.transform.parent = objHolders[(int)LayerType.TREE];

                        break;

                    case (byte)LayerType.BUSH:
                        // randomObjIndex = Random.Range(0, 9);
                        objToCreate = Instantiate(_environmentPrefabs[9].prefab, transform);
                        finalPos.y = 0.8f;
                        objToCreate.name = $"{LayerType.BUSH}_{i}";
                        objToCreate.transform.parent = objHolders[(int)LayerType.BUSH];

                        break;

                    case (byte)LayerType.GRASS:
                        randomObjIndex = Random.Range(10, 14);
                        objToCreate = Instantiate(_environmentPrefabs[randomObjIndex].prefab, transform);
                        finalPos.y = 0.8f;
                        objToCreate.name = $"{LayerType.GRASS}_{i}";
                        objToCreate.transform.parent = objHolders[(int)LayerType.GRASS];

                        break;

                    case (byte)LayerType.FLOWER:
                        randomObjIndex = Random.Range(14, 18);
                        objToCreate = Instantiate(_environmentPrefabs[randomObjIndex].prefab, transform);
                        finalPos.y = 0.8f;
                        objToCreate.name = $"{LayerType.FLOWER}_{i}";
                        objToCreate.transform.parent = objHolders[(int)LayerType.FLOWER];

                        break;

                    case (byte)LayerType.ROCK:
                        // randomObjIndex = Random.Range(14, 18);
                        objToCreate = Instantiate(_environmentPrefabs[(byte)LayerType.ROCK].prefab, transform);
                        objToCreate.name = $"{LayerType.ROCK}_{i}";
                        objToCreate.transform.parent = objHolders[(int)LayerType.ROCK];

                        break;

                    default:
                        Debug.LogError($"Unknown object: {(LayerType)_grid[i]} | i: {i}");
                        continue;
                }

                // Shift the grid towards lower left, so that it centers on the point-of-interest
                // Offset original Pos and also by grid mid row and col     | (Ver)Row is y, (Hor)Col is x
                finalPos.x = xOrigin - (_GridDimensions.y / 2) + (i % _GridDimensions.y);
                finalPos.z = zOrigin - (_GridDimensions.x / 2) + (i / _GridDimensions.y);
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