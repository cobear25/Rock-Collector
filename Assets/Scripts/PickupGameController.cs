using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class PickupGameController : MonoBehaviour
{
    public GameObject rockPrefab;
    public GameObject rockBox;
    public GameObject goToShelfButton;
    public GameObject instructionsPanel;
    List <PickupRock> pickupRocks = new List<PickupRock>();
    int maxSortingOrder = 0;
    const int MaxSavedRocks = 3;
    AudioSource audioSource;
    public AudioClip happySound;
    public AudioClip sadSound;
    public AudioClip angrySound;
    public AudioClip scaredSound;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        PopulateLevel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PopulateLevel() 
    {
        for (int i = 0; i < 120; i++)
        {
            var randomX = Random.Range(-18, 18);
            var randomY = Random.Range(-3, 10);
            var retries = 0;
            while (true && pickupRocks.Count > 0) {
                // loop until random isn't overlapping with any other pickup rock
                if (pickupRocks.Any(rock => Vector2.Distance(new Vector2(rock.transform.position.x, rock.transform.position.y), new Vector2(randomX, randomY)) < 2)) {
                    randomX = Random.Range(-18, 18);
                    randomY = Random.Range(-3, 10);
                    retries++;
                    if (retries > 100) {
                        break;
                    }
                }
                else {
                    break;
                }
            }

            var rock = Instantiate(rockPrefab, new Vector3(randomX, randomY, 0), Quaternion.identity);
            rock.transform.SetParent(transform);
            PickupRock pickupRock = rock.GetComponent<PickupRock>();
            pickupRock.shadowSpriteRenderer.sortingOrder = i;
            pickupRock.glow.sortingOrder = i;
            pickupRock.rockSpriteRenderer.sortingOrder = i + 1;
            pickupRock.face.sortingOrder = i + 2;
            pickupRock.shine.sortingOrder = i + 3;
            pickupRock.pickupGameController = this;
            maxSortingOrder = i;
            pickupRocks.Add(pickupRock);
        }
    }

    PickupRock hoveredRock;
    public void PickupRockHovered(PickupRock pickupRock)
    {
        hoveredRock = pickupRock;
        maxSortingOrder++;
        pickupRock.shadowSpriteRenderer.sortingOrder = maxSortingOrder;
        pickupRock.glow.sortingOrder = maxSortingOrder;
        pickupRock.rockSpriteRenderer.sortingOrder = maxSortingOrder + 1;
        pickupRock.shine.sortingOrder = maxSortingOrder + 3;
        pickupRock.face.sortingOrder = maxSortingOrder + 2;
    }

    public void RockPickedUp(PickupRock pickupRock)
    {
         switch (pickupRock.rock.emotion) {
            case Emotion.happy:
                audioSource.PlayOneShot(happySound);
                break;
            case Emotion.sad:
                audioSource.PlayOneShot(sadSound);
                break;
            case Emotion.angry:
                audioSource.PlayOneShot(angrySound);
                break;
            case Emotion.scared:
                audioSource.PlayOneShot(scaredSound);
                break;
        }
    }

    public void PickupRockUnHovered(PickupRock pickupRock)
    {
        if (hoveredRock == pickupRock)
        {
            hoveredRock = null;
        }
    }

    List<PickupRock> droppedRocks = new List<PickupRock>();
    public void droppedInBox(PickupRock pickupRock)
    {
        if (droppedRocks.Count == 0)
        {
            pickupRock.transform.position = new Vector2
            (
                rockBox.transform.position.x - 5,
                rockBox.transform.position.y
            );
        }
        else if (droppedRocks.Count == 1)
        {
            pickupRock.transform.position = rockBox.transform.position;
        }
        else if (droppedRocks.Count >= 2)
        {
            pickupRock.transform.position = new Vector2
            (
                rockBox.transform.position.x + 5,
                rockBox.transform.position.y
            );
        }
        droppedRocks.Add(pickupRock);
        SaveDroppedRocks();
        if (droppedRocks.Count == MaxSavedRocks)
        {
            goToShelfButton.SetActive(true);
        }
    }

    void SaveDroppedRocks()
    {
        // Keep only up to MaxSavedRocks
        while (droppedRocks.Count > MaxSavedRocks)
        {
            droppedRocks[2].transform.position = droppedRocks[1].transform.position;
            droppedRocks[1].transform.position = droppedRocks[0].transform.position;
            droppedRocks[0].transform.position = Vector2.zero;
            droppedRocks[0].isDropped = false;
            droppedRocks.RemoveAt(0);
        }

        for (int i = 0; i < MaxSavedRocks; i++)
        {
            string key = $"DroppedRock_{i}";
            if (i < droppedRocks.Count)
            {
                var rockData = droppedRocks[i].rock;
                var json = JsonUtility.ToJson(rockData);
                PlayerPrefs.SetString(key, json);
            }
            else
            {
                PlayerPrefs.DeleteKey(key);
            }
        }
        PlayerPrefs.Save();
    }

    public void GoToShelf()
    {
        SceneManager.LoadScene("ShelfScene");
    }

    public void HideShowInstructions()
    {
        instructionsPanel.SetActive(!instructionsPanel.activeSelf);
    }
}
