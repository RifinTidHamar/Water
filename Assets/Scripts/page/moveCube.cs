using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveCube : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool left = Input.GetKey(KeyCode.LeftArrow);
        bool right = Input.GetKey(KeyCode.RightArrow);
        bool up = Input.GetKey(KeyCode.UpArrow);
        bool down = Input.GetKey(KeyCode.DownArrow);

        if(left)
        {
            this.gameObject.transform.position += new Vector3(-0.25f, 0, 0) * Time.deltaTime;
        }
        if(right) 
        {
            this.gameObject.transform.position += new Vector3(0.25f, 0, 0) * Time.deltaTime;
        }
        if (up) 
        {
            this.gameObject.transform.position += new Vector3(0, 0.25f, 0) * Time.deltaTime;
        }
        if (down)
        {
            this.gameObject.transform.position += new Vector3(0, -0.25f, 0) * Time.deltaTime;
        }
    }
}
