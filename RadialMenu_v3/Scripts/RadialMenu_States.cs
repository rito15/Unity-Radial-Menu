using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 날짜 : 2021-04-26 PM 5:23:19
// 작성자 : Rito

namespace Rito.RadialMenu_v3
{
    public partial class RadialMenu : MonoBehaviour
    {
        public enum AppearanceType
        {
            None,
            Fade,
            ScaleChange,
            FadeAndScaleChange,
            Spread,
            Progressive,
            ProgressiveFade,
            ProgressiveScaleChange,
            ProgressiveSpread,
        }
        private enum MainType
        {
            AlphaChange,
            ScaleChange,
            AlphaAndScaleChange
        }

        //[Header("Animation Types")]
        [SerializeField] private AppearanceType _appearanceType;
        [SerializeField] private EasingType _appearanceEasing;

        //[Space(6)]
        [SerializeField] private MainType _mainType;

        //[Space(6)]
        [SerializeField] private AppearanceType _disappearanceType;
        [SerializeField] private EasingType _disappearanceEasing;

        private Dictionary<AppearanceType, AppearanceState> _appStateDict = 
            new Dictionary<AppearanceType, AppearanceState>();

        private Dictionary<MainType, MainState> _mainStateDict =
            new Dictionary<MainType, MainState>();

        private Dictionary<AppearanceType, DisappearanceState> _disStateDict = 
            new Dictionary<AppearanceType, DisappearanceState>();

        private MenuState _currentState;
        private MenuState AState => _appStateDict[_appearanceType];
        private MenuState MState => _mainStateDict[_mainType];
        private MenuState DState => _disStateDict[_disappearanceType];

        // 상태 전환 진행도
        private float _stateProgress = 0f;

        private void Update()
        {
            _currentState.Update();
        }

        private void InitStateDicts()
        {
            _currentState = NullState.Instance;

            // 1. Appearance
            _appStateDict.Add(AppearanceType.None, new DefaultAppearance(this));
            _appStateDict.Add(AppearanceType.Fade, new FadeIn(this));
            _appStateDict.Add(AppearanceType.ScaleChange, new ScaleUp(this));
            _appStateDict.Add(AppearanceType.FadeAndScaleChange, new FadeInAndScaleUp(this));
            _appStateDict.Add(AppearanceType.Spread, new Spread(this));
            _appStateDict.Add(AppearanceType.Progressive, new ProgressiveAppearance(this));
            _appStateDict.Add(AppearanceType.ProgressiveFade, new ProgressiveFadeIn(this));
            _appStateDict.Add(AppearanceType.ProgressiveScaleChange, new ProgressiveScaleUp(this));
            _appStateDict.Add(AppearanceType.ProgressiveSpread, new ProgressiveSpread(this));

            // 2. Main
            _mainStateDict.Add(MainType.AlphaChange, new MainAlphaChange(this));
            _mainStateDict.Add(MainType.ScaleChange, new MainScaleChange(this));
            _mainStateDict.Add(MainType.AlphaAndScaleChange, new MainAlphaAndScaleChange(this));

            // 3. Disappearance
            _disStateDict.Add(AppearanceType.None, new DefaultDisappearance(this));
            _disStateDict.Add(AppearanceType.Fade, new FadeOut(this));
            _disStateDict.Add(AppearanceType.ScaleChange, new ScaleDown(this));
            _disStateDict.Add(AppearanceType.FadeAndScaleChange, new FadeOutAndScaleDown(this));
            _disStateDict.Add(AppearanceType.Spread, new Gather(this));
            _disStateDict.Add(AppearanceType.Progressive, new ProgressiveDisappearance(this));
            _disStateDict.Add(AppearanceType.ProgressiveFade, new ProgressiveFadeOut(this));
            _disStateDict.Add(AppearanceType.ProgressiveScaleChange, new ProgressiveScaleDown(this));
            _disStateDict.Add(AppearanceType.ProgressiveSpread, new ProgressiveGather(this));
        }

        private void ChangeToNextState()
        {
            _currentState.OnExit();

            if (_currentState == NullState.Instance) _currentState = AState;
            else if(_currentState == AState) _currentState = MState;
            else if(_currentState == MState) _currentState = DState;
            else _currentState = NullState.Instance;

            _currentState.OnEnter();
        }

