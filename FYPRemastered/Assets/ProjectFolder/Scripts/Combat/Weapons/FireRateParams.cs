using UnityEngine;


[System.Serializable]
public class FireRateParams
{
    public FireRate _fireRate;

    [Range(0.5f, 2.0f)]
    public float _interval;

    [Range(0.1f, 0.5f)]
    public float _rapidInterval;

    public bool _useFixedInterval;

    [Range(0.2f, 1f)]
    public float _minFireInterval;
    [Range(1.1f, 2.0f)]
    public float _maxFireInterval;
  

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

    
}
