using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBG : MonoBehaviour
{
    Camera cam;
    Vector3 startPos;
    float length;

    [Header("스크롤 속도")]
    public float parallaxSpeedX = 1.0f;
    public float parallaxSpeedY = 1.0f;
    [Header("배경 오프셋")]
    public Vector2 offset = Vector2.zero;


    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        startPos = cam.transform.position + new Vector3(offset.x, offset.y, 0);
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        float distanceX = cam.transform.position.x * parallaxSpeedX;
        float distanceY = cam.transform.position.y * parallaxSpeedY;
        float movement = cam.transform.position.x * (1f - parallaxSpeedX);

        transform.position = new Vector3(startPos.x + distanceX, startPos.y + distanceY, transform.position.z);

        if (movement > startPos.x + length)
            startPos.x += length;
        else if (movement < startPos.x - length)
            startPos.x -= length;
    }
}
