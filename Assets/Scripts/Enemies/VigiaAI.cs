using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VigiaAI : EnemyDetectionAI
{
    public GameObject destroyed;
    public GameObject[] pieces;

    public override void Alert() {
        if (lastFrameState != "alert")
            _anim.SetTrigger("idle");
    }

    public override void Attack() {
        attackBuffer = Mathf.Clamp(attackBuffer - attackSpeed * Time.deltaTime, 0, attackChargeTime);

        if (lastFrameState != "attack")
            _anim.SetTrigger("attack");

        if (attackBuffer == 0) {
            player.GetComponent<Player>().Damage();
            attackBuffer = attackChargeTime;
        }
    }

    public override void NoAlert() {

    }

    public void Destroy(Vector2 direction) {
        destroyed.SetActive(true);
        gameObject.SetActive(false);
        foreach (GameObject piece in pieces) {
            piece.GetComponent<Rigidbody2D>().velocity = direction * Random.Range(3, 8f);
        }
    }

}
