# 个人独立游戏「斗牌谜室」

（既[kazelee/private-stalled-dcp: 个人独立游戏「斗牌谜室」搁置demo](https://github.com/kazelee/private-stalled-dcp)之后，再次搁置本项目，另开新路）

#### 2024年4月13日补充：

从 3 月份开始的狂热的努力，最终被认定是一场闹剧：太注重“做出来”，而忽视了游戏的质量。

我依然不认同业界的“前 10 个游戏是垃圾”的论调，不过我想，也许可以先做几个免费/改编游戏作为过渡？但斗牌谜室作为 Steam 上线的第一款个人商业收费游戏，必然得有好的质量，最起码自己得玩下去。

思考再三，还是决定放下「解谜」的架子，选择做有随机和策略性质的游戏，这样也不用担心关卡设计了。

当然也可以设计少量的解谜关卡，但绝对不能喧宾夺主，就像骰子地下城也有解谜，但不多。

---

游戏需要大改，如果说 doudizhu 是学习作品，搁置 demo 是 1.0，那么本次项目就是 1.2——最终还是要走向 2.0 的全新版本的，所有现有的东西，包括素材、音乐、UI 等，都会面临更改。

更不要说什么出牌、打牌限制，当然这些会以肉鸽的性质存在，变成弃牌和出牌，这样也不错。

设计游戏技能，类比小丑牌，但目标是先办法先把手中的牌出完。

要有非常精致的像素，现在的版本肯定不能看。

人物和游戏模式还要进一步减少！

---

具体的改进计划如下：

- AI 的设计只局限于其中一关，研究 AI 打三人配合并获得胜利
- 游戏模式限制在：原版、同花？/同牌可打？（感觉可以作为特殊情况）、麻将？（自创的游戏模式，不太合理）、德州扑克和 21 点？（感觉自创的游戏模式都有不少问题，最多作为斗地主的延申，而非具体规则）
	- 可以设计一系列规则卡， 临时变换规则，这就完全改变原先的代码框架了
	- 可以看到未来的排序（解谜模式下），后面的麻将也可以参考这个思路
	- 其他规则都不考虑了，包括逆天的数学规则，死了这条心吧……
- 不一定要完全符合斗地主的思路，其实可以只以此为背景，创作一个更灵活的卡牌游戏
- 人物也要进一步减少，只保留房东、管家、博主、极客、留学生、大汉这六个人
- 或者干脆不要搞成旅社了，就是一个小别墅，人物都是合租的关系，我正好进入儿子的房间

---

不要搞太明显的致敬，至少人物还是不要太明显，可以在别的地方致敬。

Albert 这种创意就算了，致敬世界的尽头没必要，这种演出也不符合玩家的口味。

多研究研究场景叙事，写太多无意义的对话吐槽也让人烦闷，文字要精炼，偶尔来点惊喜就行。

至于玩法，尽量简单一些，不要动不动就涉及一堆牌，要有趣味性（感觉这真的很难了……）

放弃用山魔王宫殿和爵士夜曲作为宣传曲，放弃允许大量牌重复（除非两副，两副也有两副的要求）

但两副感觉就复杂了，尽量还是限制在一副牌，最多 3 个人（OK，点到为止，不要在发散了）