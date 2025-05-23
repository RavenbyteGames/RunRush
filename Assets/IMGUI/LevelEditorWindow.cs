using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.UI;
using Button = UnityEngine.UIElements.Button;
using Toggle = UnityEngine.UIElements.Toggle;

public class LevelEditorWindow : EditorWindow
{
    [MenuItem("Window/Level Editor")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<LevelEditorWindow>();
        wnd.titleContent = new GUIContent("Level Editor");
    }

    public VisualTreeAsset treeAsset;
    public LevelOlusturma levelOlusturma;

    private Foldout Leveller;
    private Leveller aktifLeveller;

    private Button balyoz, dikenliTel, ortaBalyoz, sayisalBloklar, bosKarakterler;
    private Button levelOlustur, levelSil, removeAll, saveButton, loadSelectedBtn;

    private IntegerField dusmansayisi, bosssayisi;
    private Toggle bosslevel;

    public GameObject balyozobj;
    public GameObject dikenlitelobj;
    public GameObject ortabalyozobj;
    public GameObject sayisalbloklarobj;
    public GameObject boskarakterobj;

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        CreateUI();
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSelectionChange()
    {
        if (Selection.activeObject is LevelOlusturma selected)
        {
            levelOlusturma = selected;
            CreateUI();
        }
    }

    void CreateUI()
    {
        rootVisualElement.Clear();

        if (treeAsset == null)
        {
            rootVisualElement.Add(new Label("treeAsset bağlı değil!"));
            return;
        }

        treeAsset.CloneTree(rootVisualElement);

        levelOlustur = rootVisualElement.Q<Button>("LevelEkle");
        levelSil = rootVisualElement.Q<Button>("LevelSil");
        Leveller = rootVisualElement.Q<Foldout>("Leveller");

        levelOlustur?.RegisterCallback<ClickEvent>((_) => CreateLevel());
        levelSil?.RegisterCallback<ClickEvent>((_) => DeleteLevel());

        if (levelOlusturma != null)
        {
            foreach (var l in levelOlusturma.levelList)
                AddLevelButton(l.name);
        }
        else
        {
            rootVisualElement.Add(new Label("LevelOlusturma asset atanmamış."));
        }

        balyoz = rootVisualElement.Q<Button>("balyoz");
        dikenliTel = rootVisualElement.Q<Button>("dikenlitel");
        ortaBalyoz = rootVisualElement.Q<Button>("ortabalyoz");
        sayisalBloklar = rootVisualElement.Q<Button>("sayisalbloklar");
        bosKarakterler = rootVisualElement.Q<Button>("boskarakterler");
        removeAll = rootVisualElement.Q<Button>("removeall");
        
        dusmansayisi = rootVisualElement.Q<IntegerField>("dusmansayisi");
        bosssayisi = rootVisualElement.Q<IntegerField>("bossayisi");
        
        bosslevel = rootVisualElement.Q<Toggle>("bosslevel");

        balyoz?.RegisterCallback<ClickEvent>((_) => Spawn(balyozobj));
        dikenliTel?.RegisterCallback<ClickEvent>((_) => Spawn(dikenlitelobj));
        ortaBalyoz?.RegisterCallback<ClickEvent>((_) => Spawn(ortabalyozobj));
        sayisalBloklar?.RegisterCallback<ClickEvent>((_) => Spawn(sayisalbloklarobj));
        bosKarakterler?.RegisterCallback<ClickEvent>((_) => Spawn(boskarakterobj));
        removeAll?.RegisterCallback<ClickEvent>(ClearScene);

        saveButton = new Button(() => SaveLevel()) { text = "Leveli Kaydet" };
        rootVisualElement.Add(saveButton);

        loadSelectedBtn = new Button(() =>
            {
                if (Selection.activeObject is LevelOlusturma selected)
                {
                    levelOlusturma = selected;
                    CreateUI();
                }
            })
            { text = "Seçili Asset'i Yükle" };
        rootVisualElement.Add(loadSelectedBtn);
    }

