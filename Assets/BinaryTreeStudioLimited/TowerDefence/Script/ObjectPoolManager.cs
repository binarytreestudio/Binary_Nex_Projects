using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefence
{
    public class ObjectPoolManager : Singleton<ObjectPoolManager>
    {
        [SerializeField] private List<GameObject> fireBalls;
    }
}
