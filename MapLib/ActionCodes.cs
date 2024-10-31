using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapLib
{
    public static class ActionCodes
    {
        //Sector "Actions"
        public static readonly int Light_Phased = 21;
        public static readonly int LightSequenceStart = 22;
        public static readonly int LightSequenceSpecial1 = 23;
        public static readonly int LightSequenceSpecial2 = 24;

        //For stairs that can be built up or torn down
        public static readonly int Stairs_Special1 = 26;
        public static readonly int Stairs_Special2 = 27;

        //Wind forces - mostly effect projectiles
        public static readonly int Wind_East_Weak = 40;
        public static readonly int Wind_East_Medium = 41;
        public static readonly int Wind_East_Strong = 42;
        public static readonly int Wind_North_Weak = 43;
        public static readonly int Wind_North_Medium = 44;
        public static readonly int Wind_North_Strong = 45;
        public static readonly int Wind_South_Weak = 46;
        public static readonly int Wind_South_Medium = 47;
        public static readonly int Wind_South_Strong = 48;
        public static readonly int Wind_West_Weak = 49;
        public static readonly int Wind_West_Medium = 50;
        public static readonly int Wind_West_Strong = 51;

        public static readonly int dLight_Flicker = 65;
        public static readonly int dLight_StrobeFast = 66;
        public static readonly int dLight_StrobeSlow = 67;
        public static readonly int dLight_Strobe_Hurt = 68;
        public static readonly int dDamage_Hellslime = 69;
        public static readonly int dDamage_Nukage = 71;
        public static readonly int dLight_Glow = 72;
        public static readonly int dDamage_End = 75;
        public static readonly int dLight_StrobeSlowSync = 76;
        public static readonly int dLight_StrobeFastSync = 77;
        public static readonly int dSector_DoorRaiseIn5Mins = 78;

        public static readonly int Sector_LowFriction = 79;

        //Damage sources
        public static readonly int dDamage_SuperHellslime = 80;
        public static readonly int dLight_FireFlicker = 81;
        public static readonly int dDamage_LavaWimpy = 82;
        public static readonly int dDamage_LavaHefty = 83;
        public static readonly int dScroll_EastLavaDamage = 84; //Same as Scroll_East_Slow
        public static readonly int hDamage_Sludge = 85;

        //Sector will use Fog, without map settings / ceiling
        public static readonly int Sector_Outside = 87;

        public static readonly int sDamage_Hellslime = 105;
        public static readonly int Damage_InstantDeath = 115;
        public static readonly int sDamage_SuperHellslime = 116;
        public static readonly int Scroll_StrifeCurrent = 118;

        //Sloping floors - action belongs to lines
        public static readonly int PLANE_ALIGN = 181;

        public static readonly int Sector_Hidden = 195;
        public static readonly int Sector_Heal = 196;

        //Hexen lightning and sky
        public static readonly int Light_OutdoorLightning = 197;
        public static readonly int Light_IndoorLightning1 = 198;
        public static readonly int Light_IndoorLightning2 = 199;
        public static readonly int Sky2 = 200;

        //Scroll texture and things
        public static readonly int Scroll_North_Slow = 201;
        public static readonly int Scroll_North_Medium = 202;
        public static readonly int Scroll_North_Fast = 203;
        public static readonly int Scroll_East_Slow = 204;
        public static readonly int Scroll_East_Medium = 205;
        public static readonly int Scroll_East_Fast = 206;
        public static readonly int Scroll_South_Slow = 207;
        public static readonly int Scroll_South_Medium = 208;
        public static readonly int Scroll_South_Fast = 209;
        public static readonly int Scroll_West_Slow = 210;
        public static readonly int Scroll_West_Medium = 211;
        public static readonly int Scroll_West_Fast = 212;

        public static readonly int Scroll_NorthWest_Slow = 213;
        public static readonly int Scroll_NorthWest_Medium = 214;
        public static readonly int Scroll_NorthWest_Fast = 215;
        public static readonly int Scroll_NorthEast_Slow = 216;
        public static readonly int Scroll_NorthEast_Medium = 217;
        public static readonly int Scroll_NorthEast_Fast = 218;
        public static readonly int Scroll_SouthEast_Slow = 219;
        public static readonly int Scroll_SouthEast_Medium = 220;
        public static readonly int Scroll_SouthEast_Fast = 221;
        public static readonly int Scroll_SouthWest_Slow = 222;
        public static readonly int Scroll_SouthWest_Medium = 223;
        public static readonly int Scroll_SouthWest_Fast = 224;

        //Scroll floor texture and carry content
        public static readonly int Carry_East5 = 225;
        public static readonly int Carry_East10 = 226;
        public static readonly int Carry_East25 = 227;
        public static readonly int Carry_East30 = 228;
        public static readonly int Carry_East35 = 229;
        public static readonly int Carry_North5 = 230;
        public static readonly int Carry_North10 = 231;
        public static readonly int Carry_North25 = 232;
        public static readonly int Carry_North30 = 233;
        public static readonly int Carry_North35 = 234;
        public static readonly int Carry_South5 = 235;
        public static readonly int Carry_South10 = 236;
        public static readonly int Carry_South25 = 237;
        public static readonly int Carry_South30 = 238;
        public static readonly int Carry_South35 = 239;
        public static readonly int Carry_West5 = 240;
        public static readonly int Carry_West10 = 241;
        public static readonly int Carry_West25 = 242;
        public static readonly int Carry_West30 = 243;
        public static readonly int Carry_West35 = 244;

        static readonly int[] LineActions = {
            PLANE_ALIGN
        };

        public static bool isSectorAction(int action)
        {
            return !isLineAction(action);
        }

        public static bool isLineAction(int action)
        {
            return LineActions.Contains(action);
        }
    }
}
