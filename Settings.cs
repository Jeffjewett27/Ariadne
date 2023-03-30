using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariadne
{
    public enum ShowHitbox
    {
        None,
        Show,
        Verbose
    }

    public class GlobalSettings
    {
        public ShowHitbox ShowHitBoxes = ShowHitbox.Verbose;

        public bool TrackHitboxes = true;

        public bool DebugAB = true;
    }
}
