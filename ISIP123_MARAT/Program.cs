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
                player.IsFrozen = true;
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
                player.IsFrozen = true;
                Console.WriteLine("Пестов С-- накладывает заморозку");
            }
        }
    }
}