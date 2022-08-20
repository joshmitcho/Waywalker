using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer rend;

    public List<Sprite> sprites;
    public int fps;
    public bool hasMultipleFrames = false;

    int current = 0;
    float timeLapsed;

    public void LoadSprites(List<Sprite> sp)
    {
        hasMultipleFrames = true;
        fps = sp.Count;
        sprites = sp;
        rend.sprite = sprites[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (hasMultipleFrames)
        {
            timeLapsed += Time.deltaTime;
            if (timeLapsed > 1.0f / fps)
            {
                current++;
                rend.sprite = sprites[current % sprites.Count];
                timeLapsed = 0f;
            }
        }

    }
}
