/// <summary>
/// ステージ 2-4 専用の進行管理クラス
/// 複数のゲート連動やエリア進入による退路遮断など、複雑なシーケンスを制御する
/// </summary>
public class GameManagerStage2to4 : GeneralStageManager
{
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
                MenuUIControl();
                if (!menuFlag) status = GameStatus.play;
                playerController.status = 3;
                break;
            case GameStatus.over:
                Over();
                OverUIControl();
                playerController.status = 4;
                break;

            case GameStatus.clear:
                playerController.status = 5;
                EndAnime();
                break;
        }
    }

    // --- ギミック詳細 ---

    /// <summary>
    /// 第1エリア：小手調べの敵全滅ギミック
    /// </summary>
    public void Gimmick1()
    {
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0])
        {
            ActiveLight(areaLightObjects[0], 1, 0, false, ref defeatGateFlag[0]);
            Gate(gateObjects[0], lightObjects[0], cameraPointObjects[0], true, 1.25f, 0, true, ref defeatGateFlag[0]);
        }
    }

    /// <summary>
    /// 第2エリア：敵撃破で次のエリアへの道（ゲート2つ）を同時に開く
    /// </summary>
    public void Gimmick2()
    {
        if (enemys[2].transform.childCount == 0 && defeatGateFlag[2])
        {
            ActiveLight(areaLightObjects[2], 1, 2, false, ref defeatGateFlag[2]);
            // 手前のゲートを開けつつ、奥のゲート[2]へカメラを向ける演出
            Gate(gateObjects[1], lightObjects[1], null, true, 1.25f, 1, false, ref defeatGateFlag[2]);
            Gate(gateObjects[2], lightObjects[2], cameraPointObjects[2], true, 1.25f, 2, true, ref defeatGateFlag[2]);
        }
    }

    /// <summary>
    /// 第3エリア：大規模連動。敵撃破で多数のゲートが反応
    /// </summary>
    public void Gimmick3()
    {
        //if (enterArea[3].enterAreaFlag) PreGate(gateObjects[2], lightObjects[1], cameraPointObjects[3], false, 2, 2, true, ref enterArea[3].enterAreaFlag);
        if (enemys[3].transform.childCount == 0 && defeatGateFlag[3])
        {
            ActiveLight(areaLightObjects[3], 1, 3, false, ref defeatGateFlag[3]);
            // 複数のゲートを順次、あるいは同時に開放
            Gate(gateObjects[2], lightObjects[2], null, true, 1.25f, 2, false, ref defeatGateFlag[3]);
            Gate(gateObjects[3], null, null, true, 1.25f, 3, false, ref defeatGateFlag[3]);
            Gate(gateObjects[4], null, null, true, 1.25f, 4, false, ref defeatGateFlag[3]);
            // 最後にカメラ演出を伴ってゲート[5]を開放
            Gate(gateObjects[5], lightObjects[2], cameraPointObjects[4], true, 1.25f, 5, true, ref defeatGateFlag[3]);
        }
    }

    /// <summary>
    /// 第4エリア：進入による封鎖と、敵全滅後の地形回転
    /// </summary>
    public void Gimmick4()
    {
        // エリア1進入時に背後の門を閉める演出
        if (enterArea[1].enterEnemyAreaFlag)
        {
            Gate(gateObjects[4], null, null, false, 1.25f, 4, false, ref enterArea[1].enterEnemyAreaFlag);
            Gate(gateObjects[5], lightObjects[2], cameraPointObjects[5], false, 1.25f, 5, true, ref enterArea[1].enterEnemyAreaFlag);
        }
        if (enemys[1].transform.childCount == 0 && defeatGateFlag[1])
        {
            ActiveLight(areaLightObjects[1], 1, 1, false, ref defeatGateFlag[1]);
            Gate(gateObjects[4], null, null, true,  2, 4, false, ref defeatGateFlag[1]);
            Gate(gateObjects[5], lightObjects[2], null, true, 2, 5, false, ref defeatGateFlag[1]);
            Gate(gateObjects[6], null, null, true, 2, 6, false, ref defeatGateFlag[1]);
            // スイッチを出現させて最終的な地形変化へ
            ActiveObject(buttonObjects[0], lightObjects[3], cameraPointObjects[6], 2, 0, true, ref defeatGateFlag[1]);
        }

        // 最後はエリア1を180度回転させてゴールへの道を作る
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[1], null, cameraPointObjects[7], -1, 180, 2, 0, true, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);

        // 正解の道になった時
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag && !correctJudgeFlag[0]) correctJudgeFlag[0] = true;
        if (enterArea[3].enterAreaFlag && areas[1].transform.eulerAngles.y == 180f && !correctFlag[0] && correctJudgeFlag[0])
        {
            AudioManager.Instance.PlaySE(AudioManager.SEName.gimmickSes, 8);
            correctFlag[0] = true;
        }
        else if (!enterArea[3].enterAreaFlag || areas[1].transform.eulerAngles.y != 180f) correctFlag[0] = false;
        if (!buttonObjects[0].GetComponent<ButtonManager>().buttonFlag && correctJudgeFlag[0]) correctJudgeFlag[0] = false;
        // 正解の道になった時
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag && !correctJudgeFlag[1]) correctJudgeFlag[1] = true;
        if (enterArea[1].enterAreaFlag && areas[1].transform.eulerAngles.y == 0f && !correctFlag[1] && correctJudgeFlag[1])
        {
            AudioManager.Instance.PlaySE(AudioManager.SEName.gimmickSes, 7);
            correctFlag[1] = true;
        }
        else if (!enterArea[1].enterAreaFlag || areas[1].transform.eulerAngles.y != 0f) correctFlag[1] = false;
        if (!buttonObjects[0].GetComponent<ButtonManager>().buttonFlag && correctJudgeFlag[1]) correctJudgeFlag[1] = false;
    }
}