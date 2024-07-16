using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public class ControlLobby : MonoBehaviourPunCallbacks
{
    #region CORE
    private void Awake()
    {
        Awake_PanelInicio();
    }

    void Start()
    {
        Start_PanelInicio();
    }
    #endregion

    #region PANEL INICIO
    [Header("PANEL INICIO")]
    [SerializeField] GameObject panelInicio;
    [SerializeField] TMP_InputField inputNombre;
    [SerializeField] Button botonIniciar;
    [SerializeField] TMP_Text notificacion;

    [Header("Transición")]
    [SerializeField] private Image imagenFondo;
    [SerializeField] private float duracionTransicion = 2f; // Aumentamos la duración para incluir aparición y desaparición
    private float tiempoTransicion = 0f;
    private bool enTransicion = false;
    private Material materialShader;


    private void Awake_PanelInicio()
    {
        panelInicio.SetActive(true);
        panelSeleccion.SetActive(false);
        panelChat.SetActive(false);
        botonIniciar.interactable = false;
    }

    private void Start_PanelInicio()
    {
        notificacion.text = "conectando a Photon . . .";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        notificacion.text = "entrando al Lobby . . .";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Invoke("ActivarBotonIniciar", 1f);
    }

    public void ActivarBotonIniciar()
    {
        notificacion.text = string.Empty;
        botonIniciar.onClick.AddListener(Iniciar);
        botonIniciar.interactable = true;
    }

    private void Iniciar()
    {
        string nombre = inputNombre.text;

        if (string.IsNullOrEmpty(nombre))
        {
            notificacion.text = "por favor, ingrese un nombre";
            return;
        }

        if (nombre.Length > 10)
        {
            notificacion.text = "el nombre no debe tener más de 10 caracteres";
            return;
        }

        PhotonNetwork.NickName = nombre;

        int salas = PhotonNetwork.CountOfRooms;

        if (salas == 0)
        {
            notificacion.text = "creando sala . . .";
            RoomOptions config = new RoomOptions() { MaxPlayers = 7 };

            bool seCreoSala = PhotonNetwork.CreateRoom("XP", config);

            if (!seCreoSala)
            {
                notificacion.text = "Error al crear sala";
            }
        }
        else
        {
            notificacion.text = "uniendo a sala . . .";
            bool seUnioSala = PhotonNetwork.JoinRoom("XP");
            
            if (!seUnioSala)
            {
                notificacion.text = "error al unirse a sala";
            }
        }
    }
    #endregion

    #region PANEL SELECCION
    [Header("PANEL SELECCION")]
    [SerializeField] private GameObject panelSeleccion;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            EnviarMensaje();
        }
    }

    public override void OnCreatedRoom()
    {
        InicializarChat();
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        // Iniciar la transición
        enTransicion = true;
        tiempoTransicion = 0f;
        materialShader = imagenFondo.material;

        InicializarSlots();
        ActualizarChat();

        StartCoroutine(CrControlSpam());
        StartCoroutine(ActualizarTransicion());
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        CrearSlot(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        EliminarSlot(otherPlayer);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        ActualizarChat();
    }

    #region PANEL SELECCION - Slots
    [Header("Panel Seleccion - Slots")]
    [SerializeField] private Transform panelJugadores;
    [SerializeField] private SlotJugador pfSlotJugador;
    private Dictionary<Player, SlotJugador> dicSlotJugadores;

    public void InicializarSlots()
    {
        dicSlotJugadores = new Dictionary<Player, SlotJugador>();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            CrearSlot(player);
        }
    }

    private void CrearSlot(Player player)
    {
        SlotJugador slot = Instantiate(pfSlotJugador, panelJugadores);
        slot.Player = player;
        dicSlotJugadores.Add(player, slot);
    }

    private void EliminarSlot(Player player)
    {
        Destroy(dicSlotJugadores[player].gameObject);
        dicSlotJugadores.Remove(player);
    }
    #endregion

    #region PANEL SELECCION - Chat
    [Header("Panel Seleccion - Chat")]
    [SerializeField] private GameObject panelChat;
    [SerializeField] private RectTransform scrollView;
    [SerializeField] private RectTransform content;
    [SerializeField] private TMP_Text chat;
    [SerializeField] private TMP_InputField inputMensaje;
    [SerializeField] private Button botonEnviar;

    private int mensajesEnviados = 0;

    private void InicializarChat()
    {
        Hashtable propiedades = PhotonNetwork.CurrentRoom.CustomProperties;
        propiedades["Chat"] = "inicio de chat";
        PhotonNetwork.CurrentRoom.SetCustomProperties(propiedades);
    }

    public void EnviarMensaje()
    {
        if (mensajesEnviados >= 5)
            return;

        string mensaje = inputMensaje.text;

        if (string.IsNullOrEmpty(mensaje) || mensaje.Length > 40)
            return;

        var propiedades = PhotonNetwork.CurrentRoom.CustomProperties;
        string chatActual = propiedades["Chat"].ToString();
        chatActual += "\n" + PhotonNetwork.NickName + ": " + mensaje;
        propiedades["Chat"] = chatActual;
        PhotonNetwork.CurrentRoom.SetCustomProperties(propiedades);

        inputMensaje.text = string.Empty;
        inputMensaje.ActivateInputField();
        mensajesEnviados++;
    }

    private void ActualizarChat()
    {
        var propiedades = PhotonNetwork.CurrentRoom.CustomProperties;
        if (!propiedades.ContainsKey("Chat"))
            return;

        string chatString = propiedades["Chat"].ToString();
        chat.text = chatString;

        int offsetSuperior = 10;
        int alturaLinea = 29;
        int espacios = chat.text.Count(c => c == '\n');
        float altura = offsetSuperior + alturaLinea * espacios;
        content.sizeDelta = new Vector2(content.sizeDelta.x, altura);

        if (content.sizeDelta.y > scrollView.sizeDelta.y)
        {
            Vector3 posicionContent = content.localPosition;
            posicionContent.y = altura - scrollView.sizeDelta.y;
            content.localPosition = posicionContent;
        }
    }

    public IEnumerator CrControlSpam()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (mensajesEnviados > 0)
                mensajesEnviados--;
        }
    }

    private IEnumerator ActualizarTransicion()
    {
        while (enTransicion)
        {
            tiempoTransicion += Time.deltaTime;
            float progreso = Mathf.Clamp01(tiempoTransicion / duracionTransicion);

            materialShader.SetFloat("_TransitionProgress", progreso);

            if (progreso >= 0.5f && !panelSeleccion.activeSelf)
            {
                // Cambiar paneles a la mitad de la transición
                panelInicio.SetActive(false);
                panelSeleccion.SetActive(true);
                panelChat.SetActive(true);
            }

            if (progreso >= 1f)
            {
                enTransicion = false;
            }

            yield return null;
        }
    }
    #endregion
    #endregion
}