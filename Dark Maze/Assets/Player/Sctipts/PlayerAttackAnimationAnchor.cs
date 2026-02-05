using UnityEngine;

public class PlayerAttackAnimationAnchor : MonoBehaviour
{
    PlayerController playerController;
    [SerializeField] GameObject sword; 
    Animator animator;
    [SerializeField] float startAttackSpeed;
    [SerializeField] float midAttackSpeed;
    [SerializeField] float endAttackSpeed;
    void Start()
    {
        playerController = transform.parent.GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
    }


    void Update()
    {
        
    }

    public void StartAttackSpeed()
    {     
        animator.SetFloat("AttackSpeed", startAttackSpeed);
    }

    public void MidAttackSpeed()
    {
        sword.SetActive(true);
        animator.SetFloat("AttackSpeed", midAttackSpeed);
    }

    public void EndAttackSpeed()
    {
        sword.SetActive(false);
        animator.SetFloat("AttackSpeed", endAttackSpeed);
    }

    public void EndAttack()
    {
        playerController.SetAttackFlag(false);

    }


}
