using CurseOfNaga.Global.Template;
using UnityEngine;

namespace CurseOfNaga.Gameplay.Enemies
{
    public class TestAttackDataLoader : MonoBehaviour
    {
        private static TestAttackDataLoader _instance;
        public static TestAttackDataLoader Instance { get => _instance; }

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
                Destroy(gameObject);
        }

        public AttackDataParser AttackDataParser;

        private void Start()
        {
            AttackDataParser = new AttackDataParser();
            AttackDataParser.LoadAttackDataJson();
        }
    }
}