using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Player : MonoBehaviour {

    BoxCollider2D _col;
    Rigidbody2D _rb2d;
    Animator _anim;
    SpriteRenderer _renderer;

    public GameObject win;
    public GameObject lose;

    public int hp = 3;
    public float immortalityTime;
    float immortalityTimeCurrent;

    public int _collidableLayer;
    float _unitsPerPixel = 1 / 16f;

    // Horizontal movement
    public float run_maxSpeed;
    public float run_distanceToMaxSpeed;
    [Range(0, 1)] public float run_airAccelerationFactor;
    [Range(0, 1)] public float run_deaccelerationDistanceFactor;
    float run_acceleration;
    float run_deacceleration;

    // Vertical movement
    public float jmp_maxJumpHeight;
    public float jmp_minJumpHeight;
    public float jmp_gravity;
    [Range(0, 90)] public float jmp_wallJumpAngle;
    public float jmp_maxDistanceWallJump;
    public float jmp_maxDistanceGrounded;
    public float jmp_maxBufferTime;
    float jmp_buffer;
    float jmp_maxVelocity;
    float jmp_minVelocity;

    // Animation aux
    public GameObject anim_wandParticle;
    public GameObject anim_jumpParticle;
    public GameObject anim_damageParticle;
    public GameObject wand;

    float anim_lastDirection = 0;
    float anim_lastFrameAction;
    float anim_midAir;
    float anim_landed;
    float anim_grounded;
    float anim_lastFrameGrounded;

    public Material damagedMaterial;
    public Material defaultMaterial;

    public float revealed;

    void Start() {
        _col = GetComponent<BoxCollider2D>();
        _rb2d = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();

        revealed = 0;

        // Setting up some constants
        _rb2d.gravityScale = jmp_gravity;

        jmp_maxVelocity = Mathf.Sqrt(2 * jmp_gravity * jmp_maxJumpHeight);
        jmp_minVelocity = Mathf.Sqrt(2 * jmp_gravity * jmp_minJumpHeight);

        run_acceleration = run_maxSpeed / ((2 * run_distanceToMaxSpeed) / run_maxSpeed);
        run_deacceleration = run_maxSpeed / ((2 * run_distanceToMaxSpeed * run_deaccelerationDistanceFactor) / run_maxSpeed);

        // Setting friction to zero so acceleration behaves as intended
        PhysicsMaterial2D tempMaterial = new PhysicsMaterial2D {
            friction = 0,
            bounciness = 0
        };
        _col.sharedMaterial = tempMaterial;
    }

    void Update() {
        HandleMovement();
        HandleAnimation();
        immortalityTimeCurrent = Mathf.Clamp(immortalityTimeCurrent - Time.deltaTime, 0, immortalityTime);
        if (immortalityTimeCurrent > 0)
            _renderer.material = damagedMaterial;
        else
            _renderer.material = defaultMaterial;

        if (hp == 0) {
            lose.SetActive(true);
            Cursor.visible = true;
            gameObject.SetActive(false);
        }

        Collider2D[] objectsInRange = Physics2D.OverlapBoxAll(transform.position, _col.size, 9, 1 << 13);
        if (objectsInRange.Length > 0) {
            lose.SetActive(true);
            Cursor.visible = true;
            gameObject.SetActive(false);
        }

    }

    public void Damage() {
        if (immortalityTimeCurrent == 0) {
            GameObject damage = Instantiate(anim_damageParticle, transform.position, Quaternion.identity);
            Destroy(damage, 1f);
            hp -= 1;
            immortalityTimeCurrent = immortalityTime;
        }
    }

    void HandleMovement() {
        float direction = Input.GetAxis("right") - Input.GetAxis("left");
        bool grounded = CheckCollision(Vector3.down, jmp_maxDistanceGrounded, 1 << _collidableLayer);
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 normalized = new Vector2(transform.position.x - mousePos.x, transform.position.y - mousePos.y).normalized;

        //// Which wall player is against so velocity is correctly apllied
        //int againstWall = 0;
        //if (CheckCollision(Vector3.left, jmp_maxDistanceWallJump, 1 << _collidableLayer))
        //    againstWall = 1;
        //else if (CheckCollision(Vector3.right, jmp_maxDistanceWallJump, 1 << _collidableLayer))
        //    againstWall = -1;

        wand.GetComponentInParent<Transform>().localPosition = normalized * -1;
        wand.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan((normalized * -1).y / (normalized * -1).x) * Mathf.Rad2Deg);

        // Run
        if (Input.GetButton("right") || Input.GetButton("left")) {
            float newVelocityX = grounded ? _rb2d.velocity.x + run_acceleration * direction * Time.deltaTime : _rb2d.velocity.x + run_acceleration * run_airAccelerationFactor * direction * Time.deltaTime;
            _rb2d.velocity = new Vector2(Mathf.Clamp(newVelocityX, -run_maxSpeed, run_maxSpeed), _rb2d.velocity.y);
        }
        if ((direction == 0 || GetSign(direction) != GetSign(_rb2d.velocity.x))) {
            float newVelocityX = _rb2d.velocity.x + (-GetSign(_rb2d.velocity.x) * run_deacceleration * Time.deltaTime);
            if (!grounded)
                newVelocityX = _rb2d.velocity.x + (-GetSign(_rb2d.velocity.x) * run_deacceleration * run_airAccelerationFactor * Time.deltaTime);
            newVelocityX = _rb2d.velocity.x > 0 ? Mathf.Max(newVelocityX, 0f) : Mathf.Min(newVelocityX, 0f);
            _rb2d.velocity = new Vector2(Mathf.Clamp(newVelocityX, -run_maxSpeed, run_maxSpeed), _rb2d.velocity.y);
        }

        // Jump + mouse1 dash
        if (Input.GetButtonDown("jump"))
            jmp_buffer = jmp_maxBufferTime;

        if (jmp_buffer > 0 && grounded) {
            _rb2d.velocity = new Vector2(_rb2d.velocity.x, jmp_maxVelocity);
            jmp_buffer = 0;

            GameObject poof = Instantiate(anim_jumpParticle, transform.position + Vector3.down * _col.bounds.extents.y, Quaternion.Euler(-90f, 90f, 0f));
            Destroy(poof, 0.3f);
        }

        //if (jmp_buffer > 0 && !grounded && againstWall != 0) {
        //    _rb2d.velocity = new Vector2(Mathf.Clamp(Mathf.Cos(Mathf.Deg2Rad * jmp_wallJumpAngle) * againstWall, -run_maxSpeed, run_maxSpeed), Mathf.Sin(Mathf.Deg2Rad * jmp_wallJumpAngle)) * jmp_maxVelocity;
        //    jmp_buffer = 0;
        //}

        // Mouse 1

        if (Input.GetButtonDown("mouse1")) {
            Camera.main.GetComponent<PlayerCamera>().DashTrauma();
            float rotation = normalized.x > 0 ? 180 - wand.transform.rotation.eulerAngles.z : -wand.transform.rotation.eulerAngles.z;
            GameObject poof = Instantiate(anim_wandParticle, wand.transform.position, Quaternion.Euler(rotation, 90f, 0f));
            Destroy(poof, 0.3f);

            _rb2d.velocity = new Vector2(normalized.x * jmp_maxVelocity + _rb2d.velocity.x, normalized.y * jmp_maxVelocity);
            Collider2D[] objectsInRange = Physics2D.OverlapBoxAll(transform.position + normalized * -1, Vector2.one * 2, 0);
            if (objectsInRange.Length > 0) {
                foreach (Collider2D obj in objectsInRange) {
                    if (obj.gameObject.GetComponent<PushableObject>() != null)
                        obj.gameObject.GetComponent<PushableObject>().PushBack(normalized * -1, 20f);

                    if (obj.gameObject.GetComponent<ExtinguishableObject>() != null) {
                        obj.gameObject.GetComponent<ExtinguishableObject>().Extinguish();
                        obj.gameObject.GetComponent<Rigidbody2D>().velocity = normalized * -3f;
                    }

                    if (obj.gameObject.GetComponent<VigiaAI>() != null) {
                        obj.gameObject.GetComponent<VigiaAI>().Destroy(normalized * -1);
                    }
                }
            }
        }

        jmp_buffer = Mathf.Clamp(jmp_buffer - Time.deltaTime, 0, jmp_maxBufferTime);
    }
    
    void HandleAnimation() {
        int direction = GetSign(Input.GetAxis("right") - Input.GetAxis("left"));
        bool grounded = CheckCollision(Vector3.down, jmp_maxDistanceGrounded, 1 << _collidableLayer);

        if (direction != anim_lastDirection && direction != 0 && GetSign(direction) == GetSign(_rb2d.velocity.x)) {
            _anim.SetBool("facingRight", direction == 1);
            _anim.SetBool("facingLeft", direction == -1);
            _anim.SetTrigger("changedAction");
            anim_lastDirection = direction;
        }

        if (_rb2d.velocity.x != 0 && !_anim.GetBool("running") && grounded) {
            AnimResetState();
            _anim.SetBool("running", true);
            _anim.SetTrigger("changedAction");
        }

        if (_rb2d.velocity.x == 0 && !_anim.GetBool("idle") && grounded) {
            AnimResetState();
            _anim.SetBool("idle", true);
            _anim.SetTrigger("changedAction");
        }

        if (!grounded) {
            AnimResetState();
            _anim.SetBool("jumping", true);
            _anim.SetTrigger("changedAction");
        }

    }

    void AnimResetState() {
        _anim.SetBool("running", false);
        _anim.SetBool("idle", false);
        _anim.SetBool("jumping", false);
    }

    void OnDrawGizmos() {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 normalized = new Vector2(transform.position.x - mousePos.x, transform.position.y - mousePos.y).normalized * -1;

        Gizmos.DrawWireCube(transform.position + normalized, Vector2.one * 2);
        Gizmos.DrawLine(transform.position, transform.position + normalized);
    }

    // Used to check if player is grounded or against wall or really touching anything in any direction
    bool CheckCollision(Vector3 direction, float distanceInPixels, int collisionMask) {
        float distance = Mathf.Abs((_col.bounds.extents.x * direction.x) + (_col.bounds.extents.y * direction.y)) + (distanceInPixels * _unitsPerPixel);

        Vector3 origin_0 = _col.bounds.center;
        Vector3 origin_1 = new Vector2(_col.bounds.center.x + (direction.y * _col.bounds.extents.x), _col.bounds.center.y + (direction.x * _col.bounds.extents.y));
        Vector3 origin_2 = new Vector2(_col.bounds.center.x - (direction.y * _col.bounds.extents.x), _col.bounds.center.y - (direction.x * _col.bounds.extents.y));

        //Debug.DrawLine(origin_0, origin_0 + (direction * distance), Color.magenta);
        //Debug.DrawLine(origin_1, origin_1 + (direction * distance), Color.magenta);
        //Debug.DrawLine(origin_2, origin_2 + (direction * distance), Color.magenta);

        RaycastHit2D cast_0 = Physics2D.Linecast(origin_0, origin_0 + (direction * distance), collisionMask);
        RaycastHit2D cast_1 = Physics2D.Linecast(origin_1, origin_1 + (direction * distance), collisionMask);
        RaycastHit2D cast_2 = Physics2D.Linecast(origin_2, origin_2 + (direction * distance), collisionMask);

        return cast_0 || cast_1 || cast_2;
    }

    // Mathf.Sign() returns positive for zero and that breaks how deacceleration is handled
    int GetSign(float num) {
        if (num > 0)
            return 1;
        if (num < 0)
            return -1;
        return 0;
    }

}
