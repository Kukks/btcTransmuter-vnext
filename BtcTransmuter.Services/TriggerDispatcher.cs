﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Data;
using BtcTransmuter.Extension.Email.Actions;

namespace BtcTransmuter.Services
{
    public class TriggerDispatcher : ITriggerDispatcher
    {
        private readonly IEnumerable<ITriggerHandler> _handlers;
        private readonly IRecipeManager _recipeManager;
        private readonly IActionDispatcher _actionDispatcher;

        public TriggerDispatcher(IEnumerable<ITriggerHandler> handlers, IRecipeManager recipeManager, IActionDispatcher actionDispatcher)
        {
            _handlers = handlers;
            _recipeManager = recipeManager;
            _actionDispatcher = actionDispatcher;
        }
        public async Task DispatchTrigger(ITrigger trigger)
        {
            var recipes = await _recipeManager.GetRecipes(new RecipesQuery()
            {
                Enabled = true,
                RecipeTriggerId = trigger.Id
            });
            
            
            var triggeredRecipes = new List<Recipe>();
            foreach (var recipe in recipes)
            {
                foreach (var triggerHandler in _handlers)
                {
                    if (await triggerHandler.IsTriggered(trigger, recipe.RecipeTrigger))
                    {
                        triggeredRecipes.Add(recipe);
                    }
                }
            }
            
            await Task.WhenAll( triggeredRecipes.Select(recipe => _actionDispatcher.Dispatch()));
        }
    }
}