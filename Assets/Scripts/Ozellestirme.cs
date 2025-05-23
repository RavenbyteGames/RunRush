using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ayberk;
using TMPro;
using UnityEngine.SceneManagement;

public class Ozellestirme : MonoBehaviour
{
   
    
    public TextMeshProUGUI ParaText;
    public TextMeshProUGUI ElmasText;
    
    public GameObject[] islemPanelleri;
    public GameObject islemCanvasi;
    public GameObject[] GenelPanaller;
    public Button[] islemButonlari;
    private int AktifislemPaneliIndex;
    [Header("----SAPKALAR----")]
    public GameObject[] Sapkalar;
    public Button[] SapkaButonlari;
    public TextMeshProUGUI SapkaText;
    [Header("----SOPALAR----")]
    public GameObject[] Sopalar;
    public Button[] SopaButonlari;
    public TextMeshProUGUI SopaText;
    [Header("----MATERİAL----")]
    public Material[] Materyaller;
    public Material VarSayilanBoya;
    public Button[] MateryalButonlari;
    public TextMeshProUGUI MaterialText;
    public SkinnedMeshRenderer _Renderer;

    private int SapkaIndex = -1;
    private int SopaIndex = -1;
    private int MaterialIndex = -1;
    
    BellekYonetim _BellekYonetim= new BellekYonetim();
    VeriYonetimi _VeriYonetimi = new VeriYonetimi();
    [Header("----GENEL VERİLER----")]
    public List<ItemBilgileri> _ItemBilgileri = new List<ItemBilgileri>();
    public List<DilVerileriAnaObje> _DilVerileriAnaObje = new List<DilVerileriAnaObje>();
    List<DilVerileriAnaObje> _DilOkunanVeriler = new List<DilVerileriAnaObje>();
    public TextMeshProUGUI[] TextObjeleri;
    public Animator Kaydedildi_Animator;

    public AudioSource[] Sesler;

    string SatinAlmaText;
    string ItemText;

    private void Start()
    {
        Application.targetFrameRate = 60;
        
        DynamicGI.UpdateEnvironment();
        
        //_BellekYonetim.VeriKaydet_int("Puan", 4000);
        
        ParaText.text = _BellekYonetim.VeriOku_i("Puan").ToString();
        ElmasText.text = _BellekYonetim.VeriOku_i("Elmas").ToString();
        
        _VeriYonetimi.Load();
        _ItemBilgileri = _VeriYonetimi.ListeyiAktar();

        DurumuKontrolEt(0, true);
        DurumuKontrolEt(1, true);
        DurumuKontrolEt(2, true);

        foreach (var item in Sesler)
        {
            item.volume = _BellekYonetim.VeriOku_f("MenuSes");
        }
        
        _VeriYonetimi.Dil_Load();
        _DilOkunanVeriler = _VeriYonetimi.DilVerileriListeyiAktar();
        
        _DilVerileriAnaObje.Add(_DilOkunanVeriler[1]);
        
        DilTercihiYonetimi();
    }
    
    public void DilTercihiYonetimi()
    {
        if (_BellekYonetim.VeriOku_s("Dil") == "TR")
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
            {
                TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_TR[i].Metin;
            }

            SatinAlmaText = _DilVerileriAnaObje[0]._DilVerileri_TR[3].Metin;
            ItemText = _DilVerileriAnaObje[0]._DilVerileri_TR[3].Metin;
        }
        else
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
            {
                TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_EN[i].Metin;
            }

