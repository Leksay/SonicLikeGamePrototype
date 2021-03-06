﻿using Players;
using UnityEngine;
namespace Data.DataScripts
{
    [System.Serializable]
    public class PlayerCameraData
    {
        public Transform   playerT;
        public Transform   cameraPositionT;
        public Transform   slidePositionT;
        public Transform   cameraStartT;
        public Transform   deathLoopPositionT;
        public PlayerMover mover;
    }
}
