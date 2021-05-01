﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterData
{
    public enum Character{
        CHARACTER1,
        CHARACTER2,
        CHARACTER3,
        NPC
    }

    public static Character getCharacter(string characterName){
        switch(characterName){
            case "Character 1":
                return Character.CHARACTER1;
            case "Character 2":
                return Character.CHARACTER2;
            case "Character 3":
                return Character.CHARACTER3;
            case "NPC":
                return Character.NPC;
            default:
                Debug.Log("Could Not Find Character Name: " + characterName);
                return Character.NPC;
        }
    }

    public static string CharacterPlateColor(string characterName){
        switch(getCharacter(characterName)){
            case Character.CHARACTER1:
                return "#ff0000";
            case Character.CHARACTER2:
                return "#00ff00";
            case Character.CHARACTER3:
                return "#0000ff";
            case Character.NPC:
                return "#aaaaaa";
            default:
                return "#aaaaaa";
        }
    }
}
