using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;

//-////////////////////////////////////////////////////
///
/// The base Stats for a character entity
///
public abstract class CharacterStats : MonoBehaviour
{
    public int baseMaxHealth = 0;
    public int currentMaxHealth = 0;

    public int baseAttack = 0;
    public int currentAttack = 0;

    public int baseDefense = 0;
    public int currentDefense = 0;

    public int baseSpeed = 0;
    public int currentSpeed = 0;

    public int baseTurnSpeed = 0;
    public int currentTurnSpeed = 0;

    //-////////////////////////////////////////////////////
    ///
    public virtual void ModifyStat(ItemTypeDefinition argItemType, int valueAmount)
    {
        switch(argItemType)
        {
            case ItemTypeDefinition.MAXHEALTH:
                currentMaxHealth += valueAmount;
                this.gameObject.GetComponent<IHealable>()?.Heal(valueAmount);
                
                break;

            case ItemTypeDefinition.ATTACK:
                currentAttack += valueAmount;
                break;

            case ItemTypeDefinition.DEFENSE:
                currentDefense += valueAmount;
                break;

            case ItemTypeDefinition.SPEED:
                currentSpeed += valueAmount;
                break;

            case ItemTypeDefinition.BOND:
                //Todo: more complicated effect?
                break;
        }
    }

    //-////////////////////////////////////////////////////
    ///
     public void OnAfterDeserialize()
    {
        ResetStat(ref currentMaxHealth, baseMaxHealth);
        ResetStat(ref currentAttack, baseAttack);
        ResetStat(ref currentDefense, baseDefense);
        ResetStat(ref currentSpeed, baseSpeed);
    }

    //-////////////////////////////////////////////////////
    ///
    public void OnBeforeSerialize(){}

    //-////////////////////////////////////////////////////
    ///
    protected void ResetStat(ref int targetStat, int defaultStat)
    {
        targetStat = defaultStat;
    }
}
