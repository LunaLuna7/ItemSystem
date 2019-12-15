using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Sirenix.OdinInspector;

namespace Item
{
    public enum ItemTypeDefinition{ NONE, MAXHEALTH, SPEED, ATTACK, DEFENSE, BOND}
    
    //-////////////////////////////////////////////////////
    ///
    /// All the data for a loot item in the game is encapsulated
    /// in this class.
    ///
    public abstract class ItemPickUps_SO : ScriptableObject
    {
        [SerializeField, Tooltip("The name that will identify this object")]
        public string itemName = "New Item";

        [SerializeField, TextArea(3, 10)]
        private string itemDescription;

        [Tooltip("How likely is this item to appear compare to others")]
        public int spawnChanceWeight = 0;

        [SerializeField, Tooltip("DOes this item goes on active equipment"), Space]
        public bool isEquipment = false;

        [Tooltip("Can the player grab multiple of this item")]
        public bool isUnique = false;

        [Tooltip("Is this a limited time item")]
        public bool isTemporal = false;

        [ShowIf("isTemporal")]
        public float effectTime = 0;

        protected Timer countdownTimer;

        [Header("Item Look"), Space]
        public Material itemMaterial = null;

        [OnInspectorGUI("DrawPreview", append:true)]
        public Sprite itemIcon = null;

        public Rigidbody itemSpawnObject = null;

        //-////////////////////////////////////////////////////
        ///
        /// What should happen when the player picks up the item
        ///
        public virtual void OnPickUpItem(StateManager stateManager)
        {
            
        }

        //-////////////////////////////////////////////////////
        ///
        private void DrawPreview()
        {
            if (this.itemIcon == null) return;

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(this.itemIcon.texture);
            GUILayout.EndVertical();
        }
    }
}