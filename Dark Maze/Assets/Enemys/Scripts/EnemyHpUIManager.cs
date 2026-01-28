using UnityEngine;
using UnityEngine.UI;

public class EnemyHpUIManager : MonoBehaviour
{
    private Camera mainCamera;

    Image backGround;
    Image damageGauge;
    Image hpGauge;

    float maxHp;
    float enemyHp;

    public bool damageFlag;
    float damageGaugeAmount;
    [SerializeField] float damageTime;
    float damageTimer;
    private void Awake()
    {
        mainCamera = Camera.main;
        backGround = transform.GetChild(0).GetComponent<Image>();
        damageGauge = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        hpGauge = transform.GetChild(0).GetChild(1).GetComponent<Image>();
        backGround.gameObject.SetActive(false);
        damageGaugeAmount = damageGauge.fillAmount;
    }

    private void Update()
    {
        if (damageFlag) DamageControl();
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        // ƒJƒƒ‰‚Ì•ûŒü‚ðŒü‚­
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
    }

    public void HpActive(bool flag)
    {
        backGround.gameObject.SetActive(flag);
    }

    public void GetMaxHp(float max)
    {
        maxHp = max;
    }
    public void HpControl(float hp)
    {
        float range = Mathf.InverseLerp(0, maxHp, hp);
        hpGauge.fillAmount = range;
        enemyHp = hp;
    }

    void DamageControl()
    {
        if (damageTimer > damageTime)
        {
            damageFlag = false;
            damageGaugeAmount = damageGauge.fillAmount;
            damageTimer = 0;
        }
        else if (damageTimer < damageTime)
        {
            float hp = Mathf.InverseLerp(0, maxHp, enemyHp);
            float range = Mathf.Lerp(damageGaugeAmount, hp, damageTimer / damageTime);
            damageGauge.fillAmount = range;
            damageTimer += Time.deltaTime;
        }
    }
}
