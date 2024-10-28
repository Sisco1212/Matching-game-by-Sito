using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionBoard : MonoBehaviour
{
    //define the size of the board
    public int width = 6;
    public int height = 8;

    //define some spacing for the board
    public float spacingX;
    public float spacingY;

    //get a reference to our potion prefabs
    public GameObject[] potionPrefabs;

    // get a reference to the collection of nodes "potionboard" + GameObject
    private Node[,] potionBoard;
    public GameObject potionBoardGO;

    public List<GameObject> potionsToDestroy = new();
    public GameObject potionParent;
    public GameObject superText;

    [SerializeField]
    private Potion selectedPotion;

    [SerializeField]
    private bool isProcessingMove;

    [SerializeField]
     List<Potion> potionsToRemove = new();

    //layout array
    public ArrayLayout arrayLayout;

    //public static of PotionBoard
    public static PotionBoard Instance;

    
    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializeBoard();
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        
        if(hit.collider != null && hit.collider.gameObject.GetComponent<Potion>())
        {
            if(isProcessingMove)
            return;
            
            Potion potion = hit.collider.gameObject.GetComponent<Potion>();
            Debug.Log("I have clicked a potion it is: " + potion.gameObject);

            SelectPotion(potion);
        }
        }
    }

    void InitializeBoard()
    {
        DestroyPotions();
        potionBoard = new Node[width, height];

        spacingX = (float)(width - 1) / 2;
        spacingY = ((float)(height - 1) / 2) + 2;

        for(int y = 0; y < height; y++)
        {

            for(int x = 0; x < width; x++)
            {
            Vector2 position = new Vector2(x - spacingX, y - spacingY);
            if(arrayLayout.rows[y].row[x])
            potionBoard[x, y] = new Node(false, null);

            else {
            int randomIndex = Random.Range(0, potionPrefabs.Length);

            GameObject potion = Instantiate(potionPrefabs[randomIndex], position, Quaternion.identity);
            potion.transform.SetParent(potionParent.transform);
            potion.GetComponent<Potion>().SetIndicies(x, y);
            potionBoard[x, y] = new Node(true, potion);
            potionsToDestroy.Add(potion);
            }
        }
        }
       if(CheckBoard())
       {
        Debug.Log("We have matches let's re-create the board");
        InitializeBoard();
       }
       else {
        Debug.Log("There are no matches, it's time to start the game!");
       }
    }

    public bool CheckBoard()
    {
        if(GameManager.Instance.isGameEnded)
        return false;
        Debug.Log("Checking Board");
        bool hasMatched = false;

        potionsToRemove.Clear();

         foreach(Node nodePotion in potionBoard)
         {
            if(nodePotion.potion != null)
            {
            nodePotion.potion.GetComponent<Potion>().isMatched = false;
            }
         }

        for(int x = 0; x < width; x++)
        {
            for( int y = 0; y < height; y++)
            {
            //checking if potion node is usable
            if(potionBoard[x, y].isUseable)
            {
             //then proceed to get potion class in node
             Potion potion = potionBoard[x, y].potion.GetComponent<Potion>();
            
            //ensure its not matched
            if(!potion.isMatched)
            {
                // run some matching logic
            MatchResult matchedPotions = IsConnected(potion);

            if(matchedPotions.connectedPotions.Count >= 3)
            {

            MatchResult superMatchPotions = SuperMatch(matchedPotions);
                potionsToRemove.AddRange(superMatchPotions.connectedPotions);
                foreach(Potion pot in superMatchPotions.connectedPotions)
                pot.isMatched = true;

                hasMatched = true;
            } 
            }
            }
            }
        }
     return hasMatched;
    }

    public IEnumerator ProcessTurnOnMatchedBoard(bool _subtractMoves)
    {
 foreach(Potion potionToRemove in potionsToRemove)
        {
            potionToRemove.isMatched = false;
        }
        RemoveAndRefill(potionsToRemove);
        GameManager.Instance.ProcessTurn(potionsToRemove.Count, _subtractMoves, potionsToRemove.Count - 2);
        yield return new WaitForSeconds(0.4f);

        if(CheckBoard())
        {
        StartCoroutine(ProcessTurnOnMatchedBoard(false));
        }
    }

    private void RemoveAndRefill(List<Potion> _potionsToRemove)
    {
        //Removing the potion and clearing the board at that location
         foreach(Potion potion in _potionsToRemove)
         {
            //getting it's x and y indicies and storing them
            int _xIndex = potion.xIndex;
            int _yIndex = potion.yIndex;

            //Destroy the potion
            Destroy(potion.gameObject);

            //Create a blank node on the potion board.
            potionBoard[_xIndex, _yIndex] = new Node(true, null);
         }
         for(int x = 0; x < width; x++)
         {
            for(int y = 0; y < height; y++)
            {
            if(potionBoard[x, y].potion == null)
            {
            Debug.Log("The location X: " + x + " Y: " + y + " is empty, attempting to refill it.");
            RefillPotion(x, y);
            }
            }
         }
    }

    
    private void RefillPotion(int x, int y)
    {
        //y offset
        int yOffset = 1;

        //while the cell above our current cell is null and we're below the height of the board
        while(y + yOffset < height && potionBoard[x, y + yOffset].potion == null)
        {
        //Increment y offset
        Debug.Log("The potion above me is null, but I'm not at the top of the board yet, so add to my yOffset and try again. Current Offset is: " + yOffset + " I'm about to add 1.");
        yOffset++;
        }

    //We've either hit the top of the board or we found a potion
    if(y + yOffset < height && potionBoard[x, y + yOffset].potion != null)
    {
    //We've found a potion

    Potion potionAbove = potionBoard[x, y + yOffset].potion.GetComponent<Potion>();

    //Move it to the correct location
    Vector3 targetPos = new Vector3(x - spacingX, y - spacingY, potionAbove.transform.position.z);
    Debug.Log("I've found a potion when refilling the board and it was in the location: [" + x + "," + (y + yOffset) + "] we have moved it to the location: [" + x + "," + y + "]");
    //Move to location
    potionAbove.MoveToTarget(targetPos);
    //update indicies
    potionAbove.SetIndicies(x, y); 
    //update our potionboard
    potionBoard[x, y] = potionBoard[x, y + yOffset];
    //set the location the potion came from to null
    potionBoard[x, y + yOffset] = new Node(true, null);
    
    }
    if(y + yOffset == height)
    {
        Debug.Log("I've reached the top of the board without finding a potion");
        SpawnPotionAtTop(x);
    }
}

  private void SpawnPotionAtTop(int x)
  {
    int index = FindIndexOfLowestNull(x);
    int locationToMoveTo = 8 - index;
    Debug.Log("About to spawn a potion, ideally I'd like to put it in the index of: " + index);
    //get a random potion
    int randomIndex = Random.Range(0, potionPrefabs.Length);
    GameObject newPotion = Instantiate(potionPrefabs[randomIndex], new Vector2(x - spacingX, height - spacingY), Quaternion.identity);
    newPotion.transform.SetParent(potionParent.transform);
    //set indicies
    newPotion.GetComponent<Potion>().SetIndicies(x, index);
    //set it on the potin borad
    potionBoard[x, index] = new Node(true, newPotion);
    //move it to that location
    Vector3 targetPosition = new Vector3(newPotion.transform.position.x, newPotion.transform.position.y - locationToMoveTo, newPotion.transform.position.x);
    newPotion.GetComponent<Potion>().MoveToTarget(targetPosition);
    }

    private int FindIndexOfLowestNull(int x)
    {
        int lowestNull = 99;
        for(int y = 7; y >= 0; y--)
        {
            if(potionBoard[x, y].potion == null)
            {
                lowestNull = y;
            }
        }
        return lowestNull;
    }

    #region Cascading Potions
    //FindIndexOfLowestNull
    #endregion

    private MatchResult SuperMatch(MatchResult _matchedResults)
    {
        //if we have a horizontal or long horizontal mathc
        //loop through the potions in my match
        //create a new list of potions "extra matches"
        //checkDirection go up
        //checkDirection go down
        //do we have 2 or more extra matches.
        //we've made a super match - return a new matchresult of type super
        //return extra matches

        if(_matchedResults.direction == MatchDirection.Horizontal || _matchedResults.direction == MatchDirection.LongHorizontal)
        {
            foreach(Potion pot in _matchedResults.connectedPotions)
            {
            List<Potion> extraConnectedPotions = new();
            CheckDirection(pot, new Vector2Int(0, 1), extraConnectedPotions);
            CheckDirection(pot, new Vector2Int(0, -1), extraConnectedPotions);
            
            if(extraConnectedPotions.Count >= 2)
            {
                superText.SetActive(true);
                StartCoroutine(DisableSuperText());
                Debug.Log("I have a super Horizontal match");
                extraConnectedPotions.AddRange(_matchedResults.connectedPotions);

                return new MatchResult
                {
                    connectedPotions = extraConnectedPotions,
                    direction = MatchDirection.Super
                };
            }
            }
    return new MatchResult
    {
        connectedPotions = _matchedResults.connectedPotions,
        direction = _matchedResults.direction
    };
        }

        //if we have a vertical or long vertical mathc
        //loop through the potions in my match
        //create a new list of potions "extra matches"
        //checkDirection go up
        //checkDirection go down
        //do we have 2 or more extra matches.
        //we've made a super match - return a new matchresult of type super
        //return extra matches

    else if(_matchedResults.direction == MatchDirection.Vertical || _matchedResults.direction == MatchDirection.LongVertical)
        {
            foreach(Potion pot in _matchedResults.connectedPotions)
            {
            List<Potion> extraConnectedPotions = new();
            CheckDirection(pot, new Vector2Int(1, 0), extraConnectedPotions);
            CheckDirection(pot, new Vector2Int(-1, 0), extraConnectedPotions);
            
            if(extraConnectedPotions.Count >= 2)
            {
                superText.SetActive(true);
                StartCoroutine(DisableSuperText());
                Debug.Log("I have a super Vertical match");
                extraConnectedPotions.AddRange(_matchedResults.connectedPotions);

                return new MatchResult
                {
                    connectedPotions = extraConnectedPotions,
                    direction = MatchDirection.Super
                };
            }
            }
    return new MatchResult
    {
        connectedPotions = _matchedResults.connectedPotions,
        direction = _matchedResults.direction
    };
        }

    return null;
    }

    private IEnumerator DisableSuperText() 
    {
        yield return new WaitForSeconds(1.0f);
        superText.SetActive(false);
    }

    private void DestroyPotions()
    {
        if(potionsToDestroy != null)
        {
            foreach(GameObject potion in potionsToDestroy)
            {
                Destroy(potion);
            }
            potionsToDestroy.Clear();
        }
    }

    //IsConnected
    MatchResult IsConnected(Potion potion)
    {
        List<Potion> connectedPotions = new();
        PotionType potionType = potion.potionType;

        connectedPotions.Add(potion);

        //check right
        CheckDirection(potion, new Vector2Int(1, 0), connectedPotions);

        //check left
        CheckDirection(potion, new Vector2Int(-1, 0), connectedPotions);
        // have we made a 3 match? (Horizontal Match)
        if(connectedPotions.Count == 3)
        {
            Debug.Log("I have a normal horizontal match, the color of my match is: " + connectedPotions[0].potionType);

            return new MatchResult 
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Horizontal
            };

        }
        //Checking for more than 3 (Long horizontal match)
        else if (connectedPotions.Count > 3)
        {
            Debug.Log("I have a Long horizontal match, the color of my match is: " + connectedPotions[0].potionType);

            return new MatchResult 
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongHorizontal
            };

        
        }
        //clearout the connectedpotions
        connectedPotions.Clear();
        //readd our inital potions
        connectedPotions.Add(potion);
        //check up
        CheckDirection(potion, new Vector2Int(0, 1), connectedPotions);
        //check down
        CheckDirection(potion, new Vector2Int(0, -1), connectedPotions);
        // have we made a 3 match? (Vertical Match)
        if(connectedPotions.Count == 3)
        {
            Debug.Log("I have a normal vertical match, the color of my match is: " + connectedPotions[0].potionType);

            return new MatchResult 
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.Vertical
            };

        }
        //Checking for more than 3 (Long vertical match)
       else if(connectedPotions.Count > 3)
        {
            Debug.Log("I have a Long Vertical match, the color of my match is: " + connectedPotions[0].potionType);

            return new MatchResult 
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.LongVertical
            };

        }

        else 
        {
            return new MatchResult 
            {
                connectedPotions = connectedPotions,
                direction = MatchDirection.None
            };
        }


    }


    //CheckDirection
    void CheckDirection(Potion pot, Vector2Int direction, List<Potion> connectedPotions)
    {
        PotionType potionType = pot.potionType;
        int x = pot.xIndex + direction.x;
        int y = pot.yIndex + direction.y;

        //check that we're within the boundaries of the board
        while(x >= 0 && x < width && y >= 0 && y < height)
        {
            if(potionBoard[x, y].isUseable)
            {
            Potion neighbourPotion = potionBoard[x, y].potion.GetComponent<Potion>();
            // Does our potion type match? it must also not be matched
            if(!neighbourPotion.isMatched && neighbourPotion.potionType == potionType)
            {
                connectedPotions.Add(neighbourPotion);

                x += direction.x;
                y += direction.y;
            }
            else
                break;
            
            }
            else 
                break;
            
        }
    }

    #region Swapping Potions
    //select potion
    public void SelectPotion(Potion _potion)
    {
        //if we don't have a potion currently selected, then set the potion i just clicked to my selectedoption
    if(selectedPotion == null)
    {
        Debug.Log(_potion);
        selectedPotion = _potion;
    }
        //if we select the same potion twice, then let's make selectedpotion null
    else if(selectedPotion == _potion)
    {
        selectedPotion = null;
    }
        //if selectedpotion is not null and is not the current position, attempt a swap
        //selectedpotion back to null
    else if(selectedPotion != _potion)
    {
        SwapPotion(selectedPotion, _potion);
        selectedPotion = null;
    }
    }

    //swap potion - logic

    private void SwapPotion(Potion _currentPotion, Potion _targetPotion)
    {
        //!IsAdjacent don't do anything
        if(!IsAdjacent(_currentPotion, _targetPotion))
        {
            return;
        }

        //DoSwap
        DoSwap(_currentPotion, _targetPotion);
        isProcessingMove = true;

        StartCoroutine(ProcessMatches(_currentPotion, _targetPotion));
    }
    //do swap
    private void DoSwap(Potion _currentPotion, Potion _targetPotion)
    {
        GameObject temp = potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion;

        potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion = potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion;
        potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion = temp;

        //update indices.
        int tempXIndex = _currentPotion.xIndex;
        int tempYIndex = _currentPotion.yIndex;
        _currentPotion.xIndex = _targetPotion.xIndex;
        _currentPotion.yIndex = _targetPotion.yIndex;
        _targetPotion.xIndex = tempXIndex;
        _targetPotion.yIndex = tempYIndex;

        _currentPotion.MoveToTarget(potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion.transform.position);
        _targetPotion.MoveToTarget(potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion.transform.position);
    }

    private IEnumerator ProcessMatches(Potion _currentPotion, Potion _targetPotion)
    {
        yield return new WaitForSeconds(0.2f);

        if(CheckBoard())
        {
        
        //Start a coroutine that is going to process our matches in out turn.
        StartCoroutine(ProcessTurnOnMatchedBoard(true));
        }
        else
        {
            DoSwap(_currentPotion, _targetPotion);
        }
        isProcessingMove = false;
    }

    //IsAdjacent
    private bool IsAdjacent(Potion _currentPotion, Potion _targetPotion)
    {
        return Mathf.Abs(_currentPotion.xIndex - _targetPotion.xIndex) + Mathf.Abs(_currentPotion.yIndex - _targetPotion.yIndex) == 1;
    }

    //ProcessMatches
    #endregion

}


public class MatchResult 
{
    public List<Potion> connectedPotions;
    public MatchDirection direction;
}

public enum MatchDirection 
{
    Vertical,
    Horizontal,
    LongVertical,
    LongHorizontal,
    Super,
    None
}