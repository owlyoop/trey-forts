using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DmgText : MonoBehaviour
{
    void Update()
    {
        Vector3 pos = transform.position;
        pos += Vector3.up * 0.8f * Time.deltaTime;
        transform.position = pos;
    }
}
