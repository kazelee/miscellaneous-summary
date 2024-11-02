using System.Collections.Generic;
using System.Linq;
using DouCardPuzzoom.scripts.classes;
using DouCardPuzzoom.scripts.enums;
using DouCardPuzzoom.scripts.manager;
using DouCardPuzzoom.scripts.tools;
using Godot;
using Godot.Collections;

using SCG = System.Collections.Generic;

namespace DouCardPuzzoom.scripts.rule; 

public class RuleMahjong : IRule {
    // 实例化本类后，通过DataLoader获取数据
    public readonly Dictionary Data = DataLoader.RuleMahjongData;
    public readonly Dictionary TypeCards = DataLoader.RuleMahjongTypeCards;

    public SCG.Dictionary<string, int> PointWeightMap = new SCG.Dictionary<string, int>() {
        { "A", 0 }, { "2", 1 }, { "3", 2 }, { "4", 3 }, { "5", 4 }, { "6", 5 }, { "7", 6 }, { "8", 7 }, { "9", 8 },
        { "10", 9 }, { "J", 10 }, { "Q", 11 }, { "K", 12 }, { "BJ", 13 }, { "CJ", 14 }
    };

    public string[] PointNames = new[] { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "BJ", "CJ" };

    public List<int> RuleList2Int(List<string> ruleList) {
        return ruleList.Select(ruleStr => PointWeightMap[ruleStr]).ToList();
    }
    
    public int GetPointWeight(PointNums pointNums) {
        switch (pointNums) {
            case PointNums.A: 
            case PointNums.N2:
                return (int)pointNums - 11;
            case PointNums.CJ:
            case PointNums.BJ:
                return (int)pointNums;
            default:
                return (int)pointNums + 2;
        }
    }
    
    // GetAllComb 和 CanFollow 返回的列表必须 不包含 需要二次筛选的项目！
    // 3 - CJ: 0 - 14 (0 - 12, 13, 14)
    
    public List<CombData> GetAllComb(List<CardData> myCardsInHand) {
        // 正常流程：逐一算牌
        var cardsRule = RuleTool.CardList2RuleList(myCardsInHand);
        return GetAllCombOri(cardsRule);
    }

    // 编写一个针对RuleList的版本
    public List<CombData> GetAllCombOri(List<string> cardsRule) {
        // var soloList = new List<int>();
        var pairList = new List<int>();
        // var trioList = new List<int>();
        var bombList = new List<int>();

        var cardsInt = RuleList2Int(cardsRule); // 使用针对规则改写的
        var cardsMap = Int2CardMap(cardsInt);

        var combList = new List<CombData>(); // { new CombData(new List<string>(), new SCG.Dictionary<string, int>()) };
        
        // 1.1 单张
        foreach (var kv in cardsMap) {
            if (kv.Value >= 1) {
                // soloList.Add(kv.Key);
                var intList = new List<int> { kv.Key };
                combList.Add(IntList2CombData(intList));
            }

            if (kv.Value >= 2) { 
                pairList.Add(kv.Key);
            }

            // if (kv.Value >= 3) {
            //     trioList.Add(kv.Key);
            // }

            if (kv.Value >= 4) { // 你说得对，但是我可以整很多牌
                bombList.Add(kv.Key);
            }
        }

        // var trios = new List<List<int>>();
        // var lines = new List<List<int>>();
        
        // 1.2 单顺
        // var lines = CreateStraight(soloList);

        // 2.1 对子
        foreach (var pair in pairList) {
            combList.Add(IntList2CombData(new List<int> { pair, pair }));
        }
        
        // 3.2 三带
        // foreach (var trio in trioList) {
        //     trios.Add(new List<int> { trio, trio, trio });
        //     // combList.Add(IntList2CombData(new List<int> { trio, trio, trio }));
        // }
        
        CreateAllTrioLine(cardsInt, combList);
        
        // 炸弹
        foreach (var bomb in bombList) {
            var intList = new List<int> { bomb, bomb, bomb, bomb };
            combList.Add(IntList2CombData(intList));
        }
        
        return CombTool.CombDistinct(combList);
    }

