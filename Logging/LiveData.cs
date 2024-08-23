using Ariadne.HitboxUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Ariadne.Logging
{
    public record LiveData
    {
        public string sceneName;

        // The current real time in seconds since game startup
        public float currentTime;

        public string inventoryState;

        public ColliderRecord camera;

        public PlayerSnapshot playerSnapshot;

        public Dictionary<int, EntitySnapshot> worldObjects;

        public List<List<List<float>>> terrainSegmentation;

        public List<List<List<float>>> staticHazardSegmentation;

    }

    public record EntitySnapshot
    {
        public string name;

        [JsonProperty(PropertyName = "type")]
        public HitboxType hitboxType;

        [JsonProperty(PropertyName = "x")]
        [DefaultValue(float.NaN)]
        public float posX;

        [JsonProperty(PropertyName = "y")]
        [DefaultValue(float.NaN)]
        public float posY;

        [JsonProperty(PropertyName = "h")]
        [DefaultValue(float.NaN)]
        public float height;

        [JsonProperty(PropertyName = "w")]
        [DefaultValue(float.NaN)]
        public float width;

        [JsonProperty(PropertyName = "a")]
        public bool? isActive;

        [JsonProperty(PropertyName = "f")]
        public string fsmState;
    }

    public record PlayerSnapshot
    {
        public int maxHealth;

        public int health;

        public int healthBlue;

        public int geo;

        public int profileId;

        public bool dash;

        public bool claw;

        public bool wings;

        public bool showdowdash;

        public bool cdash;

        public bool isma;
    }
}
