using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunInstance : MonoBehaviour
{
    public Player GunOwner;

    public Gun Gun;

    public bool CanBeSelled = false;

    public bool IsDropped = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsDropped) return;
        if(collision.gameObject.layer == 3)
        {
            StartCoroutine(SetDroppedGun(collision.GetContact(0).point));
        }
    }
    private void Start() => Invoke("setDroppedStatus",.3f);

    void setDroppedStatus()
    {
        IsDropped = true;
    }

    IEnumerator SetDroppedGun(Vector3 dropPosition)
    {
        Vector3 rotation = transform.localEulerAngles;
        yield return new WaitForSeconds(.2f);
        transform.localPosition = dropPosition + new Vector3(0,.15f,0);
        transform.localEulerAngles = new Vector3(90,rotation.y,rotation.z);
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        transform.GetChild(0).GetComponent<BoxCollider>().material = null;
    }
}
