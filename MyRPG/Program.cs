using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace MyRPG
{
    #region EquipmentType枚举：用来限制装备的种类，让代码更易读，防止乱写字符串
    //没有枚举，你写代码时判断装备类型只能靠数字或字符串
    //枚举：用来限制装备的种类，让代码更易读，防止乱写字符串
    enum EquipmentType
    {
        Weapon = 1,     //武器
        Armor = 2,      //护甲
        Accessory = 3,  //饰品
    }
    #endregion

    #region Equipment结构体：装备的数据模板（结构体是值类型）
    struct Equipment
    {
        // 1. 字段：装备的名称
        public string Name;
        // 2. 字段：装备的攻击力
        public int Attack;
        // 3. 字段：装备的血量加成
        public int HP;
        // 4. 字段：装备的类型（上面定义的枚举）
        public EquipmentType Type;

        // 5. 构造函数：方便我们快速创建装备时，一次性把数据填进去
        public Equipment(string name, int attack, int hp, EquipmentType type)
        {

            Name = name;
            Attack = attack;
            HP = hp;
            Type = type;
        }

    }
    #endregion

    [JsonDerivedType(typeof(Warrior), typeDiscriminator: "warrior")]
    [JsonDerivedType(typeof(Mage), typeDiscriminator: "mage")]
    [JsonDerivedType(typeof(Assassin), typeDiscriminator: "assassin")]

    abstract class Hero
    {
        // 重新推送一下12312412312
        //  ====共有字段（所有英雄都有的特性）=====
        public string Name;         //英雄名称
        public string Sex;            //性别（M或者F）
        public int Level;           //等级
        public float Attack;        //基础攻击力（浮点数，方便后续计算）
        public List<Equipment> Bag;     //背包列表

        // System.Text.Json 在反序列化时，如果对象有无参构造函数，就会用它创建对象，
        // 然后直接把 JSON 里的字段值赋给对象的同名字段。
        public Hero()
        {
            Bag = new List<Equipment>(); // 反序列化时，Bag 先初始化成一个空列表
        } //无参构造函数

        // ==== 有参构造函数（创建英雄时初始化数据，构造函数必须和类名相同） ==== 
        public Hero(string name, int level, string sex, float attack)
        {
            Name = name;
            Level = level;
            Sex = sex;
            Attack = attack;

            // 初始化背包数据，如果不做这一步，Bag就是null，以后往里面放东西会报空引用异常

            Bag = new List<Equipment>();
        }


        //添加一个新方法：用来打印这个英雄的完整信息
        virtual public void ShowInfo() // 子类可以重写
        {
            //1. 打印基础信息
            Console.WriteLine($"  职业：{this.GetType().Name}");
            Console.WriteLine($"名称：{Name}");
            Console.WriteLine($"  等级：{Level}");
            Console.WriteLine($"  性别：{(Sex == "M" ? "男" : "女")}");  // 三元运算符
            Console.WriteLine($"  攻击力：{Attack:F2}");                 // 保留两位小数
            Console.WriteLine($"技能：{GetSkillDescription()}");
            // 2.打印背包里的所有装备
            Console.WriteLine("背包内容:");
            Console.WriteLine("========我是分割线========:");

            if (Bag == null)
            {
                Console.WriteLine("空");

            }
            // 使用for循环遍历背包（带下标，方便以后做修改操作）
            for (int i = 0; i < Bag.Count; i++)
            {
                // 判断背包这一格是否有装备，（因为Equipment是结构体，默认Name是null）
                // Bag[0]表示为第一个装备
                if (Bag[i].Name != null)
                {
                    // 攻击力显示：正数显示 "+数值"，负数直接显示 "数值"
                    string attackDisplay = Bag[i].Attack >= 0 ? $"+{Bag[i].Attack}" : $"{Bag[i].Attack}";
                    // 血量显示：正数显示 "+数值"，负数直接显示 "数值"
                    string hpDisplay = Bag[i].HP >= 0 ? $"+{Bag[i].HP}" : $"{Bag[i].HP}";

                    Console.WriteLine($"    [{i + 1}]{Bag[i].Name}(攻击{attackDisplay}, 血量{hpDisplay},类型：{Bag[i].Type})");
                }
                else
                {
                    Console.WriteLine($"    [{i + 1}](空)");
                }
            }
        }

        public abstract string GetSkillDescription(); // 子类必须重写

    }

    class Warrior : Hero
    {
        public int Stamina; //耐力
        public Warrior(string name, int level, string sex, float attack, int stamina) : base(name, level, sex, attack)
        {
            Stamina = stamina;
        }

        public override string GetSkillDescription()
        {
            return "猛击——造成150%攻击力的物理伤害";
        }

        public override void ShowInfo()
        {
            base.ShowInfo();
            Console.WriteLine($"耐力：{Stamina}");
        }
    }

    class Mage : Hero
    {
        public int Mana; //耐力
        public Mage(string name, int level, string sex, float attack, int mana) : base(name, level, sex, attack)
        {
            Mana = mana;
        }

        public override string GetSkillDescription()
        {
            return "魔法——造成150%攻击力的魔法伤害";
        }

        public override void ShowInfo()
        {
            base.ShowInfo();
            Console.WriteLine($"法力：{Mana}");
        }
    }

    class Assassin : Hero
    {
        public int Agility; //耐力
        public Assassin(string name, int level, string sex, float attack, int agility) : base(name, level, sex, attack)
        {
            Agility = agility;
        }

        public override string GetSkillDescription()
        {
            return "背刺——造成250%攻击力的物理伤害";
        }

        public override void ShowInfo()
        {
            base.ShowInfo();
            Console.WriteLine($"敏捷：{Agility}");
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            List<Hero> heroes = LoadGame(); // 尝试加载

            if (heroes == null) // 如果没有存档，或者加载失败
            {
                heroes = new List<Hero>(); // 创建一个空列表

                Console.WriteLine("首次进入游戏，已经创建默认英雄和装备！ \n按任意键确认！");
                Console.ReadLine();

                Warrior defaultHero1 = new Warrior("剑圣", 5, "M", 60.0f, 100);
                defaultHero1.Bag.Add(new Equipment("铁剑", 12, 0, EquipmentType.Weapon));
                defaultHero1.Bag.Add(new Equipment("残暴之力", 80, 200, EquipmentType.Weapon));
                defaultHero1.Bag.Add(new Equipment("饮血剑", 90, 500, EquipmentType.Weapon));
                defaultHero1.Bag.Add(new Equipment("鬼索的狂暴之刃", 60, 300, EquipmentType.Weapon));
                defaultHero1.Bag.Add(new Equipment("破败王者之刃", 50, 300, EquipmentType.Weapon));

                Mage defaultHero2 = new Mage("阿狸", 5, "F", 50.0f, 100);
                defaultHero2.Bag.Add(new Equipment("魔力戒指", 4, 20, EquipmentType.Accessory));

                Assassin defaultHero3 = new Assassin("劫", 5, "M", 55.0f, 50);
                defaultHero3.Bag.Add(new Equipment("皮甲", 2, 10, EquipmentType.Armor));

                heroes.Add(defaultHero1);
                heroes.Add(defaultHero2);
                heroes.Add(defaultHero3);

                //用户添加英雄
                //int addHeroIndex;
                //bool addHeroIndexisOK = int.TryParse(Console.ReadLine(), out addHeroIndex);
                //if(addHeroIndexisOK && addHeroIndex > 0 )
                //{

                //    switch (addHeroIndex)
                //    {
                //        case 1:
                //            {
                //                Warrior currentHero = new Warrior("剑圣", 5, "M", 30.0f, 10);
                //                heroes.Add(currentHero);
                //                currentHero.Bag.Add(new Equipment("铁剑", 12, 0, EquipmentType.Weapon));
                //                currentHero.Bag.Add(new Equipment("a剑", 14, 0, EquipmentType.Weapon));
                //                currentHero.Bag.Add(new Equipment("g剑", 18, 0, EquipmentType.Weapon));
                //                currentHero.Bag.Add(new Equipment("c剑", 26, 0, EquipmentType.Weapon));
                //                currentHero.Bag.Add(new Equipment("e剑", 98, 0, EquipmentType.Weapon));
                //                break;
                //            }
                //        case 2:
                //            {
                //                Mage currentHero = new Mage("剑圣", 5, "M", 30.0f, 10);
                //                heroes.Add(currentHero);
                //                currentHero.Bag.Add(new Equipment("魔力戒指", 4, 20, EquipmentType.Accessory));
                //                break;
                //            }
                //        case 3:
                //            {
                //                Assassin currentHero = new Assassin("剑圣", 5, "M", 30.0f, 10);
                //                heroes.Add(currentHero);
                //                currentHero.Bag.Add(new Equipment("皮甲", 2, 10, EquipmentType.Armor));
                //                break;
                //            }
                //        default:
                //            Console.WriteLine("请重新输入");
                //            break;
                //    }
                //}
            }

            // ==== 2.  游戏主循环 ====
            while (true)
            {
                Console.Clear(); // 清屏，让菜单每次都显示在顶部（清爽）
                Console.WriteLine("======= RPG 英雄小队管理系统 =======");
                Console.WriteLine("1. 查看所有英雄信息");
                Console.WriteLine("2. 给指定英雄添加装备");
                Console.WriteLine("3. 按攻击力排序英雄背包");
                Console.WriteLine("4. 查找全队最强攻击装备");
                Console.WriteLine("5. 删除指定装备");
                Console.WriteLine("6. 退出系统");
                Console.WriteLine("====================================");
                Console.Write("请输入选项（1-6）：");

                string input = Console.ReadLine();

                // =====  3. switch 分支判断 ===
                switch (input)
                {
                    case "1": // 查看所有英雄信息
                        Console.WriteLine("====== 英雄小队详细信息 =====");

                        //用for循环遍历英雄数组（下标从0开始）
                        for (int i = 0; i < heroes.Count; i++)
                        {
                            Console.WriteLine($"【英雄{i + 1}】");
                            heroes[i].ShowInfo();   //直接调用对象的方法     
                            Console.WriteLine();  //每个英雄后面空一行
                        }
                        break;

                    case "2": //给指定英雄添加装备
                        Console.WriteLine("给英雄添加装备");
                        //查看所有英雄信息
                        for (int i = 0; i < heroes.Count; i++)
                        {
                            Console.WriteLine($"【英雄{i + 1}】");
                            heroes[i].ShowInfo();   //直接调用对象的方法     
                            Console.WriteLine();  //每个英雄后面空一行
                        }
                        Console.WriteLine("请输入英雄编号，或输入 0 返回主菜单");

                        int IndexInt = GetHeroIndex(heroes);
                        if (IndexInt == -2) { continue; } // 返回主菜单，这里的实现是continue进入下一个新的while循环


                        if (IndexInt >= 0 && IndexInt < heroes.Count)
                        {
                            Hero currentHero = heroes[IndexInt];
                            //通过输入得到装备的参数
                            Console.WriteLine($"正在为{heroes[IndexInt].Name}添加装备");
                            Console.WriteLine();
                            Console.WriteLine("请输入添加装备的名称");
                            string addEquipmentName = Console.ReadLine();
                            Console.WriteLine("请输入添加装备的攻击力（整数）");
                            int addEquipmentAttack;
                            bool addEquipmentAttackisOK = int.TryParse(Console.ReadLine(), out addEquipmentAttack);
                            Console.WriteLine("请输入添加装备的血量（整数）");
                            int addEquipmentHP;
                            bool addEquipmentHPisOK = int.TryParse(Console.ReadLine(), out addEquipmentHP);
                            Console.WriteLine("请输入添加装备的装备类型（1-武器，2-护甲，3-饰品");
                            int addEquipmentType;
                            bool addEquipmentTypeisOK = int.TryParse(Console.ReadLine(), out addEquipmentType);
                            //addEquipmentType = (EquipmentType)addEquipmentType;  //这里卡住了，不能强制转型

                            //装备类型枚举转换
                            EquipmentType currentAddEquipmentType = (EquipmentType)addEquipmentType;
                            // 往空的格子里添加装备，总是往第一个空着的格子添加

                            Equipment addEquipment = new Equipment(addEquipmentName, addEquipmentAttack, addEquipmentHP, currentAddEquipmentType);
                            currentHero.Bag.Add(addEquipment);

                        }
                        else
                        {
                            Console.WriteLine("英雄编号错误或英雄编号超限。");
                        }
                        break;

                    case "3": // 按攻击力排序英雄背包
                        Console.WriteLine("按攻击力排序英雄背包(冒泡排序-降序");
                        for (int i = 0; i < heroes.Count; i++)
                        {
                            Console.WriteLine($"【英雄{i + 1}】");
                            heroes[i].ShowInfo();   //直接调用对象的方法     
                            Console.WriteLine();  //每个英雄后面空一行
                        }

                        Console.WriteLine("要排序哪个英雄？请输入英雄编号，或输入 0 返回主菜单");

                        IndexInt = GetHeroIndex(heroes);
                        if (IndexInt == -2) { continue; } // 返回主菜单，这里的实现是continue进入下一个新的while循环

                        if (IndexInt >= 0 && IndexInt < heroes.Count)
                        {
                            Hero currentHero = heroes[IndexInt];  // 拿到英雄
                            for (int i = 0; i < currentHero.Bag.Count - 1; i++)
                            {
                                bool swapped = false;  //交换标志位,在外循环内，每次排序都能重置标志位
                                for (int j = 0; j < currentHero.Bag.Count - 1 - i; j++)
                                {
                                    if (currentHero.Bag[j].Attack < currentHero.Bag[j + 1].Attack)
                                    {
                                        Equipment temp = currentHero.Bag[j];
                                        currentHero.Bag[j] = currentHero.Bag[j + 1];
                                        currentHero.Bag[j + 1] = temp;
                                        swapped = true;
                                    }
                                }
                                heroes[IndexInt].ShowInfo();   //直接调用对象的方法     
                                Console.WriteLine();  //每个英雄后面空一行
                                if (!swapped) { break; } //整个内循环没有交换，则证明排序完成
                            }
                        }
                        break;

                    case "4":
                        Console.WriteLine("正在查找全队最强攻击装备···");
                        int maxAttack = -1;
                        string maxAttackEquimentName = string.Empty;
                        string maxAttackEquimentHeroName = string.Empty;

                        for (int i = 0; i < heroes.Count; i++)
                        {
                            for (int j = 0; j < heroes[i].Bag.Count; j++)
                            {
                                if (heroes[i].Bag[j].Name != null && heroes[i].Bag[j].Attack > maxAttack)
                                {
                                    maxAttack = heroes[i].Bag[j].Attack;
                                    maxAttackEquimentName = heroes[i].Bag[j].Name;
                                    maxAttackEquimentHeroName = heroes[i].Name;
                                }
                            }
                        }
                        if (maxAttack == -1)
                        {
                            Console.WriteLine("全队没有任何装备");
                        }
                        else
                        {
                            Console.WriteLine($"全队最强攻击装备是{maxAttackEquimentHeroName}的{maxAttackEquimentName}，攻击力为{maxAttack}点。");
                        }
                        break;

                    case "5": //删除指定装备
                              //提示并展示英雄装备
                        Console.WriteLine("删除指定装备");
                        for (int i = 0; i < heroes.Count; i++)
                        {
                            Console.WriteLine($"【英雄{i + 1}】");
                            heroes[i].ShowInfo();   //直接调用对象的方法     
                            Console.WriteLine();  //每个英雄后面空一行
                        }

                        Console.WriteLine("要删除哪个英雄的装备？请输入英雄编号，或输入 0 返回主菜单");
                        //展示被选中英雄的装备
                        IndexInt = GetHeroIndex(heroes);

                        if (IndexInt == -2) { continue; } // 返回主菜单，这里的实现是continue进入下一个新的while循环

                        heroes[IndexInt].ShowInfo();   //展示该英雄的装备
                        Console.WriteLine("要删除该英雄的哪个装备？请输入装备编号，或输入 0 返回主菜单");
                        int IndexEquipment = GetEquipmentIndex(heroes[IndexInt]); //从输入取得装备的索引（0基下标）
                        if (IndexEquipment == -1)
                        {
                            Console.WriteLine($" 该英雄没有装备！");
                            break;
                        }
                        else if (IndexEquipment == -2)
                        {
                            continue;
                        }

                        //string saveHeroName = heroes[IndexInt].Name;
                        string saveEquipmentName = heroes[IndexInt].Bag[IndexEquipment].Name;

                        heroes[IndexInt].Bag.RemoveAt(IndexEquipment);

                        Console.WriteLine($"英雄{heroes[IndexInt].Name}的{saveEquipmentName}删除完成!");
                        //展示更新后的背包
                        heroes[IndexInt].ShowInfo();
                        //先保存装备名称到变量，再执行删除，然后用保存的变量打印提示。
                        break;

                    case "6":
                        Console.WriteLine("感谢游玩，再见！");
                        SaveGame(heroes);
                        return; // 直接结束 Main 方法，退出程序

                    default:
                        Console.WriteLine("⚠️ 输入无效，请输入 1-5 之间的数字！");
                        break;

                }
                // 暂停一下，让用户看到提示信息后再回到菜单
                Console.WriteLine("\n按任意键返回菜单...");
                Console.ReadKey();
            }


        }

        #region 从输入得到英雄的0基下标，或返回主菜单
        //静态参数函数 不需要 new 对象，允许我们在没有对象的情况下调用它
        //参数允许它将外部数据（hero 数组）带入其作用域以进行验证。
        //Main 是静态的（static void Main）。静态方法只能直接调用静态方法，
        //如果 GetHeroIndex 不是静态的，你就必须写 Program p = new Program(); p.GetHeroIndex(hero); 才能用，非常麻烦。
        //它适合放一些“纯功能”逻辑（比如输入验证、数据转换），
        //这些逻辑不依赖于具体的对象实例（不需要访问 this.Name 之类的东西）。
        static int GetHeroIndex(List<Hero> heroes)
        {
            while (true)
            {
                Console.WriteLine("请输入：");
                string IndexString = Console.ReadLine();

                // 返回上一级
                if (IndexString == "0")
                {
                    return -2; // 约定：-2 代表“用户想返回”
                }

                if (!int.TryParse(IndexString, out int IndexInt))
                {
                    Console.WriteLine($" 请输入有效的数字！(1-{heroes.Count}),或输入 0 返回上一级");
                    continue;
                }
                IndexInt = IndexInt - 1; //用户输入的 1 对应 数组下标 0

                //检查范围是否合法
                if (IndexInt < 0 || IndexInt >= heroes.Count)
                {
                    Console.WriteLine($" 英雄编号错误！请输入1-{heroes.Count}之间的数字,或输入 0 返回上一级");
                    continue;
                }
                return IndexInt;

            }

        }
        #endregion

        #region 从输入得到装备的0基下标，或返回主菜单
        // 从输入得到装备的下标，或返回主菜单
        static int GetEquipmentIndex(Hero hero)
        {
            int equipCount = hero.Bag.Count;
            if (equipCount == 0)
            {
                return -1; // 约定：-1 代表“没有装备”
                //Console.WriteLine($" 该英雄没有装备！"); 
            }
            while (true)
            {
                Console.WriteLine("请输入：");
                string IndexString = Console.ReadLine();

                // 返回上一级
                if (IndexString == "0") { return -2; }// 约定：-2 代表“用户想返回”

                if (!int.TryParse(IndexString, out int IndexInt))
                {
                    Console.WriteLine($" 请输入有效的数字！(1-{equipCount}),或输入 0 返回主菜单");
                    continue;
                }

                IndexInt = IndexInt - 1; //用户输入的 1 对应 数组下标 0

                //检查范围是否合法
                if (IndexInt < 0 || IndexInt >= hero.Bag.Count)
                {
                    Console.WriteLine($" 装备编号错误！请输入1-{equipCount}之间的数字,或输入 0 返回主菜单");
                    continue;
                }

                if (hero.Bag[IndexInt].Name == null)
                {
                    Console.WriteLine($" 这个格子没有装备！请输入1-{equipCount}之间的数字,或输入 0 返回主菜单");
                    continue;
                }

                return IndexInt;

            }

        }
        #endregion

        //全局翻译配置
        static JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true, // 让生成的 JSON 文件有缩进，好看
            IncludeFields = true,  // 告诉 JSON 库把字段也当作属性一样处理
            Converters = { new JsonStringEnumConverter() } // 让枚举显示为文字
        };

        #region 保存json
        static void SaveGame(List<Hero> heroes)
        {
            // 1. 把对象列表翻译成 JSON 字符串（使用刚才定义的 options）
            string jsonString = JsonSerializer.Serialize(heroes, options);
            // 2. 把字符串写入文件
            File.WriteAllText("save.json", jsonString);
            Console.WriteLine("游戏已保存！");
        }
        #endregion

        #region 读取json
        static List<Hero> LoadGame()
        {
            // 1. 检查文件存不存在
            if (!File.Exists("save.json")) { return null; }
            // 2. 读取文件里的 JSON 字符串
            string jsonString = File.ReadAllText("save.json");
            // 3. 把 JSON 字符串反翻译回对象列表（使用同样的 options）
            List<Hero> heroes = JsonSerializer.Deserialize<List<Hero>>(jsonString, options);
            return heroes;
        }
        #endregion  

    }

}
