using UnityEngine;

public class StadioPlayer : MonoBehaviour
{
    [SerializeField] Animator animator;

    void Start()
    {
        animator.SetTrigger("Attack");
        animator.SetFloat("AttackSpeed", 0.5f);
    }
}
