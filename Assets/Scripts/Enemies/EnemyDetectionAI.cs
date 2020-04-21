using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyDetectionAI : MonoBehaviour {

    ConeProjection _fov;
    public GameObject player;
    public GameObject alertObj;
    public int playerLayer;
    public int terrainLayer;
    public float alertDecreaseRate;

    public Vector2 lastKnownPlayerPos;

    float alertLevel;

    protected Animator _anim;
    public float attackChargeTime;
    public float attackSpeed;

    protected float attackBuffer;
    protected string currentState;
    protected string lastFrameState;

    void Start() {
        _fov = GetComponent<ConeProjection>();
        _anim = GetComponent<Animator>();
    }

    void Update() {
        if (_fov.lightGameObject == null)
            return;

        Collider2D[] result = new Collider2D[12];
        _fov.lightGameObject.GetComponent<PolygonCollider2D>().OverlapCollider(new ContactFilter2D(), result);

        bool isPlayerInFov = false;

        foreach (Collider2D col in result) {
            if (!col)
                continue;
            if (col.gameObject.layer == 10) {
                isPlayerInFov = true;
                break;
            }
        }

        if (isPlayerInFov)
            alertLevel = 1;

        if (!isPlayerInFov && alertLevel > 0) {
            alertLevel = Mathf.Clamp(alertLevel - alertDecreaseRate * Time.deltaTime, 0, 1);
            player.GetComponent<Player>().revealed = Mathf.Min(player.GetComponent<Player>().revealed, alertLevel);
        }
        player.GetComponent<Player>().revealed = Mathf.Max(player.GetComponent<Player>().revealed, alertLevel);

        if (player.GetComponent<LightableObject>()._isInLight) {
            float angle = Mathf.Atan2(player.transform.position.y - transform.position.y, player.transform.position.x - transform.position.x) * Mathf.Rad2Deg;
            if (angle < 0)
                angle = 360 + angle;
            if (angle >= _fov.angleStartRotate && angle <= _fov.angleEndRotate) {
                RaycastHit2D hit = Physics2D.Linecast(transform.position, player.transform.position, 1 << terrainLayer);
                if (!hit)
                    alertLevel = 1;
            }
        }

        if (alertLevel == 0) {
            currentState = "idle";
            NoAlert();
        }

        if (alertLevel > 0 && alertLevel < 1) {
            currentState = "alert";
            Alert();
        }

        if (alertLevel == 1) {
            attackBuffer = Mathf.Min(attackChargeTime, attackBuffer);
            lastKnownPlayerPos = player.transform.position;
            currentState = "attack";
            Attack();
        }

        if (alertLevel > 0) {
            alertObj.SetActive(true);
            alertObj.GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(1, 1, 0, 0), Color.red, alertLevel * alertLevel);
        }
        else
            alertObj.SetActive(false);

        if (lastFrameState != currentState)
            lastFrameState = currentState;

    }

    public abstract void Attack();
    public abstract void Alert();
    public abstract void NoAlert();

}
