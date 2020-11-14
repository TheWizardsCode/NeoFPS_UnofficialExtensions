#if NODE_CANVAS_PRESENT
using NeoFPS;
using UnityEngine;

namespace WizardsCode.AI
{
    public interface IAiWeapon : IDamageSource
    {
        float minimumRange { get; }
        float maximumRange { get; }
        float damageAmount { get; }
        float impactForce { get; }
        float recoveryTime { get; }
        float maximumAttackAngle { get; }
        bool isOneHanded { get; }
        Transform hitDetector { get; }
    }
}
#endif
