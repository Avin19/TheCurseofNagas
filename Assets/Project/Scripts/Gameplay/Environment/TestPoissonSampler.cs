using System.Collections;
using System.Collections.Generic;
using CurseOfNaga.Utils;
using UnityEngine;

public class TestPoissonSampler : MonoBehaviour
{
    PoissonDiscSampler _poissonDiscSampler;
    private byte[] _grid;
    [Range(5f, 50f)][SerializeField] private int _rows = 10, _cols = 10;
    [Range(2f, 6f)][SerializeField] private int subLayerRows = 4, subLayerCols = 4;
    int _kAttempts = 5;

    private Texture2D _poissonTex;
    [SerializeField] private SpriteRenderer _mapPreview;


    [Range(0.001f, 1f)][SerializeField] private float _waitIntervalInSec = 0.001f;
    [SerializeField] private string RandomSeed_2 = "135653245";             //135653245

    // Start is called before the first frame update
    void Start()
    {
        _grid = new byte[_rows * _cols];
        // Set up the texture
        _poissonTex = new Texture2D(_rows, _cols);

        _poissonDiscSampler = new PoissonDiscSampler(ref _grid);

        TestRun_1();
    }

    private void TestRun_1()
    {
        //Intialize all value to default 
        for (int i = 0; i < _rows * _cols; i++)
            _grid[i] = 255;

        int runResult;
        const int RAND_OFFSET = 0;
        const float POI_RADIUS = 0f;
        const int CELL_RADIUS = 1;
        const bool SPAWN_RANDOM_CLUSTER = true;
        int startOffset = 55;

        for (int gridIndex = 0; gridIndex < 1; gridIndex++)
        {
            Debug.Log($"Bush Sub-Layer | grid: {_grid[gridIndex]}");
            // runResult = await _poissonDiscSampler.GeneratePoissonDiscSamples(_rows, _cols,
            runResult = _poissonDiscSampler.GeneratePoissonDiscSamples(_rows, _cols,
                    1, subLayerRows, subLayerCols, RAND_OFFSET, startOffset,
                    CELL_RADIUS, SPAWN_RANDOM_CLUSTER, _kAttempts,
                    POI_RADIUS, _waitIntervalInSec, RandomSeed_2);

            if (runResult == 0)
            {
                Debug.LogError($"Error occured while generating");
            }
        }
        // */

        int xIndex = 0, yIndex = 0;
        for (int i = 0; i < _grid.Length; i++)
        {
            xIndex = i % _cols;
            yIndex = i / _cols;

            //[IMPORTANT] This needs to be according to the layer data
            switch (_grid[i])
            {
                case 255:         //Do Nothing
                    break;

                case 0:
                    _poissonTex.SetPixel(xIndex, yIndex, Color.green);
                    break;

                case 1:
                    _poissonTex.SetPixel(xIndex, yIndex, Color.blue);
                    InstantitateDebugCube(xIndex, yIndex);
                    break;

                case 2:
                    _poissonTex.SetPixel(xIndex, yIndex, Color.gray);
                    break;

                case 3:
                    _poissonTex.SetPixel(xIndex, yIndex, Color.gray);
                    break;

                case 4:
                    _poissonTex.SetPixel(xIndex, yIndex, Color.gray);
                    break;
            }
            // if (_grid[i] == (byte)_layerDatas[0].CellType)
            // _poissonTex.SetPixel(xIndex, yIndex, _layerDatas[0].CellColor);
        }

        _poissonTex.Apply();


        _mapPreview.sprite = Sprite.Create(_poissonTex, new Rect(0f, 0f, _rows, _cols)
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
            tempObject.name = $"Inv_PoissonObj_{xIndex + (yIndex * _cols)}_[{xIndex},{yIndex}]";
        else
            tempObject.name = $"PoissonObj_{xIndex + (yIndex * _cols)}_[{xIndex},{yIndex}]";
    }
}
