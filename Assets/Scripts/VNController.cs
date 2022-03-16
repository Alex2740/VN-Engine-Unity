using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.UI;

public class VNController : MonoBehaviour
{
    public static VNController instance;
    [SerializeField] private List<GameObject> _canvases;
    [Header("Dialogues")]
    [SerializeField] private string _dialoguesPath;
    [SerializeField] private TextMeshProUGUI _dialogueBox;
    [SerializeField] private TextMeshProUGUI _speakerBox;
    [SerializeField] private float _speakerDefaultScale;
    [SerializeField] private float _speakerSpeakingScale;
    [Header("Images")]
    [SerializeField] private string _imgBackgroundsPath;
    [SerializeField] private string _imgCharactersPath;
    [SerializeField] private Image _background;
    [SerializeField] private GameObject _speakerContainer;
    [SerializeField] private GameObject _speakerPrefab;
    [Header("Musics")]
    [SerializeField] private string _musicsPath;
    [SerializeField] private AudioSource _musicSource;
    [Header("Sound")]
    [SerializeField] private string _soundPath;
    [SerializeField] private AudioSource _soundSource;
    [Header("Choices")]
    [SerializeField] private GameObject _choicesContainer;
    [SerializeField] private GameObject _choicePrefab;
    private StreamReader _dialogueStream;

    private bool _dialogueBlocked;

    private void Awake() {
        if (instance != null) {
            Destroy(this);
        }
        else {
            instance = this;
        }
    }

    private void ChangeCanvases(string name) {
        foreach (GameObject canvas in _canvases)
        {
            canvas.SetActive(canvas.name == name);
        }
    }

    public void OpenDialogue(int id) {
        if (_dialogueStream != null) {
            _dialogueStream.Close();
        }
        _dialogueStream = new StreamReader(Application.dataPath + "/" + _dialoguesPath + id + ".txt");
        ReadStream();
    }

    public void ReadStream() {
        if (_dialogueStream.EndOfStream) {
            _dialogueStream.Close();
            Debug.LogError("Error while reading instruction in dialogues : End Of Files");
            return;
        }
        string line = _dialogueStream.ReadLine();
        string[] data = line.Split(char.Parse("\t"));

        switch (data[0])
        {
            case "menu":
                _dialogueBlocked = true;
                ChangeCanvases("Menu");
                ReadMenu(data);
                break;
            case "show":
                UpdateSpeaker(data[1], data[2]);
                ReadStream();
                break;
            case "hide":
                HideSpeaker(data[1]);
                ReadStream();
                break;
            case "bg":
                ChangeBackground(data[1]);
                ReadStream();
                break;
            case "jump":
                OpenDialogue(int.Parse(data[1]));
                break;
            case "end":
                _dialogueStream.Close();
                Debug.Log("Fin du jeu");
                Application.Quit();
                break;
            case "music":
                SetMusic(data[1]);
                ReadStream();
                break;
            case "sound":
                PlaySound(data[1]);
                ReadStream();
                break;
            default:
                _dialogueBlocked = false;
                ChangeCanvases("Dialogue");
                ReadText(data);
                break;
        }
    }

    private void ReadText(string[] data) {
        _dialogueBox.text = data[1];
        _speakerBox.text = data[0];
        _speakerBox.gameObject.transform.parent.gameObject.SetActive(data[0] != "");

        for (int i = 0; i < _speakerContainer.transform.childCount; i++)
        {
            Transform speaker = _speakerContainer.transform.GetChild(i);
            speaker.GetChild(0).transform.localScale = new Vector3(_speakerDefaultScale, _speakerDefaultScale, 1);
            if ((speaker.name == data[0])) {
                speaker.GetChild(0).transform.localScale = new Vector3(_speakerSpeakingScale, _speakerSpeakingScale, 1);
            }
        }
    }

    private void ReadMenu(string[] data) {
        string[] jumps = _dialogueStream.ReadLine().Split(char.Parse("\t"));
        if (jumps[0] != "jump") {
            _dialogueStream.Close();
            Debug.LogError("Error while reading instruction in dialogues : No jump after a menu");
            return;
        }

        for (int id = 1; id < data.Length; id++)
        {
            GameObject choice = Instantiate(_choicePrefab, _choicesContainer.transform);
            choice.GetComponent<Choice>().UpdateData(data[id], int.Parse(jumps[id]));
        }
    }

    private void ChangeBackground(string name) {
        string path = _imgBackgroundsPath + name;
        _background.sprite = Resources.Load<Sprite>(path);
    }

    private void UpdateSpeaker(string name, string sprite) {
        bool exist = false;
        for (int i = 0; i < _speakerContainer.transform.childCount; i++)
        {
            Transform speaker = _speakerContainer.transform.GetChild(i);
            if ((speaker.name == name) & !exist) {
                exist = true;
                Destroy(speaker.gameObject);
                GameObject newSpeaker = Instantiate(_speakerPrefab, _speakerContainer.transform);
                newSpeaker.name = name;
                string path = _imgCharactersPath + name + "-" + sprite;
                newSpeaker.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>(path);
            }
        }

        if (!exist) {
            GameObject newSpeaker = Instantiate(_speakerPrefab, _speakerContainer.transform);
            newSpeaker.name = name;
            string path = _imgCharactersPath + name + "-" + sprite;
            newSpeaker.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>(path);
        }
    }

    private void HideSpeaker(string name) {
        for (int i = 0; i < _speakerContainer.transform.childCount; i++)
        {
            Transform speaker = _speakerContainer.transform.GetChild(i);
            if (speaker.name == name) {
                Destroy(speaker.gameObject);
            }
        }
    }

    private void SetMusic(string name) {
        string path = _musicsPath + name;
        _musicSource.clip = Resources.Load<AudioClip>(path);
        _musicSource.Play();
    }

    private void PlaySound(string name) {
        string path = _soundPath + name;
        _soundSource.clip = Resources.Load<AudioClip>(path);
        _soundSource.Play();
    }

    private void Update() {
        if(Input.GetMouseButtonDown(0) & !_dialogueBlocked){
            ReadStream();
        }
    }
}
