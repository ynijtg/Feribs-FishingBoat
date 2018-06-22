using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeribsFisherboat
{
    class Offsets
    {
        // 7.1.5
        public static int Spell_C_CastSpell = 0x2A83BE;
        public static int PlayerBasePtr = 0xE39950;
        public static int ObjMgrPtr = 0xD9D25C;
        public static int InteractObjByGUID = 0x53102;
        public static int LocalPlayerName = 0xF904B0;
        public static int LocalPlayerGuid = 0xF904A0;

        public static int CreateRemoteThreadPatchOffset = 0x00000000;
        public static int CodeCaveDataSize = 0xBD;
    }

}
