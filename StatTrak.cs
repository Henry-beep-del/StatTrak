using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoldfastSharedMethods;
using UnityEngine.UI;

public class StatTrak : IHoldfastSharedMethods
{
    private InputField f1MenuInputField;
    public List<playerInfo> playerList = new List<playerInfo>();
    public List<blockInfo> blockList = new List<blockInfo>();
    public List<hitInfo> hitList = new List<hitInfo>();
    public float timeelapsed;
    public float assistTime = 10.0f;

    public class playerInfo
    {
        public int pId;
        public ulong pSId;
        public GameObject pObj;
        public string faction;
        public int[] KDA;
    }

    public class blockInfo
    {
        public int attackerBlockId;
        public int defenderBlockId;
        public float timeval;
    }

    public class hitInfo
    {
        public int attackerHitId;
        public int victimHitId;
    }

    public void OnPlayerJoined(int playerId, ulong steamId, string name, string regimentTag, bool isBot)
    {
        playerInfo newPlayer = new playerInfo();
        int[] kd = { 0, 0, 0, 0, 0, 0, 0 }; //Kills_Deaths_Assists_TKs_Blocks_Score_Impact

        newPlayer.pId = playerId;
        newPlayer.pSId = steamId;
        newPlayer.pObj = null;
        newPlayer.faction = "";
        newPlayer.KDA = kd;

        playerList.Add(newPlayer);
    }

    public void OnPlayerSpawned(int playerId, int spawnSectionId, FactionCountry playerFaction, PlayerClass playerClass, int uniformId, GameObject playerObject)
    {
        for (int x = 0; x < playerList.Count; x++)
        {
            if (playerList[x].pId == playerId)
            {
                playerList[x].faction = playerFaction.ToString();
                playerList[x].pObj = playerObject;
            }
        }
    }

    public void OnPlayerLeft(int playerId)
    {
        for (int x = 0; x < playerList.Count; x++)
        {
            if (playerList[x].pId == playerId)
            {
                playerList.Remove(playerList[x]);
            }
        }
    }

    public void OnPlayerKilledPlayer(int killerPlayerId, int victimPlayerId, EntityHealthChangedReason reason, string additionalDetails)
    {
        int killerPos = 0;
        int victimPos = 0;
        int attackerBlockId = 0;
        int defenderBlockId = 0;
        int attackerHitId = 0;

        for (int x = 0; x < playerList.Count; x++)
        {
            if (playerList[x].pId == killerPlayerId)
            {
                killerPos = x;
            }

            if (playerList[x].pId == victimPlayerId)
            {
                victimPos = x;
            }
        }

        for (int k = 0; k < hitList.Count; k++)
        {
            if (hitList[k].victimHitId == victimPlayerId)
            {
                attackerHitId = hitList[k].attackerHitId;
                break;
            }
        }

        for (int y = 0; y < blockList.Count; y++)
        {
            if (blockList[y].attackerBlockId == victimPlayerId)
            {
                defenderBlockId = blockList[y].defenderBlockId;
                Debug.Log(defenderBlockId);
            }

            if (blockList[y].defenderBlockId == victimPlayerId)
            {
                attackerBlockId = blockList[y].attackerBlockId;
                Debug.Log(attackerBlockId);
            }
        }


        if (playerList[killerPos].faction == playerList[victimPos].faction)
        {
            playerList[killerPos].KDA[3] += 1;
        }
        else
        {
            for (int j = 0; j < playerList.Count; j++)
            {
                if (playerList[j].pId == attackerBlockId)
                {
                    playerList[j].KDA[2] += 1;
                }

                if (playerList[j].pId == defenderBlockId)
                {
                    playerList[j].KDA[2] += 1;
                }

                if ((playerList[j].pId == attackerHitId) && (attackerHitId != killerPlayerId))
                {
                    playerList[j].KDA[2] += 1;
                }
            }

            playerList[killerPos].KDA[0] += 1;
            playerList[victimPos].KDA[1] += 1;
        }

        Debug.Log("Killer KDA: " + playerList[killerPos].KDA[0] + "|" + playerList[killerPos].KDA[1] + "|" + playerList[killerPos].KDA[2] + "|" + playerList[killerPos].KDA[3] + "|" + playerList[killerPos].KDA[4]);
        Debug.Log("Victim KDA: " + playerList[victimPos].KDA[0] + "|" + playerList[victimPos].KDA[1] + "|" + playerList[victimPos].KDA[2] + "|" + playerList[victimPos].KDA[3] + "|" + playerList[victimPos].KDA[4]);
    }

