using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Retroverse
{
    public class EntityPool<T> where T : Entity, new()
    {
        public T[] entities { get; private set; }
        public int activeCount { get; private set; }
        public Action<T> initializer;
    
        // initializer action allows you to specialize the entities in the pool without making dedicated subclasses
        public EntityPool(int budget, Action<T> initializer)
        {
            this.initializer = initializer;
            entities = new T[budget];
            for (int i = 0; i < budget; i++)
            {
                entities[i] = new T();
                if (initializer != null)
                    initializer(entities[i]);
                entities[i].active = false;
            }
        }

        // reset action is applied to all obtained entities before returning them
        public T obtain(Action<T> reset)
        {
            // expand pool size if necessary
            if (activeCount >= entities.Length)
            {
                T[] newEntities = new T[2 * entities.Length];
                int i = 0;
                for (; i < entities.Length; i++)
                    newEntities[i] = entities[i];
                for (; i < newEntities.Length; i++)
                {
                    newEntities[i] = new T();
                    if (initializer != null)
                      initializer(newEntities[i]);
                    newEntities[i].active = false;
                }
                entities = newEntities;
            }
            entities[activeCount].active = true;
            if (reset != null)
                reset(entities[activeCount]);
            return entities[activeCount++];
        }
        
        // automatically detects and recycles dead entities
        public void Update(GameTime time)
        {
            for (int i = 0; i < entities.Length && i < activeCount; i++)
            {
                if (!entities[i].active)
                {
                    T swap = entities[i];
                    entities[i] = entities[activeCount - 1];
                    entities[activeCount - 1] = swap;
                    activeCount--;
                }
            }
        }
    }
}
