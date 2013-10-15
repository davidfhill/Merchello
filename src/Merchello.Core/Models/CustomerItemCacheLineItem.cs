﻿using System;
using System.Runtime.Serialization;

namespace Merchello.Core.Models
{
    /// <summary>
    /// Represents a customer cached line item
    /// </summary>
    /// <remarks>
    /// Needed for typed query mapper
    /// </remarks>
    [Serializable]
    [DataContract(IsReference = true)]
    public class CustomerItemCacheLineItem : LineItemBase, ICustomerItemCacheLineItem
    {
        public CustomerItemCacheLineItem(int containerId, string name, string sku, decimal amount) 
            : base(containerId, name, sku, amount)
        {}

        public CustomerItemCacheLineItem(int containerId, string name, string sku, int quantity, decimal amount) 
            : base(containerId, name, sku, quantity, amount)
        {}

        public CustomerItemCacheLineItem(int containerId, LineItemType lineItemType, string name, string sku, int quantity, decimal amount) 
            : base(containerId, lineItemType, name, sku, quantity, amount)
        {}

        public CustomerItemCacheLineItem(int containerId, LineItemType lineItemType, string name, string sku, int quantity, decimal amount, ExtendedDataCollection extendedData) 
            : base(containerId, lineItemType, name, sku, quantity, amount, extendedData)
        {}

        public CustomerItemCacheLineItem(int containerId, Guid lineItemTfKey, string name, string sku, int quantity, decimal amount, ExtendedDataCollection extendedData) 
            : base(containerId, lineItemTfKey, name, sku, quantity, amount, extendedData)
        {}
    }
}