    public void OnPlayerBlock(int attackingPlayerId, int defendingPlayerId)
    {
        blockInfo newblock = new blockInfo();

        for (int x = 0; x < playerList.Count; x++)
        {
            if (playerList[x].pId == defendingPlayerId)
            {
                newblock.attackerBlockId = attackingPlayerId;
                newblock.defenderBlockId = defendingPlayerId;
                newblock.timeval = timeelapsed;

                blockList.Add(newblock);

                playerList[x].KDA[4] += 1;

                Debug.Log("Block added: " + attackingPlayerId + " " + defendingPlayerId + " " + newblock.timeval);
            }
        }
    }

    public void OnUpdateElapsedTime(float time)
    {
        timeelapsed = time;

        for (int x = 0; x < blockList.Count; x++)
        {
            if ((blockList[x].timeval + assistTime) <= time)
            {
                blockList.Remove(blockList[x]);
            }
        }
    }

    public void OnScorableAction(int playerId, int score, ScorableActionType reason)
    {
        Debug.Log("Scorable Action Player Id: " + playerId + " Score Gained: " + score + " Reason: " + reason);

        hitInfo newHit = new hitInfo();

        if (reason.ToString() == "PlayerWoundedPlayer")
        {
            newHit.attackerHitId = playerId;
            newHit.victimHitId = 0;

            hitList.Add(newHit);
        }
    }

    public void OnPlayerHurt(int playerId, byte oldHp, byte newHp, EntityHealthChangedReason reason)
    {
        Debug.Log("Scorable Action Player Id: " + playerId + " Reason: " + reason);

        hitList[hitList.Count - 1].victimHitId = playerId;
    }

    public void OnIsClient(bool client, ulong steamId)
    {

    }

    public void OnUpdateTimeRemaining(float time)
    {

    }

    public void OnPlayerWeaponSwitch(int playerId, string weapon)
    {

    }

    public void PassConfigVariables(string[] value)
    {

    }

    public void OnTextMessage(int playerId, TextChatChannel channel, string text)
    {

    }

    public void OnIsServer(bool server)
    {
        var canvases = Resources.FindObjectsOfTypeAll<Canvas>();
        for (int i = 0; i < canvases.Length; i++)
        {
            //Find the one that's called "Game Console Panel"
            if (string.Compare(canvases[i].name, "Game Console Panel", true) == 0)
            {
                //Inside this, now we need to find the input field where the player types messages.
                f1MenuInputField = canvases[i].GetComponentInChildren<InputField>(true);
                if (f1MenuInputField != null)
                {
                    Debug.Log("Found the Game Console Panel");
                }
                else
                {
                    Debug.Log("We did Not find Game Console Panel");
                }
                break;
            }
        }
    }

    public void OnConsoleCommand(string input, string output, bool success)
    {

    }

    public void OnSyncValueState(int value)
    {

    }

    public void OnUpdateSyncedTime(double time)
    {

    }

    public void OnDamageableObjectDamaged(GameObject damageableObject, int damageableObjectId, int shipId, int oldHp, int newHp)
    {

    }

    public void OnPlayerShoot(int playerId, bool dryShot)
    {

    }

    public void OnRoundDetails(int roundId, string serverName, string mapName, FactionCountry attackingFaction, FactionCountry defendingFaction, GameplayMode gameplayMode, GameType gameType)
    {

    }

