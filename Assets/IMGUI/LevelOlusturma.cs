using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "ScriptableObjects/LevelData", order = 1)]
public class LevelOlusturma : ScriptableObject
{
    [System.Serializable]
    public struct Level
    {
        public GameObject[] prefabs;
    }

    public Level[] levels;

    // Kalıcı olarak level asset referanslarını tutar
    public List<Leveller> levelList = new List<Leveller>();

    public void Test()
    {
        Debug.Log("Selam");
    }

    public void InstanteObj()
    {
        // Prefab instantiate işlemleri burada yapılabilir
    }
}