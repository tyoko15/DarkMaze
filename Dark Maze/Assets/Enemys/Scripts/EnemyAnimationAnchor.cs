using UnityEngine;

public class EnemyAnimationAnchor : MonoBehaviour
{
    [SerializeField] Enemy1 enemy1;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void EnemyDestroy()
    {
        enemy1.EnemyDestroy();
    }
}
