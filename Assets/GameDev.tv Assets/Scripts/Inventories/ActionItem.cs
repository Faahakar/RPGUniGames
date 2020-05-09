using System;
using RPG.Inventories;
using RPG.Stats;
using UnityEngine;

namespace GameDevTV.Inventories
{
    /// <summary>
    /// An inventory item that can be placed in the action bar and "Used".
    /// </summary>
    /// <remarks>
    /// This class should be used as a base. Subclasses must implement the `Use`
    /// method.
    /// </remarks>
    [CreateAssetMenu(menuName = ("GameDevTV/GameDevTV.UI.InventorySystem/Action Item"))]
    public class ActionItem : InventoryItem
    {
        // CONFIG DATA
        [Tooltip("Does an instance of this item get consumed every time it's used.")]
        [SerializeField] bool consumable = false;

        public ItemEffect itemEffect = null;

       
        /// <summary>
        /// Trigger the use of this item. Override to provide functionality.
        /// </summary>
        /// <param name="user">The character that is using this action.</param>
        public virtual void Use(GameObject user)
        {            
             ItemEffect effectInstance = Instantiate<ItemEffect>(itemEffect);       
        }
        public ItemEffect GetItemEffect()
        {
            return itemEffect;
        }
        

        public bool isConsumable()
        {
            return consumable;
        }
    }
}