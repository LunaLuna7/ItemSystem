using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Player;

namespace Item
{
    //-////////////////////////////////////////////////////
    ///
    /// Stores items that the player has collected
    ///
    public class CharacterInventory : MonoBehaviour
    {
        [SerializeField, Tooltip("Contains all of the passive items")]
        private List<ItemPickUps_SO> passiveItems;
        [Tooltip("Contains all of the equipt items")]
        public List<ItemPickUps_SO> equiptItems;
        public ItemActivePickUps_SO emptyItem;

        [SerializeField, Tooltip("UI element use to show passive info")]
        private PassiveInventoryUI passiveInventoryUI;

        [SerializeField, Tooltip("UI element use to show passive info")]
        private EquiptInventoryUI equiptInventoryUI;
        private int currentUnequipHover = 0;
        private int prevHoverIndex = 0;

        [SerializeField]
        private List<ItemPickUps_SO> initialEquipment;

        //-////////////////////////////////////////////////////
        ///
        private void Start() 
        {
            InitialEquipment();    
        }

        //-////////////////////////////////////////////////////
        ///
        /// Sets up equip items for the start of the game
        /// 
        private void InitialEquipment()
        {
            // given that the player starts up with a invenotry full of empty we handle itme equipment ourselves
            for (int i = 0; i < initialEquipment.Count; i++)
            {
                ItemPickUps_SO temp = ScriptableObject.Instantiate(initialEquipment[i]);
                equiptItems[i] = temp;
                equiptInventoryUI.UpdateItemSlot(i, temp.itemIcon, temp.name);
            }
        }

        //-////////////////////////////////////////////////////
        ///
        public bool StoreItem(ItemPickUps_SO itemToStore)
        {
            if (itemToStore.isEquipment)
            {
                return AttemptStoreEquipmentItem(itemToStore);
            }
            else
            {
                return StorePassiveItem(itemToStore);
            }
        }

        //-////////////////////////////////////////////////////
        ///
        /// Stores the item in the player's inventory and updates
        /// the inventory UI
        /// 
        public bool StorePassiveItem(ItemPickUps_SO itemToStore)
        {
            if (itemToStore.isTemporal)
            {
                passiveItems.Add(itemToStore);
                passiveInventoryUI.CreateTemporalItemSlot(itemToStore.itemIcon, itemToStore.name, itemToStore.effectTime);
            }
            else
            {
                int itemToStoreIndex = FindPassiveItemIndex(itemToStore.name);

                // New Item
                if (itemToStoreIndex == -1)
                {
                    passiveItems.Add(itemToStore);
                    passiveInventoryUI.CreateItemSlot(itemToStore.itemIcon, itemToStore.name);
                }
                // Update existing item
                else
                {
                    passiveInventoryUI.UpdateItemSlot(itemToStoreIndex, itemToStore.itemIcon, itemToStore.name);
                }
            }

            // since we will always succed at storing a passive
            return true;
        }

        //-////////////////////////////////////////////////////
        ///
        private bool AttemptStoreEquipmentItem(ItemPickUps_SO itemToStore)
        {
            int emptySlot = AvailableSpaceInEquiptInventory();
            int repeatedItem = FindEquiptItemIndex(itemToStore.itemName);

            // (skill not equipt or it is equpt but not unqiue) and they have space
           if ((repeatedItem == -1 || equiptItems[repeatedItem].isUnique == false) && emptySlot != -1)
           {
               StoreEquipmentItem(itemToStore as ItemActivePickUps_SO, emptySlot);
               return true;
           }
           else
           {
               Debug.Log("Can't Equipt, player already has it or full equipment");
               return false;
               // Bring up the trading window system or just dont let them
           }
        }

        //-////////////////////////////////////////////////////
        ///
        private void StoreEquipmentItem(ItemActivePickUps_SO itemToStore, int argIndex)
        {
            equiptItems[argIndex] = itemToStore;
            equiptInventoryUI.UpdateItemSlot(argIndex, itemToStore.itemIcon, itemToStore.name);
        }

