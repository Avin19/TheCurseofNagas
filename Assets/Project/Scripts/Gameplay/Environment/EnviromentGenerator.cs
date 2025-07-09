// #define PERLIN_NOISE_1
#define DEBUG_POISSON_DISC
#define DEBUG_TERRAIN_GEN_1

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Environment
{
    // This can be a decoupled script which can take objects from the pool and instantiate them
    public class EnviromentGenerator : MonoBehaviour
    {
        // Randomly spawn the tree asset , brush ,and rock using noise 
        // some point to trigger cutsense  -> waypoints to ruined village where the main chacter will see the cut sence

        internal enum EnvironmentType
        {
            TREE_1 = 0, TREE_2, TREE_3, TREE_4, TREE_5, TREE_6, TREE_7, TREE_8, TREE_9,
            BUSH_1,
            GRASS_1, GRASS_2, GRASS_3, GRASS_4,
            FLOWER_1, FLOWER_2, FLOWER_3, FLOWER_4,
            ROCK_1
        }

        [System.Serializable]
        internal class EnvironmentObj
        {
            public EnvironmentType Type;
            public GameObject prefab;
        }

        [SerializeField] private EnvironmentObj[] _environmentPrefabs;

        private CancellationTokenSource _cts;

        private void OnDestroy()
        {
            if (_cts != null) _cts.Cancel();
        }

        private void Start()
        {
            _cts = new CancellationTokenSource();

            // GenerateEnvironment();

            InitializePoissonData();
            GeneratePoissonDiscSamples();

            MainGameplayManager.Instance.SetGameStatus(GameStatus.LOADED_ENVIRONMENT);

#if PERLIN_NOISE_1
            CreateSpriteRenderer();
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
                GeneratePoissonDiscSamples();
            }
#endif

#if PERLIN_NOISE_1
            if (GenerateNewMap)
            {
                GenerateNewMap = false;
                Generatemap();
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

        [SerializeField] private int _radius = 4;
        [SerializeField] private int _kAttempts = 30;
        [SerializeField] private float _wCellSize;
        [SerializeField] private int _rows = 10, _cols = 10;
        [SerializeField] private int _waitTimeInSec = 1;


        // private readonly Vector2Int _GridDimensions = new Vector2Int(10, 10);
        private Vector2Int _GridDimensions;

        [SerializeField] private bool _randomSeed = false;
        [SerializeField] private string RandomSeed_2 = "135653245";             //135653245

        private List<int> _activeGrid;
        [HideInInspector] private byte[] _grid;

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

        private async void GeneratePoissonDiscSamples()
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
            int randIndex = Random.Range(0, _rows * _cols);
            _grid[randIndex] = 1;
            _activeGrid.Add(randIndex);

            // Display on Sprite Renderer
            int xIndex = 0, yIndex = 0;

            xIndex = randIndex % _rows;
            yIndex = randIndex / _rows;
            _poissonTex.SetPixel(xIndex, yIndex, Color.white);

            // InstantitateDebugCube(xIndex, yIndex);

            // for (int i = 0; i < _grid.Length; i++)
            // {
            //     if (_grid[i] == 255) continue;

            //     xIndex = i % _rows;
            //     // yIndex = (_activeGrid[i] / _rows) * _rows;
            //     yIndex = i / _rows;
            //     _poissonTex.SetPixel(xIndex, yIndex, Color.white);
            // }

            // Loop active list | Check valid neighbour | Add List | Remove if not valid
            Vector2 randomOffsetVec = Vector2.zero;
            Vector2 currentVec = Vector2.zero;
            bool withinDistance = false, foundCell = false;
            int randomAngle = -1, additionalOffset = -1;

            System.Text.StringBuilder debugStr = new System.Text.StringBuilder();

            int emergencyBreak = 0;
            while (_activeGrid.Count > 0)
            {
                emergencyBreak++;
                if (emergencyBreak > 200)
                {
                    Debug.LogError($"Emergency Break: {emergencyBreak} | List count: {_activeGrid.Count}");
                    break;
                }

                //Choose a random point from active list
                randIndex = Random.Range(0, _activeGrid.Count);
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
                    additionalOffset = Random.Range(_radius, (2 * _radius) + 1);
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

                    //Bounds Check
                    if (xIndex < 0 || yIndex < 0 || xIndex >= _GridDimensions.x || yIndex >= _GridDimensions.y)
                    {
#if DEBUG_TERRAIN_GEN_1
                        // Debug.Log($"Outside Bounds: {debugStr}");
#endif
                        continue;
                    }

                    // Adding offset according to current active cell
                    randomOffsetVec.x = xIndex;
                    randomOffsetVec.y = yIndex;

                    // Check if the space(r) is enough between points and no neigbours are within it
                    // Also checking if the spot itself is valid or not
                    for (int hor = -1; hor <= 1 && !withinDistance; hor++)              // TODO: Fix range as this should be between r and 2r for this index
                    {
                        for (int ver = -1; ver <= 1 && !withinDistance; ver++)              // TODO: Fix range as this should be between r and 2r for this index
                        {
                            //Bounds Check
#if DEBUG_TERRAIN_GEN_1
                            debugStr.Append($"\nhor: {hor} | ver : {ver} | ");
                            // Debug.Log($"{debugStr}");               //Test
#endif

                            if ((xIndex + hor) < 0 || (yIndex + ver) < 0
                                || (xIndex + hor) >= _GridDimensions.x || (yIndex + ver) >= _GridDimensions.y)
                                continue;

                            int neighbourIndex = (xIndex + hor) + ((yIndex + ver) * _GridDimensions.x);

#if DEBUG_TERRAIN_GEN_1
                            debugStr.Append($"neighbourIndex: {neighbourIndex} | _GridDimensions: {_GridDimensions} | _grid Val: {_grid[neighbourIndex]} | ");
                            // Debug.Log($"neighbourIndex: {neighbourIndex} | xIndex: {xIndex} | yIndex: {yIndex}"
                            //         + $" | hor: {hor} | ver : {ver} | _GridDimensions: {_GridDimensions}");
#endif

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
#endif

                                withinDistance = true;

                                // InstantitateDebugCube((xIndex + hor), (yIndex + ver), true);
                            }
                        }
                    }

                    if (!withinDistance)
                    {
                        // Debug.Log($"Adding index: {xIndex + (yIndex * _GridDimensions.x)}| xIndex {xIndex} | yIndex: {yIndex}");
                        // debugStr.Append($"Added");
                        foundCell = true;
                        _grid[xIndex + (yIndex * _GridDimensions.x)] = 1;
                        _activeGrid.Add(xIndex + (yIndex * _GridDimensions.x));

                        _poissonTex.SetPixel(xIndex, yIndex, Color.white);

                        // InstantitateDebugCube(xIndex, yIndex);
                        // break;
                    }
#if DEBUG_TERRAIN_GEN_1
                    // Debug.Log($"{debugStr}");
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

                await Task.Delay(_waitTimeInSec * 1000);

                if (_cts.IsCancellationRequested) return;
            }

#if DEBUG_TERRAIN_GEN_1
            Debug.Log($"Emergency Break Count: {emergencyBreak}");
#endif

            _poissonTex.Apply();

            poissonMapPreview.sprite = Sprite.Create(_poissonTex, new Rect(0f, 0f, _rows, _cols)
                    , new Vector2(0.5f, 0.5f));
        }

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
                tempObject.name = $"Inv_PoissonObj_{xIndex + (yIndex * _GridDimensions.x)}_[{xIndex},{yIndex}]";
            else
                tempObject.name = $"PoissonObj_{xIndex + (yIndex * _GridDimensions.x)}_[{xIndex},{yIndex}]";
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


#if PERLIN_NOISE_1
        public bool GenerateNewMap = false;

        public int pixWidth = 128, pixHeight = 128, scale = 2;
        public string RandomSeed = "";
        private Texture2D noiseTex;
        private SpriteRenderer mapPreview;

        [HideInInspector] private Color[] pix;
        [SerializeField]
        private Color water = Color.blue, sand = Color.white,
            grass = Color.green, forest = Color.green,
            mountains = Color.gray;

        private void CreateSpriteRenderer()
        {
            GameObject spriteRenderer = new GameObject("DebugPerlinNoiseRenderer");
            spriteRenderer.transform.parent = transform;
            spriteRenderer.transform.localPosition = Vector3.zero;

            mapPreview = spriteRenderer.AddComponent<SpriteRenderer>();
            mapPreview.sortingLayerID = SortingLayer.NameToID("Environment");
            Debug.Log($"{mapPreview.sortingLayerName}");
        }

        void Generatemap()
        {

            // Set up the texture and a Color array to hold pixels during processing.
            noiseTex = new Texture2D(pixWidth, pixHeight);
            pix = new Color[noiseTex.width * noiseTex.height];
            float randomorg = Random.Range(0, 100);

            // For each pixel in the texture...
            float y = 0.0F;

            while (y < noiseTex.height)
            {
                float x = 0.0F;
                while (x < noiseTex.width)
                {

                    float xCoord = randomorg + x / noiseTex.width * scale;
                    float yCoord = randomorg + y / noiseTex.height * scale;
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);

                    if (sample == Mathf.Clamp(sample, 0, 0.5f))
                        pix[(int)y * noiseTex.width + (int)x] = water;
                    else if (sample == Mathf.Clamp(sample, 0.5f, 0.6f))
                        pix[(int)y * noiseTex.width + (int)x] = sand;


                    else if (sample == Mathf.Clamp(sample, 0.6f, 0.7f))
                        pix[(int)y * noiseTex.width + (int)x] = grass;
                    else if (sample == Mathf.Clamp(sample, 0.7f, 0.8f))
                        pix[(int)y * noiseTex.width + (int)x] = forest;
                    else if (sample == Mathf.Clamp(sample, 0.8f, 1f))
                        pix[(int)y * noiseTex.width + (int)x] = mountains;
                    else
                        pix[(int)y * noiseTex.width + (int)x] = water;


                    x++;
                }
                y++;
            }


            // Copy the pixel data to the texture and load it into the GPU.
            noiseTex.SetPixels(pix);
            noiseTex.Apply();

            mapPreview.sprite = Sprite.Create(noiseTex, new Rect(0f, 0f, pixWidth, pixHeight), new Vector2(0.5f, 0.5f));
            // mapPreview.texture = noiseTex;
            // worldmap = noiseTex;
        }
#endif
    }
}