using UnityEngine;
public class GameManagerStage2to1 : GeneralStageManager
{
    void Start()
    {
        StartData();
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
                MenuUIControl();
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

    // 左下エリアの敵撃破でボタン出現
    // ボタンは左下エリアの回転ギミック
    public void Gimmick1()
    {
        if (enterArea[2].enterAreaFlag) PreGate(gateObjects[0], lightObjects[0], cameraPointObjects[0], false, 2, 0, true, ref enterArea[2].enterAreaFlag);
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0])
        {
            PreActiveLight(areaLightObjects[0], 2, 0, false, ref defeatGateFlag[0]);
            PreGate(gateObjects[0], lightObjects[1], null, true, 2, 0, false, ref defeatGateFlag[0]);
            PreActiveObject(buttonObjects[0], lightObjects[2], cameraPointObjects[1], 2, 1, true, ref defeatGateFlag[0]);
        }
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag) PreAreaRotation(areas[2], null, cameraPointObjects[2],  1, 90, 2, 0, true, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);
    }
    // 右下エリアのボタンを同時押しでボタン出現
    // ボタンは右下エリアの回転ギミック
    public void Gimmick2()
    {
        if (buttonObjects[1].GetComponent<ButtonManager>().buttonFlag && buttonObjects[2].GetComponent<ButtonManager>().buttonFlag) PreActiveObject(buttonObjects[3], lightObjects[3], cameraPointObjects[3], 2, 1, true, ref buttonObjects[2].GetComponent<ButtonManager>().buttonFlag);        
        if (!buttonObjects[2].GetComponent<ButtonManager>().buttonFlag && buttonObjects[3].activeSelf && buttonObjects[3].GetComponent<MeshRenderer>().materials[1].color.a == 1f) buttonObjects[1].GetComponent<ButtonManager>().buttonFlag = false;
        if (buttonObjects[3].GetComponent<ButtonManager>().buttonFlag) PreAreaRotation(areas[3], lightObjects[4], cameraPointObjects[4], 1, 90, 2, 1, true, ref buttonObjects[3].GetComponent<ButtonManager>().buttonFlag);
    }
    // 右上エリアの敵撃破で扉開放ギミック
    public void Gimmick3()
    {
        if (enterArea[1].enterAreaFlag) PreGate(gateObjects[1], lightObjects[5], cameraPointObjects[5], false, 2, 1, true, ref enterArea[1].enterAreaFlag);
        if (enemys[1].transform.childCount == 0 && defeatGateFlag[1])
        {
            PreActiveLight(areaLightObjects[1], 2, 1, false, ref defeatGateFlag[1]);
            PreGate(gateObjects[1], null, null, true, 2, 1, false, ref defeatGateFlag[1]);
            PreGate(gateObjects[2], lightObjects[6], cameraPointObjects[6], true, 2, 2, true, ref defeatGateFlag[1]);
        }
    }
}