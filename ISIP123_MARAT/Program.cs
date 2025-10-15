using System;
using System.Collections.Generic;
using System.Numerics;

namespace Game
{
    public enum EnemyType
    {
        Goblin,
        Skeleton,
        Mage,
        Boss
    }

    public abstract class Item
    {
        public string Name { get; protected set; }
        public int Value { get; protected set; }

        protected Item(string name, int value)
        {
            Name = name;
            Value = value;
        }

        public abstract void ApplyEffect(Player player);
    }

    public class Weapon : Item
    {
        public int Attack { get; private set; }

        public Weapon(string name, int value, int attack) : base(name, value)
        {
            Attack = attack;
        }

        public override void ApplyEffect(Player player)
        {
            player.EquipWeapon(this);
        }

        public override string ToString()
        {
            return $"{Name} (Атака: {Attack}, Ценность: {Value})";
        }
    }

    public class Armor : Item
    {
        public int Defense { get; private set; }

        public Armor(string name, int value, int defense) : base(name, value)
        {
            Defense = defense;
        }

        public override void ApplyEffect(Player player)
        {
            player.EquipArmor(this);
        }

        public override string ToString()
        {
            return $"{Name} (Защита: {Defense}, Ценность: {Value})";
        }
    }

    public class HealthPotion : Item
    {
        public HealthPotion(string name, int value) : base(name, value) { }

        public override void ApplyEffect(Player player)
        {
            player.Heal(player.MaxHP);
            Console.WriteLine("Вы восстановили HP!");
        }

        public override string ToString()
        {
            return $"{Name} (восстановление здоровья, Ценность: {Value})";
        }
    }

    public abstract class Enemy
    {
        public string Name { get; set; }
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public bool Frozen { get; set; }

        protected Random random;

        protected Enemy(string name, int hp, int attack, int defense)
        {
            Name = name;
            MaxHP = hp;
            HP = hp;
            Attack = attack;
            Defense = defense;
            random = new Random();
        }

        public virtual void TakeDamage(int damage)
        {
            HP = HP - damage;
            if (HP < 0) HP = 0;
        }

        public abstract int CalculateDamage(Player player);
        public abstract void ApplyEffectDamage(Player player);

        public bool IsAlive => HP > 0;

        public virtual string GetStatus()
        {
            return $"{Name} - HP: {HP}/{MaxHP}, Атака: {Attack}, Защита: {Defense}";
        }
    }

    public class Goblin : Enemy
    {
        private double ChanceCrit = 0.2;

        public Goblin() : base("Гоблин", 30, 8, 3) { }

        public override int CalculateDamage(Player player)
        {
            int baseDamage = Attack;
            if (random.NextDouble() < ChanceCrit)
            {
                Console.WriteLine("выпал крит");
                baseDamage = (int)(baseDamage * 1.2);
            }
            return baseDamage;
        }

        public override void ApplyEffectDamage(Player player) { }
    }
    public class Skeleton : Enemy
    {
        public Skeleton() : base("Скелет", 25, 10, 2) { }

        public override int CalculateDamage(Player player)
        {
            return Attack; // игнорирует защиту игрока
        }

        public override void ApplyEffectDamage(Player player) { }
    }

    public class Mage : Enemy
    {
        private double freezeChance = 0.25;

        public Mage() : base("Маг", 20, 12, 1) { }

        public override int CalculateDamage(Player player)
        {
            return Attack;
        }

        public override void ApplyEffectDamage(Player player)
        {
            if (random.NextDouble() < freezeChance)
            {
                player.Frozen = true;
                Console.WriteLine("вы заморожены");
            }
        }
    }
    public class VVG : Goblin
    {
        public VVG() : base()
        {
            Name = "ВВГ (Босс Гоблин)";
            MaxHP = (int)(MaxHP * 2.0);
            HP = MaxHP;
            Attack = (int)(Attack * 1.5);
            Defense = (int)(Defense * 1.2);
        }

