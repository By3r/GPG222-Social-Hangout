using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class ChatUIManager : MonoBehaviour
{
    #region Variables
    [SerializeField] private int port;
    [SerializeField] private GameObject chatPopup;
    [SerializeField] private TMP_Text chatMessageDisplayer;
    [SerializeField] private TMP_InputField messageInputField;
    [SerializeField] private string iPAddress;
    private Socket _server;
    private Socket _client;
    #endregion

    private void Start()
    {
        // initialise the socket.
        // link the socket to an ip endpoint.
        // create a queue
        // sett false to socket blocking, lol.

    }

    // buffer time to encode unicode for emojis lmao
    // get the input field's content and encode it into a buffer.
    // send the buffer to the client socket.
    // display recieved buffer in another script. note: i'll be changing the script's name to be more specifuc than a general manager.
    // ++ Note: have a server mono, 

}
