using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GunInstance : NetworkBehaviour
{
    //[SyncVar]
    public Player GunOwner;
    //[SyncVar]
    public Gun Gun;
    //[SyncVar]
    public bool CanBeSelled = false;
    //[SyncVar]
    public bool IsDropped = false;
    //[SyncVar]
    public bool CanBePicked = false;
    //[SyncVar]
    public int Ammo;
    //[SyncVar]
    public int Magazine;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsDropped) return;
        if(collision.gameObject.layer == 3)
        {
            StartCoroutine(SetDroppedGun(collision.GetContact(0).point));
        }
    }

    public void SetPickStatus()
    {
        Debug.Log("Setting gun pick status");
        CanBePicked = true;
    }

    public IEnumerator SetDroppedGun(Vector3 dropPosition)
    {
        Vector3 rotation = transform.localEulerAngles;
        yield return new WaitForSeconds(.2f);
        transform.localPosition = dropPosition + new Vector3(0,.15f,0);
        transform.localEulerAngles = new Vector3(90,rotation.y,rotation.z);
        Rigidbody rb = transform.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        transform.GetChild(0).GetComponent<BoxCollider>().material = null;
    }
}
