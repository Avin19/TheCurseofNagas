// #define POISSON_EMERGENCY_BREAK_1
// #define DEBUG_TERRAIN_GEN_1
// #define DEBUG_SUB_LAYER

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

namespace CurseOfNaga.Utils
{
    [System.Serializable]
    public class PoissonDiscSampler
    {
        private int _activeCount;
        // private float _waitIntervalInSec = 0.01f;
        // private int _kAttempts = 30;
        // private float _wCellSize;
        // private int _rows = 10, _cols = 10;
        // private float _poiRadius = 2f;

        // private readonly Vector2Int _GridDimensions = new Vector2Int(10, 10);
        // private bool _randomSeed = false;
        // private string RandomSeed_2 = "135653245";             //135653245

        // private Vector2Int _GridDimensions;
        private List<int> _activeGrid;
        private byte[] _grid;

        private CancellationTokenSource _cts;

        ~PoissonDiscSampler()
        {
            _cts.Cancel();
        }

        public PoissonDiscSampler(ref byte[] grid)
        {
            _grid = grid;

            _activeGrid = new List<int>();
            _cts = new CancellationTokenSource();
        }

        public void UpdateValues(ref byte[] grid)
        {
            _grid = grid;
        }

        public int GeneratePoissonDiscSamples(int ogRows, int ogCols, byte cellType, //Color cellColor,           //TEST
        // public async Task<int> GeneratePoissonDiscSamples(int ogRows, int ogCols, byte cellType, //Color cellColor,
                int subRows = 0, int subCols = 0, int randOffset = 0, int startOffset = 0,
                int cellRadius = 2, bool spawnRandomCluster = false, int kAttempts = 30,
                float poiRadius = 0f, float waitIntervalInSec = 0.001f, string randomSeed = "")
        {
            //Intialize all value to default
            // for (int i = 0; i < _rows * _cols; i++)
            //     _grid[i] = 255;

            // if (randomSeed.CompareTo("") == 0)
            // {
            //     randomSeed = $"{System.DateTime.Now.Hour}{System.DateTime.Now.Minute}" +
            //                     $"{System.DateTime.Now.Second}{System.DateTime.Now.Millisecond}";
            // }

            //Setting Random
            Random.InitState(randomSeed.GetHashCode());
            // Debug.Log($"Seed: {randomSeed}");

            Vector2Int midPointVec;
            int randIndex;

            // Display on Sprite Renderer
            int xIndex = 0, yIndex = 0;

            // Loop active list | Check valid neighbour | Add List | Remove if not valid
            Vector2 randomOffsetVec = Vector2.zero, currentVec = Vector2.zero;
            bool withinDistance = false, foundCell = false;
            int randomAngle = -1, additionalOffset = -1;

            // Texture2D poissonTex = new Texture2D(rows, cols);

#if DEBUG_TERRAIN_GEN_1
            System.Text.StringBuilder debugStr = new System.Text.StringBuilder();
#endif

            //Layer 0: For trees | Layer 1: For Bushes
            // for (int layerId = 0; layerId < _layerDatas.Length; layerId++)
            {
                // Starting from last row + offset
                if (startOffset == 0)
                {
                    midPointVec = new Vector2Int(ogRows / 2, ogCols / 2);
                    randIndex = 3 + randOffset + startOffset;        // + _GridDimensions.y;
                    Debug.Log($"midPointVec: {midPointVec}");
                }
                else
                {
                    midPointVec = new Vector2Int(startOffset % ogCols, startOffset / ogCols);
                    randIndex = randOffset + startOffset;        // + _GridDimensions.y;

#if DEBUG_SUB_LAYER
                    Debug.Log($"midPointVec: {midPointVec} | startOffset: {startOffset}  | Clamp Offset: {Mathf.Clamp01(startOffset)}" +
                            $" | Invert Clamp Offset: {(byte)Mathf.Clamp01(startOffset) ^ (1 << 0)}" +
                            $" | Lower-X: {midPointVec.x - (subCols / 2) - (subCols % 2)} | Lower-Y: {midPointVec.y - (subRows / 2) - (subRows % 2)}" +
                            $" | Upper-X: {midPointVec.x + (subCols / 2) + (subCols % 2)} | Upper-Y: {midPointVec.y + (subRows / 2) + (subRows % 2)}");
#endif
                }
                _grid[randIndex] = cellType;
                _activeGrid.Add(randIndex);

                // xIndex = yIndex = 0;
                // randomOffsetVec = currentVec = Vector2.zero;
                // withinDistance = foundCell = false;
                // randomAngle = additionalOffset = -1;

                // Display on Sprite Renderer
                // xIndex = randIndex % cols;
                // yIndex = randIndex / cols;
                // poissonTex.SetPixel(xIndex, yIndex, cellColor);


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
                    for (int i = 0; i < kAttempts; i++)
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
                        additionalOffset = Random.Range(cellRadius, (2 * cellRadius) + 1);
                        randomOffsetVec *= additionalOffset;

                        // https://www.desmos.com/calculator/fshadmxjaa - Checking
                        // xIndex = _activeGrid[randIndex] % cols + Mathf.FloorToInt(randomOffsetVec.x / _wCellSize);
                        // yIndex = _activeGrid[randIndex] / cols + Mathf.FloorToInt(randomOffsetVec.y / _wCellSize);

                        //_wCellSize will only be needed to calculate the position afterwards, not now
                        xIndex = (_activeGrid[randIndex] % ogCols) + Mathf.RoundToInt(randomOffsetVec.x);
                        yIndex = (_activeGrid[randIndex] / ogCols) + Mathf.RoundToInt(randomOffsetVec.y);
#if DEBUG_TERRAIN_GEN_1
                        debugStr.Append($"Offset Vec: [{randomOffsetVec.x}, {randomOffsetVec.y}] | additionalOffset: {additionalOffset} | ");
                        debugStr.Append($"Floor X: {Mathf.FloorToInt(randomOffsetVec.x)} | Floor Y: {Mathf.FloorToInt(randomOffsetVec.y)} | ");
                        debugStr.Append($"xIndex: {xIndex} | yIndex: {yIndex} | ");
                        // Debug.Log($"Random Vec: {randomOffsetVec} | additionalOffset: {additionalOffset}"
                        //         + $" | xIndex: {xIndex} | yIndex: {yIndex}");
#endif


#if DEBUG_SUB_LAYER
                        // if (startOffset != 0)
                        Debug.Log(
                        // $"ogRows: {ogRows} | ogCols: {ogCols} | midPointVec: {midPointVec}" +
                        $"xIndex: {xIndex} | yIndex: {yIndex} | additionalOffset: {additionalOffset} | " +
                        $"Offset Vec: [{randomOffsetVec.x}, {randomOffsetVec.y}] | additionalOffset: {additionalOffset} | " +
                        $"Floor X: {Mathf.RoundToInt(randomOffsetVec.x)} | Floor Y: {Mathf.RoundToInt(randomOffsetVec.y)} | " +
                        $"Sqr Magnitude: {Vector2.SqrMagnitude(randomOffsetVec - midPointVec)}"
                        );
#endif

                        // Updating offset co-ords to current active cell
                        randomOffsetVec.x = xIndex;
                        randomOffsetVec.y = yIndex;

                        //Bounds Check
                        if (xIndex < 0 || yIndex < 0        // Normal-Layer Lower-X Bound
                            || xIndex > (ogRows - 1) || yIndex > (ogCols - 1)       // Out of bounds of grid length

                            || xIndex < (midPointVec.x - (subCols / 2) - (subCols % 2)) * Mathf.Clamp01(startOffset)    // Sub-Layer Lower-X Bound
                            || yIndex < (midPointVec.y - (subRows / 2) - (subRows % 2)) * Mathf.Clamp01(startOffset)

                            || xIndex > ((ogRows - 1) * ((byte)Mathf.Clamp01(startOffset) ^ (1 << 0)))      // Normal-Layer Upper-X Bound  * Invert of startOffset
                                + ((midPointVec.x + (subCols / 2) + (subCols % 2)) * Mathf.Clamp01(startOffset))        // Sub-Layer Upper-X Bound * startOffset
                            || yIndex > ((ogCols - 1) * ((byte)Mathf.Clamp01(startOffset) ^ (1 << 0)))
                                + ((midPointVec.y + (subRows / 2) + (subRows % 2)) * Mathf.Clamp01(startOffset))

                            || _grid[xIndex + (yIndex * ogCols)] != 255                                         //Cell occupied by something
                            || (Vector2.SqrMagnitude(randomOffsetVec - midPointVec) - (poiRadius * poiRadius)) <= 0)        // Also check if it is not inside the point of interest
                        {
#if DEBUG_TERRAIN_GEN_1
                            debugStr.Append($"Outside Bounds");
#endif

#if DEBUG_SUB_LAYER
                            // if (startOffset != 0)
                            Debug.Log($"Outside Bounds");
#endif
                            continue;
                        }



#if DEBUG_TERRAIN_GEN_1
                        debugStr.Append($"SqrDist: {Vector3.SqrMagnitude(randomOffsetVec - midPointVec)} | ");
#endif

                        // Check if the space(r) is enough between points and no neigbours are within it
                        // Also checking if the spot itself is valid or not
                        for (int hor = cellRadius * -1;
                            hor <= cellRadius && !withinDistance; hor++)              // TODONE: Fix range as this should check to r
                        {
                            for (int ver = cellRadius * -1;
                                ver <= cellRadius && !withinDistance; ver++)              // TODONE: Fix range as this should check to r
                            {
#if DEBUG_TERRAIN_GEN_1
                                debugStr.Append($"\nhor: {hor} | ver : {ver} | ");
                                // Debug.Log($"{debugStr}");               //Test
#endif

                                //Bounds Check
                                if ((xIndex + hor) < 0 || (yIndex + ver) < 0
                                    || (xIndex + hor) >= ogRows || (yIndex + ver) >= ogCols)
                                    continue;

                                int neighbourIndex = (xIndex + hor) + ((yIndex + ver) * ogCols);

#if DEBUG_TERRAIN_GEN_1
                                debugStr.Append($"neighbourIndex: {neighbourIndex} | _GridDimensions: {_GridDimensions} | _grid Val: {_grid[neighbourIndex]} | ");
                                // Debug.Log($"neighbourIndex: {neighbourIndex} | xIndex: {xIndex} | yIndex: {yIndex}"
                                //         + $" | hor: {hor} | ver : {ver} | _GridDimensions: {_GridDimensions}");
#endif

                                if (_grid[neighbourIndex] != 255
                                    && _grid[neighbourIndex] == cellType)
                                {
                                    withinDistance = true;
#if DEBUG_SUB_LAYER
                                    // if (startOffset != 0)
                                    Debug.Log($"Within Distance | hor: {hor} | ver: {ver} | xIndex: {xIndex + hor} | yIndex: {yIndex + ver} | "
                                            + $"neighbourIndex: {neighbourIndex} | _grid Val: {_grid[neighbourIndex]}");
#endif
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
                            // Debug.Log($"Adding index: {xIndex + (yIndex * ogCols)}| xIndex {xIndex} | yIndex: {yIndex}");
                            // debugStr.Append($"Added");
                            foundCell = true;
                            _grid[xIndex + (yIndex * ogCols)] = cellType;
                            _activeGrid.Add(xIndex + (yIndex * ogCols));

                            // poissonTex.SetPixel(xIndex, yIndex, cellColor);
                            // InstantitateDebugCube(xIndex, yIndex);

                            // Random chance to spawn cluster of objects
                            /*
                            if (spawnRandomCluster)
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
                                                || (xIndex + hor) >= ogRows || (yIndex + ver) >= ogCols
                                                || _grid[(xIndex + hor) + ((yIndex + ver) * ogCols)] != 255)             //Check if cell is occupied or not
                                                continue;

                                            _grid[(xIndex + hor) + ((yIndex + ver) * ogCols)] = cellType;
                                            // poissonTex.SetPixel((xIndex + hor), (yIndex + ver), cellColor);
                                        }
                                    }
                                }
                            }
                            */

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

                    // await Task.Delay((int)(waitIntervalInSec * 1000));

                    // if (_cts.IsCancellationRequested) return null;
                    if (_cts.IsCancellationRequested) return 0;
                }
                // Debug.Log($"Finish Check | Time: {System.DateTime.Now.Minute}m {System.DateTime.Now.Second}s "
                //     + $"{System.DateTime.Now.Millisecond}ms {System.DateTime.Now.Ticks}");

#if POISSON_EMERGENCY_BREAK_1
                Debug.Log($"Emergency Break Count: {emergencyBreak}");               //FOR DEBUG
#endif
            }

            // poissonTex.Apply();
            // poissonMapPreview.sprite = Sprite.Create(_poissonTex, new Rect(0f, 0f, _rows, _cols)
            //         , new Vector2(0.5f, 0.5f));

            // return poissonTex;
            return 1;
        }

    }
}