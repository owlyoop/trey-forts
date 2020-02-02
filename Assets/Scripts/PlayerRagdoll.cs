using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRagdoll : MonoBehaviour
{

    public Collider[] colliders;

    public Collider Head;
    public Collider Spine;
    public Collider Hips;
    public Collider LeftUpLeg;
    public Collider LeftLeg;
    public Collider RightUpLeg;
    public Collider RightLeg;
    public Collider LeftArm;
    public Collider LeftForearm;
    public Collider RightArm;
    public Collider RightForearm;

    public List<Collider> colliderCollection = new List<Collider>();

    public SkinnedMeshRenderer bodyToColor;
    public SkinnedMeshRenderer jointsToColor;

    private void Start()
    {
    }

    public void SpawnRagdoll()
    {

    }

    public void SetRagdollMaterials(Material body, Material joints)
    {
        bodyToColor.material = body;
        jointsToColor.material = joints;
    }
    

}
