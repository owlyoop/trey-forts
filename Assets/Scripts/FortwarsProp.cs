using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FortwarsProp : MonoBehaviour, IDamagable
{

  
    float buildStartAlpha = 0.1f;
    float buildEndAlpha = 1f;
    float currentAlpha = 1f;

    float HealthTickDelay = 0.2f;
    float nextTickTime = 0f;
    int HealthPerTick = 10;

    bool isHealing; // Prop starts at 1hp and heals over time when it is built.
    bool isBuilding; // Prop starts off with colliders disabled

    public int maxHealth = 500;
    int remainingBuildHealth;
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
    public bool GhostMode;

	public Collider[] cols;
	public Renderer rend;

    float startTime;
    


	private void Start()
	{
        Debug.Log("prop start");
        currentHealth = 1;
        remainingBuildHealth = maxHealth;
        startTime = Time.time;
        isBuilding = true;
	}

    public void BuildTimeStart()
    {
        if (!GhostMode)
        {
            Debug.Log("prop building");
            currentHealth = 1;
            isBuilding = true;
            rend.material = buildTimeMat;
            foreach (Collider col in cols)
            {
                col.enabled = false;
            }
        }
    }

    void Update()
    {
        if (!GhostMode)
        {
            if (isBuilding)
            {
                if (Time.time > nextTickTime)
                {
                    nextTickTime = Time.time + HealthTickDelay;
                    remainingBuildHealth -= HealthPerTick;
                    currentHealth += HealthPerTick;
                    Color newColor = new Color(1, 1, 1, Mathf.Lerp(0.0f, 1f, (float)currentHealth/(float)maxHealth));
                    rend.material.color = newColor;
                }

                if (remainingBuildHealth <= 0 || currentHealth >= maxHealth)
                {
                    Debug.Log("buildtime over");
                    isBuilding = false;
                    rend.material = defaultMat;
                    foreach (Collider col in cols)
                    {
                        col.enabled = true;
                    }
                    if (currentHealth > maxHealth)
                        currentHealth = maxHealth;
                }
            }
        }
    }

   

	public void OnDeath()
	{
		player.PropsOwnedByPlayer.Remove(this.gameObject);
		Destroy(gameObject);
	}

	public void TakeDamage(int damageTaken, Damager.DamageTypes damageType, PlayerStats giver, Vector3 damageSourceLocation)
	{
		currentHealth = currentHealth - damageTaken;
        player.dmgText.CreateFloatingText(damageTaken.ToString(), this.transform);

		if (currentHealth <= 0)
		{
			currentHealth = 0;
            OnDeath();
		}
	}



}
