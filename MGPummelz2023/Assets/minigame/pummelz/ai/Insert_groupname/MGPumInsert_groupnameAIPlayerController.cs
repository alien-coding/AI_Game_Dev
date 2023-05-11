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
            return new MGPumEndTurnCommand(this.playerID);
        }
    }
}

