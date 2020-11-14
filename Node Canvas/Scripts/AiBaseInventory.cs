#if NODE_CANVAS_PRESENT
using NeoFPS;

namespace WizardsCode.AI
{
    /// <summary>
    /// An Inventory for AI Characters.
    /// At present this is just an renamed FpsInventoryQuickSwitch
    /// class. This has been adopted for convenience in development,
    /// but we really ought to replace this with a lower functionality
    /// solution for AIs.
    /// </summary>
    public class AiBaseInventory : FpsInventoryQuickSwitch
    {
        /// <summary>
        /// Get the minimum attack range with the currently equipped weapon.
        /// If no weapon is currently equipped return zero.
        /// </summary>
        public float minimumAttackRange
        {
            get {
                // TODO Cache the weapon
                return selected.GetComponent<AiBaseWeapon>().minimumRange;
            }
        }


        /// <summary>
        /// Get the maximum attack range with the currently equipped weapon.
        /// If no weapon is currently equipped return zero.
        /// </summary>
        public float maximumAttackRange
        {
            get
            {
                // TODO Cache the weapon
                return selected.GetComponent<AiBaseWeapon>().maximumRange;
            }
        }
    }
}
#endif