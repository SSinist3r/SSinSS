using System;
using System.Collections.Generic;
using System.Linq;

namespace PROProtocol
{
    public class GameClient
    {
        public InventoryItem GroundMount;
        public InventoryItem WaterMount;

        public Random Rand { get; private set; } = new Random();
        public Language I18n { get; private set; } = new Language();

        public bool IsConnected { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public string PlayerName { get; private set; }
        public int GuildId { get; private set; } = -1;

        public int PlayerX { get; private set; }
        public int PlayerY { get; private set; }
        public string MapName { get; private set; }
        public Map Map { get; private set; }

        public int PokedexOwned { get; private set; }
        public int PokedexSeen { get; private set; }
        public int PokedexEvolved { get; private set; }
        public static int MoneyToTrade { get; set; }
        public static bool TradeGiver { get; set; } = false;
        public static bool TradeAccepter { get; set; } = false;

        //-------------------------------------------------------------------------------------------------

        public int KantoSeen { get; private set; }
        public int KantoOwned { get; private set; }
        public int KantoEvolved { get; private set; }
        public int KantoAllPoke { get; private set; }

        public int JohtoSeen { get; private set; }
        public int JohtoOwned { get; private set; }
        public int JohtoEvolved { get; private set; }
        public int JohtoAllPoke { get; private set; }

        public int HoennSeen { get; private set; }
        public int HoennOwned { get; private set; }
        public int HoennEvolved { get; private set; }
        public int HoennAllPoke { get; private set; }

        public int SinnohSeen { get; private set; }
        public int SinnohOwned { get; private set; }
        public int SinnohEvolved { get; private set; }
        public int SinnohAllPoke { get; private set; }

        public int OtherSeen { get; private set; }
        public int OtherOwned { get; private set; }
        public int OtherEvolved { get; private set; }
        public int OtherAllPoke { get; private set; }
        public static string Evolution_Left { get; set; } = "--------------Evolutions Left--------------" + Environment.NewLine;
        public static int Evolution_Counter { get; set; }

        //------------------------------------------------------------------------------------------------------

        public bool IsInBattle { get; private set; }
        public bool IsSurfing { get; private set; }
        public bool IsBiking { get; private set; }
        public bool IsOnGround { get; private set; }
        public bool IsPCOpen { get; private set; }
        public bool CanUseCut { get; private set; }
        public bool CanUseSmashRock { get; private set; }
        public bool IsPrivateMessageOn { get; private set; } = true;

        public bool IsPartyInspectionOn { get; private set; } = true;

        public bool IsNpcInteractionsOn { get; private set; } = true;

        public int Money { get; private set; }
        public int Coins { get; private set; }
        public bool IsMember { get; private set; }
        public List<Pokemon> Team { get; private set; } = new List<Pokemon>();
        public List<Pokemon> CurrentPCBox { get; private set; } = new List<Pokemon>();
        public List<InventoryItem> Items { get; private set; } = new List<InventoryItem>();
        public string PokemonTime { get; private set; }
        public string Weather { get; private set; }
        public int PCGreatestUid { get; private set; } = -1;

        public bool IsScriptActive { get; private set; }
        public string ScriptId { get; private set; }
        public int ScriptStatus { get; private set; }
        public string[] DialogContent { get; private set; }

        public Battle ActiveBattle { get; private set; }
        public Shop OpenedShop { get; private set; }
        public MoveRelearner MoveRelearner { get; private set; }

        public List<ChatChannel> Channels { get; } = new List<ChatChannel>();
        public List<string> Conversations { get; } = new List<string>();
        public Dictionary<string, PlayerInfos> Players { get; } = new Dictionary<string, PlayerInfos>();

        private DateTime _updatePlayers;
        private DateTime _refreshBoxTimeout;

        public bool IsPCBoxRefreshing { get; private set; }
        public int CurrentPCBoxId { get; private set; }
        public bool IsCreatingNewCharacter { get; private set; }

        public event Action ConnectionOpened;
        public event Action<Exception> ConnectionFailed;
        public event Action<Exception> ConnectionClosed;
        public event Action LoggedIn;
        public event Action<AuthenticationResult> AuthenticationFailed;
        public event Action<int> QueueUpdated;

        public event Action<string, int, int> PositionUpdated;
        public event Action<string, int, int> TeleportationOccuring;
        public event Action<string> MapLoaded;
        public event Action<List<Npc>> NpcReceived;
        public event Action PokemonsUpdated;
        public event Action InventoryUpdated;
        public event Action BattleStarted;
        public event Action<string> BattleMessage;
        public event Action BattleEnded;
        public event Action BattleUpdated;
        public event Action<string, string[]> DialogOpened;
        public event Action<string, string, int> EmoteMessage;
        public event Action<string, string, string> ChatMessage;
        public event Action RefreshChannelList;
        public event Action<string, string, string, string> ChannelMessage;
        public event Action<string, string> ChannelSystemMessage;
        public event Action<string, string, string, string> ChannelPrivateMessage;
        public event Action<string> SystemMessage;
        public event Action<string, string, string, string> PrivateMessage;
        public event Action<string, string, string> LeavePrivateMessage;
        public event Action<PlayerInfos> PlayerUpdated;
        public event Action<PlayerInfos> PlayerAdded;
        public event Action<PlayerInfos> PlayerRemoved;
        public event Action<string, string> InvalidPacket;
        public event Action<int, string, int> LearningMove;
        public event Action<int, int> Evolving;
        public event Action<string, string> PokeTimeUpdated;
        public event Action<Shop> ShopOpened;
        public event Action<MoveRelearner> MoveRelearnerOpened;
        public event Action<List<Pokemon>> PCBoxUpdated;
        public event Action<string> LogMessage;
        public event Action ActivePokemonChanged;
        public event Action OpponentChanged;
        
        private const string Version = "Halloween20_v2";

        private GameConnection _connection;
        private DateTime _lastMovement;
        private List<Direction> _movements = new List<Direction>();
        private Direction? _slidingDirection;
        private bool _surfAfterMovement;
        private Queue<object> _dialogResponses = new Queue<object>();

        private Timeout _movementTimeout = new Timeout();
        private Timeout _battleTimeout = new Timeout();
        private Timeout _loadingTimeout = new Timeout();
        private Timeout _mountingTimeout = new Timeout();
        private Timeout _teleportationTimeout = new Timeout();
        private Timeout _dialogTimeout = new Timeout();
        private Timeout _swapTimeout = new Timeout();
        private Timeout _itemUseTimeout = new Timeout();
        private Timeout _fishingTimeout = new Timeout();
        private Timeout _refreshingPCBox = new Timeout();
        private Timeout _moveRelearnerTimeout = new Timeout();

        private Timeout _npcBattleTimeout = new Timeout();
        private Npc _npcBattler;

        private MapClient _mapClient;
        private List<int> _requestedGuildData = new List<int>();

        public void ClearPath()
        {
            _movements.Clear();
        }

        public bool IsInactive =>
            _movements.Count == 0
            && !_movementTimeout.IsActive
            && !_battleTimeout.IsActive
            && !_loadingTimeout.IsActive
            && !_mountingTimeout.IsActive
            && !_teleportationTimeout.IsActive
            && !_dialogTimeout.IsActive
            && !_swapTimeout.IsActive
            && !_itemUseTimeout.IsActive
            && !_fishingTimeout.IsActive
            && !_refreshingPCBox.IsActive
            && !_npcBattleTimeout.IsActive
            && !_moveRelearnerTimeout.IsActive
            && !IsCreatingNewCharacter;

        public bool IsTeleporting => _teleportationTimeout.IsActive;

        public GameServer Server => _connection.Server;

        public bool IsMapLoaded => Map != null;
        public bool AreNpcReceived { get; private set; }

        public GameClient(GameConnection connection, MapConnection mapConnection)
        {
            _mapClient = new MapClient(mapConnection);
            _mapClient.ConnectionOpened += MapClient_ConnectionOpened;
            _mapClient.ConnectionClosed += MapClient_ConnectionClosed;
            _mapClient.ConnectionFailed += MapClient_ConnectionFailed;
            _mapClient.MapLoaded += MapClient_MapLoaded;

            _connection = connection;
            _connection.PacketReceived += OnPacketReceived;
            _connection.Connected += OnConnectionOpened;
            _connection.Disconnected += OnConnectionClosed;
        }

        public void Open()
        {
            _connection.Connect();
        }

        public void Close(Exception error = null)
        {
            _connection.Close(error);
        }

        public void Update()
        {
            _mapClient.Update();
            _connection.Update();
            if (!IsAuthenticated)
                return;

            _movementTimeout.Update();
            _loadingTimeout.Update();
            _mountingTimeout.Update();
            _teleportationTimeout.Update();
            _dialogTimeout.Update();
            _swapTimeout.Update();
            _itemUseTimeout.Update();
            _fishingTimeout.Update();
            _refreshingPCBox.Update();
            _moveRelearnerTimeout.Update();

            if (!_battleTimeout.Update() && ActiveBattle != null && ActiveBattle.IsFinished)
            {
                ActiveBattle = null;
                SendPacket("_");
            }

            SendRegularPing();
            UpdateMovement();
            UpdateScript();
            UpdatePlayers();
            UpdatePCBox();
            UpdateNpcBattle();
        }

        public void CloseChannel(string channelName)
        {
            if (Channels.Any(e => e.Name == channelName))
            {
                SendMessage("/cgleave " + channelName);
            }
        }

        public void CloseConversation(string pmName)
        {
            if (Conversations.Contains(pmName))
            {
                SendMessage("/pm rem " + pmName + "-=-" + PlayerName + '|' + PlayerName);
                Conversations.Remove(pmName);
            }
        }

        public void SendPokedexRequest()
        {
            try
            {
                SendPacket("p|.|1|0");
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke("Error: " + ex.ToString());
            }
        }

        private int _pingCurrentStep = 1;
        private bool _isPingSwapped = false;

        private void SendRegularPing()
        {
            if ((DateTime.UtcNow - _lastMovement).TotalSeconds >= 6)
            {
                _lastMovement = DateTime.UtcNow;
                // DSSock.Update
                int packetType;
                if (_pingCurrentStep == 5)
                {
                    packetType = _isPingSwapped ? 2 : 1;
                    _pingCurrentStep = 0;
                }
                else
                {
                    packetType = Rand.Next(2) + 1;
                }

                if (packetType == 1)
                {
                    _isPingSwapped = !_isPingSwapped;
                }
                
                _pingCurrentStep++;
                SendPacket(packetType.ToString());
            }
        }

        private void UpdateMovement()
        {
            if (!IsMapLoaded) return;

            if (!_movementTimeout.IsActive && _movements.Count > 0)
            {
                if (GroundMount != null && !_itemUseTimeout.IsActive && !IsBiking && !IsSurfing && Map.IsOutside)
                {
                    LogMessage?.Invoke($"Mounting [{GroundMount.Name}]");
                    UseItem(GroundMount.Id);
                    return;
                }
                
                Direction direction = _movements[0];
                _movements.RemoveAt(0);

                if (ApplyMovement(direction))
                {
                    SendMovement(direction.AsChar());
                    _movementTimeout.Set(IsBiking ? 125 : 250);
                    if (Map.HasLink(PlayerX, PlayerY))
                    {
                        _teleportationTimeout.Set();
                    }
                    else
                    {
                        Npc battler = Map.Npcs.FirstOrDefault(npc => npc.CanBattle && npc.IsInLineOfSight(PlayerX, PlayerY));
                        if (battler != null && IsNpcInteractionsOn == true)
                        {
                            battler.CanBattle = false;
                            LogMessage?.Invoke("The NPC " + (battler.Name ?? battler.Id.ToString()) + " saw us, interacting...");
                            _npcBattler = battler;
                            int distanceFromBattler = DistanceBetween(PlayerX, PlayerY, battler.PositionX, battler.PositionY);
                            _npcBattleTimeout.Set(Rand.Next(1000, 2000) + distanceFromBattler * 250);
                            ClearPath();
                        }
                    }
                }

                if (_movements.Count == 0 && _surfAfterMovement)
                {
                    _movementTimeout.Set(Rand.Next(750, 2000));
                }
            }
            if (!_movementTimeout.IsActive && _movements.Count == 0 && _surfAfterMovement)
            {
                _surfAfterMovement = false;
                UseSurf();
            }
        }

        private void UpdatePlayers()
        {
            if (_updatePlayers < DateTime.UtcNow)
            {
                foreach (string playerName in Players.Keys.ToArray())
                {
                    if (Players[playerName].IsExpired())
                    {
                        PlayerRemoved?.Invoke(Players[playerName]);
                        Players.Remove(playerName);
                    }
                }
                _updatePlayers = DateTime.UtcNow.AddSeconds(5);
            }
        }

        private void UpdatePCBox()
        {
            // if we did not receive an answer, then the box is empty
            if (IsPCBoxRefreshing && _refreshBoxTimeout > DateTime.UtcNow)
            {
                IsPCBoxRefreshing = false;
                if (Map.IsPC(PlayerX, PlayerY - 1))
                {
                    IsPCOpen = true;
                }
                CurrentPCBox = new List<Pokemon>();
                PCBoxUpdated?.Invoke(CurrentPCBox);
            }
        }

        private void UpdateNpcBattle()
        {
            if (_npcBattler == null) return;

            _npcBattleTimeout.Update();
            if (_npcBattleTimeout.IsActive) return;

            TalkToNpc(_npcBattler);
            _npcBattler = null;
        }

        private bool ApplyMovement(Direction direction)
        {
            int destinationX = PlayerX;
            int destinationY = PlayerY;
            bool isOnGround = IsOnGround;
            bool isSurfing = IsSurfing;

            direction.ApplyToCoordinates(ref destinationX, ref destinationY);

            Map.MoveResult result = Map.CanMove(direction, destinationX, destinationY, isOnGround, isSurfing, CanUseCut, CanUseSmashRock);
            if (Map.ApplyMovement(direction, result, ref destinationX, ref destinationY, ref isOnGround, ref isSurfing))
            {
                PlayerX = destinationX;
                PlayerY = destinationY;
                IsOnGround = isOnGround;
                IsSurfing = isSurfing;
                PositionUpdated?.Invoke(MapName, PlayerX, PlayerY);

                if (result == Map.MoveResult.Icing)
                {
                    _movements.Insert(0, direction);
                }

                if (result == Map.MoveResult.Sliding)
                {
                    int slider = Map.GetSlider(destinationX, destinationY);
                    if (slider != -1)
                    {
                        _slidingDirection = Map.SliderToDirection(slider);
                    }
                }

                if (_slidingDirection != null)
                {
                    _movements.Insert(0, _slidingDirection.Value);
                }

                return true;
            }

            _slidingDirection = null;
            return false;
        }

        private void UpdateScript()
        {
            if (IsScriptActive && !_dialogTimeout.IsActive)
            {
                if (ScriptStatus == 0)
                {
                    _dialogResponses.Clear();
                    IsScriptActive = false;
                }
                else if (ScriptStatus == 1)
                {
                    SendDialogResponse(0);
                    _dialogTimeout.Set();
                }
                else if (ScriptStatus == 3 || ScriptStatus == 4)
                {
                    SendDialogResponse(GetNextDialogResponse());
                    _dialogTimeout.Set();
                }
            }
        }

        private int GetNextDialogResponse()
        {
            if (_dialogResponses.Count > 0)
            {
                object response = _dialogResponses.Dequeue();
                if (response is int)
                {
                    return (int)response;
                }
                else if (response is string)
                {
                    string text = ((string)response).ToUpperInvariant();
                    for (int i = 1; i < DialogContent.Length; ++i)
                    {
                        if (DialogContent[i].ToUpperInvariant().Equals(text))
                        {
                            return i;
                        }
                    }
                }
            }
            return 1;
        }

        public int DistanceTo(int cellX, int cellY)
        {
            return Math.Abs(PlayerX - cellX) + Math.Abs(PlayerY - cellY);
        }

        public static int DistanceBetween(int fromX, int fromY, int toX, int toY)
        {
            return Math.Abs(fromX - toX) + Math.Abs(fromY - toY);
        }

        public void SendPacket(string packet)
        {
#if DEBUG
            Console.WriteLine("[>] " + packet);
#endif
            _connection.Send(packet);
        }

        public void SendMessage(string text)
        {
            // DSSock.sendMSG
            SendPacket("{|.|" + text);
        }

        public void SendPrivateMessage(string nickname, string text)
        {
            // DSSock.sendMSG
            string pmHeader = "/pm " + PlayerName + "-=-" + nickname;
            SendPacket("{|.|" + pmHeader + '|' + text);
        }

        public void SendStartPrivateMessage(string nickname)
        {
            SendMessage("/pm " + PlayerName + "-=-" + nickname);
        }

        public void SendFriendToggle(string nickname)
        {
            SendMessage("/friend " + nickname);
        }

        public void SendIgnoreToggle(string nickname)
        {
            SendMessage("/ignore " + nickname);
        }

        public void CreateCharacter(int hair, int colour, int tone, int clothe, int eyes)
        {
            if (!IsCreatingNewCharacter) return;
            IsCreatingNewCharacter = false;
            SendCreateCharacter(hair, colour, tone, clothe, eyes);
            _dialogTimeout.Set();
        }

        private void SendCreateCharacter(int hair, int colour, int tone, int clothe, int eyes)
        {
            SendMessage("/setchar " + hair + "," + colour + "," + tone + "," + clothe + "," + eyes);
        }

        public void SendAuthentication(string username, string password, Guid deviceId)
        {
            // DSSock.AttemptLogin
            SendPacket("+|.|" + username + "|.|" + password + "|.|" + Version + "|.|" + Encryption.FixDeviceId(deviceId) + "|.|" + "Windows 10  (10.0.0) 64bit");
            // TODO: Add an option to select the OS we want, it could be useful.
            // I use Windows 10 here because the version is the same for everyone. This is not the case on Windows 7 or Mac.
        }

        public void SendUseItem(int id, int pokemon = 0)
        {
            string toSend = "*|.|" + id;
            if (pokemon != 0)
            {
                toSend += "|.|" + Team[pokemon - 1].DatabaseId;
            }
            SendPacket(toSend);
        }

        public void SendGiveItem(int databaseId, int itemId)
        {
            SendPacket("<|.|" + databaseId + "|" + itemId);
        }

        public void SendTakeItem(int databaseId)
        {
            SendPacket(">|.|" + databaseId);
        }

        public void LearnMove(int pokemonUid, int moveToForgetUid)
        {
            _swapTimeout.Set();
            SendLearnMove(pokemonUid, moveToForgetUid);
        }

        private void SendLearnMove(int pokemonUid, int moveToForgetUid)
        {
            SendPacket("^|.|" + (pokemonUid) + "|.|" + moveToForgetUid);
        }

        private void SendMovePokemonToPC(int pokemonUid)
        {
            SendPacket("?|.|" + pokemonUid + "|.|-1");
        }

        // if there is a pokemon in teamSlot, it will be swapped
        private void SendMovePokemonFromPC(int pokemonUid, int teamSlot)
        {
            SendPacket("?|.|" + pokemonUid + "|.|" + teamSlot);
        }

        private void SendRefreshPCBox(int box, string search)
        {
            SendPacket("M|.|" + box + "|.|" + search);
        }

        private void SendReleasePokemon(int pokemonUid)
        {
            SendMessage("/release " + pokemonUid + ", 1");
            SendPacket("mb|.|/release " + pokemonUid);
        }

        private void SendPrivateMessageOn()
        {
            SendMessage("/pmon");
        }

        private void SendPrivateMessageOff()
        {
            SendMessage("/pmoff");
        }

        private void SendPartyInspectionOn()
        {
            SendMessage("/in1");
        }

        private void SendPartyInspectionOff()
        {
            SendMessage("/in0");
        }

        private void SendPrivateMessageAway()
        {
            SendMessage("/pmaway");
        }

        private void SendRequestGuildData(int id)
        {
            SendPacket(":|.|" + id);
        }

        public bool PrivateMessageOn()
        {
            IsPrivateMessageOn = true;
            SendPrivateMessageOn();
            return true;
        }

        public bool PrivateMessageOff()
        {
            IsPrivateMessageOn = false;
            SendPrivateMessageOff();
            return true;
        }

        public bool PartyInspectionOn()
        {
            IsPartyInspectionOn = true;
            SendPartyInspectionOn();
            return true;
        }

        public bool PartyInspectionOff()
        {
            IsPartyInspectionOn = false;
            SendPartyInspectionOff();
            return true;
        }

        public bool NpcInteractionsOn()
        {
            IsNpcInteractionsOn = true;
            return true;
        }

        public bool NpcInteractionsOff()
        {
            IsNpcInteractionsOn = false;
            return true;
        }

        // /pmaway does not seem to do anything
        public bool PrivateMessageAway()
        {
            SendPrivateMessageAway();
            return true;
        }

        public bool ReleasePokemonFromPC(int boxId, int boxPokemonId)
        {
            if (!IsPCOpen || IsPCBoxRefreshing || boxId < 1 || boxId > 67
                || boxPokemonId < 1 || boxPokemonId > 15 || boxPokemonId > CurrentPCBox.Count)
            {
                return false;
            }
            int pokemonUid = GetPokemonPCUid(boxId, boxPokemonId);
            if (pokemonUid == -1 || pokemonUid != CurrentPCBox[boxPokemonId - 1].Uid)
            {
                return false;
            }
            _refreshingPCBox.Set(Rand.Next(1500, 2000));
            SendReleasePokemon(pokemonUid);
            return true;
        }

        public bool ReleasePokemonFromTeam(int pokemonUid)
        {
            if (!IsPCOpen || IsPCBoxRefreshing
                || pokemonUid < 1 || pokemonUid > 6 || pokemonUid > Team.Count)
            {
                return false;
            }
            _refreshingPCBox.Set(Rand.Next(1500, 2000));
            SendReleasePokemon(pokemonUid);
            return true;
        }

        public bool RefreshPCBox(int boxId)
        {
            if (!IsPCOpen || boxId < 1 || boxId > 67 || _refreshingPCBox.IsActive || IsPCBoxRefreshing)
            {
                return false;
            }
            _refreshingPCBox.Set(Rand.Next(1500, 2000)); // this is the amount of time we wait for an answer
            CurrentPCBoxId = boxId;
            IsPCBoxRefreshing = true;
            CurrentPCBox = null;
            _refreshBoxTimeout = DateTime.UtcNow.AddSeconds(5); // this is to avoid a flood of the function
            SendRefreshPCBox(boxId - 1, "ID");
            return true;
        }

        public bool RefreshCurrentPCBox()
        {
            return RefreshPCBox(CurrentPCBoxId);
        }

        private int GetPokemonPCUid(int box, int id)
        {
            if (box < 1 || box > 67 || id < 1 || id > 15)
            {
                return -1;
            }
            int result = (box - 1) * 15 + 6 + id;
            // ensures we cannot access a pokemon we do not have or know
            if (result > PCGreatestUid || CurrentPCBox == null || box != CurrentPCBoxId)
            {
                return -1;
            }
            return result;
        }

        public bool DepositPokemonToPC(int pokemonUid)
        {
            if (!IsPCOpen || pokemonUid < 1 || pokemonUid > 6 || Team.Count < pokemonUid)
            {
                return false;
            }
            SendMovePokemonToPC(pokemonUid);
            return true;
        }

        public bool WithdrawPokemonFromPC(int boxId, int boxPokemonId)
        {
            int pcPokemonUid = GetPokemonPCUid(boxId, boxPokemonId);
            if (pcPokemonUid == -1)
            {
                return false;
            }
            if (!IsPCOpen || pcPokemonUid < 7 || pcPokemonUid > PCGreatestUid || Team.Count >= 6)
            {
                return false;
            }
            SendMovePokemonFromPC(pcPokemonUid, Team.Count + 1);
            return true;
        }

        public bool SwapPokemonFromPC(int boxId, int boxPokemonId, int teamPokemonUid)
        {
            int pcPokemonUid = GetPokemonPCUid(boxId, boxPokemonId);
            if (pcPokemonUid == -1)
            {
                return false;
            }
            if (!IsPCOpen || pcPokemonUid < 7 || pcPokemonUid > PCGreatestUid
                || teamPokemonUid < 1 || teamPokemonUid > 6 || Team.Count < teamPokemonUid)
            {
                return false;
            }
            SendMovePokemonFromPC(pcPokemonUid, teamPokemonUid);
            return true;
        }

        public bool SwapPokemon(int pokemon1, int pokemon2)
        {
            if (IsInBattle || pokemon1 < 1 || pokemon2 < 1 || Team.Count < pokemon1 || Team.Count < pokemon2 || pokemon1 == pokemon2)
            {
                return false;
            }
            if (!_swapTimeout.IsActive)
            {
                SendSwapPokemons(pokemon1, pokemon2);
                _swapTimeout.Set();
                return true;
            }
            return false;
        }

        public void Move(Direction direction)
        {
            _movements.Add(direction);
        }

        public void RequestResync()
        {
            SendMessage("/syn");
            _teleportationTimeout.Set();
        }

        public void UseAttack(int number)
        {
            SendAttack(number.ToString());
            _battleTimeout.Set();
        }

        public void UseItem(int id, int pokemonUid = 0)
        {
            if (!(pokemonUid >= 0 && pokemonUid <= 6) || !HasItemId(id))
            {
                return;
            }
            InventoryItem item = GetItemFromId(id);
            if (item == null || item.Quantity == 0)
            {
                return;
            }
            if (pokemonUid == 0) // simple use
            {
                if (!_itemUseTimeout.IsActive && !IsInBattle && (item.Scope == 8 || item.Scope == 10 || item.Scope == 15))
                {
                    SendUseItem(id);
                    _itemUseTimeout.Set();
                }
                else if (!_battleTimeout.IsActive && IsInBattle && item.Scope == 5)
                {
                    SendAttack("item" + id);
                    _battleTimeout.Set();
                }
            }
            else // use item on pokemon
            {
                if (!_itemUseTimeout.IsActive && !IsInBattle
                    && (item.Scope == 2 || item.Scope == 3 || item.Scope == 9
                        || item.Scope == 13 || item.Scope == 14))
                {
                    SendUseItem(id, pokemonUid);
                    _itemUseTimeout.Set();
                }
                else if (!_battleTimeout.IsActive && IsInBattle && item.Scope == 2)
                {
                    SendAttack("item" + id + ":" + Team[pokemonUid - 1].DatabaseId);
                    _battleTimeout.Set();
                }
            }
        }

        public bool GiveItemToPokemon(int databaseId, int itemId)
        {
            if (!(databaseId >= 1 && databaseId <= Team.Count))
            {
                return false;
            }
            InventoryItem item = GetItemFromId(itemId);
            if (item == null || item.Quantity == 0)
            {
                return false;
            }
            if (!_itemUseTimeout.IsActive && !IsInBattle
                && (item.Scope == 2 || item.Scope == 3 || item.Scope == 9 || item.Scope == 13
                || item.Scope == 14 || item.Scope == 5 || item.Scope == 12 || item.Scope == 6))
            {
                SendGiveItem(Team[databaseId - 1].DatabaseId, itemId);
                _itemUseTimeout.Set();
                return true;
            }
            return false;
        }

        public bool TakeItemFromPokemon(int databaseId)
        {
            if (!(databaseId >= 1 && databaseId <= Team.Count))
            {
                return false;
            }
            if (!_itemUseTimeout.IsActive && Team[databaseId - 1].ItemHeld != "")
            {
                SendTakeItem(Team[databaseId - 1].DatabaseId);
                _itemUseTimeout.Set();
                return true;
            }
            return false;
        }

        public bool HasSurfAbility()
        {
            return (HasMove("Surf") || WaterMount != null) &&
                (Map.Region == "1" && HasItemName("Soul Badge") ||
                Map.Region == "2" && HasItemName("Fog Badge") ||
                Map.Region == "3" && HasItemName("Balance Badge") ||
                Map.Region == "4" && HasItemName("Relic Badge"));
        }

        public bool HasCutAbility()
        {
            return (HasMove("Cut") || HasTreeaxe()) &&
                (Map.Region == "1" && HasItemName("Cascade Badge") ||
                Map.Region == "2" && HasItemName("Hive Badge") ||
                Map.Region == "3" && HasItemName("Stone Badge") ||
                Map.Region == "4" && HasItemName("Forest Badge"));
        }

        public bool HasRockSmashAbility()
        {
            return HasMove("Rock Smash") || HasPickaxe();
        }

        public bool HasTreeaxe()
        {
            return HasItemId(838) && HasItemId(317);
        }

        public bool HasPickaxe()
        {
            return HasItemId(839);
        }

        public bool PokemonUidHasMove(int pokemonUid, string moveName)
        {
            return Team.FirstOrDefault(p => p.Uid == pokemonUid)?.Moves.Any(m => m.Name?.Equals(moveName, StringComparison.InvariantCultureIgnoreCase) ?? false) ?? false;
        }

        public Pokemon GetPokemonFromDBId(int pokemonDBId) => Team.Find(pokemon => pokemon.DatabaseId == pokemonDBId);

        public bool HasMove(string moveName)
        {
            return Team.Any(p => p.Moves.Any(m => m.Name?.Equals(moveName, StringComparison.InvariantCultureIgnoreCase) ?? false));
        }

        public int GetMovePosition(int pokemonUid, string moveName)
        {
            return Team[pokemonUid].Moves.FirstOrDefault(m => m.Name?.Equals(moveName, StringComparison.InvariantCultureIgnoreCase) ?? false)?.Position ?? -1;
        }

        public InventoryItem GetItemFromId(int id)
        {
            return Items.Find(i => i.Id == id && i.Quantity > 0);
        }

        public bool HasItemId(int id)
        {
            return GetItemFromId(id) != null;
        }

        public InventoryItem GetItemFromName(string itemName)
        {
            return Items.Find(i => i.Name?.Equals(itemName, StringComparison.InvariantCultureIgnoreCase) == true 
                && i.Quantity > 0);
        }

        public bool HasItemName(string itemName)
        {
            return GetItemFromName(itemName) != null;
        }

        public bool HasPokemonInTeam(string pokemonName)
        {
            return FindFirstPokemonInTeam(pokemonName) != null;
        }

        public Pokemon FindFirstPokemonInTeam(string pokemonName)
        {
            return Team.Find(p => p.Name.Equals(pokemonName, StringComparison.InvariantCultureIgnoreCase));
        }

        public void UseSurf()
        {
            if (WaterMount == null)
            {
                SendPacket("w|.|/surf");
            }
            else
            {
                LogMessage?.Invoke($"Mounting [{WaterMount.Name}]");
                UseItem(WaterMount.Id);
            }
            
            _mountingTimeout.Set();
        }
        
        public void UseSurfAfterMovement()
        {
            _surfAfterMovement = true;
        }

        public void RunFromBattle()
        {
            UseAttack(5);
        }

        public void ChangePokemon(int number)
        {
            UseAttack(number + 5);
        }

        public void TalkToNpc(Npc npc)
        {
            npc.CanBattle = false;

            SendTalkToNpc(npc.Id);
            _dialogTimeout.Set();
        }

        public bool OpenPC()
        {
            if (!Map.IsPC(PlayerX, PlayerY - 1))
            {
                return false;
            }
            IsPCOpen = true;
            return RefreshPCBox(1);
        }

        public void PushDialogAnswer(int index)
        {
            _dialogResponses.Enqueue(index);
        }

        public void PushDialogAnswer(string text)
        {
            _dialogResponses.Enqueue(text);
        }

        public bool BuyItem(int itemId, int quantity)
        {
            if (OpenedShop != null && OpenedShop.Items.Any(item => item.Id == itemId))
            {
                _itemUseTimeout.Set();
                SendShopPokemart(OpenedShop.Id, itemId, quantity);
                return true;
            }
            return false;
        }

        public bool PurchaseMove(string moveName)
        {
            if (MoveRelearner != null && MoveRelearner.Moves.Any(move => move.Name.ToLowerInvariant() == moveName.ToLowerInvariant()))
            {
                _moveRelearnerTimeout.Set();
                SendPurchaseMove(MoveRelearner.SelectedPokemonUid, moveName);
                return true;
            }
            return false;
        }

        private void SendPurchaseMove(int pokemonUid, string moveName)
        {
            // DSSock.cs handles Move Relearn as below.

            if (MoveRelearner != null)
            {
                if (!MoveRelearner.IsEgg)
                {
                    int moveId = MovesManager.Instance.GetMoveId(moveName);
                    if (moveId != -1)
                    {
                        SendPacket("z|.|" + pokemonUid + "|.|" + moveId);
                    }
                }
                else
                {
                    int moveId = MovesManager.Instance.GetMoveId(moveName);
                    if (moveId != -1)
                    {
                        SendPacket("b|.|" + pokemonUid + "|.|" + moveId);
                    }
                }
            }
        }

        private void MapClient_ConnectionOpened()
        {
#if DEBUG
            Console.WriteLine("[+++] Connecting to the game server");
#endif
            if (MapName != null && Map == null)
            {
                _mapClient.DownloadMap(MapName);
            }
        }

        private void MapClient_ConnectionFailed(Exception ex)
        {
            ConnectionFailed?.Invoke(ex);
        }

        private void MapClient_ConnectionClosed(Exception ex)
        {
            Close(ex);
        }

        private void MapClient_MapLoaded(string mapName, Map map)
        {
            if (mapName == MapName)
            {
                Players.Clear();

                Map = map;
                // DSSock.loadMap
                SendPacket("-");
                SendPacket("k|.|" + MapName.ToLowerInvariant());

                CanUseCut = HasCutAbility();
                CanUseSmashRock = HasRockSmashAbility();

                MapLoaded?.Invoke(MapName);
            }
            else
            {
                InvalidPacket?.Invoke(mapName, "Received a map that is not the current map");
            }
        }

        private void OnPacketReceived(string packet)
        {
            ProcessPacket(packet);
        }

        private void OnConnectionOpened()
        {
            IsConnected = true;
#if DEBUG
            Console.WriteLine("[+++] Connection opened");
#endif
            ConnectionOpened?.Invoke();
        }

        private void OnConnectionClosed(Exception ex)
        {
            _mapClient.Close();
            if (!IsConnected)
            {
#if DEBUG
                Console.WriteLine("[---] Connection failed");
#endif
                ConnectionFailed?.Invoke(ex);
            }
            else
            {
                IsConnected = false;
#if DEBUG
                Console.WriteLine("[---] Connection closed");
#endif
                ConnectionClosed?.Invoke(ex);
            }
        }

        private void SendMovement(string direction)
        {
            _lastMovement = DateTime.UtcNow;
            // Consider the pokemart closed after the first movement.
            OpenedShop = null;
            MoveRelearner = null;
            IsPCOpen = false;
            // DSSock.sendMove
            SendPacket("#|.|" + direction);
        }

        private void SendAttack(string number)
        {
            // DSSock.sendAttack
            // DSSock.RunButton
            SendPacket("(|.|" + number);
        }

        private void SendTalkToNpc(int npcId)
        {
            // DSSock.Interact
            SendPacket("N|.|" + npcId);
        }

        private void SendDialogResponse(int number)
        {
            // DSSock.ClickButton
            //SendPacket("R|.|" + ScriptId + "|.|" + number);
#if DEBUG
           
#endif
            if (ScriptStatus < 4)
                SendPacket("R|.|" + number);
            else
            {
                SendPacket("R|.|" + Team[number - 1].DatabaseId);
            }

        }

        public void SendAcceptEvolution(int evolvingPokemonDBid)
        {
            // DSSock.AcceptEvo
            SendPacket("h|.|" + evolvingPokemonDBid);
        }

        public void SendCancelEvolution(int evolvingPokemonDBid)
        {
            // DSSock.CancelEvo
            SendPacket("j|.|" + evolvingPokemonDBid);
        }

        private void SendSwapPokemons(int pokemon1, int pokemon2)
        {
            SendPacket("?|.|" + pokemon2 + "|.|" + pokemon1);
        }

        private void SendShopPokemart(int shopId, int itemId, int quantity)
        {
            SendPacket("c|.|" + shopId + "|.|" + itemId + "|.|" + quantity);
        }

        private void ProcessPacket(string packet)
        {
#if DEBUG
            Console.WriteLine(packet);
#endif

            if (packet.Substring(0, 1) == "U")
            {
                packet = "U|.|" + packet.Substring(1);
            }

            string[] data = packet.Split(new [] { "|.|" }, StringSplitOptions.None);
            string type = data[0].ToLowerInvariant();
            try
            {
                switch (type)
                {
                    case "mb":
                        MoneyTradeAccepter(packet);
                        break;
                    case "tu":
                        AcceptTrade(packet);
                        //LogMessage("Check if trade window opened or not!!!");
                        break;
                    case "t":
                        InitiateMoneyTrade(packet);
                        //LogMessage("Check if trade window opened or not!!!");
                        break;
                    case "p":
                        OnPokedexInfo(packet);
                        break;
                    case "5":
                        OnLoggedIn(data);
                        break;
                    case "6":
                        OnAuthenticationResult(data);
                        break;
                    case "l":
                        //Move relearn content
                        OnMoveRelearn(data);
                        break;
                    case ")":
                        OnQueueUpdated(data);
                        break;
                    case "q":
                        OnPlayerPosition(data);
                        break;
                    case "s":
                        OnPlayerSync(data);
                        break;
                    case "i":
                        OnPlayerInfos(data);
                        break;
                    case "(":
                        // CDs ?
                        break;
                    case "e":
                        OnUpdateTime(data);
                        break;
                    case "@":
                        OnNpcBattlers(data);
                        break;
                    case "#":
                        OnTeamUpdate(data);
                        break;
                    case "d":
                        OnInventoryUpdate(data);
                        break;
                    case "&":
                        OnItemsUpdate(data);
                        break;
                    case "!":
                        OnBattleJoin(packet);
                        break;
                    case "a":
                        OnBattleMessage(data);
                        break;
                    case "r":
                        OnScript(data);
                        break;
                    case "$":
                        OnBikingUpdate(data);
                        break;
                    case "%":
                        OnSurfingUpdate(data);
                        break;
                    case "^":
                        OnLearningMove(data);
                        break;
                    case "h":
                        OnEvolving(data);
                        break;
                    case "=":
                        OnUpdatePlayer(data);
                        break;
                    case "c":
                        OnChannels(data);
                        break;
                    case "w":
                        OnChatMessage(data);
                        break;
                    case "o":
                        // Shop content
                        break;
                    case "pm":
                        OnPrivateMessage(data);
                        break;
                    case ".":
                        // DSSock.ProcessCommands
                        SendPacket("_");
                        break;
                    case "'":
                        // DSSock.ProcessCommands
                        SendPacket("'");
                        break;
                    case "m":
                        OnPCBox(data);
                        break;
                    case "z":
                        OnPlayerMovement(data);
                        break;
                    case "y":
                        OnGuildData(data);
                        break;
                    //case "k":
                    //    OnMapSpawnData(data);
                    //    break;
                    default:
#if DEBUG
                        Console.WriteLine(" ^ unhandled /!\\");
                        Console.WriteLine($"Type: ------ {type} ------");
                        Console.WriteLine($"Packet: ------ {packet} ------");
#endif
                        break;
                }
            }
            catch(System.FormatException)
            {
                LogMessage?.Invoke("Format error occurred(Probably server issue): " + packet);
            }
        }


        private void OnLoggedIn(string[] data)
        {
            Console.WriteLine("[Login] Authenticated successfully, connecting to map server");

            IsCreatingNewCharacter = data[1] == "1";

            string[] mapServerHost = data[2].Split(':');

            _mapClient.Open(Server.GetMapAddress(), int.Parse(mapServerHost[1]));

            // DSSock.ProcessCommands
            SendMessage("/in1");
            // TODO: Add a setting to disable the party inspection (send /in0 instead).
            SendPacket(")");
            SendPacket("_");
            SendPacket("g");
            SendPacket("p|.|l|0");
            SendRegularPing();
            IsAuthenticated = true;

            LoggedIn?.Invoke();
        }

        private void OnAuthenticationResult(string[] data)
        {
            AuthenticationResult result = (AuthenticationResult)Convert.ToInt32(data[1]);

            if (result != AuthenticationResult.ServerFull)
            {
                AuthenticationFailed?.Invoke(result);
                Close();
            }
        }

        private void OnQueueUpdated(string[] data)
        {
            string[] queueData = data[1].Split('|');

            int position = Convert.ToInt32(queueData[0]);
            QueueUpdated?.Invoke(position);
        }

        private void OnPlayerPosition(string[] data)
        {
            string[] mapData = data[1].Split(new[] { "|" }, StringSplitOptions.None);
            string map = mapData[0];
            int playerX = Convert.ToInt32(mapData[1]);
            int playerY = Convert.ToInt32(mapData[2]);
            if (playerX != PlayerX || playerY != PlayerY || map != MapName)
            {
                TeleportationOccuring?.Invoke(map, playerX, playerY);

                PlayerX = playerX;
                PlayerY = playerY;
                LoadMap(map);
                IsOnGround = (mapData[3] == "1");
                if (Convert.ToInt32(mapData[4]) == 1)
                {
                    IsSurfing = true;
                    IsBiking = false;
                }
                // DSSock.sendSync
                SendPacket("S");
            }

            PositionUpdated?.Invoke(MapName, PlayerX, playerY);

            _teleportationTimeout.Cancel();
        }

        // Server sends some movement data to move the character.
        private void OnPlayerMovement(string[] data)
        {
            _dialogTimeout.Set();
            _movements.Clear();
            string[] movements = data[1].Split(new[] { "|" }, StringSplitOptions.None);
            foreach(var movement in movements)
            {
                Move(DirectionExtensions.FromChar(movement[0]));
            }

            _teleportationTimeout.Cancel();
        }

        private void OnPlayerSync(string[] data)
        {
            // S|.|Pewter City|24|36|1
            string[] mapData = data[1].Split(new[] { "|" }, StringSplitOptions.None);

            if (mapData.Length < 2)
                return;

            string map = mapData[0];
            int playerX = Convert.ToInt32(mapData[1]);
            int playerY = Convert.ToInt32(mapData[2]);
            if (map.Length > 1)
            {
                PlayerX = playerX;
                PlayerY = playerY;
                LoadMap(map);
            }
            IsOnGround = (mapData[3] == "1");

            PositionUpdated?.Invoke(MapName, PlayerX, playerY);
        }

        private void AcceptTrade(string data)
        {
            if (TradeGiver)
            {
                SendPacket("tac");
            }
            else if (TradeAccepter)
            {
                SendPacket("tac");
            }
        }

        private void MoneyTradeAccepter(string data)
        {
            string[] newstr = data.Split(new[] { "|Y|" }, StringSplitOptions.None);
            data = newstr[1].Replace("|", "");
            if (TradeAccepter)
            {
                SendPacket("mb|.|" + data);
            }
        }

        private void InitiateMoneyTrade(string data)
        {
            string UsernameToTradeWith = data.Replace("t|.|", "");
            if(TradeGiver)
            {
                if(MoneyToTrade <= Money)
                {
                    if (MoneyToTrade > 0 && Money > 10000)
                    {
                        SendPacket("ta|.|m:" + MoneyToTrade);
                    }
                    else if (MoneyToTrade == -2 && Money > 10000)
                    {
                        SendPacket("ta|.|m:" + Money);
                    }
                    else if (MoneyToTrade == -1 && Money > 10000)
                    {
                        SendPacket("ta|.|m:" + (Money - new Random().Next(8000, 10000)));
                    }
                    else if(Money == 0)
                    {
                        LogMessage("No Money Left!!!");
                    }
                    else
                    {
                        LogMessage("No Money Left!!!");
                    }
                }
                else
                {
                    LogMessage("Not Enough Money");
                }
                
            }
            else if(TradeAccepter)
            {
                SendPacket("tac");
            }
        }

        private void OnPokedexInfo(string data)
        {
            string PokedexData = data.Replace("p|.|l|0|", "");
            //Console.WriteLine($"New String: {PokedexData}");
            string[] RefinedPokedexData = PokedexData.Split(new[] { "<" }, StringSplitOptions.None);
            string[] temp, temp2;
            string Evolution_Data = "002\r\n" + "003\r\n" + "005\r\n" + "006\r\n" + "008\r\n" + "009\r\n" + "011\r\n" + "012\r\n" + "014\r\n" + "015\r\n" + "017\r\n" + "018\r\n" + "020\r\n" + "022\r\n" + "024\r\n" + "025\r\n" + "026\r\n" + "028\r\n" + "030\r\n" + "031\r\n" + "033\r\n" + "034\r\n" + "035\r\n" + "036\r\n" + "038\r\n" + "039\r\n" + "040\r\n" + "042\r\n" + "044\r\n" + "045\r\n" + "047\r\n" + "049\r\n" + "051\r\n" + "053\r\n" + "055\r\n" + "057\r\n" + "059\r\n" + "061\r\n" + "062\r\n" + "064\r\n" + "065\r\n" + "067\r\n" + "068\r\n" + "070\r\n" + "071\r\n" + "073\r\n" + "075\r\n" + "076\r\n" + "078\r\n" + "080\r\n" + "082\r\n" + "085\r\n" + "087\r\n" + "089\r\n" + "091\r\n" + "093\r\n" + "094\r\n" + "097\r\n" + "099\r\n" + "101\r\n" + "103\r\n" + "105\r\n" + "106\r\n" + "107\r\n" + "110\r\n" + "112\r\n" + "113\r\n" + "117\r\n" + "119\r\n" + "121\r\n" + "122\r\n" + "124\r\n" + "125\r\n" + "126\r\n" + "130\r\n" + "134\r\n" + "135\r\n" + "136\r\n" + "139\r\n" + "141\r\n" + "143\r\n" + "148\r\n" + "149\r\n" + "153\r\n" + "154\r\n" + "156\r\n" + "157\r\n" + "159\r\n" + "160\r\n" + "162\r\n" + "164\r\n" + "166\r\n" + "168\r\n" + "169\r\n" + "171\r\n" + "176\r\n" + "178\r\n" + "180\r\n" + "181\r\n" + "182\r\n" + "183\r\n" + "184\r\n" + "185\r\n" + "186\r\n" + "188\r\n" + "189\r\n" + "192\r\n" + "195\r\n" + "196\r\n" + "197\r\n" + "199\r\n" + "202\r\n" + "205\r\n" + "208\r\n" + "210\r\n" + "212\r\n" + "217\r\n" + "219\r\n" + "221\r\n" + "224\r\n" + "226\r\n" + "229\r\n" + "230\r\n" + "232\r\n" + "233\r\n" + "237\r\n" + "242\r\n" + "247\r\n" + "248\r\n" + "253\r\n" + "254\r\n" + "256\r\n" + "257\r\n" + "259\r\n" + "260\r\n" + "262\r\n" + "264\r\n" + "266\r\n" + "267\r\n" + "268\r\n" + "269\r\n" + "271\r\n" + "272\r\n" + "274\r\n" + "275\r\n" + "277\r\n" + "279\r\n" + "281\r\n" + "282\r\n" + "284\r\n" + "286\r\n" + "288\r\n" + "289\r\n" + "291\r\n" + "292\r\n" + "294\r\n" + "295\r\n" + "297\r\n" + "301\r\n" + "305\r\n" + "306\r\n" + "308\r\n" + "310\r\n" + "315\r\n" + "317\r\n" + "319\r\n" + "321\r\n" + "323\r\n" + "326\r\n" + "329\r\n" + "330\r\n" + "332\r\n" + "334\r\n" + "340\r\n" + "342\r\n" + "344\r\n" + "346\r\n" + "348\r\n" + "350\r\n" + "354\r\n" + "356\r\n" + "358\r\n" + "362\r\n" + "364\r\n" + "365\r\n" + "367\r\n" + "368\r\n" + "372\r\n" + "373\r\n" + "375\r\n" + "376\r\n" + "388\r\n" + "389\r\n" + "391\r\n" + "392\r\n" + "394\r\n" + "395\r\n" + "397\r\n" + "398\r\n" + "400\r\n" + "402\r\n" + "404\r\n" + "405\r\n" + "407\r\n" + "409\r\n" + "411\r\n" + "413\r\n" + "414\r\n" + "416\r\n" + "419\r\n" + "421\r\n" + "423\r\n" + "424\r\n" + "426\r\n" + "428\r\n" + "429\r\n" + "430\r\n" + "432\r\n" + "435\r\n" + "437\r\n" + "444\r\n" + "445\r\n" + "448\r\n" + "450\r\n" + "452\r\n" + "454\r\n" + "457\r\n" + "460\r\n" + "461\r\n" + "462\r\n" + "463\r\n" + "464\r\n" + "465\r\n" + "466\r\n" + "467\r\n" + "468\r\n" + "469\r\n" + "470\r\n" + "471\r\n" + "472\r\n" + "473\r\n" + "474\r\n" + "475\r\n" + "476\r\n" + "477\r\n" + "478\r\n" + "496\r\n" + "497\r\n" + "499\r\n" + "500\r\n" + "502\r\n" + "503\r\n" + "505\r\n" + "507\r\n" + "508\r\n" + "510\r\n" + "512\r\n" + "514\r\n" + "516\r\n" + "518\r\n" + "520\r\n" + "521\r\n" + "523\r\n" + "525\r\n" + "526\r\n" + "528\r\n" + "530\r\n" + "533\r\n" + "534\r\n" + "536\r\n" + "537\r\n" + "541\r\n" + "542\r\n" + "544\r\n" + "545\r\n" + "547\r\n" + "549\r\n" + "552\r\n" + "553\r\n" + "555\r\n" + "558\r\n" + "560\r\n" + "563\r\n" + "565\r\n" + "567\r\n" + "569\r\n" + "571\r\n" + "573\r\n" + "575\r\n" + "576\r\n" + "578\r\n" + "579\r\n" + "581\r\n" + "583\r\n" + "584\r\n" + "586\r\n" + "589\r\n" + "591\r\n" + "593\r\n" + "596\r\n" + "598\r\n" + "600\r\n" + "601\r\n" + "603\r\n" + "604\r\n" + "606\r\n" + "608\r\n" + "609\r\n" + "611\r\n" + "612\r\n" + "614\r\n" + "617\r\n" + "620\r\n" + "623\r\n" + "625\r\n" + "628\r\n" + "630\r\n" + "634\r\n" + "635\r\n" + "637\r\n" + "651\r\n" + "652\r\n" + "654\r\n" + "655\r\n" + "657\r\n" + "658\r\n" + "660\r\n" + "662\r\n" + "663\r\n" + "665\r\n" + "666\r\n" + "668\r\n" + "670\r\n" + "671\r\n" + "673\r\n" + "675\r\n" + "678\r\n" + "680\r\n" + "681\r\n" + "683\r\n" + "685\r\n" + "687\r\n" + "689\r\n" + "691\r\n" + "693\r\n" + "695\r\n" + "697\r\n" + "699\r\n" + "700\r\n" + "705\r\n" + "706\r\n" + "709\r\n" + "711\r\n" + "713\r\n" + "715\r\n";

            for (int i = 0; i < RefinedPokedexData.Length; i++)
            {
                RefinedPokedexData[i] = RefinedPokedexData[i].PadLeft(5, '0');
            }

            for (int i = 0; i < RefinedPokedexData.Length; i++)
            {
                temp2 = RefinedPokedexData[i].Split(new[] { ">" }, StringSplitOptions.None);
                if (Evolution_Data.Contains(temp2[0]))
                {
                    if (!(temp2[1] == "3"))
                    {
                        Evolution_Counter++;
                        Evolution_Left = Evolution_Left + temp2[0] + Environment.NewLine;
                    }
                }

            }

            KantoAllPoke = 151;
            JohtoAllPoke = 100;
            HoennAllPoke = 135;
            SinnohAllPoke = 107;
            OtherAllPoke = 314;

            KantoSeen = 0;
            KantoOwned = 0;
            KantoEvolved = 0;

            JohtoSeen = 0;
            JohtoOwned = 0;
            JohtoEvolved = 0;

            HoennSeen = 0;
            HoennOwned = 0;
            HoennEvolved = 0;

            SinnohSeen = 0;
            SinnohOwned = 0;
            SinnohEvolved = 0;

            OtherSeen = 0;
            OtherOwned = 0;
            OtherEvolved = 0;

            for (int i = 0; i < RefinedPokedexData.Length; i++)
            {
                temp = RefinedPokedexData[i].Split(new[] { ">" }, StringSplitOptions.None);
                if (Convert.ToInt32(temp[0]) >= 1 && Convert.ToInt32(temp[0]) <= 151)
                {
                    if (Convert.ToInt32(temp[1]) == 1)
                    {
                        KantoSeen += 1;
                    }
                    else if (Convert.ToInt32(temp[1]) == 2)
                    {
                        KantoSeen += 1;
                        KantoOwned += 1;
                    }
                    else if (Convert.ToInt32(temp[1]) == 3)
                    {
                        KantoSeen += 1;
                        KantoOwned += 1;
                        KantoEvolved += 1;
                    }
                }
                else if (Convert.ToInt32(temp[0]) >= 152 && Convert.ToInt32(temp[0]) <= 251)
                {
                    if (Convert.ToInt32(temp[1]) == 1)
                    {
                        JohtoSeen += 1;
                    }
                    else if (Convert.ToInt32(temp[1]) == 2)
                    {
                        JohtoSeen += 1;
                        JohtoOwned += 1;
                    }
                    else if (Convert.ToInt32(temp[1]) == 3)
                    {
                        JohtoSeen += 1;
                        JohtoOwned += 1;
                        JohtoEvolved += 1;
                    }
                }
                else if (Convert.ToInt32(temp[0]) >= 252 && Convert.ToInt32(temp[0]) <= 386)
                {
                    if (Convert.ToInt32(temp[1]) == 1)
                    {
                        HoennSeen += 1;
                    }
                    else if (Convert.ToInt32(temp[1]) == 2)
                    {
                        HoennSeen += 1;
                        HoennOwned += 1;
                    }
                    else if (Convert.ToInt32(temp[1]) == 3)
                    {
                        HoennSeen += 1;
                        HoennOwned += 1;
                        HoennEvolved += 1;
                    }
                }
                else if (Convert.ToInt32(temp[0]) >= 387 && Convert.ToInt32(temp[0]) <= 493)
                {
                    if (Convert.ToInt32(temp[1]) == 1)
                    {
                        SinnohSeen += 1;
                    }
                    else if (Convert.ToInt32(temp[1]) == 2)
                    {
                        SinnohSeen += 1;
                        SinnohOwned += 1;
                    }
                    else if (Convert.ToInt32(temp[1]) == 3)
                    {
                        SinnohSeen += 1;
                        SinnohOwned += 1;
                        SinnohEvolved += 1;
                    }
                }
                else if (Convert.ToInt32(temp[0]) >= 494 && Convert.ToInt32(temp[0]) <= 807)
                {
                    if (Convert.ToInt32(temp[1]) == 1)
                    {
                        OtherSeen += 1;
                    }
                    else if (Convert.ToInt32(temp[1]) == 2)
                    {
                        OtherSeen += 1;
                        OtherOwned += 1;
                    }
                    else if (Convert.ToInt32(temp[1]) == 3)
                    {
                        OtherSeen += 1;
                        OtherOwned += 1;
                        OtherEvolved += 1;
                    }
                }

                //Console.WriteLine(mapData[i]);
            }

        }

        private void OnPlayerInfos(string[] data)
        {
            string[] playerData = data[1].Split('|');
            PlayerName = playerData[0];
            PokedexOwned = Convert.ToInt32(playerData[4]);
            PokedexSeen = Convert.ToInt32(playerData[5]);
            PokedexEvolved = Convert.ToInt32(playerData[6]);
            IsMember = playerData[10] == "1";
            if (GuildId != -1 && !_requestedGuildData.Contains(GuildId)) 
            {
                SendRequestGuildData(GuildId);
                _requestedGuildData.Add(GuildId);
            }
        }

        private void OnGuildData(string[] data)
        {
            //y|.|Guild name|999(id)|guild description|total members format: (total/max)|Leader Name|.\
            data = data[1].Split('|');
            if (data.Length > 1)
            {
                GuildId = int.Parse(data[1]);
            }
        }

        private void OnMapSpawnData(string[] data)
        {
            // k|.|Route 4,ci21,i24,41,23,c163,39,ci19,m0,s41,sci60,s79,ms0,s0,
            // c = caught
            // i = item
            // m = membership
            // s = surf
            data = data[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string mapName = data[0];

            if (data[1] == "none!")
            {
                return;
            }

            foreach (var pokemonData in data.Skip(1))
            {

            }

        }

        private void OnUpdateTime(string[] data)
        {
            string[] timeData = data[1].Split('|');

            PokemonTime = timeData[0];

            Weather = timeData[1];

            PokeTimeUpdated?.Invoke(PokemonTime, Weather);
        }

        private void OnNpcBattlers(string[] data)
        {
            if (!IsMapLoaded) return;

            var npcData = data[1].Split('*');
            var defeatedNpcs = npcData[0].Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);
            var destroyedNpcs = npcData[1].Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);

            Map.Npcs.Clear();
            foreach (Npc npc in Map.OriginalNpcs)
            {
                if (!destroyedNpcs.Contains(npc.Id))
                {
                    Npc clone = npc.Clone();
                    if (defeatedNpcs.Contains(npc.Id))
                        clone.CanBattle = false;

                    Map.Npcs.Add(clone);
                }
            }

            AreNpcReceived = true;
            NpcReceived?.Invoke(Map.Npcs);
        }

