using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mg.pummelz.Insert_groupname
{
    public class MGPumInsert_groupnameUtilFunctions : MonoBehaviour
    {
        //search for kill and return if found
        //if no kill possible, choose who to damage
        public MGPumAttackCommand chooseTarget(List<MGPumAttackCommand> allPosibilites)
        {
            MGPumAttackCommand bestOption = null;
            
            foreach (MGPumAttackCommand command in allPosibilites)
            {
                if(command.attacker.currentPower >= command.defender.currentHealth) //if enemy can be eleminated
                {
                    bestOption = chooseBestKill(bestOption, command);
                }
            }

            if(bestOption != null)
            {
                return bestOption;
            }
            else
            {
                foreach (MGPumAttackCommand command in allPosibilites)
                {
                    if (command.attacker.currentPower < command.defender.currentHealth) //can only be damaged
                    {
                        bestOption = chooseBestAttack(bestOption, command);
                    }
                }
                return bestOption;
            }
        }

        private MGPumAttackCommand chooseBestKill(MGPumAttackCommand bestUntilNow, MGPumAttackCommand toCompare)
        {
            //kill the one making the most damage
            if (bestUntilNow == null)
            {
                bestUntilNow = toCompare;
            }
            else if (bestUntilNow.defender.currentPower < toCompare.defender.currentPower)
            {
                bestUntilNow = toCompare;
            }
            return bestUntilNow;

            //maybe insert here decision involving special skills
        }

        private MGPumAttackCommand chooseBestAttack(MGPumAttackCommand bestUntilNow, MGPumAttackCommand toCompare)
        {
            //choose attack over not attacking 
            if (bestUntilNow == null)
            {
                bestUntilNow = toCompare;
            }
            //attack the one with the most power first
            else if (bestUntilNow.defender.currentPower < toCompare.defender.currentPower)
            {
                bestUntilNow = toCompare;
            }
            //if same power, choose the one with less remaining health
            else if (bestUntilNow.defender.currentPower == toCompare.defender.currentPower)
            {
                if(toCompare.defender.currentHealth < bestUntilNow.defender.currentHealth)
                {
                    bestUntilNow = toCompare;
                }
                
                //maybe insert here decision involving special skills
            }
            return bestUntilNow;
        }
    }
}
