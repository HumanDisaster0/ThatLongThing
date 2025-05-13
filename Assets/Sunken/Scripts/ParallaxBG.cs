using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBG : MonoBehaviour
{
    Camera cam;
    Vector2 startPos;
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
        startPos = new Vector2(cam.transform.position.x, cam.transform.position.y) + offset;
        length = GetComponent<SpriteRenderer>().size.x;

        transform.position = new Vector3(startPos.x, startPos.y, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        float distanceX = cam.transform.position.x * parallaxSpeedX;
        float distanceY = cam.transform.position.y * parallaxSpeedY;
        float movement = cam.transform.position.x * (1-parallaxSpeedX);

        transform.position = new Vector3(startPos.x + distanceX, startPos.y + distanceY, transform.position.z);

        if (movement > startPos.x + length)
            startPos.x += length;
        else if (movement < startPos.x - length)
            startPos.x -= length;
    }
}
