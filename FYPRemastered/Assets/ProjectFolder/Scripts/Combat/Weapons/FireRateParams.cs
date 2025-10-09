using System;
using UnityEngine;
using Random = UnityEngine.Random;


[System.Serializable]
public class FireRateParams
{
    public FireRate _fireRate;

    [Range(0.5f, 2.0f)]
    public float _interval = 1f;

    [Range(0.1f, 0.5f)]
    public float _rapidInterval = 0.25f;

    public bool _useFixedInterval = false;

    [Range(0.2f, 1f)]
    public float _minFireInterval = 0.5f;
    [Range(1.1f, 2.0f)]
    public float _maxFireInterval = 2.0f;

  //  private float _nextTick;

   // public event Action OnReachedFiringThreshold;

    public float GetNextInterval()
    {
        float rate;

        switch (_fireRate)
        {
            case FireRate.SingleAutomatic:
                if (_useFixedInterval) rate = _interval;
                else rate = Random.Range(_minFireInterval, _maxFireInterval);
                break;
            case FireRate.FullAutomatic:
                rate = _rapidInterval;
                break;
            default:
                rate = _interval;
                break;
        }

        return rate;
       
    }

  /*  public void Tick(float now)
    {
        if (_fireRate == FireRate.Single) return;
        if (now < _nextTick) return;

        OnReachedFiringThreshold?.Invoke();
        _nextTick += GetNextInterval(); ;
    }

    
*/
    
}