            SatinAlmaText = _DilVerileriAnaObje[0]._DilVerileri_EN[3].Metin;
            ItemText = _DilVerileriAnaObje[0]._DilVerileri_TR[3].Metin;
        }
    }
    void DurumuKontrolEt(int Bolum, bool islem=false)
    {
        if (Bolum == 0)
        {
            #region AktifSapka

            if (_BellekYonetim.VeriOku_i("AktifSapka") == -1)
            {
                foreach (var item in Sapkalar)
                {
                    item.SetActive(false);
                }

                TextObjeleri[3].text = SatinAlmaText;
                islemButonlari[0].interactable = false;
                islemButonlari[1].interactable = false;
                
                if (!islem)
                {
                    SapkaIndex = -1;
                    SapkaText.text = "No Hat";
                }
            }
            else
            {
                foreach (var item in Sapkalar)
                {
                    item.SetActive(false);
                }
                
                SapkaIndex = _BellekYonetim.VeriOku_i("AktifSapka");
                Sapkalar[SapkaIndex].SetActive(true);
                SapkaText.text = _ItemBilgileri[SapkaIndex].Item_Ad;
                TextObjeleri[3].text = SatinAlmaText;
                islemButonlari[0].interactable = false;
                islemButonlari[1].interactable = true;
            }

            #endregion
        }
        else if (Bolum == 1)
        {
            #region AktifSopa

            if (_BellekYonetim.VeriOku_i("AktifSopa") == -1)
            {
                foreach (var item in Sopalar)
                {
                    item.SetActive(false);
                }
                
                TextObjeleri[3].text = SatinAlmaText;
                islemButonlari[0].interactable = false;
                islemButonlari[1].interactable = false;
                
                if (!islem)
                {
                    SopaIndex = -1;
                    SopaText.text = "No Gun";
                }
            }
            else
            {
                foreach (var item in Sopalar)
                {
                    item.SetActive(false);
                }
                
                SopaIndex = _BellekYonetim.VeriOku_i("AktifSopa");
                Sopalar[SopaIndex].SetActive(true);

                SopaText.text = _ItemBilgileri[SopaIndex + 3].Item_Ad;
                TextObjeleri[3].text = SatinAlmaText;
                islemButonlari[0].interactable = false;
                islemButonlari[1].interactable = true;
            }

            #endregion
        }
        else if (Bolum == 2)
        {
            #region AktifBoya

            if (_BellekYonetim.VeriOku_i("AktifBoya") == -1)
            {
                if (!islem)
                {
                    TextObjeleri[3].text = SatinAlmaText;
                    MaterialIndex = -1;
                    MaterialText.text = "No Color";
                    islemButonlari[0].interactable = false;
                    islemButonlari[1].interactable = false;
                }
                else
                {
                    Material[] mats = _Renderer.materials;
                    mats[0] = VarSayilanBoya;
                    _Renderer.materials = mats;
                }
            }
            else
            {
                MaterialIndex = _BellekYonetim.VeriOku_i("AktifBoya");
                Material[] mats = _Renderer.materials;
                mats[0] = Materyaller[MaterialIndex];
                _Renderer.materials = mats;
                
                MaterialText.text = _ItemBilgileri[MaterialIndex + 6].Item_Ad;
                TextObjeleri[3].text = SatinAlmaText;
                islemButonlari[0].interactable = false;
                islemButonlari[1].interactable = true;
            }

            #endregion
        }
    }
    public void SatinAl()
    {
        Sesler[1].Play();
        if (AktifislemPaneliIndex != -1)
        {
            switch (AktifislemPaneliIndex)
            {
                case 0:
                    //Debug.Log("Bölüm no: " + AktifislemPaneliIndex + " Item Index " + SapkaIndex + " Item Ad: " + _ItemBilgileri[SapkaIndex].Item_Ad);
                    SatinAlmaSonuc(SapkaIndex);
                    break;
                case 1:
                    //Debug.Log("Bölüm no: " + AktifislemPaneliIndex + " Item Index " + SopaIndex + " Item Ad: " + _ItemBilgileri[SopaIndex+3].Item_Ad);
                    int Index = SopaIndex + 3;
                    SatinAlmaSonuc(Index);
                    break;
                case 2:
                    //Debug.Log("Bölüm no: " + AktifislemPaneliIndex + " Item Index " + MaterialIndex + " Item Ad: " + _ItemBilgileri[MaterialIndex+6].Item_Ad);
                    int Index2 = MaterialIndex + 6;
                    SatinAlmaSonuc(Index2);
                    break;
            }
        }
    }
    public void Kaydet()
    {
        Sesler[2].Play();
        if (AktifislemPaneliIndex != -1)
        {
            switch (AktifislemPaneliIndex)
            {
                case 0:
                    KaydetmeSonuc("AktifSapka", SapkaIndex);
                    break;
                case 1:
                    KaydetmeSonuc("AktifSopa", SopaIndex);
                    break;
                case 2:
                    KaydetmeSonuc("AktifBoya", MaterialIndex);
                    break;
            }
        }
    }
    public void Sapka_Yonbutonları(string islem)
    {
        Sesler[0].Play();
        if (islem == "ileri")
        {
            if (SapkaIndex == -1)
            {
                SapkaIndex = 0;
                Sapkalar[SapkaIndex].SetActive(true);
                SapkaText.text = _ItemBilgileri[SapkaIndex].Item_Ad;

                if (!_ItemBilgileri[SapkaIndex].SatinAlmaDurumu)
                {
                    TextObjeleri[3].text = _ItemBilgileri[SapkaIndex].Para + " - " + SatinAlmaText;
                    islemButonlari[1].interactable = false;
                    if (_BellekYonetim.VeriOku_i("Puan") < _ItemBilgileri[SapkaIndex].Para)
                    {
                        islemButonlari[0].interactable = false;
                    }
                    else
                    {
                        islemButonlari[0].interactable = true;
                    }
                }
                else
                {
                    TextObjeleri[3].text = SatinAlmaText;
                    islemButonlari[0].interactable = false;
                    islemButonlari[1].interactable = true;
                }
            }
            else
            {
                Sapkalar[SapkaIndex].SetActive(false);
                SapkaIndex++;
                Sapkalar[SapkaIndex].SetActive(true);
                SapkaText.text = _ItemBilgileri[SapkaIndex].Item_Ad;
                
                if (!_ItemBilgileri[SapkaIndex].SatinAlmaDurumu)
                {
                    TextObjeleri[3].text = _ItemBilgileri[SapkaIndex].Para + " - " + SatinAlmaText;
                    islemButonlari[1].interactable = false;
                    if (_BellekYonetim.VeriOku_i("Puan") < _ItemBilgileri[SapkaIndex].Para)
                    {
                        islemButonlari[0].interactable = false;
                    }
                    else
                    {
                        islemButonlari[0].interactable = true;
                    }
                }
                else
                {
                    TextObjeleri[3].text = SatinAlmaText;
                    islemButonlari[0].interactable = false;
                    islemButonlari[1].interactable = true;
                }
            }

            if (SapkaIndex == Sapkalar.Length - 1)
            {
                SapkaButonlari[1].interactable = false;
            }
            else
            {
                SapkaButonlari[1].interactable = true;
            }

            if (SapkaIndex != -1)
            {
                SapkaButonlari[0].interactable = true;
            }
        }
        else
        {
            if (SapkaIndex != -1)
            {
                Sapkalar[SapkaIndex].SetActive(false);
                SapkaIndex--;
                if (SapkaIndex != -1)
                {
                    Sapkalar[SapkaIndex].SetActive(true);
                    SapkaButonlari[0].interactable = true;
                    SapkaText.text = _ItemBilgileri[SapkaIndex].Item_Ad;
                    
                    if (!_ItemBilgileri[SapkaIndex].SatinAlmaDurumu)
                    {
                        TextObjeleri[3].text = _ItemBilgileri[SapkaIndex].Para + " - " + SatinAlmaText;
                        islemButonlari[1].interactable = false;
                        if (_BellekYonetim.VeriOku_i("Puan") < _ItemBilgileri[SapkaIndex].Para)
                        {
                            islemButonlari[0].interactable = false;
                        }
                        else
                        {
                            islemButonlari[0].interactable = true;
                        }
                    }
                    else
                    {
                        TextObjeleri[3].text = SatinAlmaText;
                        islemButonlari[0].interactable = false;
                        islemButonlari[1].interactable = true;
                    }
                }
                else
                {
                    SapkaButonlari[0].interactable = false;
                    SapkaText.text = "No Hat";
                    TextObjeleri[3].text = SatinAlmaText;
                    islemButonlari[0].interactable = false;
                }
            }
            else
            {
                SapkaButonlari[0].interactable = false;
                SapkaText.text = "No Hat";
                TextObjeleri[3].text = SatinAlmaText;
                islemButonlari[0].interactable = false;
            }
            if (SapkaIndex != Sapkalar.Length - 1)
            {
                SapkaButonlari[1].interactable = true;
            }
        }
    }
    public void Sopa_Yonbutonları(string islem)
    {
        Sesler[0].Play();
        if (islem == "ileri")
        {
            if (SopaIndex == -1)
            {
                SopaIndex = 0;
                Sopalar[SopaIndex].SetActive(true);
                SopaText.text = _ItemBilgileri[SopaIndex + 3].Item_Ad;
                
                if (!_ItemBilgileri[SopaIndex + 3].SatinAlmaDurumu)
                {
                    TextObjeleri[3].text = _ItemBilgileri[SopaIndex + 3].Para + " - " + SatinAlmaText;
                    islemButonlari[1].interactable = false;
                    if (_BellekYonetim.VeriOku_i("Puan") < _ItemBilgileri[SopaIndex + 3].Para)
                    {
                        islemButonlari[0].interactable = false;
                    }
                    else
                    {
                        islemButonlari[0].interactable = true;
                    }
                }
                else
                {
                    TextObjeleri[3].text = SatinAlmaText;
                    islemButonlari[0].interactable = false;
                    islemButonlari[1].interactable = true;
                }
            }
            else
            {
                Sopalar[SopaIndex].SetActive(false);
                SopaIndex++;
                Sopalar[SopaIndex].SetActive(true);
                SopaText.text = _ItemBilgileri[SopaIndex + 3].Item_Ad;
                
                if (!_ItemBilgileri[SopaIndex + 3].SatinAlmaDurumu)
                {
                    TextObjeleri[3].text = _ItemBilgileri[SopaIndex + 3].Para + " - " + SatinAlmaText;
                    islemButonlari[1].interactable = false;
                    if (_BellekYonetim.VeriOku_i("Puan") < _ItemBilgileri[SopaIndex + 3].Para)
                    {
                        islemButonlari[0].interactable = false;
                    }
                    else
                    {
                        islemButonlari[0].interactable = true;
                    }
                }
                else
                {
                    TextObjeleri[3].text = SatinAlmaText;
                    islemButonlari[0].interactable = false;
                    islemButonlari[1].interactable = true;
                }
            }

            if (SopaIndex == Sopalar.Length - 1)
            {
                SopaButonlari[1].interactable = false;
            }
            else
            {
                SopaButonlari[1].interactable = true;
            }

            if (SopaIndex != -1)
            {
                SopaButonlari[0].interactable = true;
            }
        }
        else
        {
            if (SopaIndex != -1)
            {
                Sopalar[SopaIndex].SetActive(false);
                SopaIndex--;
                if (SopaIndex != -1)
                {
                    Sopalar[SopaIndex].SetActive(true);
                    SopaButonlari[0].interactable = true;
                    SopaText.text = _ItemBilgileri[SopaIndex + 3].Item_Ad;
                    
                    if (!_ItemBilgileri[SopaIndex + 3].SatinAlmaDurumu)
                    {
                        TextObjeleri[3].text = _ItemBilgileri[SopaIndex + 3].Para + " - " + SatinAlmaText;
                        islemButonlari[1].interactable = false;
                        if (_BellekYonetim.VeriOku_i("Puan") < _ItemBilgileri[SopaIndex + 3].Para)
                        {
                            islemButonlari[0].interactable = false;
                        }
                        else
                        {
                            islemButonlari[0].interactable = true;
                        }
                    }
                    else
                    {
                        TextObjeleri[3].text = SatinAlmaText;
                        islemButonlari[0].interactable = false;
                        islemButonlari[1].interactable = true;
                    }
                }
                else
                {
                    SopaButonlari[0].interactable = false;
                    SopaText.text = "No Gun";
                    TextObjeleri[3].text = SatinAlmaText;
                    islemButonlari[0].interactable = false;
                }
            }
            else
            {
                SopaButonlari[0].interactable = false;
                SopaText.text = "No Gun";
                TextObjeleri[3].text = SatinAlmaText;
                islemButonlari[0].interactable = false;
            }
            if (SopaIndex != Sopalar.Length - 1)
            {
                SopaButonlari[1].interactable = true;
            }
        }
    }
    public void Material_Yonbutonları(string islem)
    {
        Sesler[0].Play();
        if (islem == "ileri")
        {
            if (MaterialIndex == -1)
            {
                MaterialIndex  = 0;
                Material[] mats = _Renderer.materials;
                mats[0] = Materyaller[0];
                _Renderer.materials = mats;

                MaterialText.text = _ItemBilgileri[MaterialIndex  + 6].Item_Ad;
                
                if (!_ItemBilgileri[MaterialIndex + 6].SatinAlmaDurumu)
                {
                    TextObjeleri[3].text = _ItemBilgileri[MaterialIndex + 6].Para + " - " + SatinAlmaText;
                    islemButonlari[1].interactable = false;
                    if (_BellekYonetim.VeriOku_i("Puan") < _ItemBilgileri[MaterialIndex + 6].Para)
                    {
                        islemButonlari[0].interactable = false;
                    }
                    else
                    {
                        islemButonlari[0].interactable = true;
                    }
                }
                else
                {
                    TextObjeleri[3].text = SatinAlmaText;
                    islemButonlari[0].interactable = false;
                    islemButonlari[1].interactable = true;
                }
            }
            else
            {
                MaterialIndex ++;
                Material[] mats = _Renderer.materials;
                mats[0] = Materyaller[MaterialIndex];
                _Renderer.materials = mats;
                MaterialText.text = _ItemBilgileri[MaterialIndex  + 6].Item_Ad;
                
                if (!_ItemBilgileri[MaterialIndex + 6].SatinAlmaDurumu)
                {
                    TextObjeleri[3].text = _ItemBilgileri[MaterialIndex + 6].Para + " - " + SatinAlmaText;
                    islemButonlari[1].interactable = false;
                    if (_BellekYonetim.VeriOku_i("Puan") < _ItemBilgileri[MaterialIndex + 6].Para)
                    {
                        islemButonlari[0].interactable = false;
                    }
                    else
                    {
                        islemButonlari[0].interactable = true;
                    }
                }
                else
                {
                    TextObjeleri[3].text = SatinAlmaText;
                    islemButonlari[0].interactable = false;
                    islemButonlari[1].interactable = true;
                }
            }

            if (MaterialIndex  == Materyaller.Length - 1)
            {
                MateryalButonlari[1].interactable = false;
            }
            else
            {
                MateryalButonlari[1].interactable = true;
            }

            if (MaterialIndex  != -1)
            {
                MateryalButonlari[0].interactable = true;
            }
        }
        else
        {
            if (MaterialIndex  != -1)
            {
                MaterialIndex--;
                if (MaterialIndex  != -1)
                {
                    Material[] mats = _Renderer.materials;
                    mats[0] = Materyaller[MaterialIndex];
                    _Renderer.materials = mats;
                    
                    MateryalButonlari[0].interactable = true;
                    MaterialText.text = _ItemBilgileri[MaterialIndex + 6].Item_Ad;
                    
                    if (!_ItemBilgileri[MaterialIndex + 6].SatinAlmaDurumu)
                    {
                        TextObjeleri[3].text = _ItemBilgileri[MaterialIndex + 6].Para + " - " + SatinAlmaText;
                        islemButonlari[1].interactable = false;
                        if (_BellekYonetim.VeriOku_i("Puan") < _ItemBilgileri[MaterialIndex + 6].Para)
                        {
                            islemButonlari[0].interactable = false;
                        }
                        else
                        {
                            islemButonlari[0].interactable = true;
                        }
                    }
                    else
                    {
                        TextObjeleri[3].text = SatinAlmaText;
                        islemButonlari[0].interactable = false;
                        islemButonlari[1].interactable = true;
                    }
                }
                else
                {
                    Material[] mats = _Renderer.materials;
                    mats[0] = VarSayilanBoya;
                    _Renderer.materials = mats;
                    
                    MateryalButonlari[0].interactable = false;
                    MaterialText.text = "No Color";
                    TextObjeleri[3].text = SatinAlmaText;
                    islemButonlari[0].interactable = false;
                }
            }
            else
            {
                Material[] mats = _Renderer.materials;
                mats[0] = VarSayilanBoya;
                _Renderer.materials = mats;
                
                MateryalButonlari[0].interactable = false;
                MaterialText.text = "No Color";
                TextObjeleri[3].text = SatinAlmaText;
                islemButonlari[0].interactable = false;
            }
            if (MaterialIndex != Materyaller.Length - 1)
            {
                MateryalButonlari[1].interactable = true;
            }
        }
    }
    public void islemPaneliCikart(int Index)
    {
        Sesler[0].Play();
        DurumuKontrolEt(Index);
        
        GenelPanaller[0].SetActive(true);
        AktifislemPaneliIndex = Index;
        islemPanelleri[Index].SetActive(true);
        GenelPanaller[1].SetActive(true);
        islemCanvasi.SetActive(false);
    }
    public void GeriDon()
    {
        Sesler[0].Play();
        GenelPanaller[0].SetActive(false);
        islemCanvasi.SetActive(true);
        GenelPanaller[1].SetActive(false);
        islemPanelleri[AktifislemPaneliIndex].SetActive(false);
        
        DurumuKontrolEt(AktifislemPaneliIndex, true);
        
        AktifislemPaneliIndex = -1;
    }
    public void AnaMenuyeDon()
    {
        Sesler[0].Play();
        _VeriYonetimi.Save(_ItemBilgileri);
        SceneManager.LoadScene(0);
    }
    // ---------------------------------
    void SatinAlmaSonuc(int Index)
    {
        _ItemBilgileri[Index].SatinAlmaDurumu = true;
        _BellekYonetim.VeriKaydet_int("Puan", _BellekYonetim.VeriOku_i("Puan") - _ItemBilgileri[Index].Para);
                    
        TextObjeleri[3].text = SatinAlmaText;
        islemButonlari[0].interactable = false;
        islemButonlari[1].interactable = true;
                    
        ParaText.text = _BellekYonetim.VeriOku_i("Puan").ToString();
    }
    void KaydetmeSonuc(string key, int Index)
    {
        _BellekYonetim.VeriKaydet_int(key, Index);
        islemButonlari[1].interactable = false;
        if (!Kaydedildi_Animator.GetBool("ok"))
        {
            Kaydedildi_Animator.SetBool("ok", true);
        }
    }
}