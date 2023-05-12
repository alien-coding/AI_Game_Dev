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
            //moving
            foreach (MGPumUnit unit in state.getAllUnitsInZone(MGPumZoneType.Battlegrounds))
            {
                if (stateOracle.canMove(unit) && unit.currentSpeed >= 1 && unit.ownerID == this.playerID)
                {

                    Vector2Int position = unit.field.coords;
                    Debug.Log(state.getField(position));

                    Vector2Int direction = position + Vector2Int.up;
                    Debug.Log(state.getField(position));

                    if (state.getField(direction).isEmpty())
                    {
                        MGPumMoveChainMatcher chainMatcher = unit.getMoveMatcher();
                        MGPumFieldChain chain = new(this.playerID, chainMatcher);
                        chain.add(state.getField(position));
                        if (chain.canAdd(state.getField(direction)))
                        {
                            chain.add(state.getField(direction));
                        }
                        MGPumMoveCommand move = new(this.playerID, chain, unit);

                        return move;
                    }
                }
                if(unit.ownerID == this.playerID && stateOracle.canAttack(unit) && unit.currentRange >= 1)
                {
                    Vector2Int position = unit.field.coords;
                    Debug.Log(state.getField(position));

                    Vector2Int direction = position + Vector2Int.up;
                    Debug.Log(state.getField(position));

                    MGPumField attackHere = state.getField(direction);
                    if (!attackHere.isEmpty() && state.getUnitForField(attackHere).ownerID != this.playerID) 
                    {
                        MGPumAttackChainMatcher attackMatcher = unit.getAttackMatcher();
                        MGPumFieldChain chain = new(this.playerID, attackMatcher);
                        chain.add(state.getField(position));
                        if (chain.canAdd(state.getField(attackHere)))
                        {
                            chain.add(state.getField(attackHere));
                        }
                        
                        MGPumAttackCommand attack = new(this.playerID, chain, unit);

                        return attack;
                    }
                }
            }
            return new MGPumEndTurnCommand(this.playerID);
        }
    }
}

