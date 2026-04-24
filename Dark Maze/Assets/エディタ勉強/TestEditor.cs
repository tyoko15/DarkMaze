using UnityEditor;
using UnityEngine;

public class TestEditor
{
    [MenuItem("Tools/テスト")]
    static void Test()
    {
        Debug.Log("動いた！");
    }
}