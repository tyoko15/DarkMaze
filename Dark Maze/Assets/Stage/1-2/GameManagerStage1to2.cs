using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManagerStage1to2 : GeneralStageManager
{
    void Start()
    {
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
        if(GameObject.Find("DataManager") != null)
        {
            int dataNum = GameObject.Find("DataManager").GetComponent<DataManager>().useDataNum;
            player.GetComponent<PlayerController>().clearStageNum = GameObject.Find("DataManager").GetComponent<DataManager>().data[dataNum].clearStageNum;
        }
        else if(GameObject.Find("DataManager") == null) player.GetComponent<PlayerController>().clearStageNum = 1;
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
                Gimmick4(); 
                Goal();
                if (menuFlag) status = GameStatus.menu;
                playerController.status = 1;
                break;
            case GameStatus.stop:
                Gimmick1();
                Gimmick2();
                Gimmick3();
                Gimmick4();
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
                if (dataManager.data[dataNum].clearStageNum == 2) dataManager.data[dataNum].clearStageNum = 3;
                dataManager.data[dataNum].selectStageNum = 2;
                dataManager.SaveData(dataManager.useDataNum, dataManager.data[dataManager.useDataNum].playerName, dataManager.data[dataNum].clearStageNum, dataManager.data[dataNum].selectStageNum);
            }
            SceneManager.LoadScene("StageSelect");
        }
    }
    // 右上エリアの箱を感圧版に置くギミック
    public void Gimmick1()
    {
        PreSenceGate(gateObjects[0], lightObjects[0], cameraPointObjects[0], buttonObjects[0].GetComponent<ButtonManager>().buttonFlag, 2, 0);
    }
    // 左下エリアの回転ギミック
    public void Gimmick2()
    {
        //if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag) PreAreaRotation(areas[1], lightObjects[0], cameraPointObjects[0], -1, 90, 2, 0, true, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);
        if (buttonObjects[1].GetComponent<ButtonManager>().buttonFlag) PreAreaRotation(areas[2], lightObjects[1], cameraPointObjects[1], -1, 90, 2, 0, true, ref buttonObjects[1].GetComponent<ButtonManager>().buttonFlag);
    }
    // 右下エリアの敵撃破＆離れたボタンギミック
    public void Gimmick3()
    {
        if (enterArea[3].enterAreaFlag) PreGate(gateObjects[1], lightObjects[2], cameraPointObjects[2], false, 2, 1, true, ref enterArea[3].enterAreaFlag);
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0])
        {
            ActiveObject(activeObject[0], 2, 0, false, ref defeatGateFlag[0]);
            ActiveLight(lightObjects[0], 2 , 0, false, ref defeatGateFlag[0]);
            Gate(gateObjects[1], true, 2, 1, true, ref defeatGateFlag[0]);            
        }
        if (buttonObjects[2].GetComponent<ButtonManager>().buttonFlag) Gate(gateObjects[2], true, 2, 1, true, ref buttonObjects[2].GetComponent<ButtonManager>().buttonFlag);
    }
    // 右上エリアのゲートオープンギミック
    public void Gimmick4()
    {
        if (buttonObjects[3].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[2], -1, 180, 2, 0, true, ref buttonObjects[3].GetComponent<ButtonManager>().buttonFlag);
    }
}