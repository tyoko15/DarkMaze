/// <summary>
/// ステージ 1-3 専用の進行管理クラス
/// 複数エリアの同時回転など、よりダイナミックな環境変化を制御する
/// </summary>
public class GameManagerStage1to3 : GeneralStageManager
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
                playerController.status = 1; // プレイヤーを「操作可能」状態に
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
    /// 左下エリアのボタン：右上エリア(areas[1])と左下エリア(areas[2])を同時に回転させる
    /// 1つのアクションで世界が大きく変わる、1-3の目玉ギミック
    /// </summary>
    public void Gimmick1()
    {
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag)
        {
            // 右上エリアを90度回転（カメラ移動なし）
            AreaRotation(areas[1], lightObjects[0], null, -1, 90, 2, 0, false, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);
            // 左下エリアを90度回転（カメラをこちらに寄せる）
            AreaRotation(areas[2], lightObjects[1], cameraPointObjects[0], -1, 90, 2, 1, true, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);
        }
    }

    /// <summary>
    /// 右下エリア：敵の全滅によるゲート開放
    /// 1-1, 1-2で学んだ基本ルールをここでも応用
    /// </summary>
    public void Gimmick2()
    {
        // 進入時にゲートを閉じて閉じ込める
        if (enterArea[3].enterAreaFlag) Gate(gateObjects[0], lightObjects[2], cameraPointObjects[1], false, 2, 0, true, ref enterArea[3].enterAreaFlag);

        // 敵全滅判定
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0])
        {
            ActiveLight(areaLightObjects[0], 2, 0, false, ref defeatGateFlag[0]);
            Gate(gateObjects[0],lightObjects[2], null, true, 2, 0, false, ref defeatGateFlag[0]);
            Gate(gateObjects[1], lightObjects[3], cameraPointObjects[2], true, 2, 1, true, ref defeatGateFlag[0]);
        }
    }

    /// <summary>
    /// 右上エリアのボタン：右上エリア(areas[1])をさらに180度回転させる
    /// </summary>
    public void Gimmick3()
    {
        if (buttonObjects[1].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[1], lightObjects[0], cameraPointObjects[3], -1, 180, 2, 0, true, ref buttonObjects[1].GetComponent<ButtonManager>().buttonFlag);
    }
}

