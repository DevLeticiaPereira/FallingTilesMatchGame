using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    public SaveData UserSaveData { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        UserSaveData = new SaveData();
        if (!Directory.Exists(Application.persistentDataPath + "/UserSaveData/"))
            Directory.CreateDirectory(Application.persistentDataPath + "/UserSaveData/");
    }

    public void SaveSingleModeHighScore(int highScore)
    {
        UserSaveData.SingleModeHighScore = highScore;
        var formatter = new BinaryFormatter();
        var stream = new FileStream(Application.persistentDataPath + "/UserSaveData/userData.dat", FileMode.Create);
        formatter.Serialize(stream, UserSaveData);
        stream.Close();
    }

    public SaveData LoadSaveData()
    {
        if (File.Exists(Application.persistentDataPath + "/UserSaveData/userData.dat"))
        {
            var formatter = new BinaryFormatter();
            var stream = new FileStream(Application.persistentDataPath + "/UserSaveData/userData.dat", FileMode.Open);
            UserSaveData = formatter.Deserialize(stream) as SaveData;
            stream.Close();
        }

        return UserSaveData;
    }

    [Serializable]
    public class SaveData
    {
        public int SingleModeHighScore;
        public int TwoPlayersHighScore;
    }
}