
using UnityEngine;
using UnityEngine.UI;

namespace CurseOfNaga.MainMenu
{
    public class UIManager : MonoBehaviour
    {

        [SerializeField] private RectTransform _mainMenu;

        [SerializeField] private Button _startBtn, _optionBtn, _quitBtn;

        void OnEnable()
        {
            _startBtn.onClick.AddListener(() => _mainMenu.gameObject.SetActive(false));
        }


    }
}