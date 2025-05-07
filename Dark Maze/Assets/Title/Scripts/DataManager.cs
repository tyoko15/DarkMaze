using UnityEngine;
using System.IO;

[System.Serializable]
public class Data 
{
    public string playerName;
    [Range(0, 24)] public int clearStageNum;    
}


public class DataManager : MonoBehaviour
{
    public Data[] data = new Data[3];
    public string[] filePath = new string[3];
    public int useDataNum;
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        for (int i = 0; i < 3; i++)
        {
            // filePathの取得
            filePath[i] = Application.persistentDataPath + $"/savefile{i}.json";
            LoadData(i, ref data[i].playerName, ref data[i].clearStageNum);
        }
    }

    void Update()
    {

    }

    public void SaveData(int dataNum, string name, int stage)
    {
        data[dataNum] = new Data();
        data[dataNum].playerName = name;
        data[dataNum].clearStageNum = stage;

        string json = JsonUtility.ToJson(data[dataNum], true);
        File.WriteAllText(filePath[dataNum], json);
    }

    public void LoadData(int dataNum, ref string name, ref int stage)
    {
        if (File.Exists(filePath[dataNum]))
        {
            string json = File.ReadAllText(filePath[dataNum]);
            data[dataNum] = JsonUtility.FromJson<Data>(json);
            name = data[dataNum].playerName;
            stage = data[dataNum].clearStageNum;
        }
        else
        {
            Debug.Log("このデータは存在しません。");
        }
    }

    public void DeleteData(int dataNum)
    {
        if (!File.Exists(filePath[dataNum]))
        {
            File.Delete(filePath[dataNum]);
        }
    }

    public void GetuseDataNum(int dataNum)
    {
        useDataNum = dataNum;
    }
}
