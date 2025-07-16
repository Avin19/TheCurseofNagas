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
        private List<int> _activeGrid;
        [HideInInspector] private byte[] _grid;

        private CancellationTokenSource _cts;

        ~PoissonDiscSampler()
        {
            _cts.Cancel();
        }

        public PoissonDiscSampler(ref byte[] grid, string randomSeed = "")
        {
            _grid = grid;
            //Setting Random
            Random.InitState(randomSeed.GetHashCode());

            _activeGrid = new List<int>();
            _cts = new CancellationTokenSource();
        }

        public void UpdateValues(ref byte[] grid, string randomSeed = "")
        {
            _grid = grid;

            if (randomSeed.CompareTo("") == 0)
                Random.InitState(randomSeed.GetHashCode());
        }

        public int GeneratePoissonDiscSamples(int ogRows, int ogCols, byte cellType, //Color cellColor,           //TEST
        // public async Task<int> GeneratePoissonDiscSamples(int ogRows, int ogCols, byte cellType, //Color cellColor,
                int subRows = 0, int subCols = 0, int midIndex = 0, int startOffset = 0,
                int cellRadius = 2, bool spawnRandomCluster = false, int kAttempts = 30,
                float poiRadius = 0f, float waitIntervalInSec = 0.001f)
        {
            // Debug.Log($"Seed: {randomSeed}");

            Vector2Int midPointVec;
            int randIndex;

            // Display on Sprite Renderer
            int xIndex = 0, yIndex = 0;

            // Loop active list | Check valid neighbour | Add List | Remove if not valid
            Vector2 randomOffsetVec = Vector2.zero, currentVec = Vector2.zero;
            bool withinDistance = false, foundCell = false;
            int randomAngle = -1, additionalOffset = -1;
            int randomRadius = -1;

            // Texture2D poissonTex = new Texture2D(rows, cols);

#if DEBUG_TERRAIN_GEN_1
            System.Text.StringBuilder debugStr = new System.Text.StringBuilder();
#endif

            if (startOffset == -1)
            {
                midPointVec = new Vector2Int(ogRows / 2, ogCols / 2);
                currentVec = midPointVec;
                // Starting from last row + offset
                // randIndex = midIndex + startOffset;        // + _GridDimensions.y;
                randIndex = (ogRows / 2) + ((ogCols / 2) * ogCols);        // + _GridDimensions.y;
                Debug.Log($"randIndex:{randIndex} | midPointVec: {midPointVec} | currentVec: {currentVec} | ");
            }
            else
            {
                if (spawnRandomCluster)
                    midPointVec = new Vector2Int(midIndex % ogCols, midIndex / ogCols);
                else
                    midPointVec = new Vector2Int(startOffset % ogCols, startOffset / ogCols);

                currentVec = new Vector2Int(startOffset % ogCols, startOffset / ogCols);
                // This will always go Left and Down | Starting from last row/last column | Also bounds check
                // randIndex = startOffset - (subRows + subCols * ogCols);
                randIndex = Mathf.Max(0, ((int)currentVec.x - subRows)) + (Mathf.Max(0, ((int)currentVec.y - subCols)) * ogCols);
            }


#if DEBUG_SUB_LAYER
            Debug.Log($"randIndex:{randIndex} | midPointVec: {midPointVec} | currentVec: {currentVec} | " +
                        $"startOffset: {startOffset}  | Clamp Offset: {Mathf.Clamp01(startOffset)}" +
                        $"Invert Clamp Offset: {(byte)Mathf.Clamp01(startOffset) ^ (1 << 0)} | cellRadius: {cellRadius} | " +
                        $"Lower-X: {currentVec.x - subCols} | Lower-Y: {currentVec.y - subRows} | " +
                        $"Upper-X: {currentVec.x + subCols} | Upper-Y: {currentVec.y + subRows} | ");
#endif
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
                    debugStr.Append($"SqrDist: {Vector3.SqrMagnitude(randomOffsetVec - midPointVec)} | ");
                    // Debug.Log($"Random Vec: {randomOffsetVec} | additionalOffset: {additionalOffset}"
                    //         + $" | xIndex: {xIndex} | yIndex: {yIndex}");
#endif

#if DEBUG_SUB_LAYER
                    // if (startOffset != 0)
                    if (spawnRandomCluster)
                        Debug.Log(
                        $"xIndex: {xIndex} | yIndex: {yIndex} | additionalOffset: {additionalOffset} | " +
                        $"Offset Vec: [{randomOffsetVec.x}, {randomOffsetVec.y}] | additionalOffset: {additionalOffset} | " +
                        $"Floor X: {Mathf.RoundToInt(randomOffsetVec.x)} | Floor Y: {Mathf.RoundToInt(randomOffsetVec.y)} | "
                        );
#endif

                    // Updating offset co-ords to current active cell
                    randomOffsetVec.x = xIndex;
                    randomOffsetVec.y = yIndex;

                    //--------------------------------------------------------------------------------------------

                    //Bounds Check
                    if (xIndex < 0 || yIndex < 0        // Normal-Layer Lower-X Bound
                        || xIndex > (ogRows - 1) || yIndex > (ogCols - 1)       // Out of bounds of grid length

                        || xIndex < (currentVec.x - subCols) * Mathf.Clamp01(startOffset)    // Sub-Layer Lower-X Bound
                        || yIndex < (currentVec.y - subRows) * Mathf.Clamp01(startOffset)

                        || xIndex > ((ogRows - 1) * ((byte)Mathf.Clamp01(startOffset) ^ (1 << 0)))      // Normal-Layer Upper-X Bound  * Invert of startOffset
                            + ((currentVec.x + subCols) * Mathf.Clamp01(startOffset))        // Sub-Layer Upper-X Bound * startOffset
                        || yIndex > ((ogCols - 1) * ((byte)Mathf.Clamp01(startOffset) ^ (1 << 0)))
                            + ((currentVec.y + subRows) * Mathf.Clamp01(startOffset))

                        || _grid[xIndex + (yIndex * ogCols)] != 255                                         //Cell occupied by something
                        || (Vector2.SqrMagnitude(randomOffsetVec - midPointVec) - (poiRadius * poiRadius)) <= 0)        // Also check if it is not inside the point of interest
                    {
#if DEBUG_TERRAIN_GEN_1
                        debugStr.Append($"Outside Bounds");
#endif

#if DEBUG_SUB_LAYER
                        // if (startOffset != 0)
                        if (spawnRandomCluster)
                            Debug.Log(
                                $"Outside Bounds | " +
                                $"randomOffsetVec: {randomOffsetVec} | midPointVec: {midPointVec} | " +
                                $"Sqr Magnitude: {Vector2.SqrMagnitude(randomOffsetVec - midPointVec)}"
                            );
#endif
                        continue;
                    }

                    //--------------------------------------------------------------------------------------------

                    if (spawnRandomCluster)
                        randomRadius = (int)Mathf.Clamp01(Random.Range(0, 5));
                    // randomRadius = Random.Range(0, 4);
                    else
                        randomRadius = cellRadius;

                    // Check if the space(r) is enough between points and no neigbours are within it
                    // Also checking if the spot itself is valid or not
                    for (int hor = randomRadius * -1;
                        hor <= randomRadius && !withinDistance; hor++)              // TODONE: Fix range as this should check to r
                    {
                        for (int ver = randomRadius * -1;
                            ver <= randomRadius && !withinDistance; ver++)              // TODONE: Fix range as this should check to r
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
                            debugStr.Append($"neighbourIndex: {neighbourIndex} | ogRows: {ogRows} | ogCols: {ogCols} | _grid Val: {_grid[neighbourIndex]} | ");
                            // Debug.Log($"neighbourIndex: {neighbourIndex} | xIndex: {xIndex} | yIndex: {yIndex}"
                            //         + $" | hor: {hor} | ver : {ver} | _GridDimensions: {_GridDimensions}");
#endif

                            if (_grid[neighbourIndex] != 255
                                && _grid[neighbourIndex] == cellType)
                            {
                                withinDistance = true;
#if DEBUG_SUB_LAYER
                                // if (startOffset != 0)
                                if (spawnRandomCluster)
                                    Debug.Log($"Within Distance | hor: {hor} | ver: {ver} | xIndex: {xIndex + hor} | yIndex: {yIndex + ver} | "
                                            + $"randomRadius: {randomRadius} | neighbourIndex: {neighbourIndex} | _grid Val: {_grid[neighbourIndex]}");
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


            // poissonTex.Apply();
            // poissonMapPreview.sprite = Sprite.Create(_poissonTex, new Rect(0f, 0f, _rows, _cols)
            //         , new Vector2(0.5f, 0.5f));

            // return poissonTex;
            return 1;
        }

    }
}