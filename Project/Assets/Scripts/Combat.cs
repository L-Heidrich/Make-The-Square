using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class Combat : MonoBehaviourPunCallbacks
{

    public float attackRate = 2f;
    public float thrust;
    float nextAttackTime = 0f;
    public int attackDamage = 100;
    public float attackRange = 0.2f;
    public Transform attackPoint;
    public LayerMask enemyLayers;
    private Bandit m_bandit;
     private Animator m_animator;
    private Rigidbody2D m_body2d;

    public AudioClip attacksound;
    public AudioSource sfx;

    // Start is called before the first frame update
    void Start()
    {

        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_bandit = GetComponent<Bandit>();
 
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            
            if (Input.GetMouseButtonDown(0))
                {
                  if (Time.time >= nextAttackTime)
                    {
                        if(m_bandit.getIdle() == false)
                        {
                        m_bandit.SetState(true);

                        Attack();
                         nextAttackTime = Time.time + 1f / attackRate;
                           
                    }
                    

                    }
                }


        }
    }

    [PunRPC]
    void Attack()
    {
        m_animator.SetTrigger("Attack");
        sfx.PlayOneShot(attacksound);
         Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
 
        foreach (Collider2D other in hitEnemies)
        {

            if (photonView.IsMine)
            {
              
                Rigidbody2D enemy = other.GetComponent<Rigidbody2D>();

                if(enemy != null)
                {
                    enemy.isKinematic = false;
                    Vector2 difference = enemy.transform.position - transform.position;
                    difference = difference.normalized * thrust;
                    enemy.AddForce(difference, ForceMode2D.Impulse);

                    Debug.Log("enemy hit");

                    other.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, attackDamage, PhotonNetwork.LocalPlayer.ActorNumber);
                    enemy.isKinematic = true;
                }
               
              }

        }
        

    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

   
}

