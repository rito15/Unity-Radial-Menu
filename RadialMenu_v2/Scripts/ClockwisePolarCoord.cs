using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-04-26 PM 4:25:55
// 작성자 : Rito

namespace Rito.RadialMenu_v2
{
    // 북쪽이 0도, 시계방향으로 각도가 커지는 시계 극좌표계의 좌표
    [Serializable]
    public struct ClockwisePolarCoord
    {
        /***********************************************************************
        *                           Fields, Properties
        ***********************************************************************/
        #region .
        /// <summary> 반지름 </summary>
        public float Radius { get; set; }
        /// <summary> 0 ~ 360 각도 </summary>
        public float Angle
        {
            get => _angle;
            set => _angle = ClampAngle(value);
        }
        private float _angle;

        #endregion
        /***********************************************************************
        *                           Constructor
        ***********************************************************************/
        #region .
        public ClockwisePolarCoord(float radius, float angle)
        {
            Radius = radius;
            _angle = ClampAngle(angle);
        }

        #endregion
        /***********************************************************************
        *                           Private Static
        ***********************************************************************/
        #region .
        /// <summary> 0 ~ 360 범위 내의 각도 값 리턴 </summary>
        private static float ClampAngle(float angle)
        {
            angle %= 360f;
            if (angle < 0f)
                angle += 360f;
            return angle;
        }

        /// <summary> +x축 기준 반시계 각도 <-> +y축 기준 시계 각도 서로 변환 </summary>
        private static float CovertAngle(float angle)
            => 90f - angle;

        /// <summary> Degree(0 ~ 360)로 Sin 계산 </summary>
        private static float Sin(float angle)
            => Mathf.Sin(angle * Mathf.Deg2Rad);

        /// <summary> Degree(0 ~ 360)로 Cos 계산 </summary>
        private static float Cos(float angle)
            => Mathf.Cos(angle * Mathf.Deg2Rad);

        #endregion
        /***********************************************************************
        *                           Public Static
        ***********************************************************************/
        #region .
        public static ClockwisePolarCoord Zero => new ClockwisePolarCoord(0f, 0f);
        public static ClockwisePolarCoord North => new ClockwisePolarCoord(1f, 0f);
        public static ClockwisePolarCoord East => new ClockwisePolarCoord(1f, 90f);
        public static ClockwisePolarCoord South => new ClockwisePolarCoord(1f, 180f);
        public static ClockwisePolarCoord West => new ClockwisePolarCoord(1f, 270f);

        /// <summary> 직교 좌표로부터 변환 </summary>
        public static ClockwisePolarCoord FromVector2(in Vector2 vec)
        {
            if (vec == Vector2.zero)
                return Zero;

            float radius = vec.magnitude;
            float angle = Mathf.Atan2(vec.y, vec.x) * Mathf.Rad2Deg;

            return new ClockwisePolarCoord(radius, CovertAngle(angle));
        }

        public static bool operator ==(ClockwisePolarCoord a, ClockwisePolarCoord b)
        {
            return Mathf.Approximately(a.Angle, b.Angle) &&
                   Mathf.Approximately(a.Radius, b.Radius);
        }

        public static bool operator !=(ClockwisePolarCoord a, ClockwisePolarCoord b)
        {
            return !(Mathf.Approximately(a.Angle, b.Angle) &&
                   Mathf.Approximately(a.Radius, b.Radius));
        }

        #endregion
        /***********************************************************************
        *                               Public
        ***********************************************************************/
        #region .
        public ClockwisePolarCoord Normalized => new ClockwisePolarCoord(1f, Angle);

        public Vector2 ToVector2()
        {
            if (Radius == 0f && Angle == 0f)
                return Vector2.zero;

            float angle = CovertAngle(Angle);
            return new Vector2(Radius * Cos(angle), Radius * Sin(angle));
        }

        public override string ToString()
            => $"({Radius}, {Angle})";

        public override bool Equals(object obj)
        {
            if(obj == null) return false;

            if (obj is ClockwisePolarCoord other)
            {
                return this == other;
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}