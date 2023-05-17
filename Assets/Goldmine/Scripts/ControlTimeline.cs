using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEPL;

public class ControlTimeline : MonoBehaviour {
    protected float xMin;
    protected float xMax;
    protected List<GameObject> itemsOnTimeline = new List<GameObject>();
    private System.Random rng = new System.Random();
    public float scale = 1;

    void Start() {
        Vector3[] worldCorners = new Vector3[4];
        transform.GetComponent<RectTransform>().GetWorldCorners(worldCorners);
        xMin = worldCorners[1].x; // x-coord of top left corner
        xMax = worldCorners[2].x; // x-coord of top right corner
    }

    public void SetItemPositionOnTimeline(Transform item) {
        float zPos = item.position.z;
        float yPos = transform.position.y;
        float xPos = item.position.x;

        xPos = Mathf.Min(xMax, xPos);
        xPos = Mathf.Max(xMin, xPos);

        item.position = new Vector3(xPos, yPos, zPos);
    }

    public float GetItemTimeNormalized(Transform item) {
        if (item.position.y == transform.position.y) {
            float width = xMax - xMin;
            return (item.position.x - xMin) / width;
        } else {
            return -1;
        }
    }

    public float GetItemTime(Transform item) {
        if (item.position.y == transform.position.y) {
            float width = xMax - xMin;
            return (item.position.x - xMin) / width * scale;
        } else {
            return -1;
        }
    }

    // By default, this returns a normalized value (between 0 and 1)
    // TODO: JPB: (feature) Add actualTime (store when each item is found)
    public List<Dictionary<string, object>> GetItemTimes() {
        var items = new List<Dictionary<string, object>>();
        foreach (var item in itemsOnTimeline.ToList()) // use ToList to make a copy for safe removing
        {
            if (item == null) // Remove and skip deleted items
            {
                itemsOnTimeline.Remove(item);
                continue;
            }
            float itemTime = GetItemTime(item.transform);
            items.Add(new Dictionary<string, object> { { "name", item.name }, { "chosenTime", itemTime }, { "actualTime", 0 } });
        }

        return items;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.GetComponent<DragDrop>() != null) {
            itemsOnTimeline.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.GetComponent<DragDrop>() != null) {
            Debug.Log("OnTriggerExit: " + other.gameObject.name); ;
            itemsOnTimeline.Remove(other.gameObject);
        }
    }

    // Spawn and get timeline items

    public static void SetLayerRecursively(Transform obj, int layer) {
        obj.gameObject.layer = layer;

        foreach (Transform child in obj) {
            SetLayerRecursively(child, layer);
        }
    }

    public GameObject[] GetTimelineItems() {
        return GameObject.FindGameObjectsWithTag("TimelineItem");
    }

    protected void SpawnTimelineItem(GameObject item) {
        GameObject spawnedItem = Instantiate(item, new Vector3(0, 0, 0), item.transform.rotation) as GameObject;
        spawnedItem.name = item.name;
        spawnedItem.transform.SetParent(transform.parent, false);
        spawnedItem.transform.localScale = spawnedItem.transform.localScale * 100;
        spawnedItem.tag = "TimelineItem";
        SetLayerRecursively(spawnedItem.transform, LayerMask.NameToLayer("UI"));

        spawnedItem.GetComponent<Collider>().enabled = true;
        spawnedItem.GetComponent<Collider>().isTrigger = true;
        var dragDrop = spawnedItem.AddComponent<DragDrop>();
        dragDrop.camera = GameObject.Find("Overlay Camera").GetComponent<Camera>();
    }

    public void SpawnTimelineItems(GameObject item, int numItems) {
        for (int i = 0; i < numItems; ++i) {
            SpawnTimelineItem(item);
        }

        MoveItemsToTimeline();
    }

    public void SpawnTimelineItems(GameObject[] items) {
        foreach (var item in items) {
            SpawnTimelineItem(item);
        }

        MoveItemsToTimeline();
    }

    protected void MoveItemsToTimeline() {
        // Get items
        var items = GetTimelineItems();

        // Get space to put the items on timeline
        Vector3[] itemAreaCorners = new Vector3[4];
        transform.parent.Find("ItemArea").GetComponent<RectTransform>().GetWorldCorners(itemAreaCorners);
        float xMin = itemAreaCorners[1].x; // x-coord of top left corner
        float xMax = itemAreaCorners[2].x; // x-coord of top right corner
        float yMin = itemAreaCorners[0].y; // y-coord of bottom left corner
        float yMax = itemAreaCorners[1].y; // y-coord of top left corner
        float width = xMax - xMin;
        float height = yMax - yMin;

        // Control the rows and columns shown
        const int rows = 2;
        int cols = Mathf.CeilToInt(items.Length / 2f);
        int extra = items.Length % 2;

        // Scale values for item placement
        float xScale = width / (cols - 1);
        float yScale = height / (rows - 1);

        // Generate the positions
        var positions = new List<Vector3>();
        foreach (int i in Enumerable.Range(0, cols)) {
            foreach (int j in Enumerable.Range(0, rows)) {
                Vector3 offset = new Vector3(i * xScale, j * yScale, 0);
                positions.Add(itemAreaCorners[0] + offset);
            }
        }

        // Randomize positions and move the items
        positions.Shuffle();
        foreach (int i in Enumerable.Range(0, items.Count())) {
            items[i].transform.position = positions[i];
        }
    }
}
