using Photon.Chat;
using ExitGames.Client.Photon;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class PhotonChatManager : MonoBehaviour, IChatClientListener
{
    public static PhotonChatManager Instance;

    private ChatClient chatClient;
    private string currentChannel = "Global";

    [Header("UI")]
    public GameObject fullPanel;
    public TMP_InputField inputField;
    //public TMP_Text chatDisplay;
    public ScrollRect scrollRect;
    public GameObject chatLinePrefab;
    public Transform chatContent;

    [Header("Toggle Settings")]
    public RectTransform viewPortTransform;
    public Image viewPortImage;
    public float expandedHeight = 300f;
    public float collapsedHeight = 150f;

    private bool isInputState = false;

    void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        string userId = Photon.Pun.PhotonNetwork.NickName;
        chatClient = new ChatClient(this);
        chatClient.Connect(
            Photon.Pun.PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat,
            "1.0", new AuthenticationValues(userId));

        ToggleInputState(false);
    }

    void Update()
    {
        chatClient?.Service();

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log($"⏎ ENTER PRESSED | Focus: {inputField.isFocused} | Text: '{inputField.text}'");

            if (isInputState && !string.IsNullOrWhiteSpace(inputField.text))
            {
                SendMessage();
            }

            ToggleInputState(!isInputState); // switch either way
        }
    }

    public void ToggleInputState(bool toInput)
    {
        isInputState = toInput;

        inputField.gameObject.SetActive(toInput);
        fullPanel.SetActive(true);
        inputField.text = "";

        if (viewPortTransform != null)
        {
            viewPortTransform.sizeDelta = new Vector2(
                viewPortTransform.sizeDelta.x,
                isInputState ? expandedHeight : collapsedHeight
            );
        }

        if (viewPortImage != null)
        {
            viewPortImage.enabled = isInputState;
        }

        if (isInputState)
        {
            inputField.ActivateInputField();
        }
        else
        {
            inputField.DeactivateInputField();
        }
    }

    public void OnConnected()
    {
        JoinChannelForCurrentScene();
    }

    public void JoinChannelForCurrentScene()
    {
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (scene == "Nexus") currentChannel = "Global";
        else if (Photon.Pun.PhotonNetwork.InRoom)
            currentChannel = "Dungeon_" + Photon.Pun.PhotonNetwork.CurrentRoom.Name;

        chatClient.Subscribe(new string[] { currentChannel });
    }

    public void SendMessage()
    {
        Debug.Log($"📨 Sending: {inputField.text} to channel {currentChannel}");

        if (!string.IsNullOrEmpty(inputField.text))
        {
            chatClient.PublishMessage(currentChannel, inputField.text);
        }
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
        {
            GameObject line = Instantiate(chatLinePrefab, chatContent);
            TMP_Text text = line.GetComponent<TMP_Text>();
            text.text = $"<color=#ffcc00>[{senders[i]}]</color>: {messages[i]}";
        }

        if (scrollRect != null)
        {
            StartCoroutine(ScrollToBottomNextFrame());
        }
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null; // wait one frame for layout
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    // Photon Chat interface methods
    public void OnDisconnected() { }
    public void OnChatStateChange(ChatState state) { }
    public void OnSubscribed(string[] channels, bool[] results)
    {
        for (int i = 0; i < channels.Length; i++)
            Debug.Log($"✅ Subscribed to: {channels[i]} success: {results[i]}");
    }

    public void OnUnsubscribed(string[] channels) { }
    public void OnPrivateMessage(string sender, object message, string channelName) { }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }
    public void OnUserSubscribed(string channel, string user) { }
    public void OnUserUnsubscribed(string channel, string user) { }
    public void DebugReturn(DebugLevel level, string message) => Debug.Log("[Chat] " + message);
}
