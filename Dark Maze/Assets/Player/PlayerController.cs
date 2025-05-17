using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // 0.start 1.play 2.stop 3.over 4.clear
    [Header("GameManger制御用")]
    [SerializeField] public int status; 

    [Header("プレイヤーの基本情報")]
    [SerializeField] GameObject playerObject;
    [SerializeField] Rigidbody rb;
    [SerializeField] Vector3 gravity;
    [SerializeField] float playerHorizontal;
    [SerializeField] float playerVertical;
    [SerializeField] float playerSpeed;
    bool[] controlFlag = new bool[4];
    [SerializeField] public int clearStageNum;
    [Header("Camera情報")]
    [SerializeField] GameObject mainCamera;
    [Header("プレイヤーのライト情報")]
    [SerializeField] Light playerLight;
    [SerializeField] GameObject lightRangeObject;
    [SerializeField] Image lightGauge;
    [SerializeField, Range(30f, 180f)] float playerLightRange;
    [SerializeField, Range(2.75f, 17.25f)] float lightRangeObjectRange;
    [Header("最拡大するまでにかかる秒数"),SerializeField] float lightSpreadTime;
    float lightSpreadTimer;
    [Header("最縮小するまでにかかる秒数"),SerializeField] float lightShrinkTime;
    float lightRangeMinRange;
    float lightShrinkTimer;
    [SerializeField] Image lightMaxIntervalTimerGauge;
    [SerializeField] float lightMaxIntervalTime;
    float lightMaxIntervalTimer;
    int onLight;
    [Header("プレイヤーの攻撃情報")]
    [SerializeField] bool attackFlag;
    [SerializeField] GameObject sword;
    [SerializeField] float attackTime;
    float attackTimer;
    [Header("プレイヤーのアイテム情報")]
    bool getItemFlag;
    [SerializeField] public bool[] canItemFlag;
    // 0.弓、1.縄、2.
    [SerializeField] GameObject[] itemSlots;
    [SerializeField] Sprite[] itemImageSprites;
    [SerializeField] GameObject itemSelect;
    [SerializeField] GameObject selectObject;
    [SerializeField] int itemSelectNum;
    bool itemSelectFlag;
    bool startSelectFlag;
    bool endSelectFlag = true;
    [SerializeField] float itemTime;
    float[] itemTimer = new float[3];
    [SerializeField] GameObject arrowObject;
    bool itemUseFlag;
    bool endUseFlag;
    Vector3 itemUseDirection;
    [Header("特殊効果")]
    // 砂の効果
    [SerializeField] LayerMask sandLayer;
    bool onSandFlag;
    [SerializeField] float sandTime;
    float sandTimer;
    Vector3 originPosition;

    void Start()
    {
        rb.useGravity = false;
    }

    void Update()
    {
        switch (status) 
        {
            case 0: // start
            break;
            case 1: // play
                PlayerControl();
                PlayerLightControl();
                PlayerItemUseControl();
            break;
            case 2: // stop
                break;
            case 3: // over
                break;
            case 4: // clear
                break;
        }
        PlayerItemSelectControl(); 
        PlayerAttackControl(); 
        CameraControl();
    }

    // Player行動管理関数
    void PlayerControl()
    {
        // 地面が砂の場合
        Ray ray = new Ray(playerObject.transform.position, -playerObject.transform.up);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 0.3f, sandLayer)) onSandFlag = true;
        // 砂の演出
        if(onSandFlag)
        {
            playerObject.GetComponent<Collider>().enabled = false;
            if(sandTimer == 0) originPosition = playerObject.transform.position;
            if(sandTimer > sandTime)
            {
                playerObject.GetComponent<Collider>().enabled = true;
                sandTimer = 0;
                onSandFlag = false;
                playerObject.transform.position = new Vector3(playerObject.transform.parent.transform.position.x, playerObject.transform.parent.transform.position.y + 2f, playerObject.transform.parent.transform.position.z);
            }
            else if(sandTimer < sandTime)
            {
                sandTimer += Time.deltaTime;
                float y = Mathf.Lerp(originPosition.y, originPosition.y -2.5f, sandTimer / sandTime);
                playerObject.transform.position = new Vector3(playerObject.transform.position.x, y, playerObject.transform.position.z);                
            }
        }

        if (!itemSelectFlag && !itemUseFlag && !onSandFlag)
        {
            // 行動
            Vector3 playerPosition = playerObject.transform.position;
            if (onLight == 1) playerPosition += new Vector3(playerHorizontal * playerSpeed * Time.deltaTime, 0, playerVertical * playerSpeed * Time.deltaTime) * 0.1f;
            else playerPosition += new Vector3(playerHorizontal * playerSpeed * Time.deltaTime, 0, playerVertical * playerSpeed * Time.deltaTime);
            playerObject.transform.position = playerPosition;
            //進む方向に滑らかに向く。
            transform.forward = Vector3.Slerp(transform.forward, new Vector3(playerHorizontal * playerSpeed * Time.deltaTime, 0, playerVertical * playerSpeed * Time.deltaTime), Time.deltaTime * 10f);
        }
        rb.AddForce(gravity, ForceMode.Acceleration);
    }

    void CameraControl()
    {
        mainCamera.transform.position = new Vector3(playerObject.transform.position.x, playerObject.transform.position.y + 10f, playerObject.transform.position.z -2f);
    }

    void PlayerLightControl()
    {
        // 最拡大した際、インターバル終了時
        if (onLight == 2 && lightMaxIntervalTimer > lightMaxIntervalTime)
        {
            onLight = 0;
            lightRangeMinRange = playerLightRange;
            lightMaxIntervalTimer = 0;
            lightMaxIntervalTimerGauge.fillAmount = 1f;
            lightRangeObject.tag = "LightRange";
        }
        // 最拡大した際、インターバル中
        else if (onLight == 2 && lightMaxIntervalTimer < lightMaxIntervalTime)
        {
            lightMaxIntervalTimer += Time.deltaTime;
            float normal = Mathf.InverseLerp(5f, 0f, lightMaxIntervalTimer);
            lightMaxIntervalTimerGauge.fillAmount = normal;
        }
            // 最拡大
            if (onLight == 1 && playerLightRange == 180f)
        {
            playerLightRange = 180f;
            lightSpreadTimer = 0;
            onLight = 2;
            lightRangeObject.tag = "StanRange";
        }
        // 縮小中
        if (onLight == 0 && playerLightRange > 30f)
        {
            lightShrinkTimer += Time.deltaTime;
            playerLightRange = Mathf.Lerp(lightRangeMinRange, 30f, lightShrinkTimer / lightShrinkTime);
        }
        // 拡大中
        else if (onLight == 1 && playerLightRange < 180f)
        {
            lightSpreadTimer += Time.deltaTime;
            playerLightRange = Mathf.Lerp(30f, 180f, lightSpreadTimer / lightSpreadTime);
        }
        // 最縮小
        else if (onLight == 0 && playerLightRange == 30f)
        {
            playerLightRange = 30f;
            lightShrinkTimer = 0;
            lightSpreadTimer = 0;
            playerLight.spotAngle = 0;
        }
        playerLight.spotAngle = playerLightRange;
        
        float normalized = Mathf.InverseLerp(30f, 180f, playerLightRange);
        lightGauge.fillAmount = normalized;
        lightRangeObject.transform.localScale = new Vector3(Mathf.Lerp(2.75f, 17.25f, normalized), 0.1f, Mathf.Lerp(2.75f, 17.25f, normalized));
    }

    void PlayerAttackControl()
    {
        if (attackFlag)
        {
            if (attackTimer == 0) sword.SetActive(true);
            if(attackTimer > attackTime)
            {
                attackTimer = 0;
                sword.SetActive(false);
                attackFlag = false;
            }
            else if(attackTimer < attackTime)
            {
                attackTimer += Time.deltaTime;
                float y = Mathf.Lerp(playerObject.transform.eulerAngles.y - 45f, playerObject.transform.eulerAngles.y + 45f, attackTimer / attackTime);
                sword.transform.rotation = Quaternion.Euler(0f, y, 0f);
            }
        }
        else sword.SetActive(false);
    }

    void PlayerItemSelectControl()
    {
        // 選択
        if (itemSelectFlag)
        {
            if (!startSelectFlag)
            {
                for(int i = 0; i < itemSlots.Length; i++)
                {
                    if (itemTimer[i] > itemTime)
                    {
                        itemTimer[i] = 0;
                        itemSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(i * -175f - 350f, itemSlots[i].GetComponent<RectTransform>().anchoredPosition.y);
                        startSelectFlag = true;
                    }
                    else if (itemTimer[i] < itemTime)
                    {
                        itemTimer[i] += Time.deltaTime;
                        float x = Mathf.Lerp(-150f, i * -175f -350f, itemTimer[i] / itemTime);
                        itemSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(x, itemSlots[i].GetComponent<RectTransform>().anchoredPosition.y);
                    }
                }
            }            
            for (int i = 0; i < itemSlots.Length; i++)
            {
                if (i == itemSelectNum) itemSlots[i].GetComponent<RectTransform>().sizeDelta = new Vector2(165f, 165f);
                else itemSlots[i].GetComponent<RectTransform>().sizeDelta = new Vector2(150f, 150f);
            }
        }
        else
        {
            if (!endSelectFlag)
            {
                for (int i = 0; i < itemSlots.Length; i++)
                {
                    if (itemTimer[i] > itemTime)
                    {
                        itemTimer[i] = 0;
                        itemSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(-150f, itemSlots[i].GetComponent<RectTransform>().anchoredPosition.y);
                        endSelectFlag = true;
                        itemSlots[i].GetComponent<RectTransform>().sizeDelta = new Vector2(150f, 150f);
                    }
                    else if (itemTimer[i] < itemTime)
                    {
                        itemTimer[i] += Time.deltaTime;
                        float x = Mathf.Lerp(i * -175f - 350f, -150f, itemTimer[i] / itemTime);
                        itemSlots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(x, itemSlots[i].GetComponent<RectTransform>().anchoredPosition.y);
                    }
                }
            }
        }
    }
    void PlayerItemUseControl()
    {
        if(getItemFlag)
        {
            if(clearStageNum == 1) canItemFlag[0] = true;
            else if(clearStageNum == 6) canItemFlag[1] = true;
        }
        // 弓の使用可
        if (clearStageNum > 6)
        {
            for (int i = 0; i < 2; i++) canItemFlag[i] = true;
        }
        else if (clearStageNum > 1)
        {
            canItemFlag[0] = true;
        }
        // 投げ縄の使用可
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (canItemFlag[i])
            {
                itemSlots[i].transform.GetChild(1).GetComponent<Image>().sprite = itemImageSprites[i];
            }
            else if (!canItemFlag[i])
            {
                itemSlots[i].transform.GetChild(1).GetComponent<Image>().sprite = itemImageSprites[3];
            }
            if (itemSelectNum == i) selectObject.GetComponent<RectTransform>().anchoredPosition = itemSlots[i].GetComponent<RectTransform>().anchoredPosition;
        }
        // 使用
        // 弓
        if (canItemFlag[itemSelectNum] && itemSelectNum == 0)
        {
            itemSelect.GetComponent<Image>().sprite = itemImageSprites[0];
            if (itemUseFlag)
            {
                Quaternion rotation = Quaternion.LookRotation(itemUseDirection);
                //進む方向に滑らかに向く。
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 10f);
            }
            if (endUseFlag)
            {
                Instantiate(arrowObject, playerObject.transform.forward, Quaternion.identity);                
                itemUseFlag = false;
                endUseFlag = false;
            }
        }
        // 縄
        else if (canItemFlag[itemSelectNum] &&itemSelectNum == 1)
        {
            itemSelect.GetComponent<Image>().sprite = itemImageSprites[1];
            if(itemUseFlag)
            {

            }
            if(endUseFlag)
            {
                itemUseFlag = false;
                endUseFlag = false;
            }
        }
        else if (canItemFlag[itemSelectNum] && itemSelectNum == 2)
        {
            itemSelect.GetComponent<Image>().sprite = itemImageSprites[2];
            if (itemUseFlag)
            {

            }
            if (endUseFlag)
            {
                itemUseFlag = false;
                endUseFlag = false;
            }
        }
        else itemSelect.GetComponent<Image>().sprite = itemImageSprites[3];
    }
    private void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.tag == "Wall")
        {
            float x = playerObject.transform.position.x - collision.gameObject.transform.position.x;
            float y = playerObject.transform.position.y - collision.gameObject.transform.position.y;
            float z = playerObject.transform.position.z - collision.gameObject.transform.position.z;
            if (y < 0.5f)
            {
                if (Mathf.Abs(x) > Mathf.Abs(z)) playerHorizontal = 0f;
                else if (Mathf.Abs(x) < Mathf.Abs(z)) playerVertical = 0f;
            }
        }
    }

    public void InputPlayerControl(InputAction.CallbackContext context)
    {
        if(!itemSelectFlag)
        {
            if (context.ReadValue<Vector2>().x == 0) playerHorizontal = 0;
            if (context.ReadValue<Vector2>().x > 0) playerHorizontal = context.ReadValue<Vector2>().x;
            else if (context.ReadValue<Vector2>().x < 0) playerHorizontal = context.ReadValue<Vector2>().x;
            if (context.ReadValue<Vector2>().y == 0) playerVertical = 0;
            if (context.ReadValue<Vector2>().y > 0) playerVertical = context.ReadValue<Vector2>().y;
            else if (context.ReadValue<Vector2>().y < 0) playerVertical = context.ReadValue<Vector2>().y;
        }
    }

    public void InputPlayerLightButton(InputAction.CallbackContext context)
    {
        if (context.started && playerLightRange == 30) onLight = 1;
        else if (context.canceled && onLight == 1)
        {
            lightRangeMinRange = playerLightRange;
            onLight = 0;
        }
    }

    public void InputPlayerAttackButton(InputAction.CallbackContext context)
    {
        if(context.started && !attackFlag) attackFlag = true;
    }

    public void InputPlayerSelectItemButton(InputAction.CallbackContext context)
    {
        if (context.started && status == 1)
        {
            itemSelectFlag = true;
            endSelectFlag = false;
        }
        if (context.canceled) 
        {
            itemSelectFlag = false; 
            startSelectFlag = false;
        }
    }

    public void InputPlayerSelectItemControl(InputAction.CallbackContext context)
    {
        if (itemSelectFlag)
        {
            if (context.ReadValue<Vector2>().x == 0) playerHorizontal = 0;
            if (context.started && context.ReadValue<Vector2>().x > 0) itemSelectNum--;
            else if (context.started && context.ReadValue<Vector2>().x < 0) itemSelectNum++;
            if (itemSelectNum > 2) itemSelectNum = 0;
            if (itemSelectNum < 0) itemSelectNum = 2;
        }
    }
    public void InputPlayerUseItemButton(InputAction.CallbackContext context)
    {
        if(context.started && status == 1) itemUseFlag = true;
        if (context.canceled) endUseFlag = true;
    }
    public void InputPlayerUseItemControl(InputAction.CallbackContext context)
    {
        itemUseDirection = new Vector3(context.ReadValue<Vector2>().x, 0f, context.ReadValue<Vector2>().y);
    }
    public void InputPlayerMenuButton(InputAction.CallbackContext context)
    {
        if (context.started)
        {

        }
    }
}
