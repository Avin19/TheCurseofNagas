// #define RecordPixels

using UnityEngine;

namespace CurseOfNaga.Utils
{
    [System.Serializable]
    public class PerlinNoiseGenerator
    {
        private int _pixWidth = 128, _pixHeight = 128, _scale = 2;

        private bool _seedSet = false;
        private string _randomSeed = "";

        // private Texture2D noiseTex;
        // private SpriteRenderer mapPreview;

#if RecordPixels
        [HideInInspector] private Color[] _pix;
#endif

        private Color water, sand, grass, forest, mountains;

        public PerlinNoiseGenerator()
        {
            _pixWidth = 128;
            _pixHeight = 128;
            _scale = 2;

            _seedSet = true;

            water = Color.blue;
            sand = Color.white;
            grass = Color.green;
            forest = Color.black;
            mountains = Color.gray;
        }

        public PerlinNoiseGenerator(int pixWidth, int pixHeight, int scale, string randomSeed,
            Color waterColor, Color sandColor, Color grassColor,
            Color forestColor, Color mountainsColor)
        {
            _pixWidth = pixWidth;
            _pixHeight = pixHeight;
            _scale = scale;

            _randomSeed = randomSeed;
            if (_randomSeed == null || randomSeed.Length == 0)
                _seedSet = false;
            else
                _seedSet = true;

            water = waterColor;
            sand = sandColor;
            grass = grassColor;
            forest = forestColor;
            mountains = mountainsColor;
        }

        public void SetValues(int pixWidth, int pixHeight, int scale)       //, string randomSeed)
        {
            _pixWidth = pixWidth;
            _pixHeight = pixHeight;
            _scale = scale;

            // _randomSeed = randomSeed;
            // if (_randomSeed == null || randomSeed.Length == 0)
            //     _seedSet = false;
            // else
            //     _seedSet = true;
        }

        public Texture2D GenerateMap()
        {
            // Set up the texture and a Color array to hold pixels during processing.
            Texture2D noiseTex = new Texture2D(_pixWidth, _pixHeight);
#if RecordPixels
            _pix = new Color[noiseTex.width * noiseTex.height];
#endif

            if (!_seedSet)
            {
                _randomSeed = $"{System.DateTime.Now.Hour}{System.DateTime.Now.Minute}" +
                                $"{System.DateTime.Now.Second}{System.DateTime.Now.Millisecond}";
            }
            Random.InitState(_randomSeed.GetHashCode());
            // Debug.Log($"_randomSeed: {_randomSeed}");

            float randomorg = Random.Range(0, 100);

            // For each pixel in the texture...
            int y = 0;

            while (y < noiseTex.height)
            {
                int x = 0;
                while (x < noiseTex.width)
                {

                    float xCoord = randomorg + x / (float)noiseTex.width * _scale;
                    float yCoord = randomorg + y / (float)noiseTex.height * _scale;
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);

                    if (sample == Mathf.Clamp(sample, 0, 0.5f))
                    {
#if RecordPixels
                        _pix[x + y * noiseTex.width] = water;
#endif
                        noiseTex.SetPixel(x, y, water);
                    }
                    else if (sample == Mathf.Clamp(sample, 0.5f, 0.6f))
                    {
#if RecordPixels
                        _pix[x + y * noiseTex.width] = sand;
#endif
                        noiseTex.SetPixel(x, y, sand);
                    }
                    else if (sample == Mathf.Clamp(sample, 0.6f, 0.7f))
                    {
#if RecordPixels
                        _pix[x + y * noiseTex.width] = grass;
#endif
                        noiseTex.SetPixel(x, y, grass);
                    }
                    else if (sample == Mathf.Clamp(sample, 0.7f, 0.8f))
                    {
#if RecordPixels
                        _pix[x + y * noiseTex.width] = forest;
#endif
                        noiseTex.SetPixel(x, y, forest);
                    }
                    else
                    {
#if RecordPixels
                        _pix[x + y * noiseTex.width] = mountains;
#endif
                        noiseTex.SetPixel(x, y, mountains);
                    }

                    x++;
                }
                y++;
            }


            // Copy the pixel data to the texture and load it into the GPU.
            // noiseTex.SetPixels(_pix);
            noiseTex.Apply();

            // mapPreview.sprite = Sprite.Create(noiseTex, new Rect(0f, 0f, pixWidth, pixHeight), new Vector2(0.5f, 0.5f));
            // mapPreview.texture = noiseTex;
            // worldmap = noiseTex;

            return noiseTex;
        }
    }
}