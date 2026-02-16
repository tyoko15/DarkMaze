using UnityEngine;

// 敵のAnimator経由用スクリプト
public class EnemyAnimationAnchor : MonoBehaviour
{
    [SerializeField] Enemy1 enemy1;

    public void EnemyDestroy()
    {
        enemy1.EnemyDestroy();
    }
}
