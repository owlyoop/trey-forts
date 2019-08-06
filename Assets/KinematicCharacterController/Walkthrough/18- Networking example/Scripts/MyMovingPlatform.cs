using KinematicCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Playables;

namespace KinematicCharacterController.Walkthrough.NetworkingExample
{
    public struct MyMovingPlatformState
    {
        public PhysicsMoverState MoverState;
        public float DirectorTime;
    }

    public class MyMovingPlatform : BaseMoverController
    {
        public PlayableDirector Director;

        [NonSerialized]
        public InterpolationPoint FromInterpolationPoint = new InterpolationPoint();
        [NonSerialized]
        public InterpolationPoint ToInterpolationPoint = new InterpolationPoint();

        private Transform _transform;

        private void Start()
        {
            _transform = this.transform;

            FromInterpolationPoint = new InterpolationPoint(Time.time, Mover.TransientPosition, Mover.TransientRotation);
            ToInterpolationPoint = new InterpolationPoint(Time.time, Mover.TransientPosition, Mover.TransientRotation);
        }
        
        // This is called every FixedUpdate by our PhysicsMover in order to tell it what pose it should go to
        public override void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            // Remember pose before animation
            Vector3 _positionBeforeAnim = _transform.position;
            Quaternion _rotationBeforeAnim = _transform.rotation;

            // Update animation
            float tickToTime = (float)GameStatics.GameManager.SimulationSystem.SimulationTick / (1f / Time.fixedDeltaTime);
            EvaluateAtTime(tickToTime);

            // Set our platform's goal pose to the animation's
            goalPosition = _transform.position;
            goalRotation = _transform.rotation;

            // Reset the actual transform pose to where it was before evaluating. 
            // This is so that the real movement can be handled by the physics mover; not the animation
            _transform.position = _positionBeforeAnim;
            _transform.rotation = _rotationBeforeAnim;
        }

        public void EvaluateAtTime(double time)
        {
            Director.time = time % Director.duration;
            Director.Evaluate();
        }

        public void SetPlayableTime(Playable p, float time)
        {
            p.SetTime(time);
            for (int i = 0; i < p.GetInputCount(); i++)
            {
                SetPlayableTime(p.GetInput(i), time);
            }
        }
    }
}