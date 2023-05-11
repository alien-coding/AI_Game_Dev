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
                if(stateOracle.canMove(unit) && unit.currentSpeed >= 1)
                {
                    
                    Vector2Int position = unit.field.coords;

                    Debug.Log(state.getField(position));
                    if (state.getField(Vector2Int.up).isEmpty())
                    {

                        Debug.Log("hallo 1234");
                    }
                }
            }
            return new MGPumEndTurnCommand(this.playerID);
        }
    }
}