        /// <summary> 강제로 등장 상태에 진입 </summary>
        private void ForceToEnterAppearanceState()
        {
            // 소멸 상태가 진행 중인 경우, 무시
            if (_stateProgress > 0f)
            {
                return;
            }
            _currentState = AState;
            _currentState.OnEnter();
        }

        /// <summary> 강제로 소멸 상태에 진입 </summary>
        private void ForceToEnterDisappearanceState()
        {
            // 등장 상태가 진행중이면 즉시 종료
            if (_stateProgress < 1f)
            {
                _currentState = _disStateDict[AppearanceType.None];
                _currentState.OnEnter();
            }
            // 정상 진행 : Disappearance 상태로 진입
            else
            {
                _currentState.OnExit();
                _currentState = DState;
                _currentState.OnEnter();
            }
        }

        /***********************************************************************
        *                           Abstract State Definitions
        ***********************************************************************/
        #region .
        private abstract class MenuState
        {
            protected readonly RadialMenu menu;

            public MenuState(RadialMenu menu)
                => this.menu = menu;

            public abstract void OnEnter();
            public abstract void Update();
            public abstract void OnExit();

            /// <summary> 메인 상태의 조각 알파값 구하기 </summary>
            protected float GetMainStatePieceAlpha()
            {
                switch (menu._mainType)
                {
                    case MainType.AlphaChange:
                    case MainType.AlphaAndScaleChange: 
                        return NotSelectedPieceAlpha;

                    default: 
                        return 1f;
                }
            }
        }

        private sealed class NullState : MenuState
        {
            public static NullState Instance => new NullState();

            public NullState() : base(null) { }

            public override void OnEnter() { }

            public override void OnExit() { }

            public override void Update() { }
        }

        // 1. 등장
        private abstract class AppearanceState : MenuState
        {
            public AppearanceState(RadialMenu menu) : base(menu) { }

            public override void OnEnter()
            {
                menu._selectedIndex = -1;
                menu.ShowGameObject();
                menu.SetArrow(false);

                if(menu._stateProgress == 0f)
                    OnEnterAtZeroProgress();
            }

            /// <summary> 완전히 진행도가 0인 상태에서 나타날 경우 실행 </summary>
            protected virtual void OnEnterAtZeroProgress() { }

            public override void Update()
            {
                Execute();
                menu._stateProgress += Time.deltaTime / menu._appearanceDuration;

                if (menu._stateProgress >= 1f)
                {
                    menu._stateProgress = 1f;
                    menu.ChangeToNextState();
                }
            }

            protected abstract void Execute();

            public override void OnExit() { }

            /// <summary> Easing 적용된 값 구하기 </summary>
            protected float GetEasedProgress(float progress)
            {
                return Easing.Get(progress, menu._appearanceEasing);
            }
        }

        // 2. 유지
        private abstract class MainState : MenuState
        {
            // 이전 프레임의 선택 인덱스
            protected int prevSelectedIndex = -1;

            public MainState(RadialMenu menu) : base(menu) { }

            public override void OnEnter()
            {
                prevSelectedIndex = -1;

                menu.SetAllPieceDistance(menu._pieceDist);
                menu.SetAllPieceScale(1f);
                menu.SetAllPieceAlpha(GetMainStatePieceAlpha());
                menu.SetAllPieceImageEnabled(true);
            }

            public override void Update()
            {
                bool showArrow = false;

                // 마우스의 스크린 내 좌표(0.0 ~ 1.0 범위)
                var mViewportPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

                // 스크린의 중앙을 (0, 0)으로 하는 마우스 좌표(-0.5 ~ 0.5 범위)
                var mPos = new Vector2(mViewportPos.x - 0.5f, mViewportPos.y - 0.5f);

                // 중심에서 마우스까지의 거리
                var mDist = new Vector2(mPos.x * Screen.width / Screen.height, mPos.y).magnitude;

                if (mDist < menu._centerRange)
                {
                    menu._selectedIndex = -1;
                }
                else
                {
                    // 마우스 위치의 직교 좌표를 시계 극좌표로 변환
                    ClockwisePolarCoord mousePC = ClockwisePolarCoord.FromVector2(mPos);

                    // Arrow 회전 설정
                    menu._arrowRotationZ = -mousePC.Angle;
                    showArrow = true;

                    // 각도로부터 배열 인덱스 계산
                    float fIndex = (mousePC.Angle / 360f) * menu._pieceCount;
                    menu._selectedIndex = Mathf.RoundToInt(fIndex) % menu._pieceCount;
                }

                // 화살표 회전
                menu.SetArrow(showArrow);

                // 선택 인덱스 변경
                if (prevSelectedIndex != menu._selectedIndex)
                    OnSelectedIndexChanged(prevSelectedIndex, menu._selectedIndex);

                // 이전 인덱스 기억
                prevSelectedIndex = menu._selectedIndex;
            }