    private void CreateAllTrioLine(List<int> currentCardsInHand, List<CombData> combList, List<int> currentLead = null, 
        int depth = 0) {
        
        if (depth == 4) return;
        var soloList = new List<int>();
        // var lines = new List<List<int>>();
        var trios = new List<List<int>>();
        var cardsMap = Int2CardMap(currentCardsInHand);
        foreach (var kv in cardsMap) {
            soloList.Add(kv.Key); // 有肯定大于等于1
            if (kv.Value >= 3) {
                trios.Add(new List<int>() { kv.Key, kv.Key, kv.Key });
            }
        }

        var lines = CreateStraight(soloList);
        foreach (var trio in trios) {
            var newCurrentLead = new List<int>();
            if (currentLead != null) {
                newCurrentLead.AddRange(currentLead);
            }
            
            // combList.Add(IntList2CombData(trio));
            var newList = currentCardsInHand.ToList(); // 深拷贝
            foreach (var cardInt in trio) {
                // if (!newList.Remove(cardInt)) return; // 选牌都是通过现有牌选的，而且只看三张，理论上不会没有
                newList.Remove(cardInt); // 去除第一个值
                newCurrentLead.Add(cardInt);
            }
            combList.Add(IntList2CombData(newCurrentLead));
            CreateAllTrioLine(newList, combList, newCurrentLead, depth + 1);
        }

        foreach (var line in lines) {
            var newCurrentLead = new List<int>();
            if (currentLead != null) {
                newCurrentLead.AddRange(currentLead);
            }
            
            // combList.Add(IntList2CombData(trio));
            var newList = currentCardsInHand.ToList(); // 深拷贝
            foreach (var cardInt in line) {
                newList.Remove(cardInt); // 去除第一个值
                newCurrentLead.Add(cardInt);
            }
            combList.Add(IntList2CombData(newCurrentLead));
            CreateAllLine(newList, combList, newCurrentLead, depth + 1);
        }
    }

    // 优化：设trio为1，line为0，只考虑：
    // 1，0；
    // 1 1，0 0，1 0；
    // 1 1 1，0 0 0，1 0 0，1 1 0；
    // 1 1 1 1，0 0 0 0，1 0 0 0，1 1 0 0，1 1 1 0；
    // 0 的时候只考虑 0，1 的时候都考虑
    private void CreateAllLine(List<int> currentCardsInHand, List<CombData> combList, List<int> currentLead,
        int depth = 0) {
        
        if (depth == 4) return;
        var soloList = new List<int>();
        // var lines = new List<List<int>>();
        var cardsMap = Int2CardMap(currentCardsInHand);
        foreach (var kv in cardsMap) {
            soloList.Add(kv.Key); // 有肯定大于等于1
        }

        var lines = CreateStraight(soloList);

        foreach (var line in lines) {
            var newCurrentLead = new List<int>();
            if (currentLead != null) {
                newCurrentLead.AddRange(currentLead);
            }
            
            // combList.Add(IntList2CombData(trio));
            var newList = currentCardsInHand.ToList(); // 深拷贝
            foreach (var cardInt in line) {
                newList.Remove(cardInt); // 去除第一个值
                newCurrentLead.Add(cardInt);
            }
            combList.Add(IntList2CombData(newCurrentLead));
            CreateAllLine(newList, combList, newCurrentLead, depth + 1);
        }
    }
    
    // 内部使用函数（直接add，所以无所谓ref）
    private List<List<int>> CreateStraight(List<int> list) {
        var newList = list.Where(card => card <= 9).ToList();
        newList.Sort();
        var len = newList.Count;
        if (len < 3) return new List<List<int>>(); // null
        
        var possIntLists = new List<List<int>>(); 
        // 滑动窗口 [0, min - 1]
        for (int beg = 0; beg < len - 2; beg++) {
            // for (int end = beg + 3 - 1; end < len; end++) {
            if (newList[beg + 2] - newList[beg] != 2) break;
            var tmpList = new List<int>(); // 只记录一个
            for (int i = beg; i <= beg + 2; i++) {
                tmpList.Add(newList[i]);
            }

            // combList.Add(IntList2CombData(tmpList));
            possIntLists.Add(tmpList);
            // }
        }

        return possIntLists;
    }
    
