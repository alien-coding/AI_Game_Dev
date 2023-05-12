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
                    List<MGPumCommand> allMoves = getAllPossibleMoves(unit);
                    //Debug.Log(allMoves);
                    if(allMoves != null && allMoves.Count > 0)
                    {
                        return allMoves[rng.Next(allMoves.Count)];
                    }
                }
            }
            return new MGPumEndTurnCommand(this.playerID);
        }

        private List<MGPumCommand> getAllPossibleMoves(MGPumUnit unit)
        {
            List<MGPumCommand> allPossibleMoves = new();
            allPossibleMoves.AddRange(getAllMovesForUnit(unit));
            allPossibleMoves.AddRange(getAllAttacksForUnit(unit));

            if (allPossibleMoves.Count > 0)
            {
                return allPossibleMoves;
            }
            else
            {
                return null;
            }
        }


        private List<MGPumMoveCommand> getAllMovesForUnit(MGPumUnit unit)
        {
            List<MGPumMoveCommand> allMoves = new();
            if (stateOracle.canMove(unit) && unit.currentSpeed >= 1)
            {
                Vector2Int position = unit.field.coords;

                List<Vector2Int> allVectors = getDirectionsWithDepth(unit.currentSpeed);
                foreach (Vector2Int vectorToDestination in allVectors)
                {
                    Vector2Int movingWay = position + vectorToDestination;
                    MGPumField destination = state.getField(movingWay);

                    if (destination != null && destination.isEmpty())
                    {
                        MGPumMoveChainMatcher chainMatcher = unit.getMoveMatcher();
                        MGPumFieldChain chain = new(playerID, chainMatcher);
                        chain.add(state.getField(position));
                       
                        List<Vector2Int> splittedSteps = splitVectorInSteps(position, movingWay);
                        foreach (Vector2Int stepVector in splittedSteps)
                        {
                            //Debug.Log(stepVector + " " + unit.currentSpeed);
                            MGPumField fieldOfVector = state.getField(stepVector);
                            if (chain.canAdd(fieldOfVector))
                            {
                                chain.add(fieldOfVector);
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (chain.isValidChain() && chain.getLast() == destination)
                        {
                            MGPumMoveCommand move = new(this.playerID, chain, unit);
                            allMoves.Add(move);
                        }
                        else
                        {
                            Debug.Log("Found unvalid move");
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

                foreach (Vector2Int vectorToDestination in this.getDirectionsWithDepth(unit.currentRange))
                {
                    Vector2Int attackingWay = position + vectorToDestination;
                    MGPumField attackHere = state.getField(attackingWay);

                    if (attackHere != null && !attackHere.isEmpty() && state.getUnitForField(attackHere).ownerID != this.playerID)
                    {
                        MGPumAttackChainMatcher attackMatcher = unit.getAttackMatcher();
                        MGPumFieldChain chain = new(this.playerID, attackMatcher);
                        chain.add(state.getField(position));

                        List<Vector2Int> splittedSteps = splitVectorInSteps(position, attackingWay);
                        foreach (Vector2Int stepVector in splittedSteps)
                        {
                            MGPumField fieldOfVector = state.getField(stepVector);
                            if (chain.canAdd(fieldOfVector))
                            {
                                chain.add(fieldOfVector);
                            }
                            else
                            {
                                break;
                            }
                        }

                        
                        if (chain.isValidChain() && chain.getLast() == attackHere)
                        {
                            MGPumAttackCommand attack = new(this.playerID, chain, unit);
                            allAttacks.Add(attack);
                        }
                        else
                        {
                            Debug.Log("Found unvalid attack");
                        }
                    }
                }
            }
            return allAttacks;
        }

        //returns list of Vectors for surounding, with given depth.
        //Also contains zero vector
        private List<Vector2Int> getDirectionsWithDepth(int depth)
        {
            List<Vector2Int> directions = new List<Vector2Int>();
            for (int i = -depth; i <= depth; i++)
            {
                for (int j = -depth; j <= depth; j++)
                {
                    directions.Add(new Vector2Int(i, j));
                }
            }
            
            return directions;
        }

        private List<Vector2Int> splitVectorInSteps(Vector2Int position, Vector2Int destination)
        {
            List<Vector2Int> directions = new List<Vector2Int>();
            while(position != destination)
            {
                if(position.x < destination.x && position.y < destination.y) //top right
                {
                    position += new Vector2Int(1, 1);
                }
                else if (position.x < destination.x && position.y == destination.y)  //right 
                {
                    position += new Vector2Int(1, 0);
                }
                else if (position.x < destination.x && position.y > destination.y)  //bottom right 
                {
                    position += new Vector2Int(1, -1);
                }
                else if (position.x == destination.x && position.y > destination.y)  //bottom 
                {
                    position += new Vector2Int(0, -1);
                }
                else if (position.x > destination.x && position.y > destination.y)  //bottom left
                {
                    position += new Vector2Int(-1, -1);
                }
                else if (position.x > destination.x && position.y == destination.y)  //left 
                {
                    position += new Vector2Int(-1, 0);
                }
                else if (position.x > destination.x && position.y < destination.y)  //top left 
                {
                    position += new Vector2Int(-1, 1);
                }
                else if (position.x == destination.x && position.y < destination.y)  //top 
                {
                    position += new Vector2Int(0, 1);
                }
                directions.Add(position);
            }
            return directions;
        }
    }
}