        public override int CalculateDamage(Player player)
        {
            int baseDamage = Attack;
            if (random.NextDouble() < 0.3) // +10% к базовому шансу
            {
                Console.WriteLine("ВВГ кританул");
                baseDamage = (int)(baseDamage * 1.8);
            }
            return baseDamage;
        }
    }

    public class Kovalsky : Skeleton
    {
        public Kovalsky() : base()
        {
            Name = "Ковальский (Босс Скелет)";
            MaxHP = (int)(MaxHP * 2.5);
            HP = MaxHP;
            Attack = (int)(Attack * 1.3);
            Defense = (int)(Defense * 1.4);
        }
    }

    public class ArchimageCPP : Mage
    {
        public ArchimageCPP() : base()
        {
            Name = "Архимаг C++ (Босс Маг)";
            MaxHP = (int)(MaxHP * 1.8);
            HP = MaxHP;
            Attack = (int)(Attack * 1.6);
            Defense = (int)(Defense * 1.1);
        }

        public override void ApplyEffectDamage(Player player)
        {
            if (random.NextDouble() < 0.35)
            {
                player.Frozen = true;
                Console.WriteLine("Архимаг C++ накладывает заморозку");
            }
        }
    }

    public class PestovCmm : Skeleton
    {
        private double freezeChance = 0.4;

        public PestovCmm() : base()
        {
            Name = "Пестов С-- (Босс Скелет-Маг)";
            MaxHP = (int)(MaxHP * 1.3);
            HP = MaxHP;
            Attack = (int)(Attack * 1.8);
            Defense = (int)(Defense * 0.6);
        }

        public override int CalculateDamage(Player player)
        {
            return Attack;
        }

        public override void ApplyEffectDamage(Player player)
        {
            if (random.NextDouble() < freezeChance)
            {
                player.Frozen = true;
                Console.WriteLine("Пестов С-- накладывает заморозку");
            }
        }
    }

    public class Player
    {
        public int HP { get; private set; }
        public int MaxHP { get; private set; }
        public Weapon CurrentWeapon { get; private set; }
        public Armor CurrentArmor { get; private set; }
        public bool Frozen { get; set; }
        public int TotalAttack => (CurrentWeapon?.Attack ?? 0);
        public int TotalDefense => (CurrentArmor?.Defense ?? 0);

        private Random random;

        public Player(int maxHP)
        {
            MaxHP = maxHP;
            HP = maxHP;
            random = new Random();

            // Стартовое снаряжение
            CurrentWeapon = new Weapon("меч", 5, 3);
            CurrentArmor = new Armor("армор", 5, 2);
        }

        public void TakeDamage(int damage)
        {
            HP -= damage;
            if (HP < 0) HP = 0;
        }

        public void Heal(int amount)
        {
            HP += amount;
            if (HP > MaxHP) HP = MaxHP;
        }

        public void EquipWeapon(Weapon weapon)
        {
            CurrentWeapon = weapon;
        }

        public void EquipArmor(Armor armor)
        {
            CurrentArmor = armor;
        }

        public int CalculateDamage()
        {
            return TotalAttack;
        }

        public bool TryDefend()
        {
            if (random.NextDouble() < 0.4)
            {
                Console.WriteLine("уклонились от атаки");
                return true;
            }
            return false;
        }

        public int CalculateBlockedDamage(int incomingDamage)
        {
            double blockPercentage = 0.7 + (random.NextDouble() * 0.3);
            int blockedDamage = (int)(TotalDefense * blockPercentage);
            return Math.Max(0, incomingDamage - blockedDamage);
        }

        public string GetStatus()
        {
            return $"Игрок - HP: {HP}/{MaxHP}, Атака: {TotalAttack}, Защита: {TotalDefense}";
        }

        public string GetEquipment()
        {
            return $"Оружие: {CurrentWeapon?.ToString() ?? "Нет"}\nДоспехи: {CurrentArmor?.ToString() ?? "Нет "}";
        }
    }
    public class Game
    {
        private Player player;
        private Random random;
        private int turnCount;

