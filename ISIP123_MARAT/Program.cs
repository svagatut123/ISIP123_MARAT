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
            return $"{Name} (атака: {Attack}, ценность: {Value})";
        }
    }
}
