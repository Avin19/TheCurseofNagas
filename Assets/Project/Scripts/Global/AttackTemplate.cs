using System.Collections.Generic;

//JsonUtility works with fields and not properties
namespace CurseOfNaga.Global.Template
{
    [System.Serializable]
    public class AttackData
    {
        public int pid;
        public string name;
        public List<MeleeCombo> melee_combos;

        public AttackData() { }

        public AttackData(int pid, string name, List<MeleeCombo> combos)
        {
            this.pid = pid;
            this.name = name;
            melee_combos = combos;
        }
    }

    [System.Serializable]
    public class MeleeCombo
    {
        public int id;
        public string combo;
        public byte[] ComboSequence;

        public MeleeCombo() { }

        public MeleeCombo(int id, string combo)
        {
            this.id = id;
            this.combo = combo;
        }
    }

    [System.Serializable]
    public class AttackTemplate
    {
        public List<AttackData> attack_data;
    }
}