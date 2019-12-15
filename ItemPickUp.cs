using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

namespace Item
{
    //-////////////////////////////////////////////////////
    ///
    /// All the logic for the character controller states and logic is in here!
    ///
    public class ItemPickUp : MonoBehaviour
    {
        [Tooltip("The SO that has all the items data")]
        public ItemPickUps_SO itemDefinition;
        
        [SerializeField]
        private StateManager characterAffected; //Probably want a characterStats instead so enemies cna have them and we can debuff/buff enemies in the same way

        private StateManager character;

        // The target character who pick-up the item
        private GameObject statsToModify;

        void Start()
        {
            itemDefinition = ScriptableObject.Instantiate(itemDefinition);
        }
        //-////////////////////////////////////////////////////
        ///
        public void UseItem(StateManager stateManager)
        {
            itemDefinition.OnPickUpItem(stateManager);
        }

        //-////////////////////////////////////////////////////
        ///
        private void OnTriggerEnter(Collider other) 
        {
            if(other.gameObject.tag == "Player")
            {
                // Update the character stats
                character = other.gameObject.GetComponent<StateManager>();
                UseItem(character);
                
                // Update the inventory of such character
                CharacterInventory inventory = other.gameObject.GetComponent<CharacterInventory>();
                bool storedSuccesfully = inventory.StoreItem(itemDefinition);
                
                if (storedSuccesfully)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}