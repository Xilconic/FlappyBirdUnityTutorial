using System;
using System.IO;
using UnityEngine;

namespace Assets
{
    [Serializable]
    public class SaveData
    {
        private const string SaveDataFileName = "SaveData.dat";

        public int HighScore = 0;

        public bool IsBeatingHighScore(int score)
        {
            return score > HighScore;
        }

        /// <exception cref="UnableToSaveSaveDataException"/>
        public void Save()
        {
            try
            {
                string saveDataJson = JsonUtility.ToJson(this);
                File.WriteAllText(Path.Combine(Application.persistentDataPath, SaveDataFileName), saveDataJson);
                
            }
            catch (Exception e)
            {
                throw new UnableToLoadSaveDataException("Unable to load save data", e);
            }
        }

        /// <exception cref="UnableToLoadSaveDataException"/>
        public static SaveData Load()
        {
            try
            {
                string saveDataJson = File.ReadAllText(Path.Combine(Application.persistentDataPath, SaveDataFileName));
                return JsonUtility.FromJson<SaveData>(saveDataJson);
            }
            catch (Exception e)
            {
                throw new UnableToLoadSaveDataException("Unable to load save data", e);
            }
        }

        public class UnableToLoadSaveDataException : Exception
        {
            public UnableToLoadSaveDataException(string message, Exception innerException) : base(message, innerException){ }
        }

        public class UnableToSaveSaveDataException : Exception
        {
            public UnableToSaveSaveDataException(string message, Exception innerException) : base(message, innerException) { }
        }
    }
}
