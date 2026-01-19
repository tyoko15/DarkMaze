public class GameManagerStage1to2 : GeneralStageManager
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
    // 右上エリアの箱を感圧版に置くギミック
    public void Gimmick1()
    {
        PreSenceGate(gateObjects[0], lightObjects[0], cameraPointObjects[0], buttonObjects[0].GetComponent<ButtonManager>().buttonFlag, buttonObjects[0].GetComponent<ButtonManager>().completeFlag, 2, 0);
    }
    // 左下エリアの回転ギミック
    public void Gimmick2()
    {
        if (buttonObjects[1].GetComponent<ButtonManager>().buttonFlag) PreAreaRotation(areas[2], lightObjects[1], cameraPointObjects[1] , -1, 90, 2, 0, true, ref buttonObjects[1].GetComponent<ButtonManager>().buttonFlag);
    }
    // 右下エリアの敵撃破＆離れたボタンギミック
    public void Gimmick3()
    {
        if (enterArea[3].enterAreaFlag) PreGate(gateObjects[1], lightObjects[2], cameraPointObjects[2], false, 2, 1, true, ref enterArea[3].enterAreaFlag);
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0])
        {
            PreActiveLight(areaLightObjects[0], 2 , 0, false, ref defeatGateFlag[0]);
            PreGate(gateObjects[1], null, null, true, 2, 1, false, ref defeatGateFlag[0]);            
            PreActiveObject(activeObject[0], lightObjects[3], cameraPointObjects[3], 2, 0, true, ref defeatGateFlag[0]);
        }
        if (buttonObjects[2].GetComponent<ButtonManager>().buttonFlag) PreGate(gateObjects[2],lightObjects[4], cameraPointObjects[4], true, 2, 1, true, ref buttonObjects[2].GetComponent<ButtonManager>().buttonFlag);
    }
    // 右上エリアのゲートオープンギミック
    public void Gimmick4()
    {
        if (buttonObjects[3].GetComponent<ButtonManager>().buttonFlag) PreAreaRotation(areas[2], lightObjects[1], cameraPointObjects[1] , -1, 180, 2, 0, true, ref buttonObjects[3].GetComponent<ButtonManager>().buttonFlag);
    }
}