        //-////////////////////////////////////////////////////
        ///
        public void DropEquipmentItem(StateManager stateManager)
        {
            int index = currentUnequipHover;
            ItemPickUps_SO oldItem = equiptItems[index];

            if (oldItem.itemName == "Empty")
            {
                return;
            }
            equiptItems[index] = emptyItem;
            equiptInventoryUI.UpdateItemSlot(index, equiptItems[index].itemIcon, equiptItems[index].name);

            CreateItem(oldItem);
        }

        //-////////////////////////////////////////////////////
        ///
        private void CreateItem(ItemPickUps_SO itemToCreate)
        {
            Rigidbody itemSpawned = Instantiate(itemToCreate.itemSpawnObject, transform.position - new Vector3(1.5f,-1.5f,1.5f), Quaternion.identity);
            
            Renderer itemMaterial = itemSpawned.GetComponent<Renderer>();
            itemMaterial.material = itemToCreate.itemMaterial;

            ItemPickUp itemType = itemSpawned.GetComponent<ItemPickUp>();
            itemType.itemDefinition = itemToCreate;
        }

        //-////////////////////////////////////////////////////
        ///
        /// Attempts to find and return an item slot with the correct name
        /// 
        public int FindPassiveItemIndex(string argName)
        {
            for (int i = 0; i < passiveItems.Count; i++)
            {
                if (passiveItems[i].itemName == argName)
                {
                    return i;
                }
            }

            // If the item doesnt exist then return -1
            return -1;
        }

        //-////////////////////////////////////////////////////
        ///
        /// Attempts to find and return an item slot with the correct name
        /// 
        public int FindEquiptItemIndex(string argName)
        {
            for (int i = 0; i < equiptItems.Count; i++)
            {
                if (equiptItems[i].itemName == argName)
                {
                    return i;
                }
            }

            return -1;
        }

        //-////////////////////////////////////////////////////
        ///
        public void EnableEquiptmentHover(bool argValue)
        {
            equiptInventoryUI.EnableCurrentEquiptSlotHover(argValue, currentUnequipHover);
        }

        //-////////////////////////////////////////////////////
        ///
        /// Checks if the equip inventory has an available space
        /// if so, it retruns it.
        public int AvailableSpaceInEquiptInventory()
        {
            for (int i = 0; i < equiptItems.Count; i++)
            {
                if (equiptItems[i].itemName == "Empty")
                {
                    return i;
                }
            }

            return -1;
        }

        //-////////////////////////////////////////////////////
        ///
        public void OnMoveRightEquiptInventoryHover()
        {
            prevHoverIndex = currentUnequipHover;
            if (currentUnequipHover < equiptItems.Count -1)
            {
                currentUnequipHover++;
            }
            else
            {
                currentUnequipHover = 0;
            }
            
            equiptInventoryUI.UpdateEquipSlotHover(prevHoverIndex, currentUnequipHover);
        }

        //-////////////////////////////////////////////////////
        ///
        public void OnMoveLeftEquiptInventoryHover()
        {
            prevHoverIndex = currentUnequipHover;
            
            if (currentUnequipHover > 0)
            {
                currentUnequipHover--;
            }
            else
            {
                currentUnequipHover = equiptItems.Count -1;
            }

            equiptInventoryUI.UpdateEquipSlotHover(prevHoverIndex, currentUnequipHover);
        }

        //-////////////////////////////////////////////////////
        ///
        /// Since the player can drop and pick up abilities 
        /// at all times we cant just rely on the size of the equip
        /// list to pick a random index skill to target given that
        /// player could have ability on slot 0 and 2 but not in 1.
        public int GetRandomEquipItemIndex()
        {
            List<int> possibleIndex = new List<int>();
            for (int i = 0; i < equiptItems.Count -1; i++)
            {
                // we store all the equip inv index that are none empty
                if (equiptItems[i].itemName != "Empty")
                {
                    possibleIndex.Add(i);
                }
            }

            if (possibleIndex.Count > 0)
            {
                return possibleIndex[Random.Range(0, possibleIndex.Count)];
            }

            return -1;
        }
    }
}