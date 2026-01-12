using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    public static PlayerMovement Instance { get; private set; }

    private void Awake()
    {
        // 2. Initialize the instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // Note: Do NOT use DontDestroyOnLoad here if the Player is recreated per scene.
        }
    }

    public void Teleport(Vector3 newPosition)
    {
        // For a character, you usually set the transform directly.
        // If you are using a Rigidbody or CharacterController, you may need a different approach:

        // 1. If using a standard Transform:
        transform.position = newPosition;

        // 2. If using a CharacterController (more common for player movement):
        // (You might need to temporarily disable the controller before setting the position)
        // CharacterController cc = GetComponent<CharacterController>();
        // cc.enabled = false;
        // transform.position = newPosition;
        // cc.enabled = true;
    }
    public void SetPosition(Vector3 position)
    {
        // Use the Transform component to instantly move the player to the saved position.
        transform.position = position;

        // If the PlayerMovement script is a singleton, you might need a public static Instance:
        // public static PlayerMovement Instance { get; private set; }
        // (If so, make sure to set Instance = this in Awake())
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (PauseController.IsGamePaused)
        {
            if(rb.velocity != Vector2.zero)
            {
                rb.velocity = Vector2.zero;
                StopMovementAnimations();
            }
            return;
        }

        rb.velocity = moveInput * moveSpeed;
        animator.SetBool("isWalking", rb.velocity.magnitude > 0);
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            StopMovementAnimations();
        }

        moveInput = context.ReadValue<Vector2>();

        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
    }

    void StopMovementAnimations()
    {
        animator.SetBool("isWalking", false);
        animator.SetFloat("LastInputX", moveInput.x);
        animator.SetFloat("LastInputY", moveInput.y);
    }
}
