using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

namespace Item
{
    //-////////////////////////////////////////////////////
    ///
    /// Items whose effects does not require active use from
    /// the player
    ///
    [CreateAssetMenu(fileName = "NewPassiveItem", menuName = "AloneTogether/Item/New Passive Pick-up")]
    public class ItemPassivePickUps_SO : ItemPickUps_SO
    {   
        [SerializeField]
        private List<StatModifier> statsToModify;

        //-////////////////////////////////////////////////////
        ///
        /// Pass all stat modifiers into an Entities Char stats
        /// and modifies the respective stats
        ///
        public override void OnPickUpItem(StateManager stateManager)
        {
            base.OnPickUpItem(stateManager);
            foreach (StatModifier eachStat in statsToModify)
            {
                stateManager.playerStats.ModifyStat(eachStat.itemType, eachStat.valueAmount);
                if (isTemporal)
                {
                    TimerManager.Instance.StartCountdownTimer(effectTime, () => { RemoveEffect(stateManager, eachStat); } );
                }
            }
        }

        //-////////////////////////////////////////////////////
        ///
        /// Helper function to keep track of skill cooldown
        ///
        public void RemoveEffect(StateManager stateManager, StatModifier eachStat)
        {
            stateManager.playerStats.ModifyStat(eachStat.itemType, -eachStat.valueAmount);
        }
    }
}