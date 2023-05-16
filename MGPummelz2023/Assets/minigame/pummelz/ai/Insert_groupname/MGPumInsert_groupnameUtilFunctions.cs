using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mg.pummelz.Insert_groupname
{
    public class MGPumInsert_groupnameUtilFunctions : MonoBehaviour
    {
        private MGPumGameState state;
        private int playerID;

        public MGPumInsert_groupnameUtilFunctions(MGPumGameState state, int playerID)
        {
            this.state = state;
            this.playerID = playerID;
        }

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

        //choose best option where to walk
        //trying to reach an enemy, if several options possible, reach most powerfull
        //else try to approach the closest enemy
        public MGPumMoveCommand chooseWalkingWay(List<MGPumMoveCommand> allPosibilites)
        {
            MGPumMoveCommand bestOption = null;
            MGPumUnit mostPowerfullReachableUnit = null;
            int remainingDistanceToClosestEnemy = int.MaxValue;
            foreach (MGPumMoveCommand move in allPosibilites)
            {
                List<MGPumUnit> reachableUnits = getReachableEnemy(move);
                //is there a unit reachable?
                if (reachableUnits != null && reachableUnits.Count > 0)
                {
                    MGPumUnit mostPowerfullReachableUnitOfThisMove = getUnitWithMostPower(reachableUnits);
                    if (bestOption == null)
                    {
                        bestOption = move;
                        mostPowerfullReachableUnit = mostPowerfullReachableUnitOfThisMove;
                    }
                    else
                    {
                        if (mostPowerfullReachableUnit == null || mostPowerfullReachableUnitOfThisMove.currentPower > mostPowerfullReachableUnit.currentPower)
                        {
                            bestOption = move;
                            mostPowerfullReachableUnit = mostPowerfullReachableUnitOfThisMove; ;
                        }
                    }
                }
                //walk towards the closest enemy 
                else
                {
                    int distanceToClosestEnemy = getDistanceToClosestEnemy(move);
                    if (bestOption == null)
                    {
                        bestOption = move;
                        remainingDistanceToClosestEnemy = distanceToClosestEnemy;
                    }
                    else
                    {
                        if(distanceToClosestEnemy < remainingDistanceToClosestEnemy)
                        {
                            bestOption = move;
                            remainingDistanceToClosestEnemy = distanceToClosestEnemy;
                        }
                    }
                }
            }
            return bestOption;
        }

        //Get all reachable enemies 
        private List<MGPumUnit> getReachableEnemy(MGPumMoveCommand move)
        {
            List<MGPumUnit> reachableUnits = new();
            Vector2Int newCoords = new(move.chain.getLast().x, move.chain.getLast().y);
            int range = move.mover.currentRange;
            for (int x = newCoords.x - range; x <= newCoords.x + range; x++)
            {
                for (int y = newCoords.y - range; y <= newCoords.y - range; y++)
                {
                    if(x >= 0 && y >= 0 && x < state.fields.dimSize && y < state.fields.dimSize)
                    {
                        MGPumField field = new MGPumField(x, y);
                        MGPumUnit foundUnit = state.getUnitForField(field);
                        if (foundUnit != null && foundUnit.ownerID != this.playerID)
                        {
                            reachableUnits.Add(foundUnit);
                        }
                    }
                }
            }
            return reachableUnits;
        }

        //search for closest enemy by increasing the range 1 by one
        //aborting if one is found
        //calcing distance by taking the bigger coords delta (since cross walking is also distance one)
        private int getDistanceToClosestEnemy(MGPumMoveCommand move)
        {
            Vector2Int newCoords = new (move.chain.getLast().x, move.chain.getLast().y);
            for(int range = 1; range < state.fields.dimSize; range++)
            {
                for (int x = newCoords.x - range; x <= newCoords.x + range; x++)
                {
                    for (int y = newCoords.y - range; y <= newCoords.y + range; y++)
                    {
                        if(x >= 0 && y >= 0 && x < state.fields.dimSize && y < state.fields.dimSize)
                        {                            
                            MGPumUnit foundUnit = state.getUnitForField(new MGPumField(x, y));
                            if (foundUnit != null && foundUnit.ownerID != this.playerID)
                            {
                                Vector2Int enemyPosition = foundUnit.field.coords;
                                int xDistance = Mathf.Abs(newCoords.x - enemyPosition.x);
                                int yDistance = Mathf.Abs(newCoords.y - enemyPosition.y);
                                if (xDistance > yDistance)
                                {
                                    return xDistance;
                                }
                                else
                                {
                                    return yDistance;
                                }
                            }
                        }
                    }
                }
            }
            return int.MaxValue;
        }

        private MGPumUnit getUnitWithMostPower(List<MGPumUnit> units)
        {
            MGPumUnit mostPoweredEnemy = null;
            foreach (MGPumUnit unit in units)
            {
                if(mostPoweredEnemy == null)
                {
                    mostPoweredEnemy = unit;
                }
                else
                {
                    if(unit.currentPower > mostPoweredEnemy.currentPower)
                    {
                        mostPoweredEnemy = unit;
                    }
                }
            }
            return mostPoweredEnemy;
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
