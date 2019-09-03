using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbProjectile : MonoBehaviour
{

    public float speed;
    public int damage;
    public float life;

    public Collider damageCollider;

    public OrbDamage damager;

    public PlayerStats owner;

    public PuckOrb ownersMotor;

    public Vector3 moveDirection;

    public LayerMask ignorelayers;


    private void Start()
    {
        Invoke("Kill", life);
        ownersMotor.orbIsActive = true;
        Physics.IgnoreLayerCollision(11, 13, true);
    }

    private void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;

        RaycastHit hit;
        if (Physics.Raycast(this.transform.position - (moveDirection.normalized * 0.5f), moveDirection, out hit, 1f, ignorelayers))
        {
            moveDirection = Vector3.Reflect(moveDirection, hit.normal);
        }
    }





    public void Kill()
    {
        ownersMotor.orbIsActive = false;
        Destroy(this.gameObject);
    }
}
