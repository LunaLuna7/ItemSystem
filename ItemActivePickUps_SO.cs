using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

namespace Item
{
    //-////////////////////////////////////////////////////
    ///
    /// Items that require active use from the player
    ///
    public abstract class ItemActivePickUps_SO : ItemPickUps_SO
    {
        [Header("Active Attributes"), Space]
        [SerializeField, Tooltip("is the skill currently being use")]
        public bool isActive = false;

        [SerializeField, Tooltip("can the skill be used")]
        public bool canUse = true;

        [SerializeField, Tooltip("How long the skill remains active")]
        public float skillDurationTime;

        [SerializeField, Tooltip("How long the skill remains active")]
        public float skillCoolDownTime;     

        public virtual void AttemptUseSkill(ref InputManager.InputInfo input, StateManager stateManager)
        {
            if (stateManager.playerState == StateManager.PlayerStates.HOLD || stateManager.playerState == StateManager.PlayerStates.DEAD)
            {
                return;
            }

            if (input.pressed && CanUseSkill(stateManager))
            {
                OnStartSkill(stateManager);
            }
        }

        //-////////////////////////////////////////////////////
        ///
        /// Gets call at the beginning of using the skill
        ///
        public virtual void OnStartSkill(StateManager stateManager)
        {
            isActive = true;
            canUse = false;

            // Limit skill to last the set duration
            TimerManager.Instance.StartCountdownTimer(skillDurationTime, () => { isActive = false; OnEndSkill(stateManager); } );
        }

        //-////////////////////////////////////////////////////
        ///
        /// Gets call at the end of the skill
        ///
        public virtual void OnEndSkill(StateManager stateManager)
        {

        }

        //-////////////////////////////////////////////////////
        ///
        /// Gets call while the skill is being use during FixedUpdate
        ///
        public virtual void HandleSkill(StateManager stateManager)
        {
            if (stateManager.playerState == StateManager.PlayerStates.HOLD || stateManager.playerState == StateManager.PlayerStates.DEAD)
            {
                return;
            }
        }

        //-////////////////////////////////////////////////////
        ///
        /// Gets call on Update to update the skill state
        ///
        public virtual void UpdateSkill(StateManager stateManager)
        {
            if (stateManager.playerState == StateManager.PlayerStates.HOLD || stateManager.playerState == StateManager.PlayerStates.DEAD)
            {
                return;
            }
        }

        //-////////////////////////////////////////////////////
        ///
        /// Weather this skill can be used
        ///
        public virtual bool CanUseSkill(StateManager stateManager)
        {
            return canUse;
        }

        //-////////////////////////////////////////////////////
        ///
        /// When the user collides with an object while using this skill
        ///
        public virtual void OnSkillCollision(Collision collision, StateManager stateManager)
        {

        }

        //-////////////////////////////////////////////////////
        ///
        /// When the user trigger collides with an object while using this skill
        ///
        public virtual void OnSkillTrigger(Collider collision)
        {

        }

        //-////////////////////////////////////////////////////
        ///
        /// Helper function to keep track of skill duration
        ///
        public void SkillDurationTimeTrack(ref bool inUseSkill, Player.StateManager.SkillDelegate condition, ref float skillDurationRunTime, float skillDurationInitial)
        {
            skillDurationRunTime -= Time.deltaTime;

            if (condition())
            {
                skillDurationRunTime = skillDurationInitial;
                inUseSkill = false;
            }
        }

        //-////////////////////////////////////////////////////
        ///
        public override void OnPickUpItem(StateManager stateManager)
        {
        }
    }
}