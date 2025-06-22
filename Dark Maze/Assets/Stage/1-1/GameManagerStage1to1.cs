using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManagerStage1to1 : GeneralStageManager
{
    void Start()
    {
        playUI.SetActive(true);
        menuUI.SetActive(false);
        GameObject fade = GameObject.Find("FadeManager");
        if (fade == null)
        {
            fade = Instantiate(fadeManagerObject);
            fade.gameObject.name = "FadeManager";
            fadeManager = fade.GetComponent<FadeManager>();
            fadeManager.AfterFade();
        }
        else if (fade != null) fadeManager = fade.GetComponent<FadeManager>();
        fadeManager.fadeOutFlag = true;
        fadeFlag = true;

        for (int i = 0; i < defeatGateFlag.Length; i++)
        {
            defeatGateFlag[i] = true;
        }
        if (GameObject.Find("DataManager") != null)
        {
            int dataNum = GameObject.Find("DataManager").GetComponent<DataManager>().useDataNum;
            player.GetComponent<PlayerController>().clearStageNum = GameObject.Find("DataManager").GetComponent<DataManager>().data[dataNum].clearStageNum;
        }
        else if (GameObject.Find("DataManager") == null) player.GetComponent<PlayerController>().clearStageNum = 0;
    }

    void Update()
    {
        switch (status)
        {
            case GameStatus.start:
                playerController.status = 0;
                StartAnime();
            break;
            case GameStatus.play:
                Gimmick1();
                Gimmick2();
                Gimmick3();
                Goal();
                if (menuFlag) status = GameStatus.menu;
                playerController.status = 1;
                break;
            case GameStatus.stop:
                Gimmick1();
                Gimmick2();
                Gimmick3();
                Goal();
                playerController.status = 2;
                break;
            case GameStatus.menu:
                MenuControl();
                if (!menuFlag) status = GameStatus.play;
                playerController.status = 3;
                break;
            case GameStatus.over:
                playerController.status = 4;
                break;
            case GameStatus.clear:
                playerController.status = 5;
                EndAnime();
                break;
        }
    }
    void EndAnime()
    {
        if (fadeFlag)
        {
            if (fadeManager.fadeIntervalFlag && fadeManager.endFlag) fadeFlag = false;
            fadeManager.FadeControl();
        }
        else
        {
            if (GameObject.Find("DataManager") != null)
            {
                DataManager dataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
                int dataNum = dataManager.useDataNum;
                if (dataManager.data[dataNum].clearStageNum == 1) dataManager.data[dataNum].clearStageNum = 2;
                dataManager.data[dataNum].selectStageNum = 1;
                dataManager.SaveData(dataManager.useDataNum, dataManager.data[dataManager.useDataNum].playerName, dataManager.data[dataNum].clearStageNum, dataManager.data[dataNum].selectStageNum);
            }
            SceneManager.LoadScene("StageSelect");
        }
    }
    // 右上エリアの回転ギミック
    public void Gimmick1()
    {
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[1], -1, 90, 2, 0, true, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);
    }
    // 右下エリアの敵撃破ギミック
    public void Gimmick2()
    {
        if (enterArea[3].enterAreaFlag) Gate(gateObjects[0], false, 2, 0, true, ref enterArea[3].enterAreaFlag);
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0])
        {
            ActiveLight(lightObjects[0], 2, 0, false, ref defeatGateFlag[0]);
            Gate(gateObjects[0], true, 2, 0, false, ref defeatGateFlag[0]);
            Gate(gateObjects[1], true, 2, 1, true, ref defeatGateFlag[0]);
        }
    }
    // 左下エリアの敵撃破ギミック
    public void Gimmick3()
    {
        if (enterArea[2].enterAreaFlag) Gate(gateObjects[1], false, 2, 1, true, ref enterArea[2].enterAreaFlag);
        if (enemys[1].transform.childCount == 0 && defeatGateFlag[1])
        {
            ActiveLight(lightObjects[1], 2, 1, false, ref defeatGateFlag[0]);
            Gate(gateObjects[1], true, 2, 1, false, ref defeatGateFlag[1]);
            ActiveObject(buttonObjects[1], 0.1f, 0 ,true, ref defeatGateFlag[1]);
        }
        if (buttonObjects[1].GetComponent<ButtonManager>().buttonFlag) Gate(gateObjects[2], true, 2, 2, true, ref buttonObjects[1].GetComponent<ButtonManager>().buttonFlag);
    }
}