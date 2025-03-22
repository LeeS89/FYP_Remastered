using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public class TestAgent : MonoBehaviour
{
    public NavMeshAgent _agent;
    public Transform _destination;
    public List<Transform> _wayPoints;
    private WaitUntil _waitUntilDestinationReached;
    private Func<bool> hasReachedDestination;
    private int index = 0;

    private Vector3 _direction;
    private float distance;

    private void Awake()
    {
        hasReachedDestination = CheckDestinationReached;
        _waitUntilDestinationReached = new WaitUntil(hasReachedDestination);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(TraverseWayPoints());
        //_agent.SetDestination(_wayPoints[0].position);
    }


    IEnumerator TraverseWayPoints()
    {
        while (true)
        {

            index = GetNextDestination();

            _agent.SetDestination(_wayPoints[index].position);

            yield return _waitUntilDestinationReached;
            //_walk = false;

            // Calculating the direction vector from the agent to the destination
            Vector3 directionToFace = _wayPoints[index].forward;

            //New rotation based on the direction vector
            Quaternion targetRotation = Quaternion.LookRotation(directionToFace);

            while (Quaternion.Angle(_agent.transform.rotation, targetRotation) > 2.0f)
            {
                // Smoothly rotating the agent towards the target rotation
                _agent.transform.rotation = Quaternion.Slerp(_agent.transform.rotation, targetRotation, Time.deltaTime * 3.0f);
                yield return null;
            }
            
            /*float _delayTime = GetRandomDelay();
            float elapsedTime = 0.0f;*/
            //Debug.LogError("Delay Time: " + _delayTime);
            /*while (elapsedTime < _delayTime)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }*/

            //if (_wayPoints.Count > 1)
            //{
            //    /*_walk = false;*/
            //    _agent.enabled = false;
            //    //SetAgentCarving(true);
            //    //_animation.SetLook();
            //}
            /*else
            {
                _stateController.RequestStateChange(_stationaryState);
            }*/

          /*  yield return _pauseDelay;
            //SetAgentCarving(false);

            yield return _timeToEnableAgent;
            //_shouldRotate = false;*/
            //_animation.SetStrafe(false);

            //_agent.enabled = true;


            //SwapFirstAndLastIndex();
        }
    }

    private int GetNextDestination()
    {
        int newIndex = 0;
        int _nextIndex = UnityEngine.Random.Range(0, _wayPoints.Count - 1);

        if (_wayPoints[_nextIndex].gameObject.activeInHierarchy)
        {
            newIndex = _nextIndex;
            //return _nextIndex;
        }
        else
        {
            for (int i = 0; i < _wayPoints.Count; i++)
            {
                if (_wayPoints[i].gameObject.activeInHierarchy)
                {
                    newIndex = i;
                    break;
                }
            }
        }
        return newIndex;
    }
    private bool CheckDestinationReached()
    {
        // if (_destination[index].gameObject.activeInHierarchy)
        //{
        GetDistanceToDestination();
        return distance <= (_agent.stoppingDistance + 0.5f);
        
    }


    private void GetDistanceToDestination()
    {
        if (_wayPoints[index].gameObject.activeInHierarchy)
        {
            _direction = transform.position - _wayPoints[index].position;
            distance = _direction.magnitude;
        }
        /*else
        {
            if (_coroutine != null)
            {
                
                StopAllCoroutines();
                _coroutine = null;
                
            }

            //_coroutine = StartCoroutine(StopAndLook());

        }*/
    }

    /* public Vector3 offsetFromPath;

     public SplineContainer splineContainer;  // Assign the SquareSpline in Inspector
     public float speed = 1f;
     private float t = 0f;*/

    /* void Update()
     {
         if (splineContainer == null) return;

         Spline spline = splineContainer.Splines[0]; // Get first spline

         // Get position on spline
         Vector3 worldPosition = splineContainer.transform.TransformPoint(spline.EvaluatePosition(t));
         transform.position = worldPosition + offsetFromPath;

         // Get forward direction using the tangent of the spline
         Vector3 tangent = splineContainer.transform.TransformDirection(spline.EvaluateTangent(t));

         if (tangent != Vector3.zero)
         {
             transform.rotation = Quaternion.LookRotation(tangent); // Rotate to face movement direction
         }

         t += speed * Time.deltaTime;
         if (t > 1f) t = 0f; // Loop around
     }*/
}
