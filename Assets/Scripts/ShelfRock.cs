using UnityEngine;

public class ShelfRock : MonoBehaviour
{
    public ShelfGameController shelfGameController;
    public int slotIndex = -1;
    public SpriteRenderer rockSpriteRenderer;
    public SpriteRenderer shadowSpriteRenderer;
    public SpriteRenderer shine;
    public SpriteRenderer glow;
    public SpriteRenderer face;
    public Sprite[] rockSprites;
    public Sprite[] faceSprites;
    public Color[] colors;

    public Rock rock;

    public bool isPlaced = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Hide()
    {
        if (isPlaced) return;
        rockSpriteRenderer.enabled = false;
        shadowSpriteRenderer.enabled = false;
        shine.enabled = false;
        glow.enabled = false;
        face.enabled = false;
    }

    public void ShowRock(Rock rock)
    {
        this.rock = rock;
        rockSpriteRenderer.enabled = true;
        shadowSpriteRenderer.enabled = true;
        shine.enabled = true;
        glow.enabled = true;
        face.enabled = true;

         if (rock.semiTransparent) {
            glow.enabled = true;
        }
        else {
            glow.enabled = false;
        }
        rockSpriteRenderer.sprite = rockSprites[rock.imageType];
        shadowSpriteRenderer.sprite = rockSprites[rock.imageType];
        face.sprite = faceSprites[(int)rock.emotion];
        switch (rock.emotion) {
            case Emotion.happy:
                face.GetComponent<Animator>().Play("HappyFace");
                break;
            case Emotion.sad:
                face.GetComponent<Animator>().Play("SadFace");
                break;
            case Emotion.angry:
                face.GetComponent<Animator>().Play("AngryFace");
                break;
            case Emotion.scared:
                face.GetComponent<Animator>().Play("ScaredFace");
                break;
        }
        rockSpriteRenderer.color = colors[rock.color];
        if (rock.semiTransparent) {
            rockSpriteRenderer.color = new Color(rockSpriteRenderer.color.r, rockSpriteRenderer.color.g, rockSpriteRenderer.color.b, 0.7f);
        }
        if (rock.shiny) {
            shine.enabled = true;
        }
        else {
            shine.enabled = false;
        }
    }

    public void PlaceRock(Rock rock)
    {
        this.rock = rock;
        ShowRock(rock);
        isPlaced = true;
    }

    void OnMouseEnter()
    {
        if (isPlaced) return;
        if (shelfGameController != null && !isPlaced)
        {
            ShowRock(shelfGameController.GetCurrentRock());
        }
        shelfGameController.RockHovered(slotIndex);
    }

    void OnMouseExit()
    {
        if (isPlaced) return;
        Hide();
        shelfGameController.RockUnHovered(slotIndex);
    }

    void OnMouseDown()
    {
        if (isPlaced) return;
        PlaceRock(shelfGameController.GetCurrentRock());
        shelfGameController.RockPlaced(slotIndex);
    }
}
