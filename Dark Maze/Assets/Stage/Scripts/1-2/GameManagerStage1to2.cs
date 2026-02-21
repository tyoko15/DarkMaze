/// <summary>
/// ステージ 1-2 専用の進行管理クラス
/// 1-1よりも複雑な連鎖ギミックや箱パズルを制御する
/// </summary>
public class GameManagerStage1to2 : GeneralStageManager
{
    void Start()
    {
        StartData(); // 親クラスの初期設定（プレイヤーや各オブジェクトの参照取得）
    }

    void Update()
    {
        // ゲームの現在の状態に応じて処理を分岐
        switch (status)
        {
            case GameStatus.start:
                playerController.status = 0; // プレイヤーを「開始演出待ち」状態に
                StartAnime();                // ステージ開始のアニメーション
                break;

            case GameStatus.play:
                // プレイ中のメインロジック
                Gimmick1();
                Gimmick2();
                Gimmick3();
                Gimmick4();
                JudgeOver();                 // 死亡判定チェック
                Goal();                      // ゴール判定チェック
                if (menuFlag) status = GameStatus.menu;
                playerController.status = 1; // プレイヤーを「操作可能」状態に
                DisplayGuideTexts();
                break;

            case GameStatus.stop:
                // ギミック演出中などの一時停止状態
                Gimmick1();
                Gimmick2();
                Gimmick3();
                Gimmick4();
                Goal();
                playerController.status = 2; // プレイヤーを「停止」状態に
                break;

            case GameStatus.menu:
                MenuUIControl();             // メニュー画面のUI制御
                if (!menuFlag) status = GameStatus.play;
                playerController.status = 3;
                break;

            case GameStatus.over:
                Over();                      // ゲームオーバー処理
                OverUIControl();             // オーバー表示UI
                playerController.status = 4;
                break;

            case GameStatus.clear:
                playerController.status = 5; // プレイヤーを「クリアポーズ」状態に
                EndAnime();                  // 終了演出
                break;
        }
    }

    // --- ギミック詳細 ---

    /// <summary>
    /// 右上エリア：箱を感圧版に設置するパズル
    /// completeFlag（箱が静止して置かれたか）を見てゲートを制御
    /// </summary>
    public void Gimmick1()
    {
        //SenceGate(gateObjects[0], lightObjects[0], cameraPointObjects[0], buttonObjects[0].GetComponent<ButtonManager>().buttonFlag, buttonObjects[0].GetComponent<ButtonManager>().completeFlag, 2, 0);
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag && buttonObjects[0].GetComponent<ButtonManager>().completeFlag) Gate(gateObjects[0], lightObjects[0], cameraPointObjects[0], true, 2, 1, true, ref buttonObjects[0].GetComponent<ButtonManager>().completeFlag);
    }

    /// <summary>
    /// 左下エリア：スイッチによる地形回転
    /// </summary>
    public void Gimmick2()
    {
        if (buttonObjects[1].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[2], lightObjects[1], cameraPointObjects[1] , -1, 90, 2, 0, true, ref buttonObjects[1].GetComponent<ButtonManager>().buttonFlag);
    }

    /// <summary>
    /// 右下エリア：敵撃破後に別の場所にボタンが出現する連鎖ギミック
    /// </summary>
    public void Gimmick3()
    {
        // 1. エリア進入でゲートを閉じる
        if (enterArea[3].enterAreaFlag) Gate(gateObjects[1], lightObjects[2], cameraPointObjects[2], false, 2, 1, true, ref enterArea[3].enterAreaFlag);
        
        // 2. 敵が全滅したらライトを点け、ゲートを開け、さらに別の「ボタン」を出現させる
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0])
        {
            ActiveLight(areaLightObjects[0], 2 , 0, false, ref defeatGateFlag[0]);
            Gate(gateObjects[1], null, null, true, 2, 1, false, ref defeatGateFlag[0]);
            // 離れた位置にある activeObject[0]（恐らくスイッチ）を起動
            ActiveObject(activeObject[0], lightObjects[3], cameraPointObjects[3], 2, 0, true, ref defeatGateFlag[0]);
        }

        // 3. 出現したボタン(buttonObjects[2])を押すと、別のゲートが開く
        if (buttonObjects[2].GetComponent<ButtonManager>().buttonFlag)
        {
            ActiveLight(areaLightObjects[1], 2, 1, false, ref buttonObjects[2].GetComponent<ButtonManager>().buttonFlag);
            Gate(gateObjects[2], lightObjects[4], cameraPointObjects[4], true, 2, 1, true, ref buttonObjects[2].GetComponent<ButtonManager>().buttonFlag);
        }
    }

    /// <summary>
    /// 右上エリア：別のスイッチにより地形回転
    /// </summary>
    public void Gimmick4()
    {
        if (buttonObjects[3].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[2], lightObjects[1], cameraPointObjects[1] , -1, 180, 2, 0, true, ref buttonObjects[3].GetComponent<ButtonManager>().buttonFlag);
    }
}