            /// <summary> 선택된 인덱스 변경 </summary>
            public abstract void OnSelectedIndexChanged(int prevIndex, int currentIndex);

            public override void OnExit() { }
        }

        // 3. 소멸
        private abstract class DisappearanceState : MenuState
        {
            public DisappearanceState(RadialMenu menu) : base(menu) { }

            public override void OnEnter()
            {
            }

            public override void Update()
            {
                Execute();
                menu._stateProgress -= Time.deltaTime / menu._disppearanceDuration;

                if (menu._stateProgress <= 0f)
                {
                    menu._stateProgress = 0f;
                    menu.ChangeToNextState();
                }
            }

            protected abstract void Execute();

            public override void OnExit()
            {
                menu.HideGameObject();
            }

            /// <summary> Easing 적용된 값 구하기 </summary>
            protected float GetEasedProgress(float progress)
            {
                return Easing.Get(progress, menu._disappearanceEasing);
            }
        }

        #endregion
        /***********************************************************************
        *                           Appearance States
        ***********************************************************************/
        #region .
        private sealed class DefaultAppearance : AppearanceState
        {
            public DefaultAppearance(RadialMenu menu) : base(menu) { }

            protected override void Execute()
            {
                menu.ChangeToNextState();
            }
        }

        /// <summary> 알파값 증가 </summary>
        private sealed class FadeIn : AppearanceState
        {
            private float alphaGoal;

            public FadeIn(RadialMenu menu) : base(menu) { }

            public override void OnEnter()
            {
                base.OnEnter();

                // 변화 목표 알파값 지정
                alphaGoal = GetMainStatePieceAlpha();
            }

            protected override void OnEnterAtZeroProgress()
            {
                menu.SetAllPieceDistance(menu._pieceDist);
                menu.SetAllPieceScale(1f);
                menu.SetAllPieceAlpha(0f);
                menu.SetAllPieceImageEnabled(true);
            }

            protected override void Execute()
            {
                // 알파값 서서히 변경
                menu.SetAllPieceAlpha(alphaGoal * GetEasedProgress(menu._stateProgress));
            }
        }

        /// <summary> 크기 증가 </summary>
        private sealed class ScaleUp : AppearanceState
        {
            public ScaleUp(RadialMenu menu) : base(menu) { }

            protected override void OnEnterAtZeroProgress()
            {
                menu.SetAllPieceDistance(menu._pieceDist);
                menu.SetAllPieceScale(0.001f);
                menu.SetAllPieceAlpha(GetMainStatePieceAlpha());
                menu.SetAllPieceImageEnabled(true);
            }

            protected override void Execute()
            {
                // 스케일 변경
                menu.SetAllPieceScale(GetEasedProgress(menu._stateProgress));
            }
        }

        /// <summary> 알파값 증가 + 크기 증가 </summary>
        private sealed class FadeInAndScaleUp : AppearanceState
        {
            private float alphaGoal;
            public FadeInAndScaleUp(RadialMenu menu) : base(menu) { }

            protected override void OnEnterAtZeroProgress()
            {
                alphaGoal = GetMainStatePieceAlpha();

                menu.SetAllPieceDistance(menu._pieceDist);
                menu.SetAllPieceScale(0.001f);
                menu.SetAllPieceAlpha(alphaGoal);
                menu.SetAllPieceImageEnabled(true);
            }

            protected override void Execute()
            {
                float t = GetEasedProgress(menu._stateProgress);

                // 스케일, 알파 변경
                menu.SetAllPieceScale(t);
                menu.SetAllPieceAlpha(t * alphaGoal);
            }
        }

        /// <summary> 중앙에서부터 각자 위치로 흩어지기 </summary>
        private sealed class Spread : AppearanceState
        {
            public Spread(RadialMenu menu) : base(menu) { }

