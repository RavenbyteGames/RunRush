using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(Leveller))]
public class LevellerEditor : Editor
{
    public VisualTreeAsset treeAsset;

    private Leveller _leveller;

    private Button balyoz;
    private Button dikenliTel;
    private Button ortaBalyoz;
    private Button sayisalBloklar;
    private Button RemoveAll;

    public GameObject balyozobj;
    public GameObject dikenlitelobj;
    public GameObject ortabalyozobj;
    public GameObject sayisalbloklarobj;

    public List<Object> allobj;


    public void OnEnable()
    {
        _leveller = (Leveller)target;
        allobj = new List<Object>();
        SceneView.duringSceneGui += OnSceneGUI;
    }
    
    public void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        treeAsset.CloneTree(root);
        
        balyoz = root.Q<Button>("balyoz");
        balyoz.RegisterCallback<ClickEvent>(BalyozOlustur);
        
        dikenliTel = root.Q<Button>("dikenlitel");
        dikenliTel.RegisterCallback<ClickEvent>(DikenliTelOlustur);
        
        ortaBalyoz = root.Q<Button>("ortabalyoz");
        ortaBalyoz.RegisterCallback<ClickEvent>(OrtaBalyozOlustur);
        
        sayisalBloklar = root.Q<Button>("sayisalbloklar");
        sayisalBloklar.RegisterCallback<ClickEvent>(SayisalbloklarOlustur);
        
        RemoveAll = root.Q<Button>("removeall");
        RemoveAll.RegisterCallback<ClickEvent>(RemoveAll_click);
        
        var updateBtn = new Button(() => _leveller.SaveLevelFromScene())
        {
            text = "Leveli Kaydet"
        };
        root.Add(updateBtn);

        return root;
    }

    private void BalyozOlustur(ClickEvent evt)
    { 
        ObjeAtama(balyozobj);
    }
    
    private void DikenliTelOlustur(ClickEvent evt)
    {
        ObjeAtama(dikenlitelobj);
    }
    private void OrtaBalyozOlustur(ClickEvent evt)
    {
        ObjeAtama(ortabalyozobj);
    }
    private void SayisalbloklarOlustur(ClickEvent evt)
    {
        ObjeAtama(sayisalbloklarobj);
    }

    void ObjeAtama(GameObject prefab)
    {
        var spawnObj = GameObject.Find("SpawnObjeler");
        if (spawnObj == null) return;

        // Instantiate sahneye
        var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        obj.transform.parent = spawnObj.transform;

        // Pozisyonu sakla
        var data = new Leveller.PrefabData
        {
            prefab = prefab,
            position = obj.transform.position,
            rotation = obj.transform.rotation
        };

        _leveller.prefabList.Add(data);
        EditorUtility.SetDirty(_leveller);

        Debug.Log($"'{prefab.name}' sahneye eklendi ve pozisyonla birlikte kaydedildi.");
    }
    
    private void OnSceneGUI(SceneView sceneView)
    {
        if (Event.current.type != EventType.MouseUp) return;

        var spawnObj = GameObject.Find("SpawnObjeler");
        if (spawnObj == null) return;

        var children = new List<Transform>();
        foreach (Transform child in spawnObj.transform)
            children.Add(child);

        for (int i = 0; i < Mathf.Min(children.Count, _leveller.prefabList.Count); i++)
        {
            _leveller.prefabList[i].position = children[i].position;
            _leveller.prefabList[i].rotation = children[i].rotation;
        }

        EditorUtility.SetDirty(_leveller);
    }

    private void RemoveAll_click(ClickEvent evt)
    {
        var spawnObj = GameObject.Find("SpawnObjeler");
        if (spawnObj == null) return;

        for (int i = spawnObj.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(spawnObj.transform.GetChild(i).gameObject);
        }

        Debug.Log("Tüm objeler sahneden silindi.");
    }

}
