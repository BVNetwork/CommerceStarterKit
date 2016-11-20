/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;

namespace OxxCommerceStarterKit.Core.Extensions
{
    public static class CartExtensions
    {
        public static string GeneratePredictableOrderNumber(this Cart cart)
        {
            return "PO" + cart.OrderGroupId;
        }

        public static void AddValidationIssues(this Dictionary<ILineItem, List<ValidationIssue>> issues, ILineItem lineItem, ValidationIssue issue)
        {
            if (!issues.ContainsKey(lineItem))
            {
                issues.Add(lineItem, new List<ValidationIssue>());
            }

            if (!issues[lineItem].Contains(issue))
            {
                issues[lineItem].Add(issue);
            }
        }

        public static bool HasItemBeenRemoved(this Dictionary<ILineItem, List<ValidationIssue>> issuesPerLineItem, ILineItem lineItem)
        {
            List<ValidationIssue> issues;
            if (issuesPerLineItem.TryGetValue(lineItem, out issues))
            {
                return issues.Any(x => x == ValidationIssue.RemovedDueToInactiveWarehouse ||
                        x == ValidationIssue.RemovedDueToCodeMissing ||
                        x == ValidationIssue.RemovedDueToInsufficientQuantityInInventory ||
                        x == ValidationIssue.RemovedDueToInvalidPrice ||
                        x == ValidationIssue.RemovedDueToMissingInventoryInformation ||
                        x == ValidationIssue.RemovedDueToNotAvailableInMarket ||
                        x == ValidationIssue.RemovedDueToUnavailableCatalog ||
                        x == ValidationIssue.RemovedDueToUnavailableItem);
            }
            return false;
        }


    }
}
