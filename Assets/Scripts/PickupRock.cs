using UnityEngine;

[System.Serializable]
public struct Rock {
    public int imageType;
    public int color;
    public bool shiny;
    public bool semiTransparent;
    public Emotion emotion;
    public int slotIndex;
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
    private bool isDragging = false;
    private Vector3 dragOffset;
    private float dragZ;
    public bool isDropped = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rock = new Rock();
        if (Random.Range(0, 10) == 0) {
            // rarer image types
            rock.imageType = Random.Range(0, 4);
        } else {
            rock.imageType = Random.Range(4, rockSprites.Length);
        }
        // weighted pick where lower indices are more common and higher are rarer
        // geometric decay keeps tail possible but uncommon
        float decay = 0.9f; // 0.5-0.8 works well; lower = rarer tail
        float totalWeight = 0f;
        for (int i = 0; i < colors.Length; i++) {
            totalWeight += Mathf.Pow(decay, i);
        }
        float pick = Random.value * totalWeight;
        float accum = 0f;
        for (int i = 0; i < colors.Length; i++) {
            accum += Mathf.Pow(decay, i);
            if (pick <= accum) { rock.color = i; break; }
        }
        // rock.color = Random.Range(0, colors.Length);
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
        if (!isDragging) {
            pickupGameController.PickupRockUnHovered(this);
            transform.localScale = originalScale;
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
        // Determine correct depth from camera to object for ScreenToWorldPoint
        dragZ = Camera.main.WorldToScreenPoint(transform.position).z;
        var mousePoint = Input.mousePosition;
        mousePoint.z = dragZ;
        var mouseWorld = Camera.main.ScreenToWorldPoint(mousePoint);
        dragOffset = transform.position - mouseWorld;
        // Ensure this rock is visually on top while dragging
        pickupGameController.PickupRockHovered(this);
        pickupGameController.RockPickedUp(this);
    }

    void OnMouseDrag()
    {
        if (isDropped) return;
        if (!isDragging) return;
        var mousePoint = Input.mousePosition;
        mousePoint.z = dragZ;
        var mouseWorld = Camera.main.ScreenToWorldPoint(mousePoint);
        var target = mouseWorld + dragOffset;
        transform.position = new Vector3(target.x, target.y, 0f);
    }

    void OnMouseUp()
    {
        isDragging = false;
        // Check if dropped over the rockBox
        if (pickupGameController != null && pickupGameController.rockBox != null)
        {
            bool overlaps = false;

            // Prefer collider-based overlap if both have colliders
            var thisCollider = GetComponent<CircleCollider2D>();
            var boxCollider = pickupGameController.rockBox.GetComponent<BoxCollider2D>();
            if (thisCollider != null && boxCollider != null)
            {
                overlaps = thisCollider.bounds.Intersects(boxCollider.bounds);
            }
            else
            {
                // Fallback to renderer bounds
                var thisRenderer = rockSpriteRenderer != null ? rockSpriteRenderer : GetComponentInChildren<SpriteRenderer>();
                var boxRenderer = pickupGameController.rockBox.GetComponentInChildren<SpriteRenderer>();
                if (thisRenderer != null && boxRenderer != null)
                {
                    overlaps = thisRenderer.bounds.Intersects(boxRenderer.bounds);
                }
            }

            if (overlaps)
            {
                isDropped = true;
                pickupGameController.droppedInBox(this);
            }
        }
    }

}
