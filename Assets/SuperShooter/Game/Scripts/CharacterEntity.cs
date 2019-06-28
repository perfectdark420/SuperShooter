﻿using UnityEngine;
using UnityEngine.Events;

namespace SuperShooter
{
    public interface ICharacterEntity : IGameEntity
    {

        int health { get; }
        bool isDead { get; }

        void Kill();

        void TakeDamage(int amount, ICharacterEntity from);
        void OnDamageDealt(int amount, ICharacterEntity target);
        void OnTargetKilled(ICharacterEntity target);
    }

    public enum TargetType
    {
        None = 0,
        Player = 1,
        Friendly = 2,
        Hostile = 3,
        Enemy = 4,
    }

    public class CharacterEntity : GameEntity, ICharacterEntity
    {
        [Header("Entity")]
        public TargetType type;
        public int maxHealth = 100;
        //public int startHealth = 100;
        public float headshotMultiplier = 2;
        public string characterName = "New Character Entity";

        [Header("Model")]
        public GameObject characterModel;

        [Header("Art")]
        public GameObject bloodSplatterPrefab;

        [Header("Events")]
        public UnityEvent<ICharacterEntity> CharacterDied;

        // ------------------------------------------------- //

        public int health { get; protected set; }

        public bool isDead { get; private set; }

        // ------------------------------------------------- //

        /// <summary>Adds/replenishes health to the character by the specified amount.</summary>
        public void AddHealth(int amount)
        {
            health += amount;
            if (health > maxHealth)
                health = maxHealth;
        }

        /// <summary>Sets the current amount of health the character has to the specified amount.</summary>
        public void SetHealth(int newAmount)
        {
            if (newAmount > maxHealth)
                newAmount = maxHealth;
            health = newAmount;
        }

        /// <summary>Resets character to back to its starting health value, and updates <see cref="isDead"/>.</summary>
        public void ResetHealth()
        {
            health = maxHealth;
            isDead = health == 0;
        }

        /// <summary>Resets character to maximum health and brings them back to life.
        /// If the character is not already dead when this is called, <see cref="BackFromTheDead"/> will not fire.</summary>
        public void Reincarnate()
        {
            var wasDead = isDead;       // Are we currently dead?
            ResetHealth();              // Reset our health values FIRST.
            if (wasDead)                // If we were dead before the health reset...
                BackFromTheDead();      // Allow art/effects/other events to happen.
        }

        /// <summary>Notifies derived class that its health as been reset to maximum.</summary>
        public virtual void BackFromTheDead() { }


        // ------------------------------------------------- //

        public void TakeDamage(int amount, ICharacterEntity from)
        {
            // Do nothing if we're already dead.
            if (isDead)
                return;

            //// Do nothing if we're amazing right now
            //if (Modifiers.HasFlag(EntityModifier.Invincible))
            //    return;

            // Deplete health by amount.
            // Clamp the value to zero as the minimum.
            health -= amount;
            if (health < 0)
                health = 0;

            //
            Debug.Log($"{type} '{name}' took {amount} damage from '{from.name}'. {health}HP left.");

            //
            OnDamageTaken(amount, from);

            // Did we die?
            if (health <= 0)
                Kill();

        }

        // Override
        public virtual void OnDamageTaken(int amount, ICharacterEntity from) { }    // Do nothing. MUST be overriden.

        // ------------------------------------------------- //

        public void DealDamage(int amount, ICharacterEntity target)
        {
            // Is target already dead?
            if (target.isDead)
                return; // Do nothing.

            // Deal damage
            target.TakeDamage(amount, this);

            // Run override
            OnDamageDealt(amount, target);

            if (target.isDead)
                OnTargetKilled(target);


        }

        // Override
        public virtual void OnDamageDealt(int amount, ICharacterEntity target) { }  // Do nothing. MUST be overriden.

        public virtual void OnTargetKilled(ICharacterEntity target) { } // Do nothing. MUST be overriden.

        // ------------------------------------------------- //

        /// <summary>Kills this character and runs its <see cref="OnDeath"/> function.</summary>
        public void Kill()
        {

            // Die
            isDead = true;

            //
            OnDeath();
            

            Debug.Log($"{type} '{name}' has died.");
            
        }

        // Override
        public virtual void OnDeath() { }   // Do nothing. MUST be overriden.

        // ------------------------------------------------- //

    }

}