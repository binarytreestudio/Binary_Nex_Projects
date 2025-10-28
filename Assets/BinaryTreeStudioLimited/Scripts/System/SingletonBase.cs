using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Need to be abstract but need to change its execution order in Editor
/// </summary>
public class SingletonBase : MonoBehaviour { protected virtual void Awake() { } }
public class Singleton<T> : Singleton<T, DoNothing> where T : Singleton<T> { }
public abstract class DuplicateAction { public abstract int Action { get; } }
public class DoNothing : DuplicateAction { public override int Action { get { return 1; } } }
public class RemoveLateComer : DuplicateAction { public override int Action { get { return 2; } } }
public class RemoveExisting : DuplicateAction { public override int Action { get { return 3; } } }
