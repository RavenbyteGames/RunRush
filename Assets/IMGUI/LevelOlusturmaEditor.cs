using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(LevelOlusturma))]
public class LevelOlusturmaEditor : Editor
{
    public VisualTreeAsset treeAsset;

    private Button levelOlustur;
    private Button levelSil;
    private Foldout Leveller;

    private LevelOlusturma levelolusturma;

    public GameObject spawnObjects;

    public void OnEnable()
    {
        levelolusturma = (LevelOlusturma)target;
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        treeAsset.CloneTree(root);

        levelOlustur = root.Q<Button>("LevelEkle");
        levelOlustur.RegisterCallback<ClickEvent>(LevelOLustur);

        levelSil = root.Q<Button>("LevelSil");
        levelSil.RegisterCallback<ClickEvent>(LevelSil);

        Leveller = root.Q<Foldout>("Leveller");

        // Mevcut asset'lere göre butonları yeniden oluştur
        foreach (var levelAsset in levelolusturma.levelList)
        {
            AddLevelButton(levelAsset.name);
        }

        return root;
    }

    private void LevelOLustur(ClickEvent evt)
    {
        int index = levelolusturma.levelList.Count + 1;
        string newLevelName = $"Level {index}";

        // Yeni asset oluştur
        var levelAsset = CreateInstance<Leveller>();
        levelAsset.name = newLevelName;

        string path = $"Assets/IMGUI/LevelData/{newLevelName}.asset";
        AssetDatabase.CreateAsset(levelAsset, path);
        AssetDatabase.SaveAssets();

        // Listeye ekle ve sahneye kaydet
        levelolusturma.levelList.Add(levelAsset);
        EditorUtility.SetDirty(levelolusturma);

        // UI'ya ekle
        AddLevelButton(newLevelName);
    }

    private void LevelSil(ClickEvent evt)
    {
        if (levelolusturma.levelList.Count > 0)
        {
            int lastIndex = levelolusturma.levelList.Count - 1;
            var levelToDelete = levelolusturma.levelList[lastIndex];

            // 🧹 Eğer sahnede yüklüyse, objeleri sil
            var spawnObj = GameObject.Find("SpawnObjeler");
            if (spawnObj != null)
            {
                foreach (Transform child in spawnObj.transform)
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            // Foldout'tan butonu kaldır
            if (Leveller.childCount > 0)
                Leveller.RemoveAt(Leveller.childCount - 1);

            // Listeden çıkar
            levelolusturma.levelList.RemoveAt(lastIndex);
            EditorUtility.SetDirty(levelolusturma);

            // Asset dosyasını sil
            string assetPath = AssetDatabase.GetAssetPath(levelToDelete);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.SaveAssets();
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
        
        button.RegisterCallback<ClickEvent>(test_click);
    }
    private void test_click(ClickEvent evt)
    {
        var obj = GameObject.Find("SpawnObjeler");
        if (obj == null)
        {
            Debug.LogWarning("SpawnObjeler nesnesi sahnede bulunamadı.");
            return;
        }

        // Tüm sahne objelerini sil
        for (int i = obj.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(obj.transform.GetChild(i).gameObject);
        }

        // Hangi level tıklandıysa onun prefablarını sahneye yerleştir
        var clickedButton = evt.target as Button;
        string levelName = clickedButton.text;

        var selectedLevel = levelolusturma.levelList.Find(l => l.name == levelName);

        if (selectedLevel != null)
        {
            foreach (var data in selectedLevel.prefabList)
            {
                if (data.prefab == null) continue;

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(data.prefab);
                instance.transform.parent = obj.transform;
                instance.transform.position = data.position;
                instance.transform.rotation = data.rotation;
            }

            Debug.Log($"'{levelName}' level prefabları sahneye yüklendi.");
        }
    }
    
}
