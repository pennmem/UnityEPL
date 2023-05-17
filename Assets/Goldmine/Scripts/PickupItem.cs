using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PickupItem : MonoBehaviour
{
    public bool isPickedUp { get; private set; } = false;
    public float pickupTimeMs { get; private set; } = float.MinValue;

    private DateTime itemCreationTime = DateTime.MinValue;

    private void Update()
    {
        const float RPM = 10;

        if (transform.name != "gold")
        {
            float yRot = RPM * 6f * Time.deltaTime;
            transform.Rotate(0, yRot, 0, Space.World);
        }
    }

    public void InitPickup()
    {
        isPickedUp = false;
        itemCreationTime = DateTime.Now;
    }

    public void Pickup()
    {
        isPickedUp = true;
        pickupTimeMs = (DateTime.Now - itemCreationTime).Ticks / TimeSpan.TicksPerMillisecond;
    }
}
