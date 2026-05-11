using UnityEngine;
using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Animations.API;

namespace Kruty1918.Moyva.Units.API
{
    public enum UnitRole
    {
        Worker = 0,
        Military = 1,
    }

    public enum UnitCombatType
    {
        Infantry = 0,
        Cavalry = 1,
        SiegeMachine = 2,
    }

    public enum AnimationType
    {
        Idle = 0,
        Move = 1,
        Attack = 2,
        TakeDamage = 3,
        Die = 4,
        Custom = 5,
    }

    /// <summary>
    /// Визначає анімацію, яка може бути пов'язана з юнітом.
    /// Підтримує AnimationClip, Animator параметри, або спрайт-лист.
    /// </summary>
    [Serializable]
    public class UnitAnimationClip
    {
        [Tooltip("Тип анімації: Idle, Move, Attack, TakeDamage, Die, або Custom")]
        public AnimationType Type = AnimationType.Idle;

        [Tooltip("Назва анімації для можливості пошуку/фільтрації")]
        public string Name = "New Animation";

        [Tooltip("AnimationClip для Animator")]
        public AnimationClip AnimationClip;

        [Tooltip("Параметр в Animator, який запускає цю анімацію (наприклад 'IsAttacking')")]
        public string AnimatorParameterName;

        [Tooltip("Список спрайтів для простої спрайт-анімації (альтернатива AnimationClip)")]
        public List<Sprite> SpriteFrames = new List<Sprite>();

        [Tooltip("FPS для спрайт-анімації")]
        [Min(1)] public int SpriteFPS = 10;

        [Tooltip("Повторювати анімацію після завершення")]
        public bool Loop = true;

        [Tooltip("Довжина анімації в секундах")]
        [Min(0.01f)] public float Duration = 1f;

        public bool HasClip => AnimationClip != null || (SpriteFrames != null && SpriteFrames.Count > 0);
    }

    [Serializable]
    public class UnitClassConfig
    {
        /// <summary>
        /// ВАЖЛИВО: У написані айді НЕ повино використовуватися нижнє підкреслення, окільки це є зарезервований символ для внутрішнього використання (наприклад, для позначення інстанцій юнітів). Рекомендується використовувати дефіси або camelCase. Наприклад: "warrior-01" або "Warrior01".
        /// </summary>
        [Tooltip("У написані айді НЕ повино використовуватися нижнє підкреслення, окільки це є зарезервований символ для внутрішнього використання (наприклад, для позначення інстанцій юнітів). Рекомендується використовувати дефіси або camelCase. Наприклад: \"warrior-01\" або \"Warrior01\".")]
        public string TypeId; // наприклад "warrior-01"

        [Tooltip("Класифікація юніта: Worker (економічні задачі) або Military (бойові задачі).")]
        public UnitRole Role = UnitRole.Worker;

        [Tooltip("Бойова класифікація юніта: піхота, кавалерія або облогова машина.")]
        public UnitCombatType CombatType = UnitCombatType.Infantry;

        public float BaseStamina;
        [Min(1)] public int VisionRange = 1;
        [Min(1)] public int HitPoints = 100;
        [Min(1)] public int BaseLevel = 1;

        [Tooltip("Ріжуча шкода: мечі, шаблі та інші удари лезом.")]
        [Min(0)] public int CuttingDamage;
        [Tooltip("Колюча шкода: списи, стріли та інші точкові пробивні удари.")]
        [Min(0)] public int PenetratingDamage;
        [Tooltip("Дробляча шкода: тарани, катапульти, булави та важкі удари.")]
        [Min(0)] public int CrushingDamage;

        [Tooltip("Захист від ріжучої шкоди.")]
        [Min(0)] public int CuttingDefense;
        [Tooltip("Захист від колючої шкоди.")]
        [Min(0)] public int PenetratingDefense;
        [Tooltip("Захист від дроблячої шкоди.")]
        [Min(0)] public int CrushingDefense;

        public GameObject Prefab;
        public Vector2 StaminaRandomRange = new Vector2(-5, 5); // +/- 5 випадкових одиниць до базової стаміни
        public PathAnimationSettings AnimationSettings = PathAnimationSettings.Default;

        [Tooltip("Спрайт юніта для швидкої редакції")]
        public Sprite CustomSprite;

        [Tooltip("Список анімацій, пов'язаних з юнітом")]
        public List<UnitAnimationClip> AnimationClips = new List<UnitAnimationClip>();

        /// <summary>
        /// Отримує анімацію за типом. Повертає першу знайдену анімацію цього типу.
        /// </summary>
        public UnitAnimationClip GetAnimation(AnimationType type)
        {
            if (AnimationClips == null)
                return null;

            foreach (var clip in AnimationClips)
            {
                if (clip != null && clip.Type == type)
                    return clip;
            }
            return null;
        }

        /// <summary>
        /// Отримує анімацію за назвою.
        /// </summary>
        public UnitAnimationClip GetAnimationByName(string name)
        {
            if (AnimationClips == null || string.IsNullOrWhiteSpace(name))
                return null;

            foreach (var clip in AnimationClips)
            {
                if (clip != null && clip.Name == name)
                    return clip;
            }
            return null;
        }
    }
}