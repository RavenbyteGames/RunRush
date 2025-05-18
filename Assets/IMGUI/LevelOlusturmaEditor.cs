using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

[CustomEditor(typeof(LevelOlusturma))]
public class LevelOlusturmaEditor : Editor
{
    public VisualTreeAsset treeAsset;

    private Button levelOlustur;
    private Button levelSil;
    private Foldout Leveller;

    private Button balyoz;
    private Button dikenliTel;
    private Button ortaBalyoz;
    private Button sayisalBloklar;
    private Button removeAll;
    private Button saveButton;

    public GameObject balyozobj;
    public GameObject dikenlitelobj;
    public GameObject ortabalyozobj;
    public GameObject sayisalbloklarobj;

    private LevelOlusturma levelolusturma;
    private Leveller aktifLeveller;

    public override VisualElement CreateInspectorGUI()
    {
        levelolusturma = (LevelOlusturma)target;
        VisualElement root = new VisualElement();

        treeAsset.CloneTree(root);

        levelOlustur = root.Q<Button>("LevelEkle");
        levelSil = root.Q<Button>("LevelSil");
        Leveller = root.Q<Foldout>("Leveller");

        levelOlustur.RegisterCallback<ClickEvent>(LevelOLustur);
        levelSil.RegisterCallback<ClickEvent>(LevelSil);

        // Level butonları
        foreach (var levelAsset in levelolusturma.levelList)
        {
            AddLevelButton(levelAsset.name);
        }

        // Obje ekleme butonları
        balyoz = root.Q<Button>("balyoz");
        dikenliTel = root.Q<Button>("dikenlitel");
        ortaBalyoz = root.Q<Button>("ortabalyoz");
        sayisalBloklar = root.Q<Button>("sayisalbloklar");
        removeAll = root.Q<Button>("removeall");

        balyoz?.RegisterCallback<ClickEvent>((_) => ObjeAtama(balyozobj));
        dikenliTel?.RegisterCallback<ClickEvent>((_) => ObjeAtama(dikenlitelobj));
        ortaBalyoz?.RegisterCallback<ClickEvent>((_) => ObjeAtama(ortabalyozobj));
        sayisalBloklar?.RegisterCallback<ClickEvent>((_) => ObjeAtama(sayisalbloklarobj));
        removeAll?.RegisterCallback<ClickEvent>(RemoveAll_click);

        saveButton = new Button(() => SaveLevelFromScene())
        {
            text = "Leveli Kaydet"
        };
        root.Add(saveButton);

        SceneView.duringSceneGui += OnSceneGUI;

        return root;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void LevelOLustur(ClickEvent evt)
    {
        int index = levelolusturma.levelList.Count + 1;
        string newLevelName = $"Level {index}";

        var levelAsset = CreateInstance<Leveller>();
        levelAsset.name = newLevelName;

        string path = $"Assets/IMGUI/LevelData/{newLevelName}.asset";
        AssetDatabase.CreateAsset(levelAsset, path);
        AssetDatabase.SaveAssets();

        levelolusturma.levelList.Add(levelAsset);
        EditorUtility.SetDirty(levelolusturma);

        AddLevelButton(newLevelName);
    }

    private void LevelSil(ClickEvent evt)
    {
        if (levelolusturma.levelList.Count > 0)
        {
            int lastIndex = levelolusturma.levelList.Count - 1;
            var levelToDelete = levelolusturma.levelList[lastIndex];

            if (Leveller.childCount > 0)
                Leveller.RemoveAt(Leveller.childCount - 1);

            levelolusturma.levelList.RemoveAt(lastIndex);
            EditorUtility.SetDirty(levelolusturma);

            string assetPath = AssetDatabase.GetAssetPath(levelToDelete);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.SaveAssets();
            }

            // Sahneyi temizle
            var spawnObj = GameObject.Find("SpawnObjeler");
            if (spawnObj != null)
            {
                foreach (Transform child in spawnObj.transform)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }

    private void AddLevelButton(string levelName)
    {
        var button = new Button(() => Debug.Log($"{levelName} clicked"))
        {
            text = levelName
        };
        Leveller.Add(button);
        button.RegisterCallback<ClickEvent>((evt) =>
        {
            var obj = GameObject.Find("SpawnObjeler");
            if (obj == null) return;

            for (int i = obj.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(obj.transform.GetChild(i).gameObject);
            }

            aktifLeveller = levelolusturma.levelList.Find(l => l.name == levelName);
            if (aktifLeveller == null) return;

            foreach (var data in aktifLeveller.prefabList)
            {
                if (data.prefab == null) continue;

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(data.prefab);
                instance.transform.parent = obj.transform;
                instance.transform.position = data.position;
                instance.transform.rotation = data.rotation;
            }

            Debug.Log($"'{levelName}' prefabları yüklendi.");
        });
    }

    private void ObjeAtama(GameObject prefab)
    {
        if (aktifLeveller == null)
        {
            Debug.LogWarning("Aktif bir level seçilmedi!");
            return;
        }

        var spawnObj = GameObject.Find("SpawnObjeler");
        if (spawnObj == null) return;

        var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        obj.transform.parent = spawnObj.transform;

        var data = new Leveller.PrefabData
        {
            prefab = prefab,
            position = obj.transform.position,
            rotation = obj.transform.rotation
        };

        aktifLeveller.prefabList.Add(data);
        EditorUtility.SetDirty(aktifLeveller);
        Debug.Log($"'{prefab.name}' eklendi.");
    }

    private void SaveLevelFromScene()
    {
        if (aktifLeveller == null)
        {
            Debug.LogWarning("Aktif level yok, kaydedilemedi.");
            return;
        }

        var spawnObj = GameObject.Find("SpawnObjeler");
        if (spawnObj == null)
        {
            Debug.LogWarning("SpawnObjeler bulunamadı.");
            return;
        }

        // prefabList'i sıfırla ve sahnedekileri baştan yaz
        aktifLeveller.prefabList.Clear();

        foreach (Transform child in spawnObj.transform)
        {
            var prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);

            if (prefabSource == null)
            {
                Debug.LogWarning($"{child.name} prefab bağlantısı yok, atlanıyor.");
                continue;
            }

            aktifLeveller.prefabList.Add(new Leveller.PrefabData
            {
                prefab = prefabSource,
                position = child.position,
                rotation = child.rotation
            });
        }

        EditorUtility.SetDirty(aktifLeveller);
        Debug.Log("Level sahnedeki haliyle başarıyla kaydedildi.");
    }


    private void RemoveAll_click(ClickEvent evt)
    {
        var spawnObj = GameObject.Find("SpawnObjeler");
        if (spawnObj == null) return;

        for (int i = spawnObj.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(spawnObj.transform.GetChild(i).gameObject);
        }

        Debug.Log("Tüm objeler silindi.");
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (Event.current.type != EventType.MouseUp) return;
        if (aktifLeveller == null) return;

        var spawnObj = GameObject.Find("SpawnObjeler");
        if (spawnObj == null) return;

        var children = new List<Transform>();
        foreach (Transform child in spawnObj.transform)
            children.Add(child);

        for (int i = 0; i < Mathf.Min(children.Count, aktifLeveller.prefabList.Count); i++)
        {
            aktifLeveller.prefabList[i].position = children[i].position;
            aktifLeveller.prefabList[i].rotation = children[i].rotation;
        }

        EditorUtility.SetDirty(aktifLeveller);
    }
}
