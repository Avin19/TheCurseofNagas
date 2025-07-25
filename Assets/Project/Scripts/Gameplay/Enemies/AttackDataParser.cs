using System.IO;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

using CurseOfNaga.Global.Template;
using CurseOfNaga.Utils;
using static CurseOfNaga.Global.UniversalConstant;

namespace CurseOfNaga.Gameplay.Enemies
{
    [System.Serializable]
    public class AttackDataParser
    {
        public AttackTemplate AttackTemplateData;

        private CancellationTokenSource _cts;

        #region AttackConstants
        private const string NORMAL_M0 = "NM0", NORMAL_M1 = "NM1", NORMAL_M2 = "NM2", NORMAL_M3 = "NM3", NORMAL_M4 = "NM4", NORMAL_M5 = "NM5";
        private const string HEAVY_M0 = "HM0", HEAVY_M1 = "HM1", HEAVY_M2 = "HM2", HEAVY_M3 = "HM3", HEAVY_M4 = "HM4", HEAVY_M5 = "HM5";
        private const string SHOOT1 = "SH1", THROW1 = "TH1";
        #endregion AttackConstants

        ~AttackDataParser()
        {
            _cts.Cancel();
        }

        public AttackDataParser()
        {
            AttackTemplateData = null;
            _cts = new CancellationTokenSource();
        }

        public async void LoadAttackDataJson()
        {
            // TestToJson();

            string path = Path.Combine(Application.streamingAssetsPath, "AttackData.json");

            if (!File.Exists(path))
            {
                Debug.LogError($"No file found!! | Path: {path}");
                return;
            }

            FileDataHelper fileDataHelper = new FileDataHelper();
            string attackDataStr = await fileDataHelper.GetFileData_Async(path);
            Debug.Log($"Str: {attackDataStr}");
            if (_cts.IsCancellationRequested) return;

            await Task.Run(() =>
            {
                AttackTemplateData = JsonUtility.FromJson<AttackTemplate>(attackDataStr);
            }, _cts.Token);

            if (AttackTemplateData == null)
            {
                Debug.LogError("Attack Template is null");
                return;
            }

            await Task.Run(() =>
            {
                ParseAttackCombos();
            }, _cts.Token);
        }

        private void ParseAttackCombos()
        {
            //Parse the combos and store them
            int meleeIndex = 0, attackIndex = 0;
            int tempArrSize = 0;
            byte tempAttackVal = 0;
            string tempStr;
            MeleeCombo meleeCombo;
            for (int attDataIndex = 0; attDataIndex < AttackTemplateData.attack_data.Count; attDataIndex++)
            {
                for (meleeIndex = 0; meleeIndex < AttackTemplateData.attack_data[attDataIndex].melee_combos.Count;
                    meleeIndex++)
                {
                    meleeCombo = AttackTemplateData.attack_data[attDataIndex].melee_combos[meleeIndex];

                    int.TryParse(meleeCombo.combo.Substring(0, 1), out tempArrSize);
                    meleeCombo.ComboSequence = new byte[tempArrSize];

                    for (attackIndex = 0; attackIndex < tempArrSize; attackIndex++)
                    {
                        //  +2 offset to exclude total-length and comma
                        tempStr = meleeCombo.combo.Substring((attackIndex * 4) + 2, 3);
                        // Debug.Log($"attDataIndex: {attDataIndex} | meleeIndex: {meleeIndex} | "
                        //     + $"attackIndex: {attackIndex} | Temp Str: {tempStr}");

                        // /*
                        switch (tempStr)
                        {
                            case NORMAL_M0: tempAttackVal = (byte)EnemyAttackType.NORMAL_M0; break;
                            case NORMAL_M1: tempAttackVal = (byte)EnemyAttackType.NORMAL_M1; break;
                            case NORMAL_M2: tempAttackVal = (byte)EnemyAttackType.NORMAL_M2; break;
                            case NORMAL_M3: tempAttackVal = (byte)EnemyAttackType.NORMAL_M3; break;
                            case NORMAL_M4: tempAttackVal = (byte)EnemyAttackType.NORMAL_M4; break;
                            case NORMAL_M5: tempAttackVal = (byte)EnemyAttackType.NORMAL_M5; break;

                            case HEAVY_M0: tempAttackVal = (byte)EnemyAttackType.HEAVY_M0; break;
                            case HEAVY_M1: tempAttackVal = (byte)EnemyAttackType.HEAVY_M1; break;
                            case HEAVY_M2: tempAttackVal = (byte)EnemyAttackType.HEAVY_M2; break;
                            case HEAVY_M3: tempAttackVal = (byte)EnemyAttackType.HEAVY_M3; break;
                            case HEAVY_M4: tempAttackVal = (byte)EnemyAttackType.HEAVY_M4; break;
                            case HEAVY_M5: tempAttackVal = (byte)EnemyAttackType.HEAVY_M5; break;

                            case SHOOT1: tempAttackVal = (byte)EnemyAttackType.SHOOT1; break;
                            case THROW1: tempAttackVal = (byte)EnemyAttackType.THROW1; break;
                        }

                        meleeCombo.ComboSequence[attackIndex] = tempAttackVal;
                        // */
                    }
                }
            }
        }

        public void TestToJson()
        {
            AttackTemplate attackTemplate = new AttackTemplate();

            attackTemplate.attack_data = new System.Collections.Generic.List<AttackData>()
            {
                new AttackData(0, "Enemy_0", new System.Collections.Generic.List<MeleeCombo>() {
                    new MeleeCombo(0, "N1,H1,N2,H2"),
                    new MeleeCombo(1, "N2,H1,N1,H1"),
                    new MeleeCombo(1, "N2,N1,N2,H2"),
                })
            };

            string jsonStr = JsonUtility.ToJson(attackTemplate);
            Debug.Log($"ToJson: {jsonStr}");
        }
    }
}