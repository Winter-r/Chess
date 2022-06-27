using System.Collections.Generic;
using System.Threading.Tasks;
using StreamChat.Core;
using StreamChat.Core.Auth;
using StreamChat.Core.Events;
using StreamChat.Core.Models;
using StreamChat.Core.Requests;
using StreamChat.Libs.Utils;
using UnityEngine;

public class StreamChatBehaviour : MonoBehaviour
{
    public static StreamChatBehaviour instance;

    public IStreamChatClient client;
    ChannelState channelState;

    public Message messagePrefab;
    Transform content;

    Dictionary<string, Message> idsAndMessages = new Dictionary<string, Message>();

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }

    public void SetContentObject()
    {
        content = GameObject.FindGameObjectWithTag("content").transform;
    }

    public void GetOrCreateClient(string userName)
    {
        string userId = StreamChatClient.SanitizeUserId(userName);
        AuthCredentials credentials = new AuthCredentials("xg4h594jgy57", userId, StreamChatClient.CreateDeveloperAuthToken(userId));
        client = StreamChatClient.CreateDefaultClient(credentials);
        client.Connect();
        client.Connected += OnClientConnected;
        client.MessageReceived += OnMessageReceived;

        client.MessageDeleted += OnMessageDeleted;
        client.MessageUpdated += OnMessageUpdated;
        client.ReactionReceived += OnReactionReceived;
        client.ReactionDeleted += OnReactionDeleted;
    }

    void OnClientConnected()
    {
        Debug.Log("Stream client connected!");
    }

    private void Update()
    {
        if (client != null)
        {
            client.Update(Time.deltaTime);
        }
    }

    public void GetOrCreateChannel(string roomName)
    {
        GetOrCreateChannelAsync(roomName).LogIfFailed();
    }

    async Task GetOrCreateChannelAsync(string roomName)
    {
        ChannelGetOrCreateRequest request = new ChannelGetOrCreateRequest
        {
            State = true,
            Watch = true
        };

        channelState = await client.ChannelApi.GetOrCreateChannelAsync("livestream", roomName, request);
    }

    public void SendChat(string message)
    {
        SendChatAsync(message, channelState.Channel.Type, channelState.Channel.Id).LogIfFailed();
    }

    async Task SendChatAsync(string message, string channelType, string channelId)
    {
        SendMessageRequest request = new SendMessageRequest
        {
            Message = new MessageRequest
            {
                Text = message
            }
        };

        await client.MessageApi.SendNewMessageAsync(channelType, channelId, request);
    }

    void OnMessageReceived(EventMessageNew newMessage)
    {
        Message newMessageSpawned = Instantiate(messagePrefab, content);
        newMessageSpawned.messageText.text = newMessage.Message.Text;
        newMessageSpawned.usernameText.text = newMessage.User.Id;
        newMessageSpawned.messageId = newMessage.Message.Id;
        newMessageSpawned.userId = newMessage.User.Id;

        idsAndMessages.Add(newMessage.Message.Id, newMessageSpawned);
    }

    void OnMessageDeleted(EventMessageDeleted deletedMessage)
    {
        Message m = idsAndMessages[deletedMessage.Message.Id];
        deletedMessage.Message.Text = "Deleted Message";
        m.messageText.text = deletedMessage.Message.Text;
        m.likeButton.SetActive(false);
    }

    void OnMessageUpdated(EventMessageUpdated updatedMessage)
    {
        Message m = idsAndMessages[updatedMessage.Message.Id];
        m.messageText.text = updatedMessage.Message.Text + " (edited)";
    }

    void OnReactionReceived(EventReactionNew newReaction)
    {
        Message m = idsAndMessages[newReaction.Message.Id];
        m.reactionObject.SetActive(true);
        m.numberOfLikes.text = newReaction.Message.ReactionScores["like"].ToString();
    }

    void OnReactionDeleted(EventReactionDeleted deletedReaction)
    {
        Message m = idsAndMessages[deletedReaction.Message.Id];

        if (deletedReaction.Message.ReactionScores.ContainsKey("like") == false)
        {
            m.reactionObject.SetActive(false);
        }
        else
        {
            m.numberOfLikes.text = deletedReaction.Message.ReactionScores["like"].ToString();
        }

    }

    public void DeleteMessage(string messageId)
    {
        client.MessageApi.DeleteMessageAsync(messageId, false).LogIfFailed();
    }

    public void UpdateMessage(string messageId, string updatedMessage)
    {
        client.MessageApi.UpdateMessageAsync(new UpdateMessageRequest
        {
            Message = new MessageRequest
            {
                Id = messageId,
                Text = updatedMessage
            }
        }).LogIfFailed();
    }

    public void LikeMessage(string messageId)
    {
        client.MessageApi.SendReactionAsync(messageId,
        new SendReactionRequest
        {
            Reaction = new ReactionRequest
            {
                Type = "like"
            }
        }).LogIfFailed();
    }

    public void UnlikeMessage(string messageId)
    {
        client.MessageApi.DeleteReactionAsync(messageId, "like").LogIfFailed();
    }

    public void Disconnect()
    {
        client.MessageReceived -= OnMessageReceived;
        client.Dispose();
    }
}
