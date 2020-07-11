﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(200)]
public class InputController3D : MonoBehaviour
{
    [SerializeField] GameObject gameHolder;
    [SerializeField] Camera camera;
    [SerializeField] ContuNetworkEventHandler eventHandler;
    [SerializeField] ContuConnectionHandler connectionHandler;

    [SerializeField] Transform[] freeLocations, p1Locations, p2Locations, p1ExLocations, p2ExLocations;

    ContuGame game;
    VisualToken[] visualTokens;

    InteractionState interactionState;

    VisualToken tokenSelected;
    Vector2Int tokenUse1, tokenUse2;

    private void Start()
    {
        game = gameHolder.GetComponent<IContuGameOwner>().GetGame();
        interactionState = InteractionState.Selecting;

        /*
        if(interactionStateText)
        interactionStateText.text = interactionState.ToString();
        */

        if(connectionHandler)
        connectionHandler.RoomJoined += OnRoomJoined;

        game.TurnChanged += OnTurnChanged;
    }

    private void OnGameStateChanged(BoardState obj)
    {
        //stateText.text = "State: " + game.Board.GetBoardState();
    }

    private void OnRoomJoined()
    {
        //idText.text = "You are Player " + (GetPlayerId()+1);
    }

    private void OnTurnChanged()
    {
        /*
        interactionState = InteractionState.Selecting;
        if(interactionStateText != null)
            interactionStateText.text = interactionState.ToString();

        if(turnText!=null)
        turnText.text = "Turn: " + game.Turn + " " + game.TurnState;
        */
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var mouseInput = Input.mousePosition;
            Ray r = camera.ScreenPointToRay(mouseInput);
            Plane plane = new Plane(Vector3.forward, Vector3.zero);

            if (plane.Raycast(r, out float dist))
            {
                var point = r.GetPoint(dist);
                int x = Mathf.FloorToInt(point.x);
                int y = Mathf.FloorToInt(point.y);

                if (game.Board.GetTile(x, y) != TileType.Null)
                {
                    PlayerClicked(x, y);
                }
            }
        }

    }

    private void UpdateTokenView()
    {
        Dictionary<TokenState, int> indexes = new Dictionary<TokenState, int>();
        indexes.Add(TokenState.Free, 0);
        indexes.Add(TokenState.P1Exausted, 0);
        indexes.Add(TokenState.P2Exausted, 0);
        indexes.Add(TokenState.P1Owned, 0);
        indexes.Add(TokenState.P2Owned, 0);

        foreach (var token in visualTokens)
        {
            var state = token.Source.State;
            token.transform.position = GetTokenLocation(state, indexes[state]);
            indexes[state] += 1;
        }
    }

    private Vector3 GetTokenLocation(TokenState state, int index)
    {
        switch (state)
        {
            case TokenState.Free:
                return freeLocations[index].position;
            case TokenState.P1Exausted:
                return p1ExLocations[index].position;
            case TokenState.P2Exausted:
                return p2ExLocations[index].position;
            case TokenState.P1Owned:
                return p1Locations[index].position;
            case TokenState.P2Owned:
                return p2Locations[index].position;
        }

        return Vector3.zero;
    }

    private void PlayerClicked(int x, int y)
    {
        switch (interactionState)
        {
            case InteractionState.Selecting:
                PlacePiece(x, y);
                break;

            case InteractionState.UsingTokenTarget1:
                tokenUse1 = new Vector2Int(x, y);
                interactionState = InteractionState.UsingTokenTarget2;
                //interactionStateText.text = interactionState.ToString();
                break;

            case InteractionState.UsingTokenTarget2:
                tokenUse2 = new Vector2Int(x, y);
                UseToken();
                break;
        }
    }

    private void InteractWithToken(VisualToken token)
    {
        if(GetPlayerId() != (int)game.TurnState)
        {
            Debug.Log("Not your turn");
            return;
        }

        switch (token.Source.State)
        {
            case TokenState.Free:
                TakeToken(token.Source.Type);
                break;

            case TokenState.P1Owned:
                if(GetPlayerId() == 0)
                    StartTokenUsage(token);
                break;

            case TokenState.P2Owned:
                if (GetPlayerId() == 1)
                    StartTokenUsage(token);
                break;

            case TokenState.P1Exausted:
                if (GetPlayerId() == 1)
                    TakeToken(token.Source.Type);
                break;
            case TokenState.P2Exausted:
                if (GetPlayerId() == 0)
                    TakeToken(token.Source.Type);
                break;
        }
    }

    private void StartTokenUsage(VisualToken token)
    {
        tokenSelected = token;
        interactionState = InteractionState.UsingTokenTarget1;
        //interactionStateText.text = interactionState.ToString();
        Debug.Log("Started using Token");
    }

    private void TakeToken(TokenType type)
    {
        var res = game.TryAction(GetPlayerId(), ActionType.TakeToken, true, false, (int)type);
        Debug.Log("Tried Taking a Token: " + res);
    }

    private void UseToken()
    {
        var res = game.TryAction(GetPlayerId(), ActionType.UseToken, true, false, (int)tokenSelected.Source.Type, tokenUse1.x, tokenUse1.y, tokenUse2.x, tokenUse2.y);
        ResetSelection();
        Debug.Log("Tried using Token: " +res);
    }

    private void ResetSelection()
    {
        interactionState = InteractionState.Selecting;
        //interactionStateText.text = interactionState.ToString();
        //highlight?
    }

    private void PlacePiece(int x, int y)
    {
        var res = game.TryAction(GetPlayerId(), ActionType.Place, true, false, x, y);
        Debug.Log("Tried Placing: " + res);
    }

    private int GetPlayerId()
    {
        return eventHandler.LocalPlayerId;
    }
    private void SpawnTokens()
    {
        /*
        visualTokens = new VisualToken[game.Board.TokenCount];

        for (int i = 0; i < game.Board.TokenCount; i++)
        {
            var t = game.Board.GetToken(i);
            visualTokens[i] = Instantiate(tokenPrefab).GetComponent<VisualToken>();
            visualTokens[i].Setup(t, InteractWithToken);
        }
        */
    }

    public enum InteractionState
    {
        Selecting,
        UsingTokenTarget1,
        UsingTokenTarget2
    }

}