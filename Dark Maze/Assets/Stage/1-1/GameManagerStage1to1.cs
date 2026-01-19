public class GameManagerStage1to1 : GeneralStageManager
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
                if (playerController.playerHP <= 0) status = GameStatus.over;
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
    // 右上エリアの回転ギミック
    public void Gimmick1()
    {
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag) PreAreaRotation(areas[1], lightObjects[0], cameraPointObjects[0], -1, 90, 2, 0, true, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);
    }
    // 右下エリアの敵撃破ギミック
    public void Gimmick2()
    {
        if (enterArea[3].enterAreaFlag) PreGate(gateObjects[0], lightObjects[1], cameraPointObjects[1], false, 2, 0, true, ref enterArea[3].enterAreaFlag);
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0])
        {
            PreActiveLight(areaLightObjects[0], 1.5f, 0, false, ref defeatGateFlag[0]);
            PreGate(gateObjects[0], null, null, true, 2, 0, false, ref defeatGateFlag[0]);
            PreGate(gateObjects[1], lightObjects[2], cameraPointObjects[2], true, 2, 1, true, ref defeatGateFlag[0]);
        }
    }
    // 左下エリアの敵撃破ギミック
    public void Gimmick3()
    {
        if (enterArea[2].enterAreaFlag) PreGate(gateObjects[1], lightObjects[3], cameraPointObjects[3], false, 2, 1, true, ref enterArea[2].enterAreaFlag);
        if (enemys[1].transform.childCount == 0 && defeatGateFlag[1])
        {
            PreActiveLight(areaLightObjects[1], 1.5f, 1, false, ref defeatGateFlag[1]);
            PreGate(gateObjects[1], lightObjects[3], null, true, 2, 1, false, ref defeatGateFlag[1]);
            PreActiveObject(buttonObjects[1],lightObjects[4], cameraPointObjects[4], 2, 0 ,true, ref defeatGateFlag[1]);
        }
        if (buttonObjects[1].GetComponent<ButtonManager>().buttonFlag) PreGate(gateObjects[2],lightObjects[5], cameraPointObjects[5], true, 2, 2, true, ref buttonObjects[1].GetComponent<ButtonManager>().buttonFlag);
    }
}