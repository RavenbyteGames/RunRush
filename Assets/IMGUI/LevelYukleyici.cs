using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelYukleyici : MonoBehaviour
{
    [Header("ScriptableObject - LevelOlusturma")]
    public LevelOlusturma levelOlusturma;

    private GameObject aktifLevelContainer;

    void Start()
    {
        if (levelOlusturma == null || levelOlusturma.levelList == null || levelOlusturma.levelList.Count == 0)
        {
            Debug.LogError("LevelOlusturma veya level listesi eksik!");
            return;
        }

        int aktifIndex = Mathf.Clamp(PlayerPrefs.GetInt("SonLevel", 1) - 1, 0, levelOlusturma.levelList.Count - 1);

        Leveller leveller = levelOlusturma.levelList[aktifIndex];
        if (leveller == null)
        {
            Debug.LogWarning("Seçilen level asset null.");
            return;
        }

        YukseltLevel(leveller);
    }

    public void YukseltLevel(Leveller leveller)
    {
        if (aktifLevelContainer != null)
            DestroyImmediate(aktifLevelContainer);

        aktifLevelContainer = new GameObject("YuklenenLevel");

        foreach (var p in leveller.prefabList)
        {
            if (p.prefab == null) continue;

            GameObject go = Instantiate(p.prefab, p.position, p.rotation);
            go.transform.SetParent(aktifLevelContainer.transform);
        }

        Debug.Log($"Level yüklendi: {leveller.name}");
    }
}