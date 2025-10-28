using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <typeparam name="WhenDuplicates"><see cref="DoNothing"/>, <see cref="RemoveLateComer"/>, <see cref="RemoveExisting"/></typeparam>
public class Singleton<T, WhenDuplicates> : SingletonBase
    where T : Singleton<T, WhenDuplicates>
    where WhenDuplicates : DuplicateAction, new()
{
    private static T _instance = null;
    public static T Instance
    {
        get
        {
            if (!IsAppQuit && _instance == null)
            {
                //Debug.LogErrorFormat("Class {0} is null, can not get singleton instance", typeof(T).Name);
                return null;
            }
            return _instance;
        }
    }

    protected override void Awake()
    {
#if UNITY_EDITOR
        IsAppQuit = false;
#endif

        if (IsDestroying || IsAppQuit)
            return; // skip if destroying or app quit

        if (_instance != null)
        {
            if (_instance.GetInstanceID() != this.GetInstanceID())
            {
                WhenDuplicates duplicateAction = new WhenDuplicates();
                if (duplicateAction.Action == (new RemoveLateComer()).Action)
                {
                    Debug.LogWarning("Destroying late singleton: " + this, this);
                    enabled = false;
                    Destroy(gameObject);
                }
                else if (duplicateAction.Action == (new RemoveExisting()).Action)
                {
                    Debug.LogError("Destroying existing singleton: " + _instance, this);
                    _instance.enabled = false;
                    Destroy(_instance.gameObject);
                    _instance = (T)this;
                }
            }
        }
        else
        {
            _instance = (T)this;

            Debug.LogWarning(gameObject.name.ToString() + "Singleton spawn");
        }
    }

    public static bool HasInstance()
    {
        return _instance != null;
    }   
    public bool IsTheInstance()
    {
        return _instance?.GetInstanceID() == this.GetInstanceID();
    }

    protected bool IsDestroying { get; private set; } = false;
    protected virtual void OnDestroy()
    {
        IsDestroying = true;
        // unless the instance refers to a different object, set to null.
        // (NOTE: checking if (_instance == this) doesn't work,
        // since Unity play tricks with compare(==) operator.)
        //if (ReferenceEquals(_instance, this))
        //{
        //    _instance = null;
        //}
    }

    protected static bool IsAppQuit { get; private set; } = false;
    protected virtual void OnApplicationQuit()
    {
        IsAppQuit = true;
    }
}