using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mg.pummelz.Insert_groupname
{
    public class MGPumInsert_groupnameAIPlayerController : MGPumStudentAIPlayerController
    {
        public const string type = "Insert_groupname";
        private MGPumInsert_groupnameUtilFunctions util;

        public MGPumInsert_groupnameAIPlayerController(int playerID) : base(playerID)
        {

        }

        
        protected override int[] getTeamMartikels()
        {
            return new int[] { 1269263, 5727525 };
        }


        internal override MGPumCommand calculateCommand()
        {
            //Kill (and flee)
            //approach and kill
            //damage enemy and walk away
            //approach and damage
            //apporach
            //return this.randomAI();
            return this.tacticAI();
        }


        private MGPumCommand tacticAI()
        {
            this.util = new(this.state, playerID);
            foreach (MGPumUnit unit in state.getAllUnitsInZone(MGPumZoneType.Battlegrounds))
            {
                if(unit.ownerID == this.playerID)
                {
                    List<MGPumMoveCommand> allMovingPossibilities = this.getAllMovesForUnit(unit);
                    List<MGPumAttackCommand> allAttackPossibilities = this.getAllAttacksForUnit(unit);
                    MGPumAttackCommand target = util.chooseTarget(allAttackPossibilities);
                    if(target != null)
                    {
                        return target;
                    }
                    else
                    {
                        if(allMovingPossibilities != null && allMovingPossibilities.Count > 0)
                        {
                            return util.chooseWalkingWay(allMovingPossibilities);
                        }
                    }
                }
            }
            return new MGPumEndTurnCommand(this.playerID);
        }

        //this is the randomAI from Task 2
        //calulates all possible moves, pick a random one
        private MGPumCommand randomAI()
        {
            foreach (MGPumUnit unit in state.getAllUnitsInZone(MGPumZoneType.Battlegrounds))
            {
                if (unit.ownerID == this.playerID)
                {
                    List<MGPumCommand> allMoves = getAllPossibleMoves(unit);
                    //Debug.Log(allMoves);
                    if (allMoves != null && allMoves.Count > 0)
                    {
                        return allMoves[rng.Next(allMoves.Count)];
                    }
                }
            }
            return new MGPumEndTurnCommand(this.playerID);
        }

        //Calculates all Moves that are possible
        //move here beeing either shooting or walking
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

                List<Vector2Int> allVectors = getDirectionsWithDepth(unit.currentSpeed);    //get all reachable fields unit can move to
                foreach (Vector2Int vector in allVectors)
                {
                    Vector2Int vectorToDestination = position + vector;
                    MGPumField destination = state.getField(vectorToDestination);

                    if (destination != null && destination.isEmpty())   //found field is free?
                    {
                        MGPumMoveChainMatcher chainMatcher = unit.getMoveMatcher();
                        MGPumFieldChain chain = new(playerID, chainMatcher);
                        chain.add(state.getField(position));
                       
                        List<Vector2Int> splittedSteps = splitVectorInSteps(position, vectorToDestination); //split vector to destination in single steps for chain
                        foreach (Vector2Int stepVector in splittedSteps)
                        {
                            MGPumField fieldOfVector = state.getField(stepVector);  //add the steps as fields to chain
                            if (chain.canAdd(fieldOfVector))
                            {
                                chain.add(fieldOfVector);
                            }
                            else
                            {
                                break;  //if one can't be added, the whole move is not possible, abort here (later checks for last one which is not set)
                            }
                        }
                        if (!chain.isValidChain())
                        {
                            List<List<Vector2Int>> allPaths = getAllPaths(position, vectorToDestination, unit.currentSpeed);
                            foreach (List<Vector2Int> possiblePath in allPaths)
                            {
                                chain = new(playerID, chainMatcher);
                                foreach (Vector2Int stepVector in possiblePath)
                                {
                                    if (stepVector.x >= 0 && stepVector.y >= 0 && stepVector.x < state.fields.dimSize && stepVector.y < state.fields.dimSize)
                                    {
                                        MGPumField fieldOfVector = state.getField(stepVector);  //add the steps as fields to chain
                                        if (chain.canAdd(fieldOfVector))
                                        {
                                            chain.add(fieldOfVector);
                                        }
                                        else
                                        {
                                            break;  //if one can't be added, the whole move is not possible, abort here (later checks for last one which is not set)
                                        }
                                    }
                                }
                                if (chain.isValidChain())
                                {
                                    break;
                                }
                            }
                        }

                        if (chain.isValidChain() && chain.getLast() == destination)     //only if chain is valid and every step could be added (aborts if one can't be added)
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

                foreach (Vector2Int vector in getDirectionsWithDepth(unit.currentRange))   //get all reachable fields unit can shoot
                {
                    Vector2Int vectorToTarget = position + vector;
                    MGPumField targetField = state.getField(vectorToTarget);

                    if (targetField != null && !targetField.isEmpty() && state.getUnitForField(targetField).ownerID != this.playerID)   //found field contains enemy?
                    {
                        MGPumAttackChainMatcher attackMatcher = unit.getAttackMatcher();
                        MGPumFieldChain chain = new(playerID, attackMatcher);
                        chain.add(state.getField(position));    //add starting point

                        List<Vector2Int> splittedSteps = splitVectorInSteps(position, vectorToTarget);  //split vector to target in single steps for chain
                        foreach (Vector2Int stepVector in splittedSteps)
                        {
                            MGPumField fieldOfVector = state.getField(stepVector);  //add the steps as fields to chain
                            if (chain.canAdd(fieldOfVector))
                            {
                                chain.add(fieldOfVector);
                            }
                            else
                            {
                                break;  //if one can't be added, the whole move is not possible, abort here (later checks for last one which is not set)
                            }
                        }
                        if (!chain.isValidChain())
                        {
                            List<List<Vector2Int>> allPaths = getAllPaths(position, vectorToTarget, unit.currentRange);
                            foreach (List<Vector2Int> possiblePath in allPaths)
                            {
                                chain = new(playerID, attackMatcher);
                                foreach (Vector2Int stepVector in possiblePath)
                                {
                                    if(stepVector.x >= 0 && stepVector.y >= 0 && stepVector.x < state.fields.dimSize && stepVector.y < state.fields.dimSize)
                                    {
                                        MGPumField fieldOfVector = state.getField(stepVector);  //add the steps as fields to chain
                                        if (chain.canAdd(fieldOfVector))
                                        {
                                            chain.add(fieldOfVector);
                                        }
                                        else
                                        {
                                            break;  //if one can't be added, the whole move is not possible, abort here (later checks for last one which is not set)
                                        }
                                    }
                                }
                                if(chain.isValidChain())
                                {
                                    break;
                                }
                            }
                        }

                        if (chain.isValidChain() && chain.getLast() == targetField)     //only if chain is valid and every step could be added (aborts if one can't be added)
                        {
                            MGPumAttackCommand attack = new(this.playerID, chain, unit);
                            allAttacks.Add(attack);
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

        //splits vector into single vector steps for chain adding
        //therefore starts at given position and only takes single steps toward destination
        //every new position is added to directions, therefore directions contains the absolute vectors (which are needed for chain) in the right order,
        //beginning at the first step after position and last step beeing the destination
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

        private List<List<Vector2Int>> getAllPaths(Vector2Int position, Vector2Int destination, int maxSteps)
        {

            List<List<Vector2Int>> allPaths = recursion(position, destination, maxSteps);
            List<List<Vector2Int>> stepsToGoal = new();
            foreach (List<Vector2Int> possibleSteps in allPaths)
            {
                if(possibleSteps[possibleSteps.Count-1] == destination)
                {
                    stepsToGoal.Add(possibleSteps);
                }
            }
            return stepsToGoal;
        }

        private List<List<Vector2Int>> recursion(Vector2Int position, Vector2Int destination, int maxSteps)
        {
            List<List<Vector2Int>> stepsToGoal = new();
            if (maxSteps > 0)
            {
                List<Vector2Int> moves = getAllOneStepPositions(position);
                foreach (Vector2Int move in moves)
                {
                    List<List<Vector2Int>> result = recursion(move, destination, maxSteps - 1);
                    foreach (List<Vector2Int> list in result)
                    {
                        List<Vector2Int> listWithSelf = list;
                        listWithSelf.Insert(0, position);   //adding item to beginning because it's the previous step
                        stepsToGoal.Add(listWithSelf);
                    }
                }
                return stepsToGoal;
            }
            else
            {
                List<Vector2Int> selfAtTheEnd = new();
                selfAtTheEnd.Add(position);
                List<List<Vector2Int>> toReturn = new();
                toReturn.Add(selfAtTheEnd);
                return toReturn;
            }
        }

        private List<Vector2Int> getAllOneStepPositions(Vector2Int position)
        {
            List<Vector2Int> steps = new List<Vector2Int>();
            steps.Add(position + Vector2Int.left);
            steps.Add(position + Vector2Int.right);
            steps.Add(position + Vector2Int.up);
            steps.Add(position + Vector2Int.down);
            steps.Add(position + Vector2Int.left + Vector2Int.up);
            steps.Add(position + Vector2Int.left + Vector2Int.down);
            steps.Add(position + Vector2Int.right + Vector2Int.up);
            steps.Add(position + Vector2Int.right + Vector2Int.down);
            return steps;
        }
    }
}

