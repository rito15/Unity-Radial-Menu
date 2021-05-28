using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 날짜 : 2021-04-26 PM 2:47:40
// 작성자 : Rito

namespace Rito.RadialMenu_v1
{
    [DisallowMultipleComponent]
    public class RadialMenu : MonoBehaviour
    {
        [Header("Options")]
        [Range(2, 16)]
        [SerializeField] private int _pieceCount = 8; // 조각 개수

        [Range(0.2f, 1f)]
        [SerializeField] private float _appearanceDuration = .3f; // 등장에 걸리는 시간
        [Range(0.2f, 1f)]
        [SerializeField] private float _disappearanceDuration = .3f; // 사라지는데 걸리는 시간
        [SerializeField] private float _pieceDist = 180f; // 중앙으로부터 각 조각의 거리

        [Range(0.01f, 0.5f)]
        [SerializeField] private float _centerDistThreshold = 0.1f; // 중앙에서부터의 마우스 거리 기준

        [Header("Objects")]
        [SerializeField] private GameObject _pieceSample; // 복제될 조각 게임오브젝트
        [SerializeField] private RectTransform _arrow;    // 화살표 이미지의 부모 트랜스폼
        
        // 복제된 조각들
        private Image[] _pieceImages;
        private RectTransform[] _pieceRects;
        private Vector2[] _pieceDirections; // 각 조각이 위치할 방향의 벡터

        private float _arrowRotationZ;

        [SerializeField, Header("Debug")]
        private int _selectedIndex = -1;

        private static readonly Color SelectedPieceColor    = new Color(1f, 1f, 1f, 1f);
        private static readonly Color NotSelectedPieceColor = new Color(1f, 1f, 1f, 0.3f);

        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .

        private void Awake()
        {
            InitPieceImages();
            InitPieceDirections();
            HideGameObject();
        }

        #endregion
        /***********************************************************************
        *                               Private Methods
        ***********************************************************************/
        #region .
        /// <summary> 조각 샘플 복제하여 조각들 생성 </summary>
        private void InitPieceImages()
        {
            _pieceSample.SetActive(true);

            _pieceImages = new Image[_pieceCount];
            _pieceRects = new RectTransform[_pieceCount];

            for (int i = 0; i < _pieceCount; i++)
            {
                // 조각 복제
                var clone = Instantiate(_pieceSample, transform);
                clone.name = $"Piece {i}";

                // Image, RectTransform 가져와 배열에 초기화
                _pieceImages[i] = clone.GetComponent<Image>();
                _pieceRects[i] = _pieceImages[i].rectTransform;
            }

            _pieceSample.SetActive(false);
        }

        /// <summary> 시계 극좌표계를 이용해 각 조각들의 방향벡터 계산 </summary>
        private void InitPieceDirections()
        {
            _pieceDirections = new Vector2[_pieceCount];

            float angle = 360f / _pieceCount;

            for (int i = 0; i < _pieceCount; i++)
            {
                _pieceDirections[i] = new ClockwisePolarCoord(1f, angle * i).ToVector2();
            }
        }

        private void ShowGameObject()
        {
            gameObject.SetActive(true);
        }

        private void HideGameObject()
        {
            gameObject.SetActive(false);
        }

        /// <summary> 모든 조각의 색상 변경 </summary>
        private void ResetAllPieceColors()
        {
            for (int i = 0; i < _pieceCount; i++)
            {
                _pieceImages[i].color = NotSelectedPieceColor;
            }
        }

        /// <summary> 현재 선택된 조각의 색상 변경 </summary>
        private void SetSelectedPieceColors()
        {
            ResetAllPieceColors();
            if(_selectedIndex >= 0)
                _pieceImages[_selectedIndex].color = SelectedPieceColor;
        }

        /// <summary> 화살표 이미지의 회전 설정 </summary>
        private void SetArrowRotation(bool show)
        {
            _arrow.gameObject.SetActive(show);

            if (show)
            {
                _arrow.eulerAngles = Vector3.forward * _arrowRotationZ;
            }
        }
        #endregion
        /***********************************************************************
        *                               Public Methods
        ***********************************************************************/
        #region .

        /// <summary> 등장 </summary>
        public void Show()
        {
            ShowGameObject();
            ResetAllPieceColors();
            SetArrowRotation(false);
            _selectedIndex = -1;

            StartCoroutine(nameof(MainRoutine));
        }

        /// <summary> 사라지면서 인덱스 리턴 </summary>
        public int Hide()
        {
            StopCoroutine(nameof(MainRoutine));
            HideGameObject();

            return _selectedIndex;
        }

        /// <summary> 각각 피스 이미지(스프라이트) 등록 </summary>
        public void SetPieceImageSprites(Sprite[] sprites)
        {
            int i = 0;
            int len = sprites.Length;
            for (; i < _pieceCount && i < len; i++)
            {
                if (sprites[i] != null)
                {
                    _pieceImages[i].sprite = sprites[i];
                }
            }
        }

        #endregion
        /***********************************************************************
        *                               Coroutines
        ***********************************************************************/
        #region .
        private IEnumerator MainRoutine()
        {
            float t = 0;
            int prevSelectedIndex = -1;

            // 1. 등장
            while (t < _appearanceDuration)
            {
                // 중앙으로부터의 거리 계산
                float dist = t * _pieceDist / _appearanceDuration;

                // 각 조각들을 중앙에서부터 서서히 이동
                for (int i = 0; i < _pieceCount; i++)
                {
                    _pieceRects[i].anchoredPosition = _pieceDirections[i] * dist;
                }

                t += Time.deltaTime;
                yield return null;
            }

            // 2. 유지
            while (true)
            {
                bool showArrow = false;

                // 마우스의 스크린 내 좌표(0.0 ~ 1.0 범위)
                var mViewportPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

                // 스크린의 중앙을 (0, 0)으로 하는 마우스 좌표(-0.5 ~ 0.5 범위)
                var mPos = new Vector2(mViewportPos.x - 0.5f, mViewportPos.y - 0.5f);

                // 중앙에서 마우스까지의 거리
                var mDist = new Vector2(mPos.x * Screen.width / Screen.height, mPos.y).magnitude;

                if (mDist < _centerDistThreshold)
                {
                    _selectedIndex = -1;
                }
                else
                {
                    // 마우스 위치의 직교 좌표를 시계 극좌표로 변환
                    ClockwisePolarCoord mousePC = ClockwisePolarCoord.FromVector2(mPos);

                    // Arrow 회전 설정
                    _arrowRotationZ = -mousePC.Angle;
                    showArrow = true;

                    // 각도로부터 배열 인덱스 계산
                    float fIndex = (mousePC.Angle / 360f) * _pieceCount;
                    _selectedIndex = Mathf.RoundToInt(fIndex) % _pieceCount;
                }

                // 선택된 조각 색상 변경
                if(prevSelectedIndex != _selectedIndex)
                    SetSelectedPieceColors();

                // 화살표 회전
                SetArrowRotation(showArrow);

                yield return null;

                prevSelectedIndex = _selectedIndex;
            }
        }

        #endregion
    }
}