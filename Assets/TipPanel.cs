using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using UnityEngine.Video;
using UniRx;

public class TipPanel : MonoBehaviour
{
    private Dictionary<string, string> ReplaceRules = new Dictionary<string, string>()
    {
        { "А","Δ"},
        { "Б","ƌ"},
        { "В","β"},
        { "Г","ɼ"},
        { "Д","█"},
        { "Е","ǝ"},
        { "Ё","ɇ"},
        { "Ж","ӿ"},
        { "З","ƺ"},
        { "И","ϰ"},
        { "Й","ï"},
        { "К","ӄ"},
        { "Л","λ"},
        { "М","µ"},
        { "Н","η"},
        { "О","σ"},
        { "П","π"},
        { "Р","þ"},
        { "С","¢"},
        { "Т","τ"},
        { "У","¥"},
        { "Ф","φ"},
        { "Х","×"},
        { "Ч","4"},
        { "Ц","ҵ"},
        { "Щ","Ϣ"},
        { "Ш","Ɯ"},
        { "Ъ","ȶ"},
        { "Ь","ȴ"},
        { "Э","€"},
        { "Ю","Ѥ"},
        { "Я","®"},
        {"?", "¿"},
        {"Ы", "ӹ"},
    };

    public float extraDistortion = 0.3f;

    public float Distortion = 0;

    [SerializeField]
    private TipsList Tips;

    [SerializeField]
    private BodySourceView Body;

    [SerializeField]
    private float symbolsPerSecond = 4f;

    [SerializeField]
    private float ExtraWaitTime = 7f;

    [SerializeField]
    private VideoPlayer player;

    [SerializeField]
    private Image img;

    private Coroutine ShowTipCoroutine;

    public bool CanBeTracked = true;

    [SerializeField]
    private GameObject WordPrefab;

    private PostProcessVolume volume;
    private ChromaticAberration abberation;
    private float aberationTime;
    [SerializeField]
    private AnimationCurve AberationCurve;
    public float CurrentAberation;

    [SerializeField]
    private Animator videoQuad;

    private List<Sprite> images = new List<Sprite>();
    private List<string> videos = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        Body.PoseStateChanged += PoseStateChanged;
        Body.TrackingStateChanged += TrackingStateChanged;
        StartCoroutine(BlinkRoutuine());

        volume = Camera.main.GetComponent<PostProcessVolume>();

        volume.profile.TryGetSettings(out abberation);
        
        LoadResources();

