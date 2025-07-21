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
using System.Text;
using UnityEngine;

namespace CurseOfNaga.Global
{
    [Serializable]
    public class PlayerState
    {
        public Vector3 PlayerPos;
        public float Health, Experience;
        //Inventory Items
    }

    [Serializable]
    public class WorldState
    {

    }

    public class SaveSystem
    {
        private static SaveSystem _instance;
        public static SaveSystem Instance { get => _instance; }

        public SaveSystem()
        {
            if (_instance == null)
                _instance = this;
            else
                _instance = null;
        }

        readonly string filePath = Path.Combine(Application.persistentDataPath, "PlayerStats.txt");

        public void SavePlayerState(PlayerState playerState)
        {
            string playerData = JsonUtility.ToJson(playerState);
            byte[] playerDataInBytes = Encoding.ASCII.GetBytes(playerData);

            FileStream saveStream;
            if (!File.Exists(filePath))
                saveStream = File.Create(filePath);
            else
                saveStream = File.Open(filePath, FileMode.Open);

            try
            {
                saveStream.WriteAsync(playerDataInBytes, 0, playerDataInBytes.Length);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unable to save. Error: {ex.Message}");
            }
            finally
            {
                saveStream.Close();
            }
        }

        public void LoadPlayerState()
        {
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
                return;
            }

            string playerData = Encoding.ASCII.GetString(playerDataInBytes);
            PlayerState currentPlayerState = JsonUtility.FromJson<PlayerState>(playerData);
        }
    }
}