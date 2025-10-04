using UnityEngine;

public struct Rock {
    public int imageType;
    public int color;
    public bool shiny;
    public bool semiTransparent;
    public Emotion emotion;
}

public enum Emotion {
    happy, sad, angry, scared
}

public class PickupRock : MonoBehaviour
{
    public SpriteRenderer rockSpriteRenderer;
    public SpriteRenderer shadowSpriteRenderer;
    public SpriteRenderer shine;
    public SpriteRenderer glow;
    public SpriteRenderer face;
    public Sprite[] rockSprites;
    public Sprite[] faceSprites;
    public Color[] colors;

    public PickupGameController pickupGameController;
    public Rock rock;
    private Vector3 originalScale;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rock = new Rock();
        rock.imageType = Random.Range(0, rockSprites.Length);
        rock.color = Random.Range(0, colors.Length);
        rock.shiny = Random.Range(0, 15) == 1;
        rock.semiTransparent = Random.Range(0, 8) == 1;
        rock.emotion = (Emotion)Random.Range(0, 4);
        if (rock.semiTransparent) {
            glow.enabled = true;
        }
        else {
            glow.enabled = false;
        }
        rockSpriteRenderer.sprite = rockSprites[rock.imageType];
        shadowSpriteRenderer.sprite = rockSprites[rock.imageType];
        face.sprite = faceSprites[(int)rock.emotion];
        rockSpriteRenderer.color = colors[rock.color];
        if (rock.semiTransparent) {
            rockSpriteRenderer.color = new Color(rockSpriteRenderer.color.r, rockSpriteRenderer.color.g, rockSpriteRenderer.color.b, 0.7f);
        }
        if (rock.shiny) {
            shine.enabled = true;
            // shine.GetComponent<Animator>().SetFloat("cycleOffset", Random.Range(0, 0.5f));
        }
        else {
            shine.enabled = false;
        }

        // Store the original scale for hover effects
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Called when the mouse enters the collider
    void OnMouseEnter()
    {
        // Scale up by 0.2 when hovering
        transform.localScale = originalScale + new Vector3(0.3f, 0.3f, 0.3f);
        pickupGameController.PickupRockHovered(this);
    }

    // Called when the mouse exits the collider
    void OnMouseExit()
    {
        pickupGameController.PickupRockUnHovered(this);
        transform.localScale = originalScale;
    }

}
