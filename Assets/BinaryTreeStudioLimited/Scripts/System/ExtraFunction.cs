using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using DG.Tweening;

public static class ExtraFunction
{
#if UNITY_EDITOR
    public static T[] FindAssetsByType<T>(params string[] folders) where T : Object
    {
        string type = typeof(T).ToString().Replace("UnityEngine.", "");

        string[] guids;
        if (folders == null || folders.Length == 0)
        {
            guids = AssetDatabase.FindAssets("t:" + type);
        }
        else
        {
            guids = AssetDatabase.FindAssets("t:" + type, folders);
        }

        T[] assets = new T[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            assets[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }
        return assets;
    }

    [MenuItem("Extra Tools/Open Persistent Data Path", priority = 5)]
    public static void OpenPersistentDataPaths()
    {
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        process.StartInfo.FileName = ((Application.platform == RuntimePlatform.WindowsEditor) ? "explorer.exe" : "open");
        process.StartInfo.Arguments = "file://" + Application.persistentDataPath;
        process.Start();
    }

    [MenuItem("Extra Tools/Delete Save Folder", priority = 6)]
    public static void DeleteSaveFolder()
    {
        if (Directory.Exists(Application.persistentDataPath))
        {
            if (EditorUtility.DisplayDialog("Confirm Delete",
                $"Are you sure you want to delete the save folder at:\n{Application.persistentDataPath}?\nThis action cannot be undone.",
                "Delete", "Cancel"))
            {
                try
                {
                    Directory.Delete(Application.persistentDataPath, true);
                    Debug.Log($"Save folder deleted: {Application.persistentDataPath}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to delete save folder: {e.Message}");
                }
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Info", "No save folder exists at the persistent data path.", "OK");
        }
    }
#endif

    public static void DestoryChildObject(this Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Object.Destroy(transform.GetChild(i).gameObject);
        }
    }

    public static int GetRandomArrayNum(this int[] array)
    {
        int[] levelCount = new int[array.Length];
        int totalChance = 0;


        for (int count = 0; count < array.Length; count++)
        {
            totalChance += array[count];
            levelCount[count] = totalChance;
        }

        int num = Random.Range(1, totalChance);

        for (int x = 0; x < array.Length; x++)
        {
            if (num <= levelCount[x])
            {
                return x;
            }
        }

        return -1;
    }

    public static string FormatNumberWithUnit(this double number)
    {
        double value = number;

        string[] suffixes = { "", "K", "M", "B", "T" };

        int suffixIndex = 0;
        while (value >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            value /= 1000;
            suffixIndex++;
        }

        return $"{value:0.##} {suffixes[suffixIndex]}";
    }

    public static string FormatNumberWithUnit(this float number)
    {
        double value = number;

        string[] suffixes = { "", "K", "M", "B", "T" };

        int suffixIndex = 0;
        while (value >= 1000 && suffixIndex < suffixes.Length - 1)
        {
            value /= 1000;
            suffixIndex++;
        }

        return $"{value:0.##} {suffixes[suffixIndex]}";
    }

    public static int GetRandomArrayNum(this float[] array)
    {
        float[] levelCount = new float[array.Length];
        float totalChance = 0;


        for (int count = 0; count < array.Length; count++)
        {
            totalChance += array[count];
            levelCount[count] = totalChance;
        }

        float num = Random.Range(0, totalChance);

        for (int x = 0; x < array.Length; x++)
        {
            if (num <= levelCount[x])
            {
                return x;
            }
        }


        Debug.Log("Random Error");
        return -1;
    }

    public static List<T> GetRandomObjsInList<T>(this List<T> list, int count)
    {
        var newList = new List<T>();

        list.ShuffleList();

        if (count > list.Count)
            count = list.Count;

        for (int i = 0; i < count; i++)
        {
            newList.Add(list[i]);
        }

        return newList;
    }

    public static void InsertSort<T>(this List<T> data, T insertData) where T : IPrioritySortable
    {

        if (data.Count == 0)
        {
            data.Add(insertData);
        }
        else
        {

            for (int i = 0; i < data.Count; i++)
            {
                if (insertData.priority <= data[i].priority)
                {
                    data.Insert(i, insertData);
                    return;
                }

            }

            data.Add(insertData);
        }
    }


    public static T GetRandomObjInList<T>(this List<T> list) where T : Object
    {
        if (list.Count <= 0)
            return null;
        return list[Random.Range(0, list.Count)];
    }


    public static T GetRandomObjInArray<T>(this T[] list) where T : Object
    {
        if (list.Length <= 0)
            return null;
        return list[Random.Range(0, list.Length)];
    }

    public static int GetArrayLoop<T>(this List<T> list, int index, bool next)
    {

        if (next && index + 1 >= list.Count)
        {
            return 0;
        }
        else if (!next && index - 1 < 0)
        {
            return list.Count - 1;
        }

        return index + (next ? 1 : -1);


    }


    public static int GetRandomArrayNum(this List<int> array)
    {
        return array.ToArray().GetRandomArrayNum();
    }

    public static int GetRandomArrayNum(this List<float> array)
    {
        return array.ToArray().GetRandomArrayNum();
    }

    public static void MoveItemInList<T>(this List<T> list, int oldIndex, int newIndex)
    {
        T item = list[oldIndex];
        list.RemoveAt(oldIndex);
        list.Insert(newIndex, item);

    }

    public static float Remap(this float value, float from1, float from2, float to1, float to2)
    {
        if (Mathf.Abs(from2 - from1) < Mathf.Epsilon)
        {
            Debug.LogError("Invalid Remap range: from1 and from2 cannot be equal.");
            return to1; // 或者返回一個合理的默認值
        }
        return (value - from1) / (from2 - from1) * (to2 - to1) + to1;
    }
    public static T DeepClone<T>(this T obj)
    {
        using (var ms = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;

            return (T)formatter.Deserialize(ms);
        }
    }
    public static T Pop<T>(this List<T> obj, int index = 0)
    {
        T member = obj[index];
        obj.RemoveAt(index);
        return member;
    }
    /// <summary>
    /// Fisher-Yates randomization
    /// </summary>
    public static void ShuffleList<T>(this List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    public static int Difference(int num1, int num2)
    {
        int cout;
        cout = Mathf.Max(num2, num1) - Mathf.Min(num1, num2);
        return cout;
    }

    public static Sequence SetDelayCall(System.Action action, float delayTime, int loopCount = 1)
    {
        var seq = DOTween.Sequence();

        seq.AppendCallback(() => { action.Invoke(); }).SetDelay(delayTime).SetLoops(loopCount);

        return seq;

    }
    public static void SortByPriority<T>(this List<T> list) where T : IPrioritySortable
    {
        list.Sort((a, b) => a.priority.CompareTo(b.priority));
    }
}
public interface IPrioritySortable
{
    public int priority { get; }
}

public interface BTS_VirtualCameraSetting
{
    public void SetSceneObjectsActive(bool active);
}