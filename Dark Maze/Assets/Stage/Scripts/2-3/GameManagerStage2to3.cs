/// <summary>
/// ステージ 2-3 専用の進行管理クラス
/// 同一エリアに対する双方向回転ギミック（リバーシブルなパズル）を制御する
/// </summary>
public class GameManagerStage2to3 : GeneralStageManager
{
    void Start()
    {
        correctFlag = new bool[6];
        correctJudgeFlag = new bool[6];
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
    /// 敵全滅による明かりの点灯と、別エリアの回転
    /// </summary>
    public void Gimmick1()
    {
        // 敵グループ[0]を倒すとエリアライト[0]が点灯
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0]) ActiveLight(areaLightObjects[0], 1, 0, false, ref defeatGateFlag[0]);

        // ボタン[0]でエリア[3]を正方向(1)に90度回転
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[3], lightObjects[0], cameraPointObjects[0], 1, 90, 2, 0, true, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);

        // ギミック正解になった時
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag && !correctJudgeFlag[0]) correctJudgeFlag[0] = true;
        if (areas[3].transform.eulerAngles.y == 90f && !correctFlag[0] && correctJudgeFlag[0])
        {
            AudioManager.Instance.PlaySE(AudioManager.SEName.gimmickSes, 8);
            correctFlag[0] = true;
        }
        else if (areas[3].transform.eulerAngles.y != 90f) correctFlag[0] = false;
        if (!buttonObjects[0].GetComponent<ButtonManager>().buttonFlag && correctJudgeFlag[0]) correctJudgeFlag[0] = false;

        // ギミック正解になった時
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag && !correctJudgeFlag[2]) correctJudgeFlag[2] = true;
        if (enterArea[1].enterAreaFlag && areas[3].transform.eulerAngles.y == 270f && !correctFlag[2] && correctJudgeFlag[2])
        {
            AudioManager.Instance.PlaySE(AudioManager.SEName.gimmickSes, 8);
            correctFlag[2] = true;
        }
        else if (!enterArea[1].enterAreaFlag || areas[3].transform.eulerAngles.y != 270f) correctFlag[2] = false;
        if (!buttonObjects[0].GetComponent<ButtonManager>().buttonFlag && correctJudgeFlag[2]) correctJudgeFlag[2] = false;
        
        // 正解の道になった時
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag && !correctJudgeFlag[1]) correctJudgeFlag[1] = true;
        if (areas[3].transform.eulerAngles.y == 180f && !correctFlag[1] && correctJudgeFlag[1])
        {
            AudioManager.Instance.PlaySE(AudioManager.SEName.gimmickSes, 7);
            correctFlag[1] = true;
        }
        else if (areas[3].transform.eulerAngles.y != 180f) correctFlag[1] = false;
        if (!buttonObjects[0].GetComponent<ButtonManager>().buttonFlag && correctJudgeFlag[1]) correctJudgeFlag[1] = false;
    }

    /// <summary>
    /// エリア[1]の正方向回転
    /// </summary>
    public void Gimmick2()
    {
        if (buttonObjects[1].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[1], lightObjects[1], cameraPointObjects[1], 1, 90, 2, 1, true, ref buttonObjects[1].GetComponent<ButtonManager>().buttonFlag);

        // ギミック正解になった時
        if (buttonObjects[1].GetComponent<ButtonManager>().buttonFlag && !correctJudgeFlag[3]) correctJudgeFlag[3] = true;
        if (enterArea[3].enterAreaFlag && areas[1].transform.eulerAngles.y == 90f && !correctFlag[3] && correctJudgeFlag[3])
        {
            AudioManager.Instance.PlaySE(AudioManager.SEName.gimmickSes, 8);
            correctFlag[3] = true;
        }
        else if (!enterArea[3].enterAreaFlag || areas[1].transform.eulerAngles.y != 90f) correctFlag[3] = false;
        if (!buttonObjects[1].GetComponent<ButtonManager>().buttonFlag && correctJudgeFlag[3]) correctJudgeFlag[3] = false;
    }

    /// <summary>
    /// エリア[1]の逆方向回転
    /// </summary>
    public void Gimmick3()
    {
        if (buttonObjects[2].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[1], lightObjects[1], cameraPointObjects[1], -1, 90, 2, 1, true, ref buttonObjects[2].GetComponent<ButtonManager>().buttonFlag);

        // ギミック正解になった時
        if (buttonObjects[2].GetComponent<ButtonManager>().buttonFlag && !correctJudgeFlag[4]) correctJudgeFlag[4] = true;
        if (enterArea[1].enterAreaFlag && areas[1].transform.eulerAngles.y == 0f && !correctFlag[4] && correctJudgeFlag[4])
        {
            AudioManager.Instance.PlaySE(AudioManager.SEName.gimmickSes, 7);
            correctFlag[4] = true;
        }
        else if (!enterArea[1].enterAreaFlag || areas[1].transform.eulerAngles.y != 0f) correctFlag[4] = false;
        if (!buttonObjects[2].GetComponent<ButtonManager>().buttonFlag && correctJudgeFlag[4]) correctJudgeFlag[4] = false;
    }
}