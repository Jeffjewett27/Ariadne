using System.Collections.Generic;
using System.EnterpriseServices.Internal;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityStandardAssets.ImageEffects;

namespace Ariadne
{
    public enum ColorType
    {
        Transparent,
        Yellow,
        Red,
        Cyan,
        Green,
        Blue,
        Pink,
        DarkBlue,
        LightGray,
        Gray,
        Greenish,
        DarkGray,
        Purple,
        Orange,
        White,
        Yellowish,
        Whitish,
        BlueGray,
        Pinkish,
        Blueish,
        DarkYellow,
        LightBlue,
        Orangish,
        Purplish,
    }

    public static class ColorExtensions
    {
        public static Color GetColor(this ColorType color) => color switch
        {
            ColorType.Transparent => Color.clear,
            ColorType.Yellow => Color.yellow,
            ColorType.Red => new Color(0.8f, 0, 0),
            ColorType.Cyan => Color.cyan,
            ColorType.Green => Color.green,
            ColorType.Blue => new Color(0.5f, 0.5f, 1f),
            ColorType.Pink => new Color(1f, 0.75f, 0.8f),
            ColorType.DarkBlue => new Color(0.0f, 0.0f, 0.5f),
            ColorType.LightGray => new Color(0.8f, 0.8f, 0.8f),
            ColorType.Gray => new Color(0.5f, 0.5f, 0.5f),
            ColorType.Greenish => new Color(0.5f, 0.9f, 0.5f),
            ColorType.DarkGray => new Color(0.2f, 0.2f, 0.2f),
            ColorType.Purple => new Color(0.5f, 0.0f, 0.5f),
            ColorType.Orange => new Color(0.9f, 0.6f, 0.4f),
            ColorType.White => Color.white,
            ColorType.Yellowish => new Color(0.6f, 0.6f, 0.2f),
            ColorType.Whitish => new Color(0.9f, 0.9f, 0.9f),
            ColorType.BlueGray => new Color(0.6f, 0.6f, 0.8f),
            ColorType.Pinkish => new Color(0.9f, 0.7f, 0.7f),
            ColorType.Blueish => new Color(0.7f, 0.7f, 0.9f),
            ColorType.DarkYellow => new Color(0.4f, 0.4f, 0.0f),
            ColorType.LightBlue => new Color(0.2f, 0.3f, 0.8f),
            ColorType.Orangish => new Color(0.7f, 0.3f, 0.3f),
            ColorType.Purplish => new Color(0.8f, 0.3f, 0.8f),
            _ => Color.black
        };
    }
}
