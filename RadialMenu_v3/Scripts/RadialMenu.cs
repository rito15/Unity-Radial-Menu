using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 날짜 : 2021-04-26 PM 2:47:40
// 작성자 : Rito

namespace Rito.RadialMenu_v3
{
    [DisallowMultipleComponent]
    public partial class RadialMenu : MonoBehaviour
    {
        //[Header("Options")]
        //[Range(2, 16)]
        [SerializeField] private int _pieceCount = 8; // 조각 개수

        //[Range(0.2f, 1f)]
        [SerializeField] private float _appearanceDuration = .3f; // 등장에 걸리는 시간
        //[Range(0.2f, 1f)]
        [SerializeField] private float _disppearanceDuration = .3f; // 소멸에 걸리는 시간
        [SerializeField] private float _pieceDist = 180f; // 중앙으로부터 각 조각의 거리

        //[Range(0.01f, 0.5f)]
        [SerializeField] private float _centerRange = 0.1f; // 커서 인식 안되는 중앙 범위

        //[Header("Objects")]
        [SerializeField] private GameObject _pieceSample; // 복제될 조각 게임오브젝트
        [SerializeField] private RectTransform _arrow;    // 화살표 이미지의 부모 트랜스폼
        
        // 복제된 조각들
        private Image[] _pieceImages;
        private RectTransform[] _pieceRects;
        private Vector2[] _pieceDirections; // 각 조각이 위치할 방향의 벡터

        private float _arrowRotationZ;

        //[Header("Debug"), Space(20)]
        [SerializeField]
        private int _selectedIndex = -1;

        private const float NotSelectedPieceAlpha = 0.5f;
        private static readonly Color SelectedPieceColor    = new Color(1f, 1f, 1f, 1f);
        private static readonly Color NotSelectedPieceColor = new Color(1f, 1f, 1f, NotSelectedPieceAlpha);

        /***********************************************************************
        *                               Unity Events
        ***********************************************************************/
        #region .

        private void Awake()
        {
            InitPieceImages();
            InitPieceDirections();
            InitStateDicts();

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

        /// <summary> 지정한 이미지의 알파값 변경 </summary>
        private void SetPieceAlpha(int index, float alpha)
        {
            _pieceImages[index].color = new Color(1f, 1f, 1f, alpha);
        }

        /// <summary> 지정한 이미지의 중심으로부터의 거리 변경 </summary>
        private void SetPieceDistance(int index, float distance)
        {
            _pieceRects[index].anchoredPosition = _pieceDirections[index] * distance;
        }

        /// <summary> 해당 인덱스의 조각 크기 변경 </summary>
        private void SetPieceScale(int index, float scale)
        {
            _pieceRects[index].localScale = new Vector3(scale, scale, 1f);
        }

        /// <summary> 모든 조각을 중심으로부터 지정 거리만큼 이동 </summary>
        private void SetAllPieceDistance(float distance)
        {
            for (int i = 0; i < _pieceCount; i++)
            {
                _pieceRects[i].anchoredPosition = _pieceDirections[i] * distance;
            }
        }

        /// <summary> 모든 조각 이미지의 알파값 변경 </summary>
        private void SetAllPieceAlpha(float alpha)
        {
            for (int i = 0; i < _pieceCount; i++)
            {
                _pieceImages[i].color = new Color(1f, 1f, 1f, alpha);
            }
        }

        /// <summary> 모든 조각의 크기 변경 </summary>
        private void SetAllPieceScale(float scale)
        {
            for (int i = 0; i < _pieceCount; i++)
            {
                _pieceRects[i].localScale = new Vector3(scale, scale, 1f);
            }
        }

        private void SetAllPieceImageEnabled(bool enabled)
        {
            for (int i = 0; i < _pieceCount; i++)
            {
                _pieceImages[i].enabled = enabled;
            }
        }

        /// <summary> 화살표 이미지 게임오브젝트 활성화 여부, 회전 설정 </summary>
        private void SetArrow(bool show)
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
            ForceToEnterAppearanceState();
        }

        /// <summary> 사라지면서 인덱스 리턴 </summary>
        public int Hide()
        {
            ForceToEnterDisappearanceState();
            SetArrow(false);

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
    }
}