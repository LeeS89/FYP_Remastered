using System.Collections.Generic;
using UnityEngine;

public class AIDestinationProvider
{
    
}


public interface ICandidateDestinationProvider : IZoneSink
{
    List<(Vector3 position, Vector3? forward)> TryGet(in ValidateDestination req);
    
}



public static class DestExtension
{
    public static void TestGet(this DestinationServiceId id)
    {

    }
}