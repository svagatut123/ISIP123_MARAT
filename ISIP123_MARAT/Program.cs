// пр6
using System;
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
    //типы врагов
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
        public int Attack { get; set; }

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
            return $"{Name} (атака: {Attack}, ценность: {Value})";
        }
    }

    public class Armor : Item
    {
        public int Defense {  get; set; }

        public Armor (string name, int value, int defense): base(name, value)
        {
            Defense = defense; 
        }
        public override void ApplyEffect(Player player)
        {
            player.EquipArmor(this);
        }
        public override string ToString()
        {
            return $"{Name} (защита: {Defense}, ценность: {Value})";
        }

    }

    public class HealthPotion : Item
    {
        public int Heal { get; set; }
        public HealthPotion(string name, int value, int heal) : base(name, value)
        {
            Heal = heal;
        }

        public override void ApplyEffect(Player player)
        {
            player.Heal(player.MaxHP);
            Console.WriteLine("hp восстановлено");
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

        protected Enemy (string name, int hP, int maxHP, int Attack, int defense, bool frozen, Random random)
        {
            Name = name;
            HP = hP;
            MaxHP = maxHP;
            Attack = Attack;
            Defense = defense;
            Frozen = frozen;
            random = new Random();
        }
        public virtual void Damagedealt (int  damage)
        {
            HP = HP - damage;
            if (HP < 0) HP = 0; 
        }
        public abstract int CalculateDamage(Player player);
        public abstract void ApplyEffectDamage(Player player);

        public bool IsAlive => HP > 0;

        public virtual string GetStatus()
        {
            return $"{Name} - hp: {HP}/{MaxHP}, атака:{Attack}, защита: {Defense}";
        }
    }

}
