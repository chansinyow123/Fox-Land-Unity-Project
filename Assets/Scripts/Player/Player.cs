using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    //variable
    [Header("Ability")]
    [SerializeField] bool doubleJumpAbility = true;
    [SerializeField] bool dashAbility = true;
    [SerializeField] bool wallSlideAbility = true;

    [Header("Move")]
    [SerializeField] float moveSpeed = 5f;
    float moveHorizontal = 0f;
    float moveVertical = 0f;

    [Header("Jump")]
    [SerializeField] float jumpHeight = 1f;
    [SerializeField] ForceMode2D forceMode = ForceMode2D.Force;
    bool isJumping = false;
    bool canDoubleJump = true;

    [Header("Wall Slide")]
    [SerializeField] float wallDrag = 6f;
    [SerializeField] Vector2 wallJumpForce = new Vector2(1.2f, 5f);
    [SerializeField] float movementAfterJump = 0.1f;

    [Header("Dash")]
    [SerializeField] float dashSpeed = 5f;
    [SerializeField] float dashTime = 1f;
    [SerializeField] float dashCooldownTime = 0.5f;
    bool isDashing = false;
    bool isDashCooldown = false;
    bool canDash = true;

    [Header("Jump Particle")]
    [SerializeField] GameObject jumpParticlePrefab = default;
    [SerializeField] GameObject jumpSpawnParticle = default;
    GameObject jumpParticle;
    ParticleSystem jumpParticleComponent;

    [Header("Slide Particle")]
    [SerializeField] GameObject slideParticle = default;
    [SerializeField] GameObject slideJumpEffect = default;
    [SerializeField] GameObject slideJumpPos = default;
    ParticleSystem slideParticleSystem;
    GameObject instJumpEffect;

    [Header("Sword")]
    [SerializeField] GameObject swordPrefab = default;
    [SerializeField] Transform swordTopPos = default;
    [SerializeField] Transform swordFrontPos = default;
    [SerializeField] Transform swordBtmPos = default;
    [SerializeField] float atkCooldown = 1f;
    [SerializeField] float bounce = 2f;
    bool canAttack = true;
    GameObject instSword;

    [Header("Health")]
    [Range(1, 10)]
    [SerializeField] int maxHealth = 10;
    [SerializeField] GameObject playerBody = default;
    [SerializeField] Vector2 hurtForce = new Vector2(1.5f, 1.5f);
    [SerializeField] float allowActionTime = 0.3f;
    [SerializeField] float invulnerableTime = 1.5f;
    [SerializeField] Image[] heartImage = default;
    [SerializeField] Sprite fullHeart = default;
    [SerializeField] Sprite emptyHeart = default;
    int health;
    bool invulnerable = false;
    SpriteRenderer playerSprite;
    bool isTakingDmg = false;

    [Header("Crouch")]
    [SerializeField] GameObject focusPrefab = default;
    [SerializeField] GameObject doneFocusPrefab = default;
    ParticleSystem focusParticle;
    GameObject doneFocusParticle;

    [Header("Power Bar")]
    [SerializeField] Image powerBar = default;
    [SerializeField] float maxPower = 80f;
    [SerializeField] float healingPower = 20f;
    [SerializeField] float powerPerSwing = 5f;
    [SerializeField] Color lowPowerColor = default;
    [SerializeField] Color highPowerColor = default;
    float currentPower;
    IEnumerator focusCoroutine;

    //cached
    Rigidbody2D rb;
    Animator animator;

    bool isWallSliding = false;
    bool isWallJumping = false;
    bool wallJump = false;

    bool isCrouching = false;
    bool isFocusing = false;

    bool jumpBtnDown = false;
    bool jumpBtnUp = false;
    bool dashBtnDown = false;
    bool atkBtnDown = false;
    bool focusBtnDown = false;

    bool touchGround = false;
    bool touchWall = false;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        jumpParticleComponent = jumpParticlePrefab.GetComponent<ParticleSystem>();
        slideParticleSystem = slideParticle.GetComponent<ParticleSystem>();
        playerSprite = playerBody.GetComponent<SpriteRenderer>();

        currentPower = powerBar.fillAmount * maxPower;
        focusParticle = focusPrefab.GetComponent<ParticleSystem>();

        health = maxHealth;
        UpdateHealthUI();
    }

    void FixedUpdate()
    {
        Dash();
        Crouch();
        WallSlide();
        DoubleJump();
        Move();
        Jump();
        Attack();

        jumpBtnUp = false;
        jumpBtnDown = false;
        dashBtnDown = false;
        atkBtnDown = false;
    }

    void Update()
    {
        moveHorizontal = Input.GetAxisRaw("Horizontal") * moveSpeed;   // Get the input with positive and negative
        moveVertical = Input.GetAxisRaw("Vertical");
        focusBtnDown = Input.GetButton("Focus");
        InputDash();
        InputJump();
        InputAtk();

        Debug.Log(invulnerable);

        ChangePowerColor();
        CrouchAnimation();
        JumpAnimation();
        DashAnimation();
        MoveAnimation();
        WallSlideAnimation();
    }

    private void Dash()
    {
        if (!dashAbility || isTakingDmg || isCrouching)
        {
            isDashing = false;
            return;
        }

        if (dashBtnDown && !isDashCooldown && canDash)
        {
            if (isWallSliding)   // if wall sliding
            {
                transform.localScale *= new Vector2(-1, 1);
            }
            else if ((moveHorizontal > 0 && transform.localScale.x == -1) || (moveHorizontal < 0 && transform.localScale.x == 1))    //if wall jumping
            {
                transform.localScale *= new Vector2(-1, 1);
            }

            canDash = false;
            isDashing = true;
            isDashCooldown = true;

            StartCoroutine(StopDashing());
        }

        if (touchGround || isWallSliding)
        {
            canDash = true;
        }

        if (isDashing)
        {
            rb.velocity = new Vector2(dashSpeed, 0f) * transform.localScale;
        }
    }

    IEnumerator StopDashing()
    {
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
        StartCoroutine(DashCooldown());
    }

    IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldownTime);
        isDashCooldown = false;
    }

    private void Crouch()
    {
        if (focusBtnDown && touchGround && !isDashing && !isTakingDmg)
        {
            isCrouching = true;

            // only run one time
            if (!isFocusing && currentPower >= healingPower) {
                focusCoroutine = Focusing();
                StartCoroutine(focusCoroutine);
                isFocusing = true;
            }
        }
        else
        {
            isCrouching = false;

            if (focusParticle.isPlaying)
            {
                focusParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (isFocusing)
            {
                StopCoroutine(focusCoroutine);
                isFocusing = false;
            }
        }
    }

    IEnumerator Focusing()
    {
        float tempPower = 0f;

        // play focusing particle
        if (!focusParticle.isPlaying)
        {
            focusParticle.Play(true);
        }

        while (tempPower < healingPower)
        {
            yield return new WaitForSeconds(0.05f);
            currentPower -= 1f;
            powerBar.fillAmount -= 1f / maxPower;
            tempPower += 1f;

            if (tempPower >= healingPower)
            {
                doneFocusParticle = Instantiate(doneFocusPrefab, transform.position, Quaternion.identity) as GameObject;
                Destroy(doneFocusParticle, 5f);
                tempPower = 0f;

                if (health < maxHealth)
                {
                    health++;
                    UpdateHealthUI();
                }

                if (currentPower < healingPower)
                {
                    break;
                }
            }
        }

        if (focusParticle.isPlaying)
        {
            focusParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void DoubleJump()
    {
        // if no double jump ability
        if (!doubleJumpAbility || isTakingDmg)
        {
            return;
        }

        if (!touchGround && !isWallSliding && jumpBtnDown && canDoubleJump && !isDashing)
        {
            rb.velocity = new Vector2(0f, 0f);                                      // refresh the velocity
            rb.AddForce(Vector2.up * jumpHeight, forceMode);                        // and jump

            // Spawn Particle effect
            jumpParticle = Instantiate(jumpParticlePrefab, jumpSpawnParticle.transform.position, jumpParticlePrefab.transform.rotation) as GameObject;
            Destroy(jumpParticle, jumpParticleComponent.main.duration);

            isJumping = true;
            canDoubleJump = false;
        }

        if (touchGround || isWallSliding)
        {
            canDoubleJump = true;
        }
    }

    private void WallSlide()
    {
        if (!wallSlideAbility || isTakingDmg || isDashing)
        {
            isWallSliding = false;
            isWallJumping = false;
            slideParticleSystem.Stop();
            return;
        }

        isWallSliding = (touchWall && !touchGround && rb.velocity.y <= 0.5f);

        if (isWallSliding)
        {
            rb.velocity = new Vector2(0, -wallDrag);
            slideParticleSystem.Play();

            if (jumpBtnDown)
            {
                wallJump = true;
                isWallJumping = true;

                instJumpEffect = Instantiate(slideJumpEffect, slideJumpPos.transform.position, Quaternion.identity) as GameObject;
                instJumpEffect.transform.localScale *= new Vector2(transform.localScale.x, 1);
                Destroy(instJumpEffect, 0.5f);

                rb.velocity = new Vector2(0f, 0f);                                                          // Reset Velocity first
                transform.localScale *= new Vector2(-1, 1);                                                 // Flip Player
                rb.AddForce(wallJumpForce * new Vector2(transform.localScale.x, 1), forceMode);             // Wall Jump

                StartCoroutine(AllowMovement());                                                            // Allow Movement After Jumping
            }
        }
        else
        {
            slideParticleSystem.Stop();
        }

        if (isWallSliding && moveVertical < 0 && moveHorizontal == 0)
        {
            transform.localScale *= new Vector2(-1, 1);
        }

        if (isWallJumping && jumpBtnUp)
        {
            isWallJumping = false;

            if (rb.velocity.y >= 0.01)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f);
            }

        }
    }

    IEnumerator AllowMovement()
    {
        yield return new WaitForSeconds(movementAfterJump);
        rb.velocity = new Vector2(0f, rb.velocity.y);
        wallJump = false;
    }

    private void Attack()
    {
        if (isDashing || isTakingDmg)
        {
            return;
        }

        if (atkBtnDown && canAttack)
        {
            if (moveVertical > 0)
            {
                instSword = Instantiate(swordPrefab, swordTopPos.position, Quaternion.Euler(0, 0, 90)) as GameObject;
                instSword.transform.parent = gameObject.transform;
                instSword.transform.localScale = new Vector2(1, 1);
                instSword.transform.localEulerAngles = new Vector3(0, 0, 90f);
            }
            else if (isWallSliding)
            {
                instSword = Instantiate(swordPrefab, swordFrontPos.position, Quaternion.identity) as GameObject;
                instSword.transform.parent = gameObject.transform;
                instSword.transform.localPosition *= new Vector2(-1, 1);
                instSword.transform.localScale = new Vector2(-1, 1);
            }
            else if (moveVertical < 0 && !touchGround)
            {
                instSword = Instantiate(swordPrefab, swordBtmPos.position, Quaternion.Euler(0, 0, 270)) as GameObject;
                instSword.transform.parent = gameObject.transform;
                instSword.transform.localScale = new Vector2(1, 1);
                instSword.transform.localEulerAngles = new Vector3(0, 0, 270f);
            }
            else
            {
                instSword = Instantiate(swordPrefab, swordFrontPos.position, Quaternion.identity) as GameObject;
                instSword.transform.parent = gameObject.transform;
                instSword.transform.localScale = new Vector2(1, 1);
            }

            //Destroy(instSword, 0.04f);

            canAttack = false;
            StartCoroutine(AllowAttack());
        }
    }

    IEnumerator AllowAttack()
    {
        yield return new WaitForSeconds(atkCooldown);
        canAttack = true;
    }

    private void Jump()
    {
        if (isDashing || isTakingDmg)
        {
            isJumping = false;
            return;
        }

        if (touchGround && jumpBtnDown && !isDashing)
        {
            rb.velocity = new Vector2(0f, 0f);                                      // refresh the velocity
            rb.AddForce(Vector2.up * jumpHeight, forceMode);                        // and jump
            isJumping = true;
        }

        if (isJumping && jumpBtnUp)
        {
            isJumping = false;
            if (rb.velocity.y >= 0.01)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f);
            }
        }
    }

    private void Move()
    {
        if (isDashing || isTakingDmg || wallJump)
        {
            return;
        }

        if (moveHorizontal != 0 && !isCrouching)
        {
            FlipGameObject();                                                               // Flip the Game Object based on Y axis with the given direction

            Vector2 newPos = new Vector2(moveHorizontal, rb.velocity.y);                           // Make the gameObject Move based on X-axis
            rb.velocity = newPos;
        }
        else
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }


    private void FlipGameObject()
    {
        if (moveHorizontal > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
        }
        else
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);                       // Set the gameObject Scale to left
        }
    }

    private void ChangePowerColor()
    {
        if (currentPower < healingPower)
        {
            powerBar.color = lowPowerColor;
        }
        else
        {
            powerBar.color = highPowerColor;
        }
    }

    private void CrouchAnimation()
    {
        if (isCrouching)
        {
            animator.SetBool("IsCrouching", true);
        }
        else
        {
            animator.SetBool("IsCrouching", false);
        }
    }

    private void WallSlideAnimation()
    {
        if (isWallSliding)
        {
            animator.SetBool("IsWallSliding", true);
        }
        else
        {
            animator.SetBool("IsWallSliding", false);
        }
    }

    private void DashAnimation()
    {
        if (isDashing)
        {
            animator.SetBool("IsDashing", true);
        }
        else
        {
            animator.SetBool("IsDashing", false);
        }
    }

    private void JumpAnimation()
    {
        
        if ((rb.velocity.y <= -0.01 || rb.velocity.y >= 0.01) && !isWallSliding && !isDashing && !isTakingDmg) {                          // y velocity must not be near 0

            if (rb.velocity.y <= -0.01)                                                 // if falling, set falling animation
            {
                animator.SetBool("IsJump", false);
                animator.SetBool("IsFall", true);
            }
            else
            {                                                                           // if jumping, set jumping animation
                animator.SetBool("IsJump", true);
                animator.SetBool("IsFall", false);
            }
        }
        else
        {                                                                               // if y velocity is constant, means that he is not jumping and falling
            animator.SetBool("IsJump", false);
            animator.SetBool("IsFall", false);
        }
    }

    private void MoveAnimation()
    {

        if (Mathf.Abs(moveHorizontal) != 0 && touchGround && !isDashing && !isTakingDmg && !wallJump && !isCrouching)
        {
            animator.SetBool("IsRun", true);
        }
        else
        {
            animator.SetBool("IsRun", false);
        }
    }

    private void InputDash()
    {
        if (Input.GetButtonDown("Dash"))
        {
            dashBtnDown = true;
        }
    }

    private void InputJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            jumpBtnDown = true;
        }

        if (Input.GetButtonUp("Jump"))
        {
            jumpBtnUp = true;
        }
        
    }

    private void InputAtk()
    {
        if (Input.GetButtonDown("Attack"))
        {
            atkBtnDown = true;
        }
    }

    public void setTouchGroundToTrue()
    {
        touchGround = true;
    }

    public void setTouchGroundToFalse()
    {
        touchGround = false;
    }

    public void setTouchWallToTrue()
    {
        touchWall = true;
    }

    public void setTouchWallToFalse()
    {
        touchWall = false;
    }

    public void PlayerBounce()
    {
        isJumping = false;
        canDoubleJump = true;
        canDash = true;
        rb.velocity = new Vector2(0f, 0f);
        rb.AddForce(new Vector2(0f, bounce), forceMode);
    }

    public void TakeDamage(int damage)
    {

        if (invulnerable)
        {
            return;
        }

        health -= damage;

        if (health <= 0)
        {
            health = 0; // die animation later
        }

        UpdateHealthUI(); // update heath UI

        invulnerable = true;
        isTakingDmg = true;

        rb.velocity = new Vector2(0f, 0f);
        rb.AddForce(hurtForce * new Vector2(-transform.localScale.x, 1), forceMode);
        animator.SetBool("IsHurt", true);

        playerSprite.color = new Color(1f, 1f, 1f, 0.6f);
        StartCoroutine(AllowAction());
        StartCoroutine(Notinvulnerable());

    }

    IEnumerator AllowAction()
    {
        yield return new WaitForSeconds(allowActionTime);
        rb.velocity = new Vector2(0f, 0f);
        isTakingDmg = false;
        animator.SetBool("IsHurt", false);
    }

    IEnumerator Notinvulnerable()
    {
        yield return new WaitForSeconds(invulnerableTime);
        invulnerable = false;
        playerSprite.color = new Color(1f, 1f, 1f, 1f);
    }

    private void UpdateHealthUI()
    {
        int tempHealth = health;
        int tempMaxHealth = maxHealth;

        foreach (Image heart in heartImage)
        {
            if (tempMaxHealth > 0)
            {
                if (tempHealth <= 0)
                {
                    heart.sprite = emptyHeart;
                }
                else
                {
                    heart.sprite = fullHeart;
                    tempHealth--;
                }

                tempMaxHealth--;
            }
            else
            {
                heart.enabled = false;
            }
        }
    }

    public void IncreasePowerUI()
    {
        if (currentPower < maxPower) {
            currentPower += powerPerSwing;
            powerBar.fillAmount += powerPerSwing / maxPower;
        }
    }
}