        private void OnTeamUpdate(string[] data)
        {
            string[] teamData = data[1].Split(new[] { "\r\n" }, StringSplitOptions.None);

            Team.Clear();
            foreach (string pokemon in teamData)
            {
                if (pokemon == string.Empty)
                    continue;

                string[] pokemonData = pokemon.Split('|');

                Team.Add(new Pokemon(pokemonData));
            }

            if (IsMapLoaded)
            {
                CanUseCut = HasCutAbility();
                CanUseSmashRock = HasRockSmashAbility();
            }

            if (_swapTimeout.IsActive)
            {
                _swapTimeout.Set(Rand.Next(500, 1000));
            }
            PokemonsUpdated?.Invoke();
        }

        private void OnInventoryUpdate(string[] data)
        {
            Money = Convert.ToInt32(data[1]);
            Coins = Convert.ToInt32(data[2]);
            UpdateItems(data[3]);
        }

        private void OnItemsUpdate(string[] data)
        {
            UpdateItems(data[1]);
        }

        private void UpdateItems(string content)
        {
            Items.Clear();

            string[] itemsData = content.Split(new[] { "\r\n" }, StringSplitOptions.None);
            foreach (string item in itemsData)
            {
                if (item == string.Empty)
                    continue;
                string[] itemData = item.Split(new[] { "|" }, StringSplitOptions.None);
                Items.Add(new InventoryItem(Convert.ToInt32(itemData[0]), Convert.ToInt32(itemData[1]), Convert.ToInt32(itemData[2])));
            }

            if (_itemUseTimeout.IsActive)
            {
                _itemUseTimeout.Set(Rand.Next(500, 1000));
            }
            InventoryUpdated?.Invoke();
        }

