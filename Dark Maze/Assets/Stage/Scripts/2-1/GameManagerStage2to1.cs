using UnityEngine;

/// <summary>
/// ステージ 2-1 専用の進行管理クラス
/// 複数ボタンの同時押し判定や、オブジェクトのマテリアル状態によるフラグ制御を導入
/// </summary>
public class GameManagerStage2to1 : GeneralStageManager
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
    /// 左下エリア：敵全滅でスイッチが出現し、エリアを回転させる
    /// </summary>
    public void Gimmick1()
    {
        // エリア進入時に門を閉じる
        if (enterArea[2].enterEnemyAreaFlag) Gate(gateObjects[0], lightObjects[0], cameraPointObjects[0], false, 1.25f, 0, true, ref enterArea[2].enterEnemyAreaFlag);

        // 敵が全滅したら、門を開けて「地形回転スイッチ」を出現させる
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0])
        {
            ActiveLight(areaLightObjects[0], 1, 0, false, ref defeatGateFlag[0]);
            Gate(gateObjects[0], lightObjects[1], null, true, 2, 0, false, ref defeatGateFlag[0]);
            ActiveObject(buttonObjects[0], lightObjects[2], cameraPointObjects[1], 2, 1, true, ref defeatGateFlag[0]);
        }

        // 出現したスイッチを押すと、エリア[2]が90度回転
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[2], null, cameraPointObjects[2],  1, 90, 2, 0, true, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);

        // 正解の道になった時
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag && !correctJudgeFlag[0]) correctJudgeFlag[0] = true;
        if (areas[2].transform.eulerAngles.y == 90f && !correctFlag[0] && correctJudgeFlag[0])
        {
            AudioManager.Instance.PlaySE(AudioManager.SEName.gimmickSes, 7);
            correctFlag[0] = true;
        }
        else if (areas[2].transform.eulerAngles.y != 90f) correctFlag[0] = false;
        if (!buttonObjects[0].GetComponent<ButtonManager>().buttonFlag && correctJudgeFlag[0]) correctJudgeFlag[0] = false;
    }

    /// <summary>
    /// 右下エリア：2つのボタン同時押しギミック
    /// 演出中（不透明度が1になるまで）はリセット処理を行う工夫が見られる
    /// </summary>
    public void Gimmick2()
    {
        // 1. ボタン[1]と[2]が両方押されたら、次のボタン[3]を出現させる
        if (buttonObjects[1].GetComponent<ButtonManager>().buttonFlag && buttonObjects[2].GetComponent<ButtonManager>().buttonFlag) ActiveObject(buttonObjects[3], lightObjects[3], cameraPointObjects[3], 2, 1, true, ref buttonObjects[2].GetComponent<ButtonManager>().buttonFlag);

        // 2. ボタン[3]が出現演出中（アルファ値が1になるまで）に、片方のフラグを管理する
        // ※ refで渡されたbuttonFlagが演出終了でfalseになる仕組みを利用したリセット処理
        if (!buttonObjects[2].GetComponent<ButtonManager>().buttonFlag && buttonObjects[3].activeSelf && buttonObjects[3].GetComponent<MeshRenderer>().materials[1].color.a == 1f) buttonObjects[1].GetComponent<ButtonManager>().buttonFlag = false;

        // ギミック正解になった時
        if ((buttonObjects[1].GetComponent<ButtonManager>().buttonFlag && buttonObjects[2].GetComponent<ButtonManager>().buttonFlag) && !correctJudgeFlag[1]) correctJudgeFlag[1] = true;
        if ((buttonObjects[1].GetComponent<ButtonManager>().buttonFlag && buttonObjects[2].GetComponent<ButtonManager>().buttonFlag) && !correctFlag[1] && correctJudgeFlag[1])
        {
            AudioManager.Instance.PlaySE(AudioManager.SEName.gimmickSes, 8);
            correctFlag[1] = true;
        }
        else if (!buttonObjects[1].GetComponent<ButtonManager>().buttonFlag && !buttonObjects[2].GetComponent<ButtonManager>().buttonFlag) correctFlag[1] = false;
        if (!buttonObjects[3].GetComponent<ButtonManager>().buttonFlag && correctJudgeFlag[1]) correctJudgeFlag[1] = false;

        // 3. 出現したボタン[3]を押すとエリア[3]が回転
        if (buttonObjects[3].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[3], lightObjects[4], cameraPointObjects[4], 1, 90, 2, 1, true, ref buttonObjects[3].GetComponent<ButtonManager>().buttonFlag);

        // 正解の道になった時
        if (buttonObjects[3].GetComponent<ButtonManager>().buttonFlag && !correctJudgeFlag[2]) correctJudgeFlag[2] = true;
        if (areas[3].transform.eulerAngles.y == 90f && !correctFlag[2] && correctJudgeFlag[2])
        {
            AudioManager.Instance.PlaySE(AudioManager.SEName.gimmickSes, 7);
            correctFlag[2] = true;
        }
        else if (areas[3].transform.eulerAngles.y != 90f) correctFlag[2] = false;
        if (!buttonObjects[3].GetComponent<ButtonManager>().buttonFlag && correctJudgeFlag[2]) correctJudgeFlag[2] = false;
    }

    /// <summary>
    /// 右上エリア：オーソドックスな敵撃破による扉開放
    /// </summary>
    public void Gimmick3()
    {
        if (enterArea[1].enterEnemyAreaFlag) Gate(gateObjects[1], lightObjects[5], cameraPointObjects[5], false, 1.25f, 1, true, ref enterArea[1].enterEnemyAreaFlag);
        if (enemys[1].transform.childCount == 0 && defeatGateFlag[1])
        {
            ActiveLight(areaLightObjects[1], 1, 1, false, ref defeatGateFlag[1]);
            Gate(gateObjects[1], null, null, true, 1.25f, 1, false, ref defeatGateFlag[1]);
            Gate(gateObjects[3], null, null, false, 1.25f, 3, false, ref defeatGateFlag[1]);
            Gate(gateObjects[2], lightObjects[6], cameraPointObjects[6], true, 1.25f, 2, true, ref defeatGateFlag[1]);
        }
    }
}