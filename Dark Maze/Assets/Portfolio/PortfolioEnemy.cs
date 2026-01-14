using UnityEngine;

public class PortfolioEnemy : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] int a;
    void Start()
    {
        switch (a)
        {
            case 0:
                animator.SetBool("Attack", true);
                break;
                case 1:
                animator.SetBool("Down", true);

                break;

        }
        
    }
}