            protected override void OnEnterAtZeroProgress()
            {
                menu.SetAllPieceDistance(0f);
                menu.SetAllPieceScale(0.001f);
                menu.SetAllPieceAlpha(GetMainStatePieceAlpha());
                menu.SetAllPieceImageEnabled(true);
            }

            protected override void Execute()
            {
                float t = GetEasedProgress(menu._stateProgress);

                // 중앙으로부터의 거리 계산
                float dist = t * menu._pieceDist;

                // 각 조각들을 중앙에서부터 서서히 이동
                menu.SetAllPieceDistance(dist);

                // 스케일도 변경
                menu.SetAllPieceScale(t);
            }
        }


        /// <summary> 인덱스 0번부터 순차적으로 나타나기 </summary>
        private sealed class ProgressiveAppearance : AppearanceState
        {
            public ProgressiveAppearance(RadialMenu menu) : base(menu) { }

            protected override void OnEnterAtZeroProgress()
            {
                menu.SetAllPieceDistance(menu._pieceDist);
                menu.SetAllPieceScale(1f);
                menu.SetAllPieceAlpha(GetMainStatePieceAlpha());
                menu.SetAllPieceImageEnabled(false);
            }

            protected override void Execute()
            {
                int lastIndex = (int)(menu._stateProgress * menu._pieceCount);

                for (int i = 0; i < menu._pieceCount; i++)
                {
                    menu._pieceImages[i].enabled = (i <= lastIndex);
                }
            }
        }

        /// <summary> 인덱스 0번부터 순차적으로 알파값 증가 </summary>
        private sealed class ProgressiveFadeIn : AppearanceState
        {
            private float alphaGoal;

            public ProgressiveFadeIn(RadialMenu menu) : base(menu) { }

            public override void OnEnter()
            {
                base.OnEnter();
                alphaGoal = GetMainStatePieceAlpha();
            }

            protected override void OnEnterAtZeroProgress()
            {
                menu.SetAllPieceDistance(menu._pieceDist);
                menu.SetAllPieceScale(1f);
                menu.SetAllPieceAlpha(0f);
                menu.SetAllPieceImageEnabled(true);
            }

            protected override void Execute()
            {
                float t = menu._stateProgress;
                float n = menu._pieceCount;

                for (int i = 0; i < n; i++)
                {
                    float a = i / n;
                    float alpha = (t - a) / (1 - a);

                    //float alpha = n * t - i;
                    //if (alpha > 1f) alpha = 1f;

                    menu._pieceImages[i].color = new Color(1f, 1f, 1f, alpha * alphaGoal);
                }
            }
        }

        /// <summary> 인덱스 0번부터 순차적으로 크기 증가 </summary>
        private sealed class ProgressiveScaleUp : AppearanceState
        {
            public ProgressiveScaleUp(RadialMenu menu) : base(menu) { }

            public override void OnEnter()
            {
                base.OnEnter();
            }

            protected override void OnEnterAtZeroProgress()
            {
                menu.SetAllPieceDistance(menu._pieceDist);
                menu.SetAllPieceScale(0f);
                menu.SetAllPieceAlpha(GetMainStatePieceAlpha());
                menu.SetAllPieceImageEnabled(true);
            }

            protected override void Execute()
            {
                float t = menu._stateProgress;
                float n = menu._pieceCount;

                for (int i = 0; i < n; i++)
                {
                    float a = i / n;
                    float scale = (t - a) / (1 - a);
                    if (scale < 0f) scale = 0f;

                    //float scale = n * t - i;
                    //scale = Mathf.Clamp01(scale);

                    menu._pieceRects[i].localScale = new Vector3(scale, scale, 1f);
                }
            }
        }

        /// <summary> 인덱스 0번부터 순차적으로 외곽으로 흩어지기 </summary>
        private sealed class ProgressiveSpread : AppearanceState
        {
            public ProgressiveSpread(RadialMenu menu) : base(menu) { }

            public override void OnEnter()
            {
                base.OnEnter();
            }

            protected override void OnEnterAtZeroProgress()
            {
                menu.SetAllPieceDistance(0f);
                menu.SetAllPieceScale(0f);
                menu.SetAllPieceAlpha(GetMainStatePieceAlpha());
                menu.SetAllPieceImageEnabled(true);
            }