        player.prepareCompleted += PrepareCompleted;
    }

    private void PrepareCompleted(VideoPlayer source)
    {
        videoQuad.SetBool("Show", true);
        Debug.Log((float)source.length);
        player.Play();

        Observable.Timer(TimeSpan.FromSeconds((float)source.length - 1f)).Subscribe(v=>
        {
            Debug.Log("Video completed");
            GetComponent<Animator>().SetBool("Tip", false);
            OpenEyes();
            ShowTipCoroutine = null;
            CanBeTracked = true;
            Body.PoseStateChanged += PoseStateChanged;
            Debug.Log("HIde");
            videoQuad.SetBool("Show", false);
        }).AddTo(this);
       
    }

    private void LoadResources()
    {
        string videoFolderPath = Path.Combine(Application.persistentDataPath, "Videos");
        string imagesFolderPath = Path.Combine(Application.persistentDataPath, "Images");

        if (!Directory.Exists(videoFolderPath))
        {
            Directory.CreateDirectory(videoFolderPath);
        }
        if (!Directory.Exists(imagesFolderPath))
        {
            Directory.CreateDirectory(imagesFolderPath);
        }

        videos = Directory.GetFiles(videoFolderPath).ToList();
        string[] imagesPathes = Directory.GetFiles(imagesFolderPath);

        foreach (string s in imagesPathes)
        {
            byte[] bytes = File.ReadAllBytes(s);
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            images.Add(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
        }    
    }

    private void Update()
    {

        abberation.intensity.value = Mathf.Lerp(abberation.intensity.value, CurrentAberation, Time.deltaTime);

        CurrentAberation = AberationCurve.Evaluate(Time.timeSinceLevelLoad%AberationCurve.keys.OrderByDescending(k=>k.time).FirstOrDefault().time);

        if (Distortion>0)
        {
            Distortion -= Time.deltaTime * 0.0125f;
        }
    }

    private IEnumerator BlinkRoutuine()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(1.5f, 4f));
            foreach (EyesController eye in FindObjectsOfType<EyesController>())
            {
                eye.Blink();
            }
        }
    }

        private void TrackingStateChanged(bool v)
    {
        if (ShowTipCoroutine == null)
        {
            GetComponent<Animator>().SetBool("Looking", v);

            if (v)
            {
                OpenEyes();
            }
            else
            {
                CloseEyes();
                Distortion = 0;
            }
        }
      
    }

    private void PoseStateChanged(bool v)
    {

        if (v)
        {
            CloseEyes();
            Show();
        }
       
    }

    private void CloseEyes()
    {
        foreach (EyesController eye in FindObjectsOfType<EyesController>())
        {
            eye.Close();
        }
    }

    [ContextMenu("test")]
    public void Show()
    {
        Debug.Log(ShowTipCoroutine);

        if (ShowTipCoroutine!=null || CanBeTracked == false)
        {
            if (ShowTipCoroutine != null)
            {
                StopCoroutine(ShowTipCoroutine);
            }
        }
        else
        {
            string tip = Tips.Tips.OrderBy(t => Guid.NewGuid()).FirstOrDefault();

            int v = 1; UnityEngine.Random.Range(1, 3);

            switch (v)
            {
                case 0:
                    ShowTipCoroutine = StartCoroutine(ShowTip(tip));
                    break;
                case 1:
                    Sprite sprite = images.OrderBy(ss => Guid.NewGuid()).First();
                    ShowImage(sprite);
                    break;
                case 2:
                    ShowVideoTip(videos.OrderBy(s=>Guid.NewGuid()).First());
                    break;
            }         
        }
    }

    private void ShowImage(Sprite sprite)
    {
        Body.PoseStateChanged -= PoseStateChanged;
        CanBeTracked = false;
        GetComponent<Animator>().SetBool("Tip", true);

        img.sprite = sprite;
        Debug.Log(images.IndexOf(img.sprite));

       

        img.GetComponent<Animator>().SetBool("Show", true);
        Observable.Timer(TimeSpan.FromSeconds(5f)).Subscribe(v=>
        {
            Debug.Log("hide");
            img.GetComponent<Animator>().SetBool("Show", false);
            GetComponent<Animator>().SetBool("Tip", false);
            Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(v =>
            {
                Debug.Log("open eyes");
                OpenEyes();
                ShowTipCoroutine = null;
                CanBeTracked = true;
                Body.PoseStateChanged += PoseStateChanged;
            }).AddTo(this);
        }).AddTo(this);
    }

    private void ShowVideoTip(string videoUrl)
    {
        Body.PoseStateChanged -= PoseStateChanged;
        CanBeTracked = false;

        GetComponent<Animator>().SetBool("Tip", true);

        player.url = videoUrl;
        player.Prepare();
    }



    private IEnumerator ShowTip(string text)
    {
        Body.PoseStateChanged -= PoseStateChanged;

        CanBeTracked = false;

        Queue<string> words = new Queue<string>(text.Split(' '));

        GetComponent<Animator>().SetBool("Tip", true);

        yield return new WaitForSeconds(2f);

        while (words.Count>0)
        {
            string nw = words.Dequeue();

            
            nw = Distort(nw);
            GameObject newWord = Instantiate(WordPrefab);
            newWord.GetComponent<Animator>().speed = 1.5f*(Mathf.Clamp(symbolsPerSecond/ nw.Length,0,2));
            newWord.transform.SetParent(transform);
            newWord.transform.localPosition = Vector3.zero;
            newWord.transform.localScale = Vector3.one;
            newWord.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = nw;
            Destroy(newWord, nw.Length / symbolsPerSecond+1f);
            yield return new WaitForSeconds(nw.Length/symbolsPerSecond+0.3f);
        }


        GetComponent<Animator>().SetBool("Tip", false);

        yield return new WaitForSeconds(ExtraWaitTime);

        Distortion += extraDistortion;
        if (Distortion > 1f)
        {
            Distortion = 1f;
        }

        OpenEyes();

        ShowTipCoroutine = null;

        CanBeTracked = true;
        Body.PoseStateChanged += PoseStateChanged;
    }

    private string Distort(string nw)
    {
        Queue<bool> distoreLetters = new Queue<bool>();
        for (int i = 0; i < nw.Length * (1f-Distortion) + 1; i++)
        {
            distoreLetters.Enqueue(false);
        }

        for (int i = 0; i < nw.Length *  Distortion; i++)
        {
            distoreLetters.Enqueue(true);
        }

        distoreLetters = new Queue<bool>(distoreLetters.ToList().OrderBy(l=>Guid.NewGuid()));

        for (int i = nw.Length-1; i >= 0; i--)
        {
            if (ReplaceRules.ContainsKey(nw[i].ToString().ToUpper()) && distoreLetters.Dequeue())
            {
                nw=nw.Substring(0, i)+ReplaceRules[nw[i].ToString().ToUpper()]+nw.Substring(i+1, nw.Length-i-1);
            }
        }
        return nw;
    }

    public void OpenEyes()
    {
        foreach (EyesController eye in FindObjectsOfType<EyesController>())
        {
            eye.Open();
        }
        GetComponent<Animator>().SetBool("Tip", false);
    }
}
