using RenownedGames.Apex.Serialization.Collections.Generic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData",menuName = "ScriptableObjects/LevelData")]
public class LevelData : ScriptableObject
{

    /// <summary>
    /// レベルマネージャーからロードするときに使う
    /// </summary>
    public string mapName;

    /// <summary>
    /// このマップデータが変更されてるか。真ならされてる
    /// 変更されてない場合セーブせずロードもしない
    /// レベルマネージャーに埋め込まれた初期値のまま
    /// 言うても永続変化の扉とかはフラグで管理するのでな
    /// じゃあisChangeが真の数字だけリストしてレべマネに持たせるか？
    /// こいつほんとに必要か？
    /// 今いるマップだけで良くね流石に
    /// </summary>
    public bool isChanged;






    /// <summary>
    /// 真偽値を保存して、真の物を無効化していく
    /// 生成時、最初にオブジェクトたちが自分の番号で問い合わせて無効である場合のアクションをしていく
    /// 鍵を開けたドアなど永続的に変わらないものについては別に管理しよう。というかそれはフラグでいい
    /// フラグ立ってるか確認していく
    /// 起動時は現在いるマップのこれだけかくにんする
    /// 破壊された時はオブジェクトからちゃんと申告する
    /// </summary>
    [Header("現在のマップのオブジェクトの状況")]
    [ES3Serializable]
    public bool[] disenableObjects;

    /// <summary>
    /// 真偽値を保存して、真の物を無効化していく
    /// 生成時、最初にエネミーが自分の番号で問い合わせて無効である場合のアクションをしていく
    /// ボスやレアエネミーなど永続的に変わらないものについては別に管理しよう。というかそれはフラグでいい
    /// フラグ立ってるか確認していく
    /// 起動時は現在いるマップのこれだけかくにんする
    /// 死んだ敵に関してはちゃんと死ぬときに自分の番号で申告する
    /// </summary>
    [Header("現在のマップの敵の状況")]
    [ES3Serializable]
    public bool[] disenableEnemy;


}
