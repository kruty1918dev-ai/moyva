using System;
using UnityEngine;

namespace Kruty1918.Moyva.Clouds.API
{
    [Serializable]
    public sealed class CloudSpriteVariant
    {
        [Tooltip("Спрайт хмаринки, який може бути обраний для спавну.")]
        public Sprite Sprite;

        [Tooltip("Вага вибору цієї хмаринки. 0 означає, що варіант не використовується.")]
        [Min(0f)] public float Chance = 1f;
    }
}