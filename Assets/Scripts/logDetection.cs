using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class logDetection : MonoBehaviour
{
    Vector3 halfExtents = new Vector3(0.2f, 0.2f, 2f); // Adjust the values as needed
    float timeAfterFalling = 0;
    void Update()
    {
        // Define the half-extents of the box (width, height, depth) for detection

        // Cast a box-shaped ray forward from the player's position
        RaycastHit hit;
        Collider[] hitCollider = Physics.OverlapBox(transform.position, halfExtents, transform.rotation);
        if (hitCollider.Length != 0)
        {
            for(int i = 0; i < hitCollider.Length;i++)
            {
                if (hitCollider[i].name == "stopMove")
                {
                    this.gameObject.GetComponent<gyroPlayerMovement>().enabled = false;
                    timeAfterFalling += Time.deltaTime;
                    if (timeAfterFalling > 2)
                    {
                        timeAfterFalling = 0;
                        LoadBackToTrail.Load();
                    }
                }
            }
            //Debug.Log("Hitting");
        }
        else
        {
            int childCount = transform.childCount;


            // Populate the array with child GameObjects
            for (int i = 0; i < childCount; i++)
            {
                transform.GetChild(i).gameObject.transform.SetParent(null);
            }
            this.gameObject.GetComponent<gyroPlayerMovement>().enabled = false;
            Vector3 s = this.gameObject.transform.localScale;
            this.gameObject.transform.localScale = new Vector3(s.x, s.y, s.z) * 0.95f;
            timeAfterFalling += Time.deltaTime;
            if (timeAfterFalling > 2)
            {
                timeAfterFalling = 0;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
    void OnDrawGizmos()
    {
        //// Visualize the box cast
        //Gizmos.color = Color.yellow;
        //Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        //Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2);
    }

}
