using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ayberk;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public LevelYukleyici levelYukleyici;
    
    public static GameManager Instance;
    
    public static int AnlikKarakterSayisi = 1;
    public List<GameObject> Karakterler;
    public List<GameObject> OlusmaEfektleri;
    public List<GameObject> YokOlmaEfektleri;
    public List<GameObject> AdamLekesiEfektleri;

    [Header("LEVEL VERİLERİ")] public List<GameObject> Dusmanlar;
    public List<GameObject> Bosslar;
    public bool BossLevel;
    public List<GameObject> DusmanlarKontrol;
    public int KacDusmanOlsun;
    public int KacBossOlsun;
    public bool OyunBittimi;
    public bool SonaGeldikmi;

    [Header("----SAPKALAR----")] public GameObject[] Sapkalar;
    [Header("----SOPALAR----")] public GameObject[] Sopalar;
    [Header("----MATERİAL----")] public Material[] Materyaller;
    public SkinnedMeshRenderer _Renderer;
    public Material VarSayilanBoya;

    private Matematiksel_islemler _Matematiksel_islemler = new Matematiksel_islemler();
    private BellekYonetim _BellekYonetim = new BellekYonetim();
    VeriYonetimi _VeriYonetimi = new VeriYonetimi();
    ReklamYonetim _ReklamYonetimi = new ReklamYonetim();

    private Scene _Scene;

    [Header("GENEL VERİLER")] 
    
    public AudioSource[] Sesler;
    
    public SoundManager _SoundManager;

    public Button[] genelButonlar;
    
    public List<Button> OdulluReklamButton;
    
    public GameObject[] islemPanelleri;
    public Slider OyunSesiAyar;
    public List<DilVerileriAnaObje> _DilVerileriAnaObje = new List<DilVerileriAnaObje>();
    List<DilVerileriAnaObje> _DilOkunanVeriler = new List<DilVerileriAnaObje>();
    public TextMeshProUGUI[] TextObjeleri;
    [Header("LOADİNG VERİLER")] public GameObject YuklemeEkrani;
    public Slider YuklemeSlider;

    public TextMeshProUGUI kazanilanAltin;
    public TextMeshProUGUI kazanilanElmas;

    public int kazanilanMaxAltin = 70;
    public int kazanilanMaxElmas = 4;

    public int kazanilanMinAltin = 20;
    public int kazanilanMinElmas = 1;

    public TextMeshProUGUI kaybedilenAltin;
    public TextMeshProUGUI kaybedilenElmas;

    public int kaybedilenMaxAltin = 40;
    public int kaybedilenMaxElmas = 2;

    public int kaybedilenMinAltin = 15;
    public int kaybedilenMinElmas = 1;

    public Text kaybettinReklamMiktari;
    public Text kazandinReklamMiktari;

    public TextMeshProUGUI karaktersayisi;

    private bool SavasDurumuCagirildi = false;
    private int SonParaKazanci;

    void Start()
    {
        DynamicGI.UpdateEnvironment();

        MobileAds.Initialize(initStatus => { Debug.Log("AdMob initialized."); });
        
        _ReklamYonetimi.RequestRewardedAd();
                            
        _ReklamYonetimi.RequestBanner();

        AnlikKarakterSayisi = 1;

        //Destroy(GameObject.FindWithTag("MenuSes"));

        _SoundManager = SoundManager.Instance;
        
        Sesler[0] = _SoundManager.gameObject.transform.GetChild(0).GetComponent<AudioSource>();
        Sesler[1] = _SoundManager.gameObject.transform.GetChild(1).GetComponent<AudioSource>();
        
        if (Sesler.Length > 0 && Sesler[0] != null)
            Sesler[0].volume = _BellekYonetim.VeriOku_f("OyunSes");

        if (OyunSesiAyar != null)
            OyunSesiAyar.value = _BellekYonetim.VeriOku_f("OyunSes");

        if (Sesler.Length > 1 && Sesler[1] != null)
            Sesler[1].volume = _BellekYonetim.VeriOku_f("MenuFx");

        ItemleriKontrolEt();

        OdulluReklamButton[0].interactable = true;
        OdulluReklamButton[1].interactable = true;
        
        for (int i = 0; i < _BellekYonetim.VeriOku_i("UpgradeCharacter") && i < Karakterler.Count; i++)
        {
            if (Karakterler[i] != null)
            {
                Vector3 yeniPoz = new Vector3(Karakterler[i].transform.position.x, .23f, Karakterler[i].transform.position.z);
                Karakterler[i].transform.position = yeniPoz;
                AnlikKarakterSayisi++;
                Karakterler[i].SetActive(true);
            }
        }

        ButonlariKapat("musait");

        DusmanlariOlustur();
        _Scene = SceneManager.GetActiveScene();
        print(_Scene.name);

        _VeriYonetimi.Dil_Load();
        _DilOkunanVeriler = _VeriYonetimi.DilVerileriListeyiAktar();

        if (_DilOkunanVeriler.Count > 5)
        {
            _DilVerileriAnaObje.Add(_DilOkunanVeriler[5]);
            DilTercihiYonetimi();
        }
        else
        {
            Debug.LogError("Dil verisi eksik: 6. eleman yok!");
        }
        
        if (_BellekYonetim.VeriOku_f("AltinCarpani") <= 0f)
        {
            _BellekYonetim.VeriKaydet_float("AltinCarpani", 1f);
        }
    }

    private void Update()
    {
        if (karaktersayisi != null)
            karaktersayisi.text = AnlikKarakterSayisi.ToString();
    }

    public void DilTercihiYonetimi()
    {
        if (_DilVerileriAnaObje.Count == 0) return;

        if (_BellekYonetim.VeriOku_s("Dil") == "TR")
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
            {
                if (_DilVerileriAnaObje[0]._DilVerileri_TR.Count > i)
                    TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_TR[i].Metin;
            }
        }
        else
        {
            for (int i = 0; i < TextObjeleri.Length; i++)
            {
                if (_DilVerileriAnaObje[0]._DilVerileri_EN.Count > i)
                    TextObjeleri[i].text = _DilVerileriAnaObje[0]._DilVerileri_EN[i].Metin;
            }
        }
    }

    public void DusmanlariOlustur()
    {
        if (BossLevel)
        {
            KacDusmanOlsun = 0;
            for (int i = 0; i < KacBossOlsun && i < Bosslar.Count; i++)
            {
                if (Bosslar[i] != null)
                {
                    Bosslar[i].SetActive(true);
                }
            }
        }
        else
        {
            for (int i = 0; i < KacDusmanOlsun && i < Dusmanlar.Count; i++)
            {
                if (Dusmanlar[i] != null)
                {
                    Dusmanlar[i].SetActive(true);
                    DusmanlarKontrol.Add(Dusmanlar[i]);
                }
            }
        }
    }

    public void DusmanlariTetikle()
    {
        if (BossLevel)
        {
            foreach (var item in Bosslar)
            {
                if (item != null && item.activeInHierarchy)
                {
                    item.GetComponent<Dusman>()?.AnimasyonTetikle();
                }
            }
        }
        else
        {
            foreach (var item in Dusmanlar)
            {
                if (item != null && item.activeInHierarchy)
                {
                    item.GetComponent<Dusman>()?.AnimasyonTetikle();
                }
            }
        }

        SonaGeldikmi = true;
        SavasDurumu();
        Debug.Log("Savas Durumu");
    }

    void SavasDurumu()
    {
        int kazanc = 0;
        int altin = 0;
        int elmas = 0;

        if (SavasDurumuCagirildi)
            return;

        if (!SonaGeldikmi)
        {
            ButonlariKapat("musait");
            return;
        }

        ButonlariKapat("savas");

        if (BossLevel)
        {
            if (AnlikKarakterSayisi == 1 || DusmanlarKontrol.Count == 0 || KacBossOlsun == 0)
            {
                OyunBittimi = true;
                /*foreach (var item in Dusmanlar)
                     item?.GetComponent<Animator>()?.SetBool("Saldir", false);

                 foreach (var item in Karakterler)
                     item?.GetComponent<Animator>()?.SetBool("Saldir", false);

                 _AnaKarakter?.GetComponent<Animator>()?.SetBool("Saldir", false);*/

                if (KacBossOlsun > 0 && AnlikKarakterSayisi == 1)
                {
                    altin = Random.Range(kaybedilenMinAltin, kaybedilenMaxAltin);
                    elmas = Random.Range(kaybedilenMinElmas, kaybedilenMaxElmas);

                    kazanc = Mathf.RoundToInt(altin * _BellekYonetim.VeriOku_f("AltinCarpani"));
                    
                    _BellekYonetim.VeriKaydet_int("Puan", _BellekYonetim.VeriOku_i("Puan") + kazanc);
                    _BellekYonetim.VeriKaydet_int("Elmas", _BellekYonetim.VeriOku_i("Elmas") + elmas);

                    kaybedilenAltin.text = kazanc.ToString();
                    kaybedilenElmas.text = elmas.ToString();
                    
                    int deger = kazanc * 2;
                    kaybettinReklamMiktari.text = deger.ToString();

                    //islemPanelleri[3]?.SetActive(true);
                    StartCoroutine(OpenAsyncPanel(islemPanelleri[3]));
                    _ReklamYonetimi.RequestInterstitial();
                    SavasDurumuCagirildi = true;
                }
                else if (KacBossOlsun <= 0 && AnlikKarakterSayisi > 0)
                {
                    altin = Random.Range(kazanilanMinAltin, kazanilanMaxAltin);
                    elmas = Random.Range(kazanilanMinElmas, kazanilanMaxElmas);

                    kazanc = Mathf.RoundToInt(altin * _BellekYonetim.VeriOku_f("AltinCarpani"));
                    
                    _BellekYonetim.VeriKaydet_int("Puan", _BellekYonetim.VeriOku_i("Puan") + kazanc);
                    _BellekYonetim.VeriKaydet_int("Elmas", _BellekYonetim.VeriOku_i("Elmas") + elmas);

                    kazanilanAltin.text = kazanc.ToString();
                    kazanilanElmas.text = elmas.ToString();
                    
                    int deger = kazanc * 2;
                    kazandinReklamMiktari.text = deger.ToString();
                    
                    int levelartirma = _BellekYonetim.VeriOku_i("SonLevel") + 1;
                    _BellekYonetim.VeriKaydet_int("SonLevel", levelartirma);
                    
                    Debug.Log(_BellekYonetim.VeriOku_i("SonLevel"));

                    //islemPanelleri[2]?.SetActive(true);
                    StartCoroutine(OpenAsyncPanel(islemPanelleri[2]));
                    _ReklamYonetimi.RequestInterstitial();
                    SavasDurumuCagirildi = true;
                }
            }
        }
        else
        {
            if (AnlikKarakterSayisi == 1 || DusmanlarKontrol.Count == 0 || KacBossOlsun == 0)
            {
                OyunBittimi = true;

                /*foreach (var item in Dusmanlar)
                     item?.GetComponent<Animator>()?.SetBool("Saldir", false);

                 foreach (var item in Karakterler)
                     item?.GetComponent<Animator>()?.SetBool("Saldir", false);

                 _AnaKarakter?.GetComponent<Animator>()?.SetBool("Saldir", false);*/

                if (KacDusmanOlsun > 0 && AnlikKarakterSayisi == 1)
                {
                    Debug.Log("Kaybettik");
                    altin = Random.Range(kaybedilenMinAltin, kaybedilenMaxAltin);
                    elmas = Random.Range(kaybedilenMinElmas, kaybedilenMaxElmas);

                    kazanc = Mathf.RoundToInt(altin * _BellekYonetim.VeriOku_f("AltinCarpani"));
                    
                    _BellekYonetim.VeriKaydet_int("Puan", _BellekYonetim.VeriOku_i("Puan") + kazanc);
                    _BellekYonetim.VeriKaydet_int("Elmas", _BellekYonetim.VeriOku_i("Elmas") + elmas);

                    kaybedilenAltin.text = kazanc.ToString();
                    kaybedilenElmas.text = elmas.ToString();
                    
                    int deger = kazanc * 2;
                    kaybettinReklamMiktari.text = deger.ToString();

                    //islemPanelleri[3]?.SetActive(true);
                    StartCoroutine(OpenAsyncPanel(islemPanelleri[3]));
                    _ReklamYonetimi.RequestInterstitial();
                    SavasDurumuCagirildi = true;
                }
                else if (KacDusmanOlsun <= 0 && AnlikKarakterSayisi > 0)
                {
                    Debug.Log("Kazandık");
                    altin = Random.Range(kazanilanMinAltin, kazanilanMaxAltin);
                    elmas = Random.Range(kazanilanMinElmas, kazanilanMaxElmas);
                    
                    kazanc = Mathf.RoundToInt(altin * _BellekYonetim.VeriOku_f("AltinCarpani"));
                    
                    _BellekYonetim.VeriKaydet_int("Puan", _BellekYonetim.VeriOku_i("Puan") + kazanc);
                    _BellekYonetim.VeriKaydet_int("Elmas", _BellekYonetim.VeriOku_i("Elmas") + elmas);

                    kazanilanAltin.text = kazanc.ToString();
                    kazanilanElmas.text = elmas.ToString();
                    
                    int deger = kazanc * 2;
                    kazandinReklamMiktari.text = deger.ToString();

                    int levelartirma = _BellekYonetim.VeriOku_i("SonLevel") + 1;
                    _BellekYonetim.VeriKaydet_int("SonLevel", levelartirma);
                    
                    Debug.Log(_BellekYonetim.VeriOku_i("SonLevel"));

                    //islemPanelleri[2]?.SetActive(true);
                    StartCoroutine(OpenAsyncPanel(islemPanelleri[2]));
                    _ReklamYonetimi.RequestInterstitial();
                    SavasDurumuCagirildi = true;
                }
            }
        }

        SonParaKazanci = kazanc;

        /* if (AnlikKarakterSayisi == 1 || DusmanlarKontrol.Count == 0 || KacBossOlsun == 0)
         {
             OyunBittimi = true;

             foreach (var item in Dusmanlar)
                 item?.GetComponent<Animator>()?.SetBool("Saldir", false);

             foreach (var item in Karakterler)
                 item?.GetComponent<Animator>()?.SetBool("Saldir", false);

             _AnaKarakter?.GetComponent<Animator>()?.SetBool("Saldir", false);
             //  _ReklamYonetimi.GecisReklamiGoster();

             if (AnlikKarakterSayisi < DusmanlarKontrol.Count || AnlikKarakterSayisi < KacBossOlsun)
             {
                 int altin = Random.Range(kaybedilenMinAltin, kaybedilenMaxAltin);
                 int elmas = Random.Range(kaybedilenMinElmas, kaybedilenMaxElmas);

                 _BellekYonetim.VeriKaydet_int("Puan", _BellekYonetim.VeriOku_i("Puan") + altin);
                 _BellekYonetim.VeriKaydet_int("Elmas", _BellekYonetim.VeriOku_i("Elmas") + elmas);

                 kaybedilenAltin.text = altin.ToString();
                 kaybedilenElmas.text = elmas.ToString();
                 islemPanelleri[3]?.SetActive(true);
             }
             else
             {
                 if (BossLevel)
                 {
                     if ( Bosslar[0].gameObject.GetComponent<Dusman>().boss.hp <= 0 && AnlikKarakterSayisi > 0)
                     {
                         int altin = Random.Range(kazanilanMinAltin, kazanilanMaxAltin);
                         int elmas = Random.Range(kazanilanMinElmas, kazanilanMaxElmas);

                         _BellekYonetim.VeriKaydet_int("Puan", _BellekYonetim.VeriOku_i("Puan") + altin);
                         _BellekYonetim.VeriKaydet_int("Elmas", _BellekYonetim.VeriOku_i("Elmas") + elmas);

                         kazanilanAltin.text = altin.ToString();
                         kazanilanElmas.text = elmas.ToString();

                         if (_Scene.buildIndex == _BellekYonetim.VeriOku_i("SonLevel"))
                             _BellekYonetim.VeriKaydet_int("SonLevel", _Scene.buildIndex + 1);

                         islemPanelleri[2]?.SetActive(true);
                     }
                 }
                 else
                 {

                     if (KacDusmanOlsun <= 0 && AnlikKarakterSayisi > 0)
                     {
                         Debug.Log("Kazandık");
                         int altin = Random.Range(kazanilanMinAltin, kazanilanMaxAltin);
                         int elmas = Random.Range(kazanilanMinElmas, kazanilanMaxElmas);

                         _BellekYonetim.VeriKaydet_int("Puan", _BellekYonetim.VeriOku_i("Puan") + altin);
                         _BellekYonetim.VeriKaydet_int("Elmas", _BellekYonetim.VeriOku_i("Elmas") + elmas);

                         kazanilanAltin.text = altin.ToString();
                         kazanilanElmas.text = elmas.ToString();

                         if (_Scene.buildIndex == _BellekYonetim.VeriOku_i("SonLevel"))
                             _BellekYonetim.VeriKaydet_int("SonLevel", _Scene.buildIndex + 1);

                         islemPanelleri[2]?.SetActive(true);
                     }
                 }
             }
         }*/
    }

    public void AdamYonetim(string islemturu, int GelenSayi, Transform Pozisyon)
    {
        switch (islemturu)
        {
            case "Carpma":
                _Matematiksel_islemler.Carpma(GelenSayi, Karakterler, Pozisyon, OlusmaEfektleri);
                break;
            case "Toplama":
                _Matematiksel_islemler.Toplama(GelenSayi, Karakterler, Pozisyon, OlusmaEfektleri);
                break;
            case "Cikartma":
                _Matematiksel_islemler.Cikartma(GelenSayi, Karakterler, YokOlmaEfektleri);
                break;
            case "Bolme":
                _Matematiksel_islemler.Bolme(GelenSayi, Karakterler, YokOlmaEfektleri);
                break;
        }
    }

    public void YokolmaEfektiOlustur(Vector3 Pozisyon, bool Balyoz = false, bool Durum = false, bool Bossmu = false)
    {
        foreach (var item in YokOlmaEfektleri)
        {
            if (!item.activeInHierarchy)
            {
                item.SetActive(true);
                item.transform.position = Pozisyon;
                item.GetComponent<ParticleSystem>()?.Play();
                item.GetComponent<AudioSource>()?.Play();



                if (!Durum)
                    AnlikKarakterSayisi--;
                else if (Durum)
                    KacDusmanOlsun--;

                if (Bossmu)
                    KacBossOlsun--;

                SavasDurumu();
                Debug.Log("Savas Durumu");
                break;
            }
        }

        if (Balyoz)
        {
            Vector3 yeniPoz = new Vector3(Pozisyon.x, .005f, Pozisyon.z);
            foreach (var item in AdamLekesiEfektleri)
            {
                if (!item.activeInHierarchy)
                {
                    item.SetActive(true);
                    item.transform.position = yeniPoz;
                    break;
                }
            }
        }

        if (!OyunBittimi) { SavasDurumu(); Debug.Log("Savas Durumu"); }
    }

    public void ItemleriKontrolEt()
    {
        int aktifSapka = _BellekYonetim.VeriOku_i("AktifSapka");
        if (aktifSapka >= 0 && aktifSapka < Sapkalar.Length)
            Sapkalar[aktifSapka]?.SetActive(true);

        int aktifSopa = _BellekYonetim.VeriOku_i("AktifSopa");
        if (aktifSopa >= 0 && aktifSopa < Sopalar.Length)
            Sopalar[aktifSopa]?.SetActive(true);

        int aktifBoya = _BellekYonetim.VeriOku_i("AktifBoya");
        Material[] mats = _Renderer.materials;
        if (aktifBoya >= 0 && aktifBoya < Materyaller.Length)
            mats[0] = Materyaller[aktifBoya];
        else
            mats[0] = VarSayilanBoya;

        _Renderer.materials = mats;
    }

    public void CikisButonislem(string durum)
    {
        Sesler[1]?.Play();
        //ButonSes.Play();
        if (genelButonlar.Length > 0) genelButonlar[0].interactable = false;
        Time.timeScale = 0;

        if (durum == "durdur")
            islemPanelleri[0]?.SetActive(true);
        else if (durum == "devamet")
        {
            islemPanelleri[0]?.SetActive(false);
            genelButonlar[0].interactable = true;
            Time.timeScale = 1;
        }
        else if (durum == "tekrar")
        {
            genelButonlar[0].interactable = true;
            SceneManager.LoadScene(_Scene.buildIndex);
            Time.timeScale = 1;
        }
        else if (durum == "anasayfa")
        {
            genelButonlar[0].interactable = true;
            Sesler[0].Stop();
            SceneManager.LoadScene(0);
            Time.timeScale = 1;
        }
    }

    public void Ayarlar(string durum)
    {
        if (durum == "ayarla")
        {
            islemPanelleri[1]?.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            islemPanelleri[1]?.SetActive(false);
            Time.timeScale = 1;
        }
    }

    public void SonrakiLevel()
    {
        kazanilanMaxAltin += 20;
        kazanilanMaxElmas += 2;

        kazanilanMinAltin += 15;
        kazanilanMinElmas += 1;

        kaybedilenMaxAltin += 15;
        kaybedilenMaxElmas += 1;

        kaybedilenMinAltin += 6;

        SceneManager.LoadScene(0);
    }

    public void SesiAyarla()
    {
        _BellekYonetim.VeriKaydet_float("OyunSes", OyunSesiAyar.value);
        if (Sesler.Length > 0)
            Sesler[0].volume = OyunSesiAyar.value;
    }

    public void ButonlariKapat(string durum)
    {
        if (durum == "musait")
        {
            genelButonlar[0].interactable = true;
            genelButonlar[1].interactable = true;
        }
        else if (durum == "savas")
        {
            genelButonlar[0].interactable = false;
            genelButonlar[1].interactable = false;
        }
    }

    IEnumerator LoadAsync(int SceneIndex)
    {
        AsyncOperation Op = SceneManager.LoadSceneAsync(SceneIndex);
        if (YuklemeEkrani != null)
            YuklemeEkrani.SetActive(true);

        while (!Op.isDone)
        {
            float progress = Mathf.Clamp01(Op.progress / 0.9f);
            if (YuklemeSlider != null)
                YuklemeSlider.value = progress;

            yield return null;
        }
    }

    public void OdulluReklam()
    {
        OdulluReklamButton[0].gameObject.SetActive(false);
        OdulluReklamButton[1].gameObject.SetActive(false);
        
        OdulluReklamButton[0].interactable = false;
        OdulluReklamButton[1].interactable = false;
        _ReklamYonetimi.OdulluReklamGoster(() =>
        {
            _BellekYonetim.VeriKaydet_int("Puan", _BellekYonetim.VeriOku_i("Puan") + SonParaKazanci);
            kazanilanAltin.text = (SonParaKazanci * 2).ToString();
            kazanilanAltin.text = SonParaKazanci.ToString();
        });
    }

    IEnumerator OpenAsyncPanel(GameObject panel)
    {
        yield return new WaitForSeconds(.5f);
        panel?.SetActive(true);
    }
}