    public void OnPlayerMeleeStartSecondaryAttack(int playerId)
    {

    }

    public void OnCapturePointCaptured(int capturePoint)
    {

    }

    public void OnCapturePointOwnerChanged(int capturePoint, FactionCountry factionCountry)
    {

    }

    public void OnCapturePointDataUpdated(int capturePoint, int defendingPlayerCount, int attackingPlayerCount)
    {

    }

    public void OnRoundEndFactionWinner(FactionCountry factionCountry, FactionRoundWinnerReason reason)
    {

    }

    public void OnRoundEndPlayerWinner(int playerId)
    {

    }

    public void OnPlayerStartCarry(int playerId, CarryableObjectType carryableObject)
    {

    }

    public void OnPlayerEndCarry(int playerId)
    {

    }

    public void OnPlayerShout(int playerId, CharacterVoicePhrase voicePhrase)
    {

    }

    public void OnInteractableObjectInteraction(int playerId, int interactableObjectId, GameObject interactableObject, InteractionActivationType interactionActivationType, int nextActivationStateTransitionIndex)
    {

    }

    public void OnEmplacementPlaced(int itemId, GameObject objectBuilt, EmplacementType emplacementType)
    {

    }

    public void OnEmplacementConstructed(int itemId)
    {

    }

    public void OnBuffStart(int playerId, BuffType buff)
    {

    }

    public void OnBuffStop(int playerId, BuffType buff)
    {

    }

    public void OnShotInfo(int playerId, int shotCount, Vector3[][] shotsPointsPositions, float[] trajectileDistances, float[] distanceFromFiringPositions, float[] horizontalDeviationAngles, float[] maxHorizontalDeviationAngles, float[] muzzleVelocities, float[] gravities, float[] damageHitBaseDamages, float[] damageRangeUnitValues, float[] damagePostTraitAndBuffValues, float[] totalDamages, Vector3[] hitPositions, Vector3[] hitDirections, int[] hitPlayerIds, int[] hitDamageableObjectIds, int[] hitShipIds, int[] hitVehicleIds)
    {

    }

    public void OnVehicleSpawned(int vehicleId, FactionCountry vehicleFaction, PlayerClass vehicleClass, GameObject vehicleObject, int ownerPlayerId)
    {

    }

    public void OnVehicleHurt(int vehicleId, byte oldHp, byte newHp, EntityHealthChangedReason reason)
    {

    }

    public void OnPlayerKilledVehicle(int killerPlayerId, int victimVehicleId, EntityHealthChangedReason reason, string details)
    {

    }

    public void OnShipSpawned(int shipId, GameObject shipObject, FactionCountry shipfaction, ShipType shipType, int shipNameId)
    {

    }

    public void OnShipDamaged(int shipId, int oldHp, int newHp)
    {

    }

    public void OnAdminPlayerAction(int playerId, int adminId, ServerAdminAction action, string reason)
    {

    }

    public void OnRCLogin(int playerId, string inputPassword, bool isLoggedIn)
    {

    }

    public void OnRCCommand(int playerId, string input, string output, bool success)
    {

    }

    public void OnPlayerPacket(int playerId, byte? instance, Vector3? ownerPosition, double? packetTimestamp, Vector2? ownerInputAxis, float? ownerRotationY, float? ownerPitch, float? ownerYaw, PlayerActions[] actionCollection, Vector3? cameraPosition, Vector3? cameraForward, ushort? shipID, bool swimming)
    {

    }

    public void OnVehiclePacket(int vehicleId, Vector2 inputAxis, bool shift, bool strafe, PlayerVehicleActions[] actionCollection)
    {

    }

    public void OnOfficerOrderStart(int officerPlayerId, HighCommandOrderType highCommandOrderType, Vector3 orderPosition, float orderRotationY, int voicePhraseRandomIndex)
    {

    }

    public void OnOfficerOrderStop(int officerPlayerId, HighCommandOrderType highCommandOrderType)
    {

    }
}
