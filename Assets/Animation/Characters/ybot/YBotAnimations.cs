using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YBotAnimations : MonoBehaviour {

	public Animator animator;

	public float InputForward;
	public float InputRight;
	
	void Start ()
	{
		animator = this.gameObject.GetComponent<Animator>();
	}
	
	void Update ()
	{
		InputForward = Input.GetAxis("Vertical");
		InputRight = Input.GetAxis("Horizontal");

		animator.SetFloat("InputForward", InputForward);
		animator.SetFloat("InputRight", InputRight);
	}
}
