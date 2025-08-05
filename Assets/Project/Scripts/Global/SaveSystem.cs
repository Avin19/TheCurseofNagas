/*
*   Following Minecraft saving rules
*   - Save Player State
*       [=] Position
*       [=] Health
*       [=] Inventory Items
*       [=] Experience
*   - Save Entities State
*       [=] Type of mobs
*       [=] Position
*       [=] Health maybe?
*   - Save World State
*       [=] Item Drops
*       [=] Chests/Storage units
*       [=] Time | Weather | Game Rules
*   - Save Total Stats State                    [MIGHT NOT BE NEEDED]
*   - Save Region State                         [MIGHT NOT BE NEEDED]
*   - Save Advancements/Achievements State      [MIGHT NOT BE NEEDED]
*   - Save Map State                            [MIGHT NOT BE NEEDED]
*   
*   - .dat files (NBT compression) using GZIP
*/

using System;
using System.IO;
using UnityEngine;

using static CurseOfNaga.Global.UniversalConstant;
using Encoding = System.Text.Encoding;

namespace CurseOfNaga.Global
{
    [Serializable]
    public class PlayerState
    {
        public Vector3 PlayerPos;
        public float Health, Experience;
        //Inventory Items

        public PlayerState(Vector3 pos, float hp = 0f, float exp = 0f)
        {
            PlayerPos = pos;
            Health = hp;
            Experience = exp;
        }

        public PlayerState()
        {
            PlayerPos = Vector3.zero;
            Health = 100f;
            Experience = 0f;
        }

        public override string ToString()
        {
            return $"Pos: {PlayerPos} | Health: {Health} | Experience: {Experience}";
        }
    }

    [Serializable]
    public class WorldState
    {

    }

    [Serializable]
    public class SaveSystem
    {
        private static SaveSystem _instance;
        public static SaveSystem Instance { get => _instance; }

        public SaveSystem()
        {
            if (_instance == null)
                _instance = this;
        }

        // get_persistentDataPath is not allowed to be called during serialization
        // readonly string filePath = Path.Combine(Application.persistentDataPath, "PlayerStats.txt");      

        public PlayerState CurrentPlayerState;
        public WorldState CurrentWorldState;

        public void SavePlayerState(PlayerState playerState, Action<SaveStatus, string> OnOperationComplete)
        {
            string playerData = JsonUtility.ToJson(playerState);
            byte[] playerDataInBytes = Encoding.ASCII.GetBytes(playerData);
            string filePath = Path.Combine(Application.persistentDataPath, "PlayerStats.txt");

            FileStream saveStream;
            if (!File.Exists(filePath))
                saveStream = File.Create(filePath);
            else
                saveStream = File.Open(filePath, FileMode.Open);

            try
            {
                saveStream.Write(playerDataInBytes, 0, playerDataInBytes.Length);
                // Debug.Log($"Saved file at; {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unable to save. Error: {ex.Message}");
                OnOperationComplete?.Invoke(SaveStatus.SAVE_FAILED, ex.Message);
                return;
            }
            finally
            {
                saveStream.Close();
            }
            OnOperationComplete?.Invoke(SaveStatus.SAVE_SUCCESSFUL, null);
        }

        public void LoadPlayerState(Action<SaveStatus, string> OnOperationComplete)
        {
            string filePath = Path.Combine(Application.persistentDataPath, "PlayerStats.txt");
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Error! No save file found");
                return;
            }

            byte[] playerDataInBytes = null;
            try
            {
                playerDataInBytes = File.ReadAllBytes(filePath);
            }
            catch (Exception ex)
            {
                Debug.Log($"Unable to load. Error: {ex.Message}");
                OnOperationComplete?.Invoke(SaveStatus.LOAD_FAILED, ex.Message);
                return;
            }

            string playerData = Encoding.ASCII.GetString(playerDataInBytes);
            CurrentPlayerState = JsonUtility.FromJson<PlayerState>(playerData);
            // Debug.Log($"Loaded Data: {CurrentPlayerState}");
            OnOperationComplete?.Invoke(SaveStatus.LOAD_SUCCESSFUL, null);
        }
    }
}