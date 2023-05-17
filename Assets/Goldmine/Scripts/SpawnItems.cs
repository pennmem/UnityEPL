using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEPL;
using static UnityEditor.FilePathAttribute;

public class SpawnItems : EventMonoBehaviour {
    public float xMinRange = 1.5f;
    public float xMaxRange = 31.5f;
    public float zMinRange = 1.5f;
    public float zMaxRange = 31.5f;
    public GameObject goldObject; // prefab to spawn
    public GameObject[] gemObjects; // prefabs to spawn
    private GameObject[] items;
    private int numItemsSpawned = 0;

    protected override void AwakeOverride() { }

    public void SpawnGold(int nItems) {
        Do(SpawnGoldHelper, nItems);
    }
    public void SpawnGoldHelper(int nItems) {
        manager.eventReporter.ReportScriptedEventMB("goldSpawned", new() { { "nItems", nItems } });
        for (int i = 0; i < nItems; i++) {
            SpawnItem(goldObject);
        }
    }

    

    public void SpawnGems(int nItems) {
        if (nItems > gemObjects.Length) {
            ErrorNotifier.Error(new InvalidOperationException("The game is trying to spawn repeat gems."));
        }

        manager.eventReporter.ReportScriptedEvent("gemsSpawned", new Dictionary<string, object> { { "nItems", nItems } });
        var indices = Enumerable.Range(0, gemObjects.Length).ToList();
        indices.Shuffle();
        for (int i = 0; i < nItems; i++) {
            SpawnItem(gemObjects[indices[i]], 2);
        }
    }


    public GameObject SpawnItem(GameObject item, float scaleSize = 1, Vector3? position = null, Quaternion? rotation = null) {
        Vector3 spawnPosition = new Vector3();
        GameObject spawnedItem;

        if (position.HasValue) {
            spawnedItem = Instantiate(item, position.Value, rotation ?? item.transform.rotation);
        } else {
            // Randomly generate a spawn position that doesn't collide with other objects
            int nCollisions = 1;
            while (nCollisions > 0) {
                spawnPosition.x = InterfaceManager.rnd.Value.NextFloat(xMinRange, xMaxRange);
                spawnPosition.y = item.transform.GetComponent<BoxCollider>().size.y;
                spawnPosition.z = InterfaceManager.rnd.Value.NextFloat(xMinRange, xMaxRange);

                Collider[] hitColliders = Physics.OverlapBox(spawnPosition + new Vector3(0.0f, 0.55f, 0.0f),
                                                             new Vector3(0.5f, 0.5f, 0.5f));
                nCollisions = hitColliders.Length;
            }
            spawnedItem = Instantiate(item, spawnPosition, rotation ?? item.transform.rotation);
        }


        // Spawn the game object
        //GameObject spawnedItem = Instantiate(item, spawnPosition, item.transform.rotation);
        spawnedItem.name = item.name;
        spawnedItem.transform.localScale = spawnedItem.transform.localScale * scaleSize;
        spawnedItem.GetComponent<WorldDataReporter>().reportingID = item.name + numItemsSpawned.ToString("D4");
        spawnedItem.AddComponent<PickupItem>();

        // Make the parent the spawner so hierarchy doesn't get super messy
        spawnedItem.transform.parent = gameObject.transform;

        // Misc
        numItemsSpawned++;
        manager.eventReporter.ReportScriptedEvent(item.name + "Location", new Dictionary<string, object> {
                {"reportingId", spawnedItem.GetComponent<WorldDataReporter>().reportingID},
                { "positionX", spawnPosition.x },
                { "positionZ", spawnPosition.z }
            });

        return spawnedItem;
    }

    public void HideItem(GameObject item) {
        item.GetComponentInChildren<Renderer>().enabled = false;
    }

    public void HideItems() {
        items = GameObject.FindGameObjectsWithTag("Pickups");
        foreach (GameObject item in items) {
            HideItem(item);
        }
    }

    public void UnhideItem(GameObject item) {
        item.GetComponentInChildren<Renderer>().enabled = true;
    }

    public void UnhideItems() {
        items = GameObject.FindGameObjectsWithTag("Pickups");
        foreach (GameObject item in items) {
            UnhideItem(item);
        }
    }

    public bool isItemHidden(GameObject item) {
        return !item.GetComponentInChildren<Renderer>().enabled;
    }

    public void DestroyItems() {
        items = GameObject.FindGameObjectsWithTag("Pickups");
        foreach (GameObject item in items) {
            Destroy(item);
        }
    }

    public GameObject[] GetItems() {
        //items = GameObject.FindGameObjectsWithTag("Pickups");
        return GameObject.FindGameObjectsWithTag("Pickups");
    }

    public GameObject[] GetVisibleItems() {
        //items = GameObject.FindGameObjectsWithTag("Pickups");
        return GameObject.FindGameObjectsWithTag("Pickups")
            .Where(x => x.GetComponentInChildren<Renderer>().enabled)
            .ToArray();
    }
}