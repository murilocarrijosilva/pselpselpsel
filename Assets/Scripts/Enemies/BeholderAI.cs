using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeholderAI : EnemyDetectionAI {

    public GameObject projectile;

    public override void Alert() {
        if (lastFrameState != "alert")
            _anim.SetTrigger("idle");
    }

    public override void Attack() {
        attackBuffer = Mathf.Clamp(attackBuffer - attackSpeed * Time.deltaTime, 0, attackChargeTime);

        if (lastFrameState != "attack")
            _anim.SetTrigger("attack");

        if (attackBuffer == 0) {
            GameObject project = Instantiate(projectile, transform.right * -1 + transform.position, Quaternion.identity);
            project.GetComponent<BeholderProjectile>().direction = (lastKnownPlayerPos - (Vector2) transform.position).normalized;
            project.GetComponent<BeholderProjectile>().speed = 15f;
            project.GetComponent<BeholderProjectile>().loadTime = 1f;
            attackBuffer = attackChargeTime;
        }
    }

    public override void NoAlert() {
        
    }

}
