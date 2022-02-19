using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

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

    // Start is called before the first frame update
    void Start()
    {
        Body.PoseStateChanged += PoseStateChanged;
        Body.TrackingStateChanged += TrackingStateChanged;
        StartCoroutine(BlinkRoutuine());

        volume = Camera.main.GetComponent<PostProcessVolume>();

        volume.profile.TryGetSettings(out abberation);

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
        if (ShowTipCoroutine!=null)
        {
            StopCoroutine(ShowTipCoroutine);
        }
        else
        {
            string tip = Tips.Tips.OrderBy(t => Guid.NewGuid()).FirstOrDefault();

            ShowTipCoroutine = StartCoroutine(ShowTip(tip));
        }

        
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
        Debug.Log(Distortion);


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
