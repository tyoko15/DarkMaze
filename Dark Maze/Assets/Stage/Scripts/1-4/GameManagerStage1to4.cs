/// <summary>
/// ステージ 1-4 専用の進行管理クラス
/// 複数条件の組み合わせや、攻略済みフラグの複雑な判定を行う
/// </summary>
public class GameManagerStage1to4 : GeneralStageManager
{
    bool bothflag = true; // 複数条件（Gimmick3）の実行管理用フラグ
    void Start()
    {
        // 親クラスで定義されている初期化処理（参照の取得など）を実行
        StartData();
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
                playerController.status = 1;
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
                MenuUIControl();            // メニュー画面のUI制御
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
    /// 左上エリア：敵を倒すとスイッチが出現し、そのスイッチで地形が回転
    /// </summary>
    public void Gimmick1()
    {
        // 敵全滅判定
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0])
        {
            ActiveLight(areaLightObjects[0], 1, 0, false, ref defeatGateFlag[0]);
            ActiveObject(buttonObjects[0], lightObjects[0], cameraPointObjects[0], 2, 0, true, ref defeatGateFlag[0]);
        }

        // 出現したスイッチを押すとエリア回転
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[0], null, cameraPointObjects[1], -1, 90, 2, 0, true, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);

        // 正解の道の演出
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag && !correctJudgeFlag[0]) correctJudgeFlag[0] = true;
        if ((areas[0].transform.eulerAngles.y == 180f || areas[0].transform.eulerAngles.y == 270f) && !correctFlag[0] && correctJudgeFlag[0])
        {
            AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.gimmickSes, 7);
            correctFlag[0] = true;
        }
        else if (areas[0].transform.eulerAngles.y != 180f && areas[0].transform.eulerAngles.y != 270f) correctFlag[0] = false;
        if (!buttonObjects[0].GetComponent<ButtonManager>().buttonFlag && correctJudgeFlag[0]) correctJudgeFlag[0] = false;
    }

    /// <summary>
    /// 左下と右上：それぞれのエリアで敵を倒し、個別の門を開放する
    /// </summary>
    public void Gimmick2()
    {
        // 左下エリアの進入と開放
        if (enterArea[2].enterEnemyAreaFlag) Gate(gateObjects[0], lightObjects[1], cameraPointObjects[2], false, 1.25f, 0, true, ref enterArea[2].enterEnemyAreaFlag);
        if (enemys[2].transform.childCount == 0 && defeatGateFlag[2])
        {
            ActiveLight(areaLightObjects[2], 1, 2, false, ref defeatGateFlag[2]);
            Gate(gateObjects[0], lightObjects[1], cameraPointObjects[2], true, 1.25f, 0, true, ref defeatGateFlag[2]);
        }

        // 右上エリアの進入と開放
        if (enterArea[1].enterEnemyAreaFlag) Gate(gateObjects[1], lightObjects[2], cameraPointObjects[3], false, 1.25f, 1, true, ref enterArea[1].enterEnemyAreaFlag);
        if (enemys[1].transform.childCount == 0 && defeatGateFlag[1])
        {
            ActiveLight(areaLightObjects[1], 1, 1, false, ref defeatGateFlag[1]);
            Gate(gateObjects[1], null, null, true, 1.25f, 1, false, ref defeatGateFlag[1]);
            Gate(gateObjects[2], lightObjects[3], cameraPointObjects[4], true, 1.25f, 2, true, ref defeatGateFlag[1]);
        }
    }

    /// <summary>
    /// 条件合流：左下(2)と右上(1)の両方の敵を倒した時に初めて道が開く
    /// </summary>
    public void Gimmick3()
    {
        // !defeatGateFlag は「敵を倒し終わった（フラグがfalseになった）」ことを意味する
        if (!defeatGateFlag[2] && !defeatGateFlag[1] && bothflag)
        {
            Gate(gateObjects[3], null, null, true, 2, 3, false, ref bothflag);
            ActiveObject(buttonObjects[1], null, cameraPointObjects[5], 2, 0, true, ref bothflag);
        }

        // 出現したスイッチで左下エリア(areas[2])を回転
        if (buttonObjects[1].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[2], null, cameraPointObjects[6], -1, 90, 2, 0, true, ref buttonObjects[1].GetComponent<ButtonManager>().buttonFlag);

        // 正解の道の演出
        if (buttonObjects[1].GetComponent<ButtonManager>().buttonFlag && !correctJudgeFlag[2]) correctJudgeFlag[2] = true;
        if (areas[2].transform.eulerAngles.y == 270f && !correctFlag[2] && correctJudgeFlag[2])
        {
            AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.gimmickSes, 7);
            correctFlag[2] = true;
        }
        else if (areas[2].transform.eulerAngles.y != 270f) correctFlag[2] = false;
        if (!buttonObjects[1].GetComponent<ButtonManager>().buttonFlag && correctJudgeFlag[2]) correctJudgeFlag[2] = false;
    }

    /// <summary>
    /// 右下エリア：最終試練。敵全滅後に「地形回転スイッチ」と「ゴール出現スイッチ」の2つが出る
    /// </summary>
    public void Gimmick4()
    {
        if (enterArea[3].enterEnemyAreaFlag) Gate(gateObjects[2], lightObjects[3], cameraPointObjects[4], false, 1.25f, 2, true, ref enterArea[3].enterEnemyAreaFlag);
        if (enemys[3].transform.childCount == 0 && defeatGateFlag[3])
        {
            ActiveLight(areaLightObjects[3], 1, 3, false, ref defeatGateFlag[3]);
            // スイッチ2つを同時に出現させる
            ActiveObject(buttonObjects[2], null, null, 1, 2, false, ref defeatGateFlag[3]);
            ActiveObject(buttonObjects[3], lightObjects[4], null, 1, 3, false, ref defeatGateFlag[3]);
            Gate(gateObjects[2], null, cameraPointObjects[8], true, 1.25f, 4, true, ref defeatGateFlag[3]);
        }
        // 最後の地形調整
        if (!defeatGateFlag[3] && buttonObjects[2].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[2], null, cameraPointObjects[6], -1, 90, 2, 0, true, ref buttonObjects[2].GetComponent<ButtonManager>().buttonFlag);
        
        // ゴール出現！
        if (!defeatGateFlag[3] && buttonObjects[3].GetComponent<ButtonManager>().buttonFlag) ActiveObject(goalObject, null, cameraPointObjects[9], 2, 4, true, ref buttonObjects[3].GetComponent<ButtonManager>().buttonFlag);
        // 正解の道の演出
        if (buttonObjects[3].GetComponent<ButtonManager>().buttonFlag && !correctJudgeFlag[3]) correctJudgeFlag[3] = true;
        if (areas[2].transform.eulerAngles.y == 180f && !correctFlag[3] && correctJudgeFlag[3])
        {
            AudioManager.Instance.PlayOneShotSE(AudioManager.SEName.gimmickSes, 8);
            correctFlag[3] = true;
        }
        else if (areas[2].transform.eulerAngles.y != 180f) correctFlag[3] = false;
        if (!buttonObjects[3].GetComponent<ButtonManager>().buttonFlag && correctJudgeFlag[3]) correctJudgeFlag[3] = false;
    }
}