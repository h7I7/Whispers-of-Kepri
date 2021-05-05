using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {

    //\===========================================================================================
    //\ Variables
    //\===========================================================================================

    #region Variables

    [Header("Movement varaibles")]
    [SerializeField]
    private float m_jumpHeight;        // How high the player can jump.
    private float m_adjustedJumpHeight;
    [SerializeField]
    private float m_walkSpeed;         // How fast the player walks.
    [SerializeField]
    private float m_runSpeedMultiplier;// How fast the player runs.
    [SerializeField]
    private float m_rotateSpeed;       // How fast the camera can turn.
    [SerializeField]
    private float m_gravity;           // Gravity of the player
    [SerializeField]
    [Range(0.5f, 0.99f)]
    private float m_movementSlowDown;  // The amount the player will slow down every frame when not pressing movement keys


    [Space(5)]
    [Header("Gameplay variables")]
    [SerializeField]
    private float m_health = 100f;
    [SerializeField]
    private float m_plague = 0f;

    [SerializeField]
    private Vector2 m_plagueToAdd;

	[SerializeField]
	private Image m_healthBar;

    private bool gameEnded;

	private CharacterController m_cc = null;  // The objects character controller
    private Vector3 m_moveDir;                // Vector3 used for controller the character controller

    public Controller controller;

    public GameObject gameManager;

    [Space(2.5f)]
    [Header("Path Tracing Prefab")]
    [SerializeField]
    private GameObject m_pathTracer;

    #endregion

    //\===========================================================================================
    //\ Unity Methods
    //\===========================================================================================

    #region Unity Methods
    private void Awake()
    {
        m_cc = GetComponent<CharacterController>();
        m_moveDir = Vector3.zero; // Set moveDir to 0, 0, 0
        m_adjustedJumpHeight = m_jumpHeight * 0.01f;
    }

    private void OnValidate()
    {
        m_adjustedJumpHeight = m_jumpHeight * 0.01f;
    }

    void Update()//Updates the players position, rotation, animation depending on inputs.
	{
        if (m_health < 0f && !gameEnded)
        {
            gameEnded = true;
            m_health = 0f;
            PauseManager.instance.EndGame();
			
		}

		m_healthBar.fillAmount = m_health / 100;


		controller = gameManager.GetComponent<CameraDisplaySwap>().playerController;

        float rot = Input.GetAxisRaw(controller.X2Axis) * Time.deltaTime * m_rotateSpeed;//Sets x to be equal to how much to rotate the player left or right depending on horizontal axis input.
        transform.Rotate(0, rot, 0);//Rotate the player on the y axis so it turns in accordance to horizontal input.


        // Movement when grounded
        if (m_cc.isGrounded)
        {
            float moveSpeed = -Input.GetAxisRaw(controller.YAxis) * Time.deltaTime;

            // Slowing down the player
            m_moveDir.x *= m_movementSlowDown;
            m_moveDir.z *= m_movementSlowDown;

            if (-Input.GetAxisRaw(controller.YAxis) != 0 && Input.GetButton(controller.LAnalog))//If the player is running.
            {
                m_moveDir = transform.forward * moveSpeed * (m_walkSpeed * m_runSpeedMultiplier);
            }
            else if (-Input.GetAxisRaw(controller.YAxis) != 0)//If the player is walking.
            {
                m_moveDir = transform.forward * moveSpeed * m_walkSpeed;
            }

            if (Input.GetButton(controller.AButton))
            {
                m_moveDir.y = m_adjustedJumpHeight;
            }
        }

        m_moveDir.y -= m_gravity * Time.deltaTime;
        m_cc.Move(m_moveDir);   //Move the player forwards or backwards at a lower speed depending on vertical input.
    }

    private void FixedUpdate()
    {
        m_health -= m_plague;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Enemy")
        {
            TKGenerator.instance.PlaceEnemy();

            m_plague += UnityEngine.Random.Range(m_plagueToAdd.x, m_plagueToAdd.y);

            SpawnTracer();
        }
    }

    public void SpawnTracer()
    {
        GameObject newTracer = Instantiate(m_pathTracer, transform.position + m_pathTracer.transform.position, m_pathTracer.transform.rotation, TKGenerator.instance.m_mapTransform);
        newTracer.GetComponent<Unit>().target = TKGenerator.instance.m_exit.gameObject.transform;
    }

    public void ResetLife()
    {
        m_health = 100f;
        m_plague = 0f;
    }

	#endregion
}
