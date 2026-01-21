using UnityEngine;
using System.IO;

/// <summary>
/// セーブデータ1枠分の情報をまとめたクラス
/// JsonUtilityでそのままJSON化できるよう Serializable を付与
/// </summary>
[System.Serializable]
public class Data
{
    public string playerName;                 // プレイヤー名
    [Range(0, 24)] public int clearStageNum;  // クリア済みステージ数
    [Range(0, 24)] public int selectStageNum; // 現在選択中のステージ番号
}

/// <summary>
/// セーブデータ全体を管理するクラス
/// ・3スロット分のデータを保持
/// ・JSON形式で保存 / 読み込み
/// ・シーンを跨いで保持されるシングルトン的存在
/// </summary>
public class DataManager : MonoBehaviour
{
    // ===== セーブデータ =====
    public Data[] data = new Data[3];          // 3つのセーブスロット
    public string[] filePath = new string[3]; // 各スロットの保存先パス

    public int useDataNum;                     // 現在使用中のデータ番号
    public bool nextFieldFlag;                 // 次フィールド遷移用フラグ（拡張用）

    void Start()
    {
        // シーン切り替え後も破棄されないようにする
        DontDestroyOnLoad(gameObject);

        // 各セーブスロットの初期化と読み込み
        for (int i = 0; i < 3; i++)
        {
            // 端末ごとの永続保存パスを使用
            filePath[i] = Application.persistentDataPath + $"/savefile{i}.json";

            // 保存データの読み込み
            LoadData(
                i,
                ref data[i].playerName,
                ref data[i].clearStageNum,
                ref data[i].selectStageNum
            );
        }
    }

    void Update()
    {
        // 常時更新処理は不要なため空
    }

    /// <summary>
    /// 指定したスロットにセーブデータを書き込む
    /// </summary>
    /// <param name="dataNum">セーブスロット番号</param>
    /// <param name="name">プレイヤー名</param>
    /// <param name="stage">クリアステージ数</param>
    /// <param name="selectNum">選択中ステージ番号</param>
    public void SaveData(int dataNum, string name, int stage, int selectNum)
    {
        // 新しいデータを作成
        data[dataNum] = new Data
        {
            playerName = name,
            clearStageNum = stage,
            selectStageNum = selectNum
        };

        // JSON形式に変換（整形あり）
        string json = JsonUtility.ToJson(data[dataNum], true);

        // ファイルへ書き込み
        File.WriteAllText(filePath[dataNum], json);
    }

    /// <summary>
    /// 指定したスロットのデータを読み込む
    /// 存在しない場合は空データを生成する
    /// </summary>
    public void LoadData(int dataNum, ref string name, ref int stage, ref int selectNum)
    {
        // ファイルが存在するか確認
        if (File.Exists(filePath[dataNum]))
        {
            string json = File.ReadAllText(filePath[dataNum]);
            data[dataNum] = JsonUtility.FromJson<Data>(json);

            // 読み込み失敗時の保険
            if (data[dataNum] == null)
            {
                Debug.Log("このデータは存在しません。");
                SaveData(dataNum, "", 0, 0);
            }
        }
        else
        {
            // 初回起動時など
            Debug.Log("このデータは存在しません。");
            SaveData(dataNum, "", 0, 0);
        }
    }

    /// <summary>
    /// 指定したセーブデータを削除する
    /// </summary>
    public void DeleteData(int dataNum)
    {
        if (File.Exists(filePath[dataNum]))
        {
            File.Delete(filePath[dataNum]);
        }
    }

    /// <summary>
    /// 使用中のセーブデータ番号を記録
    /// </summary>
    public void GetuseDataNum(int dataNum)
    {
        useDataNum = dataNum;
    }
}
