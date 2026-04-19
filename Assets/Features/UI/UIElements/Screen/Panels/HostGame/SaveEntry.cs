using FishFlingers.Networking;
using FishFlingers.Pools;
using FishFlingers.Saving;
using FishFlingers.UI;
using ShinyOwl.Common;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveEntry : MonoBehaviour, IPoolable
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _thumbnailImage;
    [SerializeField] private AspectRatioFitter _thumbnailAspectRatioFitter;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Image _newWorldImage;

    private SaveManager _saveManager;
    private LobbyManager _lobbyManager;
    private UIManager _uiManager;

    private SaveFile _saveFile;

    private PanelInstance<WorldPanel> _worldPanelInstance;

    private void Awake()
    {
        _saveManager = GameManager.Instance.Get<SaveManager>();
        _lobbyManager = GameManager.Instance.Get<LobbyManager>();
        _uiManager = GameManager.Instance.Get<UIManager>();
    }

    private void Start()
    {
        _worldPanelInstance = new PanelInstance<WorldPanel>(_uiManager.Config.WorldPanel);

        _button.onClick.AddListener(Pressed);
    }

    private void Pressed()
    {
        _worldPanelInstance.Toggle(null);

        return;

        SaveFile file = _saveFile;

        if (file == null)
        {
            file = new SaveFile(null);
            _saveManager.AddSaveFile(file);
        }

        _saveManager.SelectSaveFile(file);

        _ = _lobbyManager.CreateLobbyAsync();
    }

    public void Setup(SaveFile file)
    {
        _saveFile = file;

        _thumbnailImage.gameObject.SetActive(file != null);
        _nameText.gameObject.SetActive(file != null);
        _newWorldImage.gameObject.SetActive(file == null);

        if (file != null)
        {
            _nameText.text = Path.GetFileName(file.FolderPath);

            byte[] bytes = File.ReadAllBytes(file.ThumbnailPath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            _thumbnailImage.sprite = sprite;
            _thumbnailAspectRatioFitter.aspectRatio = (float)texture.width / texture.height;
        }
    }

    public void OnReturnedToPool()
    {
        _saveFile = null;
    }

    public void OnTakenFromPool()
    { }
}
