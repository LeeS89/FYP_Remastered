using System.Collections.Generic;
using UnityEngine;
using static OVRPlugin;

public class TestingThrowaway : MonoBehaviour
{
   

    public bool _test = false;

    // Update is called once per frame
    void Update()
    {
        if (_test)
        {
            _test = false;

            List<int> stepsToTry = new List<int>();
            int randomIndex = Random.Range(4, 13);
            Debug.LogError("Random Index: " + randomIndex);
            int temp = randomIndex;
            while (temp >= 4)
            {
                stepsToTry.Add(temp);
                temp--;
            }
            temp = randomIndex + 1;
            while (temp < 13)
            {
                stepsToTry.Add(temp);
                temp++;
            }

            foreach (int step in stepsToTry)
            {
                Debug.LogError("Step: " + step);
            }
        }
    }
}
