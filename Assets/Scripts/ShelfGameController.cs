using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class ShelfGameController : MonoBehaviour
{
    public ShelfRock[] shelfRocks;
    public int currentRock = 0;
    public TextMeshPro[] rowScoreTexts;
    public TextMeshPro[] colScoreTexts;
    public TextMeshPro totalText;
    public TextMeshProUGUI totalScoreText;
    public GameObject gameOverPanel;
    public ShelfRock[] shelfRockPreviews;
    public GameObject instructionsPanel;
    
    int[] rowBaseScores = new int[3];
    int[] colBaseScores = new int[5];
    int[] rowMultipliers = new int[3];
    int[] colMultipliers = new int[5];
    int[] rowScores = new int[3];
    int[] colScores = new int[5];

    Rock[] droppedRocks = new Rock[3];
    List<Rock> placedRocks = new List<Rock>();
    List<Rock> tempPlacedRocks = new List<Rock>();
    int placedThisRound = 0;
    AudioSource audioSource;
    public AudioClip happySound;
    public AudioClip sadSound;
    public AudioClip angrySound;
    public AudioClip scaredSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        for (int i = 0; i < shelfRocks.Length; i++)
        {
            var shelfRock = shelfRocks[i];
            shelfRock.Hide();
            shelfRock.shelfGameController = this;
            shelfRock.slotIndex = i;
        }

        // Load previously placed rocks per slot (supports multiple rounds and preserves positions)
        placedRocks.Clear();
        for (int i = 0; i < shelfRocks.Length; i++)
        {
            string placedKeyForSlot = $"PlacedRockSlot_{i}";
            if (!PlayerPrefs.HasKey(placedKeyForSlot)) continue;
            var rock = JsonUtility.FromJson<Rock>(PlayerPrefs.GetString(placedKeyForSlot));
            rock.slotIndex = i;
            placedRocks.Add(rock);
            tempPlacedRocks.Add(rock);
            shelfRocks[i].PlaceRock(rock);
        }

        // New round starts from first dropped rock
        currentRock = 0;
        placedThisRound = 0;

        // Load rocks picked up in the current round (always exactly 3)
        for (int i = 0; i < 3; i++)
        {
            string key = $"DroppedRock_{i}";
            if (PlayerPrefs.HasKey(key))
            {
                droppedRocks[i] = JsonUtility.FromJson<Rock>(PlayerPrefs.GetString(key));
            }
            shelfRockPreviews[i].ShowRock(droppedRocks[i]);
            shelfRockPreviews[i].isPlaced = true;
        }
        CalculateScores();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Rock GetCurrentRock()
    {
        return droppedRocks[currentRock];
    }

    public void RockHovered(int slotIndex)
    {
        var tempRock = shelfRocks[slotIndex].rock;
        tempRock.slotIndex = slotIndex;
        tempPlacedRocks.Add(tempRock);
        CalculateScores();
    }

    public void RockUnHovered(int slotIndex)
    {
        tempPlacedRocks.RemoveAt(tempPlacedRocks.Count - 1);
        CalculateScores();
    }

    public void RockPlaced(int slotIndex)
    {
        tempPlacedRocks.RemoveAt(tempPlacedRocks.Count - 1);
        var rock = droppedRocks[currentRock];
        rock.slotIndex = slotIndex;
        droppedRocks[currentRock] = rock;
        placedRocks.Add(rock);
        tempPlacedRocks.Add(rock);
        // Save rock to its specific slot immediately
        string placedKeyForSlot = $"PlacedRockSlot_{slotIndex}";
        PlayerPrefs.SetString(placedKeyForSlot, JsonUtility.ToJson(rock));
        PlayerPrefs.Save();
        currentRock++;
        placedThisRound++;
        if (placedThisRound == 3)
        {
            if (placedRocks.Count < 15)
            {
                Invoke("GoToCollectScene", 0.3f);
            }
            else
            {
                GameOver();
            }
        }
        switch (rock.emotion) {
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
        CalculateScores();
    }

    void GoToCollectScene()
    {
        SceneManager.LoadScene("CollectScene");
    }

    void GameOver()
    {
        gameOverPanel.SetActive(true);
        totalScoreText.text = "Total Score: " + GetTotalScore().ToString();
    }

    void CalculateScores()
    {
        var rowOne = tempPlacedRocks.Where(rock => rock.slotIndex < 5).ToList();
        var rowTwo = tempPlacedRocks.Where(rock => rock.slotIndex >= 5 && rock.slotIndex < 10).ToList();
        var rowThree = tempPlacedRocks.Where(rock => rock.slotIndex >= 10).ToList();

        var colOne = tempPlacedRocks.Where(rock => rock.slotIndex % 5 == 0).ToList();
        var colTwo = tempPlacedRocks.Where(rock => rock.slotIndex % 5 == 1).ToList();
        var colThree = tempPlacedRocks.Where(rock => rock.slotIndex % 5 == 2).ToList();
        var colFour = tempPlacedRocks.Where(rock => rock.slotIndex % 5 == 3).ToList();
        var colFive = tempPlacedRocks.Where(rock => rock.slotIndex % 5 == 4).ToList();

        rowBaseScores[0] = GetCollectionScore(rowOne, 5).x;
        rowBaseScores[1] = GetCollectionScore(rowTwo, 5).x;
        rowBaseScores[2] = GetCollectionScore(rowThree, 5).x;

        colBaseScores[0] = GetCollectionScore(colOne, 3).x;
        colBaseScores[1] = GetCollectionScore(colTwo, 3).x;
        colBaseScores[2] = GetCollectionScore(colThree, 3).x;
        colBaseScores[3] = GetCollectionScore(colFour, 3).x;
        colBaseScores[4] = GetCollectionScore(colFive, 3).x;

        rowMultipliers[0] = GetCollectionScore(rowOne, 5).y;
        rowMultipliers[1] = GetCollectionScore(rowTwo, 5).y;
        rowMultipliers[2] = GetCollectionScore(rowThree, 5).y;

        colMultipliers[0] = GetCollectionScore(colOne, 3).y;
        colMultipliers[1] = GetCollectionScore(colTwo, 3).y;
        colMultipliers[2] = GetCollectionScore(colThree, 3).y;
        colMultipliers[3] = GetCollectionScore(colFour, 3).y;
        colMultipliers[4] = GetCollectionScore(colFive, 3).y;

        rowScores[0] = GetCollectionScore(rowOne, 5).z;
        rowScores[1] = GetCollectionScore(rowTwo, 5).z;
        rowScores[2] = GetCollectionScore(rowThree, 5).z;

        colScores[0] = GetCollectionScore(colOne, 3).z;
        colScores[1] = GetCollectionScore(colTwo, 3).z;
        colScores[2] = GetCollectionScore(colThree, 3).z;
        colScores[3] = GetCollectionScore(colFour, 3).z;
        colScores[4] = GetCollectionScore(colFive, 3).z;
        UpdateAllScoreTexts();
    }

    Vector3Int GetCollectionScore(List<Rock> rocks, int maxLength)
    {
        int score = 0;
        foreach (var rock in rocks)
        {
            score += rock.color;
            if (rock.shiny)
            {
                score += 3;
            }
            if (rock.semiTransparent)
            {
                score += 2;
            }
            if (rock.color < 4)
            {
                score += 1;
            }
        }

        int multiplier = 1;

        if (rocks.Count() == maxLength)
        {

            if (rocks.Select(r => r.color).Distinct().Count() == rocks.Count())
            {
                multiplier += 1;
            }
            if (rocks.Select(r => r.emotion).Distinct().Count() == rocks.Count())
            {
                multiplier += 1;
            }
            if (rocks.Select(r => r.imageType).Distinct().Count() == rocks.Count())
            {
                multiplier += 1;
            }
            // if rocks are all same color, multiplier + 1
            if (rocks.All(r => r.color == rocks[0].color))
            {
                multiplier += 1;
            }
            // if rocks are all same emotion, multiplier + 1
            if (rocks.All(r => r.emotion == rocks[0].emotion))
            {
                multiplier += 1;
            }
            // if rocks are all same image type, multiplier + 1
            if (rocks.All(r => r.imageType == rocks[0].imageType))
            {
                multiplier += 1;
            }
            // if all rocks are semi transparent, multiplier + 1
            if (rocks.All(r => r.semiTransparent))
            {
                multiplier += 1;
            }
            // if all rocks are shiny, multiplier + 1
            if (rocks.All(r => r.shiny))
            {
                multiplier += 1;
            }
        }

        return new Vector3Int(score, multiplier, score * multiplier);
    }

    void UpdateRowScoreTexts()
    {
        for (int i = 0; i < 3; i++)
        {
            // rowScoreTexts[i].text = rowScores[i].ToString();
            rowScoreTexts[i].text = rowBaseScores[i].ToString() + "x" + rowMultipliers[i].ToString();
        }
    }

    void UpdateColScoreTexts()
    {
        for (int i = 0; i < 5; i++)
        {
            // colScoreTexts[i].text = colScores[i].ToString();
            colScoreTexts[i].text = colBaseScores[i].ToString() + "x" + colMultipliers[i].ToString();
        }
    }

    int GetTotalScore()
    {
        return rowScores.Sum() + colScores.Sum();
    }

    void UpdateAllScoreTexts()
    {
        UpdateRowScoreTexts();
        UpdateColScoreTexts();
        totalText.text = GetTotalScore().ToString();
    }

    public void PlayAgain()
    {
        PlayerPrefs.DeleteAll();
        GoToCollectScene();
    }

    public void HideShowInstructions()
    {
        instructionsPanel.SetActive(!instructionsPanel.activeSelf);
    }
}