    public bool IsCombValid(List<CardData> tryLeadCards, out CombData toLeadComb) {
        var cards = RuleTool.CardList2RuleList(tryLeadCards);
        // 这里的排序针对点数，最终封装的还是原参数 tryLeadCards
        var sortedCards = SortCards(cards);
        var cardsStr = RuleTool.RuleList2RuleStr(sortedCards);
        // 合法的情况：将 Godot Array 转成 C# Dictionary 格式
        if (Data.TryGetValue(cardsStr, out var value)) {
            var localTypes = new SCG.Dictionary<string, int>();
            // value: [["pair", 4], ["xxx", n], ...]
            // typeAndWeight: ["pair", 4]
            foreach (var typeAndWeight in (Array)value) {
                var tmpType = (string)((Array)typeAndWeight)[0];
                var tmpWeight = (int)((Array)typeAndWeight)[1];
                // 【补充】JSON包括同type多权值，但①情况仅限于三带四带且差值无所谓；②字典覆盖使得自动获得最大值
                localTypes[tmpType] = tmpWeight;
            }
            // 该部分借用canFollow的代码，出于保险起见也加判断条件（没用）
            // if (localTypes.Contains(new KeyValuePair<string, int>("rocket", -1))) {
            //     localTypes.Remove("rocket");
            // }
            if (localTypes.Contains(new KeyValuePair<string, int>("bomb", -1))) {
                localTypes.Remove("bomb");
            }
            // 封装的时候就排序，使得comb一定按照当前rule的出牌画面排序
            SortToLead(tryLeadCards);
            toLeadComb = new CombData(tryLeadCards, localTypes);

            // // [Debug] 打印类型
            // var tmp = "[Types] ";
            // foreach (var kv in toLeadComb.Types) {
            //     // GD.Print($"{kv.Key}: {kv.Value}");
            //     tmp += $"{kv.Key}: {kv.Value}; ";
            // }
            // GD.Print(tmp);
            
            return true;
        }
  
        toLeadComb = null;
        return false;
    }

    public bool CanBeat(CombData comb1, CombData comb2) {
        // types: {"xxx": 10, "yyy": 5}, {"yyy": 6}
        var types1 = comb1.Types;
        var types2 = comb2.Types;
        
        // 先判断炸弹和火箭（该逻辑下不能添加 "rocket": -1）
        // 最终尝试添加-1的判断，莫名其妙地成功了，原因不明！！！！！
        // if (types2.ContainsKey("rocket") && types2["rocket"] != -1) {
        //     return false;
        // }

        // TMD 今天（2024年3月21日）才发现原来自己的王炸是打不出来的……
        // 好在canbeat只有玩家用，只要玩家的牌没有王炸就不受影响……赶紧改过来……
        // if (types1.ContainsKey("rocket")) {
        //     return true;
        // }

        var isType1Bomb = types1.ContainsKey("bomb") && types1["bomb"] != -1;
        var isType2Bomb = types2.ContainsKey("bomb") && types2["bomb"] != -1;
        
        // 我的牌能算炸弹而对手牌不算，能压
        if (isType1Bomb && !isType2Bomb) {
            return true;
        }
        // 反之，压不了
        if (!isType1Bomb && isType2Bomb) {
            return false;
        }
        
        // 【补充】原版规则下炸弹只能是炸弹，只要对手牌的某一个type能压，就能压
        foreach (var kv in types2) {
            if (types1.TryGetValue(kv.Key, out var weight) && weight > kv.Value) {
                return true;
            }
        }

        return false;
    }

    public bool CanFollow(CombData target, List<CardData> myCardsInHand, out List<CombData> possibleCombs) {
        var myCardList = RuleTool.CardList2RuleList(myCardsInHand);
        if (CanFollowOri(target, myCardList, out var pcs)) {
            possibleCombs = pcs;
            return true;
        }

        possibleCombs = null;
        return false;
    }
    
