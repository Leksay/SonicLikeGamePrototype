﻿using UnityEngine;
namespace Data.DataScripts
{
    [CreateAssetMenu(fileName = "OpponentsData", menuName = "Objects/OpponentsData")]
    public class OpponentsData : ScriptableObject
    {
        [Header("Move")]
        public float defaultSpeed;
        public float changeRoadTime;
        public float changeRoadTreshold;
        public float accelerationSpeed;
        public float minSpeed;
        public float maxSpeed;

        [Header("Jump")]
        public float jumpHeigh;
        public float upJumpTime;
        public float downJumpTime;
        public float inAirTime;

        [Header("Slide")]
        public float slideTime;
        public OpponentsData Clone() => Instantiate(this);
    }
}
