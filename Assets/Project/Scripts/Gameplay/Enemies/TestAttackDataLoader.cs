using CurseOfNaga.Global.Template;
using UnityEngine;

namespace CurseOfNaga.Gameplay.Enemies
{
    public class TestAttackDataLoader : MonoBehaviour
    {
        private AttackDataParser attackDataParser;
        private void Start()
        {
            attackDataParser = new AttackDataParser();
            attackDataParser.LoadAttackDataJson();
        }
    }
}