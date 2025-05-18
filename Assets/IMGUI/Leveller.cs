using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Leveller", menuName = "ScriptableObjects/Leveler", order = 1)]
public class Leveller : ScriptableObject
{
    public string Note;

    [System.Serializable]
    public class PrefabData
    {
        public GameObject prefab;
        public Vector3 position;
        public Quaternion rotation;
    }
    
    public int kacDusman;
    public int kacBoss;
    public bool bossLevel;

    public List<PrefabData> prefabList = new List<PrefabData>();
    
    [ContextMenu("Leveli Kaydet")]
    public void SaveLevelFromScene()
    {
        var spawnObj = GameObject.Find("SpawnObjeler");
        if (spawnObj == null) return;

        // Eski prefab verilerini sıfırla
        prefabList.Clear();

        // Yeni sahnedeki objeleri kaydet
        foreach (Transform child in spawnObj.transform)
        {
            var prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
            if (prefabSource == null) continue;

            prefabList.Add(new PrefabData
            {
                prefab = prefabSource,
                position = child.position,
                rotation = child.rotation
            });
        }

        EditorUtility.SetDirty(this);
        Debug.Log("Level sahnedeki haliyle kaydedildi.");
    }

}