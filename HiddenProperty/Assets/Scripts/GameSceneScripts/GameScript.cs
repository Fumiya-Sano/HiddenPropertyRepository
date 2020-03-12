using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScript : MonoBehaviour
{
    // タイルの種類
    private enum TileType
    {
        NONE,  // 何もない
        GROUND,  //  地面
    }

    // ウイルスの種類
    private enum VirusType
    {
        VIRUS_1 = 1,
        VIRUS_2 = 2,
        VIRUS_3 = 3,
        VIRUS_4 = 4,
        VIRUS_5 = 5,
        VIRUS_6 = 6,
        VIRUS_7 = 7,
        VIRUS_8 = 8,
    }

    // 方向の種類
    private enum ActionType
    {
        UP,  // 上
        RIGHT,  //　右
        DOWN, 　// 下
        LEFT,  // 左
        ATTACH_VIRUS,  // ウイルスをつける
    }

    private int playerNum = TurnPlayerManagerScript.getTotalPlayers();  // プレイヤーの人数
    public TextAsset stageFile;  // ステージ構造が記述されたテキストファイル
    
    private int rows; // 行数
    private int columns;  // 列数
    private TileType[,] tileList;  // タイル情報を管理する二次元配列

    public float tileSize;  // タイルのサイズ

    public Sprite groundSprite;  // 地面のスプライト
    public Sprite[] playerSprite;  // プレイヤー1~8のスプライト配列
    public Sprite[] virusSprite;  // ウイルスに感染した地面のスプライト配列

    private GameObject[] playerlist = new GameObject [8];  // プレイヤーのゲームオブジェクト
    public static List<Vector2Int> OnlyPositionList = new List<Vector2Int>();
    private List<ActionType> actionHistoryList = new List<ActionType>();
    private Vector2 middleOffset;  // 中心位置

    public static int IsInitial;
    public GameObject turnPlayer;

    public int countMove;
    public int countAttachVirus;
    private int IsSuccessAction;


    // 各位置に存在するゲームオブジェクトを管理する連想配列
    
    public static Dictionary<Vector2Int, int> posVirusTable = new Dictionary<Vector2Int, int>();

    // ゲームプレイヤーの位置を管理する連想配列
    public Dictionary<GameObject, Vector2Int> playerPosTable = new Dictionary<GameObject, Vector2Int>();




    // Start is called before the first frame update
    void Start()
    {
        LoadTileData();
        CreateStage();
        LoadMyVirus();
        if(IsInitial == 1)
        {
            InitialCreatePlayerList();
        }
        else
        {
            Debug.Log("OK");
            CreatePlayerList();
        }
        // ターンプレイヤーの取得
        turnPlayer = playerlist[TurnPlayerManagerScript.getTurnPlayerNum() - 1];

        countMove = 0;
        countAttachVirus = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickUpButton()
    {
        if(countMove < 3)
        {   
            IsSuccessAction = 0;
            TryMovePlayer(ActionType.UP, turnPlayer);
            if(IsSuccessAction == 1)
            {
                // 行動履歴に行動を追加
                actionHistoryList.Add(ActionType.UP);
                countMove += 1;
            }
        }
    }

    public void OnClickRightButton()
    {
        if(countMove < 3)
        {
            IsSuccessAction = 0;
            TryMovePlayer(ActionType.RIGHT, turnPlayer);
            if(IsSuccessAction == 1)
            {
                actionHistoryList.Add(ActionType.RIGHT);
                countMove += 1;
            }
        }
    }

    public void OnClickDownButton()
    {
        if(countMove < 3)
        {
            IsSuccessAction = 0;
            TryMovePlayer(ActionType.DOWN, turnPlayer);
            if(IsSuccessAction == 1)
            {
                actionHistoryList.Add(ActionType.DOWN);
                countMove += 1;
            }
        }
    }

    public void OnClickLeftButton()
    {
        if(countMove < 3)
        {
            IsSuccessAction = 0;
            TryMovePlayer(ActionType.LEFT, turnPlayer);
            if(IsSuccessAction == 1)
            {
                actionHistoryList.Add(ActionType.LEFT);
                countMove += 1;
            }
        }
    }

    public void OnClickCenterButton()
    {
        if(1 <= countMove && countMove <= 3 && countAttachVirus == 0)  // 一歩でも歩いていたら
        {
            Vector2Int turnPlayerPos;
            turnPlayerPos = playerPosTable[turnPlayer];
            AttachVirus(turnPlayerPos, TurnPlayerManagerScript.getTurnPlayerNum());
            countAttachVirus += 1;
        }
    }

    public void OnClickOneStepBuckButton()
    {
        int NumActionList = actionHistoryList.Count;
        if(NumActionList == 0)
        {
            return;
        }
        else
        {
            var lastAction = actionHistoryList[NumActionList - 1];
            if(lastAction == ActionType.ATTACH_VIRUS)
            {
                return;
            }
            else
            {
                switch(lastAction)
                {
                    case ActionType.UP:
                        TryMovePlayer(ActionType.DOWN, turnPlayer);
                        actionHistoryList.RemoveAt(NumActionList - 1);
                        countMove -= 1;
                        break;
                    case ActionType.RIGHT:
                        TryMovePlayer(ActionType.LEFT, turnPlayer);
                        actionHistoryList.RemoveAt(NumActionList - 1);
                        countMove -= 1;
                        break;
                    case ActionType.DOWN:
                        TryMovePlayer(ActionType.UP, turnPlayer);
                        actionHistoryList.RemoveAt(NumActionList - 1);
                        countMove -= 1;
                        break;
                    case ActionType.LEFT:
                        TryMovePlayer(ActionType.RIGHT, turnPlayer);
                        actionHistoryList.RemoveAt(NumActionList - 1);
                        countMove -= 1;
                        break;
                    default:
                        break;
                }
            }
        }
    }


    // タイル情報を読み込む
    private void LoadTileData()
    {
        // タイル情報を1行ごとに分割
        var lines = stageFile.text.Split
        (
            new[] { '\r',  '\n'},  // \rか\nで区切って、配列化
            System.StringSplitOptions.RemoveEmptyEntries  // 要素がないところがなくす
        );

        // 1行目を、,で区切って配列化
        var nums = lines[0].Split(new[] {','});

        // タイルの列数と行数を保持
        rows = lines.Length;  // 行数
        columns = nums.Length;  // 列数

        // タイル情報をint型の２次元配列で保持
        tileList = new TileType[ columns, rows ];
        for (int y = 0; y < rows; y++)
        {
            // 1文字ずつ取得
            var st = lines[y];
            nums = st.Split(new[] { ',' });
            for (int x = 0; x < columns; x++)
            {
                // 読み込んだ文字を数値に変換して保持
                tileList[x, y] = ( TileType )int.Parse(nums[x]);
            }
        }  
    }

    // 自分のウイルスのタイルをロードする
    private void LoadMyVirus()
    {
        int i = 1;
        foreach(var pair in posVirusTable)
        {
            if(pair.Value == TurnPlayerManagerScript.getTurnPlayerNum())
            {
                GameObject myVirus = new GameObject("myVirus_" + i);
                var sr = myVirus.AddComponent<SpriteRenderer>();
                sr.sprite = virusSprite[TurnPlayerManagerScript.getTurnPlayerNum() - 1];
                sr.sortingOrder = 3;
                myVirus.transform.position = GetDisplayPosition(pair.Key.x, pair.Key.y);
                i += 1;
                Debug.Log("aaa" + pair.Key.x);
            }
        }
    }

    // プレイヤーリストの作成と配置
    private void InitialCreatePlayerList()
    {
        for(int i = 1; i <= playerNum; i++)
        {
            var name = "player" + i;

            playerlist[i - 1] = new GameObject(name);

            var sr = playerlist[i - 1].AddComponent<SpriteRenderer>();

            sr.sprite = playerSprite[i - 1];

            sr.sortingOrder = 4;
            int x = 0;
            int y = 0;
            int IsWithoutCover = 0;
            while(IsWithoutCover == 0)
            {
                x = Random.Range(0, columns);
                y = Random.Range(0, rows);

                int IsSamePair = 0;
                foreach(var pair in playerPosTable)
                {
                    if(pair.Value == new Vector2Int(x, y))
                    {
                        IsSamePair = 1;
                    }
                }
                if(IsSamePair == 0)
                {
                    IsWithoutCover = 1;
                }
            }
            playerlist[i - 1].transform.position = GetDisplayPosition(x, y);

            playerPosTable.Add(playerlist[i-1], new Vector2Int(x, y));

            OnlyPositionList.Add(new Vector2Int(x, y));
        }
        IsInitial = 0;
    }

    public void CreatePlayerList()
    {
        int i = 1;
        foreach(var pos in OnlyPositionList)
        {

            var name = "player" + i;

            playerlist[i - 1] = new GameObject(name);

            var sr = playerlist[i - 1].AddComponent<SpriteRenderer>();

            sr.sprite = playerSprite[i - 1];

            sr.sortingOrder = 4;
            
            playerlist[i - 1].transform.position = GetDisplayPosition(pos.x, pos.y);

            playerPosTable.Add(playerlist[i-1], new Vector2Int(pos.x, pos.y));
            i += 1;
        }
    }

    // ステージ作成
    private void CreateStage()
    {
        // ステージの中心位置を計算
        middleOffset.x = columns * tileSize * 0.5f - tileSize * 0.5f;
        middleOffset.y = rows * tileSize * 0.5f - tileSize * 0.5f;

        for(int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                var val = tileList[x, y];

                // 何もない場所は無視
                if(val == TileType.NONE) continue;

                // タイルの名前に行番号と列番号を付与
                var name = "tile" + y + "_" + x;

                // タイルのゲームオブジェクトを作成
                var tile = new GameObject(name);

                // タイルにスプライトを描画する機能を追加
                var sr = tile.AddComponent<SpriteRenderer>();

                // タイルのスプライトを設定
                sr.sprite = groundSprite;

                sr.sortingOrder = 2;

                // タイルの位置を設定
                tile.transform.position = GetDisplayPosition(x, y);
            }
        }
    }

    // 指定された行番号と列番号からスプライトの表示位置を計算して返す
    private Vector2 GetDisplayPosition(int x, int y)
    {
        return new Vector2
        (
        x * tileSize - middleOffset.x,
        y * -tileSize + middleOffset.y + 3
        );
    }
    /*
    // 指定された位置に存在するウイルスを返します
    private GameObject GetVirusAtPosition(Vector2Int pos)
    {
        foreach (var pair in virusPosTable)
        {
            // 指定された位置が見つかった場合
            if(pair.Value == pos)
            {
                // その位置の存在するゲームオブジェクトを返す
                return pair.Key;
            }
        }
        return null;
    }
    */

    // 指定された位置に存在するプレイヤーを返します
    private GameObject GetPlayerAtPosition(Vector2Int pos)
    {
        foreach(var pair in playerPosTable)
        {
            // 指定された位置が見つかった場合
            if(pair.Value == pos)
            {
                // その位置の存在するゲームオブジェクトを返す
                return pair.Key;
            }
        }
        return null;
    }

    // 指定された位置がステージ内でかつNONE以外ならtrueを返す
    private bool IsValidPosition(Vector2Int pos)
    {
        if(0 <= pos.x && pos.x < columns && 0 <= pos.y && pos.y < rows)
        {
            return tileList[pos.x, pos.y] != TileType.NONE;
        }
        return false;
    }
    
    // 指定された位置に他のプレイヤーがいるならtrueを返す
    public bool IsOtherPlayer(Vector2Int pos)
    {
        bool IsExist = false;
        foreach(var pair in playerPosTable)
        {
            if(pair.Value == pos)
            {
                IsExist = true;
            }
        }
        return IsExist;
    }
    
    /*
    // 指定された位置のタイルがウイルスに感染しているならtrueを返す
    private bool IsVirus(Vector2Int pos)
    {
        var cell = tileList[pos.x, pos.y];
        return (cell == TileType.VIRUS_1) || (cell == TileType.VIRUS_2) || (cell == TileType.VIRUS_3) || (cell == TileType.VIRUS_4);
    }
    */

    

    // 指定された方向にプレイヤーが移動できるか検証
    // 移動できる場合は移動する
    private void TryMovePlayer(ActionType direction, GameObject player)
    {
        // プレイヤーの現在地を取得
        var currentPlayerPos = playerPosTable[player];  // 任意のプレイヤーの現在地を取得

        // プレイヤーの移動先の位置を計算
        var nextPlayerPos = GetNextPositionAlong(currentPlayerPos, direction);

        // プレイヤーの移動先がステージ内ではない場合無視
        if(!IsValidPosition(nextPlayerPos)) return;

        // プレイヤーの移動先に他のプレイヤーがいる場合、無視
        if(IsOtherPlayer(nextPlayerPos)) return;

        // プレイヤーの移動
        player.transform.position = GetDisplayPosition(nextPlayerPos.x, nextPlayerPos.y);

        // プレイヤーの位置を更新
        playerPosTable[player] = nextPlayerPos;

        OnlyPositionList[TurnPlayerManagerScript.getTurnPlayerNum() - 1] = nextPlayerPos;

        IsSuccessAction = 1;

    }

    // 指定された方向の位置を返す
    private Vector2Int GetNextPositionAlong(Vector2Int pos, ActionType direction)
    {
        switch(direction)
        {
            // 上
            case ActionType.UP:
                pos.y -= 1;
                break;
            
            // 右
            case ActionType.RIGHT:
                pos.x += 1;
                break;
            
            //　下
            case ActionType.DOWN:
                pos.y += 1;
                break;

            // 左
            case ActionType.LEFT:
                pos.x -= 1;
                break;
        }
        return pos;
    }

    // 指定した場所に指定した人のウイルスをつける関数 他のウイルスがある場合は上書きする
    private void AttachVirus(Vector2Int pos, int playerNum)
    {
        int IsSamePosVirus = 0;
        GameObject attachedVirus;
        foreach(var pair in posVirusTable)
        {
            if(pair.Key == pos)
            {
                IsSamePosVirus = 1;
            }
        }
        if(IsSamePosVirus == 0)
        {
            posVirusTable.Add(pos, playerNum);
            attachedVirus = new GameObject("attachedVirus");
            var sr = attachedVirus.AddComponent<SpriteRenderer>();
            sr.sprite = virusSprite[playerNum - 1];
            sr.sortingOrder = 3;
             attachedVirus.transform.position = GetDisplayPosition(pos.x, pos.y);
             Debug.Log(pos);
            
        }
        else
        {
            posVirusTable[pos] = playerNum;
            attachedVirus = new GameObject("attachedVirus");
            var sr = attachedVirus.AddComponent<SpriteRenderer>();
            sr.sprite = virusSprite[playerNum - 1];
            sr.sortingOrder = 3;
            attachedVirus.transform.position = GetDisplayPosition(pos.x, pos.y);
        }
        actionHistoryList.Add(ActionType.ATTACH_VIRUS);
    }
}
