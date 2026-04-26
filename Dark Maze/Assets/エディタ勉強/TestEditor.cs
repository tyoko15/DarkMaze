using UnityEditor;
using UnityEngine;


public class TestEditor : Editor
{
    [MenuItem("Tools/テスト")]
    static void Test()
    {
        Debug.Log("動いた！");
    }
}