            protected override void Execute()
            {
                float t = menu._stateProgress;
                float n = menu._pieceCount;

                for (int i = 0; i < n; i++)
                {
                    //float a = i / n;
                    //float value = (t - a) / (1 - a);
                    //if (value < 0f) value = 0f;

                    float value = n * t - i;
                    value = Mathf.Clamp01(value);

                    menu._pieceRects[i].localScale = new Vector3(value, value, 1f);
                    menu._pieceRects[i].anchoredPosition = menu._pieceDirections[i] * value * menu._pieceDist;
                }
            }
        }

        #endregion
        /***********************************************************************
        *                           Main States
        ***********************************************************************/
        #region .

        /// <summary> 선택된 조각 알파값 증가 </summary>
        private sealed class MainAlphaChange : MainState
        {
            public MainAlphaChange(RadialMenu menu) : base(menu) { }

            public override void OnSelectedIndexChanged(int prevIndex, int currentIndex)
            {
                if(prevIndex >= 0)
                    menu.SetPieceAlpha(prevIndex, NotSelectedPieceAlpha);

                if (currentIndex >= 0)
                    menu.SetPieceAlpha(currentIndex, 1f);
            }

            public override void OnExit()
            {
                if (menu._selectedIndex >= 0)
                {
                    menu.SetPieceAlpha(menu._selectedIndex, NotSelectedPieceAlpha);
                }
            }
        }

        /// <summary> 선택된 조각 크기 증가 </summary>
        private sealed class MainScaleChange : MainState
        {
            public MainScaleChange(RadialMenu menu) : base(menu) { }

            public override void OnSelectedIndexChanged(int prevIndex, int currentIndex)
            {
                if (prevIndex >= 0)
                    menu.SetPieceScale(prevIndex, 1f);

                if (currentIndex >= 0)
                    menu.SetPieceScale(currentIndex, 1.2f);
            }

            public override void OnExit()
            {
                if (menu._selectedIndex >= 0)
                {
                    menu.SetPieceScale(menu._selectedIndex, 1f);
                }
            }
        }

        /// <summary> 선택된 조각 알파, 크기 증가 </summary>
        private sealed class MainAlphaAndScaleChange : MainState
        {
            public MainAlphaAndScaleChange(RadialMenu menu) : base(menu) { }

            public override void OnSelectedIndexChanged(int prevIndex, int currentIndex)
            {
                if (prevIndex >= 0)
                {
                    menu.SetPieceAlpha(prevIndex, NotSelectedPieceAlpha);
                    menu.SetPieceScale(prevIndex, 1f);
                }

                if (currentIndex >= 0)
                {
                    menu.SetPieceAlpha(currentIndex, 1f);
                    menu.SetPieceScale(currentIndex, 1.2f);
                }
            }

            public override void OnExit()
            {
                if (menu._selectedIndex >= 0)
                {
                    menu.SetPieceAlpha(menu._selectedIndex, NotSelectedPieceAlpha);
                    menu.SetPieceScale(menu._selectedIndex, 1f);
                }
            }
        }

        #endregion
        /***********************************************************************
        *                           Disappearance States
        ***********************************************************************/
        #region .

        private sealed class DefaultDisappearance : DisappearanceState
        {
            public DefaultDisappearance(RadialMenu menu) : base(menu) { }

            protected override void Execute()
            {
                menu._stateProgress = 0f;
                menu.ChangeToNextState();
            }
        }

        /// <summary> 알파값 감소 </summary>
        private sealed class FadeOut : DisappearanceState
        {
            private float alphaGoal;
            public FadeOut(RadialMenu menu) : base(menu) { }

            public override void OnEnter()
            {
                base.OnEnter();

                // 변화 목표 알파값 지정
                alphaGoal = GetMainStatePieceAlpha();
            }

            protected override void Execute()
            {
                // 알파값 서서히 감소
                menu.SetAllPieceAlpha(alphaGoal * GetEasedProgress(menu._stateProgress));
            }
        }

        /// <summary> 크기 감소 </summary>
        private sealed class ScaleDown : DisappearanceState
        {
            public ScaleDown(RadialMenu menu) : base(menu) { }

            protected override void Execute()
            {
                // 스케일 감소
                menu.SetAllPieceScale(GetEasedProgress(menu._stateProgress));
            }
        }

