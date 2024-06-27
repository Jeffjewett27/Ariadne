using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariadne.Logging
{
    using UnityEngine;

    internal class CameraLoggerHelper : MonoBehaviour
    {
        public HitboxLogger hitboxLogger;

        void OnPostRender()
        {
            if (hitboxLogger != null)
            {
                hitboxLogger.LogUpdate();
            }
        }
    }
}
