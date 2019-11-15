using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FortwarsProp : MonoBehaviour, IDamagable
{

    const float BUILD_PHASE_BUILD_TIME = 0.25f;
    const float COMBAT_PHASE_BUILD_TIME = 5f;

    float buildStartAlpha = 50f;
    float buildEndAlpha = 255f;
    float currentAlpha = 50f;

    bool isBuilding;

    public int maxHealth = 500;
	
	public int currentHealth;

	public int currencyCost = 10;

	public int idOfOwner;

	public PlayerStats player;
	public int team;
	
	public Transform snapPositionBottom;
	public Transform snapPositionLeft;
	public Transform snapPositionRight;
	public Transform snapPositionTop;
    public Transform snapPositionFront;
    public Transform snapPositionBack;
    public Material defaultMat;
    public Material placementModeMat;
    public Material buildTimeMat;

    public bool CanRotateX = true;
    public bool CanRotateY = true;

	public Collider[] cols;
	public Renderer rend;


	private void Start()
	{
        isBuilding = true;
        currentHealth = 1;
        buildTimeMat.color = new Color(buildTimeMat.color.r, buildTimeMat.color.g, buildTimeMat.color.b, buildStartAlpha);
        StartCoroutine(BuildTimeFadeTo(buildStartAlpha, BUILD_PHASE_BUILD_TIME));
	}

    void Update()
    {
    }

    IEnumerator BuildTimeFadeTo(float goalAlpha, float duration)
    {
        float alpha = buildTimeMat.color.a;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
        {
            Color newColor = new Color(1,1,1, Mathf.Lerp(alpha, goalAlpha, t));
            buildTimeMat.color = newColor;
            yield return null;
        }
    }

	public void OnDeath()
	{
		player.PropsOwnedByPlayer.Remove(this.gameObject);
		Destroy(gameObject);
	}

	public void TakeDamage(int GiverPunID, int damageTaken, Damager.DamageTypes damageType)
	{
		currentHealth = currentHealth - damageTaken;
        Debug.Log(GiverPunID);
        Debug.Log(PhotonView.Find(GiverPunID));
        PhotonView.Find(GiverPunID).GetComponent<PlayerStats>().dmgText.CreateFloatingText(damageTaken.ToString(), this.transform);

		if (currentHealth <= 0)
		{
			currentHealth = 0;
            StopAllCoroutines();
            OnDeath();
		}
	}



}
