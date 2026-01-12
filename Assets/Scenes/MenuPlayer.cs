using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPlayer : MonoBehaviour
{
    Rigidbody2D rb;
    Animator animator;
    SpriteRenderer spriteRenderer;
    float moveSpeed = 10f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {

        // Get the mouse position in the world space
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Set the y-position to the player's current y-position to restrict movement to left and right
        mousePosition.y = transform.position.y;

        Vector2 moveDirection = (mousePosition - transform.position).normalized;

        // Set the velocity based on the direction and move speed
        rb.velocity = new Vector2(moveDirection.x * moveSpeed, 0f);
        spriteRenderer.flipX = moveDirection.x < 0;
    }
}
