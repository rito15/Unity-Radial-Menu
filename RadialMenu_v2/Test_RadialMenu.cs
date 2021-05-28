using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 날짜 : 2021-04-26 PM 3:12:50
// 작성자 : Rito

namespace Rito.RadialMenu_v2.Test
{
    public class Test_RadialMenu : MonoBehaviour
    {
        public RadialMenu radialMenu;
        public KeyCode key = KeyCode.G;

        [Space]
        public Sprite[] sprites;

        private void Start()
        {
            radialMenu.SetPieceImageSprites(sprites);
        }

        private void Update()
        {
            if (Input.GetKeyDown(key))
            {
                radialMenu.Show();
            }
            else if (Input.GetKeyUp(key))
            {
                int selected = radialMenu.Hide();
                Debug.Log($"Selected : {selected}");
            }
        }
    }
}