using Ariadne.HitboxUtils;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel;
using Newtonsoft.Json.Converters;

namespace Ariadne.Logging
{
    public record SceneData
    {
        // The name of the Hollow Knight scene (e.g. Crossroads_08)
        public string sceneName;
        // The ISO timestamp when the room was loaded
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime entryTime;
        // The ISO timestamp when the room is exited
        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime exitTime;
        // The number of milliseconds between records (default is 20ms at 50fps)
        public float intervalMs;
        // Records of the camera movement
        public List<ColliderRecord> camera;
        // Records of entities and terrain
        public List<HitboxCategoryData> categories;
        // Give precise millisecond offsets to correct deviations
        public List<short> deviationsMs;
        // Note skipped frames
        public List<int> skippedFrames;
    }

    public record HitboxCategoryData
    {
        //[JsonProperty(ItemConverterType = typeof(StringEnumConverter), nam)]
        public HitboxType hitboxType;
        public List<HitboxData> entities;
    }

    public class HitboxData
    {
        [JsonProperty(PropertyName = "type")]
        [JsonIgnore]
        public HitboxType hitboxType;
        public string name;
        public List<ColliderRecord> records;

        [JsonIgnore]
        public ColliderRecord current;
        public List<List<float>> segmentationBounds;
    }

    public record ColliderRecord
    {
        [JsonProperty(PropertyName ="t")]
        [DefaultValue(-1)]
        public int timestamp;

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

        private bool FloatEquals(float x, float y)
        {
            return Math.Abs(x - y) < 0.001f;
        }

        public ColliderRecord Diff(ColliderRecord other)
        {
            return new ColliderRecord()
            {
                timestamp = other.timestamp,
                posX = FloatEquals(posX, other.posX) ? float.NaN : other.posX,
                posY = FloatEquals(posY, other.posY) ? float.NaN : other.posY,
                height = FloatEquals(height, other.height) ? float.NaN : other.height,
                width = FloatEquals(width, other.width) ? float.NaN : other.width,
                isActive = isActive == other.isActive ? null : other.isActive,
                fsmState = fsmState == other.fsmState ? null : other.fsmState
            };
        }

        public bool IsEmpty()
        {
            return float.IsNaN(posX)
                && float.IsNaN(posY)
                && float.IsNaN(height)
                && float.IsNaN(width)
                && isActive == null
                && fsmState == null;
        }
    }
}
