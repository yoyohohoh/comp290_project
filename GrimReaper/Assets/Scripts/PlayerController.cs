using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : Subject
{
    public static PlayerController Instance { get; private set; }

    private GrimReaper_LossofMemories _inputs;
    private Vector2 _move;
    public bool IsJumped { get; private set; }
    public bool IsAttacking { get; private set; }

    [Header("Character Controller")]
    [SerializeField] private CharacterController _controller;
    [SerializeField] private Vector3 initialPosition;

    [Header("Joystick")]
    [SerializeField] private bool isUsingJoystick = true;
    [SerializeField] private Joystick _joystick;

    [Header("Movement")]
    [SerializeField] private float _speed;
    [SerializeField] private float _walkSpeed = 2.0f;
    [SerializeField] private float _runSpeed = 5.0f;
    [SerializeField] private float _gravity = -30.0f;
    [SerializeField] private float _jumpHeight = 3.0f;
    private Vector3 _velocity;
    [SerializeField] private float bounceForce = 5.0f;

    [Header("Ground Detection")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private float _groundRadius = 0.5f;
    [SerializeField] private LayerMask _groundMask;
    private bool _isGrounded;

    [Header("Bounce Detection")]
    [SerializeField] private string bounceTag = "BounceObject";

    [Header("Shooting")]
    [SerializeField] private GameObject playerSight;
    [SerializeField] private float _projectileForce = 0f;
    [SerializeField] private Quest quest1, quest2, quest3;

    // References
    private CharacterController controller;
    private Animator animator;

    private void Awake()
    {
        Instance = this;
        _controller = GetComponent<CharacterController>();
        _inputs = new GrimReaper_LossofMemories();

        _inputs.Player.Move.performed += context => _move = context.ReadValue<Vector2>();
        _inputs.Player.Move.canceled += context => _move = Vector2.zero;
        //_inputs.Player.Jump.performed += context => Jump();
        //_inputs.Player.Fire.performed += context => Attack();
    }

    private void OnEnable() => _inputs.Enable();
    private void OnDisable() => _inputs.Disable();

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        quest1 = new Quest(1, "Tutorial", QuestState.Active);
        quest2 = new Quest(2, "CollectItem", QuestState.Null);
        quest3 = new Quest(3, "KillEnemy", QuestState.Null);

        NotifyObservers(QuestState.Pending, quest2);
        NotifyObservers(QuestState.Pending, quest3);

        InitializePlayerPosition();
        IsJumped = false;
        IsAttacking = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Attack();
        }
    }

    private void FixedUpdate()
    {
        CheckGroundStatus();
        HandleMovement();

        if (DataKeeper.Instance.isTutorialDone)
        {
            NotifyObservers(QuestState.Completed, quest1);
            NotifyObservers(QuestState.Active, quest2);
            NotifyObservers(QuestState.Active, quest3);
        }

        CountEnemies();
    }

    private void CountEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (quest3.state == QuestState.Active && enemies.Length == 1 &&
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 1)
        {
            NotifyObservers(QuestState.Completed, quest3);
        }
    }

    public void InitializePlayerPosition()
    {
        _controller.enabled = false;
        transform.position = DataKeeper.Instance.save1 != Vector3.zero ? DataKeeper.Instance.save1 : initialPosition;
        _controller.enabled = true;
    }

    private void CheckGroundStatus()
    {
        _isGrounded = Physics.CheckSphere(_groundCheck.position, _groundRadius, _groundMask);
        if (_isGrounded && _velocity.y < 0.0f)
        {
            _velocity.y = -2.0f;
        }
    }

    private void HandleMovement()
    {
        if (isUsingJoystick)
        {
            _move = _joystick.Direction;
        }

        if (_move == Vector2.zero)
        {
            Idle();
        }
        else
        {
            Move();
        }

        // Apply gravity
        _velocity.y += _gravity * Time.fixedDeltaTime;
        _controller.Move(new Vector3(0, _velocity.y, 0) * Time.fixedDeltaTime);

        // Keep player at fixed Z position
        Vector3 position = transform.position;
        position.z = initialPosition.z;
        transform.position = position;
    }

    private void Move()
    {
        if (IsRunning())
        {
            Run();
        }
        else
        {
            Walk();
        }

        // Flip the character horizontally based on movement direction
        if (_move.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = _move.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }


    private bool IsRunning()
    {
        // Logic to determine if the player should run (e.g., holding a sprint button)
        return _move.magnitude > 0.7f;
    }

    private void Idle()
    {
        animator.SetFloat("Speed", 0, 0.1f, Time.deltaTime);
    }

    private void Walk()
    {
        Vector3 movement = new Vector3(_move.x * _walkSpeed * Time.fixedDeltaTime, 0.0f, 0.0f);
        _controller.Move(movement);
        animator.SetFloat("Speed", 0.5f, 0.1f, Time.deltaTime);
    }

    private void Run()
    {
        Vector3 movement = new Vector3(_move.x * _runSpeed * Time.fixedDeltaTime, 0.0f, 0.0f);
        _controller.Move(movement);
        animator.SetFloat("Speed", 1, 0.1f, Time.deltaTime);
    }

    public void Jump()
    {
        if (_isGrounded)
        {
            animator.SetTrigger("Jump");

            IsJumped = true;
            SoundController.instance.Play("Jump");
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2.0f * _gravity);
        }
    }
    public void Attack()
    {
        animator.SetTrigger("Attack");

        // Play sound and set IsAttacking to true
        SoundController.instance.Play("Attack");
        IsAttacking = true;

        // Get and activate the projectile
        var projectile = ProjectilePoolManager.Instance.Get();
        if (projectile != null)
        {
            // Set the position and rotation
            projectile.transform.SetPositionAndRotation(playerSight.transform.position, Quaternion.identity);
            projectile.gameObject.SetActive(true);

            // Set the projectile size to twice its original
            projectile.transform.localScale = Vector3.one * 2;

            // Find the nearest enemy
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            Transform nearestEnemy = null;
            float closestDistance = Mathf.Infinity;

            int ctr = 1;
            foreach (GameObject enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestEnemy = enemy.transform;
                }
            }

            // Assign the nearest enemy as the target for the homing projectile
            HomingProjectile homingScript = projectile.GetComponent<HomingProjectile>();
            if (homingScript != null && nearestEnemy != null)
            {
                homingScript.SetTarget(nearestEnemy);
            }
            else
            {
                // If no target, shoot forward based on player's facing direction
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 shootDirection = transform.localScale.x > 0 ? transform.right : -transform.right;
                    rb.velocity = shootDirection * _projectileForce;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("On Tigger = "+ other.name);
        if (other.CompareTag("SuperShield"))
        {
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Enemy") && GameObject.FindGameObjectsWithTag("SuperShield").Length > 0)
        {
            SoundController.instance.Play("EnemyAttack");
            GamePlayUIController.Instance.UpdateHealth(-1.0f);
        }

        if (quest2.state == QuestState.Active && other.CompareTag("Item") && UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 1)
        {
            NotifyObservers(QuestState.Completed, quest2);
        }

        if (other.gameObject.CompareTag("BounceObject"))
        {
            float originalJumHeight = _jumpHeight;
            _jumpHeight = 30f;
            Jump();
            _jumpHeight = originalJumHeight;
        }
    }
}