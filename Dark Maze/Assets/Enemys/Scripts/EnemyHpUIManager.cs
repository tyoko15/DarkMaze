using UnityEngine;
using UnityEngine.UI;

public class EnemyHpUIManager : MonoBehaviour
{
    private Camera mainCamera;

    Image hpGaugeImage;

    private void Awake()
    {
        mainCamera = Camera.main;
        hpGaugeImage = transform.GetChild(0).GetComponent<Image>();
        hpGaugeImage.gameObject.SetActive(false);
    }

    private void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        // ƒJƒƒ‰‚Ì•ûŒü‚ğŒü‚­
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
    }

    public void HpActive(bool flag)
    {
        hpGaugeImage.gameObject.SetActive(flag);
    }
}
