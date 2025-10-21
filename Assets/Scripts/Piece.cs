using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Piece : MonoBehaviour
{
    public static Piece Instance { get; private set; }
    
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }

    public float stepDelay = 1f;
    public float lockDelay = 0.5f;
    
    [Header("Level Speed Settings")]
    [SerializeField] private float baseStepDelay = 1f;
    [SerializeField] private float minStepDelay = 0.05f;
    [SerializeField] private float speedIncreasePerLevel = 0.15f;

    private float stepTime;
    private float lockTime;

    [SerializeField] private float moveRepeatDelay = 0.15f;
    [SerializeField] private float moveRepeatInterval = 0.05f;

    private float leftNextRepeatTime;
    private float rightNextRepeatTime;
    private float downNextRepeatTime;

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + stepDelay;
        this.lockTime = 0f;
        
        if (ScoreManager.Instance != null)
        {
            UpdateSpeedForLevel(ScoreManager.Instance.level);
        }

        if (this.cells == null)
        {
            this.cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
        {
            return;
        }

        if (this.board == null)
        {
            return;
        }
        
        this.board.Clear(this);

        this.lockTime += Time.deltaTime;



        if (Keyboard.current == null)
        {
            return;
        }

        bool leftPressedThisFrame = Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame;
        bool leftIsPressed = Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed;
        HandleRepeat(leftPressedThisFrame, leftIsPressed, ref leftNextRepeatTime, Vector2Int.left);

        bool rightPressedThisFrame = Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame;
        bool rightIsPressed = Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed;
        HandleRepeat(rightPressedThisFrame, rightIsPressed, ref rightNextRepeatTime, Vector2Int.right);

        bool downPressedThisFrame = Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame;
        bool downIsPressed = Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed;
        HandleRepeat(downPressedThisFrame, downIsPressed, ref downNextRepeatTime, Vector2Int.down);

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            HardDrop();
        }

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            Rotate(-1);
        }
        else if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Rotate(1);
        }

        if (Time.time >= this.stepTime)
        {
            Step();
        }

        this.board.Set(this);
    }
    private void Step()
    {
        this.stepTime = Time.time + stepDelay;

        Move(Vector2Int.down);

        if (this.lockTime >= lockDelay)
        {
            Lock();
        }
        
    }

    private void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }

        Lock();
    }

    private void Lock()
    {
        this.board.Set(this);
        this.board.ClearLines();
        this.board.SpawnPiece();
    }

    private bool Move(Vector2Int translation)
    {
        this.board.Clear(this);

        Vector3Int newPosition = this.position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = this.board.IsValidPosition(this, newPosition);

        if (valid)
        {
            this.position = newPosition;
            this.lockTime = 0f; 
        }

        this.board.Set(this);

        return valid;
    }

    private void Rotate(int direction)
    {
        int originalRotation = this.rotationIndex;
        Vector3Int[] originalCells = (Vector3Int[])this.cells.Clone();
        this.board.Clear(this);

        this.rotationIndex = Wrap(this.rotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction);

        if (!TestWallKicks(this.rotationIndex, direction))
        {
            this.rotationIndex = originalRotation;
            this.cells = originalCells;
        }
        this.board.Set(this);
    }

    private void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3 cell = this.cells[i];

            int x, y;

            switch (this.data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
            }

            this.cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < this.data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = this.data.wallKicks[wallKickIndex, i];
            Vector3Int testPosition = this.position + new Vector3Int(translation.x, translation.y, 0);
            if (this.board.IsValidPosition(this, testPosition))
            {
                this.position = testPosition;
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, this.data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }

    private void HandleRepeat(bool pressedThisFrame, bool isPressed, ref float nextRepeatTime, Vector2Int direction)
    {
        if (pressedThisFrame)
        {
            Move(direction);
            nextRepeatTime = Time.time + moveRepeatDelay;
            return;
        }

        if (isPressed)
        {
            if (Time.time >= nextRepeatTime)
            {
                Move(direction);
                nextRepeatTime = Time.time + moveRepeatInterval;
            }
        }
        else
        {
            nextRepeatTime = 0f;
        }
    }

    public void UpdateSpeedForLevel(int level)
    {
        float newStepDelay = baseStepDelay - (speedIncreasePerLevel * (level - 1));
        stepDelay = Mathf.Max(newStepDelay, minStepDelay);
        
        this.stepTime = Time.time + stepDelay;
    }
}