    // 保留原版的的函数
    public bool CanFollowOri(CombData target, List<string> myCardList, out List<CombData> possibleCombs) {
        // var targetTypes = target.Types;

        var myCardStr = RuleTool.RuleList2RuleStr(myCardList);
        // GD.PrintErr(target == null);
        // 必须使用深度拷贝！！！
        var targetTypes = target.Types.ToDictionary(kv => kv.Key, kv => kv.Value);
        
        // 如果目标牌型为 rocket，则一定打不过，直接返回空
        // if (targetTypes.ContainsKey("rocket") && targetTypes["rocket"] != -1) {
        //     possibleCombs = new List<CombData>();
        //     return false;
        // }
        // 目标牌型不是 rocket，可能是 bomb，出于方便可以临时添加 -1 便于比较
        // targetTypes.Add("rocket", -1);
        // 如果不是 bomb，加入 -1 便于比较
        targetTypes.TryAdd("bomb", -1);
        
        var myCardMap = Str2CardMap(myCardStr);
        
        // 该部分逻辑与废弃代码类似，使用 Godot 数据结构
        // cardsGt: {"trio_solo": ["A-A-A-5"], "bomb": ["A-A-A-A"]}
        var cardsGt = new Dictionary();
        foreach (var kv in targetTypes) {
            var weightGt = new List<string>();
            // targetTypes是封装好的CombData中的，无需判断合法，直接调用
            // w = weight，
            foreach (var w in ((Dictionary)TypeCards[kv.Key]).Keys) {
                var wStr = (string)w;
                if (wStr.ToInt() > kv.Value) {
                    weightGt.Add(wStr);
                }

                // 对方牌type不存在，就先设置为 空array
                if (!cardsGt.ContainsKey(kv.Key)) {
                    cardsGt[kv.Key] = new Array();
                }
            }
            weightGt.Sort((x, y) => x.ToInt().CompareTo(y.ToInt()));
            
            foreach (var wStr in weightGt) {
                foreach (var wc in (Array)((Dictionary)TypeCards[kv.Key])[wStr]) {
                    var wCards = (string)wc;
                    var wCardmap = Str2CardMap(wCards);
                    if (CardsContain(myCardMap, wCardmap)
                        && !((Array)cardsGt[kv.Key]).Contains(wCards)) {
                        ((Array)cardsGt[kv.Key]).Add(wCards);
                    }
                } 
            }
            
            if (((Array)cardsGt[kv.Key]).Count == 0) {
                cardsGt.Remove(kv.Value);
            }
        }

        // 一个能打的都没有
        if (cardsGt.Count == 0) {
            possibleCombs = new List<CombData>();
            return false;
        }
        
        // return cardsGt;
        
        // 将 cardsGt 转换成 List<CombData>
        // 【待办】这部分逻辑需要优化，暂时搁置（无限期搁置……）
        possibleCombs = new List<CombData>();
        foreach (var kv in cardsGt) {
            foreach (var cardsStr in (Array)kv.Value) {
                // cardsStr就是通过TypeCards得到的，无需再检查
                // 这里直接借用 IsCombValid 的代码
                if (Data.TryGetValue(cardsStr, out var value)) {
                    var localTypes = new SCG.Dictionary<string, int>();
                    // value: [["pair", 4], ["xxx", n], ...]
                    // typeAndWeight: ["pair", 4]
                    foreach (var typeAndWeight in (Array)value) {
                        var tmpType = (string)((Array)typeAndWeight)[0];
                        var tmpWeight = (int)((Array)typeAndWeight)[1];
                        // 【补充】JSON包括同type多权值，但①情况仅限于三带四带且差值无所谓；②字典覆盖使得自动获得最大值
                        localTypes[tmpType] = tmpWeight;
                    }
                    // 去除-1的简便情况（实为想保留canBeat纯净性的无奈之举）
                    // if (localTypes.Contains(new KeyValuePair<string, int>("rocket", -1))) {
                    //     localTypes.Remove("rocket");
                    // }
                    if (localTypes.Contains(new KeyValuePair<string, int>("bomb", -1))) {
                        localTypes.Remove("bomb");
                    }
                    // 注：List<string>的排序遵从Rule字典排序，即完全依赖点数大小
                    var cards = RuleTool.RuleStr2RuleList((string)cardsStr);
                    possibleCombs.Add(new CombData(cards, localTypes));
                }
            }
        }

        // 再检查一遍，有备无患
        if (possibleCombs.Count == 0) {
            return false;
        }
        
        return true;
    }
    
    public void SortInHand(List<CardData> cardsInHand) {
        // 1. Point 小的在前；2. Suit 小的在前；3. 大小王在最后面
        // 直接比较 PointNum * 100 + SuitNum 即可
        cardsInHand.Sort((x, y) => 
            (100 * GetPointWeight(x.PointNum) + (int)x.SuitNum) - 
            (100 * GetPointWeight(y.PointNum) + (int)y.SuitNum));
    }

    public void SortToLead(List<CardData> combToLead, SCG.Dictionary<string, int> type = null) {
        // 麻将模式下，排序与手牌一致（玩家自己理牌）
        SortInHand(combToLead);
        // var cardsStr = RuleTool.CardList2RuleStr(combToLead);
        // var cardsMap = Str2CardMap(cardsStr);
        // // cardsMap 的数量作为放在前面的依据（大者在前），如果数量一致，再比较点数和花色
        // combToLead.Sort((x, y) =>
        //     cardsMap[CardTool.GetPointName(x.PointNum)] == cardsMap[CardTool.GetPointName(y.PointNum)]
        //         ? (100 * (int)y.PointNum - (int)y.SuitNum) - (100 * (int)x.PointNum - (int)x.SuitNum)
        //         : cardsMap[CardTool.GetPointName(y.PointNum)] - cardsMap[CardTool.GetPointName(x.PointNum)]);
    }
    
