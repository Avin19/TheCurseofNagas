using UnityEngine;
using UnityEngine.UI;

namespace CurseOfNaga.Gameplay.Managers.Test
{
    public class TestMinimapManager : MonoBehaviour
    {
        [SerializeField] private Camera _mainCamTerrain, _mainCamOther;
        [SerializeField] private RenderTexture _minimapRT;
        [SerializeField] private RawImage _minimapImg;

        private RenderTexture _currentRT;
        public bool updateRender = false;

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
            _currentRT = RenderTexture.active;
            RenderTexture.active = _minimapRT;

            _mainCamTerrain.Render();

            Texture2D image = new Texture2D(_minimapRT.width, _minimapRT.height);
            image.ReadPixels(new Rect(0, 0, _minimapRT.width, _minimapRT.height), 0, 0);
            image.Apply();

            _minimapImg.texture = image;

            RenderTexture.active = _currentRT;
        }
    }
}