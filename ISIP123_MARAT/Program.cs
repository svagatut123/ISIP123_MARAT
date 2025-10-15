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
}
