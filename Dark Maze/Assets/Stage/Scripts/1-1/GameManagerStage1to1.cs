/// <summary>
/// ステージ 1-1 専用の進行管理クラス
/// ギミックの発動条件や演出のシーケンスを定義する
/// </summary>
public class GameManagerStage1to1 : GeneralStageManager
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
    /// 右上エリア：スイッチによる地形回転
    /// </summary>
    public void Gimmick1()
    {
        // ボタンが押されたら AreaRotation を実行
        if (buttonObjects[0].GetComponent<ButtonManager>().buttonFlag) AreaRotation(areas[1], lightObjects[0], cameraPointObjects[0], -1, 90, 2, 0, true, ref buttonObjects[0].GetComponent<ButtonManager>().buttonFlag);
    }

    /// <summary>
    /// 右下エリア：敵を倒してゲートを開く
    /// </summary>
    public void Gimmick2()
    {
        // エリア進入時に門を閉める演出
        if (enterArea[3].enterAreaFlag) Gate(gateObjects[0], lightObjects[1], cameraPointObjects[1], false, 2, 0, true, ref enterArea[3].enterAreaFlag);
        
        // そのエリアの敵が全滅
        if (enemys[0].transform.childCount == 0 && defeatGateFlag[0])
        {
            ActiveLight(areaLightObjects[0], 1.5f, 0, false, ref defeatGateFlag[0]); // 明かりを灯す
            Gate(gateObjects[0], null, null, true, 2, 0, false, ref defeatGateFlag[0]); // 前の門を開ける
            Gate(gateObjects[1], lightObjects[2], cameraPointObjects[2], true, 2, 1, true, ref defeatGateFlag[0]); // 次の門を開ける
        }
    }

    /// <summary>
    /// 左下エリア：連鎖ギミック
    /// </summary>
    public void Gimmick3()
    {
        // 進入判定
        if (enterArea[2].enterAreaFlag) Gate(gateObjects[1], lightObjects[3], cameraPointObjects[3], false, 2, 1, true, ref enterArea[2].enterAreaFlag);
        
        // 敵全滅判定
        if (enemys[1].transform.childCount == 0 && defeatGateFlag[1])
        {
            ActiveLight(areaLightObjects[1], 1.5f, 1, false, ref defeatGateFlag[1]);
            Gate(gateObjects[1], lightObjects[3], null, true, 2, 1, false, ref defeatGateFlag[1]);
            // 敵を倒したご褒美として、新しいスイッチ(buttonObjects[1])を出現させる
            ActiveObject(buttonObjects[1],lightObjects[4], cameraPointObjects[4], 2, 0 ,true, ref defeatGateFlag[1]);
        }

        // 出現したスイッチを押すと、最終のゲートが開く
        if (buttonObjects[1].GetComponent<ButtonManager>().buttonFlag) Gate(gateObjects[2],lightObjects[5], cameraPointObjects[5], true, 2, 2, true, ref buttonObjects[1].GetComponent<ButtonManager>().buttonFlag);
    }
}