    // ------ 内部函数部分 ------

    /// <summary>
    /// 计数，相当于 Python 的 Counter
    /// </summary>
    /// <param name="cards"></param>
    /// <returns></returns>
    public SCG.Dictionary<int, int> Int2CardMap(List<int> cards) {
        var cardmap = new SCG.Dictionary<int, int>();
        foreach (var card in cards) {
            if (cardmap.ContainsKey(card)) {
                cardmap[card] += 1;
            }
            else {
                cardmap[card] = 1;
            }
        }

        return cardmap;
    }

    public List<string> Int2StrList(List<int> cards) {
        var res = new List<string>();
        foreach (var cardInt in cards) {
            res.Add(PointNames[cardInt]);
        }

        return res;
    }
    
    /// <summary>
    /// 封装intList得到Combdata，无花色的strList！理论上不会为空<br/>
    /// 不必轻易调用，理论上只有极其复杂的三连顺才有多types的可能……
    /// </summary>
    /// <param name="cards"></param>
    /// <returns></returns>
    public CombData IntList2CombData(List<int> cards) {
        var strList = Int2StrList(cards); // 使用自己的函数！！！！！
        // 借用 isCombValid 的代码（不能直接用，因为有 toLeadCards)
        
        // 这里的排序针对点数，最终封装的还是原参数 tryLeadCards
        var sortedCards = SortCards(strList);
        var cardsStr = RuleTool.RuleList2RuleStr(sortedCards);
        // 合法的情况：将 Godot Array 转成 C# Dictionary 格式
        if (Data.TryGetValue(cardsStr, out var value)) {
            var localTypes = new SCG.Dictionary<string, int>();
            // value: [["pair", 4], ["xxx", n], ...]
            // typeAndWeight: ["pair", 4]
            foreach (var typeAndWeight in (Array)value) {
                var tmpType = (string)((Array)typeAndWeight)[0];
                var tmpWeight = (int)((Array)typeAndWeight)[1];
                // 【补充】JSON包括同type多权值，但①情况仅限于三带四带且差值无所谓；②字典覆盖使得自动获得最大值
                localTypes[tmpType] = tmpWeight;
            }

            // 该部分借用canFollow的代码，出于保险起见也加判断条件（没用）
            // if (localTypes.Contains(new KeyValuePair<string, int>("rocket", -1))) {
            //     localTypes.Remove("rocket");
            // }

            if (localTypes.Contains(new KeyValuePair<string, int>("bomb", -1))) {
                localTypes.Remove("bomb");
            }

            // 注意，这里是没有花色的！！！
            return new CombData(strList, localTypes);
        }

        // 理论上不会到达这里
        return null;
    }
    
    /// 计数："A-A-J" -> {"A": 2, "J": 1}
    public SCG.Dictionary<string, int> Str2CardMap(string str) {
        var cards = RuleTool.RuleStr2RuleList(str);
        var cardmap = new SCG.Dictionary<string, int>();
        foreach (var card in cards) {
            if (cardmap.ContainsKey(card)) {
                cardmap[card] += 1;
            }
            else {
                cardmap[card] = 1;
            }
        }

        return cardmap;
    }
    
    /// mine: {"A": 3, "10": 2}, others: {"K": 3, "5": 1} => return true;
    public bool CardsContain(SCG.Dictionary<string, int> candidateCardmap, SCG.Dictionary<string, int> cardmap) {
        foreach (var kv in cardmap) {
            // 从自己的牌数字典中获取这个点数，如果没有就过
            if (!candidateCardmap.TryGetValue(kv.Key, out var value)) {
                return false;
            }
            // 如果有这个点数，但数量不够，过
            if (value < kv.Value) {
                return false;
            }
        }

        return true;
    }
    
    /// 排序：["A", "J"] -> ["J", "A"]
    public List<string> SortCards(List<string> cards) {
        // (x, y): x < y
        cards.Sort((x, y) => PointWeightMap[x].CompareTo(PointWeightMap[y]));
        return cards;
    }
}