using UnityEngine;

public class PreWarmShaders : MonoBehaviour
{
    public ShaderVariantCollection Variants;

    private void Awake()
    {
        if(Variants != null && !Variants.isWarmedUp)
        {
            Debug.LogError("Warming shader");
            Variants.WarmUp();
        }
    }

    
}
