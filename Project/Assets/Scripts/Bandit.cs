using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class Bandit : MonoBehaviourPunCallbacks, IPunObservable
{ 
    [SerializeField] float      m_speed = 4f  ;
    [SerializeField] float      m_jumpForce = 7.5f;


    public float maxHealth = 1000;
    float currentHealth;

    private Transform myTransform;
    private GameManager manager;

    public ProfileData playerProfile;
    public TextMeshPro AHName;
  
    public float blockRate = 0.2f;

    private Transform ui_healthbar;
    private Text ui_username;
    public Transform oh_health;
    private bool attacking;
    float direction = 0;

    private Animator            m_animator;
    private Rigidbody2D         m_body2d;
    private Sensor_Bandit       m_groundSensor;
    private bool                m_grounded = false;
    private bool                m_combatIdle = false;
    private AudioSource sfx;
     // Use this for initialization
    void Start () {

        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        currentHealth = maxHealth;


        if (photonView.IsMine)
        {
 
            sfx = GameObject.Find("Main Camera").GetComponent<AudioSource>();
            sfx.Play();
            ui_healthbar = GameObject.Find("HUD/Health/Bar").transform;
            m_animator = GetComponent<Animator>();
            m_body2d = GetComponent<Rigidbody2D>();
            m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_Bandit>();
            RefreshHealthBar();
         }
 }
	
    // Update is called once per frame
    void Update() {

       if (!photonView.IsMine)
       {
          return;
       };

        if (attacking)
        {
            m_speed = 0f;
            StartCoroutine(AttackRoutine());
            StopCoroutine(AttackRoutine());
        }
        else
        {
            m_speed = 4f;
            m_jumpForce = 7.5f;
        }
        
      


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameObject.Find("Pause").GetComponent<Pause>().TogglePause();
        }

        if (Pause.pause)
        {
            m_speed = 0f;
            m_jumpForce = 0f;
        }



        if (!m_grounded && m_groundSensor.State()) {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        if (m_grounded && !m_groundSensor.State()) {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        float inputX = Input.GetAxis("Horizontal");


        if (inputX > 0 && !attacking)
        {
            AHName.transform.eulerAngles = Vector3.up * 180;

            transform.localScale = new Vector2(-1.0f, 1.0f);

            m_combatIdle = false;
        }

        else if (inputX < 0 && !attacking)
        {
 
            AHName.transform.eulerAngles = Vector3.up * 360;

            transform.localScale = new Vector2(1.0f, 1.0f);
 
            m_combatIdle = false;

        }

        else if (Input.GetMouseButtonDown(1))
        {
             
                m_combatIdle = true;
                m_speed = 0f;
   
        }else if(Input.GetMouseButtonUp(1)){
            m_combatIdle = false;
        }

        m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);

        m_animator.SetFloat("AirSpeed", m_body2d.velocity.y);

        

        if (Input.GetKeyDown("space") && m_grounded)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
        }
      
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
            m_animator.SetInteger("AnimState", 2);

        else if (m_combatIdle)
            m_animator.SetInteger("AnimState", 1);

        else
            m_animator.SetInteger("AnimState", 0);

        RefreshHealthBar();

        photonView.RPC("SyncProfile", RpcTarget.All, MainMenu.profile.username, MainMenu.profile.level, MainMenu.profile.exp);

    }

    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.IsWriting)
        {
            stream.SendNext(transform.localScale);
        }
        else
        {
            transform.localScale = (Vector3)stream.ReceiveNext();
        }
     }

    [PunRPC]
    private void SyncProfile(string username, int lvl, int exp)
    {
        playerProfile = new ProfileData(username, lvl, exp);
        AHName.text = playerProfile.username;

    }

    [PunRPC]
    public void TakeDamage(int damage, int n_actor)
    {
        if (photonView.IsMine)
        {
            if (n_actor != PhotonNetwork.LocalPlayer.ActorNumber)
            {

                if (m_combatIdle)
                    damage = 0;

                currentHealth -= damage;
                RefreshHealthBar();

                if (currentHealth <= 0)
                {
                    m_animator.SetTrigger("Death");
                    manager.Spawn();
                    manager.ChangeStatSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);
                    manager.ChangeStatSend(n_actor, 0, 1);
                    PhotonNetwork.Destroy(gameObject);
                  }
                else
                {
                    if (!m_combatIdle)
                        m_animator.SetTrigger("Hurt");
                }
            }
        }
    }

    private void RefreshHealthBar()
    {
        float temp_healt_ratio = currentHealth / maxHealth;
        ui_healthbar.localScale = Vector3.Lerp(ui_healthbar.localScale, new Vector3(temp_healt_ratio, 1, 1), Time.deltaTime * 8f);
 
    }



    public bool getIdle()
    {
        return m_combatIdle;
    }

    public void SetState(bool s )
    {
        attacking = s;
    }


    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(0.7f);
        attacking = false;

    }
}
