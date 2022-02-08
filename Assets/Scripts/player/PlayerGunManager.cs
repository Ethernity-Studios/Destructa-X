using System.Linq;
using JetBrains.Annotations;
using material;
using Mirror;
using UnityEngine;

public class PlayerGunManager : NetworkBehaviour
{
    public Collider coll;

    public Gun Primary;
    public Gun Secondary;
    private Camera camyr;

    private void Start()
    {
        if (!isLocalPlayer) return;

        camyr = Camera.main;
        coll = GetComponent<Collider>();
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        control();
    }

    public void Shoot()
    {
        // todo
    }

    public void Reload()
    {
        // todo
    }


    public void control()
    {
        // todo
        // switch weapons
        // shoot
        // reload
        // drop weapon
        // pickup weapon
    }

    // Debug.DrawRay(pos.position, pos.TransformDirection(Vector3.forward) * 10, Color.cyan);
    public BulletPath BulletPenetration(Transform pos, int maxpen = 5) // FIXME
    {
        var direction = pos.TransformDirection(Vector3.forward) * 10;

        BulletImpact[] impacts = null;
        RaycastHit? hitus = null;
        var damagemod = 0f;

        var origin = pos;

        for (var i = 0; i <= maxpen; i++)
        {
            var ray = new Ray(origin.position, direction);

            if (Physics.Raycast(ray, out var hit) && hit.collider.GetComponent<EntityBase>())
            {
                hitus = hit;
            }
            else if (hit.collider.GetComponent<Penetration>())
            {
                var pen = hit.collider.GetComponent<Penetration>();

                if (coll.Raycast(new Ray(hit.transform.position + Vector3.one * 20, direction * -1), out var hitb,
                        50.0f))
                {
                    var len = (hitb.transform.position - hit.transform.position).magnitude;
                    origin = hitb.transform; // todo some small offset

                    damagemod += pen.value * len;
                    if (damagemod >= 100)
                    {
                        impacts.Append(new BulletImpact {Location = hit, Penetrated = true});
                        impacts.Append(new BulletImpact {Location = hitb, Penetrated = false});
                        return new BulletPath {Impacts = impacts, hit = hitus, DamageModifier = damagemod};
                    }

                    impacts.Append(new BulletImpact {Location = hit, Penetrated = true});
                    impacts.Append(new BulletImpact {Location = hitb, Penetrated = true});
                }
                else
                {
                    impacts.Append(new BulletImpact {Location = hit, Penetrated = false});
                    return new BulletPath {Impacts = impacts, hit = hitus, DamageModifier = damagemod};
                }
            }
        }

        return new BulletPath {Impacts = impacts, hit = hitus, DamageModifier = damagemod};
    }


    public struct BulletImpact
    {
        public bool Penetrated;
        public RaycastHit Location;
    }

    public struct BulletPath
    {
        [CanBeNull] public BulletImpact[] Impacts;
        public RaycastHit? hit;
        public float? DamageModifier;
    }
}