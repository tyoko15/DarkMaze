using UnityEngine;
using System.IO;

[System.Serializable]
public class Data 
{
    public string playerName;
    [Range(0, 24)] public int clearStageNum;
    [Range(0, 24)] public int selectStageNum;
}


public class DataManager : MonoBehaviour
{
    public Data[] data = new Data[3];
    public string[] filePath = new string[3];
    public int useDataNum;
    public bool nextFieldFlag;
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        for (int i = 0; i < 3; i++)
        {
            // filePathの取得
            filePath[i] = Application.persistentDataPath + $"/savefile{i}.json";
            LoadData(i, ref data[i].playerName, ref data[i].clearStageNum, ref data[i].selectStageNum);
        }
    }

    void Update()
    {

    }

    public void SaveData(int dataNum, string name, int stage, int selectNum)
    {
        data[dataNum] = new Data();
        data[dataNum].playerName = name;
        data[dataNum].clearStageNum = stage;
        data[dataNum].selectStageNum = selectNum;
        string json = JsonUtility.ToJson(data[dataNum], true);
        File.WriteAllText(filePath[dataNum], json);
    }

    public void LoadData(int dataNum, ref string name, ref int stage, ref int selectNum)
    {
        if (File.Exists(filePath[dataNum]))
        {
            string json = File.ReadAllText(filePath[dataNum]);
            data[dataNum] = JsonUtility.FromJson<Data>(json);
            name = data[dataNum].playerName;
            stage = data[dataNum].clearStageNum;
            selectNum = data[dataNum].selectStageNum;
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
