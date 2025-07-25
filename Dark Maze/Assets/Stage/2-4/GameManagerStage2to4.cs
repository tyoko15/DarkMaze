using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManagerStage2to4 : GeneralStageManager
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
        if (GameObject.Find("DataManager") != null)
        {
            int dataNum = GameObject.Find("DataManager").GetComponent<DataManager>().useDataNum;
            player.GetComponent<PlayerController>().clearStageNum = GameObject.Find("DataManager").GetComponent<DataManager>().data[dataNum].clearStageNum;
        }
        else if (GameObject.Find("DataManager") == null) player.GetComponent<PlayerController>().clearStageNum = 8;
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
                if (dataManager.data[dataNum].clearStageNum == 8) dataManager.data[dataNum].clearStageNum = 10;
                dataManager.data[dataNum].selectStageNum = 8;
                dataManager.SaveData(dataManager.useDataNum, dataManager.data[dataManager.useDataNum].playerName, dataManager.data[dataNum].clearStageNum, dataManager.data[dataNum].selectStageNum);
            }
            SceneManager.LoadScene("StageSelect");
        }
    }
    // “G‘SŒ‚”j‚Å‰ð•úƒMƒ~ƒbƒN
    public void Gimmick1()
    {
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0])
        {
            PreActiveLight(areaLightObjects[0], 2, 0, false, ref defeatGateFlag[0]);
            PreGate(gateObjects[0], lightObjects[0], cameraPointObjects[0], true, 2, 0, true, ref defeatGateFlag[0]);
        }
    }
    // 
    public void Gimmick2()
    {
        if (enterArea[2].enterAreaFlag) PreGate(gateObjects[0], lightObjects[1], cameraPointObjects[1], false, 2, 2, true, ref enterArea[2].enterAreaFlag);
        if (enemys[2].transform.childCount == 0 && defeatGateFlag[2])
        {
            PreActiveLight(areaLightObjects[2], 2, 2, false, ref defeatGateFlag[2]);
            PreGate(gateObjects[1], lightObjects[1], null, true, 2, 1, false, ref defeatGateFlag[2]);
            PreGate(gateObjects[2], lightObjects[2], cameraPointObjects[2], true, 2, 2, true, ref defeatGateFlag[2]);
        }
    }
    // 
    public void Gimmick3()
    {
        if (enterArea[3].enterAreaFlag) PreGate(gateObjects[2], lightObjects[1], cameraPointObjects[3], false, 2, 2, true, ref enterArea[3].enterAreaFlag);
        if (enemys[3].transform.childCount == 0 && defeatGateFlag[3])
        {
            PreActiveLight(areaLightObjects[3], 2, 3, false, ref defeatGateFlag[3]);
            PreGate(gateObjects[2], lightObjects[2], null, true, 2, 2, false, ref defeatGateFlag[3]);
            PreGate(gateObjects[3], null, null, true, 2, 3, false, ref defeatGateFlag[3]);
            PreGate(gateObjects[4], null, null, true, 2, 4, false, ref defeatGateFlag[3]);
            PreGate(gateObjects[5], lightObjects[2], cameraPointObjects[4], true, 2, 5, true, ref defeatGateFlag[3]);
        }
    }
    // 
    public void Gimmick4()
    {
        if (enterArea[1].enterAreaFlag)
        {
            PreGate(gateObjects[4], null, null, false, 2, 4, false, ref enterArea[1].enterAreaFlag);
            PreGate(gateObjects[5], lightObjects[2], cameraPointObjects[5], false, 2, 5, true, ref enterArea[1].enterAreaFlag);
        }
        if (enemys[1].transform.childCount == 0 && defeatGateFlag[1])
        {
            PreActiveLight(areaLightObjects[1], 2, 1, false, ref defeatGateFlag[1]);
            PreGate(gateObjects[4], null, null, true, 2, 4, false, ref defeatGateFlag[1]);
            PreGate(gateObjects[5], lightObjects[2], null, true, 2, 5, false, ref defeatGateFlag[1]);
            PreGate(gateObjects[6], null, null, true, 2, 6, false, ref defeatGateFlag[1]);
            PreActiveObject(buttonObjects[0], lightObjects[3], cameraPointObjects[6], 2, 0, true, ref defeatGateFlag[1]);
        }
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag) PreAreaRotation(areas[1], null, cameraPointObjects[7], -1, 180, 2, 0, true, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);
    }
}