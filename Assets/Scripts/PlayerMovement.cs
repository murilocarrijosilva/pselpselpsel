using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour {

    BoxCollider2D _col;
    Rigidbody2D _rb2d;

    public int collidableLayer;

    float unitsPerPixel = 0.0625f;

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

    bool m2_connected;
    float m2_distanceFromContactPoint;
    Vector2 m2_contactPoint;

    public GameObject zaHando;
    GameObject zaHandoClone;

    void Start() {
        _col = GetComponent<BoxCollider2D>();
        _rb2d = GetComponent<Rigidbody2D>();

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
    }

    void HandleMovement() {
        float direction = Input.GetAxis("right") - Input.GetAxis("left");
        bool grounded = CheckCollision(Vector3.down, jmp_maxDistanceGrounded, 1 << collidableLayer);
        
        // Which wall player is against so velocity is correctly apllied
        int againstWall = 0;
        if (CheckCollision(Vector3.left, jmp_maxDistanceWallJump, 1 << collidableLayer))
            againstWall = 1;
        else if (CheckCollision(Vector3.right, jmp_maxDistanceWallJump, 1 << collidableLayer))
            againstWall = -1;

        // Run
        if (Input.GetButton("right") || Input.GetButton("left")) {
            float newVelocityX = grounded ? run_acceleration * direction * Time.deltaTime : run_acceleration * run_airAccelerationFactor * direction * Time.deltaTime;
            _rb2d.velocity = new Vector2(Mathf.Clamp(_rb2d.velocity.x + newVelocityX, -run_maxSpeed, run_maxSpeed), _rb2d.velocity.y);
        }
        if ((direction == 0 || GetSign(direction) != GetSign(_rb2d.velocity.x)) && grounded) {
            float newVelocityX = _rb2d.velocity.x + (-GetSign(_rb2d.velocity.x) * run_deacceleration * Time.deltaTime);
            newVelocityX = _rb2d.velocity.x > 0 ? Mathf.Max(newVelocityX, 0f) : Mathf.Min(newVelocityX, 0f);
            _rb2d.velocity = new Vector2(newVelocityX, _rb2d.velocity.y);
        }

        // Jump + mouse1 dash

        if (Input.GetButtonDown("jump"))
            jmp_buffer = jmp_maxBufferTime;

        if (jmp_buffer > 0 && grounded) {
            _rb2d.velocity = new Vector2(_rb2d.velocity.x, jmp_maxVelocity);
            jmp_buffer = 0;
        }

        if (jmp_buffer > 0 && !grounded && againstWall != 0) {
            _rb2d.velocity = new Vector2(Mathf.Clamp(Mathf.Cos(Mathf.Deg2Rad * jmp_wallJumpAngle) * againstWall, -run_maxSpeed, run_maxSpeed), Mathf.Sin(Mathf.Deg2Rad * jmp_wallJumpAngle)) * jmp_maxVelocity;
            jmp_buffer = 0;
        }

        if (Input.GetButtonDown("mouse1")) {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 normalized = new Vector2(transform.position.x - mousePos.x, transform.position.y - mousePos.y).normalized;
            _rb2d.velocity = new Vector2(normalized.x * jmp_maxVelocity + _rb2d.velocity.x, normalized.y * jmp_maxVelocity);
        }

        //if (Input.GetButtonDown("mouse2")) {
        //    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //    Vector3 normalized = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y).normalized;

        //    zaHandoClone = Instantiate(zaHando, transform.position, Quaternion.identity);
        //    zaHandoClone.GetComponent<Rigidbody2D>().gravityScale = jmp_gravity;

        //    zaHandoClone.GetComponent<Rigidbody2D>().velocity = normalized * 30f;
        //}

        //if (Input.GetButtonUp("mouse2")) {
        //    Destroy(zaHandoClone);
        //}

        if (Input.GetButtonDown("mouse2")) {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 normalized = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, normalized, 100f, 1 << collidableLayer);

            if (m2_connected = hit) {
                m2_contactPoint = hit.point;
                m2_distanceFromContactPoint = Vector2.Distance(transform.position, m2_contactPoint);
            }

        }

        if (m2_connected) {
            GetComponent<SpringJoint2D>().enabled = true;
            GetComponent<SpringJoint2D>().enableCollision = true;
            GetComponent<SpringJoint2D>().autoConfigureDistance = false;
            GetComponent<SpringJoint2D>().anchor = Vector2.zero;
            GetComponent<SpringJoint2D>().connectedAnchor = m2_contactPoint;
            GetComponent<SpringJoint2D>().distance = m2_distanceFromContactPoint;
        }

        if (Input.GetButtonUp("mouse2")) {
            m2_connected = false;
            GetComponent<SpringJoint2D>().enabled = false;
        }


        //if (Input.GetButtonDown("mouse2")) {
        //    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //    Vector3 normalized = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y).normalized;
        //    RaycastHit2D hit = Physics2D.Raycast(transform.position, normalized, 100f, 1 << collidableLayer);

        //    if (connected = hit) {
        //        contactPoint = hit.point;
        //        mouseStartPos = Input.mousePosition;
        //    }
        //}

        //if (Input.GetButtonUp("mouse2"))
        //    connected = false;

        //if (connected) {
        //    Vector2 mousePos = Input.mousePosition;
        //    Vector2 distanceFromContact = new Vector2(mousePos.x - mouseStartPos.x, mousePos.y - mouseStartPos.y);

        //    Vector2 teste = new Vector2(contactPoint.x - transform.position.x, contactPoint.y - transform.position.y);
        //    Vector2 perpendicular = Vector2.Perpendicular(teste).normalized;

        //    _rb2d.velocity = Vector2.Dot(_rb2d.velocity, perpendicular) * perpendicular;

        //    Debug.DrawLine(transform.position, teste.normalized + (Vector2) transform.position);
        //    Debug.DrawLine(transform.position, _rb2d.velocity.normalized + (Vector2)transform.position, Color.cyan);
        //}

        //Debug.Log(_rb2d.velocity);

        jmp_buffer = Mathf.Clamp(jmp_buffer - Time.deltaTime, 0, jmp_maxBufferTime);

    }

    void CorrectVelocity() {

    }
    
    // Used to check if player is grounded or against wall or really touching anything in any direction
    bool CheckCollision(Vector3 direction, float distanceInPixels, int collisionMask) {
        float distance = Mathf.Abs((_col.bounds.extents.x * direction.x) + (_col.bounds.extents.y * direction.y)) + (distanceInPixels * unitsPerPixel);

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
    float GetSign(float num) {
        if (num > 0)
            return 1;
        if (num < 0)
            return -1;
        return 0;
    }

}
