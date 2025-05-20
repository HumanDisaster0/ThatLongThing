using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class instTwoButton : MonoBehaviour
{
    public GameObject selBtnPrefab;
    // Start is called before the first frame update
    void Start()
    {
        GameObject instance = Instantiate(selBtnPrefab, transform.position, Quaternion.identity);
        //instance.
    }
        // Update is called once per frame
        void Update()
    {
        
    }
}
