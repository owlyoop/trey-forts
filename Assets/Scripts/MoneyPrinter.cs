using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyPrinter : FortwarsProp
{

    public int MoneyPerTick = 1;
    public float TickDelay = 0.5f;
    float timer = 0f;
    float nextActionTime = 0f;

    private void Update()
    {
        timer = Time.time;
        if (Time.time > nextActionTime)
        {
            nextActionTime = Time.time + TickDelay;
            player.OnChangeCurrencyAmount(player.currentCurrency + MoneyPerTick);
        }
    }

}
