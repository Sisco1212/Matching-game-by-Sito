using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
 // to determine whether the space can be filled with potions or not
    public bool isUseable;
    public GameObject potion;

    public Node(bool _isUseable, GameObject _potion)
    {
        isUseable = _isUseable;
        potion = _potion;
    }

}
