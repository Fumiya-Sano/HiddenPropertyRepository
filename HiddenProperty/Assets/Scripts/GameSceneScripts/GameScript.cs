using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameScript : MonoBehaviour
{
    // タイルの種類
    private enum TileType
    {
        NONE,  // 何もない
        GROUND,  //  地面
    }

    // 財産の種類
    private enum MoneyType
    {
        Money_1 = 1,
        Money_2 = 2,
        Money_3 = 3,
        Money_4 = 4,
        Money_5 = 5,
        Money_6 = 6,
        Money_7 = 7,
        Money_8 = 8,
    }

    // 方向の種類
    private enum ActionType
    {
        UP,  // 上
        RIGHT,  //　右
        DOWN, 　// 下
        LEFT,  // 左
        ATTACH_MONEY,  // 財産をつける
    }

    private int playerNum = TurnPlayerManagerScript.getTotalPlayers();  // プレイヤーの人数
    public TextAsset stageFile;  // ステージ構造が記述されたテキストファイル
    public GameObject playerPrefab;
    public GameObject canvas;
    
    private int rows; // 行数
    private int columns;  // 列数
    private TileType[,] tileList;  // タイル情報を管理する二次元配列

    public float tileSize;  // タイルのサイズ

    public Sprite groundSprite;  // 地面のスプライト
    public Sprite[] playerSprite;  // プレイヤー1~8のスプライト配列
    public Sprite[] MoneySprite;  // 財産に感染した地面のスプライト配列

    private GameObject[] playerlist = new GameObject [8];  // プレイヤーのゲームオブジェクト
    public static List<Vector2Int> OnlyPositionList = new List<Vector2Int>();
    private List<ActionType> actionHistoryList = new List<ActionType>();  // ActionTypeでプレイヤーの行動履歴を残すリスト
    private Dictionary<Vector2Int, int> OverWrittenPropertyTable = new Dictionary<Vector2Int, int>();  // このターンに上書きされた財産の履歴を残す連想配列
    private Vector2 middleOffset;  // 中心位置

    public static int IsInitial;  // このターンがゲームを開始して、初めてのターンかどうか
    public GameObject turnPlayer;  // このターンのプレイヤーオブジェクト

    public int countMove;  // このターンに動いた回数
    public int countAttachMoney;  // このターンに隠した財産の数
    private int IsSuccessAction;  // 行おうとしたアクションが達成されたかどうか


    // 各位置に存在する財産を管理する連想配列
    
    public static Dictionary<Vector2Int, int> posMoneyTable = new Dictionary<Vector2Int, int>();

    // ゲームプレイヤーの位置を管理する連想配列
    public Dictionary<GameObject, Vector2Int> playerPosTable = new Dictionary<GameObject, Vector2Int>();

    // 見えている財産の位置を管理する連想配列
    private Dictionary<Vector2Int, GameObject> posMoneyObjectTable = new Dictionary<Vector2Int, GameObject>();




    // Start is called before the first frame update
    void Start()
    {
        LoadTileData();
        CreateStage();
        LoadMyMoney();
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
        countAttachMoney = 0;
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
        if(1 <= countMove && countMove <= 3 && countAttachMoney == 0)  // 一歩でも歩いていたら
        {
            Vector2Int turnPlayerPos;
            IsSuccessAction = 0;
            int IsPreAttached = 0;
            // すでに自分が置いた場所には置けないようにする
            foreach(var pair in posMoneyTable)
            {
                if(playerPosTable[turnPlayer] == pair.Key && pair.Value == TurnPlayerManagerScript.getTurnPlayerNum())
                {
                    IsPreAttached = 1;
                }
            }
            if(IsPreAttached == 0)
            {
                turnPlayerPos = playerPosTable[turnPlayer];
                AttachMoney(turnPlayerPos, TurnPlayerManagerScript.getTurnPlayerNum());
                countAttachMoney += 1;
            }
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
            if(lastAction == ActionType.ATTACH_MONEY)
            {
                // 隠した財産を取り消す
                // 他のプレイヤーの財産があったら元に戻す
                posMoneyTable.Remove(playerPosTable[turnPlayer]);
                Destroy(posMoneyObjectTable[playerPosTable[turnPlayer]]);
                posMoneyObjectTable.Remove(playerPosTable[turnPlayer]);
                int IsOverWritten = 0;
                foreach(var pair in OverWrittenPropertyTable)
                {
                    if(pair.Key == playerPosTable[turnPlayer])
                    {
                        // 上書きされたプレイヤーの財産を元に戻す
                        posMoneyTable.Add(pair.Key, pair.Value);
                        IsOverWritten = 1;
                    }
                }
                if(IsOverWritten == 1)
                {
                    // 履歴リストから元に戻した財産の履歴を削除
                    OverWrittenPropertyTable.Remove(playerPosTable[turnPlayer]);
                }
                countAttachMoney -= 1;
                actionHistoryList.RemoveAt(NumActionList - 1);
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

    // 自分の財産のタイルをロードする
    private void LoadMyMoney()
    {
        int i = 1;
        foreach(var pair in posMoneyTable)
        {
            if(pair.Value == TurnPlayerManagerScript.getTurnPlayerNum())
            {
                GameObject myMoney = new GameObject("myMoney_" + i);
                posMoneyObjectTable.Add(new Vector2Int(pair.Key.x, pair.Key.y), myMoney);
                var sr = myMoney.AddComponent<SpriteRenderer>();
                sr.sprite = MoneySprite[TurnPlayerManagerScript.getTurnPlayerNum() - 1];
                sr.sortingOrder = 3;
                myMoney.transform.position = GetDisplayPosition(pair.Key.x, pair.Key.y);
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
            GameObject tmpPlayer = Instantiate(playerPrefab) as GameObject;

            tmpPlayer.transform.SetParent(canvas.transform, false);

            // GameObject canvas = tmpPlayer.transform.Find("Canvas").gameObject;

            GameObject playerTag = tmpPlayer.transform.Find("PlayerTag").gameObject;

            Text playerTagText = playerTag.GetComponent<Text>();

            var rectTransform = playerTag.GetComponent<RectTransform>();

            var playerNames =  StartButtonScript.getPlayerDict();

            playerTagText.text = playerNames[i];

            var name = "player" + i;

            tmpPlayer.name = name;

            playerlist[i - 1] = tmpPlayer;

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

            rectTransform.localPosition = GetNameTagDisplayPosition(x, y);

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
            GameObject tmpPlayer = Instantiate(playerPrefab) as GameObject;

            tmpPlayer.transform.SetParent(canvas.transform, false);

            // GameObject canvas = tmpPlayer.transform.Find("Canvas").gameObject;

            GameObject playerTag = tmpPlayer.transform.Find("PlayerTag").gameObject;

            playerTag.transform.localPosition = new Vector3(0, 5, 0);

            Text playerTagText = playerTag.GetComponent<Text>();

            var playerNames =  StartButtonScript.getPlayerDict();

            playerTagText.text = playerNames[i];

            var name = "player" + i;

            tmpPlayer.name = name;

            playerlist[i - 1] = tmpPlayer;

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

    private Vector3 GetNameTagDisplayPosition(int x, int y)
    {
        return new Vector3
        (
            x * tileSize - middleOffset.x,
            y * -tileSize + middleOffset.y + 0.5f,
            0
        );
    }
    /*
    // 指定された位置に存在する財産を返します
    private GameObject GetMoneyAtPosition(Vector2Int pos)
    {
        foreach (var pair in MoneyPosTable)
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
    // 指定された位置のタイルが財産に感染しているならtrueを返す
    private bool IsMoney(Vector2Int pos)
    {
        var cell = tileList[pos.x, pos.y];
        return (cell == TileType.Money_1) || (cell == TileType.Money_2) || (cell == TileType.Money_3) || (cell == TileType.Money_4);
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

    // 指定した場所に指定した人の財産をつける関数 他の財産がある場合は上書きする
    private void AttachMoney(Vector2Int pos, int playerNum)
    {
        int IsSamePosMoney = 0;
        GameObject attachedMoney;
        foreach(var pair in posMoneyTable)
        {
            if(pair.Key == pos)
            {
                IsSamePosMoney = 1;
            }
        }
        if(IsSamePosMoney == 0)
        {
            // 初めて財産が置かれる場合
            posMoneyTable.Add(pos, playerNum);
            attachedMoney = new GameObject("attachedMoney");
            posMoneyObjectTable.Add(pos, attachedMoney);
            var sr = attachedMoney.AddComponent<SpriteRenderer>();
            sr.sprite = MoneySprite[playerNum - 1];
            sr.sortingOrder = 3;
            attachedMoney.transform.position = GetDisplayPosition(pos.x, pos.y);
            Debug.Log(pos);
            
        }
        else
        {
            // もともと財産が置かれていた場合
            OverWrittenPropertyTable.Add(pos, posMoneyTable[pos]);  // もともと置いていた財産の持ち主の情報を履歴に残す
            posMoneyTable[pos] = playerNum;
            attachedMoney = new GameObject("attachedMoney");
            posMoneyObjectTable.Add(pos, attachedMoney);
            var sr = attachedMoney.AddComponent<SpriteRenderer>();
            sr.sprite = MoneySprite[playerNum - 1];
            sr.sortingOrder = 3;
            attachedMoney.transform.position = GetDisplayPosition(pos.x, pos.y);
        }
        actionHistoryList.Add(ActionType.ATTACH_MONEY);
    }
}