        // Списки предметов для генерации
        private List<Weapon> weapons = new List<Weapon>
        {
            new Weapon("Деревянный меч", 10, 5),
            new Weapon("Железный меч", 20, 8),
            new Weapon("зачарованный лук", 25, 10),
            new Weapon("аганим скипетр", 30, 12),
            new Weapon("алмазный меч", 50, 15)
        };

        private List<Armor> armors = new List<Armor>
        {
            new Armor("Кожаная броня", 10, 3),
            new Armor("Кольчуга", 20, 5),
            new Armor("железная броня", 30, 8),
            new Armor("алмазная броня", 25, 6),
            new Armor("незеритовая броня", 50, 12)
        };

        public Game()
        {
            player = new Player(100);
            random = new Random();
            turnCount = 0;
        }

        public void Start()
        {
            
            Console.WriteLine("начало игры\n");

            while (player.HP > 0)
            {
                turnCount++;
                Console.WriteLine($"\nХод {turnCount}");
                Console.WriteLine(player.GetStatus());

                if (player.Frozen)
                {
                    Console.WriteLine("Вы заморожены");
                    player.Frozen = false;
                    continue;
                }

                // Случайное событие: 50% сундук, 50% враг
                if (random.Next(2) == 0)
                {
                    EncounterChest();
                }
                else
                {
                    EncounterEnemy();
                }

                // Каждые 10 ходов - босс
                if (turnCount % 10 == 0)
                {
                    Console.WriteLine("\nвнимаени босс!");
                    EncounterBoss();
                }

                if (player.HP > 0)
                {
                    Console.WriteLine("\nНажмите любую клавишу для продолжения");
                    Console.ReadKey();
                }
            }

            Console.WriteLine("\nигра окончена");
        }

        private void EncounterChest()
        {
            Console.WriteLine("сундук");

            // Случайный предмет: 33% зелье, 33% оружие, 33% доспехи
            int itemType = random.Next(3);
            Item item = null;

            switch (itemType)
            {
                case 0:
                    item = new HealthPotion("зелье хп", 15);
                    break;
                case 1:
                    item = weapons[random.Next(weapons.Count)];
                    break;
                case 2:
                    item = armors[random.Next(armors.Count)];
                    break;
            }

            Console.WriteLine($"В сундуке: {item}");

            if (item is HealthPotion)
            {
                item.ApplyEffect(player);
            }
            else
            {
                Console.WriteLine("\nтекущая экипировка:");
                Console.WriteLine(player.GetEquipment());
                Console.WriteLine("\nвзять этот предмет? (д/н)");

                string choice = Console.ReadLine().ToLower();
                if (choice == "д" || choice == "y")
                {
                    item.ApplyEffect(player);
                    Console.WriteLine("Предмет получен");
                }
                else
                {
                    Console.WriteLine("Вы выбросили предмет.");
                }
            }
        }

        private void EncounterEnemy()
        {
            Enemy enemy;
            int enemyType = random.Next(3);

            switch (enemyType)
            {
                case 0:
                    enemy = new Goblin();
                    break;
                case 1:
                    enemy = new Skeleton();
                    break;
                case 2:
                    enemy = new Mage();
                    break;
                default:
                    enemy = new Goblin();
                    break;
            }

            Console.WriteLine($"Вы встретили {enemy.Name}!");
            StartCombat(enemy);
        }

        private void EncounterBoss()
        {
            Enemy boss;
            int bossType = random.Next(4);

            switch (bossType)
            {
                case 0:
                    boss = new VVG();
                    break;
                case 1:
                    boss = new Kovalsky();
                    break;
                case 2:
                    boss = new ArchimageCPP();
                    break;
                case 3:
                    boss = new PestovCmm();
                    break;
                default:
                    boss = new VVG();
                    break;
            }

            Console.WriteLine($"Перед вами {boss.Name}!");
            StartCombat(boss);
        }
    }
}