        private void OnBattleJoin(string packet)
        {
            string[] data = packet.Substring(4).Split('|');

            IsScriptActive = false;

            IsInBattle = true;
            ActiveBattle = new Battle(PlayerName, data);
            ActiveBattle.ActivePokemonChanged += ActivePokemonChanged;
            ActiveBattle.OpponentChanged += OpponentChanged;

            _movements.Clear();
            _slidingDirection = null;

            _battleTimeout.Set(Rand.Next(4000, 5000));
            _fishingTimeout.Cancel();

            BattleStarted?.Invoke();

            string[] battleMessages = ActiveBattle.BattleText.Split(new[] { "\r\n" }, StringSplitOptions.None);

            foreach (string message in battleMessages)
            {
                if (!ActiveBattle.ProcessMessage(Team, message))
                {
                    BattleMessage?.Invoke(I18n.Replace(message));
                }
            }

            BattleUpdated?.Invoke();
        }

        private void OnBattleMessage(string[] data)
        {
            if (!IsInBattle)
            {
                return;
            }

            string[] battleData = data[1].Split(new string[] { "|" }, StringSplitOptions.None);
            string[] battleMessages = battleData[4].Split(new string[] { "\r\n" }, StringSplitOptions.None);

            foreach (string message in battleMessages)
            {
                if (!ActiveBattle.ProcessMessage(Team, message))
                {
                    BattleMessage?.Invoke(I18n.Replace(message));
                }
            }

            PokemonsUpdated?.Invoke();
            BattleUpdated?.Invoke();

            if (ActiveBattle.IsFinished)
            {
                _battleTimeout.Set(Rand.Next(4000, 7000));
            }
            else
            {
                _battleTimeout.Set(Rand.Next(4000, 7000));
            }

            if (ActiveBattle.IsFinished)
            {
                IsInBattle = false;
                BattleEnded?.Invoke();
            }
        }

        private void OnScript(string[] data)
        {
            string id = data[2];
            int status = Convert.ToInt32(data[1]);
            string script = data[3];

            DialogContent = script.Split(new string[] { "-#-" }, StringSplitOptions.None);
            bool isPrompt = script.Contains("-#-") && status > 1;
            if (isPrompt)
            {
                script = DialogContent[0];
            }
            string[] messages = script.Split(new string[] { "-=-" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < messages.Length; i++)
            {
                string message = messages[i];
                if (message.StartsWith("emote") || message.StartsWith("playsound") || message.StartsWith("playmusic") || message.StartsWith("playcry"))
                    continue;
                if (message.StartsWith("shop"))
                {
                    OpenedShop = new Shop(message.Substring(4));
                    ShopOpened?.Invoke(OpenedShop);
                    continue;
                }
                if (message.StartsWith("moverelearner"))
                {
                    int pokemonUid = Convert.ToInt32(message.Substring(13));
                    MoveRelearner = new MoveRelearner(pokemonUid, false);

                    SendPacket("a|.|" + pokemonUid);
                    continue;
                }
                if (message.StartsWith("eggsrelearner"))
                {
                    int pokemonUid = Convert.ToInt32(message.Substring(13));
                    MoveRelearner = new MoveRelearner(pokemonUid, true);

                    SendPacket(".|.|" + message.Substring(13));
                    continue;
                }

                bool lastMessage = (i == messages.Length - 1);
                if (lastMessage && isPrompt)
                {
                    var dialogOptions = new string[DialogContent.Length - 1];
                    Array.Copy(DialogContent, 1, dialogOptions, 0, dialogOptions.Length);
                    DialogOpened?.Invoke(message, dialogOptions);
                }
                else
                {
                    DialogOpened?.Invoke(message, new string[0]);
                }
            }
            
            IsScriptActive = true;
            _dialogTimeout.Set(Rand.Next(1500, 4000));
            ScriptId = id;
            ScriptStatus = status;
        }

        private void OnMoveRelearn(string[] data)
        {
            if (MoveRelearner != null)
            {
                MoveRelearner.ProcessMessage(data[1]);
                MoveRelearnerOpened?.Invoke(MoveRelearner);
            }
        }

        private void OnBikingUpdate(string[] data)
        {
            if (data[1] == "1")
            {
                IsBiking = true;
                IsSurfing = false;
            }
            else
            {
                IsBiking = false;
            }
            _mountingTimeout.Set(Rand.Next(500, 1000));
            _itemUseTimeout.Cancel();
        }

        private void OnSurfingUpdate(string[] data)
        {
            if (data[1] == "1")
            {
                IsSurfing = true;
                IsBiking = false;
            }
            else
            {
                IsSurfing = false;
            }
            _mountingTimeout.Set(Rand.Next(500, 1000));
            _itemUseTimeout.Cancel();
        }

        private void OnLearningMove(string[] data)
        {
            int moveId = Convert.ToInt32(data[1]);
            string moveName = Convert.ToString(data[2]);
            int pokemonDBid = Convert.ToInt32(data[3]);
            int movePp = Convert.ToInt32(data[4]);
            LearningMove?.Invoke(moveId, moveName, pokemonDBid);
            MoveRelearner = null;
            _itemUseTimeout.Cancel();
            _moveRelearnerTimeout.Cancel();
            // ^|.|348|.|Cut|.|26703356|.|30
        }

        private void OnEvolving(string[] data)
        {
            int evlovingPokemonDBid = Convert.ToInt32(data[1]);
            int evolvingItem = Convert.ToInt32(data[2]);

            // h|.|41258652|.|178
            //      ^^ Data base id
            Evolving.Invoke(evlovingPokemonDBid, evolvingItem);
        }

        private void OnUpdatePlayer(string[] data)
        {
            string[] updateData = data[1].Split('|');

            bool isNewPlayer = false;
            PlayerInfos player;
            DateTime expiration = DateTime.UtcNow.AddSeconds(20);
            if (Players.ContainsKey(updateData[0]))
            {
                player = Players[updateData[0]];
                player.Expiration = expiration;
            }
            else
            {
                isNewPlayer = true;
                player = new PlayerInfos(expiration);
                player.Name = updateData[0];
            }

            player.Updated = DateTime.UtcNow;
            player.PosX = Convert.ToInt32(updateData[1]);
            player.PosY = Convert.ToInt32(updateData[2]);
            player.Direction = updateData[3][0];
            player.Skin = updateData[3].Substring(1);
            player.IsAfk = updateData[4][0] != '0';
            player.IsInBattle = updateData[4][1] != '0';
            player.PokemonPetId = Convert.ToInt32(updateData[4].Substring(2));
            player.IsPokemonPetShiny = updateData[5][0] != '0';
            player.IsMember = updateData[5][1] != '0';
            player.IsOnground = updateData[5][2] != '0';
            player.GuildId = Convert.ToInt32(updateData[5].Substring(3));
            player.PetForm = Convert.ToInt32(updateData[6]); // ???

            Players[player.Name] = player;

            if (isNewPlayer)
            {
                PlayerAdded?.Invoke(player);
                if (!_requestedGuildData.Contains(player.GuildId) && player.GuildId != 0)
                {
                    SendRequestGuildData(player.GuildId);
                    _requestedGuildData.Add(player.GuildId);
                }
            }
            else
            {
                PlayerUpdated?.Invoke(player);
            }
        }

        private void OnChannels(string[] data)
        {
            Channels.Clear();
            string[] channelsData = data[1].Split('|');
            for (int i = 1; i < channelsData.Length; i += 2)
            {
                string channelId = channelsData[i];
                string channelName = channelsData[i + 1];
                Channels.Add(new ChatChannel(channelId, channelName));
            }
            RefreshChannelList?.Invoke();
        }

        private void OnChatMessage(string[] data)
        {
            string fullMessage = data[1];
            string[] chatData = fullMessage.Split(':');

            if (fullMessage[0] == '*' && fullMessage[2] == '*')
            {
                fullMessage = fullMessage.Substring(3);
            }

            string message;
            if (chatData.Length <= 1) // we are not really sure what this stands for
            {
                string channelName;

                int start = fullMessage.IndexOf('(') + 1;
                int end = fullMessage.IndexOf(')');
                if (fullMessage.Length <= end || start == 0 || end == -1)
                {
                    string packet = string.Join("|.|", data);
                    InvalidPacket?.Invoke(packet, "Channel System Message with invalid channel");
                    channelName = "";
                }
                else
                {
                    channelName = fullMessage.Substring(start, end - start);
                }

                if (fullMessage.Length <= end + 2 || start == 0 || end == -1)
                {
                    string packet = string.Join("|.|", data);
                    InvalidPacket?.Invoke(packet, "Channel System Message with invalid message");
                    message = "";
                }
                else
                {
                    message = fullMessage.Substring(end + 2);
                }

                ChannelSystemMessage?.Invoke(channelName, message);
                return;
            }
            if (chatData[0] != "*G*System")
            {
                string channelName = null;
                string mode = null;
                string author;

                int start = (fullMessage[0] == '(' ? 1 : 0);
                int end;
                if (start != 0)
                {
                    end = fullMessage.IndexOf(')');
                    if (end != -1 && end - start > 0)
                    {
                        channelName = fullMessage.Substring(start, end - start);
                    }
                    else
                    {
                        string packet = string.Join("|.|", data);
                        InvalidPacket?.Invoke(packet, "Channel Message with invalid channel name");
                        channelName = "";
                    }
                }
                start = fullMessage.IndexOf('[') + 1;
                if (start != 0 && fullMessage[start] != 'n')
                {
                    end = fullMessage.IndexOf(']');
                    if (end == -1)
                    {
                        string packet = string.Join("|.|", data);
                        InvalidPacket?.Invoke(packet, "Message with invalid mode");
                        message = "";
                    }
                    mode = fullMessage.Substring(start, end - start);
                }
                string conversation = null;
                if (channelName == "PM")
                {
                    end = fullMessage.IndexOf(':');
                    string header = "";
                    if (end == -1)
                    {
                        string packet = string.Join("|.|", data);
                        InvalidPacket?.Invoke(packet, "Channel Private Message with invalid author");
                        conversation = "";
                    }
                    else
                    {
                        header = fullMessage.Substring(0, end);
                        start = header.LastIndexOf(' ') + 1;
                        if (end == -1)
                        {
                            string packet = string.Join("|.|", data);
                            InvalidPacket?.Invoke(packet, "Channel Private Message with invalid author");
                            conversation = "";
                        }
                        else
                        {
                            conversation = header.Substring(start);
                        }
                    }
                    if (header.Contains(" to "))
                    {
                        author = PlayerName;
                    }
                    else
                    {
                        author = conversation;
                    }
                }
                else
                {
                    start = fullMessage.IndexOf("[n=") + 3;
                    end = fullMessage.IndexOf("][/n]:");
                    if (end == -1)
                    {
                        string packet = string.Join("|.|", data);
                        InvalidPacket?.Invoke(packet, "Message with invalid author");
                        author = "";
                    }
                    else
                    {
                        author = fullMessage.Substring(start, end - start);
                    }
                }
                start = fullMessage.IndexOf(':') + 2;
                if (end == -1)
                {
                    string packet = string.Join("|.|", data);
                    InvalidPacket?.Invoke(packet, "Channel Private Message with invalid message");
                    message = "";
                }
                else
                {
                    message = fullMessage.Substring(start == 1 ? 0 : start);
                }
                if (channelName != null)
                {
                    if (channelName == "PM")
                    {
                        ChannelPrivateMessage?.Invoke(conversation, mode, author, message);
                    }
                    else
                    {
                        ChannelMessage?.Invoke(channelName, mode, author, message);
                    }
                }
                else
                {
                    if (message.IndexOf("em(") == 0)
                    {
                        end = message.IndexOf(")");
                        int emoteId;
                        if (end != -1 && end - 3 > 0)
                        {
                            string emoteIdString = message.Substring(3, end - 3);
                            if (int.TryParse(emoteIdString, out emoteId) && emoteId > 0)
                            {
                                EmoteMessage?.Invoke(mode, author, emoteId);
                                return;
                            }
                        }
                    }
                    ChatMessage?.Invoke(mode, author, message);
                }
                return;
            }

            int offset = fullMessage.IndexOf(':') + 2;
            if (offset == -1 + 2) // for clarity... I prefectly know it's -3
            {
                string packet = string.Join("|.|", data);
                InvalidPacket?.Invoke(packet, "Channel Private Message with invalid author");
                message = "";
            }
            else
            {
                message = fullMessage.Substring(offset == 1 ? 0 : offset);
            }

            if (message.Contains("$YouUse the ") && message.Contains("Rod!"))
            {
                _itemUseTimeout.Cancel();
                _fishingTimeout.Set(2500 + Rand.Next(500, 1500));
            }

            SystemMessage?.Invoke(I18n.Replace(message));
        }

        private void OnPrivateMessage(string[] data)
        {
            if (data.Length < 2)
            {
                string packet = string.Join("|.|", data);
                InvalidPacket?.Invoke(packet, "PM with no parameter");
            }
            string[] nicknames = data[1].Split(new[] { "-=-" }, StringSplitOptions.None);
            if (nicknames.Length < 2)
            {
                string packet = string.Join("|.|", data);
                InvalidPacket?.Invoke(packet, "PM with invalid header");
                return;
            }

            string conversation;
            if (nicknames[0] != PlayerName)
            {
                conversation = nicknames[0];
            }
            else
            {
                conversation = nicknames[1];
            }

            if (data.Length < 3)
            {
                string packet = string.Join("|.|", data);
                InvalidPacket?.Invoke(packet, "PM without a message");
                /*
                 * the PM is sent since the packet is still understandable
                 * however, PRO client does not allow it
                 */
                PrivateMessage?.Invoke(conversation, null, conversation + " (deduced)", "");
                return;
            }

            string mode = null;
            int offset = data[2].IndexOf('[') + 1;
            int end = 0;
            if (offset != 0 && offset < data[2].IndexOf(':'))
            {
                end = data[2].IndexOf(']');
                mode = data[2].Substring(offset, end - offset);
            }

            if (data[2].Substring(0, 4) == "rem:")
            {
                LeavePrivateMessage?.Invoke(conversation, mode, data[2].Substring(4 + end));
                return;
            }
            else if (!Conversations.Contains(conversation))
            {
                Conversations.Add(conversation);
            }

            string modeRemoved = data[2];
            if (end != 0)
            {
                modeRemoved = data[2].Substring(end + 2);
            }
            offset = modeRemoved.IndexOf(' ');
            string speaker = modeRemoved.Substring(0, offset);

            offset = data[2].IndexOf(':') + 2;
            string message = data[2].Substring(offset);

            PrivateMessage?.Invoke(conversation, mode, speaker, message);
        }

        public int GetBoxIdFromPokemonUid(int lastUid)
        {
            return (lastUid - 7) / 15 + 1;
        }

        private void OnPCBox(string[] data)
        {
            _refreshingPCBox.Cancel();
            IsPCBoxRefreshing = false;
            if (Map.IsPC(PlayerX, PlayerY - 1))
            {
                IsPCOpen = true;
            }
            string[] body = data[1].Split('=');
            if (body.Length < 3)
            {
                InvalidPacket?.Invoke(data[0] + "|.|" + data[1], "Received an invalid PC Box packet");
                return;
            }
            PCGreatestUid = Convert.ToInt32(body[0]);

            int pokemonCount = Convert.ToInt32(body[1]);
            if (pokemonCount <= 0 || pokemonCount > 15)
            {
                InvalidPacket?.Invoke(data[0] + "|.|" + data[1], "Received an invalid PC Box size");
                return;
            }
            string[] pokemonListDatas = body[2].Split(new[] { "\r\n" }, StringSplitOptions.None);
            if (body.Length < 1)
            {
                InvalidPacket?.Invoke(data[0] + "|.|" + data[1], "Received an empty box");
                return;
            }
            List<Pokemon> pokemonBox = new List<Pokemon>();
            foreach (var pokemonDatas in pokemonListDatas)
            {
                if (pokemonDatas == string.Empty) continue;
                string[] pokemonDatasArray = pokemonDatas.Split('|');
                Pokemon pokemon = new Pokemon(pokemonDatasArray);
                if (CurrentPCBoxId != GetBoxIdFromPokemonUid(pokemon.Uid))
                {
                    InvalidPacket?.Invoke(data[0] + "|.|" + data[1], "Received a box packet for an unexpected box: expected #"
                        + CurrentPCBox + ", received #" + GetBoxIdFromPokemonUid(pokemon.Uid));
                    return;
                }
                pokemonBox.Add(pokemon);
            }
            if (pokemonBox.Count != pokemonCount)
            {
                InvalidPacket?.Invoke(data[0] + "|.|" + data[1], "Received a PC Box size that does not match the content");
                return;
            }
            CurrentPCBox = pokemonBox;
            PCBoxUpdated?.Invoke(CurrentPCBox);
        }

        private void LoadMap(string mapName)
        {
            mapName = MapClient.RemoveExtension(mapName);

            _loadingTimeout.Set(Rand.Next(1500, 4000));

            OpenedShop = null;
            MoveRelearner = null;
            _movements.Clear();
            _surfAfterMovement = false;
            _slidingDirection = null;
            _dialogResponses.Clear();
            _movementTimeout.Cancel();
            _mountingTimeout.Cancel();
            _itemUseTimeout.Cancel();

            if (Map == null || MapName != mapName)
            {
                DownloadMap(mapName);
            }
        }

        private void DownloadMap(string mapName)
        {
            Console.WriteLine("[Map] Requesting: " + MapName);

            Map = null;
            AreNpcReceived = false;
            MapName = mapName;
            Players.Clear();

            if (_mapClient.IsConnected)
            {
                _mapClient.DownloadMap(MapName);
            }
        }
    }
}
