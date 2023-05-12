using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mg.pummelz.Insert_groupname
{
    public class MGPumInsert_groupnameAIPlayerController : MGPumStudentAIPlayerController
    {
        public const string type = "Insert_groupname";

        public MGPumInsert_groupnameAIPlayerController(int playerID) : base(playerID)
        {
            
        }

        
        protected override int[] getTeamMartikels()
        {
            return new int[] { 1269263, 5727525 };
        }


        internal override MGPumCommand calculateCommand()
        {
            foreach (MGPumUnit unit in state.getAllUnitsInZone(MGPumZoneType.Battlegrounds))
            {
                if(unit.ownerID == this.playerID)
                {
                    List<MGPumMoveCommand> allMoves = this.getAllMovesForUnit(unit);
                    List<MGPumAttackCommand> allAttacks = this.getAllAttacksForUnit(unit);
                    int attackOrMove = this.rng.Next(2); // number is < smaller than maxValue

                    if (attackOrMove == 0 && allMoves.Count > 0)
                    {
                        int moveIndex = this.rng.Next(allMoves.Count);   //number is < than maxValue, therefore do not sutract 1
                        return allMoves[moveIndex];
                    }
                    else if (attackOrMove == 1 && allAttacks.Count > 0)
                    {
                        int attackIndex = this.rng.Next(allAttacks.Count);   //number is < than maxValue, therefore do not sutract 1
                        return allAttacks[attackIndex];
                    }
                }
            }
            return new MGPumEndTurnCommand(this.playerID);
        }


        private List<MGPumMoveCommand> getAllMovesForUnit(MGPumUnit unit)
        {
            List<MGPumMoveCommand> allMoves = new();
            if (stateOracle.canMove(unit) && unit.currentSpeed >= 1)
            {
                Vector2Int position = unit.field.coords;

                foreach (Vector2Int direction in this.getDirections())
                {
                    Vector2Int movingWay = position + direction;
                    MGPumField destination = state.getField(movingWay);

                    if (destination != null && destination.isEmpty())
                    {
                        MGPumMoveChainMatcher chainMatcher = unit.getMoveMatcher();
                        MGPumFieldChain chain = new(playerID, chainMatcher);
                        chain.add(state.getField(position));
                        if (chain.canAdd(destination))
                        {
                            chain.add(destination);
                        }
                        if (chain.isValidChain())
                        {
                            MGPumMoveCommand move = new(this.playerID, chain, unit);
                            allMoves.Add(move);
                        }
                        
                    }
                }
            }
            return allMoves;
        }

        private List<MGPumAttackCommand> getAllAttacksForUnit(MGPumUnit unit)
        {
            List<MGPumAttackCommand> allAttacks = new();

            if (stateOracle.canAttack(unit) && unit.currentRange >= 1)
            {
                Vector2Int position = unit.field.coords;

                foreach (Vector2Int direction in this.getDirections())
                {
                    Vector2Int attackingWay = position + direction;
                    MGPumField attackHere = state.getField(attackingWay);

                    if (attackHere != null && !attackHere.isEmpty() && state.getUnitForField(attackHere).ownerID != this.playerID)
                    {
                        MGPumAttackChainMatcher attackMatcher = unit.getAttackMatcher();
                        MGPumFieldChain chain = new(this.playerID, attackMatcher);
                        chain.add(state.getField(position));
                        if (chain.canAdd(attackHere))
                        {
                            chain.add(attackHere);
                        }
                        if (chain.isValidChain())
                        {
                            MGPumAttackCommand attack = new(this.playerID, chain, unit);
                            allAttacks.Add(attack);
                        }
                    }
                }
            }
            return allAttacks;
        }
    }
}

