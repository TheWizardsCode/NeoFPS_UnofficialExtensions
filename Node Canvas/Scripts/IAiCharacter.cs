#if NODE_CANVAS_PRESENT
using NeoFPS;
using UnityEngine;

namespace WizardsCode.NeoFPS.BehaviourTree
{
    public interface IAiCharacter : ICharacter
    {
        /// <summary>
        /// Add a force to the character outside of the usual movement logic (eg. explosions, etc)
        /// </summary>
        /// <param name="force">Force vector in world coordinates.</param>
        /// <param name="mode">Type of force to apply. See the <see href="https://docs.unity3d.com/ScriptReference/ForceMode.html">Unity Scripting Reference.</see></param>
        /// <param name="disableGroundSnapping">Block the controller from snapping to the ground on the frame this is applied</param>
        void AddForce(Vector3 force, ForceMode mode = ForceMode.Force, bool disableGroundSnapping = false);    }
}
#endif