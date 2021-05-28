using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-04-28 PM 7:11:21
// 작성자 : Rito

namespace Rito.RadialMenu_v3
{
    public enum EasingType
    {
        None,
        InExpo,
        OutExpo,
        OutBack,
        InBounce,
        OutBounce,
        InOutBounce,
        OutElastic,
    }

    public static class Easing
    {
        public static float Get(float x, EasingType type)
        {
            switch (type)
            {
                default:
                case EasingType.None: return x;
                case EasingType.InExpo: return EaseInExpo(x);
                case EasingType.OutExpo: return EaseOutExpo(x);
                case EasingType.OutBack: return EaseOutBack(x);
                case EasingType.InBounce: return EaseInBounce(x);
                case EasingType.OutBounce: return EaseOutBounce(x);
                case EasingType.InOutBounce: return EaseInOutBounce(x);
                case EasingType.OutElastic: return EaseOutElastic(x);
            }
        }

        public static float EaseInExpo(float x)
        {
            return x == 0f ? 0f : Mathf.Pow(2f, 10f * x - 10f);
        }
        public static float EaseOutExpo(float x)
        {
            return x == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * x);
        }

        public static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }

        public static float EaseInBounce(float x)
        {
            return 1 - EaseOutBounce(1 - x);
        }
        public static float EaseOutBounce(float x)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (x < 1f / d1)
            {
                return n1 * x * x;
            }
            else if (x < 2f / d1)
            {
                return n1 * (x -= 1.5f / d1) * x + 0.75f;
            }
            else if (x < 2.5f / d1)
            {
                return n1 * (x -= 2.25f / d1) * x + 0.9375f;
            }
            else
            {
                return n1 * (x -= 2.625f / d1) * x + 0.984375f;
            }
        }
        public static float EaseInOutBounce(float x)
        {
            return x < 0.5f
              ? (1f - EaseOutBounce(1f - 2f * x)) * 0.5f
              : (1f + EaseOutBounce(2f * x - 1f)) * 0.5f;
        }

        public static float EaseOutElastic(float x)
        {
            const float c4 = (2f * Mathf.PI) / 3f;

            return x == 0f
              ? 0f
              : x == 1f
              ? 1f
              : Mathf.Pow(2f, -10f * x) * Mathf.Sin((x * 10f - 0.75f) * c4) + 1f;
        }
    }
}