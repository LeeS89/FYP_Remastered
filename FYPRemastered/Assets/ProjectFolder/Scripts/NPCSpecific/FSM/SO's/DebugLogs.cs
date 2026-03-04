using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UObject = UnityEngine.Object;

public static class DebugLogs
{
    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void Log(string msg, object context = null) => Emit(LogType.DebugLog, msg, context);

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void Warn(string msg, object context = null) => Emit(LogType.DebugWarning, msg, context);

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void Err(string msg, object context = null) => Emit(LogType.DebugError, msg, context);


    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void ArgNotNull(object value, string paramName, object ctx = null)
    {
        if (!IsUnityNull(value)) return;
        Throw(new ArgumentNullException(paramName), $"[ARG NULL] {paramName}", ctx);
        
    }

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void RequireNotNull(object value, string nameOrExp, object ctx = null)
    {
        if (!IsUnityNull(value)) return;
        Throw(new InvalidOperationException($"{nameOrExp} was null"), $"[REQUIRE {nameOrExp}]", ctx);
    }

    [Conditional("UNITY_EDITOR")]
    [Conditional("DEVELOPMENT_BUILD")]
    public static void Nre(object value, string nameOrExp, object ctx = null)
    {
        if (!IsUnityNull(value)) return;
        Throw(new NullReferenceException($"{nameOrExp} was null"), $"[NRE] {nameOrExp}", ctx);
    }



    private static void Emit(LogType type, string msg, object ctx)
    {
        if(ctx is UObject u)
        {
            if(u == null)
            {
                Write(type, $"{msg} | ctx: <destroyed Unityengine.Object>", null);
                return;
            }

            Write(type, msg, u);
            return;
        }

        if (ctx != null) msg = $"{msg} | ctx: {ctx}";
        Write(type, msg, null);
    }

    private static void Write(LogType type, string msg, UObject unityContext)
    {
        switch (type)
        {
            case LogType.DebugWarning:
                if (unityContext == null) Debug.LogWarning(msg, unityContext);
                else Debug.LogWarning(msg);
                break;
            case LogType.DebugError:
                if (unityContext == null) Debug.LogError(msg, unityContext);
                else Debug.LogError(msg);
                break;
            default:
                if (unityContext == null) Debug.Log(msg, unityContext);
                else Debug.Log(msg);
                break;
        }

    }

    private static void Throw(Exception ex, string tagMsg, object ctx)
    {
       // Emit(LogType.DebugError, tagMsg, ctx);

        throw ex;
    }

    private static bool IsUnityNull(object value)
    {
        if (value is null) return true;
        if (value is UObject u) return u == null; // Unity "fake null" for destroyed objects
        return false;
    }

    private enum LogType
    {
        DebugLog,
        DebugWarning,
        DebugError,
        NRE
    }

}