    void CreateLevel()
    {
        if (levelOlusturma == null) return;

        int index = levelOlusturma.levelList.Count + 1;
        string name = $"Level {index}";

        var leveller = CreateInstance<Leveller>();
        leveller.name = name;

        // ❗ Kaydetme yolunu Resources klasörüne al
        string path = $"Assets/Resources/LevelData/{name}.asset";
        AssetDatabase.CreateAsset(leveller, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(); // yeni dosyayı asset database'e tanıt

        levelOlusturma.levelList.Add(leveller);
        EditorUtility.SetDirty(levelOlusturma);

        AddLevelButton(name);
    }

    void DeleteLevel()
    {
        if (levelOlusturma == null || levelOlusturma.levelList.Count == 0)
            return;

        int lastIndex = levelOlusturma.levelList.Count - 1;
        var levelToDelete = levelOlusturma.levelList[lastIndex];

        // Asset silinmeden önce ismi sakla (Debug için)
        string deletedLevelName = levelToDelete != null ? levelToDelete.name : "Bilinmiyor";

        // Eğer sahnede bu level yüklüyse, sahne objelerini sil
        if (aktifLeveller == levelToDelete)
        {
            var spawnObj = GameObject.Find("SpawnObjeler");
            if (spawnObj != null)
            {
                for (int i = spawnObj.transform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(spawnObj.transform.GetChild(i).gameObject);
                }
            }

            aktifLeveller = null; // artık geçerli değil
        }

        // UI'dan kaldır
        if (Leveller.childCount > 0)
            Leveller.RemoveAt(Leveller.childCount - 1);

        // Liste ve assetten sil
        levelOlusturma.levelList.RemoveAt(lastIndex);
        EditorUtility.SetDirty(levelOlusturma);

        string assetPath = AssetDatabase.GetAssetPath(levelToDelete);
        if (!string.IsNullOrEmpty(assetPath))
        {
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.SaveAssets();
        }

        Debug.Log($"Level silindi: {deletedLevelName}");
    }



    void AddLevelButton(string name)
    {
        var btn = new Button(() => LoadLevel(name)) { text = name };
        Leveller.Add(btn);
    }

    void LoadLevel(string name)
    {
        var obj = GameObject.Find("SpawnObjeler");
        if (obj == null)
        {
            Debug.LogWarning("SpawnObjeler sahnede bulunamadı.");
            return;
        }

        for (int i = obj.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(obj.transform.GetChild(i).gameObject);
        }

        aktifLeveller = levelOlusturma.levelList.Find(x => x.name == name);
        if (aktifLeveller == null)
        {
            Debug.LogWarning($"'{name}' adlı level bulunamadı.");
            return;
        }

        foreach (var p in aktifLeveller.prefabList)
        {
            if (p.prefab == null) continue;

            var go = (GameObject)PrefabUtility.InstantiatePrefab(p.prefab);
            go.transform.SetParent(obj.transform);
            go.transform.position = p.position;
            go.transform.rotation = p.rotation;
        }
        
        dusmansayisi.value = aktifLeveller.dusmansayisi;
        bosssayisi.value = aktifLeveller.bosssayisi;
        bosslevel.value = aktifLeveller.bosslevel;

        Debug.Log($"'{name}' prefabları sahneye yüklendi.");
    }

    void Spawn(GameObject prefab)
    {
        if (aktifLeveller == null) return;

        var spawnObj = GameObject.Find("SpawnObjeler");
        if (spawnObj == null) return;

        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.parent = spawnObj.transform;

        aktifLeveller.prefabList.Add(new Leveller.PrefabData
        {
            prefab = prefab,
            position = go.transform.position,
            rotation = go.transform.rotation
        });

        EditorUtility.SetDirty(aktifLeveller);
    }

    void SaveLevel()
    {
        if (aktifLeveller == null) return;

        var obj = GameObject.Find("SpawnObjeler");
        if (obj == null) return;

        aktifLeveller.prefabList.Clear();

        foreach (Transform child in obj.transform)
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
            if (source == null) continue;

            aktifLeveller.prefabList.Add(new Leveller.PrefabData
            {
                prefab = source,
                position = child.position,
                rotation = child.rotation
            });
        }
        
        aktifLeveller.dusmansayisi = dusmansayisi.value;
        aktifLeveller.bosssayisi = bosssayisi.value;
        aktifLeveller.bosslevel = bosslevel.value;

        EditorUtility.SetDirty(aktifLeveller);
        Debug.Log("Level kaydedildi.");
    }

    void ClearScene(ClickEvent evt)
    {
        var spawnObj = GameObject.Find("SpawnObjeler");
        if (spawnObj == null) return;

        for (int i = spawnObj.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(spawnObj.transform.GetChild(i).gameObject);
        }

        Debug.Log("Sahne temizlendi.");
    }

// ✅ OTOMATİK KAYIT
    void OnSceneGUI(SceneView scene)
    {
        if (aktifLeveller == null) return;

        var obj = GameObject.Find("SpawnObjeler");
        if (obj == null) return;

        bool changed = false;
        var newList = new List<Leveller.PrefabData>();

        foreach (Transform child in obj.transform)
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
            if (source == null) continue;

            var existing = aktifLeveller.prefabList.Find(p => 
                p.prefab == source &&
                p.position == child.position &&
                p.rotation == child.rotation);

            if (existing == null)
            {
                changed = true;
            }

            newList.Add(new Leveller.PrefabData
            {
                prefab = source,
                position = child.position,
                rotation = child.rotation
            });
        }
        
        // Prefab listesi değiştiyse ve/veya inputlarda değişiklik varsa kaydet
       /* if (changed || 
            aktifLeveller.dusmansayisi != dusmansayisi.value ||
            aktifLeveller.bosssayisi != bosssayisi.value ||
            aktifLeveller.bosslevel != bosslevel.value)
        {
            aktifLeveller.prefabList = newList;
            aktifLeveller.dusmansayisi = dusmansayisi.value;
            aktifLeveller.bosssayisi = bosssayisi.value;
            aktifLeveller.bosslevel = bosslevel.value;
        }*/
       
       EditorUtility.SetDirty(aktifLeveller);
       Debug.Log("Level otomatik kaydedildi.");
    }
}