﻿using Photon.Pun;
using System.Text;
using UnityEngine;

public enum ContuEventCode
{
    GameAction = 100,
    Chat = 110,
    ChatSoundMessage = 111,
    LoadScene = 90
}

public struct ContuActionData
{
    public int UserId;
    public ActionType Action;
    public int[] Parameters;

    public ContuActionData(int userId, ActionType action, params int[] parameters)
    {
        this.UserId = userId;
        this.Action = action;
        this.Parameters = parameters;
    }


    public byte[] ToByteArray()
    {
        byte[] data = new byte[7];
        data[0] = (byte)UserId;
        data[1] = (byte)Action;

        for (int i = 0; i < Parameters.Length; i++)
        {
            data[i + 2] = (byte)Parameters[i];
        }

        return data;
    }

    public static ContuActionData FromByteArray(byte[] data)
    {
        int userId = data[0];
        ActionType actionType = (ActionType)data[1];
        int[] parameters = new int[5];

        for (int i = 0; i < parameters.Length; i++)
        {
            parameters[i] = data[i + 2];
        }

        return new ContuActionData(userId, actionType, parameters);
    }

    public string ToByteString()
    {
        return System.BitConverter.ToString(ToByteArray()).Replace("-", "");
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(UserId + ": " + Action.ToString());

        foreach (var item in Parameters)
        {
            sb.Append(" " + item);
        }

        return sb.ToString();
    }

}
