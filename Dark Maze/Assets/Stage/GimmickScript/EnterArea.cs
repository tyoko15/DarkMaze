using UnityEngine;

public class EnterArea : MonoBehaviour
{
    [SerializeField] GameObject area;
    public bool enterAreaFlag;
    [SerializeField] bool enemyAreaFlag;
    int count;
    private void OnTriggerEnter(Collider other)
    {
        // 敵がいるエリアに侵入した時
        if (enemyAreaFlag && count == 0 && !enterAreaFlag && other.gameObject.tag == "Player")
        {
            enterAreaFlag = true;
            count = 1;
        }
        // エリア侵入時、エリアのペアレント化
        if(other.gameObject.tag == "Player") other.gameObject.transform.parent = area.transform;
    }
}
