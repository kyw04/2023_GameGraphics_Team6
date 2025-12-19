using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float radius;
    public float movementSpeed = 5f;
    public float jumpPower = 5f;
    public float coinCount = 0;
    public AudioClip[] moveSounds;
    public AudioClip jumpSound;
    public AudioClip changeSound;
    public CameraMove cameraMove;

    private AudioSource audioSource;
    private GameObject controlledObj;
    private Rigidbody2D controlledObjRb;
    private SpriteRenderer spriteRenderer;
    private Animator ani;
    private Rigidbody2D rb;
    private CapsuleCollider2D characterBox;
    private BoxCollider2D shadowBox;
    private Vector2 movement;
    private int moveSoundIndex;
    private float moveSoundDlay = 0.3f;
    private float moveSoundLastTime;
    private bool isShadow = false;
    private bool isGround = false;
    private bool isMove = true;
    [HideInInspector]
    public bool onChange = false;

    private void Start()
    {
        isMove = true;
        controlledObj = null;
        controlledObjRb = null;
        moveSoundIndex = 0;
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        ani = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        characterBox = GetComponent<CapsuleCollider2D>();
        shadowBox = GetComponent<BoxCollider2D>();
        onChange = false;
    }

    private void Update()
    {
        if (!isMove)
        {
            ani.SetBool("isRun", false);

            return;
        }

        Move();

        if (onChange && !controlledObj && Input.GetKeyDown(KeyCode.LeftShift))
        {
            Change();
        }

        Vector3 pos = transform.position;
        pos.y += 1f;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, radius, LayerMask.GetMask("Enemy") | LayerMask.GetMask("DynamicObject") | LayerMask.GetMask("Coin") | LayerMask.GetMask("Elevator"));
        Collider2D selectCollider = null;
        float min = -1;

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Coin"))
            {
                collider.GetComponent<Coin>().GetCoin(this);
                continue;
            }

            float dis = Mathf.Abs(Vector2.Distance(collider.transform.position, transform.position));
            if (min == -1 || min > dis)
            {
                min = dis;
                selectCollider = collider;
            }
        }

        if (isShadow)
        {
            onChange = true;

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (controlledObj != null)
                {
                    controlledObj.transform.parent = null;
                    shadowBox.enabled = true;
                    rb.gravityScale = 1f;
                    spriteRenderer.enabled = true;
                    Vector3 shadowPos = controlledObj.transform.position;
                    shadowPos.y = shadowPos.y - controlledObj.transform.localScale.y / 2f;
                    transform.position = shadowPos;
                    cameraMove.target = transform;
                    controlledObjRb = null;
                    gameObject.layer = LayerMask.NameToLayer("Shadow");
                    controlledObj = null;
                }
                else if (selectCollider)
                {
                    if (selectCollider.GetComponent<Shadow>() && selectCollider.GetComponent<Shadow>().onShadow)
                    {
                        gameObject.layer = selectCollider.gameObject.layer;
                        transform.position = selectCollider.transform.position;
                        shadowBox.enabled = false;
                        rb.gravityScale = 0f;
                        rb.linearVelocity = Vector2.zero;
                        spriteRenderer.enabled = false;
                        cameraMove.target = selectCollider.transform;
                        if (selectCollider.GetComponent<Rigidbody2D>())
                            controlledObjRb = selectCollider.GetComponent<Rigidbody2D>();
                        selectCollider.transform.parent = this.transform;
                        controlledObj = selectCollider.gameObject;
                    }
                    else if (selectCollider.CompareTag("ElevatorChain"))
                    {
                        selectCollider.transform.parent.GetComponentInChildren<Elevator>().FixChain();
                    }
                }
            }
        }
        else
        {
            if (controlledObj != null)
                controlledObj.transform.parent = null;
            controlledObj = null;

            if (selectCollider && Input.GetKeyDown(KeyCode.F))
            { 
                if (selectCollider.CompareTag("ElevatorButton"))
                {
                    selectCollider.transform.parent.GetComponentInChildren<Elevator>().ButtonClick();
                }
            }

            //onChange = false;
            gameObject.layer = LayerMask.NameToLayer("Player");
        }
    }

    private void Move()
    {
        movement.x = Input.GetAxisRaw("Horizontal");

        if (movement.x == 0f)
        {
            ani.SetBool("isRun", false);
        }
        else
        {
            if (movement.x > 0)
                spriteRenderer.flipX = true;
            else
                spriteRenderer.flipX = false;
            ani.SetBool("isRun", true);

            if (moveSoundLastTime + moveSoundDlay <= Time.time && isGround && !isShadow)
            {
                moveSoundLastTime = Time.time;
                audioSource.PlayOneShot(moveSounds[moveSoundIndex++]);
                moveSoundIndex = moveSoundIndex % moveSounds.Length;
            }

        }

        if (controlledObjRb != null)
        {
            controlledObjRb.AddForce(movement * movementSpeed * Time.deltaTime * 250f, ForceMode2D.Force);
        }
        else if (controlledObj != null)
        {
            controlledObj.transform.Translate(movement * movementSpeed * Time.deltaTime);
        }
        else
        {
            transform.Translate(movement * movementSpeed * Time.deltaTime);
        }

        if (rb.linearVelocity.y != 0)
        {
            isGround = false;
            ani.SetBool("isGround", false);
        }
        if (Input.GetButtonDown("Jump") && isGround && !isShadow)
        {
            OnJump();
        }
    }

    private void Change()
    {
        if ((!isShadow && isGround) || isShadow)
        {
            isMove = false;
            isShadow = !isShadow;

            audioSource.PlayOneShot(changeSound);
            characterBox.enabled = !isShadow;
            shadowBox.enabled = isShadow;
            spriteRenderer.flipY = isShadow;
            gameObject.transform.position += !isShadow ? Vector3.up * 1.25f : Vector3.zero;
            gameObject.layer = isShadow ? LayerMask.NameToLayer("Shadow") : LayerMask.NameToLayer("Player");
            onChange = !isShadow;
            ani.SetTrigger("Change");
        }
    }

    private void OnJump()
    {
        audioSource.PlayOneShot(jumpSound);
        isGround = false;
        ani.SetTrigger("doJump");
        ani.SetBool("isGround", false);
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
    }

    public void OnMove()
    {
        isMove = true;
    }

    public void GameOver()
    {
        SceneChangeManager.instance.GameOver();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isShadow)
            return;

        if (collision.gameObject.CompareTag("Floor"))
        {
            ani.SetBool("isGround", true);
            isGround = true;
        }

        if (collision.gameObject.CompareTag("EnemyHead"))
        {
            OnJump();
            collision.GetComponentInParent<Enemy>().Die();
        }

        if (collision.gameObject.CompareTag("ShadowChangeZone"))
        {
            onChange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("ShadowChangeZone"))
        {
            onChange = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isMove && collision.gameObject.CompareTag("Enemy"))
        {
            isMove = false;
            ani.SetTrigger("doDie");
        }

        if (collision.gameObject.CompareTag("End"))
        {
            isMove = false;
            SceneChangeManager.instance.ChangeScene(collision.gameObject.name);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Vector3 pos = transform.position;
        pos.y += 1f;
        Gizmos.DrawWireSphere(pos, radius);
    }
}
