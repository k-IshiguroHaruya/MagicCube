
using UnityEngine;

namespace MagicCube
{
    public static class Vector3Ext
    {
        public static Vector3 Round( this Vector3 self )
        {
            return new Vector3( Mathf.Round(self.x), Mathf.Round(self.y), Mathf.Round(self.z) );
        }
    }
}
