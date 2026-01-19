using UnityEngine;

public class TestingMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool useWorldSpace = true;
    public bool moving = false;
    // Update is called once per frame
    void Update()
    {
        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) { x -= 1f; moving = true; }
        else if (Input.GetKey(KeyCode.RightArrow)) { x += 1f; moving = true; }
        else if (Input.GetKey(KeyCode.UpArrow)) { z += 1f; moving = true; }
        else if (Input.GetKey(KeyCode.DownArrow)) { z -= 1f; moving = true; }
        else moving = false;
            Vector3 moveDirection = new Vector3(x, 0f, z).normalized;
        if (useWorldSpace)
        {
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.Self);
        }
        if(moving)
        Debug.LogError("TestingMovement script is active.");
    }
}
