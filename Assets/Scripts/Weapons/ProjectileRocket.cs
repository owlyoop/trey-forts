using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileRocket : Projectile
{


    public Explosion explosion;

    public LayerMask layermask;

    Rigidbody rb;


    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        Physics.IgnoreCollision(this.GetComponent<Collider>(), player.GetComponent<Collider>());
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
        if (player.photonView.IsMine)
        {
            if (layermask == (layermask | (1 << col.gameObject.layer)))
            {
                if (OwnerPunID == player.GetComponent<PhotonView>().ViewID && col.gameObject.GetComponent<PlayerStats>() != player)
                {
                    bool isValid = true;
                    if (col.gameObject.GetComponent<PlayerHitbox>() != null && col.gameObject.GetComponent<PlayerHitbox>().player == player)
                    {
                        isValid = false;
                    }

                    if (isValid)
                    {
                        player.GetComponent<PhotonView>().RPC("SpawnExplosion", RpcTarget.AllViaServer, OwnerPunID, transform.position, baseDamage, damageType);
                        Kill();
                    }
                }
            }
        }
    }
}