        /// <summary> 알파값 감소 + 크기 감소 </summary>
        private sealed class FadeOutAndScaleDown : DisappearanceState
        {
            private float alphaGoal;
            public FadeOutAndScaleDown(RadialMenu menu) : base(menu) { }

            public override void OnEnter()
            {
                base.OnEnter();

                // 변화 목표 알파값 지정
                alphaGoal = GetMainStatePieceAlpha();
            }

            protected override void Execute()
            {
                float t = GetEasedProgress(menu._stateProgress);

                // 스케일, 알파 변경
                menu.SetAllPieceScale(t);
                menu.SetAllPieceAlpha(t * alphaGoal);
            }
        }

        /// <summary> 중심으로 이동 </summary>
        private sealed class Gather : DisappearanceState
        {
            public Gather(RadialMenu menu) : base(menu) { }

            protected override void Execute()
            {
                float t = GetEasedProgress(menu._stateProgress);

                // 중앙으로부터의 거리 계산
                float dist = t * menu._pieceDist;

                // 각 조각들을 중앙에서부터 서서히 이동
                menu.SetAllPieceDistance(dist);

                // 스케일도 변경
                menu.SetAllPieceScale(t);
            }
        }


        /// <summary> 마지막 인덱스부터 순차적으로 사라지기 </summary>
        private sealed class ProgressiveDisappearance : DisappearanceState
        {
            public ProgressiveDisappearance(RadialMenu menu) : base(menu) { }

            protected override void Execute()
            {
                int lastIndex = (int)(menu._stateProgress * menu._pieceCount);

                for (int i = 0; i < menu._pieceCount; i++)
                {
                    menu._pieceImages[i].enabled = (i <= lastIndex);
                }
            }
        }

        /// <summary> 마지막 인덱스부터 순차적으로 알파값 감소 </summary>
        private sealed class ProgressiveFadeOut : DisappearanceState
        {
            private float alphaGoal;

            public ProgressiveFadeOut(RadialMenu menu) : base(menu) { }

            public override void OnEnter()
            {
                base.OnEnter();

                // 변화 목표 알파값 지정
                alphaGoal = GetMainStatePieceAlpha();
            }

            protected override void Execute()
            {
                float t = menu._stateProgress;
                float n = menu._pieceCount;

                for (int i = 0; i < n; i++)
                {
                    float a = i / n;
                    float alpha = (t - a) / (1 - a);

                    //float alpha = n * t - i;
                    //if (alpha > 1f) alpha = 1f;

                    menu._pieceImages[i].color = new Color(1f, 1f, 1f, alpha * alphaGoal);
                }
            }
        }

        /// <summary> 마지막 인덱스부터 순차적으로 크기 감소 </summary>
        private sealed class ProgressiveScaleDown : DisappearanceState
        {
            public ProgressiveScaleDown(RadialMenu menu) : base(menu) { }

            public override void OnEnter()
            {
                base.OnEnter();
            }

            protected override void Execute()
            {
                float t = menu._stateProgress;
                float n = menu._pieceCount;

                for (int i = 0; i < n; i++)
                {
                    float a = i / n;
                    float scale = (t - a) / (1 - a);
                    if (scale < 0f) scale = 0f;

                    //float scale = n * t - i;
                    //scale = Mathf.Clamp01(scale);

                    menu._pieceRects[i].localScale = new Vector3(scale, scale, 1f);
                }
            }
        }

        /// <summary> 마지막 인덱스부터 순차적으로 중앙으로 모으기 </summary>
        private sealed class ProgressiveGather : DisappearanceState
        {
            public ProgressiveGather(RadialMenu menu) : base(menu) { }

            public override void OnEnter()
            {
                base.OnEnter();
            }

            protected override void Execute()
            {
                float t = menu._stateProgress;
                float n = menu._pieceCount;

                for (int i = 0; i < n; i++)
                {
                    //float a = i / n;
                    //float value = (t - a) / (1 - a);
                    //if (value < 0f) value = 0f;

                    float value = n * t - i;
                    value = Mathf.Clamp01(value);

                    menu._pieceRects[i].localScale = new Vector3(value, value, 1f);
                    menu._pieceRects[i].anchoredPosition = menu._pieceDirections[i] * value * menu._pieceDist;
                }
            }
        }

        #endregion
    }
}