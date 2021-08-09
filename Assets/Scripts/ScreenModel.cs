
using UniRx;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace MagicCube
{
    public enum ScreenType
    {
        ホーム画面,
        キューブサイズ調整画面,
        メイン画面,
        エンディング画面,
    }

    public class ScreenModel
    {
        private readonly ReactiveProperty<ScreenType> _screenType = new ReactiveProperty<ScreenType>( ScreenType.ホーム画面 );
        public IReadOnlyReactiveProperty<ScreenType> screenType { get => _screenType; }

        private List<PlaneData> lastGameData = new List<PlaneData>();

        public ScreenModel()
        {
            // ここで前回の続きがあるかどうかをみる
            lastGameData = new List<PlaneData>();
        }

        public bool GetIsLastGameData()
        {
            return lastGameData.Count != 0;
        }

        public void SetScreenType(ScreenType screenType)
        {
            _screenType.Value = screenType;
        }

    }
}
