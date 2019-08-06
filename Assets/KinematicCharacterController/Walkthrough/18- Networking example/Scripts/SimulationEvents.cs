using KinematicCharacterController.Walkthrough.NetworkingExample;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct LazerHitEvent : ISimulationEvent
{
    public MyCharacterController OnCharacter;

    public LazerHitEvent(MyCharacterController character)
    {
        OnCharacter = character;
    }

    public void Apply()
    {
        OnCharacter.IsStunned = true;
        OnCharacter.Motor.BaseVelocity = Vector3.Project(OnCharacter.Motor.BaseVelocity, OnCharacter.Gravity);
        OnCharacter.StunTimeRemaining = OnCharacter.StunDuration;
    }
}
