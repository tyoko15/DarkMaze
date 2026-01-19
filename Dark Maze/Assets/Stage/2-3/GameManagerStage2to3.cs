public class GameManagerStage2to3 : GeneralStageManager
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
    // 右下エリア回転ギミック
    public void Gimmick1()
    {
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0]) PreActiveLight(areaLightObjects[0], 2, 0, false, ref defeatGateFlag[0]);
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag) PreAreaRotation(areas[3], lightObjects[0], cameraPointObjects[0], 1, 90, 2, 0, true, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);
    }
    // 
    public void Gimmick2()
    {
        if (buttonObjects[1].GetComponent<ButtonManager>().buttonFlag) PreAreaRotation(areas[1], lightObjects[1], cameraPointObjects[1], 1, 90, 2, 1, true, ref buttonObjects[1].GetComponent<ButtonManager>().buttonFlag);
    }
    // 
    public void Gimmick3()
    {
        if (buttonObjects[2].GetComponent<ButtonManager>().buttonFlag) PreAreaRotation(areas[1], lightObjects[1], cameraPointObjects[1], -1, 90, 2, 1, true, ref buttonObjects[2].GetComponent<ButtonManager>().buttonFlag);
    }
}