using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Actions;
using BtcTransmuter.Abstractions.Recipes;
using BtcTransmuter.Abstractions.Triggers;
using BtcTransmuter.Helpers;
using BtcTransmuter.Models;
using Microsoft.AspNetCore.Mvc;

namespace BtcTransmuter.ViewComponents
{
    public class RecipeActionFooterViewComponent : ViewComponent
    {
        private readonly IEnumerable<ITriggerDescriptor> _triggerDescriptors;
        private readonly IEnumerable<IActionDescriptor> _actionDescriptors;
        private readonly IRecipeManager _recipeManager;

        public RecipeActionFooterViewComponent(IEnumerable<ITriggerDescriptor> triggerDescriptors,
            IEnumerable<IActionDescriptor>actionDescriptors,
            IRecipeManager recipeManager)
        {
            _triggerDescriptors = triggerDescriptors;
            _actionDescriptors = actionDescriptors;
            _recipeManager = recipeManager;
        }

        public async Task<IViewComponentResult> InvokeAsync(string recipeId, string recipeActionIdInGroupBeforeThisOne)
        {
            var recipe = await _recipeManager.GetRecipe(recipeId);

            var properties = new Dictionary<string, object>();

            if (recipe?.RecipeTrigger != null)
            {
                var descriptor = _triggerDescriptors.FirstOrDefault(triggerDescriptor =>
                    triggerDescriptor.TriggerId == recipe?.RecipeTrigger.TriggerId);

                if (descriptor != null && descriptor.GetType().IsSubclassOfRawGeneric(typeof(BaseTriggerHandler<,>)))
                {
                    var type = descriptor.GetType().BaseType.GetGenericArguments().First();
                    properties.Add("TriggerData", GetRecursiveAvailableProperties(type));
                }
            }

            if (!string.IsNullOrEmpty(recipeActionIdInGroupBeforeThisOne))
            {
                 var recipeAction = recipe.RecipeActions.FirstOrDefault(action =>
                    action.Id.Equals(recipeActionIdInGroupBeforeThisOne, StringComparison.InvariantCultureIgnoreCase));

                 if (recipeAction != null)
                 {
                     var type = _actionDescriptors.FirstOrDefault(handler =>
                         handler.ActionId == recipeAction.ActionId)?.ActionResultDataType;

                     properties.Add("PreviousAction", GetRecursiveAvailableProperties(type));
                 }
                 
                 //TODO: query recipe action id above and retrieve its previous ones
                 
            }

            return View(new RecipeActionFooterViewModel()
            {
                Properties = properties,
                NoRecipeTrigger = recipe?.RecipeTrigger == null
            });
        }


        private Dictionary<string, object> GetRecursiveAvailableProperties(Type type, int currentDepth = 0)
        {
            var properties = new Dictionary<string, dynamic>();
            if (currentDepth >= 5)
            {
                properties.Add(type.Name, "Too deep, guess the rest bro");
                return properties;
            }

            var tProps = type.GetProperties().GroupBy(info => info.Name + info.PropertyType).Select(infos => infos.First());
            foreach (var prop in tProps)
            {
                if (properties.ContainsKey(prop.Name))
                {
                    var currentValue = properties[prop.Name];
                    if (currentValue is Dictionary<string, object> currentValueDict)
                    {
                        properties[prop.Name] = currentValueDict
                            .Concat(GetRecursiveAvailableProperties(prop.PropertyType, currentDepth + 1))
                            .ToDictionary(pair => pair.Key, pair => pair.Value);
                    }
                    continue;
                }
                
                properties.Add(prop.Name,
                    prop.PropertyType.IsPrimitive
                        ? (dynamic) prop.PropertyType.ToString()
                        : GetRecursiveAvailableProperties(prop.PropertyType, currentDepth+1));
            }

            return properties;
        }
    }
}