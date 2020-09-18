using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileRocket : Projectile
{
    public Explosion explosion;

    public LayerMask layermask;

    Rigidbody rb;

    int safeguard = 1;


    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        Physics.IgnoreCollision(this.GetComponent<Collider>(), player.GetComponent<Collider>());
        Physics.IgnoreLayerCollision(9,11);
        Invoke("Kill", life);
        rb.AddForce(transform.forward * speed, ForceMode.VelocityChange);
    }

    void Kill()
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        
    }

    private void OnTriggerEnter(Collider col)
    {

    }

    private void OnCollisionEnter(Collision col)
    {
        if (layermask == (layermask | (1 << col.gameObject.layer)))
        {
            if (col.gameObject.GetComponent<PlayerStats>() != player)
            {
                bool isValid = true;
                if (col.gameObject.GetComponent<PlayerHitbox>() != null && col.gameObject.GetComponent<PlayerHitbox>().player == player)
                {
                    isValid = false;
                }

                if (isValid && safeguard == 1)
                {
                    player.GetComponent<PlayerNetworkCalls>().RpcSpawnExplosion(player.netIdentity, transform.position, baseDamage, damageType);
                    safeguard++;
                    Kill();
                }
            }
        }
    }
}
