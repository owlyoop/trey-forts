using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
	public delegate void OnWaitingForPlayersStart();
	public static event OnWaitingForPlayersEnd onWaitingForPlayersStart;
	public delegate void OnWaitingForPlayersEnd();
	public static event OnWaitingForPlayersEnd onWaitingForPlayersEnd;

	public delegate void OnBuildPhaseStart();
	public static event OnWaitingForPlayersEnd onBuildPhaseStart;
	public delegate void OnBuildPhaseEnd();
	public static event OnWaitingForPlayersEnd onBuildPhaseEnd;

	public delegate void OnCOmbatPhaseStart();
	public static event OnWaitingForPlayersEnd onCombatPhaseStart;
	public delegate void OnCombatPhaseEnd();
	public static event OnWaitingForPlayersEnd onCombatPhaseEnd;


	public static void RaiseOnWaitingForPlayersStart()
	{
		onWaitingForPlayersStart?.Invoke();
	}

	public static void RaiseOnWaitingForPlayersEnd()
	{
		onWaitingForPlayersEnd?.Invoke();
	}

	public static void RaiseOnBuildPhaseStart()
	{
		onBuildPhaseStart?.Invoke();
	}

	public static void RaiseOnBuildPhaseEnd()
	{
		onBuildPhaseEnd?.Invoke();
	}

	public static void RaiseOnCombatPhaseStart()
	{
		onCombatPhaseStart?.Invoke();
	}

	public static void RaiseOnCombatPhaseEnd()
	{
		onCombatPhaseEnd?.Invoke